using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			Sensor.Create(OnSensorCreate, OnSensorUpdate, 1000);

			addrEntry.Completed += AddrEntry_Completed;
		}

		protected Sensor sensor;

		protected Client client;

		protected void OnSensorCreate(Sensor sensor)
		{
			if (sensor is null)
			{
				messageLabel.Text = "Permission failed!";
				return;
			}

			this.sensor = sensor;
			addrEntry.IsEnabled = true;
		}

		private void AddrEntry_Completed(object sender, EventArgs e)
		{
			addrEntry.IsEnabled = false;
			messageLabel.Text = "Connecting...";
			Client.Connect(addrEntry.Text.Trim(), OnClientConnect);
		}

		protected void OnClientConnect(Client client)
		{
			if (client is null)
			{
				messageLabel.Text = "Connection failed!";
				addrEntry.IsEnabled = true;
				return;
			}

			this.client = client;
			addrEntry.IsVisible = false;
			sensor.Start();
		}

		protected void OnSensorUpdate(int timestamp, int heartRate, float accelX, float accelY, float accelZ)
		{
			string dataText = $"{timestamp}, {heartRate}, {accelX:0.0}, {accelY:0.0}, {accelZ:0.0}";
			messageLabel.Text = dataText;
			client.Send(dataText + '\n');
		}
	}
}
