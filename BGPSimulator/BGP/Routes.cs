using System;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Collections.Generic;
/**
3.  Summary of Operation
The primary function of a BGP speaking system is to exchange network reachability information with other BGP systems.  This network
reachability information includes information on the list of Autonomous Systems (ASes) that reachability information traverses.
This information is sufficient for constructing a graph of AS connectivity, from which routing loops may be pruned, and, at the AS
level, some policy decisions may be enforced.
In the context of this document, we assume that a BGP speaker advertises to its peers only those routes that it uses itself (in
this context, a BGP speaker is said to "use" a BGP route if it is the most preferred BGP route and is used in forwarding).  All other cases
are outside the scope of this document.
In the context of this document, the term "IP address" refers to an IP Version 4 address [RFC791].
Routing information exchanged via BGP supports only the destination-based forwarding paradigm, which assumes that a router forwards a
packet based solely on the destination address carried in the IP header of the packet.  This, in turn, reflects the set of policy
decisions that can (and cannot) be enforced using BGP.  Note that some policies cannot be supported by the destination-based forwarding
paradigm, and thus require techniques such as source routing (aka explicit routing) to be enforced.  Such policies cannot be enforced
using BGP either.  For example, BGP does not enable one AS to send traffic to a neighboring AS for forwarding to some destination
(reachable through but) beyond that neighboring AS, intending that the traffic take a different route to that taken by the traffic
originating in the neighboring AS (for that same destination).  On the other hand, BGP can support any policy conforming to the
destination-based forwarding paradigm.
BGP-4 provides a new set of mechanisms for supporting Classless Inter-Domain Routing (CIDR) [RFC1518, RFC1519].  These mechanisms
include support for advertising a set of destinations as an IP prefix and eliminating the concept of a network "class" within BGP.  BGP-4
also introduces mechanisms that allow aggregation of routes, including aggregation of AS paths.
This document uses the term `Autonomous System' (AS) throughout.  The classic definition of an Autonomous System is a set of routers under
a single technical administration, using an interior gateway protocol (IGP) and common metrics to determine how to route packets within the
AS, and using an inter-AS routing protocol to determine how to route packets to other ASes.  Since this classic definition was developed,
it has become common for a single AS to use several IGPs and, sometimes, several sets of metrics within an AS.  The use of the term
Autonomous System stresses the fact that, even when multiple IGPs and metrics are used, the administration of an AS appears to other ASes
to have a single coherent interior routing plan and presents a consistent picture of the destinations that are reachable through it.
BGP uses TCP [RFC793] as its transport protocol.  This eliminates the need to implement explicit update fragmentation, retransmission,
acknowledgement, and sequencing.  BGP listens on TCP port 179.  The error notification mechanism used in BGP assumes that TCP supports a
"graceful" close (i.e., that all outstanding data will be delivered before the connection is closed).
A TCP connection is formed between two systems.  They exchange messages to open and confirm the connection parameters.
The initial data flow is the portion of the BGP routing table that is allowed by the export policy, called the Adj-Ribs-Out (see 3.2).
Incremental updates are sent as the routing tables change.  BGP does not require a periodic refresh of the routing table.  To allow local
policy changes to have the correct effect without resetting any BGP connections, a BGP speaker SHOULD either (a) retain the current
version of the routes advertised to it by all of its peers for the duration of the connection, or (b) make use of the Route Refresh extension [RFC2918].
KEEPALIVE messages may be sent periodically to ensure that the connection is live.  NOTIFICATION messages are sent in response to
errors or special conditions.  If a connection encounters an error condition, a NOTIFICATION message is sent and the connection is closed.
A peer in a different AS is referred to as an external peer, while a peer in the same AS is referred to as an internal peer.  Internal BGP
and external BGP are commonly abbreviated as IBGP and EBGP.
If a particular AS has multiple BGP speakers and is providing transit service for other ASes, then care must be taken to ensure a
consistent view of routing within the AS.  A consistent view of the interior routes of the AS is provided by the IGP used within the AS.
For the purpose of this document, it is assumed that a consistent view of the routes exterior to the AS is provided by having all BGP
speakers within the AS maintain IBGP with each other.
This document specifies the base behavior of the BGP protocol.  This behavior can be, and is, modified by extension specifications.  When
the protocol is extended, the new behavior is fully documented in the extension specifications.
3.1.  Routes: Advertisement and Storage
For the purpose of this protocol, a route is defined as a unit of information that pairs a set of destinations with the attributes of a
path to those destinations.  The set of destinations are systems whose IP addresses are contained in one IP address prefix that is
carried in the Network Layer Reachability Information (NLRI) field of an UPDATE message, and the path is the information reported in the
path attributes field of the same UPDATE message.
Routes are advertised between BGP speakers in UPDATE messages. Multiple routes that have the same path attributes can be advertised
in a single UPDATE message by including multiple prefixes in the NLRI field of the UPDATE message.
Routes are stored in the Routing Information Bases (RIBs): namely, the Adj-RIBs-In, the Loc-RIB, and the Adj-RIBs-Out, as described in Section 3.2.
If a BGP speaker chooses to advertise a previously received route, it MAY add to, or modify, the path attributes of the route before
advertising it to a peer.
BGP provides mechanisms by which a BGP speaker can inform its peers that a previously advertised route is no longer available for use.
There are three methods by which a given BGP speaker can indicate that a route has been withdrawn from service:
a) the IP prefix that expresses the destination for a previously advertised route can be advertised in the WITHDRAWN ROUTES
field in the UPDATE message, thus marking the associated route as being no longer available for use,
b) a replacement route with the same NLRI can be advertised, or
c) the BGP speaker connection can be closed, which implicitly removes all routes the pair of speakers had advertised to eachother from service.
Changing the attribute(s) of a route is accomplished by advertising a replacement route.  The replacement route carries new (changed)
attributes and has the same address prefix as the original route.
3.2.  Routing Information Base
The Routing Information Base (RIB) within a BGP speaker consists of three distinct parts:
a) Adj-RIBs-In: The Adj-RIBs-In stores routing information learned from inbound UPDATE messages that were received from other BGP
speakers.  Their contents represent routes that are available as input to the Decision Process.
b) Loc-RIB: The Loc-RIB contains the local routing information the BGP speaker selected by applying its local policies to the
routing information contained in its Adj-RIBs-In.  These are the routes that will be used by the local BGP speaker.  The
next hop for each of these routes MUST be resolvable via the local BGP speaker's Routing Table.
c) Adj-RIBs-Out: The Adj-RIBs-Out stores information the local BGP speaker selected for advertisement to its peers.  The routing
information stored in the Adj-RIBs-Out will be carried in the local BGP speaker's UPDATE messages and advertised to its peers.
In summary, the Adj-RIBs-In contains unprocessed routing information that has been advertised to the local BGP speaker by its peers; the
Loc-RIB contains the routes that have been selected by the local BGP speaker's Decision Process; and the Adj-RIBs-Out organizes the routes
for advertisement to specific peers (by means of the local speaker's UPDATE messages).
Although the conceptual model distinguishes between Adj-RIBs-In, Loc-RIB, and Adj-RIBs-Out, this neither implies nor requires that an
implementation must maintain three separate copies of the routing information.  The choice of implementation (for example, 3 copies of
the information vs 1 copy with pointers) is not constrained by the protocol.
Routing information that the BGP speaker uses to forward packets (or to construct the forwarding table used for packet forwarding) is
maintained in the Routing Table.  The Routing Table accumulates routes to directly connected networks, static routes, routes learned
from the IGP protocols, and routes learned from BGP.  Whether a specific BGP route should be installed in the Routing Table, and
whether a BGP route should override a route to the same destination installed by another source, is a local policy decision, and is not
specified in this document.  In addition to actual packet forwarding, the Routing Table is used for resolution of the next-hop addresses
specified in BGP updates (see Section 5.1.3).
**/

