using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.Xml;
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

            foreach (var typesByNamespace in typesByNamespaces)
            {
                var namespaceNode = new AssemblyTreeNode(typesByNamespace.Key.ToString());

                foreach (var type in typesByNamespace)
                {
                    var typeNode = new AssemblyTreeNode(getTypeDeclaration(type));

                    namespaceNode.ChildNodes.Add(typeNode);
                }

                assemblyInfo.ChildNodes.Add(namespaceNode);
            }

            return assemblyInfo;
        }

        private string getTypeDeclaration(Type type)
        {
            var builder = new StringBuilder();

            builder.Append($"{getTypeModifiers(type)}{type.Name}");

            var constraints = string.Empty;

            if (type.IsGenericType)
            {
                builder.Append(GetClassOrMethodGenericArguments(type.GetGenericArguments(), out constraints));
            }

            var parents = GetTypeParents(type);

            if (parents != string.Empty)
            {
                builder.Append($": {parents} ");
            }

            if (constraints != string.Empty)
            {
                builder.Append($" {constraints}");
            }

            return builder.ToString();
        }

        private string GetClassOrMethodGenericArguments(Type[] genericArgumentsTypes, out string constraints)
        {
            var constraintsList = new List<string>();
            var genericArguments = new List<string>();

            foreach (var genericArgumentType in genericArgumentsTypes)
            {
                genericArguments.Add(genericArgumentType.Name);

                if (genericArgumentType.IsGenericParameter)
                {
                    var genericParameterConstraints = GetGenericArgumentConstraints(genericArgumentType);

                    if (genericParameterConstraints != string.Empty)
                    {
                        constraintsList.Add(genericParameterConstraints);
                    }
                }
            }

            constraints = constraintsList.Count > 0 ? $" where {string.Join(",", constraintsList)}" : string.Empty;

            return $"<{string.Join(", ", genericArguments)}>";
        }


        private string getTypeModifiers(Type type)
        {
            var builder = new StringBuilder();

            if (type.IsPublic)
            {
                builder.Append("public ");
            }
            else if (type.IsNotPublic)
            {
                builder.Append("internal ");
            }

            if (type.IsClass)
            {
                if (type.IsAbstract && type.IsSealed)
                {
                    builder.Append("static ");
                }
                else if (type.IsAbstract)
                {
                    builder.Append("abstract ");
                }
                else if (type.IsSealed)
                {
                    builder.Append("sealed ");
                }

                builder.Append("class ");
            }
            else if (type.IsEnum)
            {
                builder.Append("enum ");
            }
            else if (type.IsInterface)
            {
                builder.Append("interface ");
            }
            else if (type.IsValueType && !type.IsPrimitive)
            {
                builder.Append("struct ");
            }

            return builder.ToString();
        }

        private string GetTypeParents(Type type)
        {
            var parents = new List<string>();

            if (type.BaseType is not null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType) &&
                type.BaseType != typeof(Enum))
            {
                var parent = type.BaseType.Name;

                if (type.BaseType.IsGenericType)
                {
                    parent += GetVariableGenericArguments(type.BaseType.GetGenericArguments());
                }

                parents.Add(parent);
            }

            var interfacesTypes = type.GetInterfaces();

            foreach (var interfaceType in interfacesTypes)
            {
                var parent = interfaceType.Name;

                if (interfaceType.IsGenericType)
                {
                    parent += GetVariableGenericArguments(interfaceType.GetGenericArguments());
                }

                parents.Add(parent);
            }

            return string.Join(", ", parents);
        }

        private string GetVariableGenericArguments(Type[] genericArgumentsTypes)
        {
            var genericArguments = new List<string>();

            foreach (var genericArgumentType in genericArgumentsTypes)
            {
                var genericArgument = genericArgumentType.Name;
                if (genericArgumentType.IsGenericType)
                {
                    genericArgument += GetVariableGenericArguments(genericArgumentType.GetGenericArguments());
                }

                genericArguments.Add(genericArgument);
            }

            return $"<{string.Join(", ", genericArguments)}>";
        }

        private string GetGenericArgumentConstraints(Type genericArgument)
        {
            var constraints = new List<string>();

            var genericParameterConstraints =
                genericArgument.GetGenericParameterConstraints();

            foreach (var typeConstraint in genericParameterConstraints)
            {
                constraints.Add(typeConstraint.Name);
            }

            var genericParameterAttributes = genericArgument.GenericParameterAttributes;
            var attributes = genericParameterAttributes &
                             GenericParameterAttributes.SpecialConstraintMask;

            if (attributes != GenericParameterAttributes.None)
            {
                if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                {
                    constraints.Add("class");
                }

                if ((attributes &
                     GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                {
                    constraints.Add("notnull");
                }

                if ((attributes &
                     GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                {
                    constraints.Add("new()");
                }
            }

            return constraints.Count > 0 ? $"{genericArgument.Name}: {string.Join(", ", constraints)}" : string.Empty;
        }
    }
}
