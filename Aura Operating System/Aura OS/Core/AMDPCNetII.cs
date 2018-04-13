using System;
using System.Collections.Generic;
using Cosmos.Common;
using Cosmos.Core;
using Cosmos.Core.IOGroup.Network;
using Cosmos.HAL.Network;
using IL2CPU.API.Attribs;

namespace Cosmos.HAL.Drivers.PCI.Network
{

    [Plug(Target = typeof(Cosmos.Core.INTs))]
    public class AMDPCNetII : NetworkDevice
    {
        protected PCIDevice pciCard;
        public static AMDPCNetIIIOGroup io;
        protected MACAddress mac;
        public static bool mInitDone;

        protected ManagedMemoryBlock mInitBlock;
        protected ManagedMemoryBlock mRxDescriptor;
        protected ManagedMemoryBlock mTxDescriptor;
        protected List<ManagedMemoryBlock> mRxBuffers;
        protected List<ManagedMemoryBlock> mTxBuffers;
        protected Queue<byte[]> mRecvBuffer;
        public static  Queue<byte[]> mTransmitBuffer;
        private int mNextTXDesc;

        static ushort register_data_port;
        static ushort register_address_port;

        static ushort bus_control_register_data_port;

        public AMDPCNetII(PCIDevice device)
            : base()
        {

            this.pciCard = device;
            //this.pciCard.Claimed = true;
            //this.pciCard.EnableDevice();

            io = new AMDPCNetIIIOGroup((ushort)this.pciCard.BaseAddressBar[0].BaseAddress);

            //this.io.RegisterData.DWord = 0;
            io.RegisterData.DWord &= 0xffff0000;
            io.RegisterData.DWord |= 0x5;

            register_data_port = (ushort)(device.BaseAddressBar[0].BaseAddress + 0x10);
            register_address_port = (ushort)(device.BaseAddressBar[0].BaseAddress + 0x12);

            bus_control_register_data_port = (ushort)(device.BaseAddressBar[0].BaseAddress + 0x16);

            EnableDevice();

            // Get the EEPROM MAC Address and set it as the devices MAC
            byte[] eeprom_mac = new byte[6];
            UInt32 result = io.MAC1.DWord;
            eeprom_mac[0] = BinaryHelper.GetByteFrom32bit(result, 0);
            eeprom_mac[1] = BinaryHelper.GetByteFrom32bit(result, 8);
            eeprom_mac[2] = BinaryHelper.GetByteFrom32bit(result, 16);
            eeprom_mac[3] = BinaryHelper.GetByteFrom32bit(result, 24);
            result = io.MAC2.DWord;
            eeprom_mac[4] = BinaryHelper.GetByteFrom32bit(result, 0);
            eeprom_mac[5] = BinaryHelper.GetByteFrom32bit(result, 8);

            Aura_OS.System.Networking.CDDI.outw(register_address_port, 20);
            Aura_OS.System.Networking.CDDI.outw(bus_control_register_data_port, 0x102);

            Aura_OS.System.Networking.CDDI.outw(register_address_port, 0);
            Aura_OS.System.Networking.CDDI.outw(register_data_port, 0x04);

            mac = new MACAddress(eeprom_mac);

            mInitBlock = new ManagedMemoryBlock(28, 4);
            mRxDescriptor = new ManagedMemoryBlock(256, 16);
            mTxDescriptor = new ManagedMemoryBlock(256, 16);

            mInitBlock.Write32(0x00, (0x4 << 28) | (0x4 << 20));
            mInitBlock.Write32(0x04,
                (UInt32)(eeprom_mac[0] | (eeprom_mac[1] << 8) | (eeprom_mac[2] << 16) | (eeprom_mac[3] << 24)));
            mInitBlock.Write32(0x08, (UInt32)(eeprom_mac[4] | (eeprom_mac[5] << 8)));
            mInitBlock.Write32(0x0C, 0x0);
            mInitBlock.Write32(0x10, 0x0);
            mInitBlock.Write32(0x14, mRxDescriptor.Offset);
            mInitBlock.Write32(0x18, mTxDescriptor.Offset);

            InitializationBlockAddress = mInitBlock.Offset;
            SoftwareStyleRegister = 0x03;

            mRxBuffers = new List<ManagedMemoryBlock>();
            mTxBuffers = new List<ManagedMemoryBlock>();
            for (uint rxd = 0; rxd < 16; rxd++)
            {
                uint xOffset = rxd * 16;

                ManagedMemoryBlock buffer = new ManagedMemoryBlock(2048);
                mRxDescriptor.Write32(xOffset + 8, buffer.Offset);
                UInt16 buffer_len = (UInt16)(~buffer.Size);
                buffer_len++;
                UInt32 flags = (UInt32)(buffer_len & 0x0FFF) | 0xF000 | 0x80000000;
                mRxDescriptor.Write32(xOffset + 4, flags);
                mRxBuffers.Add(buffer);
            }
            for (uint txd = 0; txd < 16; txd++)
            {
                uint xOffset = txd * 16;

                ManagedMemoryBlock buffer = new ManagedMemoryBlock(2048);
                mTxDescriptor.Write32(xOffset + 8, buffer.Offset);
                mTxBuffers.Add(buffer);
            }

            mNextTXDesc = 0;

            // Setup our Receive and Transmit Queues
            mTransmitBuffer = new Queue<byte[]>();
            mRecvBuffer = new Queue<byte[]>();

            Console.WriteLine("[PCNETII Debug] Interrupt line: " + this.pciCard.InterruptLine);

            INTs.SetIrqHandler(this.pciCard.InterruptLine, HandleNetworkInterrupt); //it's calling HandleNetworkInterrupt() but it doesn't work


        }

