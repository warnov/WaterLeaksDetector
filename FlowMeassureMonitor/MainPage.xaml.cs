using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace FlowMeassureMonitor
{
    public sealed partial class MainPage : Page
    {
        private I2cDevice arduinoOne;
        private Timer periodicTimer;

        static DeviceClient deviceClient;
        static string iotHubUri = "youriothuburi.azure-devices.net";
        static string deviceKey = "yourdevicekey";
        static string deviceId = "yourdeviceid";
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            deviceClient = DeviceClient.Create(iotHubUri, AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Http1);
            IniciateMonitoring();
        }

        private async void IniciateMonitoring()
        {
            var settings = new I2cConnectionSettings(0X40);
            settings.BusSpeed = I2cBusSpeed.StandardMode;
            string aqs = I2cDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            arduinoOne = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            periodicTimer = new Timer(this.TimerCallback, null, 0, 2000);
        }

        private void TimerCallback(object state)
        {
            byte[] RegAddrBuf = new byte[] { 0x40 };
            byte[] ReadBuf = new byte[51];
            try
            {
                arduinoOne.Read(ReadBuf);
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }

            char[] cArray = Encoding.UTF8.GetString(ReadBuf, 0, 51).ToCharArray();
            String c = new String(cArray);


            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                HandleResults(c);
            });
        }

        private void HandleResults(string c)
        {
            string[] receivedData = c.Split('|');

            txtFlowOne.Text = String.Format("Flow: {0}", receivedData[0]);
            txtLtMinOne.Text = String.Format("Lt/min: {0}", receivedData[1]);
            txtTotalMlOne.Text = String.Format("Total ml: {0}", receivedData[2]);

            txtFlowTwo.Text = String.Format("Flow: {0}", receivedData[3]);
            txtLtMinTwo.Text = String.Format("Lt/min: {0}", receivedData[4]);
            txtTotalMlTwo.Text = String.Format("Total ml: {0}", receivedData[5].Substring(0, receivedData[5].IndexOf((char)65533)));

            SendInformationToHub(receivedData);
        }

        private async void SendInformationToHub(string[] telemetryData)
        {
            double sensorOneFlowValue = Convert.ToDouble(telemetryData[0]);
            double sensorOneLtMinValue = Convert.ToDouble(telemetryData[1]);
            double sensorOneTotalMlValue = Convert.ToDouble(telemetryData[2]);

            double sensorTwoFlowValue = Convert.ToDouble(telemetryData[3]);
            double sensorTwoLtMinValue = Convert.ToDouble(telemetryData[4]);
            double sensorTwoTotalMlValue = Convert.ToDouble(telemetryData[5].Substring(0, telemetryData[5].IndexOf((char)65533)));

            var telemetryOneDataPoint = new
            {
                deviceId = deviceId,
                dev = 0,
                flow = sensorOneFlowValue,
                sensorOneLtMin = sensorOneLtMinValue,
                sum = sensorOneTotalMlValue,
                date = TimeSpan.FromTicks(DateTime.Now.Ticks - DateTime.Today.Ticks).TotalMilliseconds
            };

            var telemetryTwoDataPoint = new
            {
                deviceId = deviceId,
                dev = 1,
                flow = sensorTwoFlowValue,
                sensorTwoLtMin = sensorTwoLtMinValue,
                sum = sensorTwoTotalMlValue,
                date = TimeSpan.FromTicks(DateTime.Now.Ticks - DateTime.Today.Ticks).TotalMilliseconds
            };

            var messageSensorOneString = JsonConvert.SerializeObject(telemetryOneDataPoint);
            var messageSensorTwoString = JsonConvert.SerializeObject(telemetryTwoDataPoint);
            var messageSensorOne = new Message(Encoding.ASCII.GetBytes(messageSensorOneString));
            var messageSensorTwo = new Message(Encoding.ASCII.GetBytes(messageSensorTwoString));

            Debug.WriteLine(messageSensorOneString);
            Debug.WriteLine(messageSensorTwoString);
            await deviceClient.SendEventAsync(messageSensorOne);
            await deviceClient.SendEventAsync(messageSensorTwo);
        }
    }
}
