namespace WebApi.Contract
{
    public class CameraSettings
    {
        public int Camera { get; set; }
        public bool Enabled { get; set; } = true;
        public ImageSettings Settings { get; set; } = ImageSettings.GetDefault();

        public bool EnableConversion { get; set; } = false;

        public const int SettingsForAllCameras = 0;
        public bool IsSettingsForAllCameras => Camera == SettingsForAllCameras;

        public CameraSettings() { }

        public CameraSettings(int camera) : this()
        {
            Camera = camera;
        }

        public CameraSettings(int camera, bool enabled, bool enableConversion, ImageSettings settings) : this(camera)
        {
            Enabled = enabled;
            EnableConversion = enableConversion;
            Settings = settings;
        }

        public void Update(CameraSettings obj)
        {
            Enabled = obj.Enabled;
            EnableConversion = obj.EnableConversion;
            Settings.Update(obj.Settings);
        }

        public static CameraSettings GetDefault(int camera) => new CameraSettings(camera);

        public override string ToString() => $"camera[{Camera}]={Settings}";

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Camera;
                hashCode += (hashCode * 397) ^ Enabled.GetHashCode();
                if (Settings != null) 
                    hashCode += (hashCode * 397) ^ Settings.GetHashCode();
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
            return Camera == other.Camera 
                && EnableConversion == other.EnableConversion && Settings.Equals(other.Settings);
        }
    }
}
