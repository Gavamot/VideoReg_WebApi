using System;
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
    public class TrendsArchiveController : AppController
    {
        readonly ITrendsArchiveRep arc;
        private readonly IMapper mapper;

        public TrendsArchiveController(ITrendsArchiveRep arc,
            IMapper mapper,
            IDateTimeService dateTimeService) : base(dateTimeService)
        {
            this.arc = arc;
            this.mapper = mapper;
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
        public async Task<IActionResult> GetFile(DateTime pdt)
        {
            var file = await arc.GetTrendFileAsync(pdt);
            if (file == default)
                return NotFound();

            SetHeaderToResponseBrigade(file.File.brigade);

            Response.StatusCode = StatusCodes.Status206PartialContent;
            return File(file.Data, "application/json", enableRangeProcessing: true);
        }
    }
}
