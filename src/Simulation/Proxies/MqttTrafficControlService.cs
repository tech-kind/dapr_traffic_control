using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Proxies
{
    internal class MqttTrafficControlService : ITrafficControlService
    {
        private IMqttClient _client;

        private MqttTrafficControlService(IMqttClient mqttClient)
        {
            _client = mqttClient;
        }

        public static async ValueTask<MqttTrafficControlService> CreateAsync(int camNumber)
        {
            var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost";
            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();
            // ポート番号はMosQuittoのコンテナ起動時のポート番号と合わせる
            // src/Infrastructure/mosquittoのディレクトリ参照
            var mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttHost, 1883)
                .WithClientId($"camerasim{camNumber}")
                .Build();

            await client.ConnectAsync(mqttOptions, CancellationToken.None);
            return new MqttTrafficControlService(client);
        }

        public async ValueTask SendVehicleEntryAsync(VehicleRegistered vehicleRegistered)
        {
            var eventJson = JsonSerializer.Serialize(vehicleRegistered);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("trafficcontrol/entrycam")
                .WithPayload(Encoding.UTF8.GetBytes(eventJson))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            await _client.PublishAsync(message, CancellationToken.None);
        }

        public async ValueTask SendVehicleExitAsync(VehicleRegistered vehicleRegistered)
        {
            var eventJson = JsonSerializer.Serialize(vehicleRegistered);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("trafficcontrol/exitcam")
                .WithPayload(Encoding.UTF8.GetBytes(eventJson))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            await _client.PublishAsync(message, CancellationToken.None);
        }
    }
}
