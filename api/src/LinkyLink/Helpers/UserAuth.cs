using LinkyLink.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkyLink.Helpers
{
    public class UserAuth
    {
        protected IHttpContextAccessor _contextAccessor;
        protected Hasher _hasher;

        public UserAuth(IHttpContextAccessor contextAccessor, Hasher hasher)
        {
            _contextAccessor = contextAccessor;
            _hasher = hasher;
        }

        public UserInfo GetUserAccountInfo()
        {
            var socialIdentity = _contextAccessor.HttpContext.User.Identities.FirstOrDefault();

            if (socialIdentity.Claims.Count() != 0)
            {
                var provider = _contextAccessor.HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].FirstOrDefault();
                var email = _contextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (provider != null && email != null)
                {
                    return new UserInfo(provider, _hasher.HashString(email));
                }
            }
            return UserInfo.Empty;
        }
    }
}
