using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TizenSensor.lib;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TizenSensor
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

			Sensor.Create(OnSensorCreated, OnSensorUpdated, 1000);

			addrEntry.Completed += OnAddrEntryCompleted;
		}

		protected Sensor sensor;

		protected Client client;

		protected void OnSensorCreated(Sensor sensor)
		{
			if (sensor is null)
			{
				messageLabel.Text = "Permission failed!";
				return;
			}

			this.sensor = sensor;
			addrEntry.IsEnabled = true;
		}

		protected void OnAddrEntryCompleted(object sender, EventArgs e)
		{
			addrEntry.IsEnabled = false;
			messageLabel.Text = "Connecting...";
			Client.Connect(addrEntry.Text.Trim(), OnClientConnected);
		}

		protected void OnClientConnected(Client client)
		{
			if (client is null)
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					messageLabel.Text = "Connection failed!";
					addrEntry.IsEnabled = true;
				});
				return;
			}

			this.client = client;
			Device.BeginInvokeOnMainThread(() => addrEntry.IsVisible = false);
			sensor.Start();
		}

		protected void OnSensorUpdated(int timestamp, int heartRate, float accelX, float accelY, float accelZ)
		{
			Device.BeginInvokeOnMainThread(
				() => messageLabel.Text = $"{timestamp}, {heartRate}, {accelX:0.0}, {accelY:0.0}, {accelZ:0.0}"
			);
			client.Send($"{timestamp} {heartRate} {accelX} {accelY} {accelZ}\n");
		}
	}
}
