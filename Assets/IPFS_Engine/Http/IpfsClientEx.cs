using Ipfs.CoreApi;
using Ipfs.ExtendedApi;
using System.Linq.Expressions;

namespace Ipfs.Http
{
    public class IpfsClientEx : IpfsClient
    {
        public IpfsClientEx() : base() 
        { 
            InitEx();
        }

        public IpfsClientEx(string host) : base(host) 
        { 
            InitEx();
        }

        private void InitEx() 
        {
            Routing = new RoutingApi(this);
        }

        public IRoutingApi Routing { get; private set; }

    }
}