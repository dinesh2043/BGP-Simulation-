using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BGPSimulator.BGP;
using System.Data;
using BGPSimulator.BGPMessage;
using BGPSimulator.FSM;
/**
9.  UPDATE Message Handling
An UPDATE message may be received only in the Established state. Receiving an UPDATE message in any other state is an error.  When an
UPDATE message is received, each field is checked for validity, as specified in Section 6.3.
If an optional non-transitive attribute is unrecognized, it is quietly ignored.  If an optional transitive attribute is unrecognized, the Partial bit 
(the third high-order bit) in the attribute flags octet is set to 1, and the attribute is retained for propagation to other BGP speakers.
If an optional attribute is recognized and has a valid value, then, depending on the type of the optional attribute, it is processed
locally, retained, and updated, if necessary, for possible propagation to other BGP speakers.
If the UPDATE message contains a non-empty WITHDRAWN ROUTES field, the previously advertised routes, whose destinations (expressed as IP
prefixes) are contained in this field, SHALL be removed from the Adj-RIB-In.  This BGP speaker SHALL run its Decision Process because
the previously advertised route is no longer available for use.
If the UPDATE message contains a feasible route, the Adj-RIB-In will be updated with this route as follows: if the NLRI of the new route
is identical to the one the route currently has stored in the Adj-RIB-In, then the new route SHALL replace the older route in the Adj-
RIB-In, thus implicitly withdrawing the older route from service. Otherwise, if the Adj-RIB-In has no route with NLRI identical to the
new route, the new route SHALL be placed in the Adj-RIB-In.
Once the BGP speaker updates the Adj-RIB-In, the speaker SHALL run its Decision Process.
9.1.  Decision Process
The Decision Process selects routes for subsequent advertisement by applying the policies in the local Policy Information Base (PIB) to
the routes stored in its Adj-RIBs-In.  The output of the Decision Process is the set of routes that will be advertised to peers; the
selected routes will be stored in the local speaker's Adj-RIBs-Out, according to policy.
The BGP Decision Process described here is conceptual, and does not have to be implemented precisely as described, as long as the
implementations support the described functionality and they exhibit the same externally visible behavior.
The selection process is formalized by defining a function that takes the attribute of a given route as an argument and returns either 
(a)a non-negative integer denoting the degree of preference for the route, or (b) a value denoting that this route is ineligible to be
installed in Loc-RIB and will be excluded from the next phase of route selection.
The function that calculates the degree of preference for a given route SHALL NOT use any of the following as its inputs: the existence
of other routes, the non-existence of other routes, or the path attributes of other routes.  Route selection then consists of the
individual application of the degree of preference function to each feasible route, followed by the choice of the one with the highest degree of preference.
The Decision Process operates on routes contained in the Adj-RIBs-In, and is responsible for:
- selection of routes to be used locally by the speaker
- selection of routes to be advertised to other BGP peers
- route aggregation and route information reduction
The Decision Process takes place in three distinct phases, each triggered by a different event:
a) Phase 1 is responsible for calculating the degree of preference for each route received from a peer.
b) Phase 2 is invoked on completion of phase 1.  It is responsible for choosing the best route out of all those available for each
distinct destination, and for installing each chosen route into the Loc-RIB.
c) Phase 3 is invoked after the Loc-RIB has been modified.  It is responsible for disseminating routes in the Loc-RIB to each
peer, according to the policies contained in the PIB.  Route aggregation and information reduction can optionally be performed within this phase.
9.1.1.  Phase 1: Calculation of Degree of Preference
The Phase 1 decision function is invoked whenever the local BGP speaker receives, from a peer, an UPDATE message that advertises a
new route, a replacement route, or withdrawn routes.
The Phase 1 decision function is a separate process,f which completes when it has no further work to do.
The Phase 1 decision function locks an Adj-RIB-In prior to operating on any route contained within it, and unlocks it after operating on
all new or unfeasible routes contained within it.
9.1.2.  Phase 2: Route Selection
The Phase 2 decision function is blocked from running while the Phase 3 decision function is in process.  The Phase 2 function locks all
Adj-RIBs-In prior to commencing its function, and unlocks them on completion.
If the NEXT_HOP attribute of a BGP route depicts an address that is not resolvable, or if it would become unresolvable if the route was
installed in the routing table, the BGP route MUST be excluded from the Phase 2 decision function.
It is critical that BGP speakers within an AS do not make conflicting decisions regarding route selection that would cause forwarding loops to occur.
For each set of destinations for which a feasible route exists in the Adj-RIBs-In, the local BGP speaker identifies the route that has:
a) the highest degree of preference of any route to the same setof destinations, or
b) is the only route to that destination, or
c) is selected as a result of the Phase 2 tie breaking rules specified in Section 9.1.2.2.
The local speaker SHALL then install that route in the Loc-RIB, replacing any route to the same destination that is currently being
held in the Loc-RIB.  When the new BGP route is installed in the Routing Table, care must be taken to ensure that existing routes to
the same destination that are now considered invalid are removed from the Routing Table.  Whether the new BGP route replaces an existing
non-BGP route in the Routing Table depends on the policy configured on the BGP speaker.
The local speaker MUST determine the immediate next-hop address from the NEXT_HOP attribute of the selected route (see Section 5.1.3).  If
either the immediate next-hop or the IGP cost to the NEXT_HOP (where the NEXT_HOP is resolved through an IGP route) changes, Phase 2 Route Selection MUST be performed again.
Notice that even though BGP routes do not have to be installed in the Routing Table with the immediate next-hop(s), implementations MUST
take care that, before any packets are forwarded along a BGP route, its associated NEXT_HOP address is resolved to the immediate
(directly connected) next-hop address, and that this address (or multiple addresses) is finally used for actual packet forwarding.
Unresolvable routes SHALL be removed from the Loc-RIB and the routing table.  However, corresponding unresolvable routes SHOULD be kept in
the Adj-RIBs-In (in case they become resolvable).
9.1.2.1.  Route Resolvability Condition
As indicated in Section 9.1.2, BGP speakers SHOULD exclude unresolvable routes from the Phase 2 decision.  This ensures that
only valid routes are installed in Loc-RIB and the Routing Table.
The route resolvability condition is defined as follows:
1) A route Rte1, referencing only the intermediate network address, is considered resolvable if the Routing Table contains
at least one resolvable route Rte2 that matches Rte1's intermediate network address and is not recursively resolved
(directly or indirectly) through Rte1.  If multiple matching routes are available, only the longest matching route SHOULD be considered.
2) Routes referencing interfaces (with or without intermediate addresses) are considered resolvable if the state of the
referenced interface is up and if IP processing is enabled on this interface.
BGP routes do not refer to interfaces, but can be resolved through the routes in the Routing Table that can be of both types (those that
specify interfaces or those that do not).  IGP routes and routes to directly connected networks are expected to specify the outbound
interface.  Static routes can specify the outbound interface, the intermediate address, or both.
Note that a BGP route is considered unresolvable in a situation wherethe BGP speaker's Routing Table contains no route matching the BGP route's NEXT_HOP. 
Mutually recursive routes (routes resolving each other or themselves) also fail the resolvability check.
It is also important that implementations do not consider feasible routes that would become unresolvable if they were installed in the
Routing Table, even if their NEXT_HOPs are resolvable using the current contents of the Routing Table (an example of such routes would be mutually recursive routes). 
This check ensures that a BGP speaker does not install routes in the Routing Table that will be removed and not used by the speaker.  Therefore, in addition 
to local Routing Table stability, this check also improves behavior of the protocol in the network.
Whenever a BGP speaker identifies a route that fails the resolvability check because of mutual recursion, an error message SHOULD be logged.
9.1.2.2.  Breaking Ties (Phase 2)
In its Adj-RIBs-In, a BGP speaker may have several routes to the same destination that have the same degree of preference.  The local
speaker can select only one of these routes for inclusion in the associated Loc-RIB.  The local speaker considers all routes with the
same degrees of preference, both those received from internal peers, and those received from external peers.
The following tie-breaking procedure assumes that, for each candidate route, all the BGP speakers within an autonomous system can ascertain
the cost of a path (interior distance) to the address depicted by the NEXT_HOP attribute of the route, and follow the same route selection algorithm.
Several of the criteria are described using pseudo-code.  Note that the pseudo-code shown was chosen for clarity, not efficiency.  It is
not intended to specify any particular implementation.  BGP implementations MAY use any algorithm that produces the same results as those described here.
a) Remove from consideration all routes that are not tied for having the smallest number of AS numbers present in their
AS_PATH attributes.  Note that when counting this number, an AS_SET counts as 1, no matter how many ASes are in the set.
b) Remove from consideration all routes that are not tied for having the lowest Origin number in their Origin attribute.
c) Remove from consideration routes with less-preferred MULTI_EXIT_DISC attributes.  MULTI_EXIT_DISC is only comparable
between routes learned from the same neighboring AS (the neighboring AS is determined from the AS_PATH attribute).
Routes that do not have the MULTI_EXIT_DISC attribute are considered to have the lowest possible MULTI_EXIT_DISC value.
d) If at least one of the candidate routes was received via EBGP, remove from consideration all routes that were received via IBGP.
e) Remove from consideration any routes with less-preferred interior cost.  The interior cost of a route is determined by
calculating the metric to the NEXT_HOP for the route using the Routing Table.  If the NEXT_HOP hop for a route is reachable,
but no cost can be determined, then this step should be skipped (equivalently, consider all routes to have equal costs).
f) Remove from consideration all routes other than the route that was advertised by the BGP speaker with the lowest BGP Identifier value.
g) Prefer the route received from the lowest peer address.
9.1.3.  Phase 3: Route Dissemination
The Phase 3 decision function is invoked on completion of Phase 2, or when any of the following events occur:
a) when routes in the Loc-RIB to local destinations have changed
b) when locally generated routes learned by means outside of BGP have changed
c) when a new BGP speaker connection has been established
9.1.4.  Overlapping Routes
A BGP speaker may transmit routes with overlapping Network Layer Reachability Information (NLRI) to another BGP speaker.  NLRI overlap
occurs when a set of destinations are identified in non-matching multiple routes.  Because BGP encodes NLRI using IP prefixes, overlap
will always exhibit subset relationships.  A route describing a smaller set of destinations (a longer prefix) is said to be more
specific than a route describing a larger set of destinations (a shorter prefix); similarly, a route describing a larger set of
destinations is said to be less specific than a route describing a smaller set of destinations.
The precedence relationship effectively decomposes less specific routes into two parts:
- a set of destinations described only by the less specific route, and
- a set of destinations described by the overlap of the less specific and the more specific routes
9.2.  Update-Send Process
The Update-Send process is responsible for advertising UPDATE messages to all peers.  For example, it distributes the routes chosen
by the Decision Process to other BGP speakers, which may be located in either the same autonomous system or a neighboring autonomous system.
When a BGP speaker receives an UPDATE message from an internal peer, the receiving BGP speaker SHALL NOT re-distribute the routing
information contained in that UPDATE message to other internal peers (unless the speaker acts as a BGP Route Reflector [RFC2796]).
As part of Phase 3 of the route selection process, the BGP speaker has updated its Adj-RIBs-Out.  All newly installed routes and all
newly unfeasible routes for which there is no replacement route SHALL be advertised to its peers by means of an UPDATE message.
A BGP speaker SHOULD NOT advertise a given feasible BGP route from its Adj-RIB-Out if it would produce an UPDATE message containing the
same BGP route as was previously advertised.
Any routes in the Loc-RIB marked as unfeasible SHALL be removed. Changes to the reachable destinations within its own autonomous
system SHALL also be advertised in an UPDATE message.
If, due to the limits on the maximum size of an UPDATE message (see Section 4), a single route doesn't fit into the message, the BGP
speaker MUST not advertise the route to its peers and MAY choose to log an error locally.
9.2.1.  Controlling Routing Traffic Overhead
The BGP protocol constrains the amount of routing traffic (that is, UPDATE messages), in order to limit both the link bandwidth needed to
advertise UPDATE messages and the processing power needed by the Decision Process to digest the information contained in the UPDATE messages.

9.2.1.1.  Frequency of Route Advertisement
The parameter MinRouteAdvertisementIntervalTimer determines the minimum amount of time that must elapse between an advertisement
and/or withdrawal of routes to a particular destination by a BGP speaker to a peer.  This rate limiting procedure applies on a per-
destination basis, although the value of MinRouteAdvertisementIntervalTimer is set on a per BGP peer basis.
Two UPDATE messages sent by a BGP speaker to a peer that advertise feasible routes and/or withdrawal of unfeasible routes to some common
set of destinations MUST be separated by at least MinRouteAdvertisementIntervalTimer.  This can only be achieved by
keeping a separate timer for each common set of destinations.  This would be unwarranted overhead.  Any technique that ensures that the
interval between two UPDATE messages sent from a BGP speaker to a peer that advertise feasible routes and/or withdrawal of unfeasible
routes to some common set of destinations will be at least MinRouteAdvertisementIntervalTimer, and will also ensure that a
constant upper bound on the interval is acceptable.
9.2.1.2.  Frequency of Route Origination
The parameter MinASOriginationIntervalTimer determines the minimum amount of time that must elapse between successive advertisements of
UPDATE messages that report changes within the advertising BGP speaker's own autonomous systems.
9.2.2.  Efficient Organization of Routing Information
Having selected the routing information it will advertise, a BGP speaker may avail itself of several methods to organize this
information in an efficient manner.
9.2.2.1.  Information Reduction
Information reduction may imply a reduction in granularity of policy control - after information is collapsed, the same policies will
apply to all destinations and paths in the equivalence class.
The Decision Process may optionally reduce the amount of information that it will place in the Adj-RIBs-Out by any of the following methods:
a) Network Layer Reachability Information (NLRI):
Destination IP addresses can be represented as IP address prefixes.  In cases where there is a correspondence between the
address structure and the systems under control of an autonomous system administrator, it will be possible to reduce
the size of the NLRI carried in the UPDATE messages.
b) AS_PATHs:
AS path information can be represented as ordered AS_SEQUENCEs or unordered AS_SETs.  AS_SETs are used in the route
aggregation algorithm described in Section 9.2.2.2.  They reduce the size of the AS_PATH information by listing each AS
number only once, regardless of how many times it may have appeared in multiple AS_PATHs that were aggregated.
An AS_SET implies that the destinations listed in the NLRI can be reached through paths that traverse at least some of the
constituent autonomous systems.  AS_SETs provide sufficient information to avoid routing information looping; however,
their use may prune potentially feasible paths because such paths are no longer listed individually in the form of
AS_SEQUENCEs.  In practice, this is not likely to be a problem because once an IP packet arrives at the edge of a group of
autonomous systems, the BGP speaker is likely to have more detailed path information and can distinguish individual paths from destinations.
9.2.2.2.  Aggregating Routing Information
Aggregation is the process of combining the characteristics of several different routes in such a way that a single route can be
advertised.  Aggregation can occur as part of the Decision Process to reduce the amount of routing information that will be placed in the Adj-RIBs-Out.
Aggregation reduces the amount of information that a BGP speaker must store and exchange with other BGP speakers.  Routes can be aggregated
by applying the following procedure, separately, to path attributes of the same type and to the Network Layer Reachability Information.
Routes that have different MULTI_EXIT_DISC attributes SHALL NOT be aggregated.
If the aggregated route has an AS_SET as the first element in its AS_PATH attribute, then the router that originates the route SHOULD
NOT advertise the MULTI_EXIT_DISC attribute with this route.
Path attributes that have different type codes cannot be aggregated together.  Path attributes of the same type code may be aggregated,
according to the following rules:
NEXT_HOP:
When aggregating routes that have different NEXT_HOP attributes, the NEXT_HOP attribute of the aggregated route
SHALL identify an interface on the BGP speaker that performs the aggregation.
ORIGIN attribute:
If at least one route among routes that are aggregated has ORIGIN with the value INCOMPLETE, then the aggregated route
MUST have the ORIGIN attribute with the value INCOMPLETE. Otherwise, if at least one route among routes that are
aggregated has ORIGIN with the value EGP, then the aggregated route MUST have the ORIGIN attribute with the value EGP.  In
all other cases,, the value of the ORIGIN attribute of the aggregated route is IGP.
AS_PATH attribute:
If routes to be aggregated have identical AS_PATH attributes, then the aggregated route has the same AS_PATH attribute as each individual route.
For the purpose of aggregating AS_PATH attributes, we model each AS within the AS_PATH attribute as a tuple <type, value>,
where "type" identifies a type of the path segment the AS belongs to (e.g., AS_SEQUENCE, AS_SET), and "value" identifies the AS number.
ATOMIC_AGGREGATE:
If at least one of the routes to be aggregated has ATOMIC_AGGREGATE path attribute, then the aggregated route SHALL have this attribute as well.
AGGREGATOR:
Any AGGREGATOR attributes from the routes to be aggregated MUST NOT be included in the aggregated route.  The BGP speaker
performing the route aggregation MAY attach a new AGGREGATOR attribute (see Section 5.1.7).
9.3.  Route Selection Criteria
Generally, additional rules for comparing routes among several alternatives are outside the scope of this document.  There are two exceptions:
- If the local AS appears in the AS path of the new route being considered, then that new route cannot be viewed as better than
any other route (provided that the speaker is configured to accept such routes).  If such a route were ever used, a routing loop could result.
- In order to achieve a successful distributed operation, only routes with a likelihood of stability can be chosen.  Thus, an
AS SHOULD avoid using unstable routes, and it SHOULD NOT make rapid, spontaneous changes to its choice of route.  Quantifying
the terms "unstable" and "rapid" (from the previous sentence) will require experience, but the principle is clear.  Routes
that are unstable can be "penalized" (e.g., by using the procedures described in [RFC2439]).
9.4.  Originating BGP routes
A BGP speaker may originate BGP routes by injecting routing information acquired by some other means (e.g., via an IGP) into BGP.
A BGP speaker that originates BGP routes assigns the degree of preference (e.g., according to local configuration) to these routes
by passing them through the Decision Process (see Section 9.1). These routes MAY also be distributed to other BGP speakers within the
local AS as part of the update process (see Section 9.2).  The decision of whether to distribute non-BGP acquired routes within an
AS via BGP depends on the environment within the AS (e.g., type of IGP) and SHOULD be controlled via configuration.
10.  BGP Timers
BGP employs five timers: ConnectRetryTimer (see Section 8), HoldTimer (see Section 4.2), KeepaliveTimer (see Section 8),
MinASOriginationIntervalTimer (see Section 9.2.1.2), and MinRouteAdvertisementIntervalTimer (see Section 9.2.1.1).



**/

