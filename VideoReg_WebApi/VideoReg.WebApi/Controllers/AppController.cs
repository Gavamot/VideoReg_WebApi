using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VideoReg.Infra.Services;

namespace VideoReg.WebApi.Controllers
{
    [ApiController]
    public class AppController : ControllerBase
    {
        const string HeaderTimestamp = "X-IMAGE-DATE";
        const string HeaderBrigade = "X-BRIGADE";
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
            ReadFromRequest(HeaderTimestamp, timestamp => dateTimeService.Parse(timestamp, DateTimeService.DefaultMsFormat));
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