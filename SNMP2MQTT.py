from pysnmp.entity import engine, config
from pysnmp.carrier.asyncore.dgram import udp
from pysnmp.entity.rfc3413 import ntfrcv

import paho.mqtt.client as paho

import yaml

import sys
import os





class SNMPDevice:
  def __init__(device, oid, age):
    device.ticks = ticks
    device.deviceoid = deviceoid
    device.ip = ip
    device.community = community
    device.octet = octet    
    device.payload = payload







#ConfigPath = os.path.join(sys.path[0], 'config.yaml')

#with open(ConfigPath) as file:
#    # The FullLoader parameter handles the conversion from YAML
#    # scalar values to Python the dictionary format
#    ConfigSettings = yaml.load(file, Loader=yaml.FullLoader)
#    doc = yaml.load(file, Loader=yaml.FullLoader)

#    sort_file = yaml.dump(doc, sort_keys=True)
#    print(sort_file)


#    print(ConfigSettings)



#dict_file = [{'sports' : ['soccer', 'football', 'basketball', 'cricket', 'hockey', 'table tennis']},
#{'countries' : ['Pakistan', 'USA', 'India', 'China', 'Germany', 'France', 'Spain']}]

#with open(ConfigPath, 'w') as file:
#    documents = yaml.dump(dict_file, file, sort_keys=True)




#MQTT Publishing Section

broker="192.168.86.22"
port=1883
def on_publish(client,userdata,result):             #create function for callback
    print("data published \n")
    pass
client1= paho.Client("control1")                           #create client object
client1.on_publish = on_publish                          #assign function to callback



#SNMP Trap Section

snmpEngine = engine.SnmpEngine()

TrapAgentAddress='192.168.86.229'; #Trap listerner address
Port=162;  #trap listerner port

print("Agent is listening SNMP Trap on "+TrapAgentAddress+" , Port : " +str(Port));
print('--------------------------------------------------------------------------');
config.addTransport(
    snmpEngine,
    udp.domainName + (1,),
    udp.UdpTransport().openServerMode((TrapAgentAddress, Port))
)

#Configure community here
config.addV1System(snmpEngine, 'Test', 'Test')
config.addV1System(snmpEngine, 'my-area1', 'Waef')

def cbFun(snmpEngine, stateReference, contextEngineId, contextName,
          varBinds, cbCtx):
    print("Received new Trap message");
    for name, val in varBinds:        
        print('%s = %s' % (name.prettyPrint(), val.prettyPrint()))
    client1.connect(broker,port)                             
    client1.publish("Wewo","Trap Message Converted")


ntfrcv.NotificationReceiver(snmpEngine, cbFun)

snmpEngine.transportDispatcher.jobStarted(1)


try:
    snmpEngine.transportDispatcher.runDispatcher()
except:
    snmpEngine.transportDispatcher.closeDispatcher()
    raise
