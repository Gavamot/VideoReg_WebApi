using System;
using Microsoft.CSharp.RuntimeBinder;

namespace VideoReg.Domain.OnlineVideo
{
    public static class ImageTransformSettingsExt
    {
        public static bool IsDefault(this ImageTransformSettings self) 
            => self.Height == 0 || self.Width  == 0 || self.Quality == 0;
        

        public static bool IsNotDefault(this ImageTransformSettings self)
        {
            return !self.IsDefault();
        }
    }

    public class ImageTransformSettings
    {
        protected bool Equals(ImageTransformSettings other)
        {
            return Width == other.Width 
                   && Height == other.Height 
                   && Quality == other.Quality;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ImageTransformSettings) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ Quality;
                return hashCode;
            }
        }

        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int Quality { get; set; } = 0;

        public override string ToString() => $"Size={Width}x{Height} | Quality={Quality}";
    }
}
