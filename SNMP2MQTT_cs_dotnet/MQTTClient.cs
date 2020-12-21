using MQTTnet;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SNMP2MQTT_cs_dotnet
{
    class MQTTClient
    {
        private static MQTTnet.Client.MqttClient Client;

        public async void Connect()
        {
            var factory = new MqttFactory();
           Client = (MQTTnet.Client.MqttClient)factory.CreateMqttClient();
            
            string FileContents;

            string SettingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings.json";
            using (StreamReader FileReader = new StreamReader(SettingsPath))
            {

                FileContents = FileReader.ReadToEnd();
            }

            JObject ProgramSettings = JObject.Parse(FileContents);

            var JSONSettings = ProgramSettings[nameof(MQTTSettings)].ToString();

            var Settings = JsonConvert.DeserializeObject<MQTTSettings>(JSONSettings);

            var options = new MqttClientOptionsBuilder()
                    .WithClientId(Environment.MachineName)
                    .WithTcpServer(Settings.MQTTBrokerIP, Settings.MQTTBrokerPort)
                    .WithCleanSession()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(60));

            if (Settings.MQTTBrokerUserName != "" && Settings.MQTTBrokerPassword != "")
            {
                options.WithCredentials(Settings.MQTTBrokerUserName, Settings.MQTTBrokerPassword);
                options.WithTls();
            }

            await Client.ConnectAsync(options.Build(), CancellationToken.None);

            while (true)
            {
                Thread.Sleep(60);
                lock (Client)
                { Client.PingAsync(CancellationToken.None); }                
            }
        }

        public async void SendMessage()
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("MyTopic")
                .WithPayload("Hello World")
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            lock (Client)
            { Client.PublishAsync(message, CancellationToken.None); }
        }
    }
}
