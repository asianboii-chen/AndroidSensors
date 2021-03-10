using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using Tizen.Security;
using Tizen.Sensor;

namespace TizenSensor.lib
{
	public class Sensor
	{
		protected const string HealthInfoPermissionString = "http://tizen.org/privilege/healthinfo";

		public static void Create(Action<Sensor> onCreated, Action<Sensor, SensorData> onUpdated)
		{
			var initPermission = PrivacyPrivilegeManager.CheckPermission(HealthInfoPermissionString);
			if (initPermission == CheckResult.Allow)
			{
				onCreated(new Sensor(onUpdated));
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
				else onCreated(new Sensor(onUpdated));
			};
			PrivacyPrivilegeManager.RequestPermission(HealthInfoPermissionString);
		}

		protected Sensor(Action<Sensor, SensorData> onUpdated)
		{
			OnUpdated = onUpdated;

			heartRateMonitor = new HeartRateMonitor
			{
				PausePolicy = SensorPausePolicy.None
			};
			accelerometer = new Accelerometer
			{
				PausePolicy = SensorPausePolicy.None
			};
			gyroscope = new Gyroscope
			{
				PausePolicy = SensorPausePolicy.None
			};
			stopwatch = new Stopwatch();
		}

		public Action<Sensor, SensorData> OnUpdated { get; set; }

		public uint ReportInterval { get; protected set; }

		public bool IsRunning { get; protected set; } = false;

		protected HeartRateMonitor heartRateMonitor;

		protected Accelerometer accelerometer;

		protected Gyroscope gyroscope;

		protected Stopwatch stopwatch;

		public void Start(uint updateInterval)
		{
			if (IsRunning) return;

			IsRunning = true;
			heartRateMonitor.Interval = updateInterval;
			accelerometer.Interval = updateInterval;
			gyroscope.Interval = updateInterval;
			heartRateMonitor.Start();
			accelerometer.Start();
			gyroscope.Start();

			new Thread(() =>
			{
				stopwatch.Start();
				long lastReportTime = -updateInterval; // force immediate report
				while (IsRunning)
				{
					if (stopwatch.ElapsedMilliseconds - lastReportTime < updateInterval) continue;

					lastReportTime = stopwatch.ElapsedMilliseconds;
					Update(lastReportTime);
				}
				stopwatch.Stop();
				stopwatch.Reset();
			}).Start();
		}

		public void Stop()
		{
			if (!IsRunning) return;

			IsRunning = false;
			heartRateMonitor.Stop();
			accelerometer.Stop();
			gyroscope.Stop();
		}

		protected void Update(long elapsedMilliseconds)
		{
			OnUpdated.Invoke(this, new SensorData
			{
				Seconds = elapsedMilliseconds * .001F,
				HeartRate = heartRateMonitor.HeartRate,
				AccelerationX = accelerometer.X,
				AccelerationY = accelerometer.Y,
				AccelerationZ = accelerometer.Z,
				AngularVelocityX = gyroscope.X,
				AngularVelocityY = gyroscope.Y,
				AngularVelocityZ = gyroscope.Z
			});
		}
	}
}
