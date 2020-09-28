namespace Bercy.Front.Client.Models
{
    public class SimulationModel
    {
        public double Wage { get; set; }
        public int Year { get; set; }
        public int NbAdults { get; set; }
        public int NbChildren { get; set; }

        public static SimulationModel Default
        {
            get
            {
                return new SimulationModel
                {
                    Year = 2019,
                    NbAdults = 1,
                    NbChildren = 0,
                    Wage = 40000
                };
            }
        }
    }
}
