using System;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VideoReg.Domain.Archive;

namespace VideoReg.WebApi.Controllers
{
    public class FileVideoMp4Dto
    {
        public int num { get; set; }
        public int? brig { get; set; }
        public string name { get; set; }
        public int duration { get; set; }
    }

    [ApiController]
    public class VideoArchiveController : ControllerBase
    {
        readonly IVideoArchiveRep videoArc;
        private readonly IMapper mapper;

        public VideoArchiveController(IVideoArchiveRep videoArc, IMapper mapper)
        {
            this.videoArc = videoArc;
            this.mapper = mapper;
        }

        //netstat -n | wc -l
        /// <summary>
        /// Получить структуру видеоархива
        /// </summary>
        /// <response code="200">Возвращает последнюю актуальную информацию о видеорегистраторе</response>
        [HttpGet]
        [Route("/[controller]/Structure")]
        public ActionResult<FileVideoMp4Dto[]> GetStructure(DateTime startWith)
        {
            var data = videoArc.GetStructure(startWith);
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
        public IActionResult GetFile(string fileName)
        {
            var stream = videoArc.GetVideoFileStream(fileName);
            if (stream == default)
                return NotFound(); 
            Response.StatusCode = StatusCodes.Status206PartialContent;
            return File(stream, "video/mp4", enableRangeProcessing: true);
        }
    }
}
