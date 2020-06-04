using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApi.Core;

namespace WebApi.Controllers
{
    [ApiController]
    public class AdminController : ControllerBase
    {
        readonly ILogger<AdminController> log;
        readonly IApp app;

        public AdminController(ILogger<AdminController> log, IApp app)
        {
            this.log = log;
            this.app = app;
        }

        /// <summary>
        /// Выключить сервис. Он будет запущен заново скриптом через какоето время
        /// </summary>
        /// <response code="200">Настройки были изменены</response>
        /// <response code="500">Произошла ошибка</response>
        /// <exception cref="System.Security.SecurityException"></exception>
        [HttpGet]
        [Route("/[controller]/[action]")]
        public void RestartService()
        {
            app.Close(nameof(AdminController));
        }
    }
}