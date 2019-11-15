using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf.Core.Helpers
{
    public static class StringHelpers
    {
        public static string GetDescription<T>(this T enumValue, bool lowerCase = false) where T : struct, IConvertible
        {

            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            string description = enumValue.ToString();

            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (null != fieldInfo)
            {
                object[] attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            if (lowerCase && !string.IsNullOrWhiteSpace(description))
            {
                description = description.ToLower();
            }

            return description;
        }
    }
}
