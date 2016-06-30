using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGPSimulator.BGPMessage
{
    public class NotificationMessage : MessageStructure
    {
        /*
        In addition to the fixed-size BGP header, the NOTIFICATION message contains the following fields:
          0                   1                   2                   3
          0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
          | Error code    | Error subcode |   Data (variable)             |
          +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

            Error Code: This 1-octet unsigned integer indicates the type of NOTIFICATION.  The following Error Codes have been defined:
          Error Code = 1 Symbolic Name = Message Header Error, Error Code =  2 Symbolic Name = OPEN Message Error, Error Code =  3 Symbolic Name = UPDATE Message Error,            
          Error Code =  4 Symbolic Name = Hold Timer Expired, Error Code =  5 Symbolic Name = Finite State[init_BGP.connCount] Machine Error, Error Code = 6 Symbolic Name = Cease                           
            
            Error subcode: This 1-octet unsigned integer provides more specific information about the nature of the reported error.  Each Error Code may have one or more
         Error Subcodes associated with it. If no appropriate Error Subcode is defined, then a zer (Unspecific) value is used for the Error Subcode field.

            Message Header Error subcodes: 1 - Connection Not Synchronized. 2 - Bad Message Length. 3 - Bad Message Type.

            OPEN Message Error subcodes: 1 - Unsupported Version Number. 2 - Bad Peer AS. 3 - Bad BGP Identifier. 4 - Unsupported Optional Parameter.
        5 - [Deprecated - see Appendix A]. 6 - Unacceptable Hold Time.

            UPDATE Message Error subcodes: 1 - Malformed Attribute List. 2 - Unrecognized Well-known Attribute. 3 - Missing Well-known Attribute. 
       4 - Attribute Flags Error. 5 - Attribute Length Error. 6 - Invalid ORIGIN Attribute. 7 - [Deprecated - see Appendix A]. 8 - Invalid NEXT_HOP Attribute.
       9 - Optional Attribute Error. 10 - Invalid Network Field. 11 - Malformed AS_PATH.
            
            Data: This variable-length field is used to diagnose the reason for the NOTIFICATION.  The contents of the Data field depend upon the Error Code and Error 
       Subcode.  See Section 6 for more details.

         Note that the length of the Data field can be determined from the message Length field by the formula: Message Length = 21 + Data Length
    The minimum length of the NOTIFICATION message is 21 octets (including message header). i.e. 17 + 4

        */
        private ushort _errorCode;
        private ushort _errorSubCode;
        private string _data;
        private ushort _type;

        public NotificationMessage(ushort errorCode, ushort errorSubCode, string data)
            : base((ushort)(38 + 2 + 2 + 2 + data.Length), 21)
        {
            Type = 3;
            ErrorCode = errorCode;
            ErrorSubCode = errorSubCode;
            Data = data;
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
        public ushort ErrorCode
        {
            get { return _errorCode; }
            set
            {
                _errorCode = value;
                writeErrorCode(value, 40);
            }
        }

        public ushort ErrorSubCode
        {
            get { return _errorSubCode; }
            set
            {
                _errorSubCode = value;
                writeErrorSubCode(value, 42);
            }
        }
        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                writeData(value, 44);
            }
        }
    }
}
