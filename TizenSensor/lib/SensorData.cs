using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TizenSensor.lib
{
	[JsonObject(MemberSerialization.OptIn)]
	public class SensorData
	{
		public static SensorData FromJson(string json)
		{
			return JsonConvert.DeserializeObject<SensorData>(json);
		}

		public SensorData()
		{
		}

		[JsonProperty(PropertyName = "s")]
		public float Seconds { get; set; }

		[JsonProperty(PropertyName = "hr")]
		public int HeartRate { get; set; }

		[JsonProperty(PropertyName = "ax")]
		public float AccelerationX { get; set; }

		[JsonProperty(PropertyName = "ay")]
		public float AccelerationY { get; set; }

		[JsonProperty(PropertyName = "az")]
		public float AccelerationZ { get; set; }

		[JsonProperty(PropertyName = "vx")]
		public float AngularVelocityX { get; set; }

		[JsonProperty(PropertyName = "avy")]
		public float AngularVelocityY { get; set; }

		[JsonProperty(PropertyName = "avz")]
		public float AngularVelocityZ { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
