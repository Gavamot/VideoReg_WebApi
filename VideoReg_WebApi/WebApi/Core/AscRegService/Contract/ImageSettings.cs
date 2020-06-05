using System.Net.Http.Headers;

namespace WebApi.Contract
{
    public class ImageSettings
    {
        public static ImageSettings GetDefault() => new ImageSettings();

        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        public int Quality { get; set; } = 20;

        public bool IsDefault => Width == 0 || Height == 0 || Quality == 0;
        public bool IsNotDefault => !IsDefault;

        public void Update(ImageSettings obj)
        {
            Width = obj.Width;
            Height = obj.Height;
            Quality = obj.Quality;
        }

        public override string ToString() => $"Size={Width}x{Height} | Quality={Quality}";

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ImageSettings)obj);
        }

        protected bool Equals(ImageSettings other)
        {
            return Width == other.Width
                   && Height == other.Height
                   && Quality == other.Quality;
        }
    }
}
