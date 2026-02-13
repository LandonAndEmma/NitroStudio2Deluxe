using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSoundIO.IO {
    public abstract class Version : IReadable, IWriteable {
        public byte Major;
        public byte Minor;
        public byte Revision;
        public abstract void Read(FileReader r);
        public abstract void Write(FileWriter w);
        public static bool operator >(Version v1, Version v2) {
            if (v1.Major > v2.Major) { return true; }
            if (v1.Major < v2.Major) { return false; }
            if (v1.Minor > v2.Minor) { return true; }
            if (v1.Minor < v2.Minor) { return false; }
            if (v1.Revision > v2.Revision) { return true; }
            return false;
        }
        public static bool operator <(Version v1, Version v2) {
            if (v1.Major < v2.Major) { return true; }
            if (v1.Major > v2.Major) { return false; }
            if (v1.Minor < v2.Minor) { return true; }
            if (v1.Minor > v2.Minor) { return false; }
            if (v1.Revision < v2.Revision) { return true; }
            return false;
        }
        public static bool operator <=(Version v1, Version v2) {
            return v1 < v2 || v1 == v2;
        }
        public static bool operator >=(Version v1, Version v2) {
            return v1 > v2 || v1 == v2;
        }
        public static bool operator ==(Version v1, Version v2) {
            return v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Revision == v2.Revision;
        }
        public static bool operator !=(Version v1, Version v2) {
            return !(v1 == v2);
        }
        public override bool Equals(object obj) {
            if (obj as Version != null) {
                return (obj as Version) == this;
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            return Major.GetHashCode() * Minor.GetHashCode() * Revision.GetHashCode();
        }
    }
}
