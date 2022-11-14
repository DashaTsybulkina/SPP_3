using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace AssenblyBrowserLib
{
    public class AssemblyParser
    {
        public AssemblyTreeNode GetAssemblyInfo(string filePath)
        {
            var assembly = Assembly.LoadFrom(filePath);
            var assemblyInfo = new AssemblyTreeNode("root");

            var types = assembly.GetTypes();
            var typesByNamespaces = types.ToLookup(grouping => grouping.Namespace);
            var extensionMethodsInfos = new List<MemberMethodInfo>();

            foreach (var typesByNamespace in typesByNamespaces)
            {
                var namespaceNode = new AssemblyTreeNode(typesByNamespace.Key.ToString());

                foreach (var type in typesByNamespace)
                {
                    var typeNode = new AssemblyTreeNode(getNameDeclaration(type))
                    {
                        ChildNodes = GetMembers(type, ref extensionMethodsInfos)
                    };
                    namespaceNode.ChildNodes.Add(typeNode);
                }
                assemblyInfo.ChildNodes.Add(namespaceNode);
            }
            return assemblyInfo;
        }


        private List<AssemblyTreeNode> GetMembers(Type type, ref List<MemberMethodInfo> extensionMethodsInfos)
        {
            var members = new List<AssemblyTreeNode>();

            var membersByType = type.GetMembers(BindingFlags.Instance | BindingFlags.Public |
                                                BindingFlags.NonPublic | BindingFlags.Static |
                                                BindingFlags.DeclaredOnly);

            foreach (var member in membersByType)
            {
                if (member is FieldInfo fieldInfo)
                {
                    members.Add(new AssemblyTreeNode(GetFieldDeclaration(fieldInfo)));
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    members.Add(new AssemblyTreeNode(GetPropertyDeclaration(propertyInfo)));
                }
                else if (member is MethodInfo methodInfo)
                {
                    var methodSignature = GetMethodSignature(methodInfo);

                    if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
                    {
                        extensionMethodsInfos.Add(new MemberMethodInfo(methodInfo, methodSignature));
                    }
                    else
                    {
                        members.Add(new AssemblyTreeNode(methodSignature));
                    }
                }
            }

            return members;
        }

        private string GetFieldDeclaration(FieldInfo fieldInfo)
        {
            var builder = new StringBuilder();
            builder.Append($"{GetFieldModifiers(fieldInfo)}{fieldInfo.FieldType.Name} ");
            if (fieldInfo.FieldType.IsGenericType)
            {
                builder.Append($"{GetParentsGenericArguments(fieldInfo.FieldType.GetGenericArguments())} ");
            }
            builder.Append($"{fieldInfo.Name}");
            return builder.ToString();
        }

        private string GetPropertyDeclaration(PropertyInfo propertyInfo)
        {
            var builder = new StringBuilder();

            var getModifiers = string.Empty;
            var setModifiers = string.Empty;

            if (propertyInfo.GetMethod is not null)
            {
                getModifiers = GetMethodModifiers(propertyInfo.GetMethod);
            }

            if (propertyInfo.SetMethod is not null)
            {
                setModifiers = GetMethodModifiers(propertyInfo.SetMethod);
            }

            builder.Append($"{getModifiers}{propertyInfo.PropertyType.Name}");
            if (propertyInfo.PropertyType.IsGenericType)
            {
                builder.Append(GetParentsGenericArguments(propertyInfo.PropertyType.GetGenericArguments()));
            }
            builder.Append($" {propertyInfo.Name} {{ ");
            if (getModifiers != string.Empty)
            {
                builder.Append($"{getModifiers}get; ");
            }
            if (setModifiers != string.Empty)
            {
                builder.Append($"{setModifiers}set; ");
            }
            builder.Append("}");
            return builder.ToString();
        }

        private string getNameDeclaration(Type type)
        {
            var diclaration = new StringBuilder();
            diclaration.Append($"{getTypeModifiers(type)}{type.Name}");
            if (type.IsGenericType)
            {
                diclaration.Append(GetNameGenericArguments(type.GetGenericArguments()));
            }
            var parents = GetTypesParents(type);
            if (parents != string.Empty)
            {
                diclaration.Append($": {parents} ");
            }
            return diclaration.ToString();
        }

        private string GetNameGenericArguments(Type[] genericArgumentsTypes)
        {
            var genericArguments = new List<string>();
            foreach (var genericArgumentType in genericArgumentsTypes)
            {
                genericArguments.Add(genericArgumentType.Name);
            }
            return $"<{string.Join(", ", genericArguments)}>";
        }

        private string getTypeModifiers(Type type)
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

        private string GetFieldModifiers(FieldInfo fieldInfo)
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

        private string GetMethodSignature(MethodInfo methodInfo)
        {
            var builder = new StringBuilder();

            builder.Append($"{GetMethodModifiers(methodInfo)}{methodInfo.ReturnType.Name} ");

            if (methodInfo.ReturnType.IsGenericType)
            {
                builder.Append($"{GetParentsGenericArguments(methodInfo.ReturnType.GetGenericArguments())} ");
            }

            builder.Append($"{methodInfo.Name}");

            if (methodInfo.IsGenericMethod)
            {
                builder.Append(GetNameGenericArguments(methodInfo.GetGenericArguments()));
            }
            builder.Append($"({GetMethodParameters(methodInfo)})");
            return builder.ToString();
        }

        private string GetMethodModifiers(MethodInfo methodInfo)
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

        private string GetTypesParents(Type type)
        {
            var parents = new List<string>();

            if (type.BaseType is not null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType) &&
                type.BaseType != typeof(Enum))
            {
                var parent = type.BaseType.Name;
                if (type.BaseType.IsGenericType)
                {
                    parent += GetParentsGenericArguments(type.BaseType.GetGenericArguments());
                }
                parents.Add(parent);
            }

            var interfacesTypes = type.GetInterfaces();
            foreach (var interfaceType in interfacesTypes)
            {
                var parent = interfaceType.Name;
                if (interfaceType.IsGenericType)
                {
                    parent += GetParentsGenericArguments(interfaceType.GetGenericArguments());
                }
                parents.Add(parent);
            }
            return string.Join(", ", parents);
        }

        private string GetParentsGenericArguments(Type[] genericArgumentsTypes)
        {
            var genericArguments = new List<string>();
            foreach (var genericArgumentType in genericArgumentsTypes)
            {
                var genericArgument = genericArgumentType.Name;
                if (genericArgumentType.IsGenericType)
                {
                    genericArgument += GetParentsGenericArguments(genericArgumentType.GetGenericArguments());
                }
                genericArguments.Add(genericArgument);
            }
            return $"<{string.Join(", ", genericArguments)}>";
        }

        private string GetMethodParameters(MethodInfo methodInfo)
        {
            var parameters = new List<string>();

            var parametersInfos = methodInfo.GetParameters();
            if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
            {
                if (parametersInfos[0].ParameterType.IsGenericType)
                {
                    parameters.Add($"this {parametersInfos[0].ParameterType.Name} " +
                                   $"{GetParentsGenericArguments(parametersInfos[0].ParameterType.GetGenericArguments())}");
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
                                       $"{GetParentsGenericArguments(parametersInfos[i].ParameterType.GetGenericArguments())}");
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
                                       $"{GetParentsGenericArguments(parameterInfo.ParameterType.GetGenericArguments())}");
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

