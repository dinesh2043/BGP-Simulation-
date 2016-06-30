using System;
using System.Net;

namespace BGPSimulator.BGPMessage
{ 

public class UpdateMessage : MessageStructure
{

        //See more on https://tools.ietf.org/html/rfc4271#section-4.1 page 19
        //UPDATE Message Format
        /*
          +-----------------------------------------------------+
          |   Withdrawn Routes Length(2 octets)                |
          +-----------------------------------------------------+
          |   Withdrawn Routes(variable)                       |
          +-----------------------------------------------------+
          |   Total Path Attribute Length(2 octets)            |
          +-----------------------------------------------------+
          |   Path Attributes(variable)                        |
          +-----------------------------------------------------+
          |   Network Layer Reachability Information(variable) |
          +-----------------------------------------------------+
        */
        //type = 2
        private ushort _type;
        //_withdrawnRoutesLength 2-octets unsigned integer indicates the total length of the Withdrawn Routes field in octets.Its value allows the
        //length of the Network Layer Reachability Information field to be determined i.e. _withdrawnRoutes.Length;
        private UInt16 _withdrawnRoutesLength;
        //_withdrawnRoutes is a variable-length field that contains a list of IP address prefixes for the routes that are being withdrawn from
        //service.Each IP address prefix is encoded as a 2-tuple of the form <length(1 0ct), prefix(variable)>, whose fields are described below:
        private string _withdrawnRoutes;
        //withdraw routes is a duple (ip_prefix_length, prefix) 
        //assigned to variable in constructor
        private ushort _ipPrefixLength;
        private string _ipPrefix;

