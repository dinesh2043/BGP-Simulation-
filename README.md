# BGP-Simulation-
## C# project with async sockets and 10 routers

### Introduction

Border Gateway protocol (BGP) is considered to be the routing protocol between inter-autonomous systems. Which is also known as the external boarder gateway protocol (EBGP) and the same protocol can also be used for routing within the same autonomous system (AS) known as internal boarder gateway protocol (IBGP). BGP system consists of the set of routers where it uses port 179 for BGP messaging. BGP system consists of BGP listeners and BGP speakers, where listeners listens for the connection and receives BGP messages and BGP speaker enforces the BGP protocol.   In general a router can be understood as a combination of listener’s sockets and speakers sockets where the IP address of all the sockets are managed by the default gateway. The main functionality of the BGP speaking system is to exchange the network reachability information between each other BGP with the network connection. Basically in the network reachability information there is the information about the AS’s that are reachable using that particular network. BGP-4 is famous with its set of features which supports a Classless Inter-Domain Routing (CIDR). CIDR supports the mechanism for advertising a set of destinations in the network as IP prefix. Another important feature of BGP protocol is the mechanism to support the routes and AS path aggregation. This protocol supports only destination based routing information exchange. Assuming that the routers forward a packets with destination address inside the IP header of the packet. Due to this reason BGP supports only those policies which uses destination based packet forwarding. [1] 

BGP describes Autonomous System (AS) as a collection of routers which is controlled by the same administration using interior gateway protocol (IGP). BGP suggests us to use common metrics implementation to determine the routes of the incoming packets within the internal AS and external AS. This is the particular portion of the implementation where the administrators of AS can enforce their local policies. BGP uses transport control protocol (TCP) to have end to end connection between the networks to ensure the delivery of the packets. This protocol facilitates us for separate implementation of update fragmentation, retransmission, acknowledgement, and sequencing of packets. BGP has explicitly defined that the TCP port 179 is used for listening the incoming connection request and incoming packets. In BGP first of all the TCP connection is established between two systems which exchange messages to open and conform the connection. Routing in the BGP is done according to the adjacent routing information base out (Adj-Ribs-Out), which is updated according to the changes in the routing table with the help of incremental updates. To ensure the connection between the routers KEEPALIVE message is send periodically to keep the connection alive. BGP sends NOTIFICATION message as a response to some errors or special conditions. When the TCP connection is established between two routers they send OPEN message to each other. After the acceptance of the OPEN message, listener sends KEEPALIVE message. UPDATE message is used to transfer the routing information between the connected peer. A peer of routers in the same AS is known as internal peer and the routers which are located in different AS are known as external peer. One AS system can have multiple BGP speakers which provides the transit services for other AS’s. [1] 

According to the BGP specification Finite State Machine (FSM) can be divided into two sub categories called “description of Events for the state machine and description of the FSM”. Basically the events of state machine are determined by the session attributes like state, connection retry counter, connection retry time, hold timer, hold time, keepalive timer and keepalive time. In my implementation I have implemented the optional session attributes called allow automatic start and allow automatic stop as administrative events.  According to the BGP specification there are three mandatory timer events called connection retry timer expires event, hold timer expires event and keepalive timer expires event. There are also some mandatory TCP connection based events called TCP connection acknowledged, TCP connection conformed and TCP connection fail. BGP also specifies the mandatory BGP message based events like; BGP open, BGP header error, notify message verification error, notify message, keepalive message, update message, and update message error. According to the BGP specification, BGP must have a separate FSM for each connected peer. The active or connecting end of the TCP connection is called outgoing connection. The passive or listening end (TCP port 179) of the connection is called incoming connection. When the connection between the peer is established it doesn’t matter which side is active and which is passive but the particular end of connection with TCP port 179 must be known. BGP specifications also defines different state of connection and their particular implementations. According to BGP specification the different state are idle state, connect state, active state, open sent state, open conformed state and established state. When the connection is in established state update message is send by the BGP speaker and received by the BGP listener. In the normal condition the withdrawn routes field is empty, but if the previously advertised destination address is ceased and is not available then the value of withdrawn route is equal to that destination address. According to the implementation of the local policy of AS and the adjacent routing information base in (Adj-RiB-In) and local routing information base (Loc-RIB) BGP speaker runs decision process to generates adjacent routing information base out (Adj-RIB-Out). [1]

