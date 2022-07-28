namespace TrafficControlService.DomainServices
{
    public class DefaultSpeedingViolationCalculator : ISpeedingViolationCalculator
    {
        private readonly string _roadId;
        private readonly int _sectionLengthInKm;
        private readonly int _maxAllowedSpeedInKmh;
        private readonly int _legalCorrectionInKmh;

        public DefaultSpeedingViolationCalculator(string roadId, int sectionLengthInKm, int maxAllowedSpeedInKmh, int legalCorrectionInKmh)
        {
            _roadId = roadId;
            _sectionLengthInKm = sectionLengthInKm;
            _maxAllowedSpeedInKmh = maxAllowedSpeedInKmh;
            _legalCorrectionInKmh = legalCorrectionInKmh;
        }

        public int DetermineSpeedingViolationInKmh(DateTime entryTimestamp, DateTime exitTimestamp)
        {
            // 経過時間とセクションの距離から平均速度を求める
            double elapsedMinutes = exitTimestamp.Subtract(entryTimestamp).TotalSeconds;  // 1 sec. == 1min. in simulation
            double avgSpeedInKmh = Math.Round((_sectionLengthInKm / elapsedMinutes) * 60);

            // 平均速度が許容される最大速度以上か判断する
            int violation = Convert.ToInt32(avgSpeedInKmh - _maxAllowedSpeedInKmh - _legalCorrectionInKmh);
            return violation;
        }

        public string GetRoadId()
        {
            return _roadId;
        }
    }
}
