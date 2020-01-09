using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoReg.Domain.ValueType
{
    public struct DeviceSerialNumber
    {
        public readonly ushort model;
        public readonly ushort number;
        public readonly ushort release;
        public readonly ushort serial;
        public const int ArgsCount = 4;
        public ushort[] Values => new[] {model, number, release, serial};
        public DeviceSerialNumber(IEnumerable<ushort> values)
        {
            var v = values.ToArray();
            
            if(v.Length != ArgsCount)
                throw new ArgumentException($"DeviceSerialNumber constructor array argument must contains {ArgsCount} arguments");
            model = v[0];
            number = v[1];
            release = v[2];
            serial = v[3];
        }
        public override string ToString() => $"{model:X}{number:X}{release:X}{serial:X}";
    }
}
