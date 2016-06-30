using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;

namespace BGPSimulator.BGP
{
    public static class GlobalVariables
    {
        public static string listnerConnectionState;
        public static string speakerConnectionState;
        public static int listnerPortNumber = 179;
        public static int speakerPortNumber = 176;
        public static ushort AS1 = 1;
        public static string prefixAS1 = "127.1";
        public static string prefixAS2 = "127.2";
        public static string prefixAS3 = "127.3";
        public static string as1_IP_peifix = "127.1.0.";
        public static ushort AS2 = 2;
        public static string as2_IP_Prefix = "127.2.0.";
        public static ushort AS3 = 3;
        public static string as3_IP_Prefix = "127.3.0.";
        //public static ushort autonomousSystemSpeaker;
        //public static ushort autonomousSystemListner;
        public static ushort packetAS;
        public static int connCountListner;
        public static int ConnectionCount;
        public static int openMsgSendCount;
        public static int openMsgRecivedCount;
        public static int keepAliveMsgSendCount;
        public static int keepAliveExpiredCount;
        public static int currentConnectionCount;
        public static string connectionStatus;
        public static bool True;
        public static bool keepAlive;
        public static int speakerNumber;
        public static int listnerNumber;
        public static ushort bgpVerson = 4;
        public static ushort holdTime = 4;
        public static ushort optimalParLength = 0;
        //implement ip
        public static string speakerIpAddress;
        public static string listnerIpAddress;
        public static int conCountSpeaker;
        public static int currentSpeakerCount;
       
        //connectCount and Listner
        //ConcurrentDictionary is used for the thread safty
        public static ConcurrentDictionary<string, ushort> speaker_AS = new ConcurrentDictionary<string, ushort>();
        public static ConcurrentDictionary<string, ushort> listner_AS = new ConcurrentDictionary<string, ushort>();
        public static ConcurrentDictionary<int, string> conAnd_Listner = new ConcurrentDictionary<int, string>();
        public static ConcurrentDictionary<int, string> conAnd_Speaker = new ConcurrentDictionary<int, string>();
        
        public static ConcurrentDictionary<ushort, ushort> speakerConAnd_AS = new ConcurrentDictionary<ushort, ushort>();
        public static ConcurrentDictionary<ushort, ushort> listnerConAnd_AS = new ConcurrentDictionary<ushort, ushort>();
        public static ConcurrentDictionary<int, Socket> listnerSocket_Dictionary = new ConcurrentDictionary<int, Socket>();
        public static ConcurrentDictionary<int, Socket> SpeakerSocket_Dictionary = new ConcurrentDictionary<int, Socket>();

        //UPDATE Variables
        public static Dictionary<int, Tuple<string, ushort, string, ushort>> conSpeakerAs_ListnerAs = new Dictionary<int, Tuple<string, ushort, string, ushort>>();
        public static DataTable data = new DataTable();
        public static Dictionary<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> Adj_RIB_Out = new Dictionary<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>>();
        public static Dictionary<int, Tuple<int, string>> NLRI = new Dictionary<int, Tuple<int, string>>();
        public static Dictionary<int, Tuple<int, string,int, ushort>> pathAttribute = new Dictionary<int, Tuple<int, string, int, ushort>>();
        public static string withdrawl_IP_Address;
        public static int withdrawl_Length;
        public static Dictionary<int, Tuple<string, int>> withdrawnRoutes = new Dictionary<int, Tuple <string, int>>();
        public static Dictionary<int, Tuple<int, string>> pathSegment = new Dictionary<int, Tuple<int, string>>();
        public static Dictionary<int, string> interASConIP = new Dictionary<int, string>();
        //Notification Message Variables
        public static ushort errorCode = 6;
        public static ushort erorSubCode = 8;
        
    }
}
