namespace UCL_Impearl
{
    using UnityEngine;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Xml;
    
    public class UDPSend : MonoBehaviour
    {   
        // prefs
        public enum UDPTarget
        {
            CarSimulator,                 // establish a connection to the car simulator. IP should be 192.168.1.103 and port should be 2241
            Injection,                    // establish a connection to the HMI. IP should be 192.168.1.109 and port should be 2000
            Seat,                         // establish a connection to the seat. IP should be 192.168.1.119 and port should be 5264
        }

        public UDPTarget TargetConnection;
        public string IP;
        public int port;

    
        // "connection" things
        IPEndPoint remoteEndPoint;
        UdpClient client;
        //Socket client;
    
        //Params
        public TextAsset SendParamsXML;
        List<Parameter> Parameters;
    
        // START\\
        //Via Unity this is called when we enter runtime
        public void Start()
        {
            //We have tried both of the client creation methods found below (one is commented) and neither seemed to send a signal to your machine,
            // although on our network display (attached in email) it seems to send about 100,000 bytes a second.

            //client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // Debug.Log(client);
            if (client == null) {
                client = new UdpClient(port);
            }
            // Debug.Log(client);

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

            Parameters = SetParamsFromXML();
        }

        //UPDATE\\
        //Via Unity this is called one per frame during runtime
        public void Update(){
            SendUDP();
        }

        // public void OnDisable() {
        //     CloseConnection();
        // }

        public void OnApplicationQuit() {
            CloseConnection();
        }


        //This function reads the XML Parameters file and create a Parameter array from it
        public List<Parameter> SetParamsFromXML(){
            List<Parameter> ParameterList = new List<Parameter>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(SendParamsXML.text);
            XmlNodeList XMLParameters = xmlDoc.GetElementsByTagName("Parameter");
    
            foreach (XmlNode XmlParameter in XMLParameters)
            {
                Parameter Param = new Parameter();
                Param.Name = XmlParameter["Name"].InnerText;
                Param.Type = XmlParameter["Type"].InnerText;
                Param.SetValue(Convert.ToSingle(XmlParameter["Value"].InnerText));
                ParameterList.Add(Param);
            }

            return ParameterList;
        }   




        //Reads the Paramter array and converts the Values to a byte array which is sent
        private void SendUDP(){
            List<byte> Packet = new List<byte>();

            //This cycles through all the parameters listed in your pdf document and converts them into bytes and adds them to the Packet
            foreach(Parameter Param in Parameters){
                byte[] Bytes = Param.ConvertValueToBytes();
                if(Param.Name == "MSteer"){
                    //Debug.Log(Param.GetValue());
                }

                foreach (byte Byte in Bytes){
                    Packet.Add(Byte);
                }

                if (Param.Name == "MessageIdx"){
                    Param.Int32Value++;
                }

                if (Param.Name == "Timestamp"){
                    Param.Int32Value = Convert.ToInt32(Time.fixedDeltaTime/200);
                }
            }

            SendBytes(Packet.ToArray());
        }

        //Sends the supplied Bytes array to the remoteEndPoint via the client set in Init
        private void SendBytes(byte[] Bytes)
        {
            try
            {
                int SendSize = client.Send(Bytes, Bytes.Length, remoteEndPoint);
                //int SendSize = client.SendTo(Bytes, remoteEndPoint);
                //UnityEngine.Debug.Log(SendSize);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }

        public void UpdateParameterValue(string ParameterName, float Value){
            foreach (Parameter Param in Parameters){
                if (Param.Name == ParameterName){
                    Param.SetValue(Value);
                }
            }
        }

        public void CloseConnection() {
            if (client != null) {
                client.Close();
                Debug.Log("Connection closed " + port);
            }
        }
    }

        
}