using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssenblyBrowserLib
{
    public static class ClassFormatter
    {
        public static String FormatClass(Type type) {
            var result = new StringBuilder();
            result.Append($"{getTypeModifiers(type)}{type.Name}");
            if (type.IsGenericType)
            {
                result.Append(GetGenericArguments(type.GetGenericArguments()));
            }
            var parents = GetTypesParents(type);
            if (parents != string.Empty)
            {
                result.Append($": {parents} ");
            }
            return result.ToString();
        }

        private static string getTypeModifiers(Type type)
        {
            var modifiers = new StringBuilder();
            if (type.IsPublic)
            {
                modifiers.Append("public ");
            }
            else if (type.IsNotPublic)
            {
                modifiers.Append("internal ");
            }
            if (type.IsClass)
            {
                if (type.IsAbstract && type.IsSealed)
                {
                    modifiers.Append("static ");
                }
                else if (type.IsAbstract)
                {
                    modifiers.Append("abstract ");
                }
                else if (type.IsSealed)
                {
                    modifiers.Append("sealed ");
                }
                modifiers.Append("class ");
            }
            else if (type.IsEnum)
            {
                modifiers.Append("enum ");
            }
            else if (type.IsInterface)
            {
                modifiers.Append("interface ");
            }
            else if (type.IsValueType && !type.IsPrimitive)
            {
                modifiers.Append("struct ");
            }
            return modifiers.ToString();
        }

        private static string GetTypesParents(Type type)
        {
            var parents = new List<string>();

            if (type.BaseType is not null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
            {
                var parent = type.BaseType.Name;
                if (type.BaseType.IsGenericType)
                {
                    parent += GetGenericArguments(type.BaseType.GetGenericArguments());
                }
                parents.Add(parent);
            }

            var interfacesTypes = type.GetInterfaces();
            foreach (var interfaceType in interfacesTypes)
            {
                var parent = interfaceType.Name;
                if (interfaceType.IsGenericType)
                {
                    parent += GetGenericArguments(interfaceType.GetGenericArguments());
                }
                parents.Add(parent);
            }
            return string.Join(", ", parents);
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
