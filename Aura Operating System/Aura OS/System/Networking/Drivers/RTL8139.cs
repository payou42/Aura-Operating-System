using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Networking.Drivers
{
    unsafe class RTL8139
    {
        struct tx_desc
        {
            uint phys_addr;
            uint packet_size;
        };

        struct rtl8139_dev
        {
            public byte bar_type;
            public ushort io_base;
            public uint mem_base;
            public int eeprom_exist;
            public byte[] mac_addr;
            public char* rx_buffer;
            public int tx_cur;
        };

        enum RTL8139_registers
        {
            MAG0 = 0x00,       // Ethernet hardware address
            MAR0 = 0x08,       // Multicast filter
            TxStatus0 = 0x10,       // Transmit status (Four 32bit registers)
            TxAddr0 = 0x20,       // Tx descriptors (also four 32bit)
            RxBuf = 0x30,
            RxEarlyCnt = 0x34,
            RxEarlyStatus = 0x36,
            ChipCmd = 0x37,
            RxBufPtr = 0x38,
            RxBufAddr = 0x3A,
            IntrMask = 0x3C,
            IntrStatus = 0x3E,
            TxConfig = 0x40,
            RxConfig = 0x44,
            Timer = 0x48,        // A general-purpose counter
            RxMissed = 0x4C,        // 24 bits valid, write clears
            Cfg9346 = 0x50,
            Config0 = 0x51,
            Config1 = 0x52,
            FlashReg = 0x54,
            GPPinData = 0x58,
            GPPinDir = 0x59,
            MII_SMI = 0x5A,
            HltClk = 0x5B,
            MultiIntr = 0x5C,
            TxSummary = 0x60,
            MII_BMCR = 0x62,
            MII_BMSR = 0x64,
            NWayAdvert = 0x66,
            NWayLPAR = 0x68,
            NWayExpansion = 0x6A,

            // Undocumented registers, but required for proper operation
            FIFOTMS = 0x70,        // FIFO Control and test
            CSCR = 0x74,        // Chip Status and Configuration Register
            PARA78 = 0x78,
            PARA7c = 0x7c,        // Magic transceiver parameter register
        };

        public const int RX_BUF_SIZE = 8192;
        public const int CAPR = 0x38;

        public const int RX_READ_POINTER_MASK = (~3);
        public const int ROK = (1 << 0);
        public const int RER = (1 << 1);
        public const int TOK = (1 << 2);
        public const int TER = (1 << 3);
        public const int TX_TOK = (1 << 15);

        PCIDevice pci_rtl8139_device;
        rtl8139_dev rtl8139_device;

        uint current_packet_ptr;

        // Four TXAD register, you must use a different one to send packet each time(for example, use the first one, second... fourth and back to the first)
        byte[] TSAD_array = { 0x20, 0x24, 0x28, 0x2C };
        byte[] TSD_array = { 0x10, 0x14, 0x18, 0x1C };

        void rtl8139_init()
        {
            // First get the network device using PCI
            pci_rtl8139_device = PCI.GetDevice((VendorID)0x10EC, (DeviceID)0x8139);
            uint ret = pci_rtl8139_device.BaseAddressBar[0].BaseAddress();
            rtl8139_device.bar_type = (byte)(ret & 0x1);
            // Get io base or mem base by extracting the high 28/30 bits
            rtl8139_device.io_base = (byte)(ret & (~0x3));
            rtl8139_device.mem_base = (byte)(ret & (~0xf));
            Console.WriteLine("rtl8139 use %s access (base: %x)\n", (rtl8139_device.bar_type == 0) ? "mem based" : "port based", (rtl8139_device.bar_type != 0) ? rtl8139_device.io_base : rtl8139_device.mem_base);

            // Set current TSAD
            rtl8139_device.tx_cur = 0;

            // Enable PCI Bus Mastering
            //uint pci_command_reg = pci_read(pci_rtl8139_device, PCI_COMMAND);
           // if (!(pci_command_reg & (1 << 2)))
            //{
            //    pci_command_reg |= (1 << 2);
             //   pci_write(pci_rtl8139_device, PCI_COMMAND, pci_command_reg);
            //}

            // Send 0x00 to the CONFIG_1 register (0x52) to set the LWAKE + LWPTN to active high. this should essentially *power on* the device.
            CDDI.outb((ushort)(rtl8139_device.io_base + 0x52), 0x0);

            // Soft reset
            CDDI.outb((ushort)(rtl8139_device.io_base + 0x37), 0x10);
            while ((CDDI.inb((ushort)(rtl8139_device.io_base + 0x37)) & 0x10) != 0)
            {
                // Do nothibg here...
            }

            // Allocate receive buffer
            rtl8139_device.rx_buffer = (char*)Cosmos.Core.Memory.Old.Heap.MemAlloc(8192 + 16 + 1500);
            Core.Memory.Memset((byte*)rtl8139_device.rx_buffer, 0x0, 8192 + 16 + 1500);
            //outportl(rtl8139_device.io_base + 0x30, (uint)virtual2phys(kpage_dir, rtl8139_device.rx_buffer));

            // Sets the TOK and ROK bits high
            //outports(rtl8139_device.io_base + 0x3C, 0x0005);

            // (1 << 7) is the WRAP bit, 0xf is AB+AM+APM+AAP
            //outportl(rtl8139_device.io_base + 0x44, 0xf | (1 << 7));

            // Sets the RE and TE bits high
            CDDI.outb((ushort)(rtl8139_device.io_base + 0x37), 0x0C);

            // Register and enable network interrupts
            //Cosmos.Core.INTs.SetIrqHandler(pci_rtl8139_device.InterruptLine, rtl8139_handler);
            Console.WriteLine("Registered irq interrupt for rtl8139, irq num = %d\n", pci_rtl8139_device.InterruptLine);

            //read_mac_addr();
        }
    }
}
