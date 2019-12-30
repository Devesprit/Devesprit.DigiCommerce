using Devesprit.Core;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Discounts;
using Devesprit.Services.Events;
using Devesprit.Services.LicenseManager;
using Devesprit.Services.Localization;
using Devesprit.Services.Products;
using Devesprit.Services.Taxes;
using Devesprit.Services.Users;
using Devesprit.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Devesprit.Services.Currency;
using Mapster;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Invoice
{
    public partial class InvoiceService : IInvoiceService
    {

        #region Fields

        private readonly AppDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly IUsersService _usersService;
        private readonly IUserGroupsService _userGroupsService;
        private readonly IProductService _productService;
        private readonly IProductCheckoutAttributesService _productCheckoutAttributesService;
        private readonly ITaxesService _taxesService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILicenseManager _licenseManager;
        private readonly ISettingService _settingService;
        private readonly IEventPublisher _eventPublisher;

        #endregion


        #region Ctor

        public InvoiceService(AppDbContext dbContext,
            IWorkContext workContext,
            IUsersService usersService,
            IUserGroupsService userGroupsService,
            IProductService productService,
            IProductCheckoutAttributesService productCheckoutAttributesService,
            ITaxesService taxesService,
            IPluginFinder pluginFinder,
            ILicenseManager licenseManager,
            ISettingService settingService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _workContext = workContext;
            _usersService = usersService;
            _userGroupsService = userGroupsService;
            _productService = productService;
            _productCheckoutAttributesService = productCheckoutAttributesService;
            _taxesService = taxesService;
            _pluginFinder = pluginFinder;
            _licenseManager = licenseManager;
            _settingService = settingService;
            _eventPublisher = eventPublisher;
        }

        #endregion


        #region CURD

        public virtual async Task<TblInvoices> FindByIdAsync(Guid id, bool checkUserAccess = true)
        {
            var result = await _dbContext.Invoices
                .Include(p => p.InvoiceDetails)
                .Include(p => p.User)
                .Include(p => p.Currency)
                .Include(p => p.BillingAddress)
                .AsNoTracking()
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.Invoice);

            //Check User
            if (checkUserAccess)
                if (!await UserHasAccessToInvoiceAsync(result, _workContext.CurrentUser?.Id))
                    return null;

            return result;
        }

        public virtual async Task<Guid> AddAsync(TblInvoices record)
        {
            _dbContext.Invoices.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.Invoices.Where(p => p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual IQueryable<TblInvoices> GetAsQueryable()
        {
            return _dbContext.Invoices.OrderByDescending(p => p.CreateDate);
        }

        #endregion


        #region CURD Invoice Details

        public virtual async Task<TblInvoices> FindInvoiceByItemIdAsync(int id)
        {
            var item = await _dbContext.InvoiceDetails
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.Invoice);

            var result = await FindByIdAsync(item.InvoiceId);

            return result;
        }

        public async Task<TblInvoiceDetails> FindInvoiceItemByIdAsync(int id)
        {
            return await _dbContext.InvoiceDetails.DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.Invoice);
        }

        public virtual async Task AddItemToInvoiceAsync(InvoiceDetailsItemType itemType, string itemName, string itemHomePage, int itemId, double priceInMainCurrency, int qty, Guid? invoiceId = null)
        {
            var invoice = invoiceId != null
                ? await FindByIdAsync(invoiceId.Value)
                : await GetUserCurrentInvoiceAsync();

            //User can add only one Subscription Plan
            if (itemType == InvoiceDetailsItemType.SubscriptionPlan)
            {
                var records = (await _dbContext.InvoiceDetails.Where(p =>
                        p.ItemType == InvoiceDetailsItemType.SubscriptionPlan
                        && p.InvoiceId == invoice.Id)
                    .FromCacheAsync(CacheTags.Invoice)).ToList();
                await _dbContext.InvoiceDetails.Where(p => p.ItemType == InvoiceDetailsItemType.SubscriptionPlan
                                                              && p.InvoiceId == invoice.Id).DeleteAsync();
                records.ForEach(x => _eventPublisher.EntityDeleted(x));
            }

            //if item already exist, increase it quantity
            var oldItems = invoice.InvoiceDetails?.FirstOrDefault(p => p.ItemId == itemId &&
                                                             p.InvoiceId == invoice.Id &&
                                                             p.ItemType == itemType &&
                                                             p.ItemType != InvoiceDetailsItemType.SubscriptionPlan &&
                                                             p.ItemType != InvoiceDetailsItemType.Other);
            if (oldItems != null)
            {
                await _dbContext.InvoiceDetails.Where(p => p.Id == oldItems.Id)
                    .UpdateAsync(p => new TblInvoiceDetails()
                    {
                        ItemHomePage = itemHomePage,
                        ItemName = itemName,
                        UnitPrice = priceInMainCurrency
                    });
                await IncreaseItemQtyAsync(oldItems.Id, qty);
            }
            else
            {
                var item = new TblInvoiceDetails()
                {
                    InvoiceId = invoice.Id,
                    ItemId = itemId,
                    ItemName = itemName,
                    ItemType = itemType,
                    Qty = qty,
                    UnitPrice = priceInMainCurrency,
                    ItemHomePage = itemHomePage
                };
                _dbContext.InvoiceDetails.Add(item);
                await _dbContext.SaveChangesAsync();

                _eventPublisher.EntityInserted(item);
            }

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            await ApplyInvoiceDiscountsAsync(invoice.Id);
            await ApplyInvoiceTaxesAsync(invoice.Id);
        }

        public virtual async Task RemoveItemAsync(int itemId)
        {
            var invoice = await FindInvoiceByItemIdAsync(itemId);
            //Check User
            if (!_workContext.IsAdmin)
            {
                var currentUserId = _workContext.CurrentUser?.Id;
                var records = (await _dbContext.InvoiceDetails.Where(p => p.Id == itemId &&
                                                                             (p.Invoice.UserId == null ||
                                                                              p.Invoice.UserId == currentUserId))
                    .FromCacheAsync(CacheTags.Invoice)).ToList();
                await _dbContext.InvoiceDetails
                    .Where(p => p.Id == itemId &&
                                (p.Invoice.UserId == null || p.Invoice.UserId == currentUserId))
                    .DeleteAsync();
                records.ForEach(x => _eventPublisher.EntityDeleted(x));
            }
            else
            {
                var records = (await _dbContext.InvoiceDetails.Where(p => p.Id == itemId)
                    .FromCacheAsync(CacheTags.Invoice)).ToList();
                await _dbContext.InvoiceDetails.Where(p => p.Id == itemId)
                    .DeleteAsync();
                records.ForEach(x => _eventPublisher.EntityDeleted(x));
            }

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            await ApplyInvoiceDiscountsAsync(invoice.Id);
            await ApplyInvoiceTaxesAsync(invoice.Id);
        }

        public virtual async Task SetItemLicenseAsync(int itemId, string license)
        {
            var oldRecord = await FindInvoiceItemByIdAsync(itemId);
            await _dbContext.InvoiceDetails.Where(p => p.Id == itemId)
                    .UpdateAsync(p => new TblInvoiceDetails() { ItemLicenseCode = license });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoiceDetails>();
            newRecord.ItemLicenseCode = license;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task SetItemExpirationAsync(int itemId, DateTime? expiration)
        {
            var oldRecord = await FindInvoiceItemByIdAsync(itemId);
            await _dbContext.InvoiceDetails.Where(p => p.Id == itemId)
                .UpdateAsync(p => new TblInvoiceDetails()
                {
                    PurchaseExpiration = expiration
                });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoiceDetails>();
            newRecord.PurchaseExpiration = expiration;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task IncreaseItemQtyAsync(int itemId, int qty = 1)
        {
            if (qty <= 0)
            {
                return;
            }
            var oldRecord = await FindInvoiceItemByIdAsync(itemId);
            if (oldRecord.UnitPrice <= 0)
            {
                return;
            }
            //Check User
            if (!_workContext.IsAdmin)
            {
                var currentUserId = _workContext.CurrentUser?.Id;
                await _dbContext.InvoiceDetails
                    .Where(p => p.Id == itemId &&
                                (p.Invoice.UserId == null || p.Invoice.UserId == currentUserId))
                    .UpdateAsync(p => new TblInvoiceDetails() { Qty = p.Qty + qty });
            }
            else
            {
                await _dbContext.InvoiceDetails.Where(p => p.Id == itemId)
                    .UpdateAsync(p => new TblInvoiceDetails() { Qty = p.Qty + qty });
            }

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoiceDetails>();
            newRecord.Qty += qty;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);

            var invoice = await FindInvoiceByItemIdAsync(itemId);
            await ApplyInvoiceDiscountsAsync(invoice.Id);
            await ApplyInvoiceTaxesAsync(invoice.Id);
        }

        public virtual async Task DecreaseItemQtyAsync(int itemId, int qty = 1)
        {
            if (qty <= 0)
            {
                return;
            }
            var oldRecord = await FindInvoiceItemByIdAsync(itemId);
            if (oldRecord.UnitPrice <= 0)
            {
                return;
            }
            //Check User
            if (!_workContext.IsAdmin)
            {
                var currentUserId = _workContext.CurrentUser?.Id;
                await _dbContext.InvoiceDetails
                    .Where(p => p.Id == itemId &&
                                (p.Invoice.UserId == null || p.Invoice.UserId == currentUserId))
                    .UpdateAsync(p => new TblInvoiceDetails() { Qty = p.Qty > qty ? p.Qty - qty : 1 });
            }
            else
            {
                await _dbContext.InvoiceDetails.Where(p => p.Id == itemId)
                    .UpdateAsync(p => new TblInvoiceDetails() { Qty = p.Qty > qty ? p.Qty - qty : 1 });
            }

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoiceDetails>();
            newRecord.Qty = newRecord.Qty > qty ? newRecord.Qty - qty : 1;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);

            var invoice = await FindInvoiceByItemIdAsync(itemId);
            await ApplyInvoiceDiscountsAsync(invoice.Id);
            await ApplyInvoiceTaxesAsync(invoice.Id);
        }

        public virtual async Task SetItemUnitPriceAsync(int itemId, double price)
        {
            var oldRecord = await FindInvoiceItemByIdAsync(itemId);
            await _dbContext.InvoiceDetails
                .Where(p => p.Id == itemId)
                .UpdateAsync(p => new TblInvoiceDetails() { UnitPrice = price });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoiceDetails>();
            newRecord.UnitPrice = price;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);

            var invoice = await FindInvoiceByItemIdAsync(itemId);
            await ApplyInvoiceDiscountsAsync(invoice.Id);
            await ApplyInvoiceTaxesAsync(invoice.Id);
        }

        #endregion


        #region Other Actions

        public virtual async Task CheckoutInvoiceAsync(Guid invoiceId, string transactionId)
        {
            var invoice = await FindByIdAsync(invoiceId, false);
            if (invoice == null)
            {
                return;
            }
            var paidAmount = invoice.ComputeInvoiceTotalAmount(false);
            await _dbContext.Invoices.Where(p => p.Id == invoice.Id)
                .UpdateAsync(p => new TblInvoices()
                {
                    PaidAmount = paidAmount.ExchangeCurrency(invoice.Currency),
                    PaidAmountInMainCurrency = paidAmount,
                    Status = InvoiceStatus.Paid
                });

            if (invoice.PaymentDate == null)
            {
                await _dbContext.Invoices.Where(p => p.Id == invoice.Id)
                    .UpdateAsync(p => new TblInvoices()
                    {
                        PaymentDate = DateTime.Now,
                    });
            }

            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                await _dbContext.Invoices.Where(p => p.Id == invoice.Id)
                    .UpdateAsync(p => new TblInvoices()
                    {
                        PaymentGatewayTransactionId = transactionId
                    });
            }

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            _eventPublisher.Publish(new InvoiceCheckoutEvent(invoice, transactionId, paidAmount,
                paidAmount.ExchangeCurrencyStr(), invoice.PaymentGatewayName, invoice.Currency.IsoCode));


            //Process User Group Upgrade
            var userGroupItem = invoice.InvoiceDetails.FirstOrDefault(p => p.ItemType == InvoiceDetailsItemType.SubscriptionPlan);
            if (userGroupItem != null)
            {
                var userGroup = await _userGroupsService.FindByIdAsync(userGroupItem.ItemId);
                if (userGroup != null && invoice.User != null)
                {
                    //Upgrade user plan
                    await _usersService.UpgradeCustomerUserGroupAsync(invoice.UserId, userGroup);

                    //Purchase Expiration
                    await SetItemExpirationAsync(userGroupItem.Id, invoice.PaymentDate?.AddTimePeriodToDateTime(userGroup.SubscriptionExpirationPeriodType, userGroup.SubscriptionExpirationTime));
                }
            }

            //Process Products
            foreach (var productItem in invoice.InvoiceDetails.Where(p => p.ItemType == InvoiceDetailsItemType.Product))
            {
                var product = await _productService.FindByIdAsync(productItem.ItemId);
                if (product != null)
                {
                    //Purchase Expiration
                    await SetItemExpirationAsync(productItem.Id, invoice.PaymentDate?.AddTimePeriodToDateTime(product.PurchaseExpirationTimeType, product.PurchaseExpiration));

                    //License Code
                    if (!string.IsNullOrWhiteSpace(product.LicenseGeneratorServiceId))
                    {
                        var licenseGenerator = _licenseManager.FindLicenseGeneratorById(product.LicenseGeneratorServiceId);
                        if (licenseGenerator != null)
                        {
                            var license = await licenseGenerator.GenerateLicenseForProductAsync(invoice, invoice.User, product, productItem.Qty);
                            await SetItemLicenseAsync(productItem.Id, license);
                        }
                    }
                }
            }

            //Process Products Attributes
            foreach (var attributeItem in invoice.InvoiceDetails.Where(p => p.ItemType == InvoiceDetailsItemType.ProductAttribute))
            {
                var attribute = await _productCheckoutAttributesService.FindByIdAsync(attributeItem.ItemId);
                if (attribute != null)
                {
                    //License Code
                    if (!string.IsNullOrWhiteSpace(attribute.LicenseGeneratorServiceId))
                    {
                        var licenseGenerator = _licenseManager.FindLicenseGeneratorById(attribute.LicenseGeneratorServiceId);
                        if (licenseGenerator != null)
                        {
                            var license = await licenseGenerator.GenerateLicenseForProductAttributeAsync(invoice, invoice.User, attribute, attributeItem.Qty);
                            await SetItemLicenseAsync(attributeItem.Id, license);
                        }
                    }
                }
            }

            //Process Products Attribute Options
            foreach (var attributeOptionItem in invoice.InvoiceDetails.Where(p => p.ItemType == InvoiceDetailsItemType.ProductAttributeOption))
            {
                var attributeOption = await _productCheckoutAttributesService.FindOptionByIdAsync(attributeOptionItem.ItemId);
                if (attributeOption != null)
                {
                    //Purchase Expiration
                    await SetItemExpirationAsync(attributeOptionItem.Id, invoice.PaymentDate?.AddTimePeriodToDateTime(attributeOption.PurchaseExpirationTimeType, attributeOption.PurchaseExpiration));

                    //License Code
                    if (!string.IsNullOrWhiteSpace(attributeOption.LicenseGeneratorServiceId))
                    {
                        var licenseGenerator = _licenseManager.FindLicenseGeneratorById(attributeOption.LicenseGeneratorServiceId);
                        if (licenseGenerator != null)
                        {
                            var license = await licenseGenerator.GenerateLicenseForProductAttributeOptionAsync(invoice, invoice.User, attributeOption, attributeOptionItem.Qty);
                            await SetItemLicenseAsync(attributeOptionItem.Id, license);
                        }
                    }
                }
            }
        }

        public virtual async Task<bool> UserCanEditInvoiceAsync(Guid id, string userId)
        {
            var invoice = await FindByIdAsync(id);
            if (!await UserHasAccessToInvoiceAsync(invoice, userId))
            {
                return false;
            }

            if (await _usersService.UserIsAdminAsync(userId)) return true;

            if (invoice.Status == InvoiceStatus.Pending) return true;

            return false;
        }

        public virtual async Task<TblInvoices> GetUserCurrentInvoiceAsync(bool createNewIfNull = true)
        {
            var currentUser = await _usersService.UserManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            TblInvoices invoice = null;
            if (HttpContext.Current?.Session["CurrentInvoice"] is string invoiceIdStr)
            {
                if (Guid.TryParse(invoiceIdStr, out Guid invoiceId))
                {
                    //from session
                    invoice = await FindByIdAsync(invoiceId);
                }
            }

            //from cookies
            if (invoice == null)
            {
                var cookie = HttpContext.Current?.Request.Cookies["UserSetting"] ?? new HttpCookie("UserSetting");
                if (!string.IsNullOrWhiteSpace(cookie?.Values["CurrentInvoice"]))
                {
                    if (Guid.TryParse(cookie.Values["CurrentInvoice"], out Guid invoiceId))
                    {
                        //from cookie
                        invoice = await FindByIdAsync(invoiceId);
                    }
                }
            }

            if (invoice?.Status == InvoiceStatus.Paid)
            {
                if (invoice.UserId == null && currentUser != null)
                {
                    //Set Invoice user Id if User loged in
                    await SetUserIdAsync(invoice.Id, currentUser.Id);
                }

                invoice = null;
            }

            if (invoice?.UserId != null)
            {
                invoice = null;
            }

            //get current user latest open invoice from DB
            if (currentUser != null && invoice == null)
            {
                invoice = await GetAsQueryable()
                    .Include(p => p.InvoiceDetails)
                    .Include(p => p.User)
                    .Include(p => p.Currency)
                    .Include(p => p.BillingAddress)
                    .OrderByDescending(p => p.CreateDate)
                    .AsNoTracking()
                    .DeferredFirstOrDefault(p => p.UserId == currentUser.Id && p.Status == InvoiceStatus.Pending)
                    .FromCacheAsync(CacheTags.Invoice);
            }

            if (invoice == null && createNewIfNull)
            {
                //Create new invoice
                invoice = new TblInvoices()
                {
                    CreateDate = DateTime.Now,
                    Status = InvoiceStatus.Pending,
                };
                await AddAsync(invoice);
            }


            if (invoice != null && invoice.UserId == null && currentUser != null)
            {
                //Set Invoice user Id, if User loged in
                invoice.UserId = currentUser.Id;
                await SetUserIdAsync(invoice.Id, currentUser.Id);
            }


            //Save Invoice Id in Session & user Cookies
            if (invoice != null && HttpContext.Current != null)
            {
                HttpContext.Current.Session["CurrentInvoice"] = invoice.Id.ToString();

                if (!HttpContext.Current.Response.HeadersWritten && !new HttpRequestWrapper(HttpContext.Current.Request).IsAjaxRequest())
                {
                    var responseCookie = HttpContext.Current.Request.Cookies["UserSetting"] ?? new HttpCookie("UserSetting");
                    responseCookie.Values["CurrentInvoice"] = invoice.Id.ToString();
                    responseCookie.Expires = DateTime.Now.AddYears(1).ToUniversalTime();
                    HttpContext.Current.Response.Cookies.Add(responseCookie);
                }
            }

            return invoice;
        }

        public virtual async Task SetInvoiceNoteAsync(Guid invoiceId, string note, bool isForAdmin)
        {
            var oldRecord = await FindByIdAsync(invoiceId);
            var newRecord = oldRecord.Adapt<TblInvoices>();
            if (isForAdmin)
            {
                await _dbContext.Invoices.Where(p => p.Id == invoiceId)
                    .UpdateAsync(p => new TblInvoices()
                    {
                        InvoiceNoteAdmin = note
                    });
                newRecord.InvoiceNoteAdmin = note;
            }
            else
            {
                await _dbContext.Invoices.Where(p => p.Id == invoiceId)
                    .UpdateAsync(p => new TblInvoices()
                    {
                        InvoiceNote = note
                    });
                newRecord.InvoiceNote = note;
            }

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task AddUpdateBillingAddressAsync(Guid invoiceId, TblInvoiceBillingAddress address)
        {
            var invoice = await FindByIdAsync(invoiceId);
            if (invoice != null)
            {
                if (invoice.BillingAddress.Any())
                {
                    var records = (await _dbContext.InvoiceBillingAddress.Where(p => p.InvoiceId == invoiceId)
                        .FromCacheAsync(CacheTags.Invoice)).ToList();
                    await _dbContext.InvoiceBillingAddress.Where(p => p.InvoiceId == invoiceId)
                        .DeleteAsync();

                    records.ForEach(x => _eventPublisher.EntityDeleted(x));
                }

                address.InvoiceId = invoiceId;
                _dbContext.InvoiceBillingAddress.Add(address);
                await _dbContext.SaveChangesAsync();

                QueryCacheManager.ExpireTag(CacheTags.Invoice);

                _eventPublisher.EntityInserted(address);
            }
        }

        public virtual async Task<TblInvoiceBillingAddress> FindUserLatestBillingAddressAsync(string userid)
        {
            var result = await _dbContext.Invoices.OrderByDescending(p => p.CreateDate)
                .DeferredFirstOrDefault(p => p.UserId == userid && p.BillingAddress.Any())
                .FromCacheAsync(CacheTags.Invoice);

            return result?.BillingAddress?.FirstOrDefault();
        }

        public virtual async Task SetGatewayNameAndTokenAsync(Guid invoiceId, string gatewayName, string gatewaySystemName, string token, int currencyId)
        {
            var oldRecord = await FindByIdAsync(invoiceId);
            await _dbContext.Invoices.Where(p => p.Id == invoiceId)
                .UpdateAsync(p => new TblInvoices()
                {
                    PaymentGatewayToken = token,
                    PaymentGatewaySystemName = gatewaySystemName,
                    PaymentGatewayName = gatewayName,
                    CurrencyId = currencyId
                });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoices>();
            newRecord.PaymentGatewayToken = token;
            newRecord.PaymentGatewaySystemName = gatewaySystemName;
            newRecord.PaymentGatewayName = gatewayName;
            newRecord.CurrencyId = currencyId;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task SetStatusAsync(Guid invoiceId, InvoiceStatus status)
        {
            var oldRecord = await FindByIdAsync(invoiceId);
            await _dbContext.Invoices.Where(p => p.Id == invoiceId)
                .UpdateAsync(p => new TblInvoices()
                {
                    Status = status
                });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoices>();
            newRecord.Status = status;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task SetPaymentDateAsync(Guid invoiceId, DateTime? paymentDate)
        {
            var oldRecord = await FindByIdAsync(invoiceId);
            await _dbContext.Invoices.Where(p => p.Id == invoiceId)
                .UpdateAsync(p => new TblInvoices()
                {
                    PaymentDate = paymentDate
                });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoices>();
            newRecord.PaymentDate = paymentDate;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task SetUserIdAsync(Guid invoiceId, string userId)
        {
            var oldRecord = await FindByIdAsync(invoiceId);
            await _dbContext.Invoices.Where(p => p.Id == invoiceId)
                .UpdateAsync(p => new TblInvoices()
                {
                    UserId = userId
                });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoices>();
            newRecord.UserId = userId;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task SetTaxAsync(Guid invoiceId, double? taxAmount, string taxDescription)
        {
            var oldRecord = await FindByIdAsync(invoiceId);
            await _dbContext.Invoices
                .Where(p => p.Id == invoiceId)
                .UpdateAsync(p => new TblInvoices()
                { TotalTaxAmount = taxAmount, TaxDescription = taxDescription });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoices>();
            newRecord.TotalTaxAmount = taxAmount;
            newRecord.TaxDescription = taxDescription;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task SetDiscountAsync(Guid invoiceId, double? discountAmount, string discountDescription)
        {
            var oldRecord = await FindByIdAsync(invoiceId);
            await _dbContext.Invoices
                .Where(p => p.Id == invoiceId)
                .UpdateAsync(p => new TblInvoices()
                { DiscountAmount = discountAmount, DiscountDescription = discountDescription });

            QueryCacheManager.ExpireTag(CacheTags.Invoice);

            var newRecord = oldRecord.Adapt<TblInvoices>();
            newRecord.DiscountAmount = discountAmount;
            newRecord.DiscountDescription = discountDescription;
            _eventPublisher.EntityUpdated(newRecord, oldRecord);
        }

        public virtual async Task<TblInvoices> ApplyInvoiceTaxesAsync(Guid invoiceId)
        {
            var invoice = await FindByIdAsync(invoiceId, false);
            if (invoice == null)
            {
                return null;
            }
            var invoiceTotalAmount = invoice.ComputeInvoiceTotalAmount(false, false);
            invoice.TotalTaxAmount = 0;
            invoice.TaxDescription = "";
            if (invoiceTotalAmount > 0)
            {
                foreach (var tax in (await _taxesService.GetAsEnumerableAsync()).Where(p => p.IsActive && p.Amount > 0))
                {
                    invoice.TotalTaxAmount += (invoiceTotalAmount * tax.Amount) / 100;
                    invoice.TaxDescription += $"{tax.GetLocalized(p => p.TaxName)} (%{tax.Amount}) + ";
                }
            }

            invoice.TaxDescription = invoice.TaxDescription.TrimEnd(" + ");

            var taxProcessors = _pluginFinder.GetPlugins<ITaxProcessor>();
            foreach (var taxProcessor in taxProcessors.OrderBy(p => p.Order))
            {
                var result = taxProcessor.ProcessorInvoice(invoice);
                if (result.Apply)
                {
                    invoice.TotalTaxAmount += result.TaxAmountInMainCurrency;
                    invoice.TaxDescription += result.TaxDescription + " + ";
                }
            }

            invoice.TaxDescription = invoice.TaxDescription.TrimEnd(" + ");
            await SetTaxAsync(invoice.Id, invoice.TotalTaxAmount, invoice.TaxDescription);
            return invoice;

        }

        public virtual async Task<TblInvoices> ApplyInvoiceDiscountsAsync(Guid invoiceId)
        {
            var invoice = await FindByIdAsync(invoiceId, false);
            if (invoice == null)
            {
                return null;
            }
            invoice.DiscountAmount = 0;
            invoice.DiscountDescription = "";
            var discountProcessors = _pluginFinder.GetPlugins<IDiscountProcessor>();
            foreach (var discountProcessor in discountProcessors.OrderBy(p => p.Order))
            {
                var result = discountProcessor.ProcessorInvoice(invoice);
                if (result.Apply)
                {
                    invoice.DiscountAmount += result.DiscountAmountInMainCurrency;
                    invoice.DiscountDescription += result.DiscountDescription + " + ";
                }
            }

            invoice.DiscountDescription = invoice.DiscountDescription.TrimEnd(" + ");
            await SetDiscountAsync(invoice.Id, invoice.DiscountAmount, invoice.DiscountDescription);
            return invoice;
        }

        public virtual async Task DeletePendingInvoices()
        {
            var settings = await _settingService.LoadSettingAsync<SiteSettings>();
            if (settings.DeletePendingInvoices || settings.DeleteEmptyInvoices)
            {
                if (settings.DeleteEmptyInvoices)
                {
                    var emptyFilterDate = DateTime.Now.AddDays(-settings.DeleteEmptyInvoicesAfterDays);
                    await _dbContext.Invoices
                        .Where(p => p.Status == InvoiceStatus.Pending && p.CreateDate < emptyFilterDate &&
                                    !p.InvoiceDetails.Any())
                        .DeleteAsync();
                }

                if (settings.DeletePendingInvoices)
                {
                    var pendingFilterDate = DateTime.Now.AddDays(-settings.DeletePendingInvoicesAfterDays);
                    await _dbContext.Invoices
                        .Where(p => p.Status == InvoiceStatus.Pending && p.CreateDate < pendingFilterDate)
                        .DeleteAsync();
                }

                QueryCacheManager.ExpireTag(CacheTags.Invoice);
            }
        }

        #endregion


        #region Report Generators

        public virtual async Task<Dictionary<DateTime, int>> InvoicesReportAsync(DateTime fromDate, DateTime toDate,
            TimePeriodType periodType, InvoiceStatus? status)
        {
            var query = _dbContext.Invoices.Where(p => p.CreateDate >= fromDate && p.CreateDate <= toDate);
            if (status != null)
            {
                query = query.Where(p => p.Status == status);
            }

            var invoices = await query.OrderBy(p => p.CreateDate)
                .Select(p => new { p.CreateDate }).FromCacheAsync(CacheTags.Invoice);

            var report = new Dictionary<DateTime, int>();
            var datetimeToStringFormat = "g";
            switch (periodType)
            {
                case TimePeriodType.Hour:
                    datetimeToStringFormat = "yyyy/MM/dd HH:mm";
                    report = invoices.GroupBy(p =>
                            new DateTime(p.CreateDate.Year, p.CreateDate.Month, p.CreateDate.Day, p.CreateDate.Hour, 0,
                                0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Day:
                    datetimeToStringFormat = "yyyy/MM/dd";
                    report = invoices.GroupBy(p =>
                            new DateTime(p.CreateDate.Year, p.CreateDate.Month, p.CreateDate.Day, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Month:
                    datetimeToStringFormat = "yyyy/MM";
                    report = invoices.GroupBy(p =>
                            new DateTime(p.CreateDate.Year, p.CreateDate.Month, 1, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Year:
                    datetimeToStringFormat = "yyyy";
                    report = invoices.GroupBy(p =>
                            new DateTime(p.CreateDate.Year, 1, 1, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
            }

            //Insert Zero Values
            var dateCounter = fromDate;
            while (dateCounter < toDate)
            {
                if (report.All(p => p.Key.ToString(datetimeToStringFormat) != dateCounter.ToString(datetimeToStringFormat)))
                {
                    report.Add(dateCounter, 0);
                }
                switch (periodType)
                {
                    case TimePeriodType.Hour:
                        dateCounter = dateCounter.AddHours(1);
                        break;
                    case TimePeriodType.Day:
                        dateCounter = dateCounter.AddDays(1);
                        break;
                    case TimePeriodType.Month:
                        dateCounter = dateCounter.AddMonths(1);
                        break;
                    case TimePeriodType.Year:
                        dateCounter = dateCounter.AddYears(1);
                        break;
                }
            }

            return report.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        }

        public virtual async Task<Dictionary<DateTime, double>> SellsReportAsync(DateTime fromDate, DateTime toDate, TimePeriodType periodType, int currencyId)
        {
            var query = _dbContext.Invoices.Where(p => p.CreateDate >= fromDate && p.CreateDate <= toDate && p.Status == InvoiceStatus.Paid && p.PaymentDate != null && p.CurrencyId == currencyId);

            var invoices = await query.OrderBy(p => p.PaymentDate).Select(p => new { p.PaymentDate, p.PaidAmount })
                .FromCacheAsync(CacheTags.Invoice);

            var report = new Dictionary<DateTime, double>();
            var datetimeToStringFormat = "g";
            switch (periodType)
            {
                case TimePeriodType.Hour:
                    datetimeToStringFormat = "yyyy/MM/dd HH:mm";
                    report = invoices.GroupBy(p =>
                            new DateTime(p.PaymentDate.Value.Year, p.PaymentDate.Value.Month, p.PaymentDate.Value.Day, p.PaymentDate.Value.Hour, 0,
                                0, 0))
                        .Select(p => new { date = p.Key, sum = p.Sum(x => x.PaidAmount) })
                        .ToDictionary(p => p.date, p => p.sum);
                    break;
                case TimePeriodType.Day:
                    datetimeToStringFormat = "yyyy/MM/dd";
                    report = invoices.GroupBy(p =>
                            new DateTime(p.PaymentDate.Value.Year, p.PaymentDate.Value.Month, p.PaymentDate.Value.Day, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, sum = p.Sum(x => x.PaidAmount) })
                        .ToDictionary(p => p.date, p => p.sum);
                    break;
                case TimePeriodType.Month:
                    datetimeToStringFormat = "yyyy/MM";
                    report = invoices.GroupBy(p =>
                            new DateTime(p.PaymentDate.Value.Year, p.PaymentDate.Value.Month, 1, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, sum = p.Sum(x => x.PaidAmount) })
                        .ToDictionary(p => p.date, p => p.sum);
                    break;
                case TimePeriodType.Year:
                    datetimeToStringFormat = "yyyy";
                    report = invoices.GroupBy(p =>
                            new DateTime(p.PaymentDate.Value.Year, 1, 1, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, sum = p.Sum(x => x.PaidAmount) })
                        .ToDictionary(p => p.date, p => p.sum);
                    break;
            }

            //Insert Zero Values
            var dateCounter = fromDate;
            while (dateCounter < toDate)
            {
                if (report.All(p => p.Key.ToString(datetimeToStringFormat) != dateCounter.ToString(datetimeToStringFormat)))
                {
                    report.Add(dateCounter, 0);
                }
                switch (periodType)
                {
                    case TimePeriodType.Hour:
                        dateCounter = dateCounter.AddHours(1);
                        break;
                    case TimePeriodType.Day:
                        dateCounter = dateCounter.AddDays(1);
                        break;
                    case TimePeriodType.Month:
                        dateCounter = dateCounter.AddMonths(1);
                        break;
                    case TimePeriodType.Year:
                        dateCounter = dateCounter.AddYears(1);
                        break;
                }
            }

            return report.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        }

        #endregion


        #region Private Functions

        protected virtual async Task<bool> UserHasAccessToInvoiceAsync(TblInvoices invoice, string userId)
        {
            if (invoice == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return invoice.UserId == null;
            }
            else
            {
                if (await _usersService.UserIsAdminAsync(userId)) return true;

                return userId == invoice.UserId || invoice.UserId == null;
            }
        }

        #endregion
    }
}
