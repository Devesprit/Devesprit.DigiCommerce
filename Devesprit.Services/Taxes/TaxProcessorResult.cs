namespace Devesprit.Services.Taxes
{
    public partial class TaxProcessorResult
    {
        public double TaxAmountInMainCurrency { get; set; }
        public string TaxDescription { get; set; }
        public bool Apply { get; set; }
    }
}