using System.Threading.Tasks;

namespace VideoReg.Domain.VideoRegInfo
{
    /// <summary>
    /// Получение информации о видеорегистраторе
    /// </summary>
    public interface IRegInfoRep
    {
        /// <summary>
        /// Получить общюю информацию о видеорегистраторе
        /// </summary>
        Task<VideoRegInfo> GetInfo();
    }
}
