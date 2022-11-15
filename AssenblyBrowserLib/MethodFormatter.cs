using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AssenblyBrowserLib
{
    public static class MethodFormatter
    {
        public static string Format(MethodInfo methodInfo) {
            var builder = new StringBuilder();
            builder.Append($"{GetModifiers(methodInfo)}{methodInfo.ReturnType.Name} ");

            if (methodInfo.ReturnType.IsGenericType)
            {
                builder.Append($"{GetGenericArguments(methodInfo.ReturnType.GetGenericArguments())} ");
            }

            builder.Append($"{methodInfo.Name}");
            builder.Append($"({GetMethodParameters(methodInfo)})");
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

        public static string GetModifiers(MethodInfo methodInfo)
        {
            var builder = new StringBuilder();
            if (methodInfo.IsFamily)
            {
                builder.Append("protected ");
            }
            else if (methodInfo.IsAssembly)
            {
                builder.Append("internal ");
            }
            else if (methodInfo.IsFamilyOrAssembly)
            {
                builder.Append("protected internal ");
            }
            else if (methodInfo.IsFamilyAndAssembly)
            {
                builder.Append("private protected ");
            }
            else if (methodInfo.IsPrivate)
            {
                builder.Append("private ");
            }
            else if (methodInfo.IsPublic)
            {
                builder.Append("public ");
            }
            if (methodInfo.IsStatic)
            {
                builder.Append("static ");
            }
            if (methodInfo.IsAbstract)
            {
                builder.Append("abstract ");
            }
            else if (methodInfo.IsVirtual)
            {
                builder.Append("virtual ");
            }
            return builder.ToString();
        }

        private static string GetMethodParameters(MethodInfo methodInfo)
        {
            var parameters = new List<string>();

            var parametersInfos = methodInfo.GetParameters();
            if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
            {
                if (parametersInfos[0].ParameterType.IsGenericType)
                {
                    parameters.Add($"this {parametersInfos[0].ParameterType.Name} " +
                                   $"{GetGenericArguments(parametersInfos[0].ParameterType.GetGenericArguments())}");
                }
                else
                {
                    parameters.Add($"this {parametersInfos[0].ParameterType.Name} {parametersInfos[0].Name}");
                }

                for (var i = 1; i < parametersInfos.Length; i++)
                {
                    if (parametersInfos[i].ParameterType.IsGenericType)
                    {
                        parameters.Add($"{parametersInfos[i].ParameterType.Name} " +
                                       $"{GetGenericArguments(parametersInfos[i].ParameterType.GetGenericArguments())}");
                    }
                    else
                    {
                        parameters.Add($"{parametersInfos[i].ParameterType.Name} {parametersInfos[i].Name}");
                    }
                }
            }
            else
            {
                foreach (var parameterInfo in parametersInfos)
                {
                    if (parameterInfo.ParameterType.IsGenericType)
                    {
                        parameters.Add($"{parameterInfo.ParameterType.Name} " +
                                       $"{GetGenericArguments(parameterInfo.ParameterType.GetGenericArguments())}");
                    }
                    else
                    {
                        parameters.Add($"{parameterInfo.ParameterType.Name} {parameterInfo.Name}");
                    }
                }
            }

            return parameters.Count > 0 ? string.Join(", ", parameters) : string.Empty;
        }
    }
}
