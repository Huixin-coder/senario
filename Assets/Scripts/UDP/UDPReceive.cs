namespace UCL_Impearl{
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

    public class UDPReceive : MonoBehaviour
    {
        // prefs
        public enum UDPTarget
        {
            CarSimulator,                 // establish a connection to the car simulator. Port should be 2240
            Injection,                    // establish a connection to the HMI. Port should be 2242
            Seat,                         // establish a connection to the seat. Port hasn't been assigned yet
        }

        public UDPTarget TargetConnection;
        public int port;
    
        // udpclient object
        UdpClient client;
        IPEndPoint IPEP;
        Thread ReceiveThread;
        bool DataProcessed = false;
        string returnData = "";
        static readonly object lockObject = new object();

        //Params
        public TextAsset SendParamsXML;
        List<Parameter> Parameters;
        
        // start from unity3d
        public void Start()
        {
            Parameters = SetParamsFromXML();
            client = new UdpClient(port);

            //  Setup background UDP listener thread.
            ReceiveThread = new Thread(new ThreadStart(ReceiveData));
            ReceiveThread.IsBackground = true;
            ReceiveThread.Start();
        }

        public void Update(){
            //ReceiveData();
            foreach (Parameter param in Parameters){
                if (param.Name == "aSteer" && param.GetValue()>3.3){
                    //UnityEngine.Debug.Log("NOW");
                }
                if (param.Name == "CAN_VIn1"){
                    // UnityEngine.Debug.Log(param.SingleValue);
                } 
                if (param.Name == "CAN_VIn2"){
                    // UnityEngine.Debug.Log(param.SingleValue);
                }
            }

            if (DataProcessed){
                /*lock object to make sure there data is 
                *not being accessed from multiple threads at the same time*/
                lock (lockObject)
                {
                    DataProcessed = false;

                    //Process received data
                    Debug.Log("Received: " + returnData);

                    //Reset it for next read(OPTIONAL)
                    returnData = "";
                }
            }
        }

        // public void OnDisable() {
        //     CloseConnection();
        // }

        public void OnApplicationQuit(){
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
                
                ParameterList.Add(Param);
            }
            return ParameterList;
        }
    
        private void ReceiveData() {
            //  While thread is still alive.
            while(true) {
                IPEP = new IPEndPoint(IPAddress.Any, port);
                //  Grab the data.
                byte[] data = client.Receive(ref IPEP);
                ReadUDPPacket(data);
                lock (lockObject)
                {
                    returnData = Encoding.ASCII.GetString(data);

                    if (returnData == "1\n")
                    {
                        //Done, notify the Update function
                        DataProcessed = true;
                    }
                }


                //  Sleep the thread.
                Thread.Sleep(client.Client.ReceiveTimeout);
            }
        }

        public void ReadUDPPacket(byte[] Packet){ // TODO: Needs heavy refactor and move to Parameter.cs
            // UnityEngine.Debug.Log(Packet);
            int PacketIdx = 0;
            //This cycles through all the parameters listed in your pdf document and converts them into bytes and adds them to the Packet
            foreach(Parameter Param in Parameters){
                int ParamByteSize;
                int StartPacketIdx;
                switch (Param.Type){
                    case "single":
                        ParamByteSize = 4;
                        StartPacketIdx = PacketIdx;
                        byte[] ParamBytesSingle = new byte[ParamByteSize];
                        int x1 = 0;
                        for (int Ind = PacketIdx; PacketIdx < (StartPacketIdx + ParamByteSize); PacketIdx++){
                            ParamBytesSingle[x1] = Packet[PacketIdx];
                            x1++;
                        }
                        Param.SingleValue = BitConverter.ToSingle(ParamBytesSingle, 0);
                        break;
                    case "uint32":
                        ParamByteSize = 4;
                        StartPacketIdx = PacketIdx;
                        byte[] ParamBytesU32 = new byte[ParamByteSize];
                        int x2 = 0;
                        for (int Ind = PacketIdx; PacketIdx < (StartPacketIdx + ParamByteSize); PacketIdx++){
                            ParamBytesU32[x2] = Packet[PacketIdx];
                            x2++;
                        }
                        Param.Int32Value = BitConverter.ToInt32(ParamBytesU32, 0);
                        // if (Param.Name == "HMIenableAV") {
                        if (port == 2242) {
                            UnityEngine.Debug.Log(Param.Name + ": " +  Param.Int32Value);
                        }
                        // }
                        break;
                    case "int32":
                        ParamByteSize = 4;
                        StartPacketIdx = PacketIdx;
                        byte[] ParamBytes32 = new byte[ParamByteSize];
                        int x3 = 0;
                        for (int Ind = PacketIdx; PacketIdx < StartPacketIdx + ParamByteSize; PacketIdx++){
                            ParamBytes32[x3] = Packet[PacketIdx];
                            x3++;
                        }
                        Param.Int32Value = BitConverter.ToInt32(ParamBytes32, 0);
                        break;
                    case "uint16":
                        ParamByteSize = 2;
                        StartPacketIdx = PacketIdx;
                        byte[] ParamBytes16 = new byte[ParamByteSize];
                        int x4 = 0;
                        for (int Ind = PacketIdx;PacketIdx < StartPacketIdx + ParamByteSize; PacketIdx++){
                            ParamBytes16[x4] = Packet[PacketIdx];
                            x4++;
                        }
                        Param.Int16Value = BitConverter.ToInt16(ParamBytes16, 0);
                        break;
                    case "uint8":
                        Param.Int8Value = Packet[PacketIdx];
                        PacketIdx++;
                        break;
                    default:
                        throw new Exception("Parameter Type Not Found: " + Param.Type);
                }
            }
        }

        public Parameter GetParameter(string ParameterName){
            Parameter returnParameter = new Parameter();
            foreach(Parameter Param in Parameters){
                if (Param.Name == ParameterName){
                    returnParameter = Param;
                }
            }
            if (returnParameter.Name == ""){
                throw new Exception("Parameter does not exist with that name");

            }
            return returnParameter;
        }

        public void CloseConnection() {
            
        if (ReceiveThread != null) {
            ReceiveThread.Abort();
        }

        if (client != null) {
            client.Close();
            Debug.Log("Connection closed " + port);
        }
            
        }


    }

        
}