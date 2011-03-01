using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Shi_tsu
{
    public class FolderWatch
    {
        protected static void outputUsage(bool help = true)
        {
            Console.WriteLine("   USAGE: "+System.Diagnostics.Process.GetCurrentProcess().ProcessName +
                ".exe [OPTIONS] DIRECTORY");
            if (!help)
                Console.WriteLine("   For more information, use folderwatch.exe -h");
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
            help("DIRECTORY", "Path to the directory to be watched.");
            help("-h", "Display this Help Message");
            help("--help", "Same as -h");
            help("-d", "Report file deletions also");
            help("-c", "Don't report created or changed files");
            help("-r", "Don't report renamed files");
            help("-s", "Report Changes in Subdirectories also");
            help("-t timeout", "Set an integer timeout value (in ms). The program will automatically");
            help("", "exit after the timeout has been exceeded.");
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
            if(e.Name != last.Item1 || DateTime.Now > last.Item2)
                Console.WriteLine(e.FullPath.Replace('\\', '/'));
            last = new Tuple<string,DateTime>(e.Name, DateTime.Now.AddMilliseconds(200));
        }

        public static int Main(string[] args)
        {
            List<string> pargs = ParamForm(args);
            if (pargs.Contains("-h") || pargs.Contains("--help"))
            {
                outputHelp();
                return 1;
            }
            if (pargs.Count < 1)
            {
                outputUsage(false);
                return 1;
            }

            FileSystemWatcher watch = new FileSystemWatcher();
            int timeout = -1;
            try
            {
                if (!pargs.Contains("-t"))
                    watch.Path = AbandonSwitches(pargs).First();
                else
                {
                    timeout = int.Parse(AbandonSwitches(pargs)[0]);
                    watch.Path = AbandonSwitches(pargs)[1];
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Error: Directory path does not exist or was not in the right format.");
                outputUsage();
                return 1;
            }
            catch (FormatException e)
            {
                Console.WriteLine("Error: Timeout value not in the right format. Must be an integer.");
                outputUsage();
                return 1;
            }
            
            if (pargs.Contains("-d"))
                watch.Deleted += new FileSystemEventHandler(handler);
            if (!pargs.Contains("-c"))
                watch.Changed += new FileSystemEventHandler(handler);
            if (!pargs.Contains("-r"))
                watch.Renamed += new RenamedEventHandler(handler);
            watch.IncludeSubdirectories = pargs.Contains("-s");

            last = new Tuple<string, DateTime>("", DateTime.Now);

            watch.EnableRaisingEvents = true;
            DateTime started = DateTime.Now;
            if (timeout == -1)
                while (Console.ReadKey(true).KeyChar != 'q') ;
            else
            {
                System.Threading.Thread.Sleep(timeout);
            }
            return 0;
        }
    }
}
