﻿using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebApi.Archive.ArchiveFiles;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    public class AppController : ControllerBase
    {
        public const string HeaderTimestamp = "X-TIMESTAMP";
        public const string HeaderBrigade = "X-BRIGADE";
        public const string ImageDateHeaderFormat = "yyyy-MM-ddTHH:mm:ss.fff";

        private readonly IDateTimeService dateTimeService;


        public AppController(IDateTimeService dateTimeService)
        {
            this.dateTimeService = dateTimeService;
        }

        private T? ReadFromRequest<T>(string header, Func<string, T> parseFunc) where T : struct
        {
            if (Response.Headers.TryGetValue(HeaderTimestamp, out var dtStr))
            {
                string parameter = dtStr.First();
                return parseFunc(parameter);
            }
            return null;
        }

        protected DateTime? ReadFromRequestTimestamp() =>
            ReadFromRequest(HeaderTimestamp, timestamp => dateTimeService.Parse(timestamp, ImageDateHeaderFormat));
        protected int? ReadFromRequestBrigade() =>
            ReadFromRequest(HeaderBrigade, int.Parse);
        
        private void SetHeaderToResponse<T>(string header, T value)
        {
            if(value == null) return;
            Response.Headers.Add(header, value.ToString());
        }

        protected void SetHeaderToResponseTimestamp(DateTime? timestamp)
        {
            string timestampString = timestamp == null ?  null : dateTimeService.ToStringFullMs(timestamp.Value);
            SetHeaderToResponse(HeaderTimestamp, timestampString );
        }

        protected void SetHeaderToResponseBrigade(int? brigade) => 
            SetHeaderToResponse(HeaderBrigade, brigade);
    }
}