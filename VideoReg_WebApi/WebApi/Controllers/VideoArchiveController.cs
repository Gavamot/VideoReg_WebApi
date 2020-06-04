using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Archive;
using WebApi.Dto;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    public class VideoArchiveController : AppController
    {
        readonly ICameraArchiveRep videoArc;
        private readonly IMapper mapper;

        public VideoArchiveController(ICameraArchiveRep videoArc,
            IMapper mapper,
            IDateTimeService dateTimeService) : base(dateTimeService)
        {
            this.videoArc = videoArc;
            this.mapper = mapper;
        }

        //netstat -n | wc -l
        /// <summary>
        /// Получить структуру видеоархива
        /// </summary>
        /// <response code="200">Возвращает последнюю актуальную структуру видеоархива</response>
        [HttpGet]
        [Route("/[controller]/Structure")]
        public ActionResult<FileVideoMp4Dto[]> GetStructure(DateTime startWith)
        {
            var data = videoArc.GetFullStructure(startWith);
            var res = mapper.Map<FileVideoMp4Dto[]>(data);
            return Ok(res);
        }

        /// <summary>
        /// Получить информацию по трендам
        /// </summary>
        /// <response code="200">Возвращает информацию о приборе и тренды</response>
        /// <response code="404">Запрошенный файл не существует</response>  
        [HttpGet]
        [Route("/[controller]/File")]
        public async Task<IActionResult> GetFile([Range(1, 9)]int camera, DateTime pdt)
        {
            var file = await videoArc.TryGetVideoFileAsync(pdt, camera);
            if (file == default)
                return NotFound();

            SetHeaderToResponseBrigade(file.File.brigade);
            Response.StatusCode = StatusCodes.Status206PartialContent;
            return File(file.Data, "video/mp4",  true);
        }
    }
}
