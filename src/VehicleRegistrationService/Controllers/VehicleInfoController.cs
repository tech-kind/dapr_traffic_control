namespace VehicleRegistrationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehicleInfoController : ControllerBase
    {
        private readonly ILogger<VehicleInfoController> _logger;
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleInfoController(ILogger<VehicleInfoController> logger, IVehicleRepository vehicleRepository)
        {
            _logger = logger;
            _vehicleRepository = vehicleRepository;
        }

        [HttpGet("{licenseNumber}")]
        public ActionResult<VehicleInfo> GetVehicleInfo(string licenseNumber)
        {
            _logger.LogInformation($"Retrieving vehicle-info for licensenumber {licenseNumber}");
            VehicleInfo info = _vehicleRepository.GetVehicleInfo(licenseNumber);
            return info;
        }
    }
}
