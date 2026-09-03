using Infrastracture.Base.BaseEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastracture.Base.API.AuthWrapper
{
    [Authorize]
    public class InAppIdentityAuthorizedController : ControllerBase
    {
        private PersonalInformationBase? userRequester { get; set; }
        private IHttpContextAccessor? HttpContextAccessor { get; set; }

        public InAppIdentityAuthorizedController()
        {
        }

        public InAppIdentityAuthorizedController(IHttpContextAccessor httpContextAccessor)
        {
            this.HttpContextAccessor = httpContextAccessor;
        }

        public PersonalInformationBase User
        {
            get
            {
                try
                {
                    if (this.userRequester == null)
                    {
                        var user = this.HttpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "urn:user");

                        if (user != null) // Build from know token name
                        {
                            userRequester = JsonSerializer.Deserialize<PersonalInformationBase>(user.Value);
                        }
                        else
                        {
                            userRequester = new PersonalInformationBase();
                            userRequester.ID = this.HttpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "uuid")?.Value ?? string.Empty;
                            userRequester.FirstName = this.HttpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "given_name")?.Value ?? string.Empty;
                            userRequester.MiddleName = this.HttpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "middle_name")?.Value ?? string.Empty;
                            userRequester.LastName = this.HttpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "family_name")?.Value ?? string.Empty;
                            userRequester.Email = this.HttpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email")?.Value ?? string.Empty;

                            var courtIDClaim = this.HttpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "courtID")?.Value;
                            userRequester.CourtID = courtIDClaim != null ? new Guid(courtIDClaim) : Guid.Empty;

                            var benchIDClaim = this.HttpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "benchID")?.Value;
                            userRequester.BenchID = benchIDClaim != null ? new Guid(benchIDClaim) : Guid.Empty;
                        }

                        return userRequester;
                    }
                    else return this.userRequester;
                }
                catch (Exception ex)
                {
                    throw new UnauthorizedAccessException(HttpStatusCode.Unauthorized.ToString());
                }
            }
        }
    }
}
