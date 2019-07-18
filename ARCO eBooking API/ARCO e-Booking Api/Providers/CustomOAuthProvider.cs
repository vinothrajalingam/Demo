using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ARCO.EndPoint.eCard.Providers
{
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
		private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(CustomOAuthProvider));

		public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
		{
			context.Validated();
			return Task.FromResult<object>(null);
		}

		public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
		{
			try
			{
				_log.Info("Inside GrantResourceOwnerCredentials");
				// Get the context for the local Active Directory

				using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, WebConfigurationManager.AppSettings["Domain"])) 
				{
					bool isValid = pc.ValidateCredentials(context.UserName, context.Password);
					if (isValid)
					{
						var identity = new ClaimsIdentity("JWT");

						// Get the user from the Active Directory pool
						UserPrincipal user = UserPrincipal.FindByIdentity(pc, context.UserName);
						
						identity.AddClaim(new Claim("firstName", user.GivenName));
						identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
						
						user.GetGroups().ToList().ForEach(p =>
						{
							identity.AddClaim(new Claim(ClaimTypes.Role, p.Name));
						});

						if (Convert.ToBoolean(WebConfigurationManager.AppSettings["SuperAdmin"]))
							identity.AddClaim(new Claim(ClaimTypes.Role, WebConfigurationManager.AppSettings["ManagerRoleName"]));
						
						if (!Convert.ToBoolean(WebConfigurationManager.AppSettings["SuperAdmin"])
							&& !(identity.HasClaim((c) => c.Type == ClaimTypes.Role && c.Value == WebConfigurationManager.AppSettings["CSRRoleName"])
							|| identity.HasClaim((c) => c.Type == ClaimTypes.Role && c.Value == WebConfigurationManager.AppSettings["ManagerRoleName"])))
						{
							context.SetError("invalid_grant", "You do not have permission to access this resource.");
							return;
						}

						_log.Info("Valid grant");
						context.Validated(identity);
						return;
					}
				}
			}
			catch (Exception ex)
			{
				_log.Error("Error inside GrantResourceOwnerCredentials", ex);
                _log.Info("invalid grant", ex);
                context.SetError("invalid_grant", ex.Message);
                return;
            }

			
		}
	}
}
