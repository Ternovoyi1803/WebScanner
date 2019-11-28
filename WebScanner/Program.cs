using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            var scanner = new UrlScanner("https://ustimov.org/posts/20/", "hello", 1000, 10);
            Task task = scanner.DoScan();
            task.Wait();

            Console.ReadKey();
        }
    }
}
