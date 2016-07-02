using System;
using System.Net;
using System.Net.Sockets;
using BGPSimulator.BGPMessage;
using System.Text;
using System.Threading;

namespace BGPSimulator.BGP
{
    public static class PacketHandler
    {
       
        
        public static void Handle(byte [] packet, Socket clientSocket)
        {
            
            ushort marker;

            Console.Write("\n" +"Router : " + IPAddress.Parse(((IPEndPoint)clientSocket.LocalEndPoint).Address.ToString()) + " Has recived packet !! Marker: ");
            //Console.Write("Router : " + IPAddress.Parse(((IPEndPoint)clientSocket.LocalEndPoint).Address.ToString()) + " Has recived: ");
            for (int i = 0; i < 16; i++)
            {
                marker = BitConverter.ToUInt16(packet, i * 2);
                Console.Write(marker);
            }
            //packetMarkerDone.Set();
            ushort packetLength = BitConverter.ToUInt16(packet, 32);
            ushort packetType = BitConverter.ToUInt16(packet, 38);
            
            switch (packetType)
            {
                case 1:
                    
                    //Console.WriteLine("OPEN MESSAGE !!");
                    ushort bgpVersion = BitConverter.ToUInt16(packet, 40);
                    ushort autoSystem = BitConverter.ToUInt16(packet, 42);
                    ushort holdTime = BitConverter.ToUInt16(packet, 44);
                    string bgpIdentifier = Encoding.UTF8.GetString(packet, 46, 10);
                    ushort optimalParaLength = BitConverter.ToUInt16(packet, 56);

                     Console.Write(" Length: {0} | Type: {1} | Version: {2} | AS: {3} | HoldTime in Min: {4} | BGPIdentifier: {5} | OptimalParaLenth: {6} ",
                     packetLength, packetType, bgpVersion, autoSystem, holdTime, bgpIdentifier, optimalParaLength);
                    //Console.Write("OPEN MESSAGE");
                    Console.WriteLine(" from Router : " + IPAddress.Parse(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString())+"\n");
                    
                    //packetOpenDone.Set();
                    
                    break;
                case 2:
                    //UpdateMessage(ushort type, UInt16 withdrawRouteLength, ushort ipPrefixLength, string ipPrefix, ushort totalPathAttributeLength, UInt32 attributeLength, 
                    //UInt32 attrFlags, ushort typeCode, string attribute, ushort pathSegmentType,ushort pathSegmentLength,string pathSegmentValue, ushort nlrLength, 
                    //string nlrPrefix)
                    UInt16 withdrawlRouteLength = BitConverter.ToUInt16(packet, 40);
                    string withdrawlRoutes = Encoding.UTF8.GetString(packet, 42, 9);
                    ushort ipPrefixLength = BitConverter.ToUInt16(packet, 51);
                    string ipPrefix = Encoding.UTF8.GetString(packet, 53, 5);
                    ushort totalPathAttribute = BitConverter.ToUInt16(packet, 62);
                    UInt32 attributeLength = BitConverter.ToUInt16(packet, 64);
                    string attribute = Encoding.UTF8.GetString(packet, 66, 9);
                    UInt32 attrFlag = BitConverter.ToUInt16(packet, 75);
                    ushort attrTypeCode = BitConverter.ToUInt16(packet, 77);
                    
                    ushort pathSegmentType = BitConverter.ToUInt16(packet, 79);
                    ushort pathSegmentLength = BitConverter.ToUInt16(packet, 81);
                    string pathSegmentValue = Encoding.UTF8.GetString(packet, 83, 2);
                    ushort nlrLength = BitConverter.ToUInt16(packet, 85);
                    string nlrPrefix = Encoding.UTF8.GetString(packet, 87, 5);
                    Console.Write(" Length: {0} | Type: {1} | WithDrawlRouteLength: {2} | WithdrawlRoute: {3} IP_PrifixLenght: {4} | IP_Prefix: {5} | TotalPathAttributeLength: {6} | AttributeLength: {7} | AttributeFlag: {8} | AttributeTypeCode: {9} | Attribute: {10} | pathSegmentType: {11} | pathSegmentLength: {12} | pathSegmentValue: {13} | nlrLength: {14} | nlrPrefix: {15} ", 
                     packetLength, packetType, withdrawlRouteLength, withdrawlRoutes, ipPrefixLength, ipPrefix, totalPathAttribute,attributeLength, attrFlag, attrTypeCode,attribute, pathSegmentType,
                     pathSegmentLength, pathSegmentValue, nlrLength, nlrPrefix);
                    //Console.Write("OPEN MESSAGE");
                    Console.WriteLine(" from Router : " + IPAddress.Parse(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString()) + "\n");
                    break;
                case 3:
                    ushort errorCode = BitConverter.ToUInt16(packet, 40);
                    ushort errorSubCode = BitConverter.ToUInt16(packet, 42);
                    string error = Encoding.UTF8.GetString(packet, 44, 26);
                    Console.WriteLine("Length: {0} | Type: {1} | ErrorCode: {2} | ErrorSubCode: {3} | Error: {4}", packetLength, packetType, errorCode, errorSubCode, error);
                    break;
                case 4:

                    Console.Write(" Length: {0} | Type: {1} ", packetLength, packetType + " Description: KEEPALIVE");
                    //Console.Write("KeepAlive MESSAGE");
                    Console.WriteLine(" from Router : " + IPAddress.Parse(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString()) + "\n");

                    //packetKeepAliveDone.Set();
                    
                    break;
            }

            
        }
    }
}
