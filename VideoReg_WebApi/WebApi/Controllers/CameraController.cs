using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        readonly ICameraSettingsStore cameraSettingsStore;

        public CameraController(//ICameraSettingsRep cameraSettingsRep,
            IMapper mapper, 
            ICameraStore cameraCache,
            IDateTimeService dateTimeService,
            ICameraSettingsStore cameraSettingsStore,
            ILog log) : base(dateTimeService)
        {
            this.mapper = mapper;
            this.cameraCache = cameraCache;
            this.cameraSettingsStore = cameraSettingsStore;
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

        /// <summary>
        /// Получить изображение по камере.
        /// Если не пепредавать настройки (или передать любую по умолчанию значение 0) то будет отдано оригинальное изображение.
        /// Если передать настройки хоть одну из настроек передать -1 то конвертация отключится.
        /// !Конвертация изображений позволяет уменьшить обьем передаваемого трафика но занимает много времени.
        /// Выставлять высокие настройки конвертации не имеет смысла так как изображение лучше исходного не станет
        /// Но увеличится в размерах и долго будет конвертироватся. Что значительно уменьшит fps.
        /// Пережимать изображения стоит только если канал связи плохой и выставлять низкие настройки. 
        /// Также в тело http запроса следует добавлять заголовок "X-TIMESTAMP" в формате yyyy-MM-ddTHH:mm:ss.fff
        /// Каждому кадру взятому с камеры присваевается временная метка
        /// Это позволит не отдавать кадр пока не изменится эта временная метка тоесть кадр.
        /// Также этот же заголовок будет установлен в ответе. И его следует использовать при следующем запросе чтобы получить измененное изображение а не копию предыдущего.
        /// </summary>
        /// <param name="camera">Номер камеры от 1-9</param>
        /// <param name="settings">Настройки камеры</param>
        /// <response code="200">Изображение с камеры</response>
        /// <response code="404">Переданная камера не найдена</response>
        /// <response code="400">Неверные параметры запроса</response>
        /// <response code="304">Изображение с камеры не изменялось</response>
        [HttpGet]
        [ProducesResponseType(typeof(byte[]),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [Route("/[controller]/[action]")]
        public async Task<IActionResult> GetImage([FromQuery]int camera, [FromQuery]ImageTransformSettingsDto settings = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.ToArray()[0].Errors);

            ImageSettings imgSettings = null;
            if (settings.IsResetValue())
            {
                cameraSettingsStore.Set(camera, false);
            }
            else if (!settings.IsDefaultValue())
            {
                imgSettings = mapper.Map<ImageSettings>(settings);
                cameraSettingsStore.Set(camera, true, imgSettings);
            }
            return await GenerateFileContentResultAsync(timeStamp => cameraCache.GetCameraAsync(camera, imgSettings, timeStamp));
        }

        [HttpGet]
        [Route("/[controller]/[action]")]
        public async Task<IActionResult> GetConvertedImage([FromQuery]int camera)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.ToArray()[0].Errors);
            return File(cameraCache.GetImage(camera).ConvertedImg.Image, "image/jpeg");
        }

        [HttpGet]
        [Route("/[controller]/[action]")]
        public async Task<IActionResult> GetNativeImage([FromQuery]int camera)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.ToArray()[0].Errors);
            return File(cameraCache.GetImage(camera).NativeImg, "image/jpeg");
        }

    }
}