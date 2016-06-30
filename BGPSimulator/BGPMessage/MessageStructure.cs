using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGPSimulator.BGPMessage
{
    //Message Header Format
    /*
    0                   1                   2                   3
      0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                                                               |
      +                                                               +
      |                                                               |
      +                                                               +
      |                           Marker                              |
      +                                                               +
      |                                                               |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |          Length               |      Type     |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

      */

public abstract class MessageStructure
{
        //value of bytes and octets are same
        //Marker 16-octet field is included for compatibility; it MUST be set to all ones.
        //Length field MUST always be at least 19 and no greater than 4096 2-octet size
        //This 1-octet unsigned integer indicates the type code of the message. 1 - OPEN 2-UPDATE 3-NOTIFICATION 4-KEEPALIVE

        private byte[] _buffer;
        public ulong marker;

        public MessageStructure(ulong marker, uint length)
        {
            //marker value is 16 octet which consists 32 slots
            //length value is 3 octet which consists 6 slots
            _buffer = new byte[marker];
            for (int i = 0; i < 16; i++)
            {
                writeMarker(marker,i*2);
            }
            writeLength(length,32);
        }

        public void writeMarker(ulong value, int offset)
        {
            byte[] tempBuf = new byte[32];
           
            tempBuf = BitConverter.GetBytes(1);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset,2);
            
            //throw new NotImplementedException();
        }

        public void writeLength(uint value, int offset)
        {
            byte[] tempBuf = new byte[6];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
            //throw new NotImplementedException();
        }
        // assigning message tupe value
        public void writeType(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
            //throw new NotImplementedException();
        }

        // OPENMESSAGE implementation from here
        //+ version + holdTime + bgpIdentifier + optimalParLength
        public void writeVersion(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf,0,_buffer,offset,2);
        }
        public void writeMyAS(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeHoldTime(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeBgpIdentifier(string value, int offset)
        {
            byte[] tempBuf = new byte[value.Length];
            tempBuf = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, value.Length);
        }
        
        public void writeOptimalPerLength(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset,2);
        }
        

        public void writeString (string value, int offset)
        {
            byte[] tempBuf = new byte[value.Length];
            tempBuf = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, value.Length);
        }
        //UPDATEMESSAGE Implementation from here
        //(38 + 2 + 4 + 2 +ipPrefix.Length + 4 + 4 + 2 + 2 + 2 +attribute.Length+ 2 + 2 + pathSegmentValue.Length + 2 + nlrPrefix.Length),19)
        public void writeWithdrawRoutesLength(UInt16 value, int offset)
        {
            byte[] tempBuf = new byte[4];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeWithdrawlRoutes(string value, int offset)
        {
            byte[] tempBuf = new byte[value.Length];
            tempBuf = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, value.Length);
        }
        public void writeIpPrifixLength(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeIpPrefix(string value, int offset)
        {
            byte[] tempBuf = new byte[value.Length];
            tempBuf = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0 , _buffer, offset, value.Length);
        }
        public void writeTotalPathAttribute(ushort value, int offset)
        {
            byte[] tempBuf = new byte[4];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        
        public void writeAttributeLength(UInt32 value, int offset)
        {
            byte[] tempBuf = new byte[4];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeAttribute(string value, int offset)
        {
            byte[] tempBuf = new byte[value.Length];
            tempBuf = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, value.Length);
        }
        public void writeAttrFlags(UInt32 value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeTypeCode(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
       
        public void writePathSegmentType(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writePathSegmentLength(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writePathSegmentValue(string value, int offset)
        {
            byte[] tempBuf = new byte[value.Length];
            tempBuf = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, value.Length);
        }
        
        public void writeNlrLength(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeNlrPrefix(string value, int offset)
        {
            byte[] tempBuf = new byte[value.Length];
            tempBuf = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, value.Length);
        }
        // NOTIFICATION MESSAGE Section
        public void writeErrorCode(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeErrorSubCode(ushort value, int offset)
        {
            byte[] tempBuf = new byte[2];
            tempBuf = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, 2);
        }
        public void writeData(string value, int offset)
        {
            byte[] tempBuf = new byte[value.Length];
            tempBuf = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuf, 0, _buffer, offset, value.Length);
        }
        public MessageStructure(byte [] packet)
        {
            _buffer = packet;
        }
        //complete message is stored in BGPmessage Buffer
        public byte[] BGPmessage { get { return _buffer; } }
    }
}