In the following section of this report I am going to explain about the implementation of BGP simulation using C# programming language in console application with the help of asynchronous server and client socket API. The detailed explanation of the theoretical background and related practical implementation and methods are discussed in the particular heading inside the project implementation section. First section of the document will have a class diagram of the project for the explanation of the general overview of the project. Since, I was unable to find a prepare routers API and routing API for this implementation, which forced me to code for the routers and its connection using C# Socket class. All of those details are discussed in the second section of this document. In the third section I have the implementation methods and explanations of BGP messages. Implementation of state machine and FSM will be discussed in the fifth section of the document. Along with FSM explanation I will also discuss about the requirements of each BGP state along with the actual implementation with open and keepalive message communication mechanism. The main explanation of the sixth section will be about update message handling, Adj-RIB-Out, decision process and sending update messages by BGP speakers. The final section will be about shutting down the routers which will invoke the notification message to notify the administrator for resends the update message to update the changes in the routing table of the AS’s. 

This was my first network programming experience and it was challenging task for me. Where I got an opportunity to learn more about network programming. It also helped me to make my understanding deeper in the router and routing implementation. It was difficult and I had many problems when I worked in this project with multiple threads. This helped me to understand more about the thread safety. Before this project I had some experience in using C# dictionary class but I have never encountered the thread safety problem. But during this project I had that issues which helped me to learn about C# concurrent dictionary. Similarly in my previous coding experience I had never used asynchronous programming module. It was difficult in the beginning but it was also another good learning experience that I have had during this project. Since, it was a group project and if our group members had worked together then, maybe we could have finished those further improvement section also. It was a big project for me while doing it individually but I feel good about it when I think about the things that I have learned while doing this project. At the time of writing this conclusion of this project I realised that it was a nice project which helped me to realise the complexity of protocol processing. It also helped me to get some experience of protocol processing while writing the code by following RFC 4271 documentation.

### Class Diagram

I started the implementation of the project from abstract BGP message structure class where, I have used byte array to store the data stream of packet. All the four different BGP message classes inherits this class. The actual implementation and its complication will be discussed in the upcoming section of this document. As the second part of the implementation I have used state machine class to initialize BGP session attributes, timer events, TCP connection events and BGP message events. FSM class inherits state machine where it uses all of these attributes and events to handle individual session and events for each routers within the AS. Router class is used to define the speaker and listener sockets to work as a single router with single IP address. BGP speaker and BGP listener inherits the router to create speaker and listener sockets. To address the project requirements where, the simulation must contain 10 routers in mesh topology with AS’s implementations. I have implemented InitilizeBGPSpeakerListner class. When BGP speakers and listeners sockets initialization is complete and they are online FSM updates their connection status to be in idle state. Then BGP speaker initiates TCP connection to the listener and sends the open message to listeners and then FSM changes its connection state to active. When BGP listener receives the open message it sends open conformation message and keep alive message to the BGP speakers. When speaker receives the open message it sets the FSM connection state to open conformed and after receiving keep alive message speaker sets the connection state to established state. For all those connection which are in established state BGP speaker initiates the local policy and decision process to create the routes for the AS’s, this part of the implemented can be found in routes class. When the routing table of the local AS’s is ready then BGP speaker uses the UpdateMessageHandling class to advertise the routing information in the internal AS system. In this simulation project I have used 10 asynchronous listener sockets and 14 asynchronous speaker sockets with individual BGP session attributes, timer events, TCP connection events and BGP message events which are handled by FSM. Which requires a large amount of the data to be stored and accessed during the run time from multiple threads. To address this particular need I have used a static global variables class. As all the data communicated between the routers in this implementation are in byte stream. To print those data in human readable format in console window all the messages are passed through packet handler. Implementation of help information, execution of control methods and initiation of routers is done through program class. It is the main entry point for the execution of the software and the communication between the routers is triggered from this class with the help of automatic start event of FSM class. In the following class diagram we can see the classes and the detailed explanation of each class will be discussed in the upcoming section of this document.

 
Figure 1: Class Diagram of the project

