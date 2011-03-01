using System;
using Shi_tsu;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class UnitTest : FolderWatch 
    {
        List<string> testArgs;
        MemoryStream output;
        StreamReader consoleout;

        [TestFixtureSetUp]
        public void initStreamandArgs()
        {
            testArgs = new List<string>();
            output = new MemoryStream(512);
            StreamWriter consoleredirect = new StreamWriter(output);
            consoleredirect.AutoFlush = true;
            Console.SetOut(consoleredirect);
            consoleout = new StreamReader(output);
        }
        [TestFixtureTearDown]
        public void deallocateMS()
        {
            output.Close();
            output.Dispose();
            output = null;
        }

        [TearDown]
        public void teardown()
        {
            output.Position = 0;
            testArgs.Clear();
        }

        [Test]
        public void hArg()
        {
            testArgs.Add("-h");
            Assert.AreEqual(0, Main(testArgs.ToArray()));
            consoleout.BaseStream.Position = 0;
            Assert.AreEqual("USAGE: " + System.Diagnostics.Process.GetCurrentProcess().ProcessName +
                ".exe [OPTIONS] [DIRECTORY]", consoleout.ReadLine());
            consoleout.ReadLine();
            consoleout.ReadLine();
            consoleout.ReadLine();
            Assert.AreEqual(string.Format("   {0}{1}","DIRECTORY".PadRight(17, ' '),
                "Path to the directory to be watched. Defaults to '.'"), consoleout.ReadLine());
        }
        [Test]
        public void tArg()
        {
            testArgs.Add("-t");
            testArgs.Add("500");
            DateTime before = DateTime.Now;
            Assert.AreEqual(0, Main(testArgs.ToArray()));
            DateTime after = DateTime.Now;
            Assert.IsTrue(after - before >= TimeSpan.FromMilliseconds(500));
        }
        [Test]
        public void directoryArg()
        {
        }
        [Test]
        public void directoryandtArgs()
        {
        }
        [Test]
        public void dcrArg()
        {
        }
        [Test]
        public void sArg()
        {
        }
    }
}
