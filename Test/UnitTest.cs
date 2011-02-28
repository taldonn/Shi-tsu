using System;
using Shi_tsu;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace Test
{
    [TestFixture]
    public class UnitTest : FolderWatch 
    {
        protected string PATH = "../../../Shi-tsu/bin/Debug/";
        protected string procName = "folderwatch.exe";
        protected ProcessStartInfo ProcInfo;
        [SetUp]
        public void setupProcInfo()
        {
            ProcInfo = new ProcessStartInfo(PATH + procName);
            ProcInfo.UseShellExecute = false;
            ProcInfo.RedirectStandardOutput = true;
            ProcInfo.RedirectStandardInput = true;
        }
        [Test]
        public void NoArgs()
        {
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo = ProcInfo;
                myProcess.Start();

                Assert.AreEqual("   USAGE: folderwatch.exe [OPTIONS] DIRECTORY",
                    myProcess.StandardOutput.ReadLine());
                Assert.AreEqual("   For more information, use folderwatch.exe -h",
                    myProcess.StandardOutput.ReadLine());
            }
        }
        [Test]
        public void hArg()
        {
            using (Process myProcess = new Process())
            {
                ProcInfo.Arguments = "-h";
                myProcess.StartInfo = ProcInfo;
                myProcess.Start();

                Assert.AreEqual("   USAGE: folderwatch.exe [OPTIONS] DIRECTORY",
                    myProcess.StandardOutput.ReadLine());
                myProcess.StandardOutput.ReadLine();
                myProcess.StandardOutput.ReadLine();
                Assert.AreEqual(string.Format("   {0}{1}", "DIRECTORY".PadRight(17, ' '), "Path to the directory to be watched."),
                    myProcess.StandardOutput.ReadLine());
            }
        }
        [Test]
        public void fileArg()
        {
            using (Process myProcess = new Process())
            {
                ProcInfo.Arguments = ".";
                myProcess.StartInfo = ProcInfo;
                myProcess.Start();
            }
        }
    }
}
