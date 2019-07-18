using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Thinktecture.IdentityModel.Tokens;

namespace ARCO.EndPoint.eCard.Providers
{
    public class CustomJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
		private readonly string _issuer = string.Empty;

		public CustomJwtFormat(string issuer)
		{
			_issuer = issuer;
		}

		public string Protect(AuthenticationTicket data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			string audienceId = WebConfigurationManager.AppSettings["AudienceId"];

			string symmetricKeyAsBase64 = WebConfigurationManager.AppSettings["AudienceSecret"];

			var keyByteArray = TextEncodings.Base64Url.Decode(symmetricKeyAsBase64);

			var signingKey = new HmacSigningCredentials(keyByteArray);

			var issued = data.Properties.IssuedUtc;

			var expires = data.Properties.ExpiresUtc;

			var token = new JwtSecurityToken(_issuer, audienceId, data.Identity.Claims, issued.Value.UtcDateTime, expires.Value.UtcDateTime, signingKey);

			var handler = new JwtSecurityTokenHandler();

			var jwt = handler.WriteToken(token);

			return jwt;
		}

		public AuthenticationTicket Unprotect(string protectedText)
		{
			throw new NotImplementedException();
		}
	}
}
