using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace BGPSimulator.BGPMessage
{ 

public class OpenMessage : MessageStructure
{

        // This message have format of this kind:

        //Message Header Format
        /*
        
            //OPEN Message 
        0                   1                   2                   3
           0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
           +-+-+-+-+-+-+-+-+
           |    Version    |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |     My Autonomous System      |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |           Hold Time           |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                         BGP Identifier                        |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           | Opt Parm Len  |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           |                                                               |
           |             Optional Parameters(variable)                    |
           |                                                               |
           +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        */
        //private string _message;
        //BGP open message value is 1 i.e. _type = 1
        private ushort _type;
        //BGP version which is 4 i.e. _version = 4
        private ushort _version;
        //MY AS 2-octet unsigned integer indicates the Autonomous System number of the sender.
        private ushort _myAS;
        //Hold Time 2-octet unsigned integer indicates the number of seconds the sender proposes for the value of the Hold Timer.
        //The Hold Time MUST(0-3 sec) set it 0 i.e. _holdTime = 0;
        private ushort _holdTime;
        //BGP Identifier 4-octet unsigned integer indicates the BGP Identifier of the sender A given BGP speaker sets the value of its BGPIdentifier to an IP address that is assigned to that BGP
        //speaker.The value of the BGP Identifier is determined upon startup and is the same for every local interface and BGP peer.
        private string _bgpIndentifier;
        //This 1-octet unsigned integer indicates the total length of the Optional Parameters field in octets.If the value of this field is zero, no Optional Parameters are present.
        // i.e. _optionalParLength = 0
        private ushort _optionalParLength;
        //byte p_type;
        //byte p_length;
        
        // type 1 octet = 2, version 1 octet = 2, myAS 2 octet = 4, holdTime 2 octet = 4, bgpIdentifyer 4 octet = 8, optionalParamater 1 octet = 2
        public OpenMessage(ushort version,ushort myAS, ushort holdTime, string bgpIdentifier, ushort optimalParLength)
            : base ((ushort)(38 + 2 + 2 + 4 + bgpIdentifier.Length + 1 + 2),40)
        {
            //Text = message;
            Type = 1;
            Version = version;
            MyAS = myAS;
            HoldTime = holdTime;
            BgpIdentifier = bgpIdentifier;
            OptimalParLength = optimalParLength; 
        }
        
        
        public ushort Type
        {
            get { return _type; }
            set
            {
                _type = value;
                writeType(value,38);
            }
        }
        public ushort Version
        {
            get { return _version; }
            set
            {
                _version = value;
                writeVersion(value, 40);
            }
        }
        public ushort MyAS
        {
            get { return _myAS; }
            set
            {
                _myAS = value;
                writeMyAS(value, 42);
            }
        }
        public ushort HoldTime
        {
            get { return _holdTime; }
            set
            {
                _holdTime = value;
                writeHoldTime(value, 44);
            }
        }

        public string BgpIdentifier
        {
            get { return _bgpIndentifier; }
            set
            {
                _bgpIndentifier = value;
                writeBgpIdentifier(value, 46);
            }
        }
        public ushort OptimalParLength
        {
            get { return _optionalParLength; }
            set
            {
                _optionalParLength = value;
                writeOptimalPerLength(value, 55);
            }
        }
      

    }
}