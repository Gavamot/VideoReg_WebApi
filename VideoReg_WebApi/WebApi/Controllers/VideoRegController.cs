using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApi.Contract;
using WebApi.OnlineVideo.Store;
using WebApi.Core;
using Microsoft.AspNetCore.Http;

namespace WebApi.Controllers
{
    public class RegInfoDto
    {
        public int Brigade { get; set; }
        public string Vpn { get; set; }
        public string Version { get; set; }
        public string RegSerial { get; set; }
        public int[] Cameras { get; set; }
    }

    [ApiController]
    public class VideoRegController : ControllerBase
    {
        readonly IRegInfoRep regInfoRep;
        readonly ICameraStore cameraCache;

        public VideoRegController(IRegInfoRep regInfo,
            ICameraStore cameraStore)
        {
            this.regInfoRep = regInfo;
            this.cameraCache = cameraStore;
        }

        [HttpGet]
        [Route("/[controller]/RegSerial")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public ActionResult<string> GetSerial()
        {
            return Ok(regInfoRep.RegSerial);
        }

        [HttpGet]
        [Route("/[controller]/Brigade")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetBrigade()
        {
            var brigadeCode = await regInfoRep.GetBrigadeCodeAsync();
            return Ok(brigadeCode);
        }

        [HttpGet]
        [Route("/[controller]/ApiVersion")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public ActionResult<string> GetApiVersion()
        {
            return Ok(regInfoRep.ApiVersion);
        }

        [HttpGet]
        [Route("/[controller]/Vpn")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public ActionResult<string> GetVpn()
        {
            return Ok(regInfoRep.Vpn);
        }

        /// <summary>
        /// Получить последнюю актуальную информацию о видеорегистраторе.
        /// </summary>
        /// <response code="200">Возвращает о последнюю актуальную информацию о видеорегистраторе</response>
        /// <response code="500">Источниек информации о регистраторе оказался недоступен по каким либо причинам</response>  
        [HttpGet]
        [Route("/[controller]/Info")]
        [ProducesResponseType(typeof(RegInfoDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<RegInfo>> GetInfo()
        {
            int brigade = await regInfoRep.GetBrigadeCodeAsync();
            var cameras = cameraCache.GetAvailableCameras();

            var res = new RegInfoDto()
            {
                Brigade = brigade,
                Version = regInfoRep.ApiVersion,
                Vpn = regInfoRep.Vpn,
                Cameras = cameras,
                RegSerial = regInfoRep.RegSerial
            };
            return Ok(res);
        }
    }
}
