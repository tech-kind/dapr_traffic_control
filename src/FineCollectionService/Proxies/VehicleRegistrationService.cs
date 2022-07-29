namespace FineCollectionService.Proxies
{
    public class VehicleRegistrationService
    {
        private HttpClient _httpClient;

        public VehicleRegistrationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async ValueTask<VehicleInfo> GetVehicleInfo(string licenseNumber)
        {
            return await _httpClient.GetFromJsonAsync<VehicleInfo>($"vehicleinfo/{licenseNumber}");
        }
    }
}
