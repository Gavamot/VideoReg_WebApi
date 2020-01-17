using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoReg.Domain.OnlineVideo
{
    public interface ICameraSourceRep
    {
        Task<CameraSourceSettings[]> GetAll();
        Task<CameraSourceSettings> Get(int cameraNumber);
    }
}
