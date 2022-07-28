namespace TrafficControlService.Repositories
{
    public interface IVehicleStateRepository
    {
        ValueTask SaveVehicleStateAsync(VehicleState vehicleState);
        ValueTask<VehicleState?> GetVehicleStateAsync(string licenseNumber);
    }
}
