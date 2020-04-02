using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace HoloToolkit.Unity.InputModule.Tests{
    public class XboxController : XboxControllerHandlerBase{
        [Header("Xbox Controller Test Options")]
        [SerializeField]
        private float movementSpeedMultiplier = 1f;

        [SerializeField]
        private float rotationSpeedMultiplier = 1f;

        [SerializeField]
        private XboxControllerMappingTypes resetButton = XboxControllerMappingTypes.XboxY;

        private Vector3 initialPosition;
        private Vector3 newPosition;
        private Vector3 newRotation;
       
        MqttClient client;
        string clientId;
        string topicPublishPath;
        string topicPublishPath_button;
        string topicSubscribePath;
        string BrokerAddress;
        private string msg;
        float first_buttonpressed = 0f;
        float timeBetweenbuttonpressed = 0.3f;

        public static string robotIP { get; set; } = "192.168.10.51";
        public static string droneIP { get; set; } = "192.168.10.53";

        protected virtual void Start(){
            initialPosition = transform.position;
        
            //MQTT broker
            BrokerAddress = "192.168.10.73";
            clientId = Guid.NewGuid().ToString();
            client = new MqttClient(BrokerAddress);
            client.ProtocolVersion = MqttProtocolVersion.Version_3_1;
            topicPublishPath = "HoloLens/message/push";
            topicPublishPath_button = "Xbox/button";
            topicSubscribePath = "sub/HoloLens";

            try{
                client.Connect(clientId);
            }
            catch (Exception e){
                Debug.Log(string.Format("Exception has occurred in connecting to MQTT {0} ", e ));
                throw new Exception("Exception has occurred in connecting to MQTT", e.InnerException);
            }
 
            //Subscribe
            client.Subscribe(new string[] { topicSubscribePath }, new byte[] { 2 });
        }

        public override void OnXboxInputUpdate(XboxControllerEventData eventData){
            if (string.IsNullOrEmpty(GamePadName)){
                Debug.LogFormat("Joystick {0} with id: \"{1}\" Connected", eventData.GamePadName, eventData.SourceId);
            }

            base.OnXboxInputUpdate(eventData);

            /*get information from xbox controller*/

            if (eventData.XboxLeftStickHorizontalAxis != 0 || eventData.XboxLeftStickVerticalAxis != 0){
                msg = string.Format("{1} {0} 0 {2}", (-0.2) * eventData.XboxLeftStickHorizontalAxis, (-0.2) * eventData.XboxLeftStickVerticalAxis, robotIP);
                client.Publish(topicPublishPath, Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
            }
            if (eventData.XboxLeftBumper_Pressed){
                msg = string.Format("0 0 15 {0}", robotIP);
                client.Publish(topicPublishPath, Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);  
            }
            else if (eventData.XboxRightBumper_Pressed){
                msg = string.Format("0 0 -15 {0}", robotIP);
                client.Publish(topicPublishPath, Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);  
            }
            if (eventData.XboxB_Pressed){
                if (Time.time - first_buttonpressed > timeBetweenbuttonpressed){
                    msg = string.Format("b");
                    client.Publish(topicPublishPath_button, Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                }
                first_buttonpressed = Time.time;
            }
            if (eventData.XboxX_Pressed){
                if (Time.time - first_buttonpressed > timeBetweenbuttonpressed){
                    if (robotIP == "192.168.10.51"){
                        robotIP = "192.168.10.48";
                    }else{
                        robotIP = "192.168.10.51";
                    } 
                }
                first_buttonpressed = Time.time;
            }
        }
    }
}
