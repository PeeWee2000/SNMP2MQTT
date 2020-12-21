using SnmpSharpNet;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace SNMP2MQTT_cs_dotnet
{
    class Program
    {
      static void Main()
        {
            var Wewo = new MQTTClient();
            Wewo.Connect();
            //Wewo.SendMessage();
            var Waef = new SNMPTrap();
            Waef.Start();
        }
    }
}
