namespace TrafficControlService.Actors
{
    public interface IVehicleActor : IActor
    {
        public ValueTask RegisterEntryAsync(VehicleRegistered msg);
        public ValueTask RegisterExitAsync(VehicleRegistered msg);
    }
}
