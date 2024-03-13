namespace SixDegreesOfFreedom.Models
{
	[Serializable]
	public class SensorDataWrapper
	{
		public float[] AccelerometerData { get; set; }
		public float[] GyroscopeData { get; set; }
		public float[] OrientationData { get; set; }
	}
}
