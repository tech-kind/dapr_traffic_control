namespace FineCollectionService.DomainServices
{
    public class HardCodedFineCalculator : IFineCalculator
    {
        public int CalculateFine(string licenseKey, int violationInKmh)
        {
            if (licenseKey != "HX783-K2L7V-DRJ4A-5PN1G")
            {
                throw new InvalidOperationException("Invalid license-key specified.");
            }

            int fine = 9;
            if (violationInKmh < 5)
            {
                fine += 18;
            }
            else if (violationInKmh >= 5 && violationInKmh < 10)
            {
                fine += 31;
            }
            else if (violationInKmh >= 10 && violationInKmh < 15)
            {
                fine += 64;
            }
            else if (violationInKmh >= 15 && violationInKmh < 20)
            {
                fine += 121;
            }
            else if (violationInKmh >= 20 && violationInKmh < 25)
            {
                fine += 174;
            }
            else if (violationInKmh >= 25 && violationInKmh < 30)
            {
                fine += 232;
            }
            else if (violationInKmh >= 30 && violationInKmh < 35)
            {
                fine += 297;
            }
            else if (violationInKmh == 35)
            {
                fine += 372;
            }
            else
            {
                return 0;
            }

            return fine;
        }
    }
}
