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
                var socialIdentities = _contextAccessor.HttpContext.User.Identities
                    .Where(id => !id.AuthenticationType.Equals("WebJobsAuthLevel", StringComparison.InvariantCultureIgnoreCase));

                if (socialIdentities.Any())
                {
                    var provider = _contextAccessor.HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].DefaultIfEmpty(string.Empty).FirstOrDefault();
                    var primaryIdentity = socialIdentities.First();
                    var email = primaryIdentity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email).Value;
                    var userInfo = new UserInfo(provider, HashString(email));

                    return userInfo;
                }
            }

            return UserInfo.Empty;
        }

        private string HashString(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email was null or empty", "email");
            }

            string salt = Environment.GetEnvironmentVariable("AUTH_SALT");
            if (string.IsNullOrWhiteSpace(salt)) { salt = "HASHER_SALT"; }
            byte[] keyByte = System.Text.Encoding.UTF8.GetBytes(salt);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(email);
            using (var hmacsha256 = new HMACSHA384(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
