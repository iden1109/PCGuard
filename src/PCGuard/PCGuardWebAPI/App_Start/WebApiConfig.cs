using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;


namespace PCGuardWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 設定和服務
            var cors = new EnableCorsAttribute(
                                    origins: "*",
                                    headers: "*",
                                    methods: "*");
            config.EnableCors(cors);

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{fileName}/{nameSpaceName}/{className}/{methodName}/{*pathInfo}",
                defaults: new
                {
                    controller = "DummiesApiHost",
                    id = RouteParameter.Optional
                }
            );
        }
    }
}
