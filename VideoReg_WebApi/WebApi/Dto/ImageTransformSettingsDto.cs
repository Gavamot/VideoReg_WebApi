using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
    public class ImageTransformSettingsDto
    {
        public const int ResetValue = -1;
        /// <summary>
        /// Ширина изображения (px)
        /// </summary>
        [Range(ResetValue, 1024)]
        [Display(Name = "Ширина")]
        public int Width { get; set; }

        /// <summary>
        /// Высота изображения (px)
        /// </summary>
        [Range(ResetValue, 768)]
        [Display(Name = "Высота")]
        public int Height { get; set; }

        /// <summary>
        /// Качество изображения (%)
        /// </summary>
        [Range(ResetValue, 100)]
        [Display(Name = "Качество изображения  %")]
        public int Quality { get; set; }

        public bool IsResetValue() => 
            Height == ResetValue
            || Width == ResetValue
            || Quality == ResetValue;

        public bool IsDefaultValue() =>
           Height == 0
           || Width == 0
           || Quality == 0;

    }
}