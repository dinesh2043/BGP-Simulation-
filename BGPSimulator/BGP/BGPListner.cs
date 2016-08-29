using System;
using System.Net;
using System.Net.Sockets;
using BGPSimulator.FSM;
using BGPSimulator.BGPMessage;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace BGPSimulator.BGP
{
    public class BGPListner : Router
    {
       
        FinateStateMachine FSM_Listner = new FinateStateMachine();
        
        public Socket[] tempSocket = new Socket[14];
        public string messageType;

        private static AutoResetEvent acceptedConnection = new AutoResetEvent(true);
        private static AutoResetEvent recievedMessage = new AutoResetEvent(true);
        
        //private static AutoResetEvent sendKeepAliveMessage = new AutoResetEvent(true);
        private static AutoResetEvent sendKeepAlive = new AutoResetEvent(true);
        private static AutoResetEvent sendUpdateMsg = new AutoResetEvent(true);



        public void Listen(int backlog)
        {
            // listens for 500 tcp backup connection request
            _listnerSocket.Listen(backlog);
        }
        //it is implemented to accept to listen data
        public void Accept()
        {
           
            try
            {
                
                // Start an asynchronous socket to listen for connections.
                //Console.WriteLine("Waiting for a connection...");

                // Set the event to nonsignaled state.

                // Begin to accept the client connection
                // and it asks for two parameter with AsyncCallback and object (AcceptedCallback and null) null is set for object reference parameter

                _listnerSocket.BeginAccept( AcceptedCallback, _listnerSocket);
                //acceptDone.Set();
                //acceptDone.WaitOne();
                // Wait until a connection is made before continuing.
                

            }

            
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        // This Asynccallback is called when the connection is sucessful
        private void AcceptedCallback(IAsyncResult reasult)
        {
            try
            {
                acceptedConnection.WaitOne();

                // Get the socket that handles the client request.
                Socket listnerSocket = reasult.AsyncState as Socket;
                listnerSocket = listnerSocket.EndAccept(reasult);
                
                //acceptDone.Set();
                // Create the state object.
                // StateObject state = new StateObject();
                // state.workSocket = handler;
                //allDone.Set();

                if (GlobalVariables.ConnectionCount < GlobalVariables.conAnd_Speaker.Count)
                    {
                        GlobalVariables.listnerSocket_Dictionary.TryAdd(GlobalVariables.ConnectionCount, listnerSocket);

                        GlobalVariables.ConnectionCount++;

                    }

                acceptedConnection.Set();

                // it is the place where we store data and this step is done to clear previous data from memory
                _buffer = new byte[1024];

                // this is done to be ready to recive data 
                // the four paramaters are buffer, the place in packet where 0 is the begining of packet, the amount of data where we set full capacity of buffer
                // SocketFlag is none, when the data is received the next parameter defines where to send data, and last one is the connection which should be passed 
                // to callback method
                listnerSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceivedCallback, listnerSocket);
                //reciveDone.WaitOne();
                
                //accept method is called to listen to more connection
                Accept();
                //acceptDone.WaitOne();
                recievedMessage.WaitOne();



            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ReceivedCallback(IAsyncResult reasult)
        {
            try
            {
                
                // we catch that connection we send and since AsyncState is a object so we set it as Socket to get connection
                Socket listnerSocket = reasult.AsyncState as Socket;
                

                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                //StateObject state = (StateObject)reasult.AsyncState;
                //Socket handler = state.workSocket;

                //EndReceive is used to count the amount of data received
                int bufferSize = listnerSocket.EndReceive(reasult);
                //reciveDone.Set();
                //if (bufferSize == 58 || bufferSize == 40)
                //{
                
                //it is done to store to store the data in buffer to packet
                byte[] packet = new byte[bufferSize];
                //reciveDone.Set();
               // Console.WriteLine("*********************** Listner" + IPAddress.Parse(((IPEndPoint)listnerSocket.LocalEndPoint).Address.ToString())
                 //   + "*********************** Speaker" + IPAddress.Parse(((IPEndPoint)listnerSocket.RemoteEndPoint).Address.ToString()));

                // it is done to create a shadow clone of buffer before anyone uses it
                // this method stores the data in buffer to packet
                Array.Copy(_buffer, packet, packet.Length);

                

                //Handle the packet
                PacketHandler.Handle(packet, listnerSocket);

                recievedMessage.Set();

                FSM_Listner.BGPOpenMsgRecived(GlobalVariables.True);
                
                //}
               // else
               //{

                _buffer = new byte[1024];
                listnerSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceivedCallback, listnerSocket);
              // }

            }
            catch (ObjectDisposedException ex)
            {
                // Don't care
                Console.WriteLine("Listner socket is closed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        
        public void SendingOpenMsg_Speaker()
        {
            //tracking the proper connection number and the speaker socket was the difficult task

            if (GlobalVariables.openMsgSendCount < GlobalVariables.listnerSocket_Dictionary.Count)
            {
                //tempSocket[GlobalVariables.openMsgSendCount] = GlobalVariables.listnerSocket_Dictionary[GlobalVariables.openMsgSendCount];
                Socket tempSock = GlobalVariables.listnerSocket_Dictionary[GlobalVariables.openMsgSendCount];
                OpenMessage openPacket = new OpenMessage(GlobalVariables.bgpVerson, GlobalVariables.speakerConAnd_AS[(ushort)GlobalVariables.openMsgSendCount], GlobalVariables.holdTime,
                   "" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()), GlobalVariables.optimalParLength);
                
                
                //Console.WriteLine("Sending open packet" + "verson : " + GlobalVariables.bgpVerson + "AS : " + GlobalVariables.speakerConAnd_AS[(ushort)GlobalVariables.openMsgSendCount] + "Hold Time : " + GlobalVariables.holdTime + "IP : "
                // + IPAddress.Parse(((IPEndPoint)tempSocket[GlobalVariables.openMsgSendCount].RemoteEndPoint).Address.ToString()) + "param : " + GlobalVariables.optimalParLength);
                messageType = "OPEN";
                //Socket temSoc = tempSocket[GlobalVariables.openMsgSendCount];
                Console.WriteLine("BGP Listner:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has send open Message !!");
                SendSpeaker(openPacket.BGPmessage, tempSock, messageType);

                //sendOpenDone.WaitOne();

                GlobalVariables.openMsgSendCount++;
            }
            
        }
        public void SendingKeepAliveMsg_Speaker()
        {
            //tracking the proper connection number and the speaker socket was the difficult task

            if (GlobalVariables.keepAliveMsgSendCount < GlobalVariables.listnerSocket_Dictionary.Count)
            {
                


                //tempSocket[GlobalVariables.keepAliveMsgSendCount] = GlobalVariables.listnerSocket_Dictionary[GlobalVariables.keepAliveMsgSendCount];
                Socket tempSock = GlobalVariables.listnerSocket_Dictionary[GlobalVariables.keepAliveMsgSendCount];
                KeepAliveMessage keepAlivePacket = new KeepAliveMessage();
                messageType = "KeepAlive";

                //sendKeepAliveMessage.WaitOne();

                //Socket temSoc = tempSocket[GlobalVariables.keepAliveMsgSendCount];
                Console.WriteLine("BGP Listner:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has send keepAlive Message !!");

                

                SendSpeaker(keepAlivePacket.BGPmessage, tempSock, messageType);
                //sendKeepAliveMessage.Set();

                //sendKeepAliveDone.WaitOne();

                GlobalVariables.keepAliveMsgSendCount++;

                
            }

        }
        public void KeepAliveExpired()
        {
            if (GlobalVariables.keepAliveExpiredCount < GlobalVariables.listnerSocket_Dictionary.Count)
            {
                if (GlobalVariables.listnerSocket_Dictionary.ContainsKey(GlobalVariables.keepAliveExpiredCount))
                {

                    

                    Socket tempSock = GlobalVariables.listnerSocket_Dictionary[GlobalVariables.keepAliveExpiredCount];
                    KeepAliveMessage keepAlivePacket = new KeepAliveMessage();
                    messageType = "KeepAlive";

                    sendKeepAlive.WaitOne();

                    Console.WriteLine("BGP Listner:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has send keepAlive Message !!");

                    sendKeepAlive.Set();

                    SendSpeaker(keepAlivePacket.BGPmessage, tempSock, messageType);
                    GlobalVariables.keepAliveExpiredCount++;
                }
                else
                {
                    GlobalVariables.keepAliveExpiredCount++;
                }

            }else
            {
                GlobalVariables.keepAliveExpiredCount = 0;
            }
        }

        public void SendSpeaker(byte[] data, Socket sendSock, string msg)
        {
            try
            {

                Socket listnerSocket = sendSock;

                if (msg == "OPEN")
                {
                    
                    messageType = msg;
                    //FSM_Listner.BGPKeepAliveMsgSend(GlobalVariables.True);
                    listnerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                    //sendOpenDone.Set();

                }
                else if (msg == "KeepAlive")
                {
                    
                    messageType = msg;
                    listnerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                    //sendKeepAliveDone.Set();
                    //FSM_Listner.BGPKeepAliveMsgSend(GlobalVariables.True);
                 
                }
                else if (msg == "Update")
                {
                    messageType = msg;
                    listnerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);

                    sendUpdateMsg.WaitOne();
                    //Console.WriteLine("Listner Send update message to speeker");
                }
                else if (msg == "Notify")
                {
                    messageType = msg;
                    listnerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                    //Console.WriteLine("Listner Send update message to speeker");
                }



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
                
                Socket listnerSocket = result.AsyncState as Socket;

                // Complete sending the data to the remote device.
                int bytesSent = listnerSocket.EndSend(result);
                sendUpdateMsg.Set();




                //sendOpenDone.Set();
                //sendKeepAliveDone.Set();


                //sendDone.Set();
                //FSM_Listner.BGPOpenMsgSent(GlobalVariables.True);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
