using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Web.WebSockets;
using Newtonsoft.Json;

namespace Wamplash
{
    public static class MessageTypes
    {
        public const int Hello = 1;
        public const int Welcome = 2;
        public const int Abort = 3;
        public const int Challenge = 4;
        public const int Authenticate = 5;
        public const int Goodbye = 6;
        public const int Heartbeat = 7;
        public const int Error = 8;
        public const int Publish = 16;
        public const int Published = 17;
        public const int Subscribe = 32;
        public const int Subscribed = 33;
        public const int Unsubscribe = 34;
        public const int Unsubscribed = 35;
        public const int Event = 36;
        public const int Call = 48;
        public const int Cancel = 49;
        public const int Result = 50;
        public const int Register = 64;
        public const int Registered = 65;
        public const int Unregister = 66;
        public const int Unregistered = 67;
        public const int Invocation = 68;
        public const int Interrupt = 69;
        public const int Yield = 70;
    }

    public interface IRequest
    {
        long RequestId { get; set; }
    }

    public interface ITopic
    {
        string Topic { get; set; }
    }

    public interface ISession
    {
        long SessionId { get; set; }
    }

    public interface IDetails
    {
        dynamic Details { get; set; }
    }

    public interface IRealm
    {
        string Realm { get; set; }
    }

    public interface ISubscription
    {
        long SubscriptionId { get; set; }
    }

    public interface IPublication
    {
        long PublicationId { get; set; }
    }

    public abstract class WampMessage
    {
        public abstract int MessageId { get; }
    }

    public class HelloMessage : WampMessage, IRequest, IRealm, IDetails
    {
        public HelloMessage(dynamic json)
        {
            Realm = json[1];
            if (json.Count > 2)
                Details = json[2];
        }

        public override int MessageId
        {
            get { return MessageTypes.Hello; }
        }

        public dynamic Details { get; set; }
        public string Realm { get; set; }
        public long RequestId { get; set; }
    }

    public class WelcomeMessage : WampMessage, ISession, IDetails
    {
        public override int MessageId
        {
            get { return MessageTypes.Welcome; }
        }

        public dynamic Details { get; set; }
        public long SessionId { get; set; }

        public override string ToString()
        {
            var jd = JsonConvert.SerializeObject(Details);
            return "[" + MessageId + ", " + SessionId + ", " + jd + "]";
        }
    }

    public class EventMessage : WampMessage, ISubscription, IPublication, IDetails
    {
        public override int MessageId
        {
            get { return MessageTypes.Event; }
        }

        public dynamic Details { get; set; }
        public long PublicationId { get; set; }
        public long SubscriptionId { get; set; }

        public override string ToString()
        {
            return "[" + MessageId + ", " + SubscriptionId + ", " + PublicationId + ", {}, " + Details + "]";
        }
    }

    public class SubscribeMessage : WampMessage, IRequest, ITopic
    {
        public SubscribeMessage(dynamic json)
        {
            RequestId = json[1];
            Topic = json[3];
        }

        public override int MessageId
        {
            get { return MessageTypes.Subscribe; }
        }

        public long RequestId { get; set; }
        public string Topic { get; set; }
    }

    public class SubscribedMessage : WampMessage, IRequest, ISubscription
    {
        public SubscribedMessage(long requestId, long subscriptionId)
        {
            RequestId = requestId;
            SubscriptionId = subscriptionId;
        }

        public override int MessageId
        {
            get { return MessageTypes.Subscribed; }
        }

        public long RequestId { get; set; }
        public long SubscriptionId { get; set; }

        public override string ToString()
        {
            return "[" + MessageId + ", " + RequestId + ", " + SubscriptionId + "]";
        }
    }

    public class UnsubscribeMessage : WampMessage, IRequest, ISubscription
    {
        public UnsubscribeMessage(dynamic json)
        {
            RequestId = json[1];
            SubscriptionId = json[2];
        }

        public override int MessageId
        {
            get { return MessageTypes.Unsubscribed; }
        }


        public long RequestId { get; set; }
        public long SubscriptionId { get; set; }
    }

