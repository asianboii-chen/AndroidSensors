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

			SensorPermissionGetter.GetPermission(Init);
		}

		protected Sensor sensor;

		protected void Init(bool hasPermission)
		{
			if (!hasPermission)
			{
				Application.Current.Quit();
				return;
			}

			sensor = new Sensor(OnSensorUpdate, 1000);
			sensor.Start();
		}

		protected void OnSensorUpdate(int heartRate, float accelX, float accelY, float accelZ)
		{
			dataLabel.Text = $"{heartRate}, {accelX:0.0}, {accelY:0.0}, {accelZ:0.0}";
		}
	}
}
