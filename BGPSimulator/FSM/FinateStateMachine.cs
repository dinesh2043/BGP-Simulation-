using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BGPSimulator.BGPMessage;
using BGPSimulator.BGP;
using System.Net;
/**
    8.2.  Description of FSM
        8.2.1.  FSM Definition 
            BGP MUST maintain a separate FSM for each configured peer.  Each BGP peer paired in a potential connection will attempt to connect to the
        other, unless configured to remain in the idle state, or configured to remain passive.  For the purpose of this discussion, the active or
        connecting side of the TCP connection (the side of a TCP connection sending the first TCP SYN packet) is called outgoing.  The passive or
        listening side (the sender of the first SYN/ACK) is called an incoming connection.  (See Section 8.2.1.1 for information on the terms active and passive used below.)
            A BGP implementation MUST connect to and listen on TCP port 179 for incoming connections in addition to trying to connect to peers.  For
        each incoming connection, a state machine MUST be instantiated. There exists a period in which the identity of the peer on the other
        end of an incoming connection is known, but the BGP identifier is not known.  During this time, both an incoming and outgoing connection
        may exist for the same configured peering.  This is referred to as a connection collision (see Section 6.8).
            A BGP implementation will have, at most, one FSM for each configured peering, plus one FSM for each incoming TCP connection for which the
        peer has not yet been identified.  Each FSM corresponds to exactly one TCP connection.
             There may be more than one connection between a pair of peers if the connections are configured to use a different pair of IP addresses.
        This is referred to as multiple "configured peerings" to the same peer.
    8.2.1.1.  Terms "active" and "passive"
             The words active and passive have slightly different meanings when applied to a TCP connection or a peer.  There is only one active side and one
        passive side to any one TCP connection, per the definition above and the state machine below.  When a BGP speaker is configured as active,
        it may end up on either the active or passive side of the connection that eventually gets established.  Once the TCP connection is
        completed, it doesn't matter which end was active and which was passive.  The only difference is in which side of the TCP connection has port number 179.
   8.2.1.2.  FSM and Collision Detection
            There is one FSM per BGP connection.  When the connection collision occurs prior to determining what peer a connection is associated
        with, there may be two connections for one peer.  After the connection collision is resolved (see Section 6.8), the FSM for the
        connection that is closed SHOULD be disposed.
  8.2.1.3.  FSM and Optional Session Attributes
            Optional Session Attributes specify either attributes that act as flags (TRUE or FALSE) or optional timers.  For optional attributes
        that act as flags, if the optional session attribute can be set to TRUE on the system, the corresponding BGP FSM actions must be
        supported.  For example, if the following options can be set in a BGP implementation: AutoStart and PassiveTcpEstablishment, then Events 3,
        4 and 5 must be supported.  If an Optional Session attribute cannot be set to TRUE, the events supporting that set of options do not have to be supported.
     Each of the optional timers (DelayOpenTimer and IdleHoldTimer) has a group of attributes that are:
            - flag indicating support, - Time set in Timer - Timer.
     The two optional timers show this format:
            DelayOpenTimer: DelayOpen, DelayOpenTime, DelayOpenTimer
            IdleHoldTimer:  DampPeerOscillations, IdleHoldTime, IdleHoldTimer
   8.2.1.4.  FSM Event Numbers
            The Event numbers (1-28) utilized in this state machine description aid in specifying the behavior of the BGP state machine.
        Implementations MAY use these numbers to provide network management information.  The exact form of an FSM or the FSM events are specific to each implementation.
   8.2.1.5.  FSM Actions that are Implementation Dependent
        At certain points, the BGP FSM specifies that BGP initialization will occur or that BGP resources will be deleted.  The initialization of
    the BGP FSM and the associated resources depend on the policy portion of the BGP implementation.  The details of these actions are outside the scope of the FSM document.
 ********* Actual implementation Part************
    Idle State[init_BGP.connCount]:

    Connect State[init_BGP.connCount]:
  
   Active State[init_BGP.connCount]:
            
            If the DelayOpen attribute is set to FALSE, the local system:
                    - sets the ConnectRetryTimer to zero,
                    - completes the BGP initialization,
                    - sends the OPEN message to its peer,
                    - sets its HoldTimer to a large value, and
                    - changes its state to OpenSent.
            
            
    OpenSent:
            
      OpenConfirm State[init_BGP.connCount]:
            In this state, BGP waits for a KEEPALIVE or NOTIFICATION message.
            Any start event (Events 1, 3-7) is ignored in the OpenConfirm state.
           
            If a TCP connection is attempted with an invalid port (Event 15), the local system will ignore the second connection attempt.

            If the local system receives a valid OPEN message (BGPOpen (Event 19)), the collision detect function is processed per Section 6.8.
            If this connection is to be dropped due to connection collision, the local system:
                    - sends a NOTIFICATION with a Cease,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection (send TCP FIN),
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
           
            

    Established State[init_BGP.connCount]:
           
            Each time the local system sends a KEEPALIVE or UPDATE message, it restarts its KeepaliveTimer, unless the negotiated HoldTime value is zero.
            A TcpConnection_Valid (Event 14), received for a valid port, will cause the second connection to be tracked.
            In response to an indication that the TCP connection is successfully established (Event 16 or Event 17), the second connection SHALL be tracked 
            until it sends an OPEN message.
            
**/

namespace BGPSimulator.FSM
{
    /**
     8.2.2.  Finite State[init_BGP.connCount] Machine
        Idle state:
            Initially, the BGP peer FSM is in the Idle state.Hereafter, the BGP peer FSM will be shortened to BGP FSM.In this state, BGP FSM refuses all incoming BGP connections for
        this peer.No resources are allocated to the peer.
      
        
        In response to AutomaticStart_with_PassiveTcpEstablishment event (Event 5), the local system:
                    - initializes all BGP resources,
                    - sets the ConnectRetryCounter to zero,
                    - starts the ConnectRetryTimer with the initial value,
                    - listens for a connection that may be initiated by the remote peer, and
                    - changes its state to Active.
       
            
       Connect State[init_BGP.connCount]:
            
            If the DelayOpen attribute is set to FALSE, the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - completes BGP initialization
                    - sends an OPEN message to its peer,
                    - sets the HoldTimer to a large value, and
                    - changes its state to OpenSent.
           
    **/
    public class FinateStateMachine : StateMachine 
    {
        bool autoStartEvent;
        bool connectRetryExpires;
        bool holdTimeExpires;
        bool keepAliveExpires;
        bool tcpConnectionSucceeds;
        bool tcpConnectionFail;
        bool bgpHeaderMsgErr;
        bool bgpOpenMsgErr;
        bool bgpNotifyMsgError;
        bool bgpAutoStop;
        bool bgpOpenMessage;
        //public bool bgpOpenMsgRecived;
        bool bgpNotifyMessage;
        bool bgpKeepAliveMessage;
        bool bgpUpdateMessage;
        bool bgpUpdateMsgError;
        

 

