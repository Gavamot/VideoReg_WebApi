﻿using System;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VideoReg.Domain.Archive;
using VideoReg.Domain.Archive.ArchiveFiles;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Dto;
using VideoReg.Infra.Services;

namespace VideoReg.WebApi.Controllers
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
        public IActionResult GetFile(int camera, DateTime pdt)
        {
            var fileStream = videoArc.GetVideoFileStream(pdt, camera);
            if (fileStream == default)
                return NotFound();
            
            var brigade = brigadeHistoryRep.GetBrigadeHistory().GetBrigadeCode(pdt);
            SetHeaderToResponseBrigade(brigade);

            Response.StatusCode = StatusCodes.Status206PartialContent;
            return File(fileStream, "video/mp4", enableRangeProcessing: true);
        }
    }
}
