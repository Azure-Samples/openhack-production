using LinkyLink.Helpers;
using LinkyLink.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkyLink.Service
{
    /// <summary>
    /// This class uses Entity Framework Core to perform CRUD operations (Create, Read, Update, Delete) on the database.
    /// </summary>
    public class LinksService : ILinksService
    {
        private readonly LinksContext _context;
        
        private readonly UserAuth _userAuth;

        public LinksService(LinksContext linksContext, UserAuth userAuth)
        {
            _context = linksContext;
            _userAuth = userAuth;
        }

        public async Task<bool> LinkBundleExists(string id)
        {
            return await _context.LinkBundle.AnyAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<LinkBundle>> AllLinkBundles()
        {
            return await _context.LinkBundle.ToListAsync();
        }

        public async Task<LinkBundle> FindLinkBundle(string vanityUrl)
        {
            return await _context.LinkBundle
               .FirstOrDefaultAsync(b => b.VanityUrl == vanityUrl.ToLower());
        }

        public async Task<IEnumerable<LinkBundle>> FindLinkBundlesForUser(string userId)
        {
            return await _context.LinkBundle
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task CreateLinkBundle(LinkBundle linkBundle)
        {
            _context.LinkBundle.Add(linkBundle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLinkBundle(LinkBundle linkBundle)
        {
            _context.Entry(linkBundle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLinkBundle(LinkBundle linkBundle)
        {
            _context.LinkBundle.Remove(linkBundle);
            await _context.SaveChangesAsync();
        }

        public string GetUserAccountHash()
        {
            return _userAuth.GetUserAccountInfo().HashedID;
        }
    }
}