namespace BGPSimulator.BGP
{
    public class Routes
    {
        
        public static DataTable GetTable()
        {
            GlobalVariables.conSpeakerAs_ListnerAs.Clear();
            // Here we create a DataTable with four columns.
            DataTable table = new DataTable();
            table.Columns.Add("Connection", typeof(int));
            table.Columns.Add("Network", typeof(string));
            table.Columns.Add("AS_N", typeof(int));
            table.Columns.Add("NextHop", typeof(string));
            table.Columns.Add("AS_NH", typeof(int));
            table.Columns.Add("IGP/EGP", typeof(int));

            foreach (KeyValuePair<int, string> pair in GlobalVariables.conAnd_Listner)
            {
                try { 
                if (GlobalVariables.listner_AS[pair.Value] == GlobalVariables.speaker_AS[GlobalVariables.conAnd_Speaker[pair.Key]])
                {

                    //Console.WriteLine("{0},{1}", pair.Key, pair.Value);
                    table.Rows.Add(pair.Key, GlobalVariables.conAnd_Speaker[pair.Key], GlobalVariables.speaker_AS[GlobalVariables.conAnd_Speaker[pair.Key]],
                      pair.Value, GlobalVariables.listner_AS[pair.Value], 0);
                    //Storing connection speaker (ip, AS) and listner (ip, As)
                    Tuple<string, ushort, string, ushort> conSpeakerAs_ListnerAs = new Tuple<string, ushort, string, ushort>(GlobalVariables.conAnd_Speaker[pair.Key],
                       GlobalVariables.speaker_AS[GlobalVariables.conAnd_Speaker[pair.Key]], pair.Value, GlobalVariables.listner_AS[pair.Value]);
                    GlobalVariables.conSpeakerAs_ListnerAs.Add(pair.Key, conSpeakerAs_ListnerAs);
                }
                else
                {
                    table.Rows.Add(pair.Key, GlobalVariables.conAnd_Speaker[pair.Key], GlobalVariables.speaker_AS[GlobalVariables.conAnd_Speaker[pair.Key]],
                        pair.Value, GlobalVariables.listner_AS[pair.Value], 1);
                    //Storing connection speaker (ip, AS) and listner (ip, As)
                    Tuple<string, ushort, string, ushort> conSpeakerAs_ListnerAs = new Tuple<string, ushort, string, ushort>(GlobalVariables.conAnd_Speaker[pair.Key],
                       GlobalVariables.speaker_AS[GlobalVariables.conAnd_Speaker[pair.Key]], pair.Value, GlobalVariables.listner_AS[pair.Value]);
                    GlobalVariables.conSpeakerAs_ListnerAs.Add(pair.Key, conSpeakerAs_ListnerAs);
                }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                    
            }
            
            return table;
        }
        
        public void DisplayDataAS1()
        {
            // This uses the GetTable method (please paste it in).
            //GlobalVariables.data = GetTable();

            Console.WriteLine("BGP ROUTING TABLE OF AS 1 where IGP = 0 and EGP = 1");
            Console.WriteLine("Connection"+ "   Network   "+" AS_N "+ "   NextHop  "+" AS_NH "+ "  IGP/EGP ");
            // ... Loop over all rows.
            foreach (DataRow row in GlobalVariables.data.Rows)
            {
                if(row.Field<int>(2) == 1 || row.Field<int>(4) == 1)
                {
                    // ... Write value of first field as integer.

                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     " 
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }
                
            }
        }
        public void DisplayDataAS2()
        {
            // This uses the GetTable method (please paste it in).
            
            Console.WriteLine("BGP ROUTING TABLE OF AS2 where IGP = 0 and EGP = 1");
            Console.WriteLine("Connection" + "   Network   " + " AS_N " + "   NextHop  " + " AS_NH " + "  IGP/EGP ");
            // ... Loop over all rows.
            foreach (DataRow row in GlobalVariables.data.Rows)
            {
                if (row.Field<int>(2) == 2 || row.Field<int>(4) == 2)
                {
                    // ... Write value of first field as integer.

                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     " 
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }
            }
        }
        public void DisplayDataAS3()
        {
            // This uses the GetTable method (please paste it in).
            
            Console.WriteLine("BGP ROUTING TABLE where IGP = 0 and EGP = 1");
            Console.WriteLine("Connection" + "   Network   " + " AS_N " + "   NextHop  " + " AS_NH " + "  IGP/EGP ");
            // ... Loop over all rows.
            foreach (DataRow row in GlobalVariables.data.Rows)
            {
                if (row.Field<int>(2) == 3 || row.Field<int>(4) == 3)
                {
                    // ... Write value of first field as integer.

                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }
            }
        }

    }
}
