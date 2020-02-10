using LinkyLink.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LinkyLink.Helpers
{
    /// <summary>
    /// This class provides the method to read the identity of the authenticated user and extracts identity provider and email address.
    /// </summary>

    public class UserAuth
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserAuth(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public UserInfo GetUserAccountInfo()
        {
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var identities = _contextAccessor.HttpContext.User.Identities;

                if (identities.Any())
                {
                    var primaryIdentity = identities.First();
                    var email = primaryIdentity.Claims.SingleOrDefault(c => c.Type == "emails").Value;
                    var userInfo = new UserInfo("ADB2C", email);

                    return userInfo;
                }
            }

            return UserInfo.Empty;
        }
    }
}
