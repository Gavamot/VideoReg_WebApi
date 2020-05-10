using System;
using System.Threading.Tasks;
using WebApi.Contract;

namespace WebApi.Core
{
    /// <summary>
    /// Получение информации о видеорегистраторе
    /// </summary>
    public interface IRegInfoRep
    {
        /// <summary>
        /// Получить общюю информацию о видеорегистраторе
        /// </summary>
        Task<RegInfo> GetInfoAsync();
        Action<RegInfo> RegInfoChanged { get; set; }
        string Vpn { get; }
    }
}
