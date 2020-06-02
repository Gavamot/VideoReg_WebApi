using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    public class AdminController : ControllerBase
    {
        /// <summary>
        /// Выключить сервис. Он будет запущен заново скриптом через какоето время
        /// </summary>
        /// <response code="200">Настройки были изменены</response>
        /// <response code="500">Произошла ошибка</response>  
        //[HttpGet]
        //[Route("/[controller]/[action]")]
        //public void RestartService()
        //{
        //    Environment.Exit(1);
        //} 

        [HttpGet]
        [Route("/[controller]/[action]")]
        public async Task<IActionResult> ThrowError()
        {
            throw new Exception("Все сломалось");
        }

        [HttpGet]
        [Route("/[controller]/[action]")]
        public IActionResult ThrowError2()
        {
            throw new Exception("Все сломалось");
        }

    }
}