    public class UnsubscribedMessage : WampMessage, IRequest
    {
        public override int MessageId
        {
            get { return MessageTypes.Unsubscribed; }
        }

        public long RequestId { get; set; }

        public override string ToString()
        {
            return "[" + MessageId + ", " + RequestId + "]";
        }
    }

    public class PublishMessage : WampMessage, IRequest, ITopic, IDetails
    {
        public PublishMessage()
        {
        }

        public PublishMessage(dynamic json)
        {
            RequestId = json[1];
            Acknowledge = json[2].acknowledge;
            ExcludeMe = json[2].exclude_me;
            Topic = json[3];
            Details = json[4];
        }

        public bool ExcludeMe { get; set; }
        public bool Acknowledge { get; set; }

        public override int MessageId
        {
            get { return MessageTypes.Publish; }
        }

        public dynamic Details { get; set; }

        public long RequestId { get; set; }
        public string Topic { get; set; }
    }


    public delegate void WampMessageHandler<in TMessage>(TMessage message) where TMessage : WampMessage;


    public abstract class WampWebSocketHandler : WebSocketHandler
    {
        public abstract List<Role> Roles { get; }

        public void Send(WampMessage message)
        {
            var messageString = message.ToString();
            Send(messageString);
        }

        public event WampMessageHandler<HelloMessage> Hello;
        public event WampMessageHandler<SubscribeMessage> Subscribe;
        public event WampMessageHandler<PublishMessage> Publish;
        public event WampMessageHandler<UnsubscribeMessage> Unsubscribe;
        public event WampMessageHandler<EventMessage> Event;

        public void RaiseEvent(EventMessage eventMessage)
        {
            if (Event != null)
                Event(eventMessage);
        }


        public string FindConstantName<T>(Type containingType, T value)
        {
            var comparer = EqualityComparer<T>.Default;
            return (from field in containingType.GetFields(BindingFlags.Static | BindingFlags.Public)
                where field.FieldType == typeof (T)
                      && comparer.Equals(value, (T) field.GetValue(null))
                select field.Name)
                .FirstOrDefault();
        }

        public override void OnMessage(string message)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(message);
            int messageType = json[0];
            var messageName = FindConstantName(typeof (MessageTypes), messageType);
            var messageT = Assembly.GetExecutingAssembly().GetTypes().Single(t => t.Name == messageName + "Message");
            var m = Activator.CreateInstance(messageT, json);
            var eventDelegate =
                (MulticastDelegate)
                    typeof (WampWebSocketHandler).GetField(messageName, BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(this);
            if (eventDelegate != null)
                eventDelegate.Method.Invoke(eventDelegate.Target, new object[] {m});
            else throw new Exception("Message not implemented : " + messageType);
        }
    }

    public static class RoleCodes
    {
        public const string Publisher = "publisher";
        public const string Broker = "broker";
        public const string Subscriber = "subscriber";
        public const string Caller = "caller";
        public const string Dealer = "dealer";
        public const string Callee = "callee";
    }

    public class FeatureAttribute : Attribute
    {
        public FeatureAttribute(string featureCode)
        {
            FeatureCode = featureCode;
        }

        public string FeatureCode { get; set; }
    }


    [Feature("caller_identification")]
    public interface ICallerIdentification
    {
        bool CallerIdentification { get; set; }
    }

    [Feature("call_trustlevels")]
    public interface ICallTrustLevels
    {
        bool CallTrustLevels { get; set; }
    }

    [Feature("pattern_based_registration")]
    public interface IPatternBasedRegistration
    {
        bool PatternBasedRegistration { get; set; }
    }

    [Feature("registration_meta_api")]
    public interface IRegistrationMetaApi
    {
        bool RegistrationMetaApi { get; set; }
    }

    [Feature("shared_registration")]
    public interface ISharedRegistration
    {
        bool SharedRegistration { get; set; }
    }

    [Feature("call_timeout")]
    public interface ICallTimeout
    {
        bool CallTimeout { get; set; }
    }

    [Feature("call_canceling")]
    public interface ICallCanceling
    {
        bool CallCanceling { get; set; }
    }

