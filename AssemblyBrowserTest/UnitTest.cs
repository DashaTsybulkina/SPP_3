using AssenblyBrowserLib;

namespace AssemblyBrowserTest
{
    [TestFixture]
    public class Tests
    {
        private readonly AssemblyParser parser = new AssemblyParser();
        private const string TestDirectory = "F:\\spp\\third\\AssemblyBrowser\\AssemblyBrowserTest\\TestFiles\\";

        [Test]
        public void DllBrowserParserNotNull()
        {
            parser.GetAssemblyInfo(TestDirectory + "AssenblyBrowserLib.dll");
            Assert.NotNull(parser);
        }

        [Test]
        public void GetCurrectCountNamespaces()
        {
            var result = parser.GetAssemblyInfo(TestDirectory + "AssenblyBrowserLib.dll");
            Assert.True(result.ChildNodes.Count==3);
        }


        [Test]
        public void TestNamespaseTest()
        {
            var result = parser.GetAssemblyInfo(TestDirectory + "TestClass.dll");
            Assert.True(result.ChildNodes[2].Title == "TestClass");
        }

        [Test]
        public void TestClassTest()
        {
            var result = parser.GetAssemblyInfo(TestDirectory + "TestClass.dll");
            List<string> typesTitles = new()
            {
                "public class A",
            };
            var namespaceNode = result.ChildNodes.Find(node => node.Title == "TestClass");
            Assert.True(typesTitles.TrueForAll(s => namespaceNode.ChildNodes.Exists(node => node.Title.Contains(s))));
        }


        [Test]
        public void MethodAmountIsCorrect()
        {
            var result = parser.GetAssemblyInfo(TestDirectory + "TestClass.dll");
            Assert.True(result.ChildNodes[2].ChildNodes[0].ChildNodes.Count == 2);
        }



        [Test]
        public void MethodNotNull()
        {
            var result = parser.GetAssemblyInfo(TestDirectory + "TestClass.dll");
            Assert.NotNull(result.ChildNodes[2].ChildNodes);
        }
    }
}