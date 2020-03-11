using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.VideoRegInfo
{
    public interface IVideoRegInfoStore
    {
        RegInfo GetRegInfo();
    }

    public class VideoRegInfoStore : IVideoRegInfoStore
    {
        // /home/v-1336/projects/dist/ASCWeb/brigade_code
        
        //
        // TODO : Поменять затычку на реализацию
        public RegInfo GetRegInfo()
        {
            return new RegInfo
            {
                Vpn = "10.36.1.37",
                Ip = "143.1.1.2",
                BrigadeCode = 1
            };
        }
    }
}
