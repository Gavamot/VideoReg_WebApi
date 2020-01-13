using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoReg.Domain.VideoRegInfo
{
    public class VideoRegInfoDto
    {
        /// <summary>
        /// Текущее дата-время на регистраторе
        /// </summary>
        public DateTime CurrentDate { get; set; }
        public int BrigadeCode { get; set; }
        public string VideoRegSerial { get; set; }
        public string IveSerial { get; set; }
        /// <summary>
        /// Версия прошивки
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Доступные онлайн камеры
        /// </summary>
        public int [] Cameras { get; set; } 
    }
}
