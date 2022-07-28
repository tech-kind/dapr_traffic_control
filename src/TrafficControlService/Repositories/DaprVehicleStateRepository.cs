namespace TrafficControlService.Repositories
{
    // 車両情報はDaprを通してRedisに書き込まれる
    // 実装は書き込み先を意識しなくてよい
    // ストアの指定は「src/dapr/component/statestore.yaml」を参照
    public class DaprVehicleStateRepository : IVehicleStateRepository
    {
        private const string DAPR_STORE_NAME = "statestore";
        private readonly DaprClient _daprClient;

        public DaprVehicleStateRepository(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async ValueTask<VehicleState?> GetVehicleStateAsync(string licenseNumber)
        {
            var stateEntry = await _daprClient.GetStateEntryAsync<VehicleState>(
                DAPR_STORE_NAME, licenseNumber);
            return stateEntry.Value;
        }

        public async ValueTask SaveVehicleStateAsync(VehicleState vehicleState)
        {
            await _daprClient.SaveStateAsync<VehicleState>(
                DAPR_STORE_NAME, vehicleState.LicenseNumber, vehicleState);
        }
    }
}
