using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.IO {
    public enum ByteOrder : ushort {
        System = 0,
        BigEndian = 65279, 
        LittleEndian = 65534, 
    }
}
