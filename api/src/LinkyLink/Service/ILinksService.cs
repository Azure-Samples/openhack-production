using LinkyLink.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkyLink.Service
{
    public interface ILinksService
    {
        Task<bool> LinkBundleExistsAsync(string id);

        Task<IEnumerable<LinkBundle>> AllLinkBundlesAsync();

        Task<LinkBundle> FindLinkBundleAsync(string vanityUrl);

        Task<IEnumerable<LinkBundle>> FindLinkBundlesForUserAsync(string userId);

        Task CreateLinkBundleAsync(LinkBundle linkBundle);

        Task UpdateLinkBundleAsync(LinkBundle linkBundle);

        Task RemoveLinkBundleAsync(LinkBundle linkBundle);

        string GetUserAccountEmail();
    }
}
