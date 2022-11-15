using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssenblyBrowserLib
{
    public static class FieldFormatter
    {
        public static string Formatt(FieldInfo fieldInfo) {
            var builder = new StringBuilder();
            builder.Append($"{GetModifiers(fieldInfo)}{fieldInfo.FieldType.Name} ");
            if (fieldInfo.FieldType.IsGenericType)
            {
                builder.Append($"{GetGenericArguments(fieldInfo.FieldType.GetGenericArguments())} ");
            }
            builder.Append($"{fieldInfo.Name}");
            return builder.ToString();
        }

        private static string GetModifiers(FieldInfo fieldInfo)
        {
            var builder = new StringBuilder();

            if (fieldInfo.IsFamily && fieldInfo.IsAssembly)
            {
                builder.Append("protected internal ");
            }
            else if (fieldInfo.IsFamily)
            {
                builder.Append("protected ");
            }
            else if (fieldInfo.IsAssembly)
            {
                builder.Append("internal ");
            }
            else if (fieldInfo.IsFamilyOrAssembly)
            {
                builder.Append("public protected ");
            }
            else if (fieldInfo.IsFamilyAndAssembly)
            {
                builder.Append("private protected ");
            }
            else if (fieldInfo.IsPrivate)
            {
                builder.Append("private ");
            }
            else if (fieldInfo.IsPublic)
            {
                builder.Append("public ");
            }
            if (fieldInfo.IsLiteral)
            {
                builder.Append("const ");
            }
            else if (fieldInfo.IsInitOnly)
            {
                builder.Append("readonly ");
            }
            if (fieldInfo.IsStatic)
            {
                builder.Append("static ");
            }
            return builder.ToString();
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
