using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Emvelope;


namespace ArtDayEmber
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();

            // Web API routes
            config.MapHttpAttributeRoutes();

            GlobalConfiguration.Configuration.Formatters.Insert(0, new MyEmberJsonMediaTypeFormatter());
            

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
