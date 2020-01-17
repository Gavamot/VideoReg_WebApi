using System;
using Microsoft.CSharp.RuntimeBinder;

namespace VideoReg.Domain.OnlineVideo
{
    public static class ImageTransformSettingsExt
    {
        //private static bool InRange(this int v, int min, int max) => v >= min && v <= max;
        //public static void Validate(this ImageTransformSettings self)
        //{
        //    var defImg = default(ImageTransformSettings);
        //    if (self.Equals(defImg)) return;

        //    const int mimWidth = 1, maxWidth = 1024;
        //    const int mimHeight = 1, maxHeight = 1024;
        //    const int minQuality = 1, maxQuality = 100;
        //    if (self.Width.InRange(mimWidth, maxWidth)
        //        && self.Height.InRange(mimHeight, maxHeight)
        //        && self.Quality.InRange(minQuality, maxQuality))
        //        return;
        //    throw new FormatException($@"Image settings must be in range  
        //        mimWidth = {mimWidth}, maxWidth = {maxWidth}
        //        mimHeight = {mimHeight}, maxHeight = {maxHeight}
        //        minQuality = {minQuality}, maxQuality = {maxQuality}");
        //}

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
            return Width == other.Width && Height == other.Height && Quality == other.Quality;
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

        public int Width { get; set; }
        public int Height { get; set; }
        public int Quality { get; set; }

        public override string ToString() => $"Size={Width}x{Height} | Quality={Quality}";
    }
}
