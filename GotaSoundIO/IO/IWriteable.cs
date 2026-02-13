using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.IO {
    public interface IWriteable {
        void Write(FileWriter w);
    }
}