    [Feature("progressive_call_results")]
    public interface IProgressiveCallResults
    {
        bool ProgressiveCallResults { get; set; }
    }


    [Feature("publisher_identification")]
    public interface IPublisherIdentification
    {
        bool PublisherIdentification { get; set; }
    }

    [Feature("publication_trustlevels")]
    public interface IPublicationTrustLevels
    {
        bool PublicationTrustLevels { get; set; }
    }

    [Feature("pattern_based_subscription")]
    public interface IPatternBasedSubscription
    {
        bool PatternBasedSubscription { get; set; }
    }

    [Feature("subscription_meta_api")]
    public interface ISubscriptionMetaApi
    {
        bool SubscriptionMetaApi { get; set; }
    }


    [Feature("subscriber_blackwhite_listing")]
    public interface ISubscriberBlackwhiteListing
    {
        bool SubscriberBlackwhiteListing { get; set; }
    }


    [Feature("publisher_exclusion")]
    public interface IPublisherExclusion
    {
        bool PublisherExclusion { get; set; }
    }


    [Feature("event_history")]
    public interface IEventHistory
    {
        bool EventHistory { get; set; }
    }


    /*
    public interface ICaller : ICallerIdentification, ICallTimeout, ICallCanceling, IProgressiveCallResults
    {
        
    }*/


    public abstract class Role
    {
    }


    public class Caller : Role, ICallerIdentification, ICallTimeout, ICallCanceling, IProgressiveCallResults
    {
        public bool CallCanceling { get; set; }
        public bool CallerIdentification { get; set; }
        public bool CallTimeout { get; set; }
        public bool ProgressiveCallResults { get; set; }
    }

    public class Dealer : Role, ICallerIdentification, ICallTrustLevels, IPatternBasedRegistration, IRegistrationMetaApi,
        ISharedRegistration, ICallTimeout, ICallCanceling, IProgressiveCallResults
    {
        public bool CallCanceling { get; set; }
        public bool CallerIdentification { get; set; }
        public bool CallTimeout { get; set; }
        public bool CallTrustLevels { get; set; }
        public bool PatternBasedRegistration { get; set; }
        public bool ProgressiveCallResults { get; set; }
        public bool RegistrationMetaApi { get; set; }
        public bool SharedRegistration { get; set; }
    }

    public class Callee : Role, ICallerIdentification, ICallTrustLevels, IPatternBasedRegistration, ISharedRegistration,
        ICallTimeout, ICallCanceling, IProgressiveCallResults
    {
        public bool CallCanceling { get; set; }
        public bool CallerIdentification { get; set; }
        public bool CallTimeout { get; set; }
        public bool CallTrustLevels { get; set; }
        public bool PatternBasedRegistration { get; set; }
        public bool ProgressiveCallResults { get; set; }
        public bool SharedRegistration { get; set; }
    }

    public class Publisher : Role, IPublisherIdentification, ISubscriberBlackwhiteListing, IPublisherExclusion
    {
        public bool PublisherExclusion { get; set; }
        public bool PublisherIdentification { get; set; }
        public bool SubscriberBlackwhiteListing { get; set; }
    }

    public class Broker : Role, IPublisherIdentification, IPublicationTrustLevels, IPatternBasedSubscription,
        ISubscriptionMetaApi, ISubscriberBlackwhiteListing, IPublisherExclusion, IEventHistory
    {
        public bool EventHistory { get; set; }
        public bool PatternBasedSubscription { get; set; }
        public bool PublicationTrustLevels { get; set; }
        public bool PublisherExclusion { get; set; }
        public bool PublisherIdentification { get; set; }
        public bool SubscriberBlackwhiteListing { get; set; }
        public bool SubscriptionMetaApi { get; set; }
    }

    public class Subscriber : Role, IPublisherIdentification, IPublicationTrustLevels, IPatternBasedSubscription,
        IEventHistory
    {
        public bool EventHistory { get; set; }
        public bool PatternBasedSubscription { get; set; }
        public bool PublicationTrustLevels { get; set; }
        public bool PublisherIdentification { get; set; }
    }


    public abstract class Feature
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}