### BGP Messages
Message Structure class implements message header and different message specific data handling. BGP message header consists message marker, message type and message length. All the process of writing data for different BGP messages format is done in this class. In the following code snippet we can see the implementation for storing message marker, message type and message length in temporary buffer byte array to use as data stream. This stream of the data is stored in the buffer and this buffer is used while sending the actual message packet. 








Figure 2: Writing the message marker, length and type in the temporary buffer byte array

Similarly, the message specific data like e.g. version, hold time, BGP identifier, optimal parameter length, withdrawn routes, network layer reachability information (NLRI) prefix, attribute, path segment e.t.c. are also written in the buffer using message structure class. In the following code you can see the implementation of these functionality in code;









Figure 3: Codes for writing message data inside temporary buffer of the packet 
#### BGP Open Message
BGP open message class inherits the abstract message structure class to construct the open message packets in this implementation. All the related additional information of this message is supplied through this class. According to BGP documentation, I have used BGP version, AS info, BGP Identifier IP address, hold time and optimal parameter length. Open message is constructed to have a length of 40 octets. All the different data of open message are stored in different memory location by defining the slots for the byte array. Which makes it easier to track the particular data from the particular slot while handling the message. Both BGP listeners and BGP speakers use the open message constructor to send open message to each other. In the following section you can see the code which uses the supplied values to the variables and sends it to the message structure class to store the information in the byte array; 

 





Figure 4: Construction of open message and storing it to the particular slot of byte array.

#### Keep Alive Message

Similarly keep alive is a short 19 octet message used by BGP system to keep the TCP connection alive between listener and the speaker with the help of keep alive timer event of FSM. Keep alive message implementation consists the BGP message header and the message type information. For its implementation only message header and message type information is written in the buffer array. Normally when the BGP listener receives the open message from the speaker it sends keep alive message to keep the connection alive. At the same time the keep alive timer of the state machine is set and when the timer is expired it also triggers the keep alive message. In the following code we can see the implementation;




Figure 5: Keep alive message implementation

#### Update Message

It was little bit complicated message because it has quite a lot of routing information which should be dynamically created within the AS during run time. In BGP system update message is used to exchange network reachability information within same AS. BGP speaker is responsible for the advertisement and enforcement local policies to its peer. It is a long message with 184 octet length with many information’s like; withdrawn route length, withdrawn route, IP prefix length, IP prefix, total path attribute length, attribute flag, type code, attribute, path segment type, path segment length, path segment value, NLRI length, NLRI prefix e.t.c. In the following code snippet I have tried to show, its implementation in the project;






Figure 6: Writing update variables in temporary buffer array

#### Notification Message
Similarly notification message is a short 21 octet message used to notify the BGP system when some errors happens within the same AS. Normally notify consists of error code, error sub code and error information data. In the following code we can see an example implementation of this particular message;





Figure 7: Implementation of notification message

#### Reading the data from byte array (Packet Handler Class)
According to my implementation all the data of the packets are stored in the temporary byte array and it is important to know the particular slot of the array to find the particular information. During this portion of the implementation I struggled with the different byte size in different characters of IP address information. Since all of my implementation of IP address has 6 characters except one with 7 characters (127.3.0.10). This one character difference made the code unable to read the IP address properly. It took around 3 hours for me to find this particular problem and read that IP address. As the requirement of this project was to use 10 routers and it was a dynamic implementation so that I used all the IP address with 6 characters starting from 127.1.0.0. In this implementations to read data from all four different BGP message stored in byte array I have used static packet handler class. In the following example code I have shown the implementation of update message;






Figure 8: To read the data from byte array

In the following section I have shown all four different kinds of BGP message received by the socket during the software execution. In the following screen shot we can see those messages;
 
 
 
 
Figure 9: All four different message received by the sockets.

###	State Machine
All the BGP state of the routers are assigned and handled through state machine class. All the BGP timers like; keep alive timer, connection retry timer, hold timer and time information like; connection time, keep alive time, hold time are set and handled through this class. When the timer is set in this class the respective 3 events handlers for the timers are also implemented to perform the desired operation when the timer expires.  I have also define administrative events like automatic start event and automatic stop events and event handlers to start and stop the BGP system initialization. Similarly, the TCP events like TCP connection acknowledged event, TCP connection conformed event and TCP connection failed event are implemented with their handlers. Also, all the BGP message events and their handlers like BGP open message event, BGP open message received event, BGP header error event, BGP open message error event, BGP notify message error event, BGP notify message event, BGP keep alive message event, BGP keep alive message error event, BGP update message event and BGP update message error event. All of these timers, TCP connection and BGP message events should be implemented separately for each BGP peer connection. This also has increased the complexity of the project because it is mandatory according to BGP protocol documentation. In the following code snipped we can see some of the examples of those event handlers implementation;








