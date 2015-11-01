using Autofac;
using Autofac.Core;
using Autofac.Features.Metadata;
using PiCandy.Server.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Server
{
    public class CoreComponentsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<OpenPixelsServer>()
                .SingleInstance()
                ;

            builder.Register<Func<string, IPositionalMap>>(c =>
            {
                var scope = c.Resolve<ILifetimeScope>();
                return scope.ResolveNamed<IPositionalMap>;
            });

            // map autofac's type to our internal meta type (attempting to cut off autofac reference leakage)
            builder
                .RegisterAdapter<Meta<Lazy<IPixelRenderer>>, Lazy<IPixelRenderer, ChannelMetadata>>(
                    meta => new Lazy<IPixelRenderer, ChannelMetadata>(meta.Value, new ChannelMetadata(meta.Metadata))
                );

            // Create a bunch of zig-zag maps just in case
            for (int i = 1; i <= 8; i++)
            {
                var cadence = i * i;
                var name = "zigZag" + cadence;
                builder
                    .RegisterType<ZigZagPositionalMap>()
                    .WithProperty(TypedParameter.From(cadence))
                    .Named<IPositionalMap>(name)
                    ;
            }
        }
    }
}
