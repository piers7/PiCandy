using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorldConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = System.Configuration.ConfigurationSettings.AppSettings["baseDir"];
            //= "http://localhost:8021";
            using (WebApp.Start<OwinStartup>(uri))
            {
                Console.WriteLine("Running at {0}", uri);
                Console.WriteLine("Press ENTER to halt");
                Console.ReadLine();
            }
        }
    }
}
