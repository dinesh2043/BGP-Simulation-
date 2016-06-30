using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
/** 
        Events for the BGP FSM

        8.1.1 Optional Events Linked to Optional Session Attributes (The Inputs to the BGP FSM are events.  Events can either be mandatory or optional)

        The linkage between FSM functionality, events, and the optional session attributes are described below.

            Group 1: Automatic Administrative Events (Start/Stop)
                Optional Session Attributes: AllowAutomaticStart, AllowAutomaticStop, DampPeerOscillations(all three bool value),IdleHoldTime, IdleHoldTimer(both value is time in sec)
            Group 2: Unconfigured Peers (not implementing right now complicated with security reasions)
                Optional Session Attributes: AcceptConnectionsUnconfiguredPeers (bool value)
            Group 3: TCP processing
                Optional Session Attributes: PassiveTcpEstablishment,TrackTcpState (both bool values)
            Group 4:  BGP Message Processing
                Optional Session Attributes: DelayOpen (bool), DelayOpenTime(time in sec), DelayOpenTimer(time in sec), SendNOTIFICATIONwithoutOPEN(bool), CollisionDetectEstablishedState(bool)
    **/
namespace BGPSimulator.FSM
{
    
    public class StateMachine 
    {
        /**
        1) Description of Events for the State[init_BGP.connCount] machine(Section 8.1)
    Session attributes required(mandatory) for each connection are:
        1) State[init_BGP.connCount]    2) ConnectRetryCounter  3) ConnectRetryTimer    4) ConnectRetryTime     5) HoldTimer    6) HoldTime     7) KeepaliveTimer   8) KeepaliveTime

    The state session attribute indicates the current state of the BGP FSM.

    Each timer has a "timer" and a "time" (the initial value).
    
    Two groups of the attributes which relate to timers are: (Which I am not going to implement right now.)
      group 1: DelayOpen, DelayOpenTime, DelayOpenTimer
      group 2: DampPeerOscillations, IdleHoldTime, IdleHoldTimer
    **/
        //public String State[init_BGP.connCount];
        public int connectRetryCounter;
        public Timer connectRetryTimer;
        public DateTime connectRetryTime;
        public Timer holdTimer;
        public DateTime holdTime;
        public Timer keepaliveTimer;
        public DateTime keepaliveTime;
        
        /**
          
    8.1.2.  Administrative Events
    An administrative event is an event in which the operator interface and BGP Policy engine signal the BGP-finite state machine to start or
    stop the BGP state machine.
        Event 1: ManualStart
        Event 2: ManualStop
        Event 3: AutomaticStart (Among all of the above method I am just implementing this part)
            Definition: Local system automatically starts the BGP connection.
            Status:     Optional, depending on local system
                Optional Attribute Status:     
                     1) The AllowAutomaticStart attribute SHOULD be set to TRUE if this event occurs.
                     2) If the PassiveTcpEstablishment optional session attribute is supported, it SHOULD be set to FALSE.
                     3) If the DampPeerOscillations is supported, it SHOULD be set to FALSE when this event occurs.
        Event 5: AutomaticStart_with_PassiveTcpEstablishment
         Definition: Local system automatically starts the BGP connection with the PassiveTcpEstablishment enabled.  The PassiveTcpEstablishment optional
                     attribute indicates that the peer will listen prior to establishing a connection.
         Status:     Optional, depending on local system
         Optional Attribute Status:     
                    1) The AllowAutomaticStart attribute SHOULD be set to TRUE.
                     2) The PassiveTcpEstablishment attribute SHOULD be set to TRUE.
                     3) If the DampPeerOscillations attribute is supported, the DampPeerOscillations SHOULD be set to FALSE.
         Event 6: AutomaticStart_with_DampPeerOscillations (most likely I will not implement this part also)
         Definition: Local system automatically starts the BGP peer connection with peer oscillation damping enabled.The exact method of damping persistent peer
                     oscillations is determined by the implementation and is outside the scope of this document.
         Status:     Optional, depending on local system.
         Optional Attribute Status:     
                     1) The AllowAutomaticStart attribute SHOULD be set to TRUE.
                     2) The DampPeerOscillations attribute SHOULD be set to TRUE.
                     3) The PassiveTcpEstablishment attribute SHOULD be set to FALSE.
         Event 8: AutomaticStop
         Definition: Local system automatically stops the BGP connection. An example of an automatic stop event is exceeding the number of prefixes for a given peer and 
                     the local system automatically disconnecting the peer.
         Status:     Optional, depending on local system
         Optional Attribute Status:    1) The AllowAutomaticStop attribute SHOULD be TRUE.
        **/
        public event EventHandler OnAutomaticStartEvent;
        //public event EventHandler AutomaticStart_with_PassiveTcpEstablishment;
        public event EventHandler OnAutomaticStopEvent;
        
