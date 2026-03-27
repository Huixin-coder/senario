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


    //Parameter Class which stores the parameter data
    public class Parameter{
        public string Name;
        public string Type;
        public Single SingleValue;
        public Int32 Int32Value;
        public Int16 Int16Value;
        public byte Int8Value;

        public void SetValue(float Value){
            if (Type == ""){
                throw new Exception ("Parameter type not set");
            }
            switch (Type){
                case "single":
                    SingleValue = Convert.ToSingle(Value);
                    break;
                case "uint32":
                    Int32Value = Convert.ToInt32(Value);
                    break;
                case "int32":
                    Int32Value = Convert.ToInt32(Value);
                    break;
                case "uint16":
                    Int16Value = Convert.ToInt16(Value);
                    break;
                case "uint8":
                    Int8Value = Convert.ToByte(Value);
                    break;
                default:
                    throw new Exception("Parameter Type Not Found: " + Type);
            }
        }

        public float GetValue(){
            if (Type == ""){
                throw new Exception ("Parameter type not set");
            }
            float Value;
            switch (this.Type){
                case "single":
                    Value = SingleValue;
                    break;
                case "uint32":
                    Value = Convert.ToSingle(Int32Value);
                    break;
                case "int32":
                    Value = Convert.ToSingle(Int32Value);
                    break;
                case "uint16":
                    Value = Convert.ToSingle(Int16Value);
                    break;
                case "uint8":
                    Value = Convert.ToSingle(Int8Value);
                    break;
                default:
                    throw new Exception("Parameter Type Not Found: " + Type);
            }
            return Value;
        }

        public byte[] ConvertValueToBytes(){
            byte[] Bytes;
            switch (Type){
                case "single":
                    Bytes = BitConverter.GetBytes(SingleValue);
                    break;
                case "uint32":
                    Bytes = BitConverter.GetBytes(Int32Value);
                    break;
                case "int32":
                    Bytes = BitConverter.GetBytes(Int32Value);
                    break;
                case "uint16":
                    Bytes = BitConverter.GetBytes(Int16Value);
                    break;
                case "uint8":
                    Bytes = new byte[] {Int8Value};
                    break;
                default:
                    throw new Exception("Parameter Type Not Found: " + Type);
            }
            return Bytes;
        }
    }

}