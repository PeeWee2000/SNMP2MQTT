using Newtonsoft.Json;
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
			string SettingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings.json";
			int Port = 0;
			using (StreamReader FileReader = new StreamReader(SettingsPath))
			{
				string FileContents = FileReader.ReadToEnd();
				Port = int.Parse(Regex.Match(FileContents, "(?<=\"SNMPTrapPort\"\\:\\s)\\d+").Value);			
			}

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
						// Parse SNMP Version 1 TRAP packet
						SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
						pkt.decode(indata, inlen);
						Console.WriteLine("** SNMP Version 1 TRAP received from {0}:", inep.ToString());
						Console.WriteLine("*** Trap generic: {0}", pkt.Pdu.Generic);
						Console.WriteLine("*** Trap specific: {0}", pkt.Pdu.Specific);
						Console.WriteLine("*** Agent address: {0}", pkt.Pdu.AgentAddress.ToString());
						Console.WriteLine("*** Timestamp: {0}", pkt.Pdu.TimeStamp.ToString());
						Console.WriteLine("*** VarBind count: {0}", pkt.Pdu.VbList.Count);
						Console.WriteLine("*** VarBind content:");
						foreach (Vb v in pkt.Pdu.VbList)
						{
							Console.WriteLine("**** {0} {1}: {2}", v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
						}
						Console.WriteLine("** End of SNMP Version 1 TRAP data.");


						//MessageTranslator.MQTTClient.SendMessage();
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
							Console.WriteLine("*** Community: {0}", pkt.Community.ToString());
							Console.WriteLine("*** VarBind count: {0}", pkt.Pdu.VbList.Count);
							Console.WriteLine("*** VarBind content:");
							foreach (Vb v in pkt.Pdu.VbList)
							{
								Console.WriteLine("**** {0} {1}: {2}",
								   v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
							}
							Console.WriteLine("** End of SNMP Version 2 TRAP data.");
						}
					}
				}
				else
				{
					if (inlen == 0)
						Console.WriteLine("Zero length packet received.");
				}
			}
		}
	}
}
