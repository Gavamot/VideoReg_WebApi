using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Archive;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Dto;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    public class VideoArchiveController : AppController
    {
        readonly IVideoArchiveRep videoArc;
        private readonly IMapper mapper;
        private readonly IArchiveFileGenerator _fileGenerator;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;
        public VideoArchiveController(IVideoArchiveRep videoArc,
            IMapper mapper,
            IDateTimeService dateTimeService, 
            IBrigadeHistoryRep brigadeHistoryRep) : base(dateTimeService)
        {
            this.videoArc = videoArc;
            this.mapper = mapper;
            this.brigadeHistoryRep = brigadeHistoryRep;
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
        public async Task<IActionResult> GetFile(int camera, DateTime pdt)
        {
            var fileStream = await videoArc.GetVideoFileStreamAsync(pdt, camera);
            if (fileStream == default)
                return NotFound();
            
            var brigade = brigadeHistoryRep.GetBrigadeHistory().GetBrigadeCode(pdt);
            SetHeaderToResponseBrigade(brigade);

            Response.StatusCode = StatusCodes.Status206PartialContent;
            return File(fileStream, "video/mp4", enableRangeProcessing: true);
        }
    }
}
