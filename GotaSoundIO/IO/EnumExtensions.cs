using System;
using System.Collections.Generic;
using System.Linq;

namespace GotaSoundIO.IO
{
    internal static class EnumExtensions
    {
        private static readonly Dictionary<Type, bool> _flagEnums = [];

        internal static bool IsValid(Type type, object value)
        {
            bool flag = Enum.IsDefined(type, value);
            if (!flag && EnumExtensions.IsFlagsEnum(type))
            {
                long num = 0;
                foreach (object obj in Enum.GetValues(type))
                {
                    num |= Convert.ToInt64(obj);
                }

                long int64 = Convert.ToInt64(value);
                flag = (num & int64) == int64;
            }
            return flag;
        }

        private static bool IsFlagsEnum(Type type)
        {
            if (!EnumExtensions._flagEnums.TryGetValue(type, out bool flag))
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(FlagsAttribute), true);
                flag =
                    customAttributes != null
                    && customAttributes.Any<object>();
                EnumExtensions._flagEnums.Add(type, flag);
            }
            return flag;
        }
    }
}
