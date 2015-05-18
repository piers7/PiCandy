using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HelloWorldConsole.OwinStartup))]

namespace HelloWorldConsole
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.UseWelcomePage("/welcome");
            app.Run(context =>
            {
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync(
                    string.Format("{0} {1}",
                        Environment.OSVersion,
                        System.Reflection.Assembly.GetEntryAssembly().FullName
                    )
                    );
            });
        }
    }
}
