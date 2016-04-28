using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wamplash.Handlers;
using Wamplash.Messages;

namespace Wamplash
{
    // TODO: Consider pulling this up into an interface, IWampClient, and making this one the WebsocketWampClient implementation. 
    // this way you can support other transports, like direct server process.
    public class WampClient
    {
        private static readonly ClientWebSocket Client = new ClientWebSocket();

        private readonly object sendLock = new object();
        public Dictionary<long, string> SubscribedTopics = new Dictionary<long, string>();
        public Dictionary<long, string> SubscribingTopics = new Dictionary<long, string>();

        public Uri Endpoint { get; private set; }
        public long SessionId { get; private set; }

        public async Task Connect(Uri endpoint, string realm)
        {
            Endpoint = endpoint;
            await Client.ConnectAsync(Endpoint, CancellationToken.None);
            (new Thread(ReceiveLoop)).Start();
            await Send(new HelloMessage(realm));
        }


        public async Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public async Task Send(WampMessage message)
        {
            if (Client.State == WebSocketState.Open)
            {
                var json = message.ToString();
                var bytes = Encoding.UTF8.GetBytes(json);
                var segment = new ArraySegment<byte>(bytes);
                lock (sendLock)
                    Client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            }
            else throw new Exception("Websocket state is not OPEN so can't send. Status is: " + Client.State);
        }


        public async Task Subscribe(SubscribeMessage message)
        {
            SubscribingTopics.Add(message.RequestId, message.Topic);
            await Send(message);
        }

        public async Task Subscribe(string topic)
        {
            await Subscribe(new SubscribeMessage(topic));
        }

        public async Task Unsubscribe(long subscriptionId)
        {
            throw new NotImplementedException();
        }

        public async Task Publish(string topic, string json)
        {
            var details = JsonConvert.DeserializeObject<dynamic>(json);
            await Send(new PublishMessage(topic, details));
        }

        private async void ReceiveLoop()
        {
            var buffer = new byte[1024*64];
            while (Client.State == WebSocketState.Open)
            {
                var segment = new ArraySegment<byte>(buffer);
                var result = await Client.ReceiveAsync(segment, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    await Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing..", CancellationToken.None);
                var count = result.Count;
                while (!result.EndOfMessage)
                {
                    if (count >= buffer.Length)
                    {
                        await
                            Client.CloseAsync(WebSocketCloseStatus.InvalidPayloadData,
                                "Message too big, size: " + count + ". Max supported size is: " + buffer.Length,
                                CancellationToken.None);
                        return;
                    }
                    segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                    result = await Client.ReceiveAsync(segment, CancellationToken.None);
                    count += result.Count;
                }
                var messageString = Encoding.UTF8.GetString(buffer, 0, count);
                var messageJson = JsonConvert.DeserializeObject<dynamic>(messageString);
                MessageReceived(messageJson);
            }
        }

        public event WampMessageHandler<WelcomeMessage> Welcome;
        public event WampMessageHandler<SubscribedMessage> Subscribed;
        public event WampMessageHandler<EventMessage> Event;


        private void MessageReceived(dynamic message)
        {
            int messageTypeId = message[0];

            switch (messageTypeId)
            {
                case MessageTypes.Welcome:
                    var welcome = new WelcomeMessage(message);
                    SessionId = welcome.SessionId;
                    if (Welcome != null)
                        Welcome(this, welcome);
                    break;
                case MessageTypes.Subscribed:
                    var subscribed = new SubscribedMessage(message);
                    var topic = SubscribingTopics[subscribed.RequestId];
                    SubscribingTopics.Remove(subscribed.RequestId);
                    SubscribedTopics.Add(subscribed.SubscriptionId, topic);
                    if (Subscribed != null)
                        Subscribed(this, subscribed);
                    break;
                case MessageTypes.Event:
                    if (Event != null)
                        Event(this, new EventMessage(message));

                    break;
                default:

                    throw new Exception("Unsupported message type: " + messageTypeId);
            }
        }
    }
}