using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;

namespace Devesprit.Services.Invoice
{
    public partial interface IInvoiceService
    {
        #region CURD

        Task<TblInvoices> FindByIdAsync(Guid id, bool checkUserAccess = true);
        Task<Guid> AddAsync(TblInvoices record);
        Task DeleteAsync(Guid id);
        IQueryable<TblInvoices> GetAsQueryable();

        #endregion


        #region CURD Invoice Details

        Task<TblInvoices> FindInvoiceByItemIdAsync(int id);
        Task<TblInvoiceDetails> FindInvoiceItemByIdAsync(int id);
        Task AddItemToInvoiceAsync(InvoiceDetailsItemType itemType, string itemName, string itemHomePage, int itemId, double priceInMainCurrency, int qty, Guid? invoiceId = null);
        Task RemoveItemAsync(int itemId);
        Task SetItemLicenseAsync(int itemId, string license);
        Task SetItemExpirationAsync(int itemId, DateTime? expiration);
        Task IncreaseItemQtyAsync(int itemId, int qty = 1);
        Task DecreaseItemQtyAsync(int itemId, int qty = 1);
        Task SetItemUnitPriceAsync(int itemId, double price);

        #endregion


        #region Other Actions

        Task CheckoutInvoiceAsync(Guid invoiceId, string transactionId);
        Task<bool> UserCanEditInvoiceAsync(Guid id, string userId);
        Task<TblInvoices> GetUserCurrentInvoiceAsync(bool createNewIfNull = true);
        Task SetInvoiceNoteAsync(Guid invoiceId, string note, bool isForAdmin);
        Task AddUpdateBillingAddressAsync(Guid invoiceId, TblInvoiceBillingAddress address);
        Task<TblInvoiceBillingAddress> FindUserLatestBillingAddressAsync(string userid);
        Task SetGatewayNameAndTokenAsync(Guid invoiceId, string gatewayName, string gatewaySystemName, string token, int currencyId);
        Task SetStatusAsync(Guid invoiceId, InvoiceStatus status);
        Task SetPaymentDateAsync(Guid invoiceId, DateTime? paymentDate);
        Task SetUserIdAsync(Guid invoiceId, string userId);
        Task SetDiscountAsync(Guid invoiceId, double? discountAmount, string discountDescription);
        Task SetTaxAsync(Guid invoiceId, double? taxAmount, string taxDescription);
        Task<TblInvoices> ApplyInvoiceTaxesAsync(Guid invoiceId);
        Task<TblInvoices> ApplyInvoiceDiscountsAsync(Guid invoiceId);
        Task DeletePendingInvoices();

        #endregion


        #region Report Generators

        Task<Dictionary<DateTime, int>> InvoicesReportAsync(DateTime fromDate, DateTime toDate,
            TimePeriodType periodType, InvoiceStatus? status);
        Task<Dictionary<DateTime, double>> SellsReportAsync(DateTime fromDate, DateTime toDate,
            TimePeriodType periodType, int currencyId);

        #endregion
    }
}