Figure 10: Event handler’s implementation in state machine class

#### Finite State Machine
It is the most important class in this project as, it is the one which is responsible for maintaining as separate state and events for all 10 different routers. According to the BGP documentation it is necessary for BGP to maintain the separate FSM for each connected peers. FSM class inherits state machine class because it needs all of those events for handling BGP connection and sending BGP message according to those events and state to the connected peers. It is the heart of BGP implementation and it should have track of all timer, TCP connection, and BGP message events. All of these events should be triggered and handled according to the state of routers. Due to that reason this class can be considered as the backbone of this implementation. For example when the program starts the BGP listener/speaker and they are online the FSM should change their state to idle. Then it should give a signal to the BGP speaker to start sending TCP connection request to the listener. If the TCP connection between the listener and speaker is successful then the FSM should change their state to connect state. It also needs to find out if the connection is internal or external according to the AS information of listener and speaker. After that it should signal the BGP speaker to send the open message to the listener and change the speaker state to active. When the listener receives the open message, it should send the open and keep alive message to the listener and FSM should change its state to open conformed. Finally when the speaker receives both message then the connection is changed to established state. Since all of these connection are in different thread and they are key information for BGP protocol. Due to that reason it is stored in the static class as a global variables to access them whenever they are needed. In case of error situation FSM is also responsible for error handling and it should update the status to connected peers according to the error. In the following section of this document I will be explaining about the implementations of idle state, connect state, active state, open state, open conform state and established state of finite state machine class.
According to the protocol automatic state event of the finite state machine is triggered when the routers are initialized and online to accept the peer connection. At this stage FSM initializes the BGP resources to perform the connection. First of all the connection retry counter is set to zero, connection retry timer is set to 2 minutes because it should be enough to initiate TCP connection to other BGP peers. BGP speaker sends the TCP connection request to the listener and listener listens to these connection request from remote peer. All the other events like; event 8, events 9-12 and events 15-28 (RFC-4271) are not processed during this state. During this stage listener will process the connection request and FSM changes the state of the connection to connect state. At this state any other events triggered will not have any effect in the local system. In the following code you can see the implementation of this particular operation; 








Figure 11: Idle state implementation in FSM class
With the above code implementation, when the sockets are online they are detected by FSM and their connection state is set to idle. In the following screen shot you could be able to see this implementation for listener sockets when they are online.
 
Figure 12: Socket information with IP address and their state information
In the connect state BGP FSM waits for the TCP connection to be completed and all the others events, 1 and 3-7 are ignored. If the connection retry timer expires at this state then FSM drops the TCP connection and restarts the connection retry timer and it initiates a TCP connection and starts listening the connection request from other remote peer. But even though the connection retry timer has expired during this process but it still remains in connect state. Similarly, if the TCP connection succeeds it identifies connection type and then it resets the connection retry timer and stays in connect state. In a situation where the TCP fails, it rests the connection retry timer and continues to listen for the connection request from other BGP peers changing its connection state to active. As a response to other error events like BGP message header error, open message error, notification message error, and timer errors are handled accordingly. In the following example I am trying to show the implementation of TCP connection succeed event, connection retry timer expires event and TCP connection failed event;







Figure 13: Connect state implementation of FSM class
Similarly following screen shot will show all the speaker and listener sockets which are online and has a TCP connection between them and the finite state machine has updated the state information along with the information of type of connection between them.
 
