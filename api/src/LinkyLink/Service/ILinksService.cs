using LinkyLink.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkyLink.Service
{
    public interface ILinksService
    {
        Task<bool> LinkBundleExists(string id);
        Task<IEnumerable<LinkBundle>> AllLinkBundles();
        Task<LinkBundle> FindLinkBundle(string vanityUrl);
        Task<IEnumerable<LinkBundle>> FindLinkBundlesForUser(string userId);
        Task CreateLinkBundle(LinkBundle linkBundle);
        Task UpdateLinkBundle(LinkBundle linkBundle);
        Task RemoveLinkBundle(LinkBundle linkBundle);
        string GetUserAccountHash();

    }
}
