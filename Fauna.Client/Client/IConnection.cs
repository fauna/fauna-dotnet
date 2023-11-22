using System.Net.Http;
using System.Threading.Tasks;

namespace Fauna.Client.Client
{
    public interface IConnection
    {
        Task<HttpResponseMessage> PerformRequestAsync(string fql);
    }
}
