# IPFS submodule for Arteranos

Derived from Unity-IPFS-Engine it's reduced to an interface connecting the Arteranos core to an external IPFS daemon. Some common tasks (like hashes, CID or peer handling, or cryptography) still can be handled locally, but IPFS#s workload will be offloaded to the daemon via API RPC calls.

The interface strives to conform to the IPFS API specifications, but implementation specific differences can (or will) occur.

Refer to [IPFS implementations](https://docs.ipfs.tech/concepts/ipfs-implementations/#popular-or-actively-maintained)
