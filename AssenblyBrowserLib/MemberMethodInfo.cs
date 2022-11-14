using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssenblyBrowserLib
{
    public class MemberMethodInfo
    {
        public MethodInfo MethodInfo { get; set; }
        public string Signature { get; set; }
        public MemberMethodInfo(MethodInfo methodInfo, string methodSignature)
        {
            Signature = methodSignature;
            MethodInfo = methodInfo;
        }
    }
}