        //_totalPathAttributeLength 2-octet unsigned integer indicates the total length of the Path Attributes field in octets.Its value allows the length
        //of the Network Layer Reachability field to be determined
        private ushort _totalPathAttributeLength;
        //_attributePath is a triple <attribute type, attribute length, attribute value> of variable length.Attribute Type is a two-octet field that consists of the
        // _attributePath Flags octet, followed by the Attribute Type Code octet.
        private string _attributePath;
        //_attributeType Type is a two-octet field that consists of the Attribute Flags octet, followed by the Attribute Type Code octet.
        /*
            The high-order bit (bit 0) of the Attribute Flags octet is the Optional bit.It defines whether the attribute is optional (if set to 1) 
        or well-known(if set to 0).
            The second high-order bit (bit 1) of the Attribute Flags octet is the Transitive bit.  It defines whether an optional
         attribute is transitive (if set to 1) or non-transitive (if set to 0).
            The third high-order bit (bit 2) of the Attribute Flags octe is the Partial bit.  It defines whether the information contained in the optional transitive attribute is partial (if
         set to 1) or complete (if set to 0).  For well-known attributes and for optional non-transitive attributes, the Partial bit MUST be set to 0.
            The fourth high-order bit (bit 3) of the Attribute Flags octet is the Extended Length bit.  It defines whether the Attribute Length is one octet 
            (if set to 0) or two octets (if set to 1).
            The lower-order four bits of the Attribute Flags octet are unused.  They MUST be zero when sent and MUST be ignored when received.

            i.e. _attributeType = 11110000 
            0                   1
                   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                   |  Attr. Flags  |Attr. Type Code|
                   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        */
        private UInt64 _attributeType;
        private UInt32 _attributeLength;
        private string _attribute;
        private UInt32 _attrFlags;
        private ushort _typeCode;
        // The supported Attribute Type Codes, and their attribute values and uses are as follows: 
        //a) ORIGIN (Type Code 1):
        //ORIGIN is a well-known mandatory attribute that defines the origin of the path information.The data octet can assume the following values:
        //value = 0; means = IGP - Network Layer Reachability Information is interior to the originating AS
        //value = 1; means = EGP - Network Layer Reachability Information learned via the EGP protocol[RFC904]
        //value = 2 ; means = INCOMPLETE - Network Layer Reachability Information learned by some other means
        private ushort _origin;
        //b) AS_PATH (Type Code 2):
        //AS_PATH is a well-known mandatory attribute that is composed of a sequence of AS path segments.  Each AS path segment is represented by a triple 
        //<path segment type, path segment length, path segment value>
        private string _asPath;
        //The path segment type is a 1-octet length field with the following values defined:_pathSegmentType = 1; means = AS_SET: unordered set of ASes a route in the
        // UPDATE message has traversed. _pathSegmentType = 2; means = AS_SEQUENCE: ordered set of ASes a route in the UPDATE message has traversed
        private ushort _pathSegmentType;
        private ushort _pathSegmentLength;
        //The path segment value field contains one or more AS numbers, each encoded as a 2-octet length field. Usage of this attribute is defined in 5.1.2.
        private string _pathSegmentValue;
        //c) NEXT_HOP (Type Code 3): This is a well-known mandatory attribute that defines the (unicast) IP address of the router that SHOULD be used as
        //the next hop to the destinations listed in the Network Layer Reachability Information field of the UPDATE message.Usage of this attribute is defined in 5.1.3.
        private IPAddress _nextHop;
        //d) MULTI_EXIT_DISC (Type Code 4): This is an optional non-transitive attribute that is a four-octet unsigned integer.  The value of this attribute
        //MAY be used by a BGP speaker's Decision Process to discriminate among multiple entry points to a neighboring autonomous system.Usage of this attribute is 
        //defined in 5.1.4.
        private string _multiExitDisc;
        //e) LOCAL_PREF (Type Code 5): LOCAL_PREF is a well-known attribute that is a four-octet unsigned integer.  A BGP speaker uses it to inform its other internal 
        //peers of the advertising speaker's degree of preference for an advertised route. Usage of this attribute is defined in 5.1.5.
        private UInt64 _localPref;
        // f) ATOMIC_AGGREGATE (Type Code 6) ATOMIC_AGGREGATE is a well-known discretionary attribute of length 0. Usage of this attribute is defined in 5.1.6.
        private ushort _automaticAggregate;
       //g) AGGREGATOR (Type Code 7) AGGREGATOR is an optional transitive attribute of length 6. The attribute contains the last AS number that formed the
       //aggregate route(encoded as 2 octets), followed by the IP address of the BGP speaker that formed the aggregate route (encoded as 4 octets).  
       //This SHOULD be the same address as the one used for the BGP Identifier of the speaker. Usage of this attribute is defined in 5.1.7.
       private string _aggregator;
       //Network Layer Reachability Information:This variable length field contains a list of IP addressprefixes.  The length, in octets, of the Network Layer
       //Reachability Information is not encoded explicitly, but can be calculated as:UPDATE message Length - 23 - Total Path Attributes Length - Withdrawn Routes Length
       private string _networkLayerReachablity;
        /*
        Reachability information is encoded as one or more 2-tuples of
              the form <length, prefix>, whose fields are described below:

                       +---------------------------+
                       |   Length (1 octet)        |
                       +---------------------------+
                       |   Prefix (variable)       |
                       +---------------------------+

              The use and the meaning of these fields are as follows:
              a) Length: The Length field indicates the length in bits of the IP address prefix.  A length of zero indicates a prefix that
                 matches all IP addresses (with prefix, itself, of zero octets).

              b) Prefix: The Prefix field contains an IP address prefix, followed by enough trailing bits to make the end of the field fall on an
                 octet boundary.  Note that the value of the trailing bits is irrelevant.

        */

            private ushort _nlrLength;
            private string _nlrPrefix;
        //type 1 octet, withdrawRouteLength 2 octets, ipPrefixLength 1 octet, ipPrefix.Length, totalPathAttributeLength 2 octet, attributePath.Length 2 octet, 
        //attributeType 2 octet, attribute.Length = 0 1 octet, attrFlags = 1 1 0ctet, typeCode 1 octet, origin 0 or 1 (Type Code 1), pathSegmentType = 1 or 2 (1 octet), 
        //pathSegmentLength 1 Octet, pathSegment value, asPath (Type Code 2), nextHop value IPAddress(Type Code 3), multiExitDisc (Type Code 4), localPref (Type Code 5),
        //automaticAggrigator (Type Code 6),aggrigator (Type Code 7), nlrLength IpPrefix.Length, nlePrefix IP address Prefix

