using System.ComponentModel;

namespace Devesprit.Data.Enums
{
    public enum InvoiceStatus
    {
        [Description("Pending")]
        Pending,
        [Description("Paid")]
        Paid
    }
}