using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Shi_tsu
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MemoryStream s = new MemoryStream(30))
            {
                StreamWriter w = new StreamWriter(s);
                StreamWriter old = new StreamWriter(Console.OpenStandardOutput());
                old.AutoFlush = true;
                Console.SetOut(w);
                Console.WriteLine("ha!\n");
                Console.SetOut(old);
                StreamReader r = new StreamReader(s);
                r.BaseStream.Position = 0;
                s.WriteTo(Console.OpenStandardOutput());
                Console.WriteLine("ba!" + r.ReadLine());
                Console.ReadLine();
            }
        }
    }
}
