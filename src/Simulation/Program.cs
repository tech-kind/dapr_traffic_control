int lanes = 3;
CameraSimulation[] cameras = new CameraSimulation[lanes];
for (int i = 0; i < lanes; i++)
{
    int camNumber = i + 1;
    var trafficControlService = await MqttTrafficControlService.CreateAsync(camNumber);
    cameras[i] = new CameraSimulation(camNumber, trafficControlService);
}

Parallel.ForEach(cameras, (cam) => cam.Start());

Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();
