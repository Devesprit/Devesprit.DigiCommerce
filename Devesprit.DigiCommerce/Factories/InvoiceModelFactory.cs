using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Invoice;
using Devesprit.Services.Invoice;
using Devesprit.Services.PaymentGateway;
using Mapster;

namespace Devesprit.DigiCommerce.Factories
{
    public partial class InvoiceModelFactory : IInvoiceModelFactory
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentGatewayManager _paymentGatewayManager;
        private readonly IWorkContext _workContext;

        public InvoiceModelFactory(IInvoiceService invoiceService,
            IPaymentGatewayManager paymentGatewayManager, 
            IWorkContext workContext)
        {
            _invoiceService = invoiceService;
            _paymentGatewayManager = paymentGatewayManager;
            _workContext = workContext;
        }

        public virtual async Task<InvoiceModel> PrepareInvoiceModelAsync(TblInvoices invoice)
        {
            var result = invoice.Adapt<InvoiceModel>();
            result.UserName = invoice.User?.UserName;
            result.InvoiceDetails = PrepareInvoiceDetailsModel(invoice.InvoiceDetails);

            result.InvoiceSubTotal = invoice.ComputeInvoiceTotalAmount(false, false);
            result.InvoiceTotal = invoice.ComputeInvoiceTotalAmount(false);
            
            if (invoice.BillingAddress != null && invoice.BillingAddress.Any())
            {
                result.UserBillingAddress = PrepareInvoiceBillingAddressModel(invoice.BillingAddress.FirstOrDefault());
                result.UserBillingAddress.InvoiceStatus = invoice.Status;
            }
            else
            {
                if (invoice.Status == InvoiceStatus.Pending && invoice.User != null)
                {
                    //Try load user latest billing address
                    var address = await _invoiceService.FindUserLatestBillingAddressAsync(invoice.UserId) ??
                                  new TblInvoiceBillingAddress()
                                  {
                                      Email = invoice.User.Email,
                                      FirstName = invoice.User.FirstName,
                                      LastName = invoice.User.LastName,
                                      CountryId = invoice.User.UserCountryId ?? 0
                                  };
                    result.UserBillingAddress = PrepareInvoiceBillingAddressModel(address);
                    result.UserBillingAddress.InvoiceStatus = invoice.Status;
                }
            }

            if (result.UserBillingAddress == null)
            {
                result.UserBillingAddress = new InvoiceBillingAddressModel {InvoiceStatus = invoice.Status};
            }

            if (invoice.Status == InvoiceStatus.Paid)
            {
                result.PaymentGateways = new List<IPaymentMethod>()
                {
                    _paymentGatewayManager.FindPaymentMethodBySystemName(invoice.PaymentGatewaySystemName)
                };
            }
            else
            {
                result.PaymentGateways =
                    _paymentGatewayManager.GetAvailablePaymentGatewaysForCurrency(_workContext.CurrentCurrency.IsoCode);
            }
            return result;
        }

        public virtual List<InvoiceDetailsModel> PrepareInvoiceDetailsModel(ICollection<TblInvoiceDetails> details)
        {
            var result = new List<InvoiceDetailsModel>();
            if (details != null)
            {
                var rowNumber = 1;
                foreach (var detail in details.OrderBy(p=> p.ItemName))
                {
                    var item = detail.Adapt<InvoiceDetailsModel>();
                    item.RowNumber = rowNumber;
                    result.Add(item);

                    rowNumber++;
                }
            }
            return result;
        }

        public virtual InvoiceBillingAddressModel PrepareInvoiceBillingAddressModel(TblInvoiceBillingAddress address)
        {
            if (address == null)
            {
                return new InvoiceBillingAddressModel();
            }

            var result = address.Adapt<InvoiceBillingAddressModel>();
            return result;
        }

        public virtual TblInvoiceBillingAddress PrepareTblInvoiceBillingAddress(InvoiceBillingAddressModel address)
        {
            var result = address.Adapt<TblInvoiceBillingAddress>();
            result.Country = null;
            result.CountryId = address.CountryId;
            return result;
        }
    }
}