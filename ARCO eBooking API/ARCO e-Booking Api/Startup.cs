using System;
using System.Threading.Tasks;
using Microsoft.Owin.Cors;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using log4net;
using Microsoft.Owin.Security.OAuth;
using System.Web.Configuration;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using ARCO.EndPoint.eCard.Providers;
using ARCO.EndPoint.eCard.ErrorHandling;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ARCO.EndPoint.eCard.Startup))]

namespace ARCO.EndPoint.eCard
{
    public class Startup
    {
        private static readonly ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void Configuration(IAppBuilder app)
        {
            log4net.Config.XmlConfigurator.Configure();

            app.UseCors(CorsOptions.AllowAll);

            HttpConfiguration config = new HttpConfiguration();


            config.Services.Replace(typeof(IExceptionHandler), new PassthroughHandler(config.Services.GetExceptionHandler()));

            ConfigureAuth(app);

            WebApiConfig.Register(config);
            app.UseWebApi(config);
            _log.Info("End of Owin startup configuration.");

        }

        private void ConfigureAuth(IAppBuilder app)
        {
            ConfigureOAuthTokenGeneration(app);
            ConfigureOAuthTokenConsumption(app);
        }

        public void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            _log.Info("Configuring OAuth Token Generation");
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat(WebConfigurationManager.AppSettings["Issuer"]),

            };

            app.UseOAuthAuthorizationServer(OAuthServerOptions);
        }

        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            _log.Info("Configuring OAuth Token Consumption");
            var issuer = WebConfigurationManager.AppSettings["Issuer"];
            string audienceId = WebConfigurationManager.AppSettings["AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(WebConfigurationManager.AppSettings["AudienceSecret"]);

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                    {
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    }
                });
        }
    }
}
