using System;
using Shi_tsu;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class UnitTest : FolderWatch 
    {
        List<string> testArgs;
        MemoryStream output;
        StreamReader consoleout;
        List<string> touchedFiles;
        List<string> touchedDirs;

        [TestFixtureSetUp]
        public void initStreamandArgs()
        {
            testArgs = new List<string>();
            output = new MemoryStream(512);
            StreamWriter consoleredirect = new StreamWriter(output);
            consoleredirect.AutoFlush = true;
            Console.SetOut(consoleredirect);
            consoleout = new StreamReader(output);
            touchedDirs = new List<string>();
            touchedFiles = new List<string>();
            if (File.Exists("test"))
                File.Delete("test");
            if (File.Exists("test1"))
                File.Delete("test1");
            if (Directory.Exists("dir"))
                Directory.Delete("dir");
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
            if (watch != null)
            {
                watch.EnableRaisingEvents = false;
                watch = null;
            }
            foreach (string s in touchedFiles)
            {
                if (File.Exists(s))
                    File.Delete(s);
            }
            touchedFiles.Clear();
            foreach (string s in touchedDirs)
            {
                if (Directory.Exists(s))
                    Directory.Delete(s);
            }
            touchedDirs.Clear();
        }

        [Test]
        public void hArg()
        {
            testArgs.Add("-h");
            Assert.AreEqual(0, Main(testArgs.ToArray()));
            consoleout.BaseStream.Position = 0;
            Assert.AreEqual("USAGE: " + System.Diagnostics.Process.GetCurrentProcess().ProcessName +
                ".exe [OPTIONS] [DIRECTORY]", consoleout.ReadLine());
            string s;
            //throw away other usage lines
            while (!(s = consoleout.ReadLine()).Contains("DIRECTORY")) ;
            Assert.AreEqual(string.Format("   {0}{1}","DIRECTORY".PadRight(17, ' '),
                "Path to the directory to be watched. Defaults to '.'"), s);
        }

        public void doEveryFileOp(string dir)
        {
            FileStream f = File.Open(dir + "/test", FileMode.Create);
            touchedFiles.Add(dir + "/test");
            f.WriteByte(8);
            f.Close();
            //wait for the timeout
            System.Threading.Thread.Sleep(220);
            f = File.Open(dir + "/test", FileMode.Append);
            f.WriteByte(8);
            f.Close();
            //give the file events time to manifest
            System.Threading.Thread.Sleep(20);

            File.Move(dir + "/test", dir + "/test1");
            touchedFiles.Add(dir + "/test1");
            //wait for the timeout
            System.Threading.Thread.Sleep(220);
            File.Delete(dir+"/test1");

            //give the file events time to manifest
            System.Threading.Thread.Sleep(20);
        }
        [Test]
        public void tArgandbasicfunction()
        {
            testArgs.Add("-t");
            testArgs.Add("500");
            DateTime before = DateTime.Now;
            Assert.AreEqual(0, Main(testArgs.ToArray()));
            DateTime after = DateTime.Now;
            Assert.IsTrue(after - before >= TimeSpan.FromMilliseconds(475));

            doEveryFileOp(".");

            //see if it worked as expected.
            consoleout.BaseStream.Position = 0;
            //create
            Assert.AreEqual("./test", consoleout.ReadLine());
            //change
            Assert.AreEqual("./test", consoleout.ReadLine());
            //rename
            Assert.AreEqual("./test1", consoleout.ReadLine());
            //delete
            Assert.IsNull(consoleout.ReadLine());
        }
        [Test]
        public void falsetArg()
        {
            testArgs.Add("-t");
            testArgs.Add("41a");
            Assert.AreEqual(1, Main(testArgs.ToArray()));

            consoleout.BaseStream.Position = 0;
            Assert.AreEqual(timeErr,
                consoleout.ReadLine());
        }
        [Test]
        public void directoryandtArgs()
        {
            testArgs.Add("-t");
            testArgs.Add("30");
            Directory.CreateDirectory("d");
            touchedDirs.Add("d");
            testArgs.Add("d");
            Assert.AreEqual(0, Main(testArgs.ToArray()));

            doEveryFileOp("d");

            consoleout.BaseStream.Position = 0;
            //create
            Assert.AreEqual("d/test", consoleout.ReadLine());
            //change
            Assert.AreEqual("d/test", consoleout.ReadLine());
            //rename
            Assert.AreEqual("d/test1", consoleout.ReadLine());
            //delete
            Assert.IsNull(consoleout.ReadLine());
        }
        [Test]
        public void falsedirarg()
        {
            testArgs.Add("nonexist");
            Assert.AreEqual(1, Main(testArgs.ToArray()));

            consoleout.BaseStream.Position = 0;
            Assert.AreEqual(pathErr,
                consoleout.ReadLine());
        }
        [Test]
        public void dcCrArg()
        {
            testArgs.Add("-dcCrt");
            testArgs.Add("30");
            Directory.CreateDirectory("dcCr");
            touchedDirs.Add("dcCr");
            testArgs.Add("dcCr");
            Assert.AreEqual(0, Main(testArgs.ToArray()));

            doEveryFileOp("dcCr");

            consoleout.BaseStream.Position = 0;
            //delete
            Assert.AreEqual("dcCr/test1", consoleout.ReadLine());
            //nothing else
            Assert.IsNull(consoleout.ReadLine());
        }
        [Test]
        public void sArg()
        {
            testArgs.Add("-st");
            testArgs.Add("30");
            Directory.CreateDirectory("s");
            Directory.CreateDirectory("s/s");
            touchedDirs.Add("s/s");
            touchedDirs.Add("s");
            Assert.AreEqual(0, Main(testArgs.ToArray()));

            doEveryFileOp("s");
            doEveryFileOp("s/s");

            consoleout.BaseStream.Position = 0;
            //create in base directory
            Assert.AreEqual("./s/test", consoleout.ReadLine());
            string s;
            //throw away other current directory lines
            while ((s = consoleout.ReadLine()).StartsWith("./s/t"));
            //create in subdirectory
            Assert.AreEqual("./s/s/test", s);
        }
    }
}
