﻿using MQTTnet;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

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

            string SettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Settings.json");
            using (StreamReader FileReader = new StreamReader(SettingsPath))
            {
                FileContents = FileReader.ReadToEnd();
            }

            JObject ProgramSettings = JObject.Parse(FileContents);

            var JSONSettings = ProgramSettings[nameof(MQTTSettings)].ToString();

            var Settings = JsonConvert.DeserializeObject<MQTTSettings>(JSONSettings);

            if (Settings.MQTTClientName == null)
            {
                Settings.MQTTClientName = "SNMP2MQTT - " + Environment.MachineName;
            }

            var options = new MqttClientOptionsBuilder()
                    .WithClientId(Settings.MQTTClientName)
                    .WithTcpServer(Settings.MQTTBrokerIP, Settings.MQTTBrokerPort)
                    .WithCleanSession()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(65));

            if (Settings.MQTTBrokerUserName != "" && Settings.MQTTBrokerPassword != "")
            {
                options.WithCredentials(Settings.MQTTBrokerUserName, Settings.MQTTBrokerPassword);
                options.WithTls();
            }

            await Client.ConnectAsync(options.Build(), CancellationToken.None);


            var Message = new MqttApplicationMessage();
            Message.Topic = "SNMP2MQTT";
            Message.Payload = Encoding.UTF8.GetBytes("Online");


            await Client.PublishAsync(Message, CancellationToken.None);

            while (true)
            {
                Thread.Sleep(60);
                lock (Client)
                { Client.PingAsync(CancellationToken.None); }                
            }
        }

        public void SendMessage(List<MqttApplicationMessage> Messages)
        {
            if (Messages != null)
            { 
                lock (Client)
                { 
                    foreach (var Message in Messages)
                    { Client.PublishAsync(Message, CancellationToken.None); }
                }
            }
        }
    }
}
