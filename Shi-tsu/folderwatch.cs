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

        // Turn -abc into -a -b -c and keep --abc the same
        protected static List<string> ParamForm(string[] arga)
        {
            List<string> final = new List<string>();
            arga.ToList().ForEach(arg =>
            {
                if (arg.StartsWith("-") && !arg.StartsWith("--"))
                {
                    foreach (char c in arg.Replace("-", ""))
                    {
                        final.Add(string.Format("-{0}", c));
                    }
                }
                else
                {
                    final.Add(arg);
                }
            });
            return final;
        }

        protected static List<string> AbandonSwitches(List<string> a)
        {
            return (from p in a where !p.StartsWith("-") select p).ToList();
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
 
        protected static int getPath(List<string> args, FileSystemWatcher watch)
        {            
            if (AbandonSwitches(args).Count < 
                (args.Contains("-t") ? 2 : 1))
            {
                watch.Path = ".";
                return 0;
            }
            else
            {
                try
                {
                    if (!args.Contains("-t"))
                        watch.Path = AbandonSwitches(args).First();
                    else
                        watch.Path = AbandonSwitches(args)[1];
                }
                catch (ArgumentException)
                {
                    Console.WriteLine(pathErr);
                    outputUsage();
                    return 1;
                }           
                return 0; 
            }
        }
        protected static int getTimeout(List<string> args, out int timeout)
        {
            if (args.Contains("-t"))
                try
                {
                    timeout = int.Parse(AbandonSwitches(args).First());
                }
                catch (Exception)
                {
                    timeout = -1;
                    Console.WriteLine(timeErr);
                    outputUsage();
                    return 1;
                }
            else
                timeout = -1;
            return 0;
        }

        protected static FileSystemWatcher watch;
        public static int Main(string[] args)
        {
            List<string> pargs = ParamForm(args);
            if (pargs.Contains("-h") || pargs.Contains("--help"))
            {
                outputHelp();
                return 0;
            }
            watch = new FileSystemWatcher();
            if(getPath(pargs, watch) == 1)
                return 1;

            int timeout;
            if (getTimeout(pargs, out timeout) == 1)
                return 1;
            
            if (pargs.Contains("-d"))
                watch.Deleted += new FileSystemEventHandler(handler);
            if (!pargs.Contains("-c"))
                watch.Changed += new FileSystemEventHandler(handler);
            if (!pargs.Contains("-C"))
                watch.Created += new FileSystemEventHandler(handler);
            if (!pargs.Contains("-r"))
                watch.Renamed += new RenamedEventHandler(handler);
            watch.IncludeSubdirectories = pargs.Contains("-s");

            last = new Tuple<string, DateTime>("", DateTime.Now);

            watch.EnableRaisingEvents = true;
            if (timeout == -1)
                while (Console.ReadKey(true).KeyChar != 'q') ;
            else
                System.Threading.Thread.Sleep(timeout);
            return 0;
        }
    }
}
