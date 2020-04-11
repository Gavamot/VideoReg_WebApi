using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Archive;
using WebApi.Archive.BrigadeHistory;
using WebApi.Dto;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    public class TrendsArchiveController : AppController
    {
        readonly ITrendsArchiveRep arc;
        private readonly IMapper mapper;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;
        public TrendsArchiveController(ITrendsArchiveRep arc,
            IMapper mapper,
            IDateTimeService dateTimeService, 
            IBrigadeHistoryRep brigadeHistoryRep) : base(dateTimeService)
        {
            this.arc = arc;
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
        public ActionResult<FileTrendsDto[]> GetStructure(DateTime startWith)
        {
            var data = arc.GetFullStructure(startWith);
            var res = mapper.Map<FileTrendsDto[]>(data);
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
            var fileStream = await arc.GetTrendFileAsync(pdt);
            if (fileStream == default)
                return NotFound();
            
            var brigade = brigadeHistoryRep.GetBrigadeHistory().GetBrigadeCode(pdt);
            SetHeaderToResponseBrigade(brigade);

            Response.StatusCode = StatusCodes.Status206PartialContent;
            return File(fileStream, "video/mp4", enableRangeProcessing: true);
        }
    }
}
