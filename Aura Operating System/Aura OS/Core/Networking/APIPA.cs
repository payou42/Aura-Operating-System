using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.System.Network.IPv4;

namespace Aura_OS.Core.Networking
{
    class APIPA
    {
        public static void SetIP()
        {
            Random rnd = new Random();

            int arg3 = rnd.Next(0, 256);
            int arg4 = rnd.Next(0, 255);
            byte third = (byte)arg3;
            byte four = (byte)arg4;

            Address address = new Address(169, 254, third, four);
            Address subnet = new Address(255, 255, 0, 0);
            Address zero = new Address(0, 0, 0, 0);

            Netconfig.Set(AMDNetwork.AMDNetworkDevice() , address, subnet, zero);
        }

        public static void LookingForDHCP()
        {
            throw new NotImplementedException();
        }
    }
}
