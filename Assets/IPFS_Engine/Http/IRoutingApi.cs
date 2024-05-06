using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.ExtendedApi
{
    /// <summary>
    ///   Some miscellaneous methods.
    /// </summary>
    /// <seealso href="https://github.com/ipfs/interface-ipfs-core/blob/master/SPEC/MISCELLANEOUS.md">Generic API spec</seealso>
    public interface IRoutingApi
    {
        /// <summary>
        ///   Information about an IPFS peer.
        /// </summary>
        /// <param name="id">
        ///   The <see cref="MultiHash"/> ID of the IPFS peer.  
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is NOT raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation that returns
        ///   the <see cref="Peer"/> information or a closer peer.
        /// </returns>
        Task<Peer> FindPeerAsync(MultiHash id, CancellationToken cancel = default(CancellationToken));

        /// <summary>
        ///   Find the providers for the specified content.
        /// </summary>
        /// <param name="id">
        ///   The <see cref="Cid"/> of the content.
        /// </param>
        /// <param name="limit">
        ///   The maximum number of peers to return.  Defaults to 20.
        /// </param>
        /// <param name="providerFound">
        ///   An action to perform when a provider is found.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation that returns
        ///   a sequence of IPFS <see cref="Peer"/>.
        /// </returns>
        Task<IEnumerable<Peer>> FindProvidersAsync(
            Cid id,
            int limit = 20,
            Action<Peer>? providerFound = null,
            CancellationToken cancel = default);
    }
}
