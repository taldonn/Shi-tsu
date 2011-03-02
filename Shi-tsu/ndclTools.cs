/*    File: ndclTools.cs
 *  Author: Anthony "Ishpeck" Tedjamulia
 * Drafted: 13-Jan-2010
 * 
 * Basic tools found universally in the NDCL projects.
 * 
 * Currently, only contains extension methods to Lists
 * that make it easy to parse out command line argmunts.
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Shi_tsu
{
	/* Command line argument handler.  Used for identifying and applying default values to command line arguments. */
	public static class ndclTools {
		#region Support Methods
		private static bool isValidIdx(List<string> a, int i) {
			return i>=0 && i<a.Count;
		}
		
		private static bool __ClearArgument(List<string> a, int i) {
			if(isValidIdx(a,i)) { 
				a.RemoveAt(i); 
				return true;
			}
			return false;
		}
		
		private static string __ExtractArgument(List<string> a, int i, string assumedValue) {
			return __ClearArgument(a,i) ? assumedValue : null;
		}
		
		private static string ExtractArgumentByIdx(List<string> a, int i) {
			return isValidIdx(a,i) ? __ExtractArgument(a,i,a[i]) : null;
		}
		
		private static string __ExtractParam(List<string> a, int i) {
			return __ClearArgument(a,i) ? ExtractArgumentByIdx(a,i) : null;
		}
		
		#endregion
		
		/* ============================================================================= */
		
		public static string FetchAt(this List<string> argv, int location, string fallback) {
			return (isValidIdx(argv, location)) ? argv[0] : fallback;
		}
		
		public static string FetchAt(this List<string> argv, int location) {
			return argv.FetchAt(location, null);
		}
		
		public static bool FindParam(this List<string> argv, string marker) {
			return __ClearArgument(argv, argv.IndexOf(marker));
		}
		
		public static string ExtractParam(this List<string> argv, string marker, string fallback) {
			return __ExtractParam(argv, argv.IndexOf(marker)) ?? fallback;
		}
		
		public static string YankParam(this List<string> argv, int idx) {
			return ExtractArgumentByIdx(argv, idx);
		}
		
		public static string YankParam(this List<string> argv, string val) {
			return ExtractArgumentByIdx(argv, argv.IndexOf(val));
		}
		
		public static string YankParam(this List<string> argv, int idx, string fallback) {
			return ExtractArgumentByIdx(argv, idx) ?? fallback;
		}
		
		public static string YankNS(this List<string> argv, int idx, string fallback) {
			return argv.YankParam( argv.AbandonSwitches().YankParam(idx) ) ?? fallback;
		}
		
		public static List<string> AbandonSwitches(this List<string> a) {
			return (from p in a where !p.StartsWith("-") select p).ToList();
		}
        
		// Turn -abc into -a -b -c and keep --abc the same
		public static List<string> ParamForm(string []arga) {
			List<string> final = new List<string>();
			arga.ToList().ForEach(arg=>{
				if(arg.StartsWith("-") && !arg.StartsWith("--")) {
					foreach(char c in arg.Replace("-", "")) {
						final.Add(string.Format("-{0}", c));
					}
				} else {
					final.Add(arg);
				}
			});
			return final;
		}
		
		public static List<string> CollectGuids(this List<string> a, string prefix) {
			return a.Where(i=>i.StartsWith(prefix)).Distinct().ToList();
		}
		
		public static List<string> CollectEnvs(this List<string> a, string suffix) {
			return a.Where(i=>i.EndsWith(suffix)).Distinct().ToList();
		}
		
		public static List<string> CollectContaining(this List<string> a, string within) {
			return a.Where(i=>i.Contains(within)).Distinct().ToList();
		}
		
		public static List<string> ExtractGuids(this List<string> a, string prefix) {
			return a.CollectGuids(prefix).Select(i=> {
				a.RemoveAll(j=>j==i);
				return i;
			}).ToList();
		}
		
		public static List<string> ExtractEnvs(this List<string> a, string suffix) {
			return a.CollectEnvs(suffix).Select(i=>{
				a.RemoveAll(j=>i==j);
				return i;
			}).ToList();
		}
		
		public static List<string> ExtractContaining(this List<string> a, string within) {
			return a.CollectContaining(within).Select(i=>{
				a.RemoveAll(j=>j==i);
				return i;
			}).ToList();
		}

		private static KeyValuePair<string, string> ExtractWithOffset(List<string> a, int location, int offset) {
			return new KeyValuePair<string, string>(a.YankParam(location+offset, string.Empty), a.YankParam(location+offset, string.Empty));
		}
		
		public static KeyValuePair<string, string> ExtractWithLeft(this List<string> a, int location) {
			return ExtractWithOffset(a, location, -1);
		}
		
		public static KeyValuePair<string, string> ExtractWithRight(this List<string> a, int location) {
			return ExtractWithOffset(a, location, 0);
		}
		
		public static KeyValuePair<string, string> WithoutThis(this KeyValuePair<string, string> o, string forbidden) {
			return new KeyValuePair<string, string>(o.Key.Replace(forbidden, string.Empty), o.Value.Replace(forbidden, string.Empty));
		}
		
		public static Dictionary<string, string> ExtractAround(this List<string> a, string op) {
			return a.Where(i=>i.Contains(op)).ToList().Select(i=>{
				int loc = a.IndexOf(i);
				if(i==op) {
					return a.ExtractPartners(loc);
				}
				if(i.StartsWith(op)) {
					return a.ExtractWithLeft(loc).WithoutThis(op);
				}
				if(i.EndsWith(op)) {
					return a.ExtractWithRight(loc).WithoutThis(op);
				}
				a.YankParam(i);
				string [] parts = i.Split(op.ToCharArray());
				return new KeyValuePair<string, string>(parts[0], parts[1]);
			}).ToDictionary(i=>i.Key, i=>i.Value);
		}
		
		public static KeyValuePair<string,string> ExtractPartners(this List<string> a, int location) {
			a.YankParam(location);
			return a.ExtractWithLeft(location);
		}
		
		public static string ExtractSession(this List<string> a, string fallback) {
			return a.ExtractParam("-s", fallback);
		}
		
		public static string ExtractUsername(this List<string> a, string fallback) {
			return a.ExtractParam("-u", fallback);
		}
		
		public static string ExtractPassword(this List<string> a, string fallback) {
			return a.ExtractParam("-p", fallback);
		}
		
		public static string goproc(string cmd, string args) {
			ProcessStartInfo ps = new ProcessStartInfo();
			ps.FileName=cmd;
			ps.UseShellExecute=false;
			ps.RedirectStandardOutput=true;
			ps.Arguments = args;
			using(Process proc = Process.Start(ps)) {
				using(StreamReader r = proc.StandardOutput) {
					return r.ReadToEnd();
				}
			}
		}
		
		public static string goproc(string command, params string[] args) {
			return goproc(command, string.Join(" ", args));
		}
		
		public static string fmt(this string s, params Object[] rest) {
			try {
				return string.Format(s, rest);
			} catch (System.FormatException e) {
				return string.Format("{0}\n\n{1}", s, e.Message);
			}
		}
		
		public static string join(this string s, params string[] rest) {
			return string.Join(s, rest);
		}
		
		public static string join(this string s, params Object[] rest) {
			return s.join( rest.Select(i=>i.ToString()).ToArray() );
		}
		
		public static void dump(this Object o) {
			Console.Write("{0}", o);
		}
		
		public static void print(this Object o) {
			Console.WriteLine("{0}",o);
		}
		
		public static T show<T>(this T o) {
			o.print();
			return o;
		}
		
		public static T sho<T>(this T o) {
			o.dump();
			return o;
		}
		
		public static string show(this string o) {
			o.print();
			return o;
		}
		
		public static string show(this string o, params Object[] rest) {
			string p = o.fmt(rest);
			p.print();
			return p;
		}
		
		public static string sho(this string o, params Object[] rest) {
			string p = o.fmt(rest);
			p.dump();
			return p;
		}
		
		public static int err(this string m, int statusNum) {
			m.print();
			return statusNum;
		}
		
		public static int err(this string m, int statusNum, params Object [] rest) {
			m.show(rest);
			return statusNum;
		}
	}
}
