﻿using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.Web.WebSockets;
using Wamplash.Azure;
using Wamplash.Handlers;
using Wamplash.Redis.Handlers;
using Wamplash.Server.Handlers;

namespace Wamplash.Server
{
    public class WebApiApplication : HttpApplication
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

        private void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AppSettingsSynchronizationPolicy>().As<ISynchronizationPolicy>();
            builder.RegisterType<DemoRoleDescriber>().As<IRoleDescriber>();
            builder.RegisterType<RedisWampWebSocketHandler>().As<WebSocketHandler>();
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}