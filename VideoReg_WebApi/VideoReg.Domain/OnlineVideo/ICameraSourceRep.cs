using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.OnlineVideo
{
    public interface ICameraSourceRep
    {
        CameraSourceSettings[] GetAll();
        CameraSourceSettings Get(int cameraNumber);
    }
}