        bool automaticStart;
        public bool AutomaticStart
        {
            get { return automaticStart; }
            set
            {
                automaticStart = value;
                OnAutomaticStartEvent(this, new EventArgs());
            }
        }
        bool automaticStop;
       public bool AutomaticStop
        {
            get { return automaticStop; }
            set
            {
                automaticStop = value;
                OnAutomaticStopEvent(this, new EventArgs());

             }
             
        }
        /**
        string state;
        public string State[init_BGP.connCount]
        {
            get { return state; }
            set
            {
                state = value;
            }
        }
            **/
        /**
         8.1.3.  Timer Events
      Event 9: ConnectRetryTimer_Expires
         Definition: An event generated when the ConnectRetryTimer expires.
         Status:     Mandatory
      Event 10: HoldTimer_Expires
         Definition: An event generated when the HoldTimer expires.
         Status:     Mandatory
      Event 11: KeepaliveTimer_Expires
         Definition: An event generated when the KeepaliveTimer expires.
         Status:     Mandatory
        **/
        public event EventHandler ConnectRetryTimer_Expires;
        public event EventHandler HoldTimer_Expires;
        public event EventHandler KeepaliveTimer_Expires;

        public Timer ConnectRetryTimer
        {
            get { return connectRetryTimer; }
            set
            {
                connectRetryTimer = value;
                ConnectRetryTimer_Expires(this, new EventArgs());
            }
        }
        
        public Timer HoldTimer
        {
            get { return holdTimer; }
            set
            {
                holdTimer = value;
                HoldTimer_Expires(this, new EventArgs());

            }
        }
        public Timer KeepaliveTimer
        {
            get { return keepaliveTimer; }
            set
            {
                keepaliveTimer = value;
                KeepaliveTimer_Expires(this, new EventArgs());

            }

        }
        /**
         8.1.4.  TCP Connection-Based Events
      Event 14: TcpConnection_Valid (Optional)
      Event 15: Tcp_CR_Invalid (Optional)
      Event 16: Tcp_CR_Acked
         Definition: Event indicating the local system's request to establish a TCP connection to the remote peer. The local system's TCP connection sent a TCP SYN,
                     received a TCP SYN/ACK message, and sent a TCP ACK.
         Status:     Mandatory
      Event 17: TcpConnectionConfirmed
         Definition: Event indicating that the local system has received a confirmation that the TCP connection has been established by the remote site.
                     The remote peer's TCP engine sent a TCP SYN.  The local peer's TCP engine sent a SYN, ACK message and now has received a final ACK.
         Status:     Mandatory
      Event 18: TcpConnectionFails
         Definition: Event indicating that the local system has received a TCP connection failure notice. The remote BGP peer's TCP machine could have sent a FIN.  The local peer would respond with a FIN-ACK.
                     Another possibility is that the local peer indicated a timeout in the TCP connection and downed the connection.
         Status:     Mandatory
        **/
        public event EventHandler Tcp_Acked_Event;
        public event EventHandler TcpConnectionConformed_Event;
        public event EventHandler TcpConnectionFails_Event;

        public bool tcpConnectionAcknowladged;
        public bool TCPConnectionAcknowladged
        {
            get { return tcpConnectionAcknowladged; }
            set
            {
                tcpConnectionAcknowladged = value;
                Tcp_Acked_Event(this, new EventArgs());
            }
        }
        
        public bool tcpConnectionConformedValue;
        public bool TcpConnectionConformedValue
        {
            get { return tcpConnectionConformedValue; }
            set
            {
                tcpConnectionConformedValue = value;
                TcpConnectionConformed_Event(this, new EventArgs());
            }
        }
        
