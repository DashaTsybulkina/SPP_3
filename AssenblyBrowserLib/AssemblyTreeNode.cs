using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssenblyBrowserLib
{
    public class AssemblyTreeNode
    {
        public string Title { get; set; }
        public List<AssemblyTreeNode> ChildNodes { get; set; } = new();
        public AssemblyTreeNode(string title)
        {
            this.Title = title;
        }
    }
}