Figure 14: Connection between the listeners and speakers with their connection type. 
During the active state FSM checks the connection of all the BGP peers which has a successful TCP connection with each other. When the connection is successful then FSM changes the state of BGP speakers to active state. Then it signals the BGP speaker to send the open message. After that when the BGP Listener receives the open message send by BGP speaker it triggers the open message received event of FSM which changes the state information of speaker to open sent. Then in response to that event in active state FSM triggers event to listener for sending both open and keep alive message to the speakers. Other error situations are also implemented in this state but I have not explained them because I am concentrating the explanation for error free situations. It was another difficult part of my implementation to track the particular connection between connected speakers and listeners. Since I was using the asynchronous sockets for both speaker and listener which requires the connection track also in asynchronous order. According to the BGP requirements all of the listeners must handle multiple connection and in C# Socket API it was possible only through asynchronous sockets. It was the main reason to use asynchronous sockets in this project. After spending quite a long time I realised that due to asynchronous nature of the application it is better to use static global variable class to store these information in dictionary and retrieve it whenever it is necessary. As all the process of creating socket, binding socket, sending the connection request, receiving the message and sending message will be discussed in the upcoming section. But in the following code snipped I have tried to show the code where the state information assigned to the connection for different conditions in FSM;





Figure 15: FSM implementation in the active connection state
Even though it is few lines of code but this information has been used in other classes and quit a lot of similar information can be found in FSM. Due to similar approach in FSM class it was possible to allocate separate FSM instance for 10 listeners and 14 speakers, to track and use 14 different connection according to BGP requirements. In the following section I will show some of the open message send by the speaker that has been received by the listener. Since I am using asynchronous sockets so I do not have a control to bring the sockets online and process whole execution one by one. Which has an effect on the order of the execution all the routers will be online asynchronously and other BGP related methods are also executed randomly. But when the first requirement for the BGP setup is meet then the execution proceeds to the second requirements and so on. It can be seen in my project after application execution. In the following console output you can see state information and BGP open message send by speaker and listener;
 
Figure 16: BGP open message send and received by listener and speaker in connect state  
 FSM has another important state called open sent, which consists different kinds of error handling during error situation. But I am going to skip the explanations of those error handling and move my explanation for the normal functioning situation. Basically at this state when the BGP speaker receives the open message send by BGP listener, FSM changes the state of the speaker to open conformed state. In the following code I shown the implementation of open message received event condition and some of the error handling code at this state.





Figure 17: Implementation of open sent state of FSM
In the following console window I have tried to show this particular situation when the speaker receives the open message from the listener. All of these information have been dynamically generated by the program and it is printed when the execution completes the process. Same listener socket can receive message from different speaker in unpredictable sequence because in asynchronous methods request are send and processed asynchronously. In the following picture there are open message received by speakers which are not in the perfect order of speaker IP address; 
 
Figure 18: Output of the program execution when speaker’s state is changed to open conformed 
At this point both speaker and listener are already in open conformed state. But if there is some error events like listener or speaker goes offline or connection hold time expires then the respective events in FSM is triggered. According to error occur FSM changes the state information and it performs operation in response to the error. I am going to show these two error handling in the following code;




Figure 19: Open conformed error situation code implementation
Finally in FSM there is last connection state called established, when all of the routers in the BGP system are in the established state they are ready for update message handling. According to BGP documentation when BGP speaker receives the keep alive message send by the listener, FSM changes the connection state to established state. Similarly in the established state of the connection BGP speaker sends the update message to the listener. Also in the error situations like when the connection is established but the keep alive timers expires then the FSM should trigger the listener to send keep alive message to speaker. In the following section we will be able to see particular portion in FSM class where the state information is processed;

    



Figure 20: Connection Established State implementation in FSM class
As I have mentioned in the above section when the speaker receives the keep alive message FSM triggers the event and updates the state information. In the following screen shot I am trying to show the output when the speaker receives Keep alive message and FSM updates its state information accordingly;
  
Figure 21: Keep alive message received by Speaker and their connection state 
Similarly in the error situation where the connection state is established but connection keep alive timer expires then FSM is responsible to reset the timer and trigger the listener to send keep alive message to the speaker. In the following figure I have shown the output when the timer expires and listener sends keep alive message;
  
Figure 22: Keep alive timer expired and keep alive message send by listener 

