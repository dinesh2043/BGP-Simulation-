using System;
using BGPSimulator.BGPMessage;
using System.Net.Sockets;
using BGPSimulator.FSM;
using System.Net;
using System.Threading;

namespace BGPSimulator.BGP
{
    public class InitilizeBGPListnerSpeaker 
    {
        public static BGPListner[] bgpListner = new BGPListner[10];
        public static BGPSpeaker[] bgpSpeaker = new BGPSpeaker[14];

        private static AutoResetEvent speakerConnectionRequest = new AutoResetEvent(true);

        //to create 13 speaker for 10 routers
        int m = 0;
        //to create 14 different connection
        int n = 0;
        public ushort AS;
        //KeepAliveMessage packet = new KeepAliveMessage();
        public void StartListner()
        {
            
            for (int i = 0; i < 10; i++)
            {
                
                bgpListner[i] = new BGPListner();
                //bgpListner[l].ListnerSocket();
                if (i < 3)
                {
                    AS = GlobalVariables.AS1;
                    GlobalVariables.listnerConAnd_AS.TryAdd((ushort)i, AS);
                    bgpListner[i].BindListner(GlobalVariables.as1_IP_peifix + i, GlobalVariables.listnerPortNumber, i);
                    GlobalVariables.listner_AS.TryAdd(GlobalVariables.as1_IP_peifix + i, AS);
                    

                }else if (i >2 && i < 7)
                {
                    AS = GlobalVariables.AS2;
                    GlobalVariables.listnerConAnd_AS.TryAdd((ushort)i, AS);
                    bgpListner[i].BindListner(GlobalVariables.as2_IP_Prefix + i, GlobalVariables.listnerPortNumber, i);
                    GlobalVariables.listner_AS.TryAdd(GlobalVariables.as2_IP_Prefix + i, AS);
                    
                } else if (i> 6 && i<10)
                {
                    AS = GlobalVariables.AS3;
                    GlobalVariables.listnerConAnd_AS.TryAdd((ushort)i, AS);
                    bgpListner[i].BindListner(GlobalVariables.as3_IP_Prefix + i, GlobalVariables.listnerPortNumber, i);
                    GlobalVariables.listner_AS.TryAdd(GlobalVariables.as3_IP_Prefix + i, AS);
                    
                }

                Thread.Sleep(500);
                //recient computers can handle 500 connections
            }
           

        }
        

