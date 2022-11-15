using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssenblyBrowserLib
{
    public static class PropertyFormatter
    {
        public static string Format(PropertyInfo propertyInfo) {
            var result = new StringBuilder();

            var getModifiers = string.Empty;
            if (propertyInfo.GetMethod is not null)
            {
                getModifiers = MethodFormatter.GetModifiers(propertyInfo.GetMethod);
            }
            result.Append($"{getModifiers}{propertyInfo.PropertyType.Name}");
            if (propertyInfo.PropertyType.IsGenericType)
            {
                result.Append(GetGenericArguments(propertyInfo.PropertyType.GetGenericArguments()));
            }
            result.Append($" {propertyInfo.Name} {{ ");
            if (getModifiers != string.Empty)
            {
                result.Append($"{getModifiers}get; ");
            }
            if (propertyInfo.SetMethod is not null)
            {
                var setModifiers = MethodFormatter.GetModifiers(propertyInfo.SetMethod);
                result.Append($"{setModifiers}set; ");
            }
            result.Append("}");
            return result.ToString();
        }

        private static string GetGenericArguments(Type[] genericArgumentsTypes)
        {
            var genericArguments = new List<string>();
            foreach (var genericArgumentType in genericArgumentsTypes)
            {
                var genericArgument = genericArgumentType.Name;
                if (genericArgumentType.IsGenericType)
                {
                    genericArgument += GetGenericArguments(genericArgumentType.GetGenericArguments());
                }
                genericArguments.Add(genericArgument);
            }
            return $"<{string.Join(", ", genericArguments)}>";
        }
    }
}
