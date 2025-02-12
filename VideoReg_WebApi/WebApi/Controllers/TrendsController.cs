﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApi.Trends;

namespace WebApi.Controllers
{
    public class TrendsController : ControllerBase
    {
        readonly ITrendsRep trendsRep;

        public TrendsController(ITrendsRep trendsRep)
        {
            this.trendsRep = trendsRep;
        }

        /// <summary>
        /// Получить информацию по трендам
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET VideoReg/Trends
        ///     { "actual":"Apr 12, 2019 8:56:17 AM",
        ///        "data": {  
        ///             "device": { 
        ///                 "serial":"000E000011110000",
        ///                 "version":"IVE-ASC 14.9.10\\x16","type":"14" 
        ///             },
        ///             "position" : {"brigade":34,"work":0,"cluster":"5","well":"513","field":"уньва"},
        ///             "archive":{"size":10.98,"load":0.4},
        ///             "info":{"time":"01/01/2001","video":true,"available":true,"active":true,"goodConnect":true,"location":{"x":55.68,"y":33.89}},
        ///             "channels":[ 
        ///                 {"code":1,"num":0,"value":"89","limitLow":-80.0,"limitHigh":50.0,"alarmLow":-80.0,"alarmHigh":80.0}
        ///                ,{"code":17,"num":0,"value":"0","limitLow":-40.0,"limitHigh":40.0,"alarmLow":-40.0,"alarmHigh":40.0}
        ///             ]
        ///         }
        ///     }
        ///
        /// </remarks>
        /// <returns> * actual - дата получения информации (чтобы определить актуальность информации клиенту
        /// надо сделать еще один запрос и сравнить изменилась ли эта дата, либо VideoReg/Info содержит
        /// текужую дату можно взять ее и сравнить с какорйлибо погрешностью с полученой),
        ///  * device - информация об устройстве
        ///  * position - текущие код бригады, местророждение, куст, скважина, код работы
        ///  * archive - данные об архиве
        ///  * info - дополнительная информация с прибора
        ///  * channels - информация по текущая(?) каналам (тренды)
        /// </returns>
        /// <response code="200">Возвращает информацию о приборе и тренды</response>
        /// <response code="500">Произошла ошибка получения информации</response>  
        [HttpGet]
        [Route("/[controller]/Trends")]
        public async Task<ActionResult> GetTrends()
        {
            var file = await trendsRep.GetTrendsIfChangedAsync(DateTime.MinValue);
            return Ok(File(file.Value, "application/json"));
        }
    }
}