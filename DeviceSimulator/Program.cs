using System;
using System.Text;
using System.Threading;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace DeviceSimulator
{
    class Program
    {
        static DeviceClient _deviceClient;
        private const string IotHubUri = "pipelinemanager.azure-devices.net";
        private const string DeviceKey = "Kbij9r9Y15eb1p+qVoXKqMBwsFgzAfNlPxRuNSvyDGE=";
        private const string DeviceId = "RBPipelineMonitor";
        private static int _sum;

        static void Main()
        {
            Console.WriteLine("Simulated device\n");
            _deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey));

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            var rand = new Random();

            while (true)
            {
                var dev = rand.Next() % 2;
                var flow = rand.NextDouble();
                _sum += 300 + rand.Next(0, 100);

                var telemetryDataPoint = new
                {
                    dev,
                    flow,
                    date = TimeSpan.FromTicks(DateTime.Now.Ticks - DateTime.Today.Ticks).TotalMilliseconds,
                    sum = _sum
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Thread.Sleep(1000);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
