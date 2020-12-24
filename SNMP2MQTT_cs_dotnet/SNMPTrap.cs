using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SNMP2MQTT_cs_dotnet
{
    public class SNMPTrap
    {
		public void Start()
		{
			string SettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Settings.json");
			int Port = 0;
			using (StreamReader FileReader = new StreamReader(SettingsPath))
			{
				string FileContents = FileReader.ReadToEnd();
				Port = int.Parse(Regex.Match(FileContents, "(?<=\"SNMPTrapPort\"\\:\\s)\\d+").Value);			
			}

			var DeviceManagerInstance = new DeviceManager();

			// Construct a socket and bind it to the trap manager port 162
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, Port);
			EndPoint ep = ipep;
			socket.Bind(ep);

			// Disable timeout processing. Just block until packet is received
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
			bool run = true;
			int inlen = -1;
			while (run)
			{
				byte[] indata = new byte[16 * 1024];                // 16KB receive buffer int inlen = 0;
				EndPoint inep = new IPEndPoint(IPAddress.Any, 0);
				try
				{
					inlen = socket.ReceiveFrom(indata, ref inep);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Exception {0}", ex.Message);
					inlen = -1;
				}
				if (inlen > 0)
				{
					// Check protocol version int
					var ver = SnmpPacket.GetProtocolVersion(indata, inlen);
					if (ver == (int)SnmpVersion.Ver1)
					{
						var PayLoad = new SNMPPayload();
						
						// Parse SNMP Version 1 TRAP packet
						SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
						pkt.decode(indata, inlen);

						PayLoad.DeviceID = pkt.Pdu.Enterprise.ToString();
						PayLoad.DeviceIP = pkt.Pdu.AgentAddress.ToString();
						PayLoad.DeviceCommunity = pkt.Community.ToString();
						PayLoad.ChildDevices = new List<ChildDevice>();

						foreach (Vb VariablePair in pkt.Pdu.VbList)
						{							
							var ChildDevice = new ChildDevice();
							ChildDevice.OID = VariablePair.Oid.ToString();

							if (VariablePair.Value.ToString() == null)
							{ ChildDevice.Value = "0"; }
							else
							{ ChildDevice.Value = VariablePair.Value.ToString(); }

							PayLoad.ChildDevices.Add(ChildDevice);
						}
					
						var Message = DeviceManagerInstance.ConvertPayloadToMessage(PayLoad);

						MessageTranslator.MQTTClient.SendMessage(Message);
					}
					else if (ver == (int)SnmpVersion.Ver2)
					{
						// Parse SNMP Version 2 TRAP packet
						SnmpV2Packet pkt = new SnmpV2Packet();
						pkt.decode(indata, inlen);
						Console.WriteLine("** SNMP Version 2 TRAP received from {0}:", inep.ToString());
						if (pkt.Pdu.Type != PduType.V2Trap)
						{
							Console.WriteLine("*** NOT an SNMPv2 trap ****");
						}
						else
						{
							var PayLoad = new SNMPPayload();
							PayLoad.DeviceID = pkt.Pdu.TrapObjectID.ToString();
							PayLoad.DeviceIP = inep.ToString();
							PayLoad.DeviceCommunity = pkt.Community.ToString();
							PayLoad.ChildDevices = new List<ChildDevice>();

							foreach (Vb VariablePair in pkt.Pdu.VbList)
							{
								var ChildDevice = new ChildDevice();
								ChildDevice.OID = VariablePair.Oid.ToString();

								if (VariablePair.Value.ToString() == null)
								{ ChildDevice.Value = "0"; }
								else
								{ ChildDevice.Value = VariablePair.Value.ToString(); }

								PayLoad.ChildDevices.Add(ChildDevice);
							}

							var Message = DeviceManagerInstance.ConvertPayloadToMessage(PayLoad);

							MessageTranslator.MQTTClient.SendMessage(Message);
						}
					}
				}
				else
				{
						Console.WriteLine("Invalid packet received.");
				}
			}
		}
	}
}