        InitilizeBGPListnerSpeaker init_BGP = new InitilizeBGPListnerSpeaker();
       
        //public string State[init_BGP.connCount];
        //StateMachine SM = new StateMachine();
        //BGPTimers bgpTimers = new BGPTimers();
        /**
        Idle State[init_BGP.connCount]
           In response an AutomaticStart event (Event 3), the local system:
                   - initializes all BGP resources for the peer connection,
                   - sets ConnectRetryCounter to zero,
                   - starts the ConnectRetryTimer with the initial value,
                   - initiates a TCP connection to the other BGP peer,
                   - listens for a connection that may be initiated by the remote BGP peer, and
                   - changes its state to Connect.
            AutomaticStop (Event 8) event are ignored in the Idle state.
             The exact value of the ConnectRetryTimer is a local matter, but it SHOULD be sufficiently large to allow TCP initialization.
             Any other event (Events 9-12, 15-28) received in the Idle state does not cause change in the state of the local system.
      
           **/
        public void IdleState()
        {
            //Initially, the BGP peer FSM is in the Idle state.Hereafter, the BGP peer FSM will be shortened to BGP FSM.In this state, BGP FSM refuses all incoming BGP 
            //connections for this peer.No resources are allocated to the peer.

            //State[init_BGP.connCount] = "Idle";
            //State[init_BGP.connCount](State[init_BGP.connCount]);
            if (autoStartEvent == true)
            {
                GlobalVariables.listnerConnectionState = "Idle";
                GlobalVariables.speakerConnectionState = "Idle";
                //State[init_BGP.connCount] = "Idle";
                //Console.WriteLine("Auto Start Event fired Idle State[init_BGP.connCount] do Stuff Here!!");
                //initalize all BGP resources for the peer connection
                init_BGP.StartListner();
                
                connectRetryCounter = 0;
                //sets ConnectRetryCounter to zero
                //SM.connectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //listens for a connection that may be initiad by the remote  BGP peer and changes its state to connect
                init_BGP.StartListning();
                //initiates a TCP connection to the other BGP peer

                init_BGP.StartSpeaker();
                init_BGP.SpeakerConnection_Init();
                
                //init_BGP.SendMessage();
                
                //RouterState(routerState);
                autoStartEvent = false;
                //Console.WriteLine("Connection Retry Counter = " + SM.connectRetryCounter + "Connection State[init_BGP.connCount]: " + SM.State[init_BGP.connCount]);
                
               
            }
        }
        /**
        Connect State:
            In this state, BGP FSM is waiting for the TCP connection to be completed.
            The start events (Events 1, 3-7) are ignored in the Connect state.
            In response to the ConnectRetryTimer_Expires event (Event 9), the local system:
                    - drops the TCP connection,
                    - restarts the ConnectRetryTimer,
                    - stops the DelayOpenTimer and resets the timer to zero,
                    - initiates a TCP connection to the other BGP peer,
                    - continues to listen for a connection that may be initiated by the remote BGP peer, and
                    - stays in the Connect state.
            If the TCP connection succeeds (Event 16 or Event 17), the local system checks the DelayOpen attribute prior to processing.  If the
            DelayOpen attribute is set to TRUE, the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - sets the DelayOpenTimer to the initial value, and
                    - stays in the Connect state.
            A HoldTimer value of 4 minutes is suggested.
            If the TCP connection fails (Event 18), the local system checks the DelayOpenTimer.  If the DelayOpenTimer is running, the local system:
                    - restarts the ConnectRetryTimer with the initial value,
                    - stops the DelayOpenTimer and resets its value to zero,
                    - continues to listen for a connection that may be initiated by the remote BGP peer, and
                    - changes its state to Active.
        **/
        public void ConnectState()
        {
            //In this state, BGP FSM is waiting for the TCP connection to be completed.
            //If the BGP FSM receives a TcpConnection_Valid event (Event 14),the TCP connection is processed, and the connection remains in theConnect state.

            if (connectRetryExpires == true)
            {
                Console.WriteLine("Connection Retry Expired Connect State do stuff here !!");

                //drops the TCP connection
                //******ConnectRetryTimer is auto reseted in the implementation*******
                // Create a timer with a two 120000 interval.
                ConnectionRetryTimer_Reset();
                //initiates a TCP connection to the other BGP peer

                //continues to listen for a connection that may be initiated by the remote BGP peer, and
                //stays in the Connect state.
                GlobalVariables.listnerConnectionState = "Connect";

                connectRetryExpires = false;
            }
            //If the TCP connection succeeds
            if(tcpConnectionSucceeds == true)
            {
                //Console.WriteLine("TCP Connection Established Connect State[init_BGP.connCount] do stuff here !!");
                //stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                // Create a timer with a two 120000 interval.
                //SM.connectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //stays in the Connect state.
                GlobalVariables.listnerConnectionState = "Connect";
                tcpConnectionSucceeds = false;

                // If the value of the autonomous system field is the same as the local Autonomous System number, set the connection status to an
                //internal connection; otherwise it will be "external".

                //Console.WriteLine("Speaker AS "+GlobalVariables.speaker_AS[GlobalVariables.speakerIpAddress]);
                //Console.WriteLine("AS check listner  " + GlobalVariables.listner_AS[GlobalVariables.listnerIpAddress]);
                
                if (GlobalVariables.speaker_AS[GlobalVariables.speakerIpAddress] == GlobalVariables.listner_AS[GlobalVariables.listnerIpAddress])
                {
                    GlobalVariables.connectionStatus = "Internal Connection";
                    Console.WriteLine("!! With :" + GlobalVariables.connectionStatus);
                }
                else
                {
                    GlobalVariables.connectionStatus = "External Connection";
                    Console.WriteLine("!! With :" + GlobalVariables.connectionStatus);
                }
                
                
            }
            if (tcpConnectionFail == true)
            {
                Console.WriteLine("TCP Connection Failed Connect State[init_BGP.connCount] Stuff here !!");
                // Create a timer with a two 120000 interval.
                //SM.connectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //continues to listen for a connection that may be initiated by the remote BGP peer, and
                //Changes its state to Active
                GlobalVariables.speakerConnectionState = "Active";
                tcpConnectionFail = false;
            }
            /**
            If BGP message header checking(Event 21) or OPEN message checking detects an error(Event 22) (see Section 6.2), the local system:
                    - (optionally) If the SendNOTIFICATIONwithoutOPEN attribute is set to TRUE, then the local system first sends a NOTIFICATION message with the appropriate error code, and then
                    - stops the ConnectRetryTimer(if running) and sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            If a NOTIFICATION message is received with a version error(Event 24), the local system checks the DelayOpenTimer.If the DelayOpenTimer is running, the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - stops and resets the DelayOpenTimer(sets to zero),
                    - releases all BGP resources,
                    - drops the TCP connection, and
                    - changes its state to Idle.

            In response to any other events(Events 8, 10-11, 13, 19, 23, 25-28), the local system:
                    - if the ConnectRetryTimer is running, stops and resets the ConnectRetryTimer(sets to zero),
                    - if the DelayOpenTimer is running, stops and resets the DelayOpenTimer(sets to zero),
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - performs peer oscillation damping if the DampPeerOscillations attribute is set to True, and
                    - changes its state to Idle.
            **/
            if (bgpHeaderMsgErr == true || bgpOpenMsgErr == true)
            {
                Console.WriteLine("BGP Header Message or Open Message has error Connect State do stuff here!!");
                //stops the ConnectRetryTimer(if running) and sets the ConnectRetryTimer to zero,
                //Create a timer with a two 120000 interval.
                //SM.connectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP connection
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                Console.WriteLine("Connection Retry Counter = "+connectRetryCounter+"Connection State: "+ GlobalVariables.listnerConnectionState);
                bgpHeaderMsgErr = false;
                bgpOpenMsgErr = false;
            }
            if(bgpNotifyMsgError == true)
            {
                Console.WriteLine("BGP Notify Message error Connect State do stuff here!!");
                //stops the ConnectRetryTimer(if running) and sets the ConnectRetryTimer to zero,
                // Create a timer with 120000 interval.
                //SM.connectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //releses all BGP resources
                //drops the TCP connection
                GlobalVariables.listnerConnectionState = "Idle";
                bgpNotifyMsgError = false;
            }
            if (bgpAutoStop ==true || holdTimeExpires==true || keepAliveExpires ==true || bgpOpenMessage ==true || bgpNotifyMessage ==true || bgpKeepAliveMessage==true ||
                bgpUpdateMessage == true || bgpUpdateMsgError == true)
            {
                //Console.WriteLine("BGP Events in Connect State[init_BGP.connCount] 8, 10-11, 19 and 25-28 do stuff here!!");
                // Create a timer with a two 120000 interval.
                //SM.connectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                //relese all BGP resources
                //drops TCP connection
                connectRetryCounter++;
                //GlobalVariables.listnerConnectionState = "Idle";
                connectRetryExpires = false;
                bgpAutoStop = false;
                holdTimeExpires = false;
                keepAliveExpires = false;
                bgpOpenMessage = false;
                bgpNotifyMessage = false;
                bgpKeepAliveMessage = false;
                bgpUpdateMessage = false;
                bgpUpdateMsgError = false;
            }


        }
        /**
        Active State[init_BGP.connCount]:
        In this state, BGP FSM is trying to acquire a peer by listening for, and accepting, a TCP connection.
            The start events (Events 1, 3-7) are ignored in the Active state.
            In response to a ConnectRetryTimer_Expires event (Event 9), the local system:
                    - restarts the ConnectRetryTimer (with initial value),
                    - initiates a TCP connection to the other BGP peer,
                    - continues to listen for a TCP connection that may be initiated by a remote BGP peer, and
                    - changes its state to Connect.
            A HoldTimer value of 4 minutes is also suggested for this state transition.
            If the local system receives a TcpConnection_Valid event (Event 14), the local system processes the TCP connection flags and stays in the Active state.
            If the local system receives a Tcp_CR_Invalid event (Event 15), the local system rejects the TCP connection and stays in the Active State[init_BGP.connCount].
            In response to the success of a TCP connection (Event 16 or Event 17), the local system checks the DelayOpen optional attribute prior to processing.
            If the DelayOpen attribute is set to TRUE, the local system:
                    - stops the ConnectRetryTimer and sets the ConnectRetryTimer to zero,
                    - sets the DelayOpenTimer to the initial value (DelayOpenTime), and
                    - stays in the Active state.
            A HoldTimer value of 4 minutes is suggested as a "large value" for the HoldTimer.
            If the local system receives a TcpConnectionFails event (Event 18), the local system:
                    - restarts the ConnectRetryTimer (with the initial value),
                    - stops and clears the DelayOpenTimer (sets the value to zero),
                    - releases all BGP resource,
                    - increments the ConnectRetryCounter by 1,
                    - optionally performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
           
        **/
        public void ActiveState()
        {
            //In this state, BGP FSM is trying to acquire a peer by listening for, and accepting, a TCP connection.
            if (connectRetryExpires == true)
            {
                Console.WriteLine("Connection Retry Expried in Active State Do Stuff here!!");
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //initiates a TCP connection to the other BGP peer
                //continues to listen for a TCP connection that may be initiated by a remote BGP peer, and
                GlobalVariables.listnerConnectionState = "Connect";
                connectRetryExpires = false;
            }
            if(tcpConnectionSucceeds == true)
            {
                //Console.WriteLine("TCP Connection Succeeds in Active State Stuff here!!");
                //stops the ConnectRetryTimer and sets the ConnectRetryTimer to zero,
                //**** must implement reset ConnectRetryTimer here in final Solution******
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                GlobalVariables.speakerConnectionState = "Active";
                tcpConnectionSucceeds = false;
            }
            
            if (tcpConnectionFail == true)
            {
                Console.WriteLine("TCP Connection Fails in Active State[init_BGP.connCount] Stuff here!!");
                //**** must implement reset ConnectRetryTimer here in final Solution******
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //relese all BGP resource
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                Console.WriteLine("Connection Retry Counter = " + connectRetryCounter + "Connection State: " + GlobalVariables.listnerConnectionState);
                tcpConnectionFail = false;
            }
            /**
            If an OPEN message is received and the DelayOpenTimer is running (Event 20), the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - stops and clears the DelayOpenTimer (sets to zero),
                    - completes the BGP initialization,
                    - sends an OPEN message,
                    - sends a KEEPALIVE message,
                    - if the HoldTimer value is non-zero,
                    - starts the KeepaliveTimer to initial value,
                    - resets the HoldTimer to the negotiated value,
            else if the HoldTimer is zero
                    - resets the KeepaliveTimer (set to zero),
                    - resets the HoldTimer to zero, and
                    - changes its state to OpenConfirm.
            **/
            // bgpOpenMsgRecived is not implemented yet

            if (bgpOpenMsgRecived == true)
            {
                //Console.WriteLine("BGP Open Message Recived in Active Stuff here!!");
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //completes the BGP initialization
                //sends an OPEN message
                BGPListner listner = new BGPListner();
                listner.SendingOpenMsg_Speaker();

                

                //sends a KeepAlive message
                
                
                listner.SendingKeepAliveMsg_Speaker();
                
                if (HoldTimer != null)
                {
                    //SM.KeepaliveTimer = new System.Timers.Timer(80000);
                    KeepAliveTimer_Reset();
                    //SM.HoldTimer = new System.Timers.Timer(240000);
                    HoldTimer_Reset();

                }else if (HoldTimer == null)
                {
                    //SM.KeepaliveTimer = new System.Timers.Timer(80000);
                    KeepAliveTimer_Reset();
                    //SM.HoldTimer = new System.Timers.Timer(240000);
                    HoldTimer_Reset();
                    
                }
                GlobalVariables.listnerConnectionState = "OpenSent";
                GlobalVariables.speakerConnectionState = "OpenSent";
            }
            /**
            If the value of the autonomous system field is the same as the local Autonomous System number, set the connection status to an internal connection; otherwise it will be external.
            If BGP message header checking (Event 21) or OPEN message checking detects an error (Event 22) (see Section 6.2), the local system:
                    - (optionally) sends a NOTIFICATION message with the appropriate error code if the SendNOTIFICATIONwithoutOPEN attribute is set to TRUE,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            If a NOTIFICATION message is received with a version error (Event 24), the local system checks the DelayOpenTimer.  If the DelayOpenTimer is running, the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - stops and resets the DelayOpenTimer (sets to zero),
                    - releases all BGP resources,
                    - drops the TCP connection, and
                    - changes its state to Idle.
           In response to any other event (Events 8, 10-11, 13, 19, 23, 25-28), the local system:
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by one,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            **/
            //***This part is left for further implementation After implemention Autonomous System*****
            //If the value of the autonomous system field is the same as the local Autonomous System number, set the connection status to an internal connection; otherwise it will be external.
            if(bgpHeaderMsgErr == true || bgpOpenMsgErr == true)
            {
                Console.WriteLine("BGP heder message error or open message Active State error Stuff here!!");
                //**** must implement reset ConnectRetryTimer here in final Solution******
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //relese all BGP resources
                //drops the TCP connection
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                Console.WriteLine("Connection Retry Counter = " + connectRetryCounter + "Connection State: " + GlobalVariables.listnerConnectionState);
                bgpHeaderMsgErr = false;
                bgpOpenMsgErr = false;
            }
            if (bgpAutoStop == true || holdTimeExpires == true || keepAliveExpires == true || bgpOpenMessage == true || bgpNotifyMessage == true || bgpKeepAliveMessage == true ||
                bgpUpdateMessage == true || bgpUpdateMsgError == true)
            {
                //Console.WriteLine("BGP Events in Active State[init_BGP.connCount] 8, 10-11, 19 and 25-28 do stuff here!!");
                // Create a timer with a two 120000 interval.
                //SM.connectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                //relese all BGP resources
                //drops TCP connection
                connectRetryCounter++;
                //GlobalVariables.listnerConnectionState = "Idle";
                connectRetryExpires = false;
                bgpAutoStop = false;
                holdTimeExpires = false;
                keepAliveExpires = false;
                bgpOpenMessage = false;
                bgpNotifyMessage = false;
                bgpKeepAliveMessage = false;
                bgpUpdateMessage = false;
                bgpUpdateMsgError = false;
            }
        }
        /**
         OpenSent:
            In this state, BGP FSM waits for an OPEN message from its peer.
            The start events (Events 1, 3-7) are ignored in the OpenSent state.
            If an AutomaticStop event (Event 8) is issued in the OpenSent state, the local system:
                    - sends the NOTIFICATION with a Cease,
                    - sets the ConnectRetryTimer to zero,
                    - releases all the BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            If the HoldTimer_Expires (Event 10), the local system:
                    - sends a NOTIFICATION message with the error code Hold Timer Expired,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            If a TcpConnectionFails event (Event 18) is received, the local system:
                    - closes the BGP connection,
                    - restarts the ConnectRetryTimer,
                    - continues to listen for a connection that may be initiated by the remote BGP peer, and
                    - changes its state to Active.
            
        **/
        public void OpenSent()
        {
            //In this state, BGP FSM waits for an OPEN message from its peer.

            if (bgpAutoStop == true)
            {
                //sends the Notification with Cease
                ConnectionRetryTimer_Reset();
                //relese all the BGP resources,
                //drops the TCP connection
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                bgpAutoStop = false;
            }
            if(holdTimeExpires == true)
            {
                //sends a NOTIFICATION message with the error code Hold Timer Expired,
                ConnectionRetryTimer_Reset();
                //relese all BGP resources,
                //drops the TCP connection,
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                holdTimeExpires = false;
            }
            if(tcpConnectionFail == true)
            {
                //closes the BGP connection,
                ConnectionRetryTimer_Reset();
                //continues to listen for a connection that may be initiated by the remote BGP peer, and
                GlobalVariables.listnerConnectionState = "Active";
                tcpConnectionFail = false;
            }
            /**
            When an OPEN message is received, all fields are checked for correctness.  If there are no errors in the OPEN message (Event 19), the local system:
                    - resets the DelayOpenTimer to zero,
                    - sets the BGP ConnectRetryTimer to zero,
                    - sends a KEEPALIVE message, and
                    - sets a KeepaliveTimer (via the text below)
                    - sets the HoldTimer according to the negotiated value (see Section 4.2),
                    - changes its state to OpenConfirm.
            If the negotiated hold time value is zero, then the HoldTimer and KeepaliveTimer are not started.  
            If the value of the Autonomous System field is the same as the local Autonomous System number,then the connection is an "internal" 
    connection; otherwise, it is an "external" connection.  (This will impact UPDATE processing as described below.)
            If the BGP message header checking (Event 21) or OPEN message checking detects an error (Event 22)(see Section 6.2), the local system:
                    - sends a NOTIFICATION message with the appropriate error code,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is TRUE, and
                    - changes its state to Idle.
            
            **/
            //this part is not implemented yet
            if(bgpOpenMsgRecived == true)
            {
                ConnectionRetryTimer_Reset();
                //sends a KEEPALIVE message, and
                
                KeepAliveTimer_Reset();
                HoldTimer_Reset();
                GlobalVariables.listnerConnectionState = "OpenConform";
                GlobalVariables.speakerConnectionState = "OpenConform";
                bgpOpenMsgRecived = false;
            }
            //******** After implementing AS************
            //If the value of the Autonomous System field is the same as the local Autonomous System number,then the connection is an "internal" 
            //connection; otherwise, it is an "external" connection.
            if(bgpHederErr == true || bgpOpenMsgErr == true)
            {
                //sends a NOTIFICATION message with the appropriate error code,
                ConnectionRetryTimer_Reset();
                //releses all BGP resources
                //drops the TCP Connection
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                bgpHederErr = false;
                bgpOpenMsgErr = false;
            }
            /**
            If a NOTIFICATION message is received with a version error (Event 24), the local system:
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection, and
                    - changes its state to Idle.
           In response to any other event (Events 9, 11-13, 20, 25-28), the local system:
                    - sends the NOTIFICATION with the Error Code Finite State[init_BGP.connCount] Machine Error,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            **/
            if(bgpNotifyMsgError == true)
            {
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP connection, and
                GlobalVariables.listnerConnectionState = "Idle";
                bgpNotifyMsgError = false;
            }
            if (connectRetryExpires == true  || keepAliveExpires == true || bgpNotifyMessage == true || bgpKeepAliveMessage == true ||
               bgpUpdateMessage == true || bgpUpdateMsgError == true)
            {
                //Console.WriteLine("BGP Events in OpenSent (Events 9, 11-13, 20, 25-28), do stuff here!!");
                //sends the NOTIFICATION with the Error Code Finite State[init_BGP.connCount] Machine Error,
               
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                //relese all BGP resources
                //drops TCP connection
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                connectRetryExpires = false;
                keepAliveExpires = false;
                bgpNotifyMessage = false;
                bgpKeepAliveMessage = false;
                bgpUpdateMessage = false;
                bgpUpdateMsgError = false;
            }
        }
        /**
        OpenConfirm State[init_BGP.connCount]:
            In this state, BGP waits for a KEEPALIVE or NOTIFICATION message.
             In response to the AutomaticStop event initiated by the system (Event 8), the local system:
                    - sends the NOTIFICATION message with a Cease,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            If the HoldTimer_Expires event (Event 10) occurs before a KEEPALIVE message is received, the local system:
                    - sends the NOTIFICATION message with the Error Code Hold Timer Expired,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            If the local system receives a KeepaliveTimer_Expires event (Event 11), the local system:
                    - sends a KEEPALIVE message,
                    - restarts the KeepaliveTimer, and
                    - remains in the OpenConfirmed state.
            In the event of a TcpConnection_Valid event (Event 14), or the success of a TCP connection (Event 16 or Event 17) while in OpenConfirm, 
            the local system needs to track the second connection.
            
        **/
        public void OpenConfirm()
        {
            //In this state, BGP waits for a KEEPALIVE or NOTIFICATION message.
            if (bgpAutoStop == true)
            {
                // sends the Notification message with a Cease,
                ConnectionRetryTimer_Reset();
                //releases all the BGP resources,
                //drops the TCP connection,
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                bgpAutoStop = false;
            }
            if(holdTimeExpires == true)
            {
                //sends the NOTIFICATION message with the Error Code Hold Timer Expired,
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP connection,
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                holdTimeExpires = false;
            }
            if(keepAliveExpires == true)
            {
                //sends a keepAlive message,
                BGPListner listner = new BGPListner();
                listner.KeepAliveExpired();
                //listner.SendingKeepAliveMsg_Speaker();
                KeepAliveTimer_Reset();
                ConnectionRetryTimer_Reset();
                GlobalVariables.listnerConnectionState = "Idle";
                keepAliveExpires = false;
                connectRetryExpires = false;
            }
            /**
            If the local system receives a TcpConnectionFails event (Event 18) from the underlying TCP or a NOTIFICATION message (Event 25), the local system:
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            If the local system receives a NOTIFICATION message with a version error (NotifMsgVerErr (Event 24)), the local system:
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection, and
                    - changes its state to Idle.
            
            **/
            if(tcpConnectionFail == true)
            {
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP connection
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                tcpConnectionFail = false;
            }
            if(bgpNotifyMsgErr == true)
            {
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP connection
                GlobalVariables.listnerConnectionState = "Idle";
                bgpNotifyMsgErr = false;
            }
            /**
           
            If an OPEN message is received, all fields are checked for correctness.  If the BGP message header checking (BGPHeaderErr
        (Event 21)) or OPEN message checking detects an error (see Section 6.2) (BGPOpenMsgErr (Event 22)), the local system:
                    - sends a NOTIFICATION message with the appropriate error code,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            **/
           
            if(bgpHederErr == true || bgpOpenMessageErr == true)
            {
                //sends a NOTIFICATION message with the appropriate error code,
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP connection,
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                bgpHederErr = false;
                bgpOpenMessageErr = false;
            }
        }
        /**
    Established State[init_BGP.connCount]:
            In the Established state, the BGP FSM can exchange UPDATE, NOTIFICATION, and KEEPALIVE messages with its peer.
            Any Start event (Events 1, 3-7) is ignored in the Established state.
            In response to an AutomaticStop event (Event 8), the local system:
                    - sends a NOTIFICATION with a Cease,
                    - sets the ConnectRetryTimer to zero
                    - deletes all routes associated with this connection,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            One reason for an AutomaticStop event is: A BGP receives an UPDATE messages with a number of prefixes for a given peer such that the
        total prefixes received exceeds the maximum number of prefixes configured.  The local system automatically disconnects the peer.
            If the HoldTimer_Expires event occurs (Event 10), the local system:
                    - sends a NOTIFICATION message with the Error Code Hold Timer Expired,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            If the KeepaliveTimer_Expires event occurs (Event 11), the local system:
                    - sends a KEEPALIVE message, and
                    - restarts its KeepaliveTimer, unless the negotiated HoldTime value is zero.
        **/
        public void EstablishedState()
        {
            //In the Established state, the BGP FSM can exchange UPDATE, NOTIFICATION, and KEEPALIVE messages with its peer.
            if (bgpAutoStop == true)
            {
                //sends a NOTIFICATION with a Cease,
                ConnectionRetryTimer_Reset();
                //deletes all routes associated with this connection,
                //releases all BGP resources,
                //drops the TCP connection,
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                bgpAutoStop = false;
            }
            if(holdTimeExpires == true)
            {
                //sends a NOTIFICATION message with the Error Code Hold Timer Expired,
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP connection,
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                holdTimeExpires = false;
            }
            if(keepAliveExpires == true)
            {
                //sends a KEEPALIVE message,
                //restarts its KeepaliveTimer, unless the negotiated HoldTime value is zero.
                KeepAliveTimer_Reset();
                ConnectionRetryTimer_Reset();
                keepAliveExpires = false;
                connectRetryExpires = false;
            }
            /**
            If the local system receives a NOTIFICATION message (Event 24 or Event 25) or a TcpConnectionFails (Event 18) from the underlying TCP, the local system:
                    - sets the ConnectRetryTimer to zero,
                    - deletes all routes associated with this connection,
                    - releases all the BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - changes its state to Idle.
            If the local system receives a KEEPALIVE message (Event 26), the local system:
                    - restarts its HoldTimer, if the negotiated HoldTime value is non-zero, and
                    - remains in the Established state.
            If the local system receives an UPDATE message (Event 27), the local system:
                    - processes the message,
                    - restarts its HoldTimer, if the negotiated HoldTime value is non-zero, and
                    - remains in the Established state.
            **/
            if(bgpNotifyMsg == true)
            {
                ConnectionRetryTimer_Reset();
                //deletes all routes associated with this connection,
                //releases all the BGP resources,
                //drops the TCP connection,
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                bgpNotifyMsg = false;
            }
            if(bgpKeepAliveMessage == true)
            {
                HoldTimer_Reset();
                GlobalVariables.listnerConnectionState = "Established";
                bgpKeepAliveMessage = false;
            }
            if(bgpUpdateMessage == true)
            {
                //processes the message,
                HoldTimer_Reset();
                GlobalVariables.listnerConnectionState = "Established";
                bgpUpdateMessage = false;
            }
            /**
            If the local system receives an UPDATE message, and the UPDATE message error handling procedure (see Section 6.3) detects an error (Event 28), the local system:
                    - sends a NOTIFICATION message with an Update error,
                    - sets the ConnectRetryTimer to zero,
                    - deletes all routes associated with this connection,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            In response to any other event (Events 9, 12-13, 20-22), the local system:
                    - sends a NOTIFICATION message with the Error Code Finite State[init_BGP.connCount] Machine Error,
                    - deletes all routes associated with this connection,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its state to Idle.
            **/
            if(bgpUpdateMsgError == true)
            {
                //sends a NOTIFICATION message with an Update error,
                ConnectionRetryTimer_Reset();
                //deletes all routes associated with this connection,
                //releases all BGP resources,
                //drops the TCP connection,
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                bgpUpdateMsgError = false;
            }
            if (connectRetryExpires == true || bgpHeaderMsgErr == true || bgpOpenMsgErr == true)
            {
                //sends a NOTIFICATION message with the Error Code Finite State[init_BGP.connCount] Machine Error,
                //deletes all routes associated with this connection,
                ConnectionRetryTimer_Reset();
                //releases all BGP resources
                //drops the TCP connection
                connectRetryCounter++;
                GlobalVariables.listnerConnectionState = "Idle";
                connectRetryExpires = false;
                bgpHeaderMsgErr = false;
                bgpOpenMsgErr = false;
            }
        }
        
