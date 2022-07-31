namespace VehicleRegistrationService.Repositories
{
    public interface IVehicleRepository
    {
        VehicleInfo GetVehicleInfo(string licenseNumber);
    }
}
