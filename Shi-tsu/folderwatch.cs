using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Shi_tsu
{
    public class FolderWatch
    {
        protected static string pathErr 
        { 
            get { return "Error: Directory path does not exist or was not in the right format."; }
        }
        protected static string timeErr
        {
            get { return "Error: Timeout value not in the right format. Must be an integer."; }
        }

        protected static void outputUsage()
        {
            Console.WriteLine("USAGE: "+System.Diagnostics.Process.GetCurrentProcess().ProcessName +
                ".exe [OPTIONS] [DIRECTORY]");
            Console.WriteLine("   Use -t TIMEOUT to specify a timeout value or");
            Console.WriteLine("   send 'q' to end program if no timeout has been specified\n");
        }
        protected static void help(string param, string desc)
        {
            Console.WriteLine(string.Format("   {0}{1}",param.PadRight(17, ' '), desc));
        }
        protected static void outputHelp()
        {
            outputUsage();
            help("DIRECTORY", "Path to the directory to be watched. Defaults to '.'");
            help("-h", "Display this Help Message");
            help("--help", "Same as -h");
            help("-d", "Report file deletions also");
            help("-c", "Don't report changed files");
            help("-C", "Don't report created files");
            help("-r", "Don't report renamed files");
            help("-s", "Report Changes in Subdirectories also");
            help("-t timeout", "Set an integer timeout value (in ms). The program will");
            help("", "automatically exit after the timeout has been exceeded.");
        }
        
        protected static Tuple<string, DateTime> last;
        protected static void handler(object o, FileSystemEventArgs e)
        {
            bool doout = false;
            if (e.ChangeType == WatcherChangeTypes.Deleted)
                doout = true;
            else
                doout = File.Exists(e.FullPath);
            if (doout && (e.Name != last.Item1 || DateTime.Now > last.Item2))
                    Console.WriteLine(e.FullPath.Replace('\\', '/'));

            last = new Tuple<string,DateTime>(e.Name, DateTime.Now.AddMilliseconds(200));
        }
 
        protected static bool getPath(List<string> args, FileSystemWatcher watch)
        {
            try
            {
                watch.Path = args.YankNS(0, ".");
            }
            catch (ArgumentException)
            {
                Console.WriteLine(pathErr);
                outputUsage();
                return false;
            }           
            return true; 
        }
        protected static bool getTimeout(List<string> args, out int timeout)
        {
            try
            {
                timeout = int.Parse(args.ExtractParam("-t", "-1"));
                return true;
            }
            catch (Exception)
            {
                timeout = -1;
                Console.WriteLine(timeErr);
                outputUsage();
                return false;
            }
        }

        protected static FileSystemWatcher watch;
        public static int Main(string[] args)
        {
            List<string> pargs = ndclTools.ParamForm(args);
            if (pargs.FindParam("-h") || pargs.FindParam("--help"))
            {
                outputHelp();
                return 0;
            }
            int timeout;
            if (!getTimeout(pargs, out timeout))
                return 1;

            watch = new FileSystemWatcher();
            if(!getPath(pargs, watch))
                return 1;
            
            if (pargs.FindParam("-d"))
                watch.Deleted += new FileSystemEventHandler(handler);
            if (!pargs.FindParam("-c"))
                watch.Changed += new FileSystemEventHandler(handler);
            if (!pargs.FindParam("-C"))
                watch.Created += new FileSystemEventHandler(handler);
            if (!pargs.FindParam("-r"))
                watch.Renamed += new RenamedEventHandler(handler);
            watch.IncludeSubdirectories = pargs.FindParam("-s");

            last = new Tuple<string, DateTime>("", DateTime.Now);

            watch.EnableRaisingEvents = true;
            if (timeout == -1)
                while (Console.Read() != 'q') ;
            else
                System.Threading.Thread.Sleep(timeout);
            return 0;
        }
    }
}
