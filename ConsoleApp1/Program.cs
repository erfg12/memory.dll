using Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
    
       

        static async Task Main(string[] args)
        {
            Mem mem = new Mem();
            var isOpen = mem.OpenProcess("so2game.exe");

            Console.WriteLine($"isOpen = {isOpen}");

            if (!isOpen)
                return;

            List<long> addr = new List<long>();
            addr.AddRange(await mem.AoBScan("38 12 72 00 27 27 00 00 ?? 00 00 00 2C 01", true));

            if (!addr.Any())
            {
                Console.WriteLine("No Results Found!");
                return;
            }


            foreach (var addy in addr.ToArray())
            {
                Console.WriteLine(addy.ToString("X"));
            }

        }
    }
}
