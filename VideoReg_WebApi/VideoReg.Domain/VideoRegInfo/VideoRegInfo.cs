using Microsoft.VisualBasic.CompilerServices;

namespace VideoReg.Domain.VideoRegInfo
{
    public class VideoRegInfo
    {
        public int BrigadeCode { get; set; }
        public string Ip { get; set; }
        public string Vpn { get; set; }

        bool Equals(VideoRegInfo other)
        {
            return BrigadeCode == other.BrigadeCode 
                   && Ip == other.Ip 
                   && Vpn == other.Vpn;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VideoRegInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BrigadeCode;
                hashCode = (hashCode * 397) ^ (Ip != null ? Ip.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Vpn != null ? Vpn.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}