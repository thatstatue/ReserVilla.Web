namespace DummyWebApp.ViewModels
{
    public class RadialChartVM
    {
        public decimal TotalCount { get; set; }
        public decimal IncreaseDecreaseAmount { get; set; }
        public bool HasRatioIncreased { get; set; }
        public int[] Series { get; set; }
    }
}
