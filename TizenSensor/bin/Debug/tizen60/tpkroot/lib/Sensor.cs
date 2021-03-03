using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Tizen.Security;
using Tizen.Sensor;

namespace TizenSensor.lib
{
	public class Sensor
	{
		protected const string HealthInfoPermissionString = "http://tizen.org/privilege/healthinfo";

		public static void Create(
			Action<Sensor> onCreated,
			Action<int, int, float, float, float> onUpdated,
			uint reportInterval = 100
		)
		{
			var initPermission = PrivacyPrivilegeManager.CheckPermission(HealthInfoPermissionString);
			if (initPermission == CheckResult.Allow)
			{
				onCreated(new Sensor(onUpdated, reportInterval));
				return;
			}
			if (initPermission == CheckResult.Deny)
			{
				onCreated(null);
				return;
			}

			if (!PrivacyPrivilegeManager.GetResponseContext(HealthInfoPermissionString).TryGetTarget(out var context))
			{
				onCreated(null);
				return;
			}

			context.ResponseFetched += (sender, e) =>
			{
				if (e.result != RequestResult.AllowForever) onCreated(null);
				else onCreated(new Sensor(onUpdated, reportInterval));
			};
			PrivacyPrivilegeManager.RequestPermission(HealthInfoPermissionString);
		}

		protected Sensor(Action<int, int, float, float, float> onUpdated, uint reportInterval = 100)
		{
			OnUpdated = onUpdated;

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
			timer.Elapsed += OnTimerElapsed;
		}

		protected HeartRateMonitor heartRateMonitor;

		protected Accelerometer accelerometer;

		protected Timer timer;

		protected int timestamp = 0;

		public Action<int, int, float, float, float> OnUpdated { get; set; }

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

		private void OnTimerElapsed(object sender, ElapsedEventArgs e)
		{
			timestamp++;
			OnUpdated.Invoke(
				timestamp,
				heartRateMonitor.HeartRate,
				accelerometer.X,
				accelerometer.Y,
				accelerometer.Z
			);
		}
	}
}
