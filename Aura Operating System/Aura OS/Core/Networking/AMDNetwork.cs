/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Get MAC Address
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*/

using Cosmos.HAL;
using Cosmos.HAL.Drivers.PCI.Network;

namespace Aura_OS.Core.Networking
{
    class AMDNetwork
    {
        private static PCIDevice device = PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);

        /// <summary>
        /// Get MAC Address String
        /// </summary>
        public static string PhysicalAddress()
        {
            if (NetworkCardAvailable())
            {
                AMDPCNetII nic = new AMDPCNetII(device);
                return nic.MACAddress.ToString();
            }
            else
            {
                return "";
            }
        }

        public static bool NetworkCardAvailable()
        {
            PCIDevice device;
            device = PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
            if (device != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SetConfigIP()
        {

        }
    }
}
