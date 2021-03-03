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
			Action<Sensor> onCreate,
			Action<int, int, float, float, float> onUpdate,
			uint reportInterval = 100
		)
		{
			var initPermission = PrivacyPrivilegeManager.CheckPermission(HealthInfoPermissionString);
			if (initPermission == CheckResult.Allow)
			{
				onCreate(new Sensor(onUpdate, reportInterval));
				return;
			}
			if (initPermission == CheckResult.Deny)
			{
				onCreate(null);
				return;
			}

			if (!PrivacyPrivilegeManager.GetResponseContext(HealthInfoPermissionString).TryGetTarget(out var context))
			{
				onCreate(null);
				return;
			}

			context.ResponseFetched += (sender, e) =>
			{
				if (e.result != RequestResult.AllowForever) onCreate(null);
				else onCreate(new Sensor(onUpdate, reportInterval));
			};
			PrivacyPrivilegeManager.RequestPermission(HealthInfoPermissionString);
		}

		protected Sensor(Action<int, int, float, float, float> onUpdate, uint reportInterval = 100)
		{
			OnUpdate = onUpdate;

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

		protected int timestamp = 0;

		public Action<int, int, float, float, float> OnUpdate { get; set; }

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
			timestamp++;
			try
			{
				OnUpdate?.Invoke(
					timestamp,
					heartRateMonitor.HeartRate,
					accelerometer.X,
					accelerometer.Y,
					accelerometer.Z
				);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Sensor: error occurred on update (t={timestamp}): {ex.Message}");
			}
		}
	}
}
