using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Contract;
using WebApi.Dto;
using WebApi.OnlineVideo.Store;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    public class CameraController : AppController
    {
        //readonly ICameraSettingsRep cameraSettingsRep;
        readonly IMapper mapper;
        private readonly ICameraStore cameraCache;
        private readonly ILog log;

        public CameraController(//ICameraSettingsRep cameraSettingsRep,
            IMapper mapper, 
            ICameraStore cameraCache,
            IDateTimeService dateTimeService,
            ILog log) : base(dateTimeService)
        {
            //this.cameraSettingsRep = cameraSettingsRep;
            this.mapper = mapper;
            this.cameraCache = cameraCache;
            this.log = log;
        }

        /// <summary>
        /// Получить доступные камеры
        /// </summary>
        /// <response code="200">Настройки были изменены</response>
        [HttpGet]
        [ProducesResponseType(typeof(int[]), StatusCodes.Status200OK)]
        [Route("/[controller]/[action]")]
        public IActionResult GetAvailableCameras()
        {
            var cameras = cameraCache.GetAvailableCameras();
            return Ok(cameras);
        }

        /// <summary>
        /// Получить настройки камер
        /// </summary>
        /// <response code="200">Настройки были изменены</response> 
        /// <response code="500">Произошла ошибка</response>  
        //[HttpGet]
        //[ProducesResponseType(typeof(CameraTransformSettingsMV[]), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[Route("/[controller]/[action]")]
        //public async Task<IActionResult> GetSettings()
        //{
        //    try
        //    {
        //        var cameras = await cameraSettingsRep.GetAllAsync();
        //        return Ok(mapper.Map<CameraTransformSettingsMV[]>(cameras));
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, e.Message);
        //    }
        //}

        /// <summary>
        /// Установить настройки для всех камер 
        /// </summary>
        /// <param name="cameraSettings">Настройки камеры</param>
        /// <response code="200">Настройки были изменены</response>
        /// <response code="400">Неверные входные параметры</response>  
        /// <response code="500">Произошла ошибка</response>  
        //[HttpPost]
        //[Route("/[controller]/[action]")]
        //public async Task<IActionResult> SetSettingsForAll([FromBody]ImageTransformSettingsDto cameraSettings)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var camera = mapper.Map<ImageTransformSettings>(cameraSettings);
        //    try
        //    {
        //        await cameraSettingsRep.SetForAllCamerasAsync(camera);
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, e.Message);
        //    }
        //    return Ok();
        //}

        /// <summary>
        /// Установить настройки для одной камеры
        /// </summary>
        /// <param name="cameraSettings">Номер камеры с настройками камеры</param>
        /// <response code="200">Настройки были изменены</response>
        /// <response code="400">Неверные входные параметры</response>  
        /// <response code="500">Произошла ошибка</response>  
        //[HttpPost]
        //[Route("/[controller]/[action]")]
        //public async Task<IActionResult> SetSettings([FromBody]CameraTransformSettingsMV cameraSettings)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var Settings = mapper.Map<ImageTransformSettings>(cameraSettings);
        //    try
        //    {
        //        await cameraSettingsRep.SetAsync(cameraSettings.CameraNumber, Settings);
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, e.Message);
        //    }
        //    return Ok();
        //}

        private async Task<IActionResult> GenerateFileContentResultAsync(Func<DateTime?, Task<CameraResponse>> getImg)
        {
            DateTime? timeStamp = ReadFromRequestTimestamp();
            CameraResponse img = default;
            try
            {
                img = await getImg(timeStamp);
            }
            catch (NoNModifiedException) // Изображение не изменилось
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            if (img == default) // Изображение по переданной камере не найдено
                return NotFound();

            SetHeaderToResponseTimestamp(img.Timestamp);
            return File(img.Img, "image/jpeg");
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("/[controller]/[action]")]
        public async Task<IActionResult> GetImage([FromQuery]int camera, [FromQuery]ImageTransformSettingsDto settings = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.ToArray()[0].Errors);
            ImageSettings imgSettings = null;
            if (settings != null && !settings.IsDefault())
            {
                imgSettings = mapper.Map<ImageSettings>(settings);
            }
            return await GenerateFileContentResultAsync(timeStamp => cameraCache.GetCameraAsync(camera, imgSettings, timeStamp));
        }

        /// <summary>
        /// Получить картинку из кэша либо нативную
        /// </summary>
        /// <response code="200">Картинка</response> 
        /// <response code="404">Изображение не найдено</response>
        [HttpGet]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("/[controller]/[action]")]
        public async Task<IActionResult> GetImageFromCache([FromQuery]int camera)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.ToArray()[0].Errors);
            return await GenerateFileContentResultAsync(timeStamp => cameraCache.GetCameraAsync(camera, null, timeStamp));
        }
    }
}