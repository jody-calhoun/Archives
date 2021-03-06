[assembly: WebActivator.PreApplicationStartMethod(typeof(Jokes.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(Jokes.App_Start.NinjectWebCommon), "Stop")]

namespace Jokes.App_Start
{
  using System;
  using System.Web;
  using System.Web.Http;
  using Jokes.Data;
  using Jokes.Services;
  using Microsoft.Web.Infrastructure.DynamicModuleHelper;
  using Ninject;
  using Ninject.Web.Common;
  using WebApiContrib.IoC.Ninject;

  public static class NinjectWebCommon
  {
    private static readonly Bootstrapper bootstrapper = new Bootstrapper();

    /// <summary>
    /// Starts the application
    /// </summary>
    public static void Start()
    {
      DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
      DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
      bootstrapper.Initialize(CreateKernel);
    }

    /// <summary>
    /// Stops the application.
    /// </summary>
    public static void Stop()
    {
      bootstrapper.ShutDown();
    }

    /// <summary>
    /// Creates the kernel that will manage your application.
    /// </summary>
    /// <returns>The created kernel.</returns>
    private static IKernel CreateKernel()
    {
      var kernel = new StandardKernel();
      kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
      kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

      RegisterServices(kernel);

      GlobalConfiguration.Configuration.DependencyResolver =
        new NinjectResolver(kernel);

      return kernel;
    }

    /// <summary>
    /// Load your modules or register your services here!
    /// </summary>
    /// <param name="kernel">The kernel.</param>
    private static void RegisterServices(IKernel kernel)
    {
#if DEBUG
      kernel.Bind<IMailService>().To<MockMailService>().InRequestScope();
#else
      kernel.Bind<IMailService>().To<MailService>().InRequestScope();
#endif

      kernel.Bind<JokesContext>().To<JokesContext>().InRequestScope();
      kernel.Bind<IJokesRepository>().To<JokesRepository>().InRequestScope();
    }
  }
}
