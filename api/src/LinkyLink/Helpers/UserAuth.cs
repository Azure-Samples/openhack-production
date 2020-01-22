using LinkyLink.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LinkyLink.Helpers
{
    /// <summary>
    /// This class handles the user`s identity and hashes the user email address if a user is authenticated by Twitter, Facebook, etc.
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
