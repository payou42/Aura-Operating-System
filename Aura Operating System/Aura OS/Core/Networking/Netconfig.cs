using System;
using Cosmos.System.Network;
using Cosmos.System.Network.IPv4;

namespace Aura_OS.Core.Networking
{
    class Netconfig
    {
        public static void Set(Cosmos.HAL.NetworkDevice nic, Address ip, Address subnet, Address gateway)
        {
            Config config = new Config(ip, subnet, gateway);
            NetworkStack.ConfigIP(nic, config);
        }

    }
}
