using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Tizen.Sensor;

namespace TizenSensor.lib
{
	public class Sensor
	{
		public Sensor(Action<int, float, float, float> updateCallback, uint reportInterval = 100)
		{
			UpdateCallback = updateCallback;

			heartRateMonitor = new HeartRateMonitor
			{
				Interval = reportInterval,
				PausePolicy = SensorPausePolicy.None
			};
			accelerometer = new Accelerometer
			{
				Interval = reportInterval,
				PausePolicy = SensorPausePolicy.None
			};
			timer = new Timer
			{
				Interval = reportInterval,
				AutoReset = true
			};
			timer.Elapsed += Timer_Elapsed;
		}

		protected HeartRateMonitor heartRateMonitor;

		protected Accelerometer accelerometer;

		protected Timer timer;

		public Action<int, float, float, float> UpdateCallback { get; set; }

		public void Start()
		{
			heartRateMonitor.Start();
			accelerometer.Start();
			timer.Start();
		}

		public void Stop()
		{
			heartRateMonitor.Stop();
			accelerometer.Stop();
			timer.Stop();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			UpdateCallback?.Invoke(heartRateMonitor.HeartRate,
				accelerometer.X,
				accelerometer.Y,
				accelerometer.Z
			);
		}
	}
}
