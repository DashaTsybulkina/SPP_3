using AssenblyBrowserLib;

namespace Tests
{
    [TestClass]
    public class TestUnit
    {
        private readonly AssemblyParser parser = new AssemblyParser();
        private const string TestDirectory = "F:\\spp\\third\\AssemblyBrowser\\Tests\\TestFiles\\";

        [TestMethod]
        public void DllBrowserParserNotNull()
        {
            parser.GetAssemblyInfo(TestDirectory + "AssemblyBrowserLib.dll");
            Assert.NotNull(parser);
        }

        [TestMethod]
        public void ExeBrowserWorkFinishedCorrectly()
        {
            parser.GetAssemblyInfo(TestDirectory + "TestClass.exe");

            Assert.True(true);
        }

        [TestMethod]
        public void ExeBrowserWorkFinishedCorrectly2()
        {
            parser.GetAssemblyInfo(TestDirectory + "View.exe");

            Assert.True(true);
        }

        [TestMethod]
        public void NameSpaceIsCorrect()
        {
            var assemblyInfo = parser.GetAssemblyInfo(TestDirectory + "TestClass.exe");
            Assert.IsTrue(assemblyInfo[0].Signature.Equals("TestClass"));
        }

        [TestMethod]
        public void ClassIsCorrect()
        {
            var assemblyInfo = parser.GetAssemblyInfo(TestDirectory + "TestClass.exe");
            ;
            Assert.IsTrue(assemblyInfo[0].Class.Equals("public  class  A"));
        }

        [TestMethod]
        public void MethodAmountIsCorrect()
        {
            var assemblyInfo = parser.GetAssemblyInfo(TestDirectory + "TestClass.exe");
            ;
            Assert.AreEqual(assemblyInfo[0].Members.Count, 3);
        }

        [TestMethod]
        public void MethodNotNull()
        {
            var assemblyInfo = parser.GetAssemblyInfo(TestDirectory + "TestClass.exe");
            ;
            Assert.NotNull(assemblyInfo[1].Members);
        }
    }
}