using GotaSoundIO.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GotaSequenceLib {
    public static class VariableLength {
        public static uint ReadVariableLength(FileReader r, int limit = -1) {
            uint temp = (uint)r.ReadByte();
            uint val = (uint)temp & 0x7F;
            int bytesRead = 1;
            while ((temp & 0x80) > 0 && (limit == -1 || bytesRead < limit)) {
                val <<= 7;
                temp = r.ReadByte();
                bytesRead++;
                val |= temp & 0x7F;
            }
            return val;
        }
        public static void WriteVariableLength(FileWriter w, uint val) {
            List<byte> nums = new List<byte>();
            while (val > 0) {
                nums.Insert(0, (byte)(val & 0x7F));
                val >>= 7;
            }
            for (int i = 0; i < nums.Count - 1; i++) {
                nums[i] |= 0x80;
            }
            if (nums.Count < 1) {
                nums.Add(0);
            }
            w.Write(nums.ToArray());
        }
        public static int CalcVariableLengthSize(uint val) {
            List<byte> nums = new List<byte>();
            while (val > 0) {
                nums.Insert(0, (byte)(val & 0x7F));
                val >>= 7;
            }
            for (int i = 0; i < nums.Count - 1; i++) {
                nums[i] |= 0x80;
            }
            if (nums.Count < 1) {
                nums.Add(0);
            }
            return nums.Count;
        }
    }
}
