using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Logging
{
    /// <summary>
    /// Sets up automatic DI for service dependencies on <typeparamref name="TLogger"/>,
    /// via an external factory that resolves based on the type of the resolver
    /// </summary>
    /// <remarks>Unlike the sample in the docs, this has no dependency on any logging
    /// framework, and is basically just a means to resolve any ctor dependency
    /// using a Func<Type,IService> factory
    /// </remarks>
    /// <example>
    /// Use it with log4net like this:
    /// <![CDATA[
    ///     builder.RegisterModule(new LogInjectionModule<log4net.ILog>(log4net.LogManager.GetLogger));
    /// ]]>
    /// </example>
    public class LogInjectionModule<TLogger> : Module
    {
        private readonly Func<Type, TLogger> _logFactory;

        public LogInjectionModule(Func<Type,TLogger> logFactory)
        {
            _logFactory = logFactory;
        }

        public bool InjectProperties { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            // Add the logging factory method, in case it is required seperately
            builder.RegisterInstance(_logFactory);
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            registration.Preparing += registration_Preparing;

            if(InjectProperties)
                registration.Activated += registration_Activated;
        }

        /// <summary>
        /// Adds into every component's registration
        /// a property that resolves to the logger for that type
        /// See http://docs.autofac.org/en/latest/examples/log4net.html
        /// </summary>
        void registration_Preparing(object sender, PreparingEventArgs e)
        {
            e.Parameters = e.Parameters
                .Concat(new[]{
                    new ResolvedParameter(
                        (p,context) => p.ParameterType == typeof(TLogger),
                        (p,context) => _logFactory(p.Member.DeclaringType)
                    )
                });
        }

        /// <summary>
        /// Adds into every component's activation sequence a sweep
        /// of all properties, and injects the logger for the type
        /// as required
        /// </summary>
        /// <remarks>This does seem a heavy-handed way to do this,
        /// but it's hard to re-use the existing property injection functions
        /// within autofac itself</remarks>
        void registration_Activated(object sender, ActivatedEventArgs<object> e)
        {
            var limitType = e.Instance.GetType();

            // Lazy so that we only bother creating the logger
            // for types that actually have a TLogger property
            var logger = new Lazy<TLogger>(() => _logFactory(limitType));

            // pity we have to do this 'by hand' as it were
            foreach (var property in limitType.GetProperties())
            {
                if (property.PropertyType == typeof(TLogger)
                    && property.GetIndexParameters().Length == 0
                    && property.GetValue(e.Instance) == null
                    )
                    property.SetValue(e.Instance, logger.Value);
            }
        }
    }
}
