using Infrastracture.Base.API.Providers;
using Infrastracture.Base.BaseEntities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Infrastracture.Base.API
{
    public static class IdentityStartup
    {
        public static IServiceCollection ConfigureAuth(this IServiceCollection services, IConfiguration config)
        {
         

            //Token
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                var kc = config.GetSection("JwtOptions").Get<IEnumerable<JwtSettings>>().First(x => x.Name == "kc");

                options.TokenValidationParameters = GetTokenValidationParameters(config);
                options.Authority = kc.Issuer;
                options.Audience = kc.AudienceId;

                options.RequireHttpsMetadata = true;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        System.Diagnostics.Debug.WriteLine("Security validated");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        System.Diagnostics.Debug.WriteLine(context.Response.StatusCode);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var claims = context.Principal.Identities.First().Claims.ToArray();
                        var newIdentity = new ClaimsIdentity(claims, context.Scheme.Name);
                        newIdentity.AddClaim(new Claim("newCustomClaim", "newValue"));
                        context.Principal = new ClaimsPrincipal(newIdentity);
                        System.Diagnostics.Debug.WriteLine("Security validated");
                        return Task.CompletedTask;
                        // Add custom claim logic here
                    }
                };
            });

            return services;
        }

        private static IEnumerable<TokenProviderOptions> GetTokenProviderOptions(IConfiguration config)
        {
            // Fetch the JwtOptions section from the configuration
            var jwtOptionsSection = config.GetSection("JwtOptions").Get<IEnumerable<JwtSettings>>();

            var jwtOptions = new List<TokenProviderOptions>();

            foreach (var jwtOption in jwtOptionsSection)
            {
                jwtOptions.Add(new TokenProviderOptions
                {
                    Audience = jwtOption.AudienceId,
                    Issuer = jwtOption.Issuer,
                    SecretKey = jwtOption.SecretKey,
                });
            }

            return jwtOptions;
        }

        private static TokenValidationParameters GetTokenValidationParameters(IConfiguration config)
        {

            var jwtOptions = config.GetSection("JwtOptions").Get<IEnumerable<JwtSettings>>();
            //var jwtOptions = GetTokenProviderOptions(config);

            var appKey = jwtOptions.First(x => x.Name == "iccms-identity").AudienceId;
            var secretKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.First(x => x.Name == "iccms-identity").SecretKey));
            var appIssuer = jwtOptions.First(x => x.Name == "iccms-identity").Issuer;


            var kc_Key = jwtOptions.First(x => x.Name == "kc").AudienceId;
            var kc_issuer = jwtOptions.First(x => x.Name == "kc").Issuer;
            var kc_secretKey = new RsaSecurityKey(new System.Security.Cryptography.RSAParameters
            {
                Modulus = Convert.FromBase64String(jwtOptions.First(x => x.Name == "kc").SecretKey),
                Exponent = Convert.FromBase64String("AQAB") // Default exponent for RSA keys
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
              

                // The signing key must match!
                ValidateIssuerSigningKey = true,


                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuers = new[] { appIssuer, kc_issuer},

                //// Validate the JWT Audience (aud) claim
                ValidateAudience = true,

                // Validate the token expiry
                ValidateLifetime = true,

                // If you want to allow a certain amount of clock drift, set that here:
                //ClockSkew = System.TimeSpan.Zero,

                ValidAudiences = new[] { appKey, kc_Key },
                IssuerSigningKeys = new List<SecurityKey>() {
                      secretKey,kc_secretKey
                    }
            };

            return tokenValidationParameters;
        }

        private static void ConfigurePolicies(IServiceCollection services)
        {
            //CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                    });
            });

            //Authorization Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireElevatedRights", policy => policy.RequireRole("Super Admin"));
                options.AddPolicy("RequireAdminRights", policy => policy.RequireRole("Super Admin", "Administrator"));
            });
        }


    }

}
