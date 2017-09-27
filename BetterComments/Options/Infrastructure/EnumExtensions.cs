using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BetterComments.Options {

    public static class EnumExtensions 
    {

        public static T GetAttribute<T>(this Enum enumValue) where T : Attribute 
        {
            var enumType = enumValue.GetType();
            var member = enumType.GetMember(enumValue.ToString());
            if (member != null && member.Count() != 0) 
            { 
                return member[0].GetCustomAttribute<T>(false);
            }
            return default(T);
        }

    }
}
