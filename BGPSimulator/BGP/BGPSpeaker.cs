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
        
        
        //public int sendTo;
        
        
        //since client doesnot need to listen to the connection but it only does connect to server
        public void Connect(string ipAddress, int port, int speaker , int listner)
        {
            // Connect to a remote device.
            try
            {
                
                SpeakerID = speaker;
                ListnerID = listner;
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
               
                    conectionFlag = _speakerSocket.Connected;
                    GlobalVariables.True = conectionFlag;
                    // we catch that connection we send and since AsyncState is a object so we set it as Socket to get connection
                    Socket speakerSocket = reasult.AsyncState as Socket;
                    // when one client connection is accepted then it stops accepting other clients by EndAccept
                    speakerSocket.EndConnect(reasult);
                    //Store the speaker socket
                    GlobalVariables.SpeakerSocket_Dictionary.TryAdd(GlobalVariables.currentSpeakerCount, speakerSocket);
                    GlobalVariables.currentSpeakerCount++;

                    GlobalVariables.listnerNumber = ListnerID;
                    GlobalVariables.speakerIpAddress = ((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString();
                    GlobalVariables.listnerIpAddress = ((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString();
                   
                    Console.Write("BGP Speaker " + SpeakerID + " : " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " Connected to ---->" +
                    "BGP Listner " + ListnerID +  " : " + IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()));
                    
                    
                    FSM_Speaker.TcpConnectionConformed(GlobalVariables.True);
                    //connectDone.Set();
                    Console.WriteLine("BGP Listner : {0}| is in state : {1}", GlobalVariables.listnerIpAddress, GlobalVariables.listnerConnectionState);
                   
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
                

                // Read data from the remote device.
                //int bytesRead = client.EndReceive(result);
                // we catch that connection we send and since AsyncState is a object so we set it as Socket to get connection
                Socket speakerSocket = result.AsyncState as Socket;
                int bufferLength = speakerSocket.EndReceive(result);
               
                byte[] packet = new byte[bufferLength];
                Array.Copy(_buffer, packet, packet.Length);

                // Signal that all bytes have been received.
                // Signal that all bytes have been received.
                //Console.Write("\n"+"BGP Speaker:" + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has RECIVED ");
                    
                //Handle packet here
                PacketHandler.Handle(packet, speakerSocket);
                //receiveDone.Set();
                //FSM_Speaker.BGPOpenMsgRecivedSpeaker(GlobalVariables.True);

                
                if (bufferLength == 58)
                {
                    Console.WriteLine("BGP Listner : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()), GlobalVariables.listnerConnectionState);
                }
                if (bufferLength == 40)
                {
                    FSM_Speaker.BGPKeepAliveMsgSend(GlobalVariables.True);

                    Console.WriteLine("BGP Listner : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()), GlobalVariables.listnerConnectionState);
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
                    Console.WriteLine("BGP Speaker: " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has SEND  OPEN MESSAGE !!");

                    Console.WriteLine("BGP Speaker : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()), GlobalVariables.speakerConnectionState);

                }
                if (message == "Update")
                {
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