        public static void EnableDevice()
        {
            Aura_OS.System.Networking.CDDI.outw(register_address_port, 0);
            Aura_OS.System.Networking.CDDI.outw(register_data_port, 0x41);

            Aura_OS.System.Networking.CDDI.outw(register_address_port, 4);
            ushort status = Aura_OS.System.Networking.CDDI.inw(register_data_port);

            Aura_OS.System.Networking.CDDI.outw(register_address_port, 4);
            Aura_OS.System.Networking.CDDI.outw(register_data_port, (ushort)(status | 0xC00));

            Aura_OS.System.Networking.CDDI.outw(register_address_port, 0);
            Aura_OS.System.Networking.CDDI.outw(register_data_port, 0x42);
        }

        public static void HandleNetworkInterrupt(ref INTs.IRQContext aContext)
        {
            UInt32 cur_status = StatusRegister;

            Console.WriteLine("[PCNETII Debug] AMD PCNet IRQ raised!"); //it's doesn't work because we don't have this fucking message

            Console.WriteLine("[PCNETII Debug] Status: " + cur_status);

            if ((cur_status & 0x100) != 0)
            {
                mInitDone = true;
                Console.WriteLine("Init done!");
            }
            if ((cur_status & 0x200) != 0)
            {
                if (mTransmitBuffer.Count > 0)
                {
                    byte[] data = mTransmitBuffer.Peek();
                    //if (SendBytes(ref data) == true)
                    //{
                    //    mTransmitBuffer.Dequeue();
                    //}
                }
            }
            if ((cur_status & 0x400) != 0)
            {
                //ReadRawData();
            }

            StatusRegister = cur_status;
            Cosmos.Core.Global.PIC.EoiSlave();
        }

        /// <summary>
        /// Retrieve all AMD PCNetII network cards found on computer.
        /// </summary>
        public static void FindAll()
        {
            Console.WriteLine("Scanning for AMD PCNetII cards...");
            //  PCIDevice device = Cosmos.HAL.PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
            //  if (device != null)
            // {
            //      AMDPCNetII nic = new AMDPCNetII((PCIDevice) device);
            //
            //      Console.WriteLine("Found AMD PCNetII NIC on PCI " + device.bus + ":" + device.slot + ":" +
            //                        device.function);
            //      Console.WriteLine("NIC IRQ: " + device.InterruptLine);
            //      Console.WriteLine("NIC MAC Address: " + nic.MACAddress.ToString());
            //  }
        }

        #region Register Access Properties

