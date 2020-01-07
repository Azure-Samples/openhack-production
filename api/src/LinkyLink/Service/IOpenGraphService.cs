using LinkyLink.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkyLink.Service
{
    public interface IOpenGraphService
    {
        Task<OpenGraphResult> GetGraphResult(HttpRequest req, dynamic singleLinkItem);

        Task<IEnumerable<OpenGraphResult>> GetMultipleGraphResults(HttpRequest req, dynamic multiLinkItem);
    }
}
