using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace PCGuardWebAPI
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 應用程式啟動時執行的程式碼
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
			//無論如何都使用 JSON 回傳.
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

			//不使用JS駝峰式命名
            //var jsonformatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //jsonformatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

			//改用Json.net的序列化設定，並指定 TypeNameHandling.All 來支援序列化虛擬類別與Interface
            var config = GlobalConfiguration.Configuration;
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
        }
    }
}