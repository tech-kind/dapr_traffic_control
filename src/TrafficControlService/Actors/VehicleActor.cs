namespace TrafficControlService.Actors
{
    public class VehicleActor : Actor, IVehicleActor, IRemindable
    {
        public readonly ISpeedingViolationCalculator _speedingViolationCalculator;
        private readonly string _roadId;
        private readonly DaprClient _daprClient;

        public VehicleActor(ActorHost host, DaprClient daprClient, ISpeedingViolationCalculator speedingViolationCalculator)
            : base(host)
        {
            _daprClient = daprClient;
            _speedingViolationCalculator = speedingViolationCalculator;
            _roadId = _speedingViolationCalculator.GetRoadId();
        }

        public async ValueTask RegisterEntryAsync(VehicleRegistered msg)
        {
            try
            {
                Logger.LogInformation($"ENTRY detected in lane {msg.Lane} at " +
                    $"{msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}");

                // 車両情報を保存する
                var vehicleState = new VehicleState(msg.LicenseNumber, msg.Timestamp);
                await StateManager.SetStateAsync("VehicleState", vehicleState);

                // 進入してから20秒以内に退出しない車両が存在しないか確認するためにリマインダーに登録する
                // (故障して補助が必要な場合がある)
                await RegisterReminderAsync("VehicleLost", null,
                    TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in RegisterEntry");
            }
        }

        public async ValueTask RegisterExitAsync(VehicleRegistered msg)
        {
            try
            {
                Logger.LogInformation($"EXIT detected in lane {msg.Lane} at " +
                    $"{msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}");

                // 退出したのでリマインダーからは削除
                await UnregisterReminderAsync("VehicleLost");

                // 車両の情報を取得
                var vehicleState = await StateManager.GetStateAsync<VehicleState>("VehicleState");
                vehicleState = vehicleState with { ExitTimestamp = msg.Timestamp };
                await StateManager.SetStateAsync("VehicleState", vehicleState);

                // スピード超過が起きているかどうかを計算し、超過が発生している場合は通知を行う
                int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(
                    vehicleState.EntryTimestamp, vehicleState.ExitTimestamp.Value);
                if (violation > 0)
                {
                    Logger.LogInformation($"Speeding violation detected ({violation} KMh) of vehicle " +
                        $"with license-number {vehicleState.LicenseNumber}.");

                    var speedingViolation = new SpeedingViolation
                    {
                        VehicleId = msg.LicenseNumber,
                        RoadId = _roadId,
                        ViolationInKmh = violation,
                        Timestamp = msg.Timestamp
                    };

                    await _daprClient.PublishEventAsync("pubsub", "speedingviolations", speedingViolation);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in RegisterExit");
            }
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName == "VehicleLost")
            {
                await UnregisterReminderAsync("VehicleLost");

                var vehicleState = await StateManager.GetStateAsync<VehicleState>("VehicleState");

                Logger.LogInformation($"Lost track of vehicle with license-number {vehicleState.LicenseNumber}. " +
                    "Sending road-assistence.");
            }
        }
    }
}
