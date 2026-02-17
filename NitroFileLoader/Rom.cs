using GotaSoundIO.IO;
using System;

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