        public bool tcpConnectionFails;
        public bool TcpConnectionFails
        {
            get { return tcpConnectionFails; }
            set
            {
                tcpConnectionFails = value;
                TcpConnectionFails_Event(this, new EventArgs());
            }
        }
        /**
        8.1.5.  BGP Message-Based Events
       Event 19: BGPOpen
         Definition: An event is generated when a valid OPEN message has been received.
         Status:     Mandatory
         Optional Attribute Status:     
                     1) The DelayOpen optional attribute SHOULD be set to FALSE.
                     2) The DelayOpenTimer SHOULD not be running.
       Event 21: BGPHeaderErr
         Definition: An event is generated when a received BGP message header is not valid.
         Status:     Mandatory
      Event 22: BGPOpenMsgErr
         Definition: An event is generated when an OPEN message has been received with errors.
         Status:     Mandatory 
      Event 24: NotifMsgVerErr
         Definition: An event is generated when a NOTIFICATION message with "version error" is received.
         Status:     Mandatory
      Event 25: NotifMsg
         Definition: An event is generated when a NOTIFICATION message is received and the error code is anything but "version error".
         Status:     Mandatory
      Event 26: KeepAliveMsg
         Definition: An event is generated when a KEEPALIVE message is received.
         Status:     Mandatory 
      Event 27: UpdateMsg
         Definition: An event is generated when a valid UPDATE message is received.
         Status:     Mandatory
      Event 28: UpdateMsgErr
         Definition: An event is generated when an invalid UPDATE message is received.
         Status:     Mandatory
        **/
        public event EventHandler BGPOpenMsg_Event;
        public event EventHandler BGPOpenMsgRecived_Event;
        public event EventHandler BGPHeaderErr_Event;
        public event EventHandler BGPOpenMsgErr_Event;
        public event EventHandler BGPNotifyMsgErr_Event;
        public event EventHandler BGPNotifyMsg_Event;
        public event EventHandler BGPKeepAliveMsg_Event;
        public event EventHandler BGPUpdateMsg_Event;
        public event EventHandler BGPUpdateMsgErr_Event;
       
        public bool bgpOpenMsg;
        public bool BGPOpenMsg
        {
            get { return bgpOpenMsg; }
            set
            {
                bgpOpenMsg = value;
                BGPOpenMsg_Event(this, new EventArgs ());
            }
        }
        public bool bgpOpenMsgRecived;
        public bool BGPOpenMsgRecive
        {
            get { return bgpOpenMsgRecived; }
            set
            {
                bgpOpenMsgRecived = value;
                BGPOpenMsgRecived_Event(this, new EventArgs());
            }
        }
        public bool bgpHederErr;
        public bool BGPHederErr
        {
            get { return bgpHederErr; }
            set
            {
                bgpHederErr = value;
                BGPHeaderErr_Event(this , new EventArgs ());
            }
        }
        public bool bgpOpenMessageErr;
        public bool BGPOpenMessageErr
        {
            get { return bgpOpenMessageErr; }
            set
            {
                bgpOpenMessageErr = value;
                BGPOpenMsgErr_Event(this, new EventArgs());
            }
        }
        
        public bool bgpNotifyMsgErr;
        public bool BGPNotifyMsgErr
        {
            get { return bgpNotifyMsgErr; }
            set
            {
                bgpNotifyMsgErr = value;
                BGPNotifyMsgErr_Event(this, new EventArgs());
            }
        }
        public bool bgpNotifyMsg;
        public bool BGPNotifyMsg
        {
            get { return bgpNotifyMsg; }
            set
            {
                bgpNotifyMsg = value;
                BGPNotifyMsg_Event(this, new EventArgs());
            }
        }
        public bool bgpKeepAliveMsg;
        public bool BGPKeepAliveMsg
        {
            get { return bgpKeepAliveMsg; }
            set
            {
                bgpKeepAliveMsg = value;
                BGPKeepAliveMsg_Event(this, new EventArgs());
            }
        }
        public bool bgpUpdateMsg;
        public bool BGPUpdateMsg
        {
            get { return bgpUpdateMsg; }
            set
            {
                bgpUpdateMsg = value;
                BGPUpdateMsg_Event(this, new EventArgs());
            }
        }
        public bool bgpUpdateMsgErr;
        public bool BGPUpdateMsgErr
        {
            get { return bgpUpdateMsgErr; }
            set
            {
                bgpUpdateMsgErr = value;
                BGPUpdateMsgErr_Event(this, new EventArgs());
            }
        }
    }
}
