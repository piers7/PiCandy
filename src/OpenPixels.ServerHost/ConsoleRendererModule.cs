using Autofac;
using OpenPixels.Server.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    class ConsoleRendererModule : Module
    {
        public int Channel { get; set; }
        public int Width { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new TextWriterRenderer(Console.Out, Width)
                {
                    Channel = Channel
                })
                .As<IPixelRenderer>()
                .WithMetadata<ChannelInfo>(m => 
                    m.For(am => am.Channel, Channel)
                )
                ;
        }
    }
}
