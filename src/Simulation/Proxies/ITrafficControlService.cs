namespace Simulation.Proxies
{
    public interface ITrafficControlService
    {
        public ValueTask SendVehicleEntryAsync(VehicleRegistered vehicleRegistered);
        public ValueTask SendVehicleExitAsync(VehicleRegistered vehicleRegistered);
    }
}
