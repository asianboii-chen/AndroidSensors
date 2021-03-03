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
		}

		protected Sensor sensor;

		protected Client client;

		protected void OnSensorCreate(Sensor sensor)
		{
			if (sensor is null)
			{
				Console.WriteLine("Sensor: could not get necessary permissions");
				Application.Current.Quit();
				return;
			}

			this.sensor = sensor;
			Client.Connect("192.168.0.56", OnClientConnect);
		}

		protected void OnClientConnect(Client client)
		{
			if (client is null)
			{
				Console.WriteLine("Sensor: could not connect to the server");
				Application.Current.Quit();
				return;
			}

			this.client = client;
			sensor.Start();
		}

		protected void OnSensorUpdate(int timestamp, int heartRate, float accelX, float accelY, float accelZ)
		{
			string dataText = $"{timestamp}, {heartRate}, {accelX:0.0}, {accelY:0.0}, {accelZ:0.0}";
			dataLabel.Text = dataText;
			if (!client.Send(dataText + '\n')) Console.WriteLine("Sensor: could not send data");
		}
	}
}
