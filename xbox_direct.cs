using UnityEngine;
using UnityEngine.UI;
using MiniJSON;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Baku.LibqiDotNet;
using LibraryForDrone; //fictious

namespace HoloToolkit.Unity.InputModule.Tests{
    public class xbox_direct : XboxControllerHandlerBase{
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

        public string pepperIP;
        public string droneIP;

        public float rotation_scalefactor = 0.785f;

        //Pepper
        private const string tcpPrefix = "tcp://";
        private const string portSuffix = ":9559";
        private QiSession _session;
        public float move_scalefactor = 3.0f;

        //Drone (fictious)
        private DroneSession session_drone;
        public float move_scalefactor_drone = 5.0f;
        
        void Start(){
            initialPosition = transform.position;
            //Pepper
            if(!string.IsNullOrEmpty(pepperIP)){
                _session = QiSession.Create(tcpPrefix + pepperIP + portSuffix);
                if (!_session.IsConnected){
                    Debug.Log("Failed to establish connection");
                    return;
                }
            //Drone (fictious)
            }else if(!string.IsNullOrEmpty(droneIP)){
                session_drone = DroneSession.Create(tcpPrefix + droneIP + portSuffix);
                if (!_session.IsConnected){
                    Debug.Log("Failed to establish connection");
                    return;
                }
            }
        }

        public override void OnXboxInputUpdate(XboxControllerEventData eventData){
            if (string.IsNullOrEmpty(GamePadName)){
                Debug.LogFormat("Joystick {0} with id: \"{1}\" Connected", eventData.GamePadName, eventData.SourceId);
            }

            base.OnXboxInputUpdate(eventData);

            /*get information from xbox controller*/

            if(_session.IsConnected){
                if (eventData.XboxLeftStickHorizontalAxis != 0 || eventData.XboxLeftStickVerticalAxis != 0){
                    var motion = _session.GetService("ALMotion");
                    motion["moveTo"].Call(eventData.XboxLeftStickHorizontalAxis * move_scalefactor, eventData.XboxLeftStickVerticalAxis * (-1) * move_scalefactor, 0f);
                }
                if (eventData.XboxLeftBumper_Pressed){
                    var motion = _session.GetService("ALMotion");
                    motion["moveTo"].Call(0f, 0f, rotation_scalefactor);
                }
                else if (eventData.XboxRightBumper_Pressed){
                    var motion = _session.GetService("ALMotion");
                    motion["moveTo"].Call(0f, 0f, (-1) * rotation_scalefactor);
                }
                if (eventData.XboxB_Pressed){
                    if (Time.time - first_buttonpressed > timeBetweenbuttonpressed){
                        var motion = _session.GetService("ALMotion");
                        motion["setAngles"].Call("HeadYaw", angle, 0f);
                    }
                    first_buttonpressed = Time.time;
                }
                if (eventData.XboxX_Pressed){
                    if (Time.time - first_buttonpressed > timeBetweenbuttonpressed){
                        if (pepperIP == "192.168.10.51"){
                            _session.Close();
                            _session.Destroy();
                            pepperIP = "192.168.10.48"
                            _session = QiSession.Create(tcpPrefix + pepperIP + portSuffix);
                        }else{
                            _session.Close();
                            _session.Destroy();
                            pepperIP = "192.168.10.51"
                            _session = QiSession.Create(tcpPrefix + pepperIP + portSuffix);
                        } 
                    }
                    first_buttonpressed = Time.time;
                }
            
            //Drone (fictious)
            }else if(session_drone.isConnected){
                if (eventData.XboxLeftStickHorizontalAxis != 0 || eventData.XboxLeftStickVerticalAxis != 0){
                    CallDronesAPI_move(eventData.XboxLeftStickHorizontalAxis * move_scalefactor_drone, eventData.XboxLeftStickVerticalAxis * (-1) * move_scalefactor_drone, 0f);
                }
                if (eventData.XboxLeftBumper_Pressed){
                    CallDronesAPI_rotate(0f, 0f, rotation_scalefactor);
                }
                else if (eventData.XboxRightBumper_Pressed){
                    CallDronesAPI_rotate(0f, 0f, (-1) * rotation_scalefactor);
                }
                if (eventData.XboxB_Pressed){
                    if (Time.time - first_buttonpressed > timeBetweenbuttonpressed){
                        CallDronesAPI_rotate(0f, 0f, 0f);
                    }
                    first_buttonpressed = Time.time;
                }
                if (eventData.XboxX_Pressed){
                    if (Time.time - first_buttonpressed > timeBetweenbuttonpressed){
                        if (droneIP == "192.168.10.53"){
                            session_drone.Close();
                            session_drone.Destroy();
                            droneIP = "192.168.10.54"
                            session_drone = DroneSession.Create(tcpPrefix + droneIP + portSuffix);
                        }else{
                            session_drone.Close();
                            session_drone.Destroy();
                            droneIP = "192.168.10.53"
                            session_drone = DroneSession.Create(tcpPrefix + droneIP + portSuffix);
                        } 
                    }
                    first_buttonpressed = Time.time;
            }
        }
    }
}
