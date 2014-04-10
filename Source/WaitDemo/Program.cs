using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using UnixSignalWaiter;

namespace WaitDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start owin and trying to wait signal");
            using (WebApp.Start<Startup>("http://+:12345"))
            {
                try
                {
                    //wait until SIGINT/SIGTERM comes
                    SignalWaiter.Instance.WaitExitSignal();
                }
                catch (InvalidOperationException)
                {
                    Console.Error.WriteLine("Cannot wait exit signal. is Windows platform?");
                }
            }
        }
    }
}
