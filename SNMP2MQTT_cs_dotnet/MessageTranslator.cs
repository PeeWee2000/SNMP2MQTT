using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SNMP2MQTT_cs_dotnet
{
    class MessageTranslator
    {
      public static MQTTClient MQTTClient;

      static void Main()
        {

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            //Console.Write(host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString());


            Console.WriteLine("Connecting to MQTT Broker...");
            MQTTClient = new MQTTClient();
            MQTTClient.Connect();
            Console.WriteLine("Connected to MQTT Broker");

            Console.WriteLine("Starting SNMP Trap");
            var SNMPTrap = new SNMPTrap();

            Console.WriteLine("SNMP Trap started");

            SNMPTrap.Start();

            
            

        }
    }
}
