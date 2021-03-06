﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BGPSimulator.BGP
{
    public class Router
    {
        
        public Socket _listnerSocket;
        public Socket _speakerSocket;
        public byte[] _buffer = new byte[1024];

        private static AutoResetEvent speakerStarted = new AutoResetEvent(true);
        private static AutoResetEvent listnerStarted = new AutoResetEvent(true);

        public void ListnerSocket()
        {
            // initilize a socket of address family IPV4 , Stream Socket type, of TCP protocol
            _listnerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //GlobalVariables.currentSpeakerCount = 0;
        }
        public void SpeakerSocket()
        {
            _speakerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void BindSpeaker(string ipAddress, int port, int i)
        {
            
            //initialize router
            SpeakerSocket();
            // Binding the socket to any IPEndPoint with port parameter
            _speakerSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));

            speakerStarted.WaitOne();

            //Console.WriteLine("Speaker IPADDRESS and PORT :  "+ ipAddress+"  "+ port);

            Console.WriteLine("Router Speaker: " + i + " IPAddress:" + IPAddress.Parse(((IPEndPoint)_speakerSocket.LocalEndPoint).Address.ToString())
               + " Started!! It is in : " + GlobalVariables.speakerConnectionState + "state !!");

            speakerStarted.Set();

        }
        public void BindListner(string ipAddress, int port, int router)
        {
            //initialize router
            ListnerSocket();
            // Binding the socket to any IPEndPoint with port parameter
            _listnerSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));

            listnerStarted.WaitOne();

            Console.WriteLine("Router Listner: " + router + " IPAddress:" + IPAddress.Parse(((IPEndPoint)_listnerSocket.LocalEndPoint).Address.ToString())
                + " Started!! It is in : " + GlobalVariables.listnerConnectionState + "state !!");

            listnerStarted.Set();

        }
        // State object for reading client and server data asynchronously
        
    }
}
