using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.Contract
{
    public class CameraSettings
    {
        public int Camera { get; set; }
        public bool Enabled { get; set; }
        public ImageSettings Settings { get; set; }

        public CameraSettings() { }

        public CameraSettings(int camera, ImageSettings settings)
        {
            this.Camera = camera;
            this.Settings = settings;
        }

        public override string ToString() => $"camera[{Camera}]={Settings}";

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Camera;
                if (Settings != null) 
                    hashCode = (hashCode * 397) ^ Settings.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CameraSettings)obj);
        }

        protected bool Equals(CameraSettings other)
        {
            return Camera == other.Camera && Settings.Equals(other.Settings);
        }
    }
}