        public void StartListning()
        {
           

            for (int i = 0; i < 10; i++)
            {
                
                if (i < 3)
                {
                    AS = GlobalVariables.AS1;
                    //GlobalVariables.autonomousSystemListner = AS;
                    //bgpListner[l].Bind("127.1.0." + j, 179, j);
                    bgpListner[i].Listen(10);
                    bgpListner[i].Accept();

                }
                else if (i > 2 && i < 7)
                {
                    AS = GlobalVariables.AS2;
                    //GlobalVariables.autonomousSystemListner = AS;
                    //bgpListner[l].Bind("127.2.0." + j, 179, j);
                    bgpListner[i].Listen(10);
                    bgpListner[i].Accept();
                }
                else if (i > 6 && i < 10)
                {
                    AS = GlobalVariables.AS3;
                    bgpListner[i].Listen(10);
                    bgpListner[i].Accept();
                    //l++;
                }
            }

            
        }
        public void StartSpeaker()
        {
            for (int k = 0; k < 10; k++)
            {
                

                bgpSpeaker[m] = new BGPSpeaker();
                
                if (k < 3)
                {
                    AS = GlobalVariables.AS1;
                    if( k == 2)
                    {
                        
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as1_IP_peifix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as1_IP_peifix + k, AS);
                       
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as1_IP_peifix + k, GlobalVariables.speakerPortNumber+1, m);
                        
                    }
                    else
                    {
                        
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as1_IP_peifix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as1_IP_peifix + k, AS);
                      
                    }
                    
                }
                else if (k >2 && k < 7)
                {
                    AS = GlobalVariables.AS2;
                    if ( k == 3)
                    {
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as2_IP_Prefix + k, AS);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber+1, m);
                    }
                    else if ( k == 6)
                    {
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as2_IP_Prefix + k, AS);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber+1, m);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber+2, m);
                    }
                    else if (k == 4 || k == 5)
                    {
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);

                        //GlobalVariables.speakerConAnd_AS.Add((ushort)k, AS);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as2_IP_Prefix + k, AS);
                    }
                    

                }
                else if (k>6 && k < 9)
                {
                    AS = GlobalVariables.AS3;

                    bgpSpeaker[m].BindSpeaker(GlobalVariables.as3_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);

                    //GlobalVariables.speakerConAnd_AS.Add((ushort)k, AS);
                    GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as3_IP_Prefix + k, AS);
                    

                }
                else if (k == 9)
                {
                    AS = GlobalVariables.AS3;
                    bgpSpeaker[m].BindSpeaker(GlobalVariables.as3_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);
                    GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as3_IP_Prefix + k, AS);
                    
                }

                //bgpSpeaker[k].Bind("127.1.0.1", 179, m);
                m++;

                Thread.Sleep(500);

            }
            //SpeakerConnection_Init();
        }
        public void SpeakerConnection_Init()
        {
            for (int k = 0; k < 10; k++)
            {


                if (k < 3)
                {
                    
                    GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS1);
                    GlobalVariables.connCountListner = n;
                    if(k == 2)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k+1);

                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as1_IP_peifix + k);
                        SendOpenMessageToListner(n);
                        
                        //SendOpenMessageToListner(n);
                        n++;
                        GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS1);
                        bgpSpeaker[n].Connect(GlobalVariables.as1_IP_peifix + (k -2), GlobalVariables.listnerPortNumber, k, k-2);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as1_IP_peifix + (k - 2 ));
                        //bgpSpeaker[k].Connect();
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as1_IP_peifix + k);
                        SendOpenMessageToListner(n);

                    }
                    else
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as1_IP_peifix + (k + 1), GlobalVariables.listnerPortNumber, k, k+1);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as1_IP_peifix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as1_IP_peifix + k);
                        SendOpenMessageToListner(n);

                    }
                    
                    
                    //SendOpenMessageToListner(n);

                }
                else if (k > 2 && k < 7)
                {
                    
                    GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS2);
                    
                    GlobalVariables.connCountListner = n;
                    if(k == 3)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k+1);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);
                        /**
                        Console.WriteLine("###############ff" + k);
                        Console.WriteLine("###############fff" + (k + 1));
                        Console.WriteLine("###############fff" + n);
                        **/
                        //SendOpenMessageToListner(n);
                        n++;
                        GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS2);
                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k + 2), GlobalVariables.listnerPortNumber, k, k+2);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k + 2));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);
                        /**
                        Console.WriteLine("###############" + k);
                        Console.WriteLine("###############" + (k + 1));
                        Console.WriteLine("###############" + n);
                        **/
                    }
                    else if (k == 6)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as3_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k+1);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as3_IP_Prefix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);
                        //SendOpenMessageToListner(n);
                        n++;
                        GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS2);

                        // *********** Some problem in this connection *********************

                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k -2), GlobalVariables.listnerPortNumber, k, k-2);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k - 2));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);
                        //SendOpenMessageToListner(n);
                        n++;


                        GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS2);

                        // *********** Some problem in this connection *********************

                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k - 3), GlobalVariables.listnerPortNumber, k, k-3);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k - 3));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);
                    }
                    else if (k == 4 || k == 5)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k+1);
                        
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);
                        /**
                        Console.WriteLine("###############LLLL" + k);
                        Console.WriteLine("###############llll" + (k + 1));
                        Console.WriteLine("###############llll" + n);
                        **/
                    }

                    
                    //SendOpenMessageToListner(n);

                }
                else if (k > 6 && k < 9)
                {
                    /**
                    AS = GlobalVariables.AS3;
                   
                    bgpSpeaker[k].BindSpeaker(GlobalVariables.as3_IP_Prefix + m, 178, m);
                    **/
                    GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS3);
                    //GlobalVariables.speakerIpAddress = GlobalVariables.as1_IP_peifix + (m);
                    //GlobalVariables.listnerIpAddress = GlobalVariables.as3_IP_Prefix + (m+1);
                    GlobalVariables.connCountListner = n;

                    bgpSpeaker[n].Connect(GlobalVariables.as3_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k+1);

                    

                    GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as3_IP_Prefix + (k + 1));
                    
                    GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as3_IP_Prefix + k);
                    SendOpenMessageToListner(n);

                }
               else if(k == 9)
                {
                    /**
                    AS = GlobalVariables.AS3;
                    bgpSpeaker[k].BindSpeaker(GlobalVariables.as3_IP_Prefix + m, 178, m);
                    **/
                    GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS3);
                    //GlobalVariables.listnerIpAddress = GlobalVariables.as3_IP_Prefix +(m-2);

                    // *********** Some problem in this connection *********************


                    bgpSpeaker[n].Connect(GlobalVariables.as3_IP_Prefix + (k - 2), GlobalVariables.listnerPortNumber, k, k-2);
                    //GlobalVariables.connCountListner = k;

                    GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as3_IP_Prefix + (k - 2));
                    
                    GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as3_IP_Prefix + k);
                    SendOpenMessageToListner(n);
                    /**
                    Console.WriteLine("###############LLLL" + k);
                    Console.WriteLine("###############llll" + (k + 1));
                    Console.WriteLine("###############llll" + n);
                    //SendOpenMessageToListner(n);
                    **/
                }

                //bgpSpeaker[k].Bind("127.1.0.1", 179, m);

                n++;

            }
            

        }
        public void SendOpenMessageToListner(int k)
        {
            //for (int k = 0; k < 14; k++)
            //{
            //if (bgpSpeaker[k].conectionFlag == true)
            if (GlobalVariables.True)
                {
                
                    GlobalVariables.conCountSpeaker = k;

                //Console.WriteLine("*********** SPEAKER NUMBER**************** : " + k);

                    //OpenMessage(ushort type, ushort version,ushort myAS, ushort holdTime, string bgpIdentifier, ushort optimalParLength)
                    OpenMessage openPacket = new OpenMessage(GlobalVariables.bgpVerson, GlobalVariables.speakerConAnd_AS[(ushort)k], GlobalVariables.holdTime,
                        GlobalVariables.conAnd_Speaker[k], GlobalVariables.optimalParLength);

                    bgpSpeaker[k].Send(openPacket.BGPmessage);
               
            }

           // }
            
                   
             
        
        }
      
        
        
        
        
        /**
        ServerSocket.ServerSocket();
        ServerSocket.Bind("127.0.0.1", 179);
        //recient computers can handle 500 connections
        ServerSocket.Listen(10);
        ServerSocket.Accept();
        
        ClientSocket.ClientSocket();
        ClientSocket.Bind("127.0.0.2", 179);
        ClientSocket.Connect("127.0.0.1", 179);
        KeepAliveMessage packet = new KeepAliveMessage();

        //Since it asyncSocket and we want to execute when it is listening and accepting also so we have forever loop here
        while (true)
        {
            //Console.ReadLine();
            Console.WriteLine("Press any key to exit.");
            if (ClientSocket.conectionFlag)
            {
                ClientSocket.Send(packet.BGPmessage);
            }
            
            Console.ReadLine();
           // Console.ReadKey();
        }
        **/
    }
  }


