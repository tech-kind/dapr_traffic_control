namespace Simulation
{
    public class CameraSimulation
    {
        private readonly ITrafficControlService _trafficControlService;
        private Random _rnd;
        private int _camNumber;
        private int _minEntryDelayInMS = 50;
        private int _maxEntryDelayInMS = 5000;
        private int _minExitDelayInS = 4;
        private int _maxExitDelayInS = 10;

        public CameraSimulation(int camNumber, ITrafficControlService trafficControlService)
        {
            _rnd = new Random();
            _camNumber = camNumber;
            _trafficControlService = trafficControlService;
        }

        public ValueTask Start()
        {
            Console.WriteLine($"Start camera {_camNumber} simulation.");

            while(true)
            {
                try
                {
                    TimeSpan entryDelay = TimeSpan.FromMilliseconds(_rnd.Next(_minEntryDelayInMS, _maxEntryDelayInMS) + _rnd.NextDouble());
                    Task.Delay(entryDelay).Wait();

                    Task.Run(async () =>
                    {
                        // 一定時間経過したら車両の進入操作を行う
                        // 車両の登録番号はランダムに生成する
                        DateTime entryTimeStamp =DateTime.Now;
                        var vehicleRegistered = new VehicleRegistered
                        {
                            Lane = _camNumber,
                            LicenseNumber = GenerateRandomLicenseNumber(),
                            Timestamp = entryTimeStamp
                        };

                        await _trafficControlService.SendVehicleEntryAsync(vehicleRegistered);

                        Console.WriteLine($"Simulated ENTRY of vehicle with license-number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");

                        // 一定時間経過したら車両の退出処理を行う
                        // 車線変更した可能性を考慮し、レーンはランダムで変更する
                        TimeSpan exitDelay = TimeSpan.FromSeconds(_rnd.Next(_minExitDelayInS, _maxExitDelayInS) + _rnd.NextDouble());
                        Task.Delay(exitDelay).Wait();
                        vehicleRegistered.Timestamp = DateTime.Now;
                        vehicleRegistered.Lane = _rnd.Next(1, 4);

                        await _trafficControlService.SendVehicleExitAsync(vehicleRegistered);

                        Console.WriteLine($"Simulated EXIT of vehicle with license-number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");

                    }).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Camera {_camNumber} error: {ex.Message}");   
                }
            }
        }

        #region Private helper methods

        private string _validLicenseNumberChars = "DFGHJKLNPRSTXYZ";

        private string GenerateRandomLicenseNumber()
        {
            int type = _rnd.Next(1, 9);
            string kenteken = string.Empty;
            switch(type)
            {
                case 1: // 89-AA-99
                    kenteken = string.Format("{0:00}-{1}-{2:00}", _rnd.Next(1, 99), GenerateRandomCharacters(2), _rnd.Next(1, 99));
                    break;
                case 2: // AA-99-AA
                    kenteken = string.Format("{0}-{1:00}-{2}", GenerateRandomCharacters(2), _rnd.Next(1, 99), GenerateRandomCharacters(2));
                    break;
                case 3: // AA-AA-99
                    kenteken = string.Format("{0}-{1}-{2:00}", GenerateRandomCharacters(2), GenerateRandomCharacters(2), _rnd.Next(1, 99));
                    break;
                case 4: // 99-AA-AA
                    kenteken = string.Format("{0:00}-{1}-{2}", _rnd.Next(1, 99), GenerateRandomCharacters(2), GenerateRandomCharacters(2));
                    break;
                case 5: // 99-AAA-9
                    kenteken = string.Format("{0:00}-{1}-{2}", _rnd.Next(1, 99), GenerateRandomCharacters(3), _rnd.Next(1, 10));
                    break;
                case 6: // 9-AAA-99
                    kenteken = string.Format("{0}-{1}-{2:00}", _rnd.Next(1, 9), GenerateRandomCharacters(3), _rnd.Next(1, 10));
                    break;
                case 7: // AA-999-A
                    kenteken = string.Format("{0}-{1:000}-{2}", GenerateRandomCharacters(2), _rnd.Next(1, 999), GenerateRandomCharacters(1));
                    break;
                case 8: // A-999-AA
                    kenteken = string.Format("{0}-{1:000}-{2}", GenerateRandomCharacters(1), _rnd.Next(1, 999), GenerateRandomCharacters(2));
                    break;
            }

            return kenteken;
        }

        private string GenerateRandomCharacters(int aantal)
        {
            char[] chars = new char[aantal];
            for (int i = 0; i < aantal; i++)
            {
                chars[i] = _validLicenseNumberChars[_rnd.Next(_validLicenseNumberChars.Length - 1)];
            }
            return new string(chars);
        }

        #endregion
    }
}
