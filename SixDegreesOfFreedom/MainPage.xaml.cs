using SixDegreesOfFreedom.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Timers;
using WebSocketSharp;

namespace SixDegreesOfFreedom
{
	public partial class MainPage : ContentPage
	{
		private static WebSocket ws;
		private OrientationSensorData currentOrientationData;
		private GyroscopeData currentGyroscopeData;
		private AccelerometerData currentAccelerometerData;

		public MainPage()
		{
			currentOrientationData = new OrientationSensorData();
			currentGyroscopeData = new GyroscopeData();
			currentAccelerometerData = new AccelerometerData();

			InitializeComponent();
			ToggleAccelerometer();
			ToggleGyroScope();
			ToggleOrientation();
			Task.Run(() => InitializeWebsocketClient());
		}

		private void ToggleOrientation()
		{
			if (OrientationSensor.Default.IsSupported)
			{
				if (!OrientationSensor.Default.IsMonitoring)
				{
					// Turn on orientation
					OrientationSensor.Default.ReadingChanged += Orientation_ReadingChanged;
					OrientationSensor.Default.Start(SensorSpeed.UI);
				}
				else
				{
					// Turn off orientation
					OrientationSensor.Default.Stop();
					OrientationSensor.Default.ReadingChanged -= Orientation_ReadingChanged;
				}
			}
		}

		private void Orientation_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
		{
			// Update UI Label with orientation state
			var orientationData = e.Reading;

			OrientationLabel.TextColor = Colors.Green;
			OrientationLabel.Text = $"Orientation: {orientationData}";

			currentOrientationData = orientationData;
		}

		private void ToggleGyroScope()
		{
			if (Gyroscope.Default.IsSupported)
			{
				if (!Gyroscope.Default.IsMonitoring)
				{
					// Turn on gyroscope
					Gyroscope.Default.ReadingChanged += Gyroscope_ReadingChanged;
					Gyroscope.Default.Start(SensorSpeed.UI);
				}
				else
				{
					// Turn off gyroscope
					Gyroscope.Default.Stop();
					Gyroscope.Default.ReadingChanged -= Gyroscope_ReadingChanged;
				}
			}
		}

		private void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
		{
			// Update UI Label with gyroscope state
			var gyroscopeData = e.Reading;

			GyroscopeLabel.TextColor = Colors.Green;
			GyroscopeLabel.Text = $"Gyroscope: {gyroscopeData}";

			currentGyroscopeData = gyroscopeData;
		}

		private void ToggleAccelerometer()
		{
			if (Accelerometer.Default.IsSupported)
			{
				if (!Accelerometer.Default.IsMonitoring)
				{
					Accelerometer.Default.ReadingChanged += AccelerometerReadingChanged;
					Accelerometer.Default.Start(SensorSpeed.UI);
				}
				else
				{
					Accelerometer.Default.Stop();
					Accelerometer.Default.ReadingChanged -= AccelerometerReadingChanged;
				}
			}
		}

		private void AccelerometerReadingChanged(object sender, AccelerometerChangedEventArgs e)
		{
			// Update UI Label with accelerometer state
			var accelerometerData = e.Reading;

			AccelLabel.TextColor = Colors.Green;
			AccelLabel.Text = $"Accel: {accelerometerData}";

			currentAccelerometerData = accelerometerData;
		}

		private async Task InitializeWebsocketClient()
		{
			Trace.WriteLine("Attempting to open websocket");

			ws = new WebSocket("ws://192.168.178.114:9000/sensors");

			ws.OnOpen += OnOpenHandler;
			ws.OnMessage += OnMessageHandler;
			ws.OnError += OnErrorHandler;
			ws.OnClose += OnCloseHandler;

			ws.Connect();

			var timer = new System.Timers.Timer(50);

			timer.Elapsed += OnTimedEvent;
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			var sensorData = new SensorData
			{
				AccelerometerData = currentAccelerometerData,
				GyroscopeData = currentGyroscopeData,
				OrientationData = currentOrientationData
			};

			var data = new SensorDataWrapper
			{
				AccelerometerData = [sensorData.AccelerometerData.Acceleration.X, sensorData.AccelerometerData.Acceleration.Y, sensorData.AccelerometerData.Acceleration.Z],
				GyroscopeData = [sensorData.GyroscopeData.AngularVelocity.X, sensorData.GyroscopeData.AngularVelocity.Y, sensorData.GyroscopeData.AngularVelocity.Z],
				OrientationData = [sensorData.OrientationData.Orientation.X, sensorData.OrientationData.Orientation.Y, sensorData.OrientationData.Orientation.Z, sensorData.OrientationData.Orientation.W]
			};

			var json = JsonSerializer.SerializeToUtf8Bytes(data);

			//if (ws.IsAlive)
			//{
			//	Trace.Write($"Cannot send data, websocket is closed");
			//	return;
			//}

			ws.Send(json);
		}

		private static void OnErrorHandler(object sender, EventArgs e)
		{
			Trace.WriteLine($"Error occurred: {e}");
		}

		private static void OnCloseHandler(object sender, CloseEventArgs e)
		{
			Trace.WriteLine($"Closed, {e.Code} {e.Reason}");
		}

		private static void OnOpenHandler(object sender, EventArgs e)
		{
			Trace.WriteLine("Player");
			var data = "Player1";
			ws.Send(data);
		}

		private static void OnMessageHandler(object sender, MessageEventArgs e)
		{
			Trace.WriteLine("WebSocket server said: " + e.Data);
		}
	}

}
