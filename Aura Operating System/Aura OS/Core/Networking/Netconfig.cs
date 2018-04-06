using System;
using Cosmos.System.Network;
using Cosmos.System.Network.IPv4;

namespace Aura_OS.Core.Networking
{
    class Netconfig
    {
        private static string _ip;
        private static string _subnet;
        private static string _gw;

        public static void Set(Cosmos.HAL.NetworkDevice nic, Address ip, Address subnet, Address gateway)
        {
            Config config = new Config(ip, subnet, gateway);

            _ip = ip.ToString();
            _subnet = subnet.ToString();
            _gw = gateway.ToString();

            NetworkStack.ConfigIP(nic, config);
            NetworkStack.Init();
        }

        public static string IP()
        {
            return _ip;
        }

        public static string Subnet()
        {
            return _subnet;
        }

        public static string Gateway()
        {
            return _gw;
        }

        public static void Test()
        {
            Address address = new Address(192, 168, 1, 80);
            Address subnet = new Address(255, 255, 255, 0);
            Address zero = new Address(192, 168, 1, 254);

            Config config = new Config(address, subnet, zero);

            _ip = address.ToString();
            _subnet = subnet.ToString();
            _gw = zero.ToString();

            NetworkStack.ConfigIP(AMDNetwork.AMDNetworkDevice(), config);
            NetworkStack.Init();
        }

        public static void ARPTest()
        {
            AMDNetwork.AMDNetworkDevice().DataReceived = HandlePacket;
        }

        internal static void HandlePacket(byte[] packetData)
        {
            Console.Write("Received Packet Length=");
            if (packetData == null)
            {
                Console.WriteLine("**NULL**");
                return;
            }
            Console.WriteLine(packetData.Length);
            //Sys.Console.WriteLine(BitConverter.ToString(packetData));

            UInt16 etherType = (UInt16)((packetData[12] << 8) | packetData[13]);
            switch (etherType)
            {
                case 0x0806:
                    ARPHandler(packetData);
                    break;
                case 0x0800:
                    ARPHandler(packetData);
                    break;
            }
        }

        internal static void ARPHandler(byte[] packetData)
        {
            Console.WriteLine("Received ARP Packet");
        }

        //public static int CIDR()
        //{
        //    char spliter = '.';
        //    string[] subnet = _subnet.Split(spliter);

        //    int first = int.Parse(subnet[0]); //255
        //    int second = int.Parse(subnet[1]); //255
        //    int third = int.Parse(subnet[2]); //0
        //    int fourth = int.Parse(subnet[3]); //0

        //    //conversion to binary
        //    string first_binary = Convert.ToString(first, 2);
        //    string second_binary = Convert.ToString(second, 2);
        //    string third_binary = Convert.ToString(third, 2);
        //    string fourth_binary = Convert.ToString(fourth, 2);

        //    //ex: 11111111111111110000000000000000
        //    string compilation = first_binary + second_binary + third_binary + fourth_binary;

        //    int CIDR = 0;

        //    //on compte le nombre de 1
        //    foreach (char one in compilation)
        //    {
        //        if(one == '1')
        //        {
        //            CIDR += 1;
        //        }
        //    }

        //    return CIDR;
        //}

    }
}
