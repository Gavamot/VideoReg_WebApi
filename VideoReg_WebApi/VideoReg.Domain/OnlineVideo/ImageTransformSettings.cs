using System;

namespace VideoReg.Domain.OnlineVideo
{
    public static class ImageTransformSettingsExt
    {
        private static bool InRange(this int v, int min, int max) => v >= min && v <= max;
        public static void Validate(this ImageTransformSettings self)
        {
            var defImg = default(ImageTransformSettings);
            if (self.Equals(defImg)) return;

            const int mimWidth = 1, maxWidth = 1024;
            const int mimHeight = 1, maxHeight = 1024;
            const int minQuality = 1, maxQuality = 100;
            if (self.width.InRange(mimWidth, maxWidth)
                && self.height.InRange(mimHeight, maxHeight)
                && self.quality.InRange(minQuality, maxQuality))
                return;
            throw new FormatException($@"Image settings must be in range  
                mimWidth = {mimWidth}, maxWidth = {maxWidth}
                mimHeight = {mimHeight}, maxHeight = {maxHeight}
                minQuality = {minQuality}, maxQuality = {maxQuality}");
        }

        public static bool IsDefault<T>(this T self) where T: struct
        {
            return self.Equals(default(T));
        }

        public static bool IsNotDefault<T>(this T self) where T : struct
        {
            return !self.IsDefault();
        }
    }

    public struct ImageTransformSettings
    {
        public readonly int width;
        public readonly int height;
        public readonly int quality;
        public override string ToString() => $"Size={width}x{height} | Quality={quality}";
    }
}
