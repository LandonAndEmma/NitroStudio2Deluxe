using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundIO.IO
{
    public abstract class IOFile : IReadable, IWriteable
    {
        public Version Version;
        public ByteOrder ByteOrder;

        public IOFile() { }

        public IOFile(Stream s)
        {
            Read(s);
        }

        public IOFile(byte[] file)
        {
            Read(file);
        }

        public IOFile(string filePath)
        {
            Read(filePath);
        }

        public void Read(string filePath)
        {
            using (var f = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                Read(f);
            }
        }

        public void Read(byte[] file)
        {
            using (var m = new MemoryStream(file))
            {
                Read(m);
            }
        }

        public void Read(Stream s)
        {
            FileReader r = new FileReader(s);
            Read(r);
            r.Dispose();
        }

        public abstract void Read(FileReader r);
        public abstract void Write(FileWriter w);

        public void Write(Stream s)
        {
            using (FileWriter w = new FileWriter(s))
            {
                Write(w);
            }
        }

        public byte[] Write()
        {
            using (var f = new MemoryStream())
            {
                Write(f);
                return f.ToArray();
            }
        }

        public void Write(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (
                FileStream f = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)
            )
            {
                Write(f);
            }
        }

        public T Duplicate<T>()
            where T : IOFile
        {
            T ret = Activator.CreateInstance<T>();
            ret.Read(Write());
            return ret;
        }

        public string Md5Sum
        {
            get
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(Write());
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