        public UpdateMessage(UInt16 withdrawRouteLength, string withdrawlRoute, ushort ipPrefixLength, string ipPrefix, ushort totalPathAttributeLength, 
             UInt32 attributeLength, UInt32 attrFlags, ushort typeCode, string attribute, ushort pathSegmentType, 
            ushort pathSegmentLength,string pathSegmentValue, ushort nlrLength, string nlrPrefix)
            : base ((ushort)(38 + 2 + withdrawlRoute.Length+ 4 + 2 + ipPrefix.Length + 4 + 4 + 4 + 4 + 4 +attribute.Length+ 4 + 4 + pathSegmentValue.Length + 4 + nlrPrefix.Length),184)
        {
            //Text = message;
            Type = 2;
            WithdrawRouteLength = withdrawRouteLength;
            WithdrawRoutes = withdrawlRoute;
            IpPrefixLength = ipPrefixLength;
            IpPrefix = ipPrefix;
            TotalPathAttributeLength = totalPathAttributeLength;
            //AttributePath = attributePath;
            //AttributeType = attributeType;
            AttrFlags = attrFlags;
            TypeCode = typeCode;
            AttributeLength = attributeLength;
            Attribute = attribute;
            //Origin = origin;
            //AsPath = asPath;
            PathSegmentType = pathSegmentType;
            PathSegmentLength = pathSegmentLength;
            PathSegmentValue = pathSegmentValue;
            //NextHop = nextHop;
            //MultiExitDisc = multiExitDisc;
            //LocalPerf = localPerf;
            //AutomaticAggregate = automaticAggregate;
            //Aggregator = aggregator;
            //NetworkLayerRechablity = networkLayerRechablity;
            NlrLength = nlrLength;
            NlrPrefix = nlrPrefix;
        }
        public ushort Type
        {
            get { return _type; }
            set
            {
                _type = value;
                writeType(value, 38);
            }
        }
        // need to check this implementation for withdrawRoutes
        public UInt16 WithdrawRouteLength
        {
            get { return _withdrawnRoutesLength; }
            set
            {
                //_withdrawnRoutesLength = (UInt16)(_ipPrefixLength + _ipPrefix.Length);
                _withdrawnRoutesLength = value;
                writeWithdrawRoutesLength(value, 40);
            }
        }
        public string WithdrawRoutes
        {
            get { return _withdrawnRoutes; }
            set
            {
                _withdrawnRoutes = value;
                writeWithdrawlRoutes(value, 42);
            }
        }
        public ushort IpPrefixLength
        {
            get { return _ipPrefixLength; }
            set
            {
                _ipPrefixLength = value;
                writeIpPrifixLength(value, 51);
            }
        }
        public string IpPrefix
        {
            get { return _ipPrefix; }
            set
            {
                _ipPrefix = value;
                writeIpPrefix(value, 53);
            }
        }
        public ushort TotalPathAttributeLength
        {
            get { return _totalPathAttributeLength; }
            set
            {
                _totalPathAttributeLength = value;
                writeTotalPathAttribute(value, 62);
            }
        }
        
        public UInt32 AttributeLength
        {
            get { return _attributeLength; }
            set
            {
                _attributeLength = value;
                writeAttributeLength(value, 64);
            }
        }
        public string Attribute
        {
            get { return _attribute; }
            set
            {
                _attribute = value;
                writeAttribute(value, 66);
            }
        }
        public UInt32 AttrFlags
        {
            get { return _attrFlags; }
            set
            {
                _attrFlags = value;
                writeAttrFlags(value, 75);
            }
        }
        public ushort TypeCode
        {
            get { return _typeCode; }
            set
            {
                _typeCode = value;
                writeTypeCode(value, 77);
            }
        }
        
        public ushort PathSegmentType
        {
            get { return _pathSegmentType; }
            set
            {
                _pathSegmentType = value;
                writePathSegmentType(value,79);
            }
        }
        public ushort PathSegmentLength
        {
            get { return _pathSegmentLength; }
            set
            {
                _pathSegmentLength = value;
                writePathSegmentLength(value, 81);
            }
        }

        public string PathSegmentValue
        {
            get { return _pathSegmentValue; }
            set
            {
                _pathSegmentValue = value;
                writePathSegmentValue(value, 83);
            }
        }
       
       
        public ushort NlrLength
        {
            get { return _nlrLength; }
            set
            {
                _nlrLength = value;
                writeNlrLength(value, 85);
            }
        }
        public string NlrPrefix
        {
            get { return _nlrPrefix; }
            set
            {
                _nlrPrefix = value;
                writeNlrPrefix(value, 87);
            }
        }
    }
}