        public static UInt32 StatusRegister
        {
            get
            {
                io.RegisterAddress.DWord = 0x00;
                return io.RegisterData.DWord;
            }
            set
            {
                io.RegisterAddress.DWord = 0x00;
                io.RegisterData.DWord = value;
            }
        }

        protected UInt32 InitializationBlockAddress
        {
            get
            {
                UInt32 result;

                io.RegisterAddress.DWord = 0x01;
                result = io.RegisterData.DWord;
                io.RegisterAddress.DWord = 0x02;
                result |= (io.RegisterData.DWord << 16);

                return result;
            }
            set
            {
                io.RegisterAddress.DWord = 0x01;
                io.RegisterData.DWord = (value & 0xFFFF);
                io.RegisterAddress.DWord = 0x02;
                io.RegisterData.DWord = (value >> 16);
            }
        }

        protected UInt32 SoftwareStyleRegister
        {
            get
            {
                io.RegisterAddress.DWord = 0x14;
                return io.BusData.DWord;
            }
            set
            {
                io.RegisterAddress.DWord = 0x14;
                io.BusData.DWord = value;
            }
        }

        #endregion

        #region Network Device Implementation

        public override MACAddress MACAddress
        {
            get { return mac; }
        }

        public override bool Ready
        {
            get { return mInitDone; }
        }

        public override bool Enable()
        {
            StatusRegister = 0x43;

            return true;
        }

        [DebugStub(Off = true)]
        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            byte[] data = new byte[length];
            for (int b = 0; b < length; b++)
            {
                data[b] = buffer[b + offset];
            }

            if (SendBytes(ref data) == false)
            {
                mTransmitBuffer.Enqueue(data);
            }

            return true;
        }

        public override bool ReceiveBytes(byte[] buffer, int offset, int max)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReceivePacket()
        {
            if (mRecvBuffer.Count < 1)
            {
                return null;
            }

            byte[] data = mRecvBuffer.Dequeue();
            return data;
        }

        public override int BytesAvailable()
        {
            if (mRecvBuffer.Count < 1)
            {
                return 0;
            }

            return mRecvBuffer.Peek().Length;
        }

        public override bool IsSendBufferFull()
        {
            return false;
        }

        public override bool IsReceiveBufferFull()
        {
            return false;
        }

        #endregion

        #region Helper Functions

        [DebugStub(Off = true)]
        protected bool SendBytes(ref byte[] aData)
        {
            int txd = mNextTXDesc++;
            if (mNextTXDesc >= 16)
            {
                mNextTXDesc = 0;
            }

            uint xOffset = (uint)(txd * 16);
            UInt32 status = mTxDescriptor.Read32(xOffset + 4);
            if ((status & 0x80000000) == 0)
            {
                for (uint b = 0; b < aData.Length; b++)
                {
                    mTxBuffers[txd][b] = aData[b];
                }
                //UInt16 buffer_len = (UInt16)(aData.Length < 64 ? 64 : aData.Length);
                UInt16 buffer_len = (UInt16)aData.Length;
                buffer_len = (UInt16)(~buffer_len);
                buffer_len++;

                UInt32 flags = (UInt32)((buffer_len) & 0x0FFF) | 0x0300F000 | 0x80000000;

                mTxDescriptor.Write32(xOffset + 4, flags);

                return true;
            }

            return false;
        }

        [DebugStub(Off = true)]
        private void ReadRawData()
        {
            uint status;
            UInt16 recv_size;
            byte[] recv_data;

            for (int rxd = 0; rxd < 16; rxd++)
            {
                uint xOffset = (uint)(rxd * 16);
                status = mRxDescriptor.Read32(xOffset + 4);
                if ((status & 0x80000000) == 0)
                {
                    recv_size = (UInt16)(mRxDescriptor[xOffset + 0] & 0xFFF);
                    recv_data = new byte[recv_size];
                    for (uint b = 0; b < recv_size; b++)
                    {
                        recv_data[b] = mRxBuffers[rxd][b];
                    }

                    if (DataReceived != null)
                    {
                        DataReceived(recv_data);
                    }
                    else
                    {
                        mRecvBuffer.Enqueue(recv_data);
                    }

                    mRxDescriptor.Write32(xOffset + 4, status | 0x80000000);
                }
            }
        }

        #endregion
    }
}
