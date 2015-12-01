using Autofac;
using PiCandy.Rendering;
using PiCandy.Server.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Server
{
    class ConsoleRendererModule : Module
    {
        public int Channel { get; set; }
        public string Map { get; set; }
        public int Pixels { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new TextWriterRenderer(Console.Out, Pixels)
                {
                    Channel = Channel
                })
                .As<IPixelRenderer>()
                .WithMetadata<ChannelMetadata>(m => 
                    {
                        m.For(am => am.Channel, Channel);
                        m.For(am => am.Map, Map);
                    }
                )
                ;
        }
    }
}
