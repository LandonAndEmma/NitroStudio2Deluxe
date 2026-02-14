using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaSoundIO.IO
{
    internal static class EnumExtensions
    {
        private static Dictionary<Type, bool> _flagEnums = new Dictionary<Type, bool>();

        internal static bool IsValid(Type type, object value)
        {
            bool flag = Enum.IsDefined(type, value);
            if (!flag && EnumExtensions.IsFlagsEnum(type))
            {
                long num = 0;
                foreach (object obj in Enum.GetValues(type))
                    num |= Convert.ToInt64(obj);
                long int64 = Convert.ToInt64(value);
                flag = (num & int64) == int64;
            }
            return flag;
        }

        private static bool IsFlagsEnum(Type type)
        {
            bool flag;
            if (!EnumExtensions._flagEnums.TryGetValue(type, out flag))
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(FlagsAttribute), true);
                flag =
                    customAttributes != null
                    && ((IEnumerable<object>)customAttributes).Any<object>();
                EnumExtensions._flagEnums.Add(type, flag);
            }
            return flag;
        }
    }
}
