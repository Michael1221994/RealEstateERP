using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.API.Providers
{
    public class TokenProviderOptions
    {
        /// <summary>
        /// The relative request path to listen on.
        /// </summary>
        /// <remarks>The default path is <c>/token</c>.</remarks>
        public string Token { get; set; } = "/api/token";
        public string Validate { get; set; } = "/api/validateToken";
        public string RefreshToken { get; set; } = "/api/refreshToken";
        public string RevokeToken { get; set; } = "/api/revokeToken";
        public string Path { get; set; } = "/api/token";
        /// <summary>
        ///  The Issuer (iss) claim for generated tokens.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// The Audience (aud) claim for the generated tokens.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// The expiration time for the generated tokens.
        /// </summary>
        /// <remarks>The default is twelve hours.</remarks>
        public TimeSpan Expiration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Secret Key
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// SymmetricSecurityKey based on SecretKey
        /// </summary>
        public SymmetricSecurityKey SymmetricSecurityKey { get { return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)); } }

        /// <summary>
        /// The signing key to use when generating tokens.
        /// </summary>
        public SigningCredentials SigningCredentials => new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        /// <summary>
        /// Generates a random value (nonce) for each generated token.
        /// </summary>
        /// <remarks>The default nonce is a random GUID.</remarks>
        public Func<Task<string>> NonceGenerator { get; set; }
            = () => Task.FromResult(Guid.NewGuid().ToString());
        /// <summary>
        /// Set the timespan the token will be valid for (default is 120 min)
        /// </summary>
        public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(120);
        /// <summary>
        /// Set the ticks the token will be valid for 
        /// </summary>
        public long Ticks { get; set; } = 120;
    }

}
