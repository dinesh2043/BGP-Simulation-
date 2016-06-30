using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/**
6.  BGP Error Handling.
        This section describes actions to be taken when errors are detected while processing BGP messages.
        When any of the conditions described here are detected, a NOTIFICATION message, with the indicated Error Code, Error Subcode,
   and Data fields, is sent, and the BGP connection is closed (unless it is explicitly stated that no NOTIFICATION message is to be sent and
   the BGP connection is not to be closed).  If no Error Subcode is specified, then a zero MUST be used.
        The phrase "the BGP connection is closed" means the TCP connection has been closed, the associated Adj-RIB-In has been cleared, and all
   resources for that BGP connection have been deallocated.  Entries in the Loc-RIB associated with the remote peer are marked as invalid.
   The local system recalculates its best routes for the destinations of the routes marked as invalid.  Before the invalid routes are deleted
   from the system, it advertises, to its peers, either withdraws for the routes marked as invalid, or the new best routes before the
   invalid routes are deleted from the system.
        Unless specified explicitly, the Data field of the NOTIFICATION message that is sent to indicate an error is empty.
6.1.  Message Header Error Handling
        All errors detected while processing the Message Header MUST be indicated by sending the NOTIFICATION message with the Error Code
   Message Header Error.  The Error Subcode elaborates on the specific nature of the error.
        The expected value of the Marker field of the message header is all ones.  If the Marker field of the message header is not as expected,
   then a synchronization error has occurred and the Error Subcode MUST be set to Connection Not Synchronized.
        If at least one of the following is true:
            - if the Length field of the message header is less than 19 or greater than 4096, or
            - if the Length field of an OPEN message is less than the minimum length of the OPEN message, or
            - if the Length field of an UPDATE message is less than the minimum length of the UPDATE message, or
            - if the Length field of a KEEPALIVE message is not equal to 19, or
            - if the Length field of a NOTIFICATION message is less than the minimum length of the NOTIFICATION message,
        then the Error Subcode MUST be set to Bad Message Length.  The Data field MUST contain the erroneous Length field.
        If the Type field of the message header is not recognized, then the Error Subcode MUST be set to Bad Message Type.  The Data field MUST
   contain the erroneous Type field.
6.2.  OPEN Message Error Handling
        All errors detected while processing the OPEN message MUST be indicated by sending the NOTIFICATION message with the Error Code
   OPEN Message Error.  The Error Subcode elaborates on the specific nature of the error.
        If the version number in the Version field of the received OPEN message is not supported, then the Error Subcode MUST be set to
   Unsupported Version Number.  The Data field is a 2-octet unsigned integer, which indicates the largest, locally-supported version
   number less than the version the remote BGP peer bid (as indicated inthe received OPEN message), or if the smallest, locally-supported
   version number is greater than the version the remote BGP peer bid, then the smallest, locally-supported version number.
        If the Autonomous System field of the OPEN message is unacceptable, then the Error Subcode MUST be set to Bad Peer AS.  The determination
   of acceptable Autonomous System numbers is outside the scope of this protocol.
        If the Hold Time field of the OPEN message is unacceptable, then the Error Subcode MUST be set to Unacceptable Hold Time.  An
   implementation MUST reject Hold Time values of one or two seconds. An implementation MAY reject any proposed Hold Time.  An
   implementation that accepts a Hold Time MUST use the negotiated value for the Hold Time.
        If the BGP Identifier field of the OPEN message is syntactically incorrect, then the Error Subcode MUST be set to Bad BGP Identifier.
   Syntactic correctness means that the BGP Identifier field represents a valid unicast IP host address.
        If one of the Optional Parameters in the OPEN message is not recognized, then the Error Subcode MUST be set to Unsupported Optional Parameters.
        If one of the Optional Parameters in the OPEN message is recognized, but is malformed, then the Error Subcode MUST be set to 0 (Unspecific).
6.3.  UPDATE Message Error Handling
        All errors detected while processing the UPDATE message MUST be indicated by sending the NOTIFICATION message with the Error Code
   UPDATE Message Error.  The error subcode elaborates on the specific nature of the error.
        Error checking of an UPDATE message begins by examining the path attributes.  If the Withdrawn Routes Length or Total Attribute Length
   is too large (i.e., if Withdrawn Routes Length + Total Attribute Length + 23 exceeds the message Length), then the Error Subcode MUST
   be set to Malformed Attribute List.
        If any recognized attribute has Attribute Flags that conflict with the Attribute Type Code, then the Error Subcode MUST be set to
   Attribute Flags Error.  The Data field MUST contain the erroneous attribute (type, length, and value).
        If any recognized attribute has an Attribute Length that conflicts with the expected length (based on the attribute type code), then the
   Error Subcode MUST be set to Attribute Length Error.  The Data field MUST contain the erroneous attribute (type, length, and value).
        If any of the well-known mandatory attributes are not present, then the Error Subcode MUST be set to Missing Well-known Attribute.  The
   Data field MUST contain the Attribute Type Code of the missing, well-known attribute.
        If any of the well-known mandatory attributes are not recognized, then the Error Subcode MUST be set to Unrecognized Well-known
   Attribute.  The Data field MUST contain the unrecognized attribute (type, length, and value).
        If the ORIGIN attribute has an undefined value, then the Error Sub-code MUST be set to Invalid Origin Attribute.  The Data field MUST
   contain the unrecognized attribute (type, length, and value).
        If the NEXT_HOP attribute field is syntactically incorrect, then the Error Subcode MUST be set to Invalid NEXT_HOP Attribute.  The Data
   field MUST contain the incorrect attribute (type, length, and value).Syntactic correctness means that the NEXT_HOP attribute represents a
   valid IP host address.
        The IP address in the NEXT_HOP MUST meet the following criteria to be considered semantically correct:
            a) It MUST NOT be the IP address of the receiving speaker.
            b) In the case of an EBGP, where the sender and receiver are one IP hop away from each other, either the IP address in the
         NEXT_HOP MUST be the sender's IP address that is used to establish the BGP connection, or the interface associated with
         the NEXT_HOP IP address MUST share a common subnet with the receiving BGP speaker.
        If the NEXT_HOP attribute is semantically incorrect, the error SHOULD be logged, and the route SHOULD be ignored.  In this case, a
   NOTIFICATION message SHOULD NOT be sent, and the connection SHOULD NOT be closed.The AS_PATH attribute is checked for syntactic correctness.  If the
   path is syntactically incorrect, then the Error Subcode MUST be set to Malformed AS_PATH.
        If the UPDATE message is received from an external peer, the local system MAY check whether the leftmost (with respect to the position
   of octets in the protocol message) AS in the AS_PATH attribute is equal to the autonomous system number of the peer that sent the
   message.  If the check determines this is not the case, the Error Subcode MUST be set to Malformed AS_PATH.
        If an optional attribute is recognized, then the value of this attribute MUST be checked.  If an error is detected, the attribute
   MUST be discarded, and the Error Subcode MUST be set to Optional Attribute Error.  The Data field MUST contain the attribute (type,
   length, and value).
        If any attribute appears more than once in the UPDATE message, then the Error Subcode MUST be set to Malformed Attribute List.
        The NLRI field in the UPDATE message is checked for syntactic validity.  If the field is syntactically incorrect, then the Error
   Subcode MUST be set to Invalid Network Field.
        If a prefix in the NLRI field is semantically incorrect (e.g., anunexpected multicast IP address), an error SHOULD be logged locally,
   and the prefix SHOULD be ignored.
        An UPDATE message that contains correct path attributes, but no NLRI, SHALL be treated as a valid UPDATE message.
6.4.  NOTIFICATION Message Error Handling
        If a peer sends a NOTIFICATION message, and the receiver of the message detects an error in that message, the receiver cannot use a
   NOTIFICATION message to report this error back to the peer.  Any such error (e.g., an unrecognized Error Code or Error Subcode) SHOULD be
   noticed, logged locally, and brought to the attention of the administration of the peer.  The means to do this, however, lies
   outside the scope of this document.
6.5.  Hold Timer Expired Error Handling
        If a system does not receive successive KEEPALIVE, UPDATE, and/or NOTIFICATION messages within the period specified in the Hold Time
   field of the OPEN message, then the NOTIFICATION message with the Hold Timer Expired Error Code is sent and the BGP connection is
   closed.


**/

namespace BGPSimulator.BGP
{
    public class BGPErrorHandling
    {
    }
}