### Routers
During the implementation of the routers I have realised that it would have been easier for this project to use higher programming platforms like NS3. It was difficult to implement routers from listener and speaker sockets. Due to the additional complexity to implement the default gateway for those sockets to work as a single router. In the beginning I was confused about the implementation of routers but after spending some time in sockets API, I realised that it might be possible. There was a possibility to have same IP address for different sockets if the port number is not same which made my task little bit easier. So, that it was possible to bind more than one sockets into one IP address. And without implementing default gateway address I was able to use same IP in different sockets with different port. With this flexibility I was able to define port 179 for BGP listener and other ports for required speaker. In router class I have defined both speaker and listener socket that uses IPV4 network protocol, transfers data stream and follows TCP connection protocol. I have bind all listeners to port 179 to address BGP requirements. Similarly, using asynchronous socket I was able to have a listener capable to have multiple connection as specified by BGP. Due to that reason only 10 listener sockets with different IP address were enough for the project implementation. But in case of speaker socket it was not possible to have same socket for multiple connection initiation even though it is asynchronous socket. Which has resulted to create 14 sockets with 10 different IP address and up-to 3 different port for same IP. I am going to explain about this implementation in initialization of listener and speaker section. But router class is a general class which sets the parameter to define listener and speaker sockets. Actually for this implementation I have used the byte array of 1024 byte to store data stream of BGP message. In the following section of the code we can see the declaration and binding process of the sockets;





    


Figure 23: Code implementation for creating the routers with speaker and listener socket
#### Listener Socket
Since, listener socket inherits the router class and socket is already defined and bind in router class. Therefore in listener socket, listener specific methods like listen, accept, send method are defined in listener class. In asynchronous sockets accept and send method have their own call-back methods called accept call-back and send call-back. In the accept call-back method after the connection is accepted, received call-back method is invoked to receive the message send by the speaker. Listen method helps the socket to listen the incoming connection request. When the socket receives the connection request it calls the accept method. Then accept method triggers the accept call-back method, and the connection is accepted in accept call-back. When the connection is accepted it invokes receive call-back method to receive messages. To send the message send method is triggered which invokes send call-back method to initiate sending message. In the following section I am going to present listen and accept methods of this class;
       





Figure 24: Listener socket listen and accept method
Similarly in the following code I have shown the implementation of accept call-back, receive call-back, send and send call-back methods. As we can see the try catch statement is use to catch the error occurred during this process.











Figure 25: Call-back events implementations of listener socket class.
#### Speaker Socket
AS I have mentioned in previous step speaker socket also inherits the router class to use socket, bind socket to an IP address and to define a port number. In my application speaker port starts from 176 and I have used ports until 178 for creating 14 different speaker. Send method, send call-back method and received call-back method are similar to listener sockets. But speakers are the one to initiate connection that’s why they have connect and connect call-back methods. When connect method is called it triggers connect call-back method and if the connection is complete then it calls received call-back method. When it receives message it sends that packet to packet handler with socket information to print it in human readable format. The socket is passed to the packet handler class so that I can track both the end point of the connection. Which can be seen in printed messages with both the sending and receiving end IP address information. In the following section of the code I will show my implementation of connect and connection call-back method;









Figure 26: Connect and connect call-back method of speaker socket class
### Initialization of Listener and speaker 
It was another difficult task to implement all the listener and speaker socket to proper IP address and port number so that they can work as one router. On top of that all the routers in one AS were supposed to be in the mesh connection topology. Initially I tried to implement it as a dynamic code where if we define the numbers of router then the initialization and connection of the sockets is done by the program. But it was complex architecture it increased the difficulty level of this implementation. Due to that reason I have done static implementation of the sockets and their connection. It would have been better to have the dynamic implementation for the connection according to the numbers of routers in the AS, which leaves a space for the further development of this project. In this static implementation I have 10 listener sockets with different IP address and same port 179. Similarly I have 14 speaker sockets because in the mesh connection topology I need 14 different connection and in the same IP address there might be 1 to 3 speaking socket with different port ranging from 176 to 178. All this routers are divided into 3 AS and their IP prefixes are 127.1, 127.2 and 127.3. Since there were 3 AS and for simplicity I have implanted only 2 inter AS connection in the implantation. In the following figure I have shown my structure of the routers in 3 different AS.  
	R0: 127.1.0.0	R3: 127.2.0.3              R4: 127.2.0.4                                                       R7: 127.3.0.7



R1: 127.1.0.1             R2: 127.1.0.2                                        R5: 127.2.0.5            R6: 127.2.0.6                                    R8: 127.3.0.8            R9: 127.3.0.9
AS1: 127.1	AS2: 127.2	AS3: 127.3
Figure 27: Autonomous systems and their connection 
First of all I have initialized 10 instances of listener socket class to define 10 different IP address and port number. While defining these two information I have already divided the listeners IP address according to different AS IP prefix. The code which binds the socket is inside the loop so that I can create 10 listener sockets. This portion of the implantation can be seen in the following code;


 



Figure 28: Listener socket initialization process
When this code is executed all the listener sockets are created and they will be online to listen and accept the connection request send by the speaker. Following console output shows the result when the application is executed; 

 Figure 29: Output when the listener sockets are online
 
To address the project requirement I have initialized the 14 instance of speaker sockets to bind it to its proper IP and port address. It has followed similar steps as in listener for its implantation but it has 14 different sockets because it needs to initiate 14 different connection request. In the following section of the code I am showing its implementation in the project;












Figure 30: Speaker socket initialization
When these sockets are initialized and the code is executed the console window will have the following output as its result;
 
Figure 31: Speaker initialization output result
After all of the listener and speaker are online the next step is to make a connection request from the speaker. As the requirement of the project there should be 14 different connection request with two external connection request. It was another difficult task to track the particular connection between the listener and the speaker so I have stored it in global variable. I have achieved this by the help of following lines of codes;














Figure 32: Speakers connection request implementation
As a result of above mentioned connection code implementation all listeners and speakers will be connected in 14 different connection as explained in initialization of speaker, listener and AS section. Due to the asynchronous nature of the sockets and its connection in the following figure we can see that some routers are still getting connected and some has already send the open message. This kind of result can be seen in other section also. But if I have done 10 different console application for individual routers then this problem could have been fixed. In the following figure I have shown this result;
 
Figure 33: Connection information between the listener and the speaker with its connection type

### Implementing Routing Table
As stated on BGP protocol when all the listener and speaker were online, connected and connection state was established with FSM individual connection state. The project was ready for routing table implementation for individual AS. Since all the routers were in same console application that’s why it was easy to save all routers with established connection state in the dictionary of the global variable class. According to the BGP protocol, an AS keeps the routing information of the internal BGP peers and its external peer routing information. My implementation satisfies this condition in all 3 AS’s. I will show that in the upcoming sections. In the following code you can see the formation of routing table information retrieving the values like; connection number, IP address, AS number, next hop address, and connection type. The actual implementation about setting those values will be discussed in update message handling section. I am trying to show this process in the following code;









Figure 34: Complete routing table implementation

To get the routing table first of all we need to wait until all the connection are in established state. After that we call the update method to set adjacent RIB Out, path attribute, NLR, and path segment values. Then as we call the respective method for different AS’s it show’s their routing table information. This implementation is in program class within the forever loop because the program should facilitate this feather for ever.  Since it is handled by the administrator and he should enforce local policy any time. To enforce local policy I have implemented update feather which helps to update policies according to AS needs. But path vector matrix and trust voting implementation are missing which might be most important recommendation for further development. In the following figure I am going to show this update and routing table display method implementation;   





Figure 35: Update and AS info implementation in program class

In the following figure you can see that first I have updated the local policy and then used “as2” to view routing table information of AS2.
 
Figure 36: update command and as2 command with their output

### Update Message Handling
This was another complicated implementation in this project because all the parameters of the update message should be dynamic according to those 14 different connection. BGP speakers are the one who is responsible to enforce the routing policy within the AS. This implementation also needs to have the track of external connection between two AS and send the reachability information accordingly to the internal BGP peer. There is also the implementation to store Adj-RIB-Out, NLRI, path attribute, path segment and withdrawn routes information in global variables. But in this section of this document, I will be showing the update message handling in AS3 only. In the following code we can see that I have retrieved all the necessary information like NRLI, path segment, path attribute and adjacent-RIB-Out. Then I have send an update message to the listener using update message class. In the following figure we will be seeing the implementation of update message handling of AS3;







Figure 37: Update message handling for AS3 in update message handling class
First of all in the implementation we should update the local policies to get routing table information by typing “update” command in console window. Then if we type “as3” we will be able to see the routing table of AS3. Then if we give a command “updateAS3” then it sends the update message to other routers. Withdrawn route field is empty because all the routers are online. In the following figure I have shown this implementation.
 
