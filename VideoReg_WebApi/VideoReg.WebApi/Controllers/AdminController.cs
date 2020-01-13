using System;
using Microsoft.AspNetCore.Mvc;

namespace VideoRegService.Controllers
{
    [ApiController]
    public class AdminController : ControllerBase
    {
        /// <summary>
        /// Выключить сервис. Он будет запущен заново скриптом через какоето время
        /// </summary>
        /// <response code="200">Настройки были изменены</response>
        /// <response code="500">Произошла ошибка</response>  
        [HttpGet]
        [Route("/[controller]/[action]")]
        public void RestartService()
        {
            Environment.Exit(1);
        } 
    }
}