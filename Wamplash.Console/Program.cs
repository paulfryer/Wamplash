using System;
using System.Runtime.Remoting.Messaging;

namespace Wamplash.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new WampClient();

            client.Welcome += client_Welcome;
            client.Subscribed += client_Subscribed;
            client.Event += client_Event;

            client.Connect(new Uri("ws://localhost:63519/ws"), "defaultRealm").Wait();

            client.Subscribe("io.crossbar.demo.pubsub.082880").Wait();
            

            System.Console.ReadKey();

        }

        static void client_Event(object sender, Messages.EventMessage message)
        {
            System.Console.WriteLine("GOT EVENT: " + message.ToString());
        }

        static void client_Subscribed(object sender, Messages.SubscribedMessage message)
        {
            System.Console.WriteLine("WE HAVE SUBSCRBIED: " + message.SubscriptionId);
        }

        static void client_Welcome(object sender, Messages.WelcomeMessage message)
        {
            System.Console.WriteLine("WE ARE WELCOMED! SEssion:" + message.SessionId);
        }
    }
}