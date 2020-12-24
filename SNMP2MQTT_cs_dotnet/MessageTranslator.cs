using SnmpSharpNet;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace SNMP2MQTT_cs_dotnet
{
    class MessageTranslator
    {
      public static MQTTClient MQTTClient;

      static void Main()
        {
            MQTTClient = new MQTTClient();
            MQTTClient.Connect();

            var SNMPTrap = new SNMPTrap();
            SNMPTrap.Start();
        }
    }
}
