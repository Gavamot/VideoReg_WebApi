using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VideoReg.WebApi.Dto
{
    public class ImageTransformSettingsDto
    {
        public const int MinSizePx = 0;
        public const int MaxSizePx = 1920;
        public const int MinQuality = 0;
        public const int MaxQuality = 100;

        /// <summary>
        /// Ширина изображения (px)
        /// </summary>
        [Range(MinSizePx, MaxSizePx)]
        [Display(Name = "Ширина")]
        [DefaultValue(0)]
        public int Width { get; set; }

        /// <summary>
        /// Высота изображения (px)
        /// </summary>
        [Range(MinSizePx, MaxSizePx)]
        [Display(Name = "Высота")]
        [DefaultValue(0)]
        public int Height { get; set; }

        /// <summary>
        /// Качество изображения (%)
        /// </summary>
        [Range(MinQuality, MaxQuality)]
        [Display(Name = "Качество изображения  %")]
        [DefaultValue(0)]
        public int Quality { get; set; }

        public bool IsDefault() => 
            Height == 0 
            || Width == 0 
            || Quality == 0;
        
    }
}