using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BGPSimulator.FSM;
using BGPSimulator.BGPMessage;

namespace BGPSimulator.BGP
{
    public class BGPSpeaker: Router
    {
       
        public Socket[] tempSocket = new Socket[14];
        FinateStateMachine FSM_Speaker = new FinateStateMachine();
        public bool conectionFlag;
        public int SpeakerID;
        public int ListnerID;
        public string message = "";


        private static AutoResetEvent speakerConnectionRequest = new AutoResetEvent(true);
        private static AutoResetEvent completeSpeakerConnection = new AutoResetEvent(true);
        private static AutoResetEvent bgpSpeakerState = new AutoResetEvent(true);
        private static AutoResetEvent bgpSpeakerOpenMsg = new AutoResetEvent(true);
        private static AutoResetEvent bgpSpeakerOpenMsgState = new AutoResetEvent(true);

        private static AutoResetEvent bgpListnerState = new AutoResetEvent(true);
        private static AutoResetEvent bgpListnerOpenMsgState = new AutoResetEvent(true);
        private static AutoResetEvent bgpListnerUpdateMsgState = new AutoResetEvent(true);
        private static AutoResetEvent bgpListnerUpdateMsg = new AutoResetEvent(true);


        //public int sendTo;


        //since client doesnot need to listen to the connection but it only does connect to server
        public void Connect(string ipAddress, int port, int speaker , int listner)
        {
            // Connect to a remote device.
            try
            {
                
                SpeakerID = speaker;
                ListnerID = listner;

                speakerConnectionRequest.WaitOne();

                //Console.WriteLine("IP ADDRESS: port : speaker ID : Listner ID :"+ipAddress +"  " + port + "  " + speaker + "  " + listner);

                _speakerSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectCallback, _speakerSocket);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ConnectCallback(IAsyncResult reasult)
        {
            try
            {
                // we catch that connection we send and since AsyncState is a object so we set it as Socket to get connection
                Socket speakerSocket = reasult.AsyncState as Socket;

                if (speakerSocket.Connected)
                {
                    conectionFlag = _speakerSocket.Connected;

                    //GlobalVariables.sucessfulConnection[GlobalVariables.i] = conectionFlag;
                    //GlobalVariables.i++;
                    GlobalVariables.True = conectionFlag;

                    // when one client connection is accepted then it stops accepting other clients by EndAccept
                    speakerSocket.EndConnect(reasult);

                    speakerConnectionRequest.Set();

                    //Store the speaker socket
                    GlobalVariables.SpeakerSocket_Dictionary.TryAdd(GlobalVariables.currentSpeakerCount, speakerSocket);
                    GlobalVariables.currentSpeakerCount++;

                    GlobalVariables.listnerNumber = ListnerID;
                    GlobalVariables.speakerIpAddress = ((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString();
                    GlobalVariables.listnerIpAddress = ((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString();

             

                    completeSpeakerConnection.WaitOne();

                    Console.Write("BGP Speaker " + SpeakerID + " : " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " Connected to ---->" +
                    "BGP Listner " + ListnerID + " : " + IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()));

                    FSM_Speaker.TcpConnectionConformed(GlobalVariables.True);

                    completeSpeakerConnection.Set();

                    bgpSpeakerState.WaitOne();
                    //connectDone.Set();
                    Console.WriteLine("BGP Listner : {0}| is in state : {1}", GlobalVariables.listnerIpAddress, GlobalVariables.listnerConnectionState);
                    bgpSpeakerState.Set();

                }

                _buffer = new byte[1024];
                speakerSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceivedCallback, speakerSocket);
                  
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
           
        }
       

        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {

                bgpListnerState.WaitOne();
                // Read data from the remote device.
                //int bytesRead = client.EndReceive(result);
                // we catch that connection we send and since AsyncState is a object so we set it as Socket to get connection
                Socket speakerSocket = result.AsyncState as Socket;
                int bufferLength = speakerSocket.EndReceive(result);
               
                byte[] packet = new byte[bufferLength];
                Array.Copy(_buffer, packet, packet.Length);
                bgpListnerState.Set();
                // Signal that all bytes have been received.
                // Signal that all bytes have been received.
                //Console.Write("\n"+"BGP Speaker:" + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has RECIVED ");

                //Handle packet here
                PacketHandler.Handle(packet, speakerSocket);
                //receiveDone.Set();
                //FSM_Speaker.BGPOpenMsgRecivedSpeaker(GlobalVariables.True);

                
                if (bufferLength == 58)
                {
                    bgpListnerUpdateMsgState.WaitOne();
                    Console.WriteLine("BGP Listner : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()), GlobalVariables.listnerConnectionState);
                    bgpListnerUpdateMsgState.Set();
                }
                if (bufferLength == 40)
                {
                    
                    FSM_Speaker.BGPKeepAliveMsgSend(GlobalVariables.True);
                    bgpListnerOpenMsgState.WaitOne();
                    Console.WriteLine("BGP Listner : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()), GlobalVariables.listnerConnectionState);
                    bgpListnerOpenMsgState.Set();
                }

                

                _buffer = new byte[1024];
                speakerSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceivedCallback, speakerSocket);
                
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("Speaker Connection is Closed");
                // Don't care
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        public void SendListner(byte[] data, Socket speaker, string msg)
        {
            try
            {
                Socket speakerSocket = speaker;
                message = msg;
                // Begin sending the data to the remote device.
                speakerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, speakerSocket);
                //bgpListnerUpdateMsg.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }

        }

        public void Send(byte [] data)
        {
            try
            {
                
                // Begin sending the data to the remote device.
                _speakerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, _speakerSocket);
                    // Console.WriteLine("*********************** Speaker" + IPAddress.Parse(((IPEndPoint)_speakerSocket.LocalEndPoint).Address.ToString())
                    //   +"*********************** Listner" + IPAddress.Parse(((IPEndPoint)_speakerSocket.RemoteEndPoint).Address.ToString()));
                    //Thread.Sleep(1000);
                    //sendDone.WaitOne();
               

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                
            }

        }
        private void SendCallback(IAsyncResult result)
        {
            try
            {
                
                // Retrieve the socket from the state object.
                Socket speakerSocket = result.AsyncState as Socket;
                

                // Complete sending the data to the remote device.
                int bytesSent = speakerSocket.EndSend(result);

                

                //sendDone.Set();
                FSM_Speaker.BGPOpenMsgSent(GlobalVariables.True);
                if (message == "")
                {
                    bgpSpeakerOpenMsg.WaitOne();

                    Console.WriteLine("BGP Speaker: " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has SEND  OPEN MESSAGE !!");

                    bgpSpeakerOpenMsg.Set();

                    bgpSpeakerOpenMsgState.WaitOne();

                    Console.WriteLine("BGP Speaker : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()), GlobalVariables.speakerConnectionState);

                    bgpSpeakerOpenMsgState.Set();
                }
                if (message == "Update")
                {
                    //bgpListnerUpdateMsg.Set();
                    //Console.WriteLine("BGP Speaker: " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has SEND  UPDATE MESSAGE !!");
                    //Console.WriteLine("BGP Speaker : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()), GlobalVariables.speakerConnectionState);
                }
                if(message == "Notify")
                {

                }
                

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
