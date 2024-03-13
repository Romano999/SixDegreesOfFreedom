namespace SixDegreesOfFreedom.Models
{
	public class SensorData
	{
		public OrientationSensorData OrientationData { get; init; }
		public GyroscopeData GyroscopeData { get; init; }
		public AccelerometerData AccelerometerData { get; init; }
	}
}
