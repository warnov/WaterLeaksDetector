using System;
using Microsoft.Azure.NotificationHubs;
using Microsoft.ServiceBus.Messaging;

namespace NotificationsGathererConsole

{
    class Program
    {
        static void Main(string[] args)
        {
            string eventHubConnectionString ="Endpoint=sb://ehpipelinemanage-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=K6rb90Q1O9cXH+HCitVx1atTD5YlALs7ka6VW3KIta0=";
            string eventHubName = "EHPipelineManager";
            string storageAccountName = "sawarkathon";
            string storageAccountKey = "Cn1PoHKO2i/Zejk7SvyPyQzi3pHaOU+blb39VLiWc+u3NwPzj+fQEKXFb+vVC454hn9J1t2iVjR8S8WDsMrQA==";
            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
            Console.WriteLine("Registering EventProcessor...");
            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => { Console.WriteLine(e.Exception); };
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>(options).Wait();
            if (AnomallyDetected())
            {
                SendNotificationAsync();
            }

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }

        private static bool AnomallyDetected()
        {
            return false;
        }

        private static async void SendNotificationAsync()
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString("Endpoint=sb://pipelinemanager.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=zBuV2y8UnbVDbdLQ+cZBgzcJKvrG34Pd3ek+QP6k/ss=", "NHPipelineManager");
            string toast = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<wp:Notification xmlns:wp=\"WPNotification\">" +
                   "<wp:Toast>" +
                        "<wp:Text1>Hello from a .NET App!</wp:Text1>" +
                   "</wp:Toast> " +
                "</wp:Notification>";
            await hub.SendMpnsNativeNotificationAsync(toast);
        }
    }
}
