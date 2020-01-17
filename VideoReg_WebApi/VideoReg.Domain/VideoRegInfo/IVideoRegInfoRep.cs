using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoReg.Domain.VideoRegInfo
{
    /// <summary>
    /// Получение информации о видеорегистраторе
    /// </summary>
    public interface IVideoRegInfoRep
    {
        /// <summary>
        /// Получить общюю информацию о видеорегистраторе
        /// </summary>
        Task<VideoRegInfoDto> GetInfo();
    }
}