Figure 38: AS3 routing table and respective update message in same AS3
### Close Router
This class is created to shut down the TCP connection between the BGP peer. Actually in this implementation there are 10 routers with 24 sockets and 14 TCP connection. May be this numbers also will define little bit about the sensitivity of its implementation. It was another difficult task of this project implementation. Actually I ran into one tricky problem to track the particular connection of the specific router when it was supposed to shut down.  But after having the IP address of the router which is shutdown it was possible to track the connection and implement the notification message handling. According to BGP document all of these 14 connection should be at established state for sending the notification message. Since, when the socket is initialized and functional in one thread and if we try to shut it down from remote thread it throws the exception. Due to that reason I haven’t shut down the actual router but I have removed all the stored information of that particular router from global variable dictionary and the TCP connection is not actually closed. While using the dictionary in the global variable class I faced a problem where the same dictionary was accessed by the multiple threads of application and I was having problems to achieve this feature. After spending some time with this problem I learned that in C# there is a thread safe dictionary called concurrent dictionary. This concurrent dictionary of C# made this whole project possible to implement. The process to achieve this feature can be seen in the following code. Where I have removed all the stored information of that IP address from the global variable class.












Figure 39: Shutting down listener and speaker
Using the above mentioned implementation I was able to provide this feature to close the routers in all three different AS’s. But actually I have written to close three routers with IP addresses “127.1.0.0, 127.2.0.5, and 127.3.0.9”. Because if I close the routers which has the connection with external AS router then some other router needs to initiate external connection to the router of other AS’s. Due to its complexity I have skipped that part in this implementation but it should be implemented in the further development. In the existing implementation if we type any of the above mentioned IP address e.g. “127.1.0.0” in console and press enter then it closes that particular router. In the following section we can see the result when this particular router is closed and then BGP speaker sends the notification message to its peer. Then if we type “updateAS1” command BGP speaker sends the update message with the withdrawn route information to its peer. But we need to update the routing table first and send the updateAS1 command to the system. It is shown in the following figure;
   
Figure 40: Notify and update message when the router is closed

###	Program class
It is the entry point for the execution of the whole project and its execution starts from main method of this class. Due to that reason I have initiated FSM class inside the main method. Also the automatic start event of the FSM class is triggered to start all the speakers and listeners initialization and connection process. When the TCP connection is successful, FSM will execute other BGP messaging process as I have mentioned in the above sections of this document. Similarly when the BGP connection is established routes and update message handling class is also initialized to implement them in the forever loop. All the necessary command are also inside this forever loop because those commands should be usable for all the time after the program execution. All the sockets initialization is also done through this main method to make them responsive all the time after creating them. This simple implementation is shown in the following code. 
 










Figure 41: Program class implementation

###	Global variables class

Due to all of the above explained reasons the project required a static global variable class to store all the required variables values. The most important concurrent dictionary I have used to store these key information’s are speaker_AS, listner_AS, conAnd_Listner, con_And_Speaker, speakerConAnd_AS, listnerConAnd_AS, listnerSocket_Dictionary, speakerSocket_Dictionary, conSpeakerAs_ListnerAs, Adj_RIB_Out, NLRI, pathAttribute, withdrawlRoutes, pathSegment, interASConIP etc.  In the following code example I am showing the initialization of all of these global dictionary; 

        





Figure 42: Global dictionary declaration in global variable class


### Further improvement
All the basic implementation of the BGP protocol have been implemented in this project. But some of the tricky portion like implementing path vector, trust between the routers, routes aggregation, all the different BGP error handling are still missing in this project. The implementation of sending actual IPV4 packet with the destination address and the actual part where routers of AS uses BGP protocol to route the packet to the destination address is also missing. In the same way as I have mentioned above the dynamic initialization of sockets according to the numbers of router in mesh topology might be important part improve the project. I have written the notification message to handle all the error situation and FSM is also ready to trigger those necessary events but I have not implemented for all of those situation. Similarly the portion where we need to shut down the router from the foreign host of same AS, has not been implemented exactly according to the BGP documentation. These are the most important sectors for the further development of this project.

