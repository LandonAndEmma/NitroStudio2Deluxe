using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.IO {
    public enum StringDataFormat {
        DynamicByteCount,
        ByteCharCount,
        Int16CharCount,
        Int32CharCount,
        ZeroTerminated,
        Raw,
    }
}