        public void StartBGPConnectionMethod (bool start)
        { 
            
            OnAutomaticStartEvent += new EventHandler(SM_OnAutomaticStartEvent);
            AutomaticStart = start;
            OnAutomaticStartEvent -= new EventHandler(SM_OnAutomaticStartEvent);
        }
        private void SM_OnAutomaticStartEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Automatic Start Event is Fired here");
            autoStartEvent = GlobalVariables.True;
            IdleState();
        }
        public void StopBGPConnectionMethod(bool stop)
        {

            OnAutomaticStopEvent += new EventHandler(SM_OnAutomaticStopEvent);
            AutomaticStop = stop;
        }

        private void SM_OnAutomaticStopEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Automatic Stop Event is Fired here");
            bgpAutoStop = true;
            ConnectState();
            //flag value for another method
            bgpAutoStop = true;
            ActiveState();
            bgpAutoStop = true;
            OpenSent();
            bgpAutoStop = true;
            OpenConfirm();
            bgpAutoStop = true;
            EstablishedState();
        }
        public void TcpConnectionAckd(bool tcp_ack)
        {
            Tcp_Acked_Event += new EventHandler(SM_Tcp_Acked_Event);
            TCPConnectionAcknowladged = tcp_ack;
            Tcp_Acked_Event += new EventHandler(SM_Tcp_Acked_Event);
        }

        private void SM_Tcp_Acked_Event(object sender, EventArgs e)
        {
            Console.WriteLine("TCP Connection ACKED");
            //throw new NotImplementedException();
        }
        public void TcpConnectionConformed(bool tcp_con)
        {
            TcpConnectionConformed_Event += new EventHandler(SM_TcpConnectionConformed_Event);
            TcpConnectionConformedValue = tcp_con;
            TcpConnectionConformed_Event += new EventHandler(SM_TcpConnectionConformed_Event);
        }

        private void SM_TcpConnectionConformed_Event(object sender, EventArgs e)
        {
            //Console.WriteLine("TCP Connection Conformed");
            tcpConnectionSucceeds = GlobalVariables.True;
            ConnectState();
            //flag value for another method
            tcpConnectionSucceeds = GlobalVariables.True;
            ActiveState();
            //throw new NotImplementedException();
        }
        public void TcpConnectionFailed(bool tcp_fail)
        {
            TcpConnectionFails_Event += new EventHandler(SM_TcpConnectionFails_Event);
            TcpConnectionFails = tcp_fail;
            TcpConnectionFails_Event -= new EventHandler(SM_TcpConnectionFails_Event);
            
        }

        private void SM_TcpConnectionFails_Event(object sender, EventArgs e)
        {
            Console.WriteLine("TCP Connection Failled");
            tcpConnectionFail = true;
            ConnectState();
            //flag value for another method
            tcpConnectionFail = true;
            ActiveState();
            tcpConnectionFail = true;
            OpenSent();
            tcpConnectionFail = true;
            OpenConfirm();
            //throw new NotImplementedException();
        }
        public void BGPHederError(bool bgpHederErr)
        {
            BGPHeaderErr_Event += new EventHandler(SM_BGPHeaderErr_Event);
            BGPHederErr = bgpHederErr;
            BGPHeaderErr_Event -= new EventHandler(SM_BGPHeaderErr_Event);
        }

        private void SM_BGPHeaderErr_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Header Message Error");
            bgpHeaderMsgErr = true;
            ConnectState();
            //flag value for another method
            bgpHeaderMsgErr = true;
            ActiveState();
            bgpHeaderMsgErr = true;
            OpenSent();
            bgpHeaderMsgErr = true;
            OpenConfirm();
            bgpHeaderMsgErr = true;
            EstablishedState();
            //throw new NotImplementedException();
        }
        public void BGPOpenMsgRecived(bool openRecived)
        {
            BGPOpenMsgRecived_Event += new EventHandler(SM_BGPOpenMsgRecived_Event);
            BGPOpenMsgRecive = openRecived;
            BGPOpenMsgRecived_Event += new EventHandler(SM_BGPOpenMsgRecived_Event);
        }

        private void SM_BGPOpenMsgRecived_Event(object sender, EventArgs e)
        {
            //Console.WriteLine("BGP Open Message Recived by Listner");
            bgpOpenMsgRecived = GlobalVariables.True;
            ActiveState();
            bgpOpenMsgRecived = GlobalVariables.True;
            OpenSent();
        }
       

        public void BGPOpenMsgSent(bool openSent)
        {
            BGPOpenMsg_Event += new EventHandler(SM_BGPOpenMsgSent_Event);
            BGPOpenMsg = openSent;
            BGPOpenMsg_Event -= new EventHandler(SM_BGPOpenMsgSent_Event);
        }

        private void SM_BGPOpenMsgSent_Event(object sender, EventArgs e)
        {
            //Console.WriteLine("BGP Open Message Sent");
            bgpOpenMessage = true;
            ConnectState();
            //flag value for another method
            bgpOpenMessage = true;
            ActiveState();
            //throw new NotImplementedException();
        }
        public void BGPOpenMsgError(bool openMsgErr)
        {
            BGPOpenMsgErr_Event += new EventHandler(SM_BGPOpenMsgErr_Event);
            BGPOpenMessageErr = openMsgErr;
            BGPOpenMsgErr_Event -= new EventHandler(SM_BGPOpenMsgErr_Event);
        }

        private void SM_BGPOpenMsgErr_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Open message Error occured");
            bgpOpenMsgErr = true;
            ConnectState();
            //flag value for another method
            bgpOpenMsgErr = true;
            ActiveState();
            bgpOpenMessageErr = true;
            OpenSent();
            bgpOpenMessageErr = true;
            OpenConfirm();
            bgpOpenMessageErr = true;
            EstablishedState();
            //throw new NotImplementedException();
        }
        public void BGPNotifyMsgSent(bool notifySent)
        {
            BGPNotifyMsg_Event += new EventHandler(SM_BGPNotifyMsg_Event);
            BGPNotifyMsg = notifySent;
            BGPNotifyMsg_Event -= new EventHandler(SM_BGPNotifyMsg_Event);
        }

        private void SM_BGPNotifyMsg_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Notification message Send");
            bgpNotifyMessage = true;
            ConnectState();
            //flag value for another method
            bgpNotifyMessage = true;
            ActiveState();
            bgpNotifyMessage = true;
            OpenSent();
            bgpNotifyMessage = true;
            EstablishedState();
            //throw new NotImplementedException();
        }
        public void BGPNotifyMsgErrorSent(bool notifyErrMsg)
        {
            BGPNotifyMsgErr_Event += new EventHandler(SM_BGPNotifyMsgErr_Event);
            BGPNotifyMsgErr = notifyErrMsg;
            BGPNotifyMsgErr_Event -= new EventHandler(SM_BGPNotifyMsgErr_Event);
        }

        private void SM_BGPNotifyMsgErr_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Notification Error message Send");
            bgpNotifyMsgError = true;
            ConnectState();
            bgpNotifyMsgError = true;
            OpenSent();
            bgpNotifyMsgErr = true;
            OpenConfirm();
            //throw new NotImplementedException();
        }
        public void BGPKeepAliveMsgSend(bool keepAliveSent)
        {
            BGPKeepAliveMsg_Event += new EventHandler(SM_BGPKeepAliveMsgSend_Event);
            BGPKeepAliveMsg = keepAliveSent;
            BGPKeepAliveMsg_Event -= new EventHandler(SM_BGPKeepAliveMsgSend_Event);
        }

        private void SM_BGPKeepAliveMsgSend_Event(object sender, EventArgs e)
        {
            //Console.WriteLine("BGP Keep Alive  message Send");
            bgpKeepAliveMessage = true;
            ConnectState();
            //flag value for another method
            bgpKeepAliveMessage = true;
            ActiveState();
            bgpKeepAliveMessage = true;
            OpenSent();
            bgpKeepAliveMessage = true;
            EstablishedState();
            //throw new NotImplementedException();
        }
        public void BGPUpdateMsgSent(bool updateSent)
        {
            BGPUpdateMsg_Event += new EventHandler(SM_BGPUpdateMsg_Event);
            BGPUpdateMsg = updateSent;
            BGPUpdateMsg_Event -= new EventHandler(SM_BGPUpdateMsg_Event);
        }

        private void SM_BGPUpdateMsg_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Update message Send");
            bgpUpdateMessage = true;
            ConnectState();
            //flag value for another method
            bgpUpdateMessage = true;
            ActiveState();
            bgpUpdateMessage = true;
            OpenSent();
            bgpUpdateMessage = true;
            EstablishedState();
            //throw new NotImplementedException();
        }
        public void BGPUpdateMsgError(bool updateError)
        {
            BGPUpdateMsgErr_Event += new EventHandler(SM_BGPUpdateMsgErr_Event);
            BGPUpdateMsgErr = updateError;
            BGPUpdateMsgErr_Event -= new EventHandler(SM_BGPUpdateMsgErr_Event);
        }

        private void SM_BGPUpdateMsgErr_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Update message Error Occured");
            bgpUpdateMsgError = true;
            ConnectState();
            //flag value for another method
            bgpUpdateMsgError = true;
            ActiveState();
            bgpUpdateMsgError = true;
            OpenSent();
            bgpUpdateMsgError = true;
            EstablishedState();
            //throw new NotImplementedException();
        }

       
       //********** Timer section code *************
      
        bool connectionRetryFlag;
        bool holdTimerFlag;
        bool keepAliveTimerFlag;


        public void Timers()
        {

            ConnectionRetryTimer_Reset();
            HoldTimer_Reset();
            KeepAliveTimer_Reset();
        }
        public void ConnectionRetryTimer_Reset()
        {

            if (connectionRetryFlag == true)
            {
                ConnectRetryTimer.Close();

                connectionRetryFlag = false;
            }
            // Create a timer with a two 120000 interval.
            //SM.connectRetryTimer = new System.Timers.Timer(120000);
            connectRetryTimer = new System.Timers.Timer(120000);

            connectionRetryFlag = true;
            // Hook up the Elapsed event for the timer. 
            connectRetryTimer.Elapsed += OnConnectionRetryExpires;
            
            // Have the timer fire repeated events (true is the default)
            connectRetryTimer.AutoReset = false;

            // Start the timer
            connectRetryTimer.Enabled = true;
            //SM.ConnectRetryTimer.Start();
        }
        public void HoldTimer_Reset()
        {
            if (holdTimerFlag == true)
            {
                HoldTimer.Close();

                holdTimerFlag = false;
            }
            //SM.holdTimer = new System.Timers.Timer(240000);
            holdTimer = new System.Timers.Timer(240000);

            holdTimerFlag = true;
            holdTimer.Elapsed += OnHoldTimerExpire;

            // Have the timer fire repeated events (true is the default)
            holdTimer.AutoReset = false;

            // Start the timer
            holdTimer.Enabled = true;
        }
        public void KeepAliveTimer_Reset()
        {
        
            if (keepAliveTimerFlag == true)
            {
                keepaliveTimer.Close();

                keepAliveTimerFlag = false;
            }
            //SM.keepaliveTimer = new System.Timers.Timer(80000);
            keepaliveTimer = new System.Timers.Timer(80000);

            keepAliveTimerFlag = true;
            keepaliveTimer.Elapsed += OnkeepaliveTimerExpire;

            // Have the timer fire repeated events (true is the default)
            keepaliveTimer.AutoReset = false;

            // Start the timer
            keepaliveTimer.Enabled = true;
        }
        private void OnkeepaliveTimerExpire(object sender, ElapsedEventArgs e)
        {
            KeepaliveTimer_Expires += new EventHandler(SM_StopConnectionKeepaliveEvent);
            keepaliveTime = e.SignalTime;
            KeepaliveTimer = keepaliveTimer;
            KeepaliveTimer_Expires -= new EventHandler(SM_StopConnectionKeepaliveEvent);
            //throw new NotImplementedException();
        }

        private void OnHoldTimerExpire(object sender, ElapsedEventArgs e)
        {
            HoldTimer_Expires += new EventHandler(SM_StopConnectionHoldEvent);
            holdTime = e.SignalTime;
            HoldTimer = holdTimer;
            HoldTimer_Expires -= new EventHandler(SM_StopConnectionHoldEvent);
            //throw new NotImplementedException();
        }

        private void OnConnectionRetryExpires(object sender, ElapsedEventArgs e)
        {
            ConnectRetryTimer_Expires += new EventHandler(SM_StopConnectionRetryEvent);
            connectRetryTime = e.SignalTime;
            ConnectRetryTimer = connectRetryTimer;
            ConnectRetryTimer_Expires -= new EventHandler(SM_StopConnectionRetryEvent);
            //throw new NotImplementedException();
        }

        private void SM_StopConnectionRetryEvent(object sender, EventArgs e)
        {
            
            Console.WriteLine("Connection Retry Event is Fired here");

            connectRetryExpires = true;
            ConnectState();
            //flag value for another method
            connectRetryExpires = true;
            ActiveState();
            connectRetryExpires = true;
            OpenSent();
            connectRetryExpires = true;
            EstablishedState();
            //throw new NotImplementedException();
        }

        private void SM_StopConnectionHoldEvent(object sender, EventArgs e)
        {
            
            Console.WriteLine("Connection Hold Timer is expired Event is Fired here");
            holdTimeExpires = true;
            ConnectState();
            //flag value for another method
            holdTimeExpires = true;
            ActiveState();
            holdTimeExpires = true;
            OpenSent();
            holdTimeExpires = true;
            OpenConfirm();
            holdTimeExpires = true;
            EstablishedState();
            //throw new NotImplementedException();
        }

        private void SM_StopConnectionKeepaliveEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Connection Keepalive timer is expired Event is Fired here");
            keepAliveExpires = true;
            ConnectState();
            keepAliveExpires = true;
            OpenSent();
            keepAliveExpires = true;
            OpenConfirm();
            keepAliveExpires = true;
            EstablishedState();
            //throw new NotImplementedException();
        }
       
    }
}
