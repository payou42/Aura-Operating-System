using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Networking.Protocols
{
    class ARP
    {
        List<byte> RequestList = new List<byte>();

        private void MakingRequest(byte[] MACsrcAdress, bool ethernet = true)
        {
            RequestList.Add(0xff); //Broadcasting
            RequestList.Add(0xff); //Broadcasting
            RequestList.Add(0xff); //Broadcasting
            RequestList.Add(0xff); //Broadcasting
            RequestList.Add(0xff); //Broadcasting
            RequestList.Add(0xff); //Broadcasting
            for (int i = 0; i < 6; i++)
            {
                RequestList.Add(MACsrcAdress[i]); //MAC Address Source
            }
            RequestList.Add(0x08); //ARP Protocol ID
            RequestList.Add(0x06); //ARP Protocol ID
            if (ethernet)
            {
                RequestList.Add(0x00); //ARP Request over ethernet
                RequestList.Add(0x01); //ARP Request over ethernet 
            }


            for (int i = 0; i < 6; i++)
            {
                RequestList.Add(MACsrcAdress[i]); //MAC Address from the computer who is asking
            }
        }
    }
}