namespace BGPSimulator.FSM
{
    public class UpdateMessageHandling
    {
        FinateStateMachine FSM = new FinateStateMachine();
        public void adj_RIB_Out()
        {
            int i = 0;
            GlobalVariables.Adj_RIB_Out.Clear();
            GlobalVariables.interASConIP.Clear();
            foreach (DataRow row in GlobalVariables.data.Rows)
            {
                
                // Local policy to find adj_RIB_Out AS1
                if (row.Field<int>(2) == 1 || row.Field<int>(4) == 1)
                {
                    if (row.Field<int>(5) == 1)
                    {
                        i++;
                        Tuple<string> neighbourAS = new Tuple<string>(GlobalVariables.prefixAS2);
                        //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP,Local AS_PREFIX, Neighbour AS_prefix
                        Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out = 
                                new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                        row.Field<string>(3), row.Field<int>(4),row.Field<int>(5), GlobalVariables.prefixAS1, neighbourAS);
                        GlobalVariables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        //storing inter AS ip
                        GlobalVariables.interASConIP.Add(i, row.Field<string>(1));
                        // ... Write value of first field as integer.
                       // Console.WriteLine(" Connection Count: " + row.Field<int>(0) + " Network: " + row.Field<string>(1) + " N_AS " + row.Field<int>(2) + " Next_Hop: "
                        //   + row.Field<string>(3) + " NH_AS: " + row.Field<int>(4) + " EGP: " + row.Field<int>(5));
                    }
                }
                // Local policy to find adj_RIB_Out AS2
                if (row.Field<int>(2) == 2 || row.Field<int>(4) == 2)
                {
                    if (row.Field<int>(5) == 1)
                    {
                        i++;
                        //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
                        if(row.Field<int>(2) == 1)
                        {
                            Tuple<string> neighbourAS = new Tuple<string>(GlobalVariables.prefixAS1);
                            Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out =
                                 new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                         row.Field<string>(3), row.Field<int>(4), row.Field<int>(5),GlobalVariables.prefixAS2, neighbourAS);
                            GlobalVariables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        }
                        if (row.Field<int>(4) == 3)
                        {
                            Tuple<string> neighbourAS = new Tuple<string>(GlobalVariables.prefixAS3);
                            Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out =
                                 new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                         row.Field<string>(3), row.Field<int>(4), row.Field<int>(5),GlobalVariables.prefixAS2, neighbourAS);
                            GlobalVariables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        }

                        //storing inter AS IP
                        GlobalVariables.interASConIP.Add(i, row.Field<string>(3));
                        // ... Write value of first field as integer.

                       // Console.WriteLine(" Connection Count: " + row.Field<int>(0) + " Network: " + row.Field<string>(1) + " N_AS " + row.Field<int>(2) + " Next_Hop: "
                         //   + row.Field<string>(3) + " NH_AS: " + row.Field<int>(4) + " EGP: " + row.Field<int>(5));
                    }
                }
                // Local policy to find adj_RIB_Out AS3
                if (row.Field<int>(2) == 3 || row.Field<int>(4) == 3)
                {
                    if (row.Field<int>(5) == 1)
                    {
                        i++;
                        Tuple<string> neighbourAS = new Tuple<string>(GlobalVariables.prefixAS2);
                        //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
                        Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out =
                                 new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                         row.Field<string>(3), row.Field<int>(4), row.Field<int>(5),GlobalVariables.prefixAS3, neighbourAS);
                        //storing inter AS ip
                        GlobalVariables.interASConIP.Add(i, row.Field<string>(1));
                        GlobalVariables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        // ... Write value of first field as integer.

                       // Console.WriteLine(" Connection Count: " + row.Field<int>(0) + " Network: " + row.Field<string>(1) + " N_AS " + row.Field<int>(2) + " Next_Hop: "
                        //   + row.Field<string>(3) + " NH_AS: " + row.Field<int>(4) + " EGP: " + row.Field<int>(5));
                    }
                }

            }
           
        }

        //pathAttribute is the combination of attribute(origin), attribute length, attrFlag and attrTypeCode
        public void pathAttribute()
        {
            GlobalVariables.pathAttribute.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in GlobalVariables.Adj_RIB_Out)
            {
                i++;
                Tuple<int, string, int, ushort> pathAttribute = new Tuple<int, string, int, ushort>(entry.Value.Item2.Length, entry.Value.Item2, entry.Value.Item6, (ushort) entry.Value.Item6);
                GlobalVariables.pathAttribute.Add(i, pathAttribute);
              // Console.WriteLine("conection count: "+entry.Value.Item1 + " Network: " + entry.Value.Item2 + " N_AS: "+ entry.Value.Item3 + " NEXT_HOP: "+ entry.Value.Item4 
                //   +" NH_AS: "+ entry.Value.Item5 + " IGP/EGP: "+ entry.Value.Item6 + " nlrPrefix: "+ entry.Value.Item7);
                // do something with entry.Value or entry.Key
            }

        }
        public void networkLayerReachibility()
        {
            GlobalVariables.NLRI.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in GlobalVariables.Adj_RIB_Out)
            {
                i++;
                Tuple<int, string> nlri = new Tuple<int, string>(entry.Value.Rest.Item1.Length, entry.Value.Rest.Item1);
                GlobalVariables.NLRI.Add(i, nlri);
               // Console.WriteLine(" Prefix Length: " + entry.Value.Rest.Item1.Length + " Prifix: " + entry.Value.Rest.Item1);
            }
        }
        public void pathSegment()
        {
            GlobalVariables.pathSegment.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in GlobalVariables.Adj_RIB_Out)
            {
                i++;
                Tuple < int, string> pathSegment = new Tuple<int, string>(1, ""+entry.Value.Item3 +""+ entry.Value.Item5);
                GlobalVariables.pathSegment.Add(i, pathSegment);
               // Console.WriteLine("PathSegment: " + "" + entry.Value.Item3 + "" + entry.Value.Item5);
            }
        }
       
       

        public void sendUpdateMsg(int i)
        {
            BGPListner bgpListner = new BGPListner();
            BGPSpeaker bgpSpeaker = new BGPSpeaker();
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in GlobalVariables.Adj_RIB_Out)
            {
                //switch (entry.Key)
                if (i == entry.Key)
                {
                    switch (i)
                    {
                        case 1:
                            Tuple<int, string> nlri = GlobalVariables.NLRI[entry.Key];
                            Tuple<int, string> pathSegment = GlobalVariables.pathSegment[entry.Key];
                            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
                            Tuple<int, string, int, ushort> pathAttribute = GlobalVariables.pathAttribute[entry.Key];
                            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
                            Tuple<int, string, int, string, int, int, string, Tuple<string>> adj_RIB_Out = GlobalVariables.Adj_RIB_Out[entry.Key];


                            if (GlobalVariables.withdrawnRoutes.ContainsKey(1))
                            {
                                Tuple<string, int> withdrawlInfo = GlobalVariables.withdrawnRoutes[1];
                                GlobalVariables.withdrawl_IP_Address = withdrawlInfo.Item1;
                                GlobalVariables.withdrawl_Length = withdrawlInfo.Item2;
                            }else 
                            {
                                GlobalVariables.withdrawl_IP_Address = "";
                                GlobalVariables.withdrawl_Length = 0;
                            }
                            UpdateMessage updatePacket = new UpdateMessage((UInt16)GlobalVariables.withdrawl_Length, GlobalVariables.withdrawl_IP_Address,(ushort)adj_RIB_Out.Item7.Length,
                               adj_RIB_Out.Item7, 24, (UInt32)pathAttribute.Item1, (UInt32)pathAttribute.Item3, (ushort)pathAttribute.Item4, pathAttribute.Item2, 1,
                               (ushort)pathSegment.Item1, pathSegment.Item2, (ushort)nlri.Item1, nlri.Item2);
                            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in GlobalVariables.conSpeakerAs_ListnerAs)
                            {
                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item3) && (speakerListner.Value.Item2 == 1) && (speakerListner.Value.Item4 == 1))
                                {
                                    foreach (KeyValuePair<int, Socket> listner in GlobalVariables.listnerSocket_Dictionary)
                                    {
                                        try
                                        {
                                            if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                                (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                                            {
                                                //  Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner1.Value.LocalEndPoint).Address.ToString()),
                                                // IPAddress.Parse(((IPEndPoint)listner1.Value.RemoteEndPoint).Address.ToString()));

                                                bgpListner.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                                FSM.BGPUpdateMsgSent(GlobalVariables.True);
                                            }
                                        }catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        }
                                    }
                                }
                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 1) && (speakerListner.Value.Item4 == 1))
                                {
                                    foreach (KeyValuePair<int, Socket> speaker in GlobalVariables.SpeakerSocket_Dictionary)
                                    {
                                        try
                                        {
                                            if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                                (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                            {
                                                //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                                //  IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));
                                                bgpSpeaker.SendListner(updatePacket.BGPmessage, speaker.Value, "Update");
                                                FSM.BGPUpdateMsgSent(GlobalVariables.True);
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        }
                                    }
                                }
                            }
                            break;
                        case 2:
                            nlri = GlobalVariables.NLRI[entry.Key];
                            pathSegment = GlobalVariables.pathSegment[entry.Key];
                            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
                            pathAttribute = GlobalVariables.pathAttribute[entry.Key];
                            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
                            adj_RIB_Out = GlobalVariables.Adj_RIB_Out[entry.Key];

                           if (GlobalVariables.withdrawnRoutes.ContainsKey(2))
                            {
                                Tuple<string, int> withdrawlInfo = GlobalVariables.withdrawnRoutes[2];
                                GlobalVariables.withdrawl_IP_Address = withdrawlInfo.Item1;
                                GlobalVariables.withdrawl_Length = withdrawlInfo.Item2;

                            }
                            else
                            {
                                GlobalVariables.withdrawl_IP_Address = "";
                                GlobalVariables.withdrawl_Length = 0;
                            }
                            updatePacket = new UpdateMessage((UInt16)GlobalVariables.withdrawl_Length, GlobalVariables.withdrawl_IP_Address,(ushort)adj_RIB_Out.Item7.Length,
                               adj_RIB_Out.Item7, 24, (UInt32)pathAttribute.Item1, (UInt32)pathAttribute.Item3, (ushort)pathAttribute.Item4, pathAttribute.Item2, 1,
                               (ushort)pathSegment.Item1, pathSegment.Item2, (ushort)nlri.Item1, nlri.Item2);
                            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in GlobalVariables.conSpeakerAs_ListnerAs)
                            {

                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item3) && (speakerListner.Value.Item4 == 2) && (speakerListner.Value.Item2 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> listner in GlobalVariables.listnerSocket_Dictionary)
                                    {
                                        if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            //  Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner1.Value.LocalEndPoint).Address.ToString()),
                                            // IPAddress.Parse(((IPEndPoint)listner1.Value.RemoteEndPoint).Address.ToString()));

                                            bgpListner.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            FSM.BGPUpdateMsgSent(GlobalVariables.True);
                                        }

                                    }

                                    /**
                                    //Console.WriteLine("This is my Connection of Speaker: " + speakerListner.Key+ "AS value: "+ speakerListner.Value.Item4);
                                    Socket listnerSocket = GlobalVariables.listnerSocket_Dictionary[speakerListner.Key];
                                    BGPListner bgpListner = new BGPListner();
                                    //bgpListner.SendSpeaker(updatePacket.BGPmessage, listnerSocket, "Update");
                                    **/
                                }
                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 2) && (speakerListner.Value.Item4 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> speaker in GlobalVariables.SpeakerSocket_Dictionary)
                                    {
                                        if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                        {

                                            //Socket speakerSocket = speaker.Value;
                                            //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                            //IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));

                                            //bgpSpeaker.SendListner(updatePacket.BGPmessage, speaker.Value, "Update");
                                            //FSM.BGPUpdateMsgSent(GlobalVariables.True);
                                        }
                                    }
                                }

                            }

                            break;
                        case 3:
                            nlri = GlobalVariables.NLRI[entry.Key];
                            pathSegment = GlobalVariables.pathSegment[entry.Key];
                            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
                            pathAttribute = GlobalVariables.pathAttribute[entry.Key];
                            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
                            adj_RIB_Out = GlobalVariables.Adj_RIB_Out[entry.Key];

                           if (GlobalVariables.withdrawnRoutes.ContainsKey(2))
                            {
                                Tuple<string, int> withdrawlInfo = GlobalVariables.withdrawnRoutes[2];
                                GlobalVariables.withdrawl_IP_Address = withdrawlInfo.Item1;
                                GlobalVariables.withdrawl_Length = withdrawlInfo.Item2;

                            }
                            else
                            {
                                GlobalVariables.withdrawl_IP_Address = "";
                                GlobalVariables.withdrawl_Length = 0;
                            }
                           
                                updatePacket = new UpdateMessage((UInt16)GlobalVariables.withdrawl_Length, GlobalVariables.withdrawl_IP_Address,(ushort)adj_RIB_Out.Item7.Length,
                               adj_RIB_Out.Item7, 24, (UInt32)pathAttribute.Item1, (UInt32)pathAttribute.Item3, (ushort)pathAttribute.Item4, pathAttribute.Item2, 1,
                               (ushort)pathSegment.Item1, pathSegment.Item2, (ushort)nlri.Item1, nlri.Item2);
                            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in GlobalVariables.conSpeakerAs_ListnerAs)
                            {

                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item3) && (speakerListner.Value.Item4 == 2) && (speakerListner.Value.Item2 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> listner in GlobalVariables.listnerSocket_Dictionary)
                                    {
                                        if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            //Socket listnerSocket = listner.Value;
                                            //Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString()),
                                            //   IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString()));
                                            //BGPListner bgpListner = new BGPListner();
                                            bgpListner.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            FSM.BGPUpdateMsgSent(GlobalVariables.True);

                                        }
                                    }

                                    //Console.WriteLine("This is my Connection of Speaker: " + speakerListner.Key + "AS value: " + speakerListner.Value.Item4);
                                    //Socket listnerSocket = GlobalVariables.listnerSocket_Dictionary[speakerListner.Key];

                                }
                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 2) && (speakerListner.Value.Item4 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> speaker in GlobalVariables.SpeakerSocket_Dictionary)
                                    {
                                        if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            //Socket speakerSocket = speaker.Value;
                                            // Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                            //     IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));
                                            //BGPSpeaker bgpSpeaker = new BGPSpeaker();
                                            bgpSpeaker.SendListner(updatePacket.BGPmessage, speaker.Value, "Update");
                                            FSM.BGPUpdateMsgSent(GlobalVariables.True);
                                        }

                                    }
                                    //Console.WriteLine("This is my Connection of Speaker: " + speakerListner.Key);
                                    //Socket speakerSocket = GlobalVariables.SpeakerSocket_Dictionary[speakerListner.Key];

                                }


                            }
                            break;
                        case 4:
                            nlri = GlobalVariables.NLRI[entry.Key];
                            pathSegment = GlobalVariables.pathSegment[entry.Key];
                            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
                            pathAttribute = GlobalVariables.pathAttribute[entry.Key];
                            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
                            adj_RIB_Out = GlobalVariables.Adj_RIB_Out[entry.Key];

                           if (GlobalVariables.withdrawnRoutes.ContainsKey(3))
                            {
                                Tuple<string, int> withdrawlInfo = GlobalVariables.withdrawnRoutes[3];
                                GlobalVariables.withdrawl_IP_Address = withdrawlInfo.Item1;
                                GlobalVariables.withdrawl_Length = withdrawlInfo.Item2;
                            }
                            else
                            {
                                GlobalVariables.withdrawl_IP_Address = "";
                                GlobalVariables.withdrawl_Length = 0;
                            }
                            
                                updatePacket = new UpdateMessage((UInt16)GlobalVariables.withdrawl_Length, GlobalVariables.withdrawl_IP_Address,(ushort)adj_RIB_Out.Item7.Length,
                               adj_RIB_Out.Item7, 24, (UInt32)pathAttribute.Item1, (UInt32)pathAttribute.Item3, (ushort)pathAttribute.Item4, pathAttribute.Item2, 1,
                               (ushort)pathSegment.Item1, pathSegment.Item2, (ushort)nlri.Item1, nlri.Item2);
                            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in GlobalVariables.conSpeakerAs_ListnerAs)
                            {

                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item3) && (speakerListner.Value.Item4 == 3) && (speakerListner.Value.Item2 == 3))
                                {
                                    foreach (KeyValuePair<int, Socket> listner in GlobalVariables.listnerSocket_Dictionary)
                                    {
                                        if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            //Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString()),
                                              // IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString()));

                                            bgpListner.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            FSM.BGPUpdateMsgSent(GlobalVariables.True);
                                        }

                                    }
                                }
                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item1) && (speakerListner.Value.Item4 == 3) && (speakerListner.Value.Item2 == 3))
                                {

                                    foreach (KeyValuePair<int, Socket> speaker in GlobalVariables.SpeakerSocket_Dictionary)
                                    {
                                        if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                        {

                                            //Socket speakerSocket = speaker.Value;
                                            //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                             // IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));

                                            bgpSpeaker.SendListner(updatePacket.BGPmessage, speaker.Value, "Update");
                                            FSM.BGPUpdateMsgSent(GlobalVariables.True);
                                        }
                                    }
                                }
                            }
                            
                            break;

                    }


                }
            }
        }
        public void sendNotifyMsg(int i, string error)
        {
            BGPListner bgpListner = new BGPListner();
            BGPSpeaker bgpSpeaker = new BGPSpeaker();
            Tuple<int, string> nlri = GlobalVariables.NLRI[i];
            Tuple<int, string> pathSegment = GlobalVariables.pathSegment[i];
            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
            Tuple<int, string, int, ushort> pathAttribute = GlobalVariables.pathAttribute[i];
            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
            Tuple<int, string, int, string, int, int, string, Tuple<string>> adj_RIB_Out = GlobalVariables.Adj_RIB_Out[i];


            if (GlobalVariables.withdrawnRoutes.ContainsKey(1))
            {
                Tuple<string, int> withdrawlInfo = GlobalVariables.withdrawnRoutes[1];
                GlobalVariables.withdrawl_IP_Address = withdrawlInfo.Item1;
                GlobalVariables.withdrawl_Length = withdrawlInfo.Item2;
            }
            else
            {
                GlobalVariables.withdrawl_IP_Address = "";
                GlobalVariables.withdrawl_Length = 0;
            }
            NotificationMessage notifyPacket = new NotificationMessage(GlobalVariables.errorCode, GlobalVariables.erorSubCode, error);
            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in GlobalVariables.conSpeakerAs_ListnerAs)
            {
                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item3) && (speakerListner.Value.Item2 == 1) && (speakerListner.Value.Item4 == 1))
                {
                    foreach (KeyValuePair<int, Socket> listner in GlobalVariables.listnerSocket_Dictionary)
                    {
                        try
                        {
                            if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                            {
                                //  Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner1.Value.LocalEndPoint).Address.ToString()),
                                // IPAddress.Parse(((IPEndPoint)listner1.Value.RemoteEndPoint).Address.ToString()));

                                bgpListner.SendSpeaker(notifyPacket.BGPmessage, listner.Value, "Notify");
                                FSM.BGPNotifyMsgSent(GlobalVariables.True);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 1) && (speakerListner.Value.Item4 == 1))
                {
                    foreach (KeyValuePair<int, Socket> speaker in GlobalVariables.SpeakerSocket_Dictionary)
                    {
                        try
                        {
                            if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                            {
                                //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                //  IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));
                                bgpSpeaker.SendListner(notifyPacket.BGPmessage, speaker.Value, "Notify");
                                FSM.BGPNotifyMsgSent(GlobalVariables.True);
                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }
       


    }
}
