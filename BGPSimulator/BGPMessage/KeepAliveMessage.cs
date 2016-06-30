using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGPSimulator.BGPMessage
{
   public class KeepAliveMessage : MessageStructure
    {
        // marker and length values are 16 and 3 octets which contains 32 and 6 slots
        // type is 1 octet consists 2 slots
        private ushort _type;
        public KeepAliveMessage()
            : base ((ushort)(38 + 2),19)
        {
            Type = 4;
        }
        public KeepAliveMessage(byte[] packet) 
            : base(packet)
        {

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
    }
}
