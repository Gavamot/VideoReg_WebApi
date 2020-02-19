using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.OnlineVideo.Store;
using VideoReg.Domain.Store;
using VideoReg.Infra.Services;

namespace VideoReg.WebApi.Controllers
{
    public class ImageTransformSettingsMV
    {
        public const int MinSizePx = 0;
        public const int MaxSizePx = 1920;
        public const int MinQuality = 0;
        public const int MaxQuality = 100;

        /// <summary>
        /// Ширина изображения (px)
        /// </summary>
        [Range(MinSizePx, MaxSizePx)]
        [Display(Name = "Ширина")]
        [DefaultValue(0)]
        public int Width { get; set; }

        /// <summary>
        /// Высота изображения (px)
        /// </summary>
        [Range(MinSizePx, MaxSizePx)]
        [Display(Name = "Высота")]
        [DefaultValue(0)]
        public int Height { get; set; }

        /// <summary>
        /// Качество изображения (%)
        /// </summary>
        [Range(MinQuality, MaxQuality)]
        [Display(Name = "Качество изображения  %")]
        [DefaultValue(0)]
        public int Quality { get; set; }

        public bool IsDefault() => 
            Height == 0 
            || Width == 0 
            || Quality == 0;
        
    }

    [ApiController]
    public class CameraController : ControllerBase
    {
        //readonly ICameraSettingsRep cameraSettingsRep;
        readonly IMapper mapper;
        private readonly ICameraStore cameraCache;
        private const string ApiDateTimeFormat = "yyyy-M-dTH:m:s.fff";
        private readonly ILog log;

        public CameraController(//ICameraSettingsRep cameraSettingsRep,
            IMapper mapper, 
            ICameraStore cameraCache,
            ILog log)
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
        //public async Task<IActionResult> SetSettingsForAll([FromBody]ImageTransformSettingsMV cameraSettings)
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

        //    var settings = mapper.Map<ImageTransformSettings>(cameraSettings);
        //    try
        //    {
        //        await cameraSettingsRep.SetAsync(cameraSettings.CameraNumber, settings);
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, e.Message);
        //    }
        //    return Ok();
        //}

        /// <summary>
        /// Получить картинку с камеры
        /// </summary>
        /// <response code="200">Картинка</response> 
        /// <response code="404">Изображение не найдено</response>
        private const string HeaderTimestamp = "X-IMAGE-DATE";
        public const string ImageDateHeaderFormat = "yyyy-M-dTHH:mm:ss.fff";

        private DateTime? ReadTimeStampFromRequest()
        {
            DateTime? timeStamp = null;
            if (Response.Headers.TryGetValue(HeaderTimestamp, out var dtStr))
            {
                timeStamp = DateTime.ParseExact(dtStr[0], ImageDateHeaderFormat, CultureInfo.InvariantCulture);
            }
            return timeStamp;
        }

        private void SetResponseTimestampHeader(DateTime timestamp)
        {
            Response.Headers.Add(HeaderTimestamp, timestamp.ToString(ImageDateHeaderFormat, CultureInfo.InvariantCulture));
        }

        private async Task<IActionResult> GenerateFileContentResultAsync(Func<DateTime?, Task<CameraResponse>> getImg)
        {
            DateTime? timeStamp = ReadTimeStampFromRequest();
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

            SetResponseTimestampHeader(img.Timestamp);
            return File(img.Img, "image/jpeg");
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("/[controller]/[action]")]
        public async Task<IActionResult> GetImage([FromQuery]int num, [FromQuery]ImageTransformSettingsMV settings = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.ToArray()[0].Errors);
            ImageTransformSettings imgSettings = null;
            if (settings != null && !settings.IsDefault())
            {
                imgSettings = mapper.Map<ImageTransformSettings>(settings);
            }
            return await GenerateFileContentResultAsync(timeStamp => cameraCache.GetCameraAsync(num, imgSettings, timeStamp));
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
        public async Task<IActionResult> GetImageFromCache([FromQuery]int num)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.ToArray()[0].Errors);
            return await GenerateFileContentResultAsync(timeStamp => cameraCache.GetCameraAsync(num, null, timeStamp));
        }
    }
}