using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BGPSimulator.FSM;

namespace BGPSimulator.BGP
{
    public class CloseRouter
    {
        private Dictionary<int, Socket> listnerSocket_DictionaryCopy = new Dictionary<int, Socket>();
        private Dictionary<int, Socket> SpeakerSocket_DictionaryCopy = new Dictionary<int, Socket>();
        UpdateMessageHandling updateHandler = new UpdateMessageHandling();
        ushort value;
        string stringValue;
        Socket socket;
        ushort tempAS;
        public void CloseSpeakerListner(string ipAddress)
        {
            closeSpeaker(ipAddress);
            closeListner(ipAddress);
            withadrawlRoutes(ipAddress, value);
            update();
            sendNotificationMsg();

        }
        public void closeSpeaker(string ipAddress)
        {
           
            foreach (KeyValuePair<int, Socket> speaker in GlobalVariables.SpeakerSocket_Dictionary)
            {
                try
                {
                    if (ipAddress == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()))
                    {
                        Console.WriteLine("Shutdown Speaker with IP: " + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()));
                        SpeakerSocket_DictionaryCopy.Add(speaker.Key, speaker.Value);
                        // Release the socket.                       
                        GlobalVariables.speaker_AS.TryRemove("" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()), out value);                        
                        GlobalVariables.conAnd_Speaker.TryRemove(speaker.Key, out stringValue);
                        GlobalVariables.conAnd_Listner.TryRemove(speaker.Key, out stringValue);
                        GlobalVariables.speakerConAnd_AS.TryRemove((ushort)speaker.Key, out value);
                        GlobalVariables.listnerConAnd_AS.TryRemove((ushort)speaker.Key, out value);
                        tempAS = value;

                        //GlobalVariables.SpeakerSocket_Dictionary.Remove(speaker.Key);
                        //speaker.Value.Dispose();


                        /**
                        speaker.Value.BeginDisconnect(true, DisconnectSpeakerCallback, speaker.Value);

                        // Wait for the disconnect to complete.
                        disconnectSpeakerDone.WaitOne();
                        if (speaker.Value.Connected)
                            Console.WriteLine("We're still connected");
                        else
                            Console.WriteLine("We're disconnected");
                        speaker.Value.Shutdown(SocketShutdown.Both);
                        speaker.Value.Close();
                        **/
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            foreach (KeyValuePair<int, Socket> speakercopy in SpeakerSocket_DictionaryCopy)
            {
                GlobalVariables.SpeakerSocket_Dictionary.TryRemove(speakercopy.Key, out socket);
                
            }

            }

        public void closeListner(string ipAddress)
        {
            
            foreach (KeyValuePair<int, Socket> listner in GlobalVariables.listnerSocket_Dictionary)
            {
                try { 
                if (ipAddress == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString()))
                {
                    Console.WriteLine("Shutdown listner with IP: " + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString()));
                    // Release the socket.

                    //GlobalVariables.listnerSocket_Dictionary.Remove(listner.Key);
                    listnerSocket_DictionaryCopy.Add(listner.Key, listner.Value);
                    GlobalVariables.listner_AS.TryRemove("" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString()), out value);
                    GlobalVariables.conAnd_Listner.TryRemove(listner.Key, out stringValue);
                    GlobalVariables.conAnd_Speaker.TryRemove(listner.Key, out stringValue);
                    GlobalVariables.listnerConAnd_AS.TryRemove((ushort)listner.Key, out value);
                    GlobalVariables.speakerConAnd_AS.TryRemove((ushort)listner.Key, out value);
                    //updateHandler.withadrawlRoutes(ipAddress);
                    //listner.Value.Dispose();
                    /**
                    listner.Value.BeginDisconnect(true, DisconnectListnerCallback, listner.Value);

                    // Wait for the disconnect to complete.
                    disconnectlistnerDone.WaitOne();
                    if (listner.Value.Connected)
                        Console.WriteLine("We're still connected");
                    else
                        Console.WriteLine("We're disconnected");
                    listner.Value.Shutdown(SocketShutdown.Both);
                    listner.Value.Close();
    **/
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            foreach (KeyValuePair<int, Socket> listnercopy in listnerSocket_DictionaryCopy)
            {
                GlobalVariables.listnerSocket_Dictionary.TryRemove(listnercopy.Key, out socket);
            }

        }
        public void withadrawlRoutes(string ipPrefix, int AS)
        {
            //GlobalVariables.withdrawnRoutes.Clear();
            Tuple<string, int> withdrawl_Routes = new Tuple<string, int>(ipPrefix, ipPrefix.Length);
            //GlobalVariables.withdrawl_IP_Address = ipPrefix;
           GlobalVariables.withdrawnRoutes.Add(AS, withdrawl_Routes);
        }
        public void sendNotificationMsg()
        {
            updateHandler.sendNotifyMsg(1, "Router conection is Ceased");
        }
        public void update()
        {
            GlobalVariables.data = Routes.GetTable();
            Console.WriteLine("Local Policy For AS1, AS2 and AS3 is UPDATED");
            updateHandler.adj_RIB_Out();
            //createUpdate.withadrawlRoutes("");
            updateHandler.pathAttribute();
            updateHandler.networkLayerReachibility();
            updateHandler.pathSegment();
        }
        /**

        private static void DisconnectSpeakerCallback(IAsyncResult ar)
        {
            // Complete the disconnect request.
            Socket client = (Socket)ar.AsyncState;
            client.EndDisconnect(ar);
            //client.Close();
            // Signal that the disconnect is complete.
            disconnectSpeakerDone.Set();
           
        }

        private static void DisconnectListnerCallback(IAsyncResult ar)
        {
            // Complete the disconnect request.
            Socket listner = (Socket)ar.AsyncState;
            listner.EndDisconnect(ar);
            //listner.Close();
            // Signal that the disconnect is complete.
            disconnectlistnerDone.Set();
        }

    **/
    }
    
}
