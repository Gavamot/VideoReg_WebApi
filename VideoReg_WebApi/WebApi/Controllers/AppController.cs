using System;
using Microsoft.AspNetCore.Mvc;
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
            if (Request.Headers.TryGetValue(HeaderTimestamp, out var dtStr))
            {
                if(dtStr.Count > 0)
                {
                    return parseFunc(dtStr[0]);
                }
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