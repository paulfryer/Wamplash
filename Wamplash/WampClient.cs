using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wamplash.Handlers;
using Wamplash.Messages;

namespace Wamplash
{
    public class WampClient
    {
        private readonly ClientWebSocket client = new ClientWebSocket();


        public async Task Connect(Uri endpoint, string realm)
        {
            await client.ConnectAsync(endpoint, CancellationToken.None);

            (new Thread(ReceiveLoop)).Start();

            await Send(new HelloMessage(realm));
        }

        public async Task Send(WampMessage message)
        {
            var json = message.ToString();
            var bytes = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(bytes);


            await client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }


        public async Task Subscribe(string topic)
        {
            await Send(new SubscribeMessage(topic));
        }

        private async void ReceiveLoop()
        {
            var buffer = new byte[1024];

            while (true)
            {
                var segment = new ArraySegment<byte>(buffer);
                var result = await client.ReceiveAsync(segment, CancellationToken.None);

                var count = result.Count;

                while (!result.EndOfMessage)
                {
                    if (count >= buffer.Length)
                    {
                        await
                            client.CloseAsync(WebSocketCloseStatus.InvalidPayloadData,
                                "Message too big, size: " + count + ". Max supported size is: " + buffer.Length,
                                CancellationToken.None);
                        return;
                    }

                    segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                    result = await client.ReceiveAsync(segment, CancellationToken.None);
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
                    if (Welcome != null)
                        Welcome(this, new WelcomeMessage(message));
                    break;
                case MessageTypes.Subscribed:
                    if (Subscribed != null)
                        Subscribed(this, new SubscribedMessage(message));
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