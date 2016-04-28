using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Newtonsoft.Json;

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

            client.Connect(new Uri("ws://wamplash.azurewebsites.net/ws"), "defaultRealm").Wait();

            string topic = "io.crossbar.demo.pubsub.082880";

            client.Subscribe(topic).Wait();

            Thread.Sleep(2000);

            var counter = 0;
            while (true)
            {
                counter++;
                Thread.Sleep(10000);

                var json = "[\"" + Thread.CurrentThread.ManagedThreadId + " CONSOLE ! " + counter + "\"]";

                client.Publish(topic, json);
            }

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