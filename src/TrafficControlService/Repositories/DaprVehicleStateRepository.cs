﻿namespace TrafficControlService.Repositories
{
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
