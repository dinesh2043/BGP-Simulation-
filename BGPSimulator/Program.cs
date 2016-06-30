
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using BGPSimulator.BGPMessage;
using BGPSimulator.FSM;
using BGPSimulator.BGP;

namespace BGPSimulator
{
    public static class Program
    {
       
        public static void Main(string[] args)
        {
            
            Console.WriteLine("Run the BGP simulator");
            //InitilizeBGPListnerSpeaker init_BGP = new InitilizeBGPListnerSpeaker();
            //init_BGP.StartListner();
            FinateStateMachine FSM_Server = new FinateStateMachine();
             FSM_Server.Timers();
            GlobalVariables.True = true;
            //FSM.TcpConnectionConformed(true);
            FSM_Server.StartBGPConnectionMethod(GlobalVariables.True);

            //Console.WriteLine(FSM_Server.getState("Server"));
            //FSM.TcpConnectionConformed(true);
            //FSM.IdleState();
            //InitilizeRouters init_routers = new InitilizeRouters();
            //init_routers.StratRouters();
            Routes bgpRoutes = new Routes();
           
            UpdateMessageHandling createUpdate = new UpdateMessageHandling();
            CloseRouter close = new CloseRouter();

            while (true)
            {
                //Console.WriteLine("Type help for command info:"); // Prompt
                //Console.WriteLine("Type commands for further execution:"); // Prompt
                string line = Console.ReadLine(); // Get string from user
                if (line == "help") // Check string
                {
                    Console.WriteLine("Type 'as1' or 'as2' or 'as3' to see routing table info" );
                    //break;
                }
                if (line == "as1")
                {
                   
                    bgpRoutes.DisplayDataAS1();
                }
                if (line == "as2")
                {
                    bgpRoutes.DisplayDataAS2();
                }
                if (line == "as3")
                {
                    bgpRoutes.DisplayDataAS3();
                }
                if(line == "update")
                {
                    GlobalVariables.data = Routes.GetTable();
                    Console.WriteLine("Local Policy For AS1, AS2 and AS3 is UPDATED");
                    createUpdate.adj_RIB_Out();
                    //createUpdate.withadrawlRoutes("");
                    createUpdate.pathAttribute();
                    createUpdate.networkLayerReachibility();
                    createUpdate.pathSegment();
                    //createUpdate.sendUpdateMsg_AS1();
                    
                    //createUpdate.sendUpdateMsg_AS3();
                }
                if(line == "updateAS1")
                {
                    createUpdate.sendUpdateMsg(1);
                }
                if (line == "updateAS2")
                {
                    createUpdate.sendUpdateMsg(2);
                    createUpdate.sendUpdateMsg(3);
                }
                if (line == "updateAS3")
                {
                    createUpdate.sendUpdateMsg(4);
                }
                
                if(line == "127.1.0.0")
                {
                    close.CloseSpeakerListner(line);
                }
                
                //Console.Write("You typed "); // Report output
                //Console.Write(line.Length);
                //Console.WriteLine(" character(s)");

                //Console.ReadLine();
                //Console.WriteLine("Press any key to exit.");

                //Console.ReadLine();
                // Console.ReadKey();
            }




            // marker and length values are 16 and 3 octets which contains 32 and 6 slots
            // type is 1 octet consists 2 slots
            //KeepAliveMessage(ushort type)
            /**
            KeepAliveMessage KEEPALIVE = new KeepAliveMessage();
            Console.WriteLine("KEEPALIVE MESSAGE CHECK:");
            Console.WriteLine(string.Join(":", KEEPALIVE.BGPmessage));

            //OpenMessage(ushort type, ushort version,ushort myAS, ushort holdTime, string bgpIdentifier, ushort optimalParLength)
            OpenMessage OPEN = new OpenMessage(4, 1, 0, IPAddress.Parse("101.0.0.0"), 0);
            Console.WriteLine("OPEN MESSAGE CHECK:");
            Console.WriteLine(string.Join(":", OPEN.BGPmessage));
            
            //type 1 octet, withdrawRouteLength 2 octets, ipPrefixLength 1 octet, ipPrefix.Length, totalPathAttributeLength 2 octet, attributePath.Length 2 octet, 
            //attributeType 2 octet, attribute.Length = 0 1 octet, attrFlags = 1 1 0ctet, typeCode 1 octet, origin 0 or 1 (Type Code 1), pathSegmentType = 1 or 2 (1 octet), 
            //pathSegmentLength 1 Octet, pathSegment value, asPath (Type Code 2), nextHop value IPAddress(Type Code 3), multiExitDisc (Type Code 4), localPref (Type Code 5),
            //automaticAggrigator (Type Code 6),aggrigator (Type Code 7), nlrLength IpPrefix.Length, nlePrefix IP address Prefix

            //UpdateMessage(ushort type, UInt16 withdrawRouteLength, ushort ipPrefixLength, string ipPrefix, ushort totalPathAttributeLength, UInt32 attributeLength, 
            //UInt32 attrFlags, ushort typeCode, string attribute, ushort pathSegmentType,ushort pathSegmentLength,string pathSegmentValue, ushort nlrLength, 
            //string nlrPrefix)
            //(38 + 2 + 4 + 2 +ipPrefix.Length + 4 + 4 + 2 + 2 + 2 +attribute.Length+ 2 + 2 + pathSegmentValue.Length + 2 + nlrPrefix.Length),19)
            UpdateMessage UPDATE = new UpdateMessage(2, 4, 6, IPAddress.Parse("101.0.0.0"), 24, 9, 1, 2, "myAttr", 2, 4, "MyPathSeg", 4, IPAddress.Parse("102.0.0.0"));
            Console.WriteLine("UPDATE MESSAGE CHECK:");
            Console.WriteLine(string.Join(":", UPDATE.BGPmessage));

            //NotificationMessage(ushort type, ushort errorCode, ushort errorSubCode, string data)
            NotificationMessage NOTIFICARION = new NotificationMessage(3, 3, 6, "Invalid ORIGIN Attribute");
            Console.WriteLine("NOTIFICATION MESSAGE CHECK:");
            Console.WriteLine(string.Join(":", NOTIFICARION.BGPmessage));

            FinateStateMachine FSM = new FinateStateMachine();
            FSM.Timers();

            FSM.StartBGPConnectionMethod(true);
            FSM.StopBGPConnectionMethod(true);
            
            FSM.TcpConnectionAckd(true);
            FSM.TcpConnectionConformed(true);
            FSM.TcpConnectionFailed(true);
            FSM.BGPHederError(true);
            FSM.BGPOpenMsgSent(true);
            FSM.BGPOpenMsgError(true);
            FSM.BGPKeepAliveMsgSend(true);
            FSM.BGPNotifyMsgSent(true);
            FSM.BGPNotifyMsgErrorSent(true);
            FSM.BGPUpdateMsgSent(true);
            FSM.BGPUpdateMsgError(true);
            **/

            /**
            FSM.ConnectionRetry(new TimeSpan(0, 0, 5));
            FSM.StopConnectionHold(new TimeSpan(0, 0, 6));
            FSM.StopConnectionKeepalive(new TimeSpan(0, 0, 7));
            **/
            // Keep the console window open in debug mode.

        }
        
        
        
    }
}
