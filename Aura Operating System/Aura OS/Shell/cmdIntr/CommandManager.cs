/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*/

using Cosmos.HAL.PCInformation;
using System;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network;
using System;
using IPv4 = Cosmos.System.Network.IPv4;
using System.Text;
using Cosmos.HAL.Drivers.PCI.Network;
using Cosmos.System.Network.IPv4;

namespace Aura_OS.Shell.cmdIntr
{
    unsafe class CommandManager
    {
        //TO-DO: Do for all commands:
        //       Windows like command, Linux like command, Aura original command (optional for the last one)
        //Example: else if ((cmd.Equals("ipconfig")) || (cmd.Equals("ifconfig")) || (cmd.Equals("netconf"))) {

        /// <summary>
        /// Empty constructor. (Good for debug)
        /// </summary>
        public CommandManager() { }
        /// <summary>
        /// Shell Interpreter
        /// </summary>
        /// <param name="cmd">Command</param>
        public static void _CommandManger(string cmd)
        {

            #region Power

            if (cmd.Equals("shutdown"))
            {//NOTE: Why isn't it just the constructor? This leaves more room for <package>.<class>.HelpInfo;
                Power.Shutdown.c_Shutdown();
            }
            else if (cmd.Equals("reboot"))
            {
                Power.Reboot.c_Reboot();
            }

            #endregion Power

            #region Console

            else if ((cmd.Equals("clear")) || (cmd.Equals("cls")))
            {
                c_Console.Clear.c_Clear();
            }
            else if (cmd.StartsWith("echo "))
            {
                c_Console.Echo.c_Echo(cmd);
            }
            else if (cmd.Equals("help"))
            {
                System.Translation.List_Translation._Help();
            }

            #endregion Console

            #region FileSystem

            else if (cmd.StartsWith("cd "))
            {
                FileSystem.CD.c_CD(cmd);
            }
            else if (cmd.Equals("cp"))
            {
                FileSystem.CP.c_CP_only();
            }
            else if (cmd.StartsWith("cp "))
            {
                FileSystem.CP.c_CP(cmd);
            }
            else if ((cmd.Equals("dir")) || (cmd.Equals("ls")))
            {
                FileSystem.Dir.c_Dir();
            }
            else if ((cmd.StartsWith("dir ")) || (cmd.StartsWith("ls ")))
            {
                FileSystem.Dir.c_Dir(cmd);
            }
            else if (cmd.Equals("mkdir"))
            {
                FileSystem.Mkdir.c_Mkdir();
            }
            else if (cmd.StartsWith("mkdir "))
            {
                FileSystem.Mkdir.c_Mkdir(cmd);
            }
            else if (cmd.StartsWith("rmdir "))
            {
                FileSystem.Rmdir.c_Rmdir(cmd);
            }//TODO: orgainize
            else if (cmd.StartsWith("rmfil "))
            {
                FileSystem.Rmfil.c_Rmfil(cmd);
            }
            else if (cmd.Equals("mkfil"))
            {
                FileSystem.Mkfil.c_mkfil();
            }
            else if (cmd.StartsWith("mkfil "))
            {
                FileSystem.Mkfil.c_mkfil(cmd);
            }
            else if (cmd.StartsWith("edit "))
            {
                FileSystem.Edit.c_Edit(cmd);
            }
            else if (cmd.Equals("vol"))
            {
                FileSystem.Vol.c_Vol();
            }
            else if (cmd.StartsWith("run "))
            {
                FileSystem.Run.c_Run(cmd);
            }

            #endregion FileSystem

            #region Settings

            else if (cmd.Equals("logout"))
            {
                Settings.Logout.c_Logout();
            }
            else if (cmd.Equals("settings"))
            {
                Settings.Settings.c_Settings();
            }
            else if (cmd.StartsWith("settings "))
            {
                Settings.Settings.c_Settings(cmd);
            }
            else if (cmd.StartsWith("passwd "))
            {
                Settings.Passwd.c_Passwd(cmd);
            }
            else if (cmd.Equals("passwd"))
            {
                Settings.Passwd.c_Passwd(Kernel.userLogged);
            }

            #endregion Settings

            #region System Infomation

            else if (cmd.Equals("systeminfo"))
            {
                SystemInfomation.SystemInfomation.c_SystemInfomation();
            }
            else if ((cmd.Equals("ver")) || (cmd.Equals("version")))
            {
                SystemInfomation.Version.c_Version();
            }
            else if ((cmd.Equals("ipconfig")) || (cmd.Equals("ifconfig")) || (cmd.Equals("netconf")))
            {
                SystemInfomation.IPConfig.c_IPConfig();
            }
            else if ((cmd.Equals("time")) || (cmd.Equals("date")))
            {
                SystemInfomation.Time.c_Time();
            }

            #endregion System Infomation

            #region Tests

            else if (cmd.Equals("crash"))
            {
                Tests.Crash.c_Crash();
            }

            else if (cmd.Equals("crashcpu"))
            {
                int value = 1;
                value = value - 1;
                int result = 1 / value; //Division by 0
            }

            else if (cmd.Equals("beep"))
            {
                Kernel.speaker.beep();
            }

            //else if (cmd.StartsWith("xml "))
            //{
            //    Util.xml.CmdXmlParser.c_CmdXmlParser(cmd, 0, 4);
            //}
          
            else if (cmd.Equals("net"))
            {

                Console.WriteLine("Finding network devices...");

                PCIDeviceNormal xNicDev = (PCIDeviceNormal)PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
                if (xNicDev == null)
                {
                    Console.WriteLine("PCIDevice not found!!");
                    Console.ReadKey();
                    return;
                }

                //PCIDeviceNormal xNicDevNormal = xNicDev;
                //if (xNicDevNormal == null)
                //{
                //    Console.WriteLine("Unable to cast as PCIDeviceNormal!");
                //    Console.ReadKey();
                //    return;
                //}
                Console.WriteLine("Found AMD PCNetII NIC on PCI " + xNicDev.bus + ":" + xNicDev.slot + ":" + xNicDev.function);
                Console.WriteLine("NIC IRQ: " + xNicDev.InterruptLine);
                
                var xNic = new AMDPCNetII(xNicDev);
                Console.WriteLine("NIC MAC Address: " + xNic.MACAddress.ToString());
                NetworkStack.Init();
                xNic.Enable();

                //Console.WriteLine("Finding PCI device");
                //PCIDeviceNormal xNicDev = (PCIDeviceNormal)PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);
                //if (xNicDev == null)
                //{
                //    Console.WriteLine("  Not found!!");
                //    return;
                //}

                //var xNicDevNormal = xNicDev as PCIDeviceNormal;
                //if (xNicDevNormal == null)
                //{
                //   Console.WriteLine("Unable to cast as PCIDeviceNormal!");
                //    return;
                //}
                // var xNic = new Cosmos.HAL.Drivers.PCI.AMDPCNetII(xNicDev);
                //NetworkStack.Init();
                //xNic.Enable();

                //foreach (NetworkDevice device in NetworkDevice.Devices)
                //{
                //    Console.WriteLine("Device: ");
                //    Console.WriteLine(device.MACAddress);
                //}
                //etworkStack.ConfigIP(xNic, new Config(new Address(192, 168, 1, 70), new Address(255, 255, 255, 0)));

                //var xClient = new UdpClient(4242);
                //xClient.Connect(new Address(192, 168, 1, 12), 4242);
                //xClient.Send(new byte[]
                //             {
                //             1,
                //             2,
                //            3,
                //            4,
                //            5,
                //            6,
                //            7,
                //            8,
                ///            9,
                //            0xAA,
                //            0xBB,
                //           0xCC,
                //            0xDD,
                //             0xEE,
                //           0xFF
                //           });
                //
                // while (true)
                /// {
                //     NetworkStack.Update();

                //    Console.WriteLine("Done");
                //    Console.ReadLine();
                //}



                //PCIDevice device = PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII);

                //if (device != null)
                //{
                //
                //  Cosmos.HAL.Drivers.PCI.Network.AMDPCNetII nic = new Cosmos.HAL.Drivers.PCI.Network.AMDPCNetII(PCI.GetDevice(VendorID.AMD, DeviceID.PCNETII));
                //Cosmos.HAL.Drivers.PCI.Network.AMDPCNetII.EnableDevice();



                //Cosmos.HAL.Drivers.PCI.Network.AMDPCNetII.HandleNetworkInterrupt();
                //Console.WriteLine("AMD_AM79C973 Interrupt Line: " + device.InterruptLine);

                //Console.WriteLine("IRQ Handler set!");

                //System.Networking.Drivers.AMD_AM79C973.amd_am79c973_init(device);
                //Console.WriteLine("AMD_AM79C973 Initialized!");

                //System.Networking.Drivers.AMD_AM79C973.amd_am79c973_analyse_status();

                //System.Networking.Drivers.AMD_AM79C973.amd_am79c973_activate();
                //Console.WriteLine("AMD_AM79C973 Activated!");

                //while (true)
                //{


                //Console.WriteLine("Status first:");
                //System.Networking.Drivers.AMD_AM79C973.amd_am79c973_analyse_status();


                //Console.WriteLine("Send:");

                //string test = "Hello network";

                //fixed (char* p = test)
                //{
                //    System.Networking.Drivers.AMD_AM79C973.amd_am79c973_send((uint*)p, 13);

                //}

                //while (true)
                //{
                //    System.Networking.Drivers.AMD_AM79C973.amd_am79c973_analyse_status();
                //}

                //Console.ReadKey();
                //  Console.WriteLine("Status second:");
                //System.Networking.Drivers.AMD_AM79C973.amd_am79c973_analyse_status();
                //Console.ReadKey();
                //}

                //Console.WriteLine("Int handler activated!");
                //}



                //Cosmos.HAL.Drivers.PCI.Network.AMDPCNetII nic = new Cosmos.HAL.Drivers.PCI.Network.AMDPCNetII(PCI.GetDevice(0x1022, 0x2000));

                //Console.WriteLine(nic.MACAddress);

                //IPv4.Address myIP = new IPv4.Address(192, 168, 169, 1);
                //IPv4.Address mySubnet = new IPv4.Address(255, 255, 255, 0);
                //IPv4.Address myGateway = new IPv4.Address(192, 168, 1, 1);
                //IPv4.Config myConfig = new IPv4.Config(myIP, mySubnet, myGateway);

                //NetworkStack.ConfigIP(nic, myConfig);
                //nic.Enable();

                //while (true)
                //{
                //    NetworkStack.Update();
                //}

                //PCIDevice device;
                //device = PCI.GetDevice(0x1022, 0x2000);

                //Cosmos.HAL.Drivers.PCI.Network.AMDPCNetII nic = new Cosmos.HAL.Drivers.PCI.Network.AMDPCNetII(device);

                //NetworkDevice.Devices.Add(nic);

                //Cosmos.System.Network.NetworkStack.Init();

                //Cosmos.System.Network.IPv4.Address myIP = new Cosmos.System.Network.IPv4.Address(192, 168, 1, 128);
                //Cosmos.System.Network.IPv4.Address mySubnet = new Cosmos.System.Network.IPv4.Address(255, 255, 255, 0);
                //Cosmos.System.Network.IPv4.Address myGateway = new Cosmos.System.Network.IPv4.Address(192, 168, 135, 1);
                //Cosmos.System.Network.IPv4.Config myConfig = new Cosmos.System.Network.IPv4.Config(myIP, mySubnet, myGateway);

                //NetworkStack.ConfigIP(nic, myConfig);
                //nic.Enable();

                //Console.WriteLine("Found AMD PCNetII NIC on PCI " + device.bus + ":" + device.slot + ":" +
                //             device.function);
                // Console.WriteLine("NIC IRQ: " + device.InterruptLine);
                //Console.WriteLine("NIC MAC Address: " + nic.MACAddress.ToString());

                //var xClient = new UdpClient(55341);
                //xClient.Connect(new Address(192, 168, 1, 12), 55341);

                // Cosmos.System.Network.IPv4.EndPoint source = new Cosmos.System.Network.IPv4.EndPoint(new Address(192, 168, 1, 12), 55341);

                // xClient.Receive(ref source);
                //xClient.Send(new byte[]
                //             {
                //                 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x21
                //             });

                // NetworkStack.Update();
                // xClient.Close();
                Console.WriteLine("Done");


            }

            #endregion Tests

            #region Tools

            else if (cmd.Equals("snake"))
            {
                Tools.Snake.c_Snake();
            }
            else if (cmd.StartsWith("md5"))
            {
                Tools.MD5.c_MD5(cmd);
            }

            #endregion

            #region Util           

            else if (cmd.StartsWith("export"))
            {
                Util.EnvVar.c_Export(cmd);
            }

            else if (cmd.Equals("lspci"))
            {
                Util.Lspci.c_Lspci();
            }

            else
            {
                if (cmd.Length <= 0)
                {
                    Console.WriteLine();
                    return;
                }
                else
                { 
                    Util.CmdNotFound.c_CmdNotFound();
                }                
            }

            Console.WriteLine();

            #endregion Util

        }

    }
}