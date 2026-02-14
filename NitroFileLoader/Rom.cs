using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GotaSoundIO.IO;

namespace NitroFileLoader
{
    public class Rom : IOFile
    {
        public string GameName;
        public string GameCode;

        public override void Read(FileReader r)
        {
            throw new NotImplementedException();
        }

        public override void Write(FileWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
