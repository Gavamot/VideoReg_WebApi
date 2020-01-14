using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.OnlineVideo
{
    public struct CameraSourceSettings
    {
        public CameraSourceSettings(int number, string snapshotUrl)
        {
            this.number = number;
            this.snapshotUrl = snapshotUrl;
        }
        
        public readonly int number;
        public readonly string snapshotUrl;
        public override string ToString() => $"CameraNumber={number} URL={snapshotUrl}";
    }
}
