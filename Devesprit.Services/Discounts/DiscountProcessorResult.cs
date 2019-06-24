namespace Devesprit.Services.Discounts
{
    public partial class DiscountProcessorResult
    {
        public double DiscountAmountInMainCurrency { get; set; }
        public string DiscountDescription { get; set; }
        public bool Apply { get; set; }
    }
}