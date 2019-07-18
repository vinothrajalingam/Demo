using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Web;
using ARCO.EndPoint.Model;
using log4net;

namespace ARCO.EndPoint.eCard.Controllers
{
    [RoutePrefix("api/Application")]
    public class ApplicationController : ApiController
    {
        private static readonly ILog Log =
              LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        

        public ApplicationController()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        [HttpGet]
        [Route("GetApplicationVersion")]
        public HttpResponseMessage GetApplicationVersion()
        {
            Log.Info(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName));
            HttpResponseMessage resp;
            try
            {
                String _Version = String.Empty;
                _Version = this.GetType().Assembly.GetName().Version.ToString();

                resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(_Version, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
            catch(Exception ex)
            {
                Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                resp.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
        }

        [HttpGet]
        [Route("GetDataSchemaVersion")]
        public HttpResponseMessage  GetDataSchemaVersion()
        {
            Log.Info(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName));
            HttpResponseMessage resp;
            try
            {
                Log.Info("GetDataschemaversion");
                String _TemplateFileName = "PlanLocation.json";
                String Path = String.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), _TemplateFileName);
                
                String allJson = File.ReadAllText(Path);

                JSonSchema _JSonSchema = JsonConvert.DeserializeObject<JSonSchema>(allJson);

                String _Version = _JSonSchema.VERSION;

                resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(_Version, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                resp.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
        }        

        [HttpGet]
        [Route("GetSchema")]
        public HttpResponseMessage GetSchema()
        {
            Log.Info(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName));
            HttpResponseMessage resp;
            try
            {
                String _TemplateFileName = "PlanLocation.json";
                String Path = String.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), _TemplateFileName);

                String allJson = File.ReadAllText(Path);

                resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(allJson, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                resp.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
        }
    }
}
