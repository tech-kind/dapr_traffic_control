// #define USE_ACTORMODEL

namespace TrafficControlService.Controllers
{
    [ApiController]
    [Route("")]
    public class TrafficController : ControllerBase
    {
        private readonly ILogger<TrafficController> _logger;
        private readonly IVehicleStateRepository _vehicleStateRepository;
        private readonly ISpeedingViolationCalculator _speedingViolationCalculator;
        private readonly string _roadId;

        public TrafficController(
            ILogger<TrafficController> logger,
            IVehicleStateRepository vehicleStateRepository,
            ISpeedingViolationCalculator speedingViolationCalculator)
        {
            _logger = logger;
            _vehicleStateRepository = vehicleStateRepository;
            _speedingViolationCalculator = speedingViolationCalculator;
            _roadId = speedingViolationCalculator.GetRoadId();
        }

#if !USE_ACTORMODEL

        [HttpPost("entrycam")]
        public async Task<ActionResult> VehicleEntryAsync(VehicleRegistered msg)
        {
            try
            {
                _logger.LogInformation($"ENTRY detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                // 車両情報を保存する
                var vehicleState = new VehicleState(msg.LicenseNumber, msg.Timestamp, null);
                await _vehicleStateRepository.SaveVehicleStateAsync(vehicleState);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing ENTRY");
                return StatusCode(500);
            }
        }

        [HttpPost("exitcam")]
        public async Task<ActionResult> VehicleExitAsync(VehicleRegistered msg, [FromServices] DaprClient daprClient)
        {
            try
            {
                // 車両情報を取得
                var state = await _vehicleStateRepository.GetVehicleStateAsync(msg.LicenseNumber);
                if (state == default(VehicleState))
                {
                    return NotFound();
                }

                _logger.LogInformation($"EXIT detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                // 退出時間を更新
                var exitState = state.Value with { ExitTimestamp = msg.Timestamp };
                await _vehicleStateRepository.SaveVehicleStateAsync(exitState);

                // スピード超過が起きているかどうかを計算し、超過が発生している場合は通知を行う
                int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(exitState.EntryTimestamp, exitState.ExitTimestamp.Value);
                if (violation > 0)
                {
                    _logger.LogInformation($"Speeding violation detected ({violation} KMh) of vehicle " +
                        $"with license-number {state.Value.LicenseNumber}.");

                    var speedingViolation = new SpeedingViolation
                    {
                        VehicleId = msg.LicenseNumber,
                        RoadId = _roadId,
                        ViolationInKmh = violation,
                        Timestamp = msg.Timestamp
                    };

                    await daprClient.PublishEventAsync("pubsub", "speedingviolations", speedingViolation);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing EXIT");
                return StatusCode(500);
            }
        }

#else

        [HttpPost("entrycam")]
        public async Task<ActionResult> VehicleEntryAsync(VehicleRegistered msg)
        {
            try
            {
                var actorId = new ActorId(msg.LicenseNumber);
                var proxy = ActorProxy.Create<IVehicleActor>(actorId, nameof(VehicleActor));
                await proxy.RegisterEntryAsync(msg);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("exitcam")]
        public async Task<ActionResult> VehicleExitAsync(VehicleRegistered msg)
        {
            try
            {
                var actorId = new ActorId(msg.LicenseNumber);
                var proxy = ActorProxy.Create<IVehicleActor>(actorId, nameof(VehicleActor));
                await proxy.RegisterExitAsync(msg);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

#endif
    }
}
