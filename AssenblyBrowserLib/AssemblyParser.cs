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
            var assemblyInfo = new AssemblyTreeNode("startNode");

            var types = assembly.GetTypes();
            var typesByNamespaces = types.ToLookup(grouping => grouping.Namespace);
            var extensionMethodsInfos = new List<MemberMethodInfo>();

            foreach (var typesByNamespace in typesByNamespaces)
            {
                var namespaceNode = new AssemblyTreeNode(typesByNamespace.Key.ToString());

                foreach (var type in typesByNamespace)
                {
                    var typeNode = new AssemblyTreeNode(ClassFormatter.FormatClass(type))
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
                    members.Add(new AssemblyTreeNode(FieldFormatter.Formatt(fieldInfo)));
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    members.Add(new AssemblyTreeNode(PropertyFormatter.Format(propertyInfo)));
                }
                else if (member is MethodInfo methodInfo)
                {
                    var methodSignature = MethodFormatter.Format(methodInfo);
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
       
    }
}

