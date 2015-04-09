using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;  
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.Web.WebSockets;
using Wamplash.Redis;
using Wamplash.Server.Controllers;

namespace Wamplash.Server
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BuildContainer();
        }

        void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<CacheSharpWampWebSocketHandler>().As<WebSocketHandler>();
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

        }
    }
}
