namespace Insite.Integration.Connector.Acumatica.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Caching;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.EntityUtilities;
using Insite.Core.Plugins.Utilities;
using Insite.Core.SystemSetting.Groups.Catalog;
using Insite.Data.Entities;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Plugins.Pricing;

[DependencyName(nameof(IntegrationConnectorType.Acumatica))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Acumatica)]
public sealed class PricingServiceAcumatica : PricingServiceGeneric
{
    public PricingServiceAcumatica(
        IUnitOfWorkFactory unitOfWorkFactory,
        ICurrencyFormatProvider currencyFormatProvider,
        IOrderLineUtilities orderLineUtilities,
        IPricingServiceFactory pricingServiceFactory,
        IPerRequestCacheManager perRequestCacheManager,
        IPriceMatrixUtilities priceMatrixUtilities,
        PricingSettings pricingSettings
    )
        : base(
            unitOfWorkFactory,
            currencyFormatProvider,
            orderLineUtilities,
            pricingServiceFactory,
            perRequestCacheManager,
            priceMatrixUtilities,
            pricingSettings
        ) { }

    protected override void SetupRecordTypeMatrixQueries(
        PricingServiceParameter pricingServiceParameter,
        bool isUnitListPrice
    )
    {
        // GOAL: allow derived classes to write/update/remove queries that execute on the in memory pricematrix list based on recordtype.
        this.RecordTypeFuncs.Clear();

        // Use the customer's pricing customer, if they have one, to gather values
        var customer = isUnitListPrice ? null : this.GetPricingCustomer();

        // default values for variables we intend to use
        var customerId = customer?.Id.ToString() ?? string.Empty;
        var customerPriceCode = customer?.PriceCode.Trim() ?? string.Empty;
        var parentCustomerId = customer?.Parent?.Id.ToString() ?? string.Empty;
        var parentCustomerPriceCode = customer?.Parent?.PriceCode.Trim() ?? string.Empty;

        if (!customerId.IsBlank())
        {
            this.RecordTypeFuncs.Add(
                PriceMatrix.RecordTypeName.CustomerProductPromotion,
                query =>
                    query.Where(
                        pm =>
                            pm.RecordType.Equals(
                                PriceMatrix.RecordTypeName.CustomerProductPromotion,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.CustomerKeyPart.Equals(
                                customerId,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.ProductKeyPart.Equals(
                                this.Product.Id.ToString(),
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
            );

            this.RecordTypeFuncs.Add(
                PriceMatrix.RecordTypeName.CustomerProduct,
                query =>
                    query.Where(
                        pm =>
                            pm.RecordType.Equals(
                                PriceMatrix.RecordTypeName.CustomerProduct,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.CustomerKeyPart.Equals(
                                customerId,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.ProductKeyPart.Equals(
                                this.Product.Id.ToString(),
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
            );
        }

        if (!parentCustomerId.IsBlank())
        {
            this.RecordTypeFuncs.Add(
                PriceMatrix.RecordTypeName.CustomerProductPromotion,
                query =>
                    query.Where(
                        pm =>
                            pm.RecordType.Equals(
                                PriceMatrix.RecordTypeName.CustomerProductPromotion,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.CustomerKeyPart.Equals(
                                parentCustomerId,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.ProductKeyPart.Equals(
                                this.Product.Id.ToString(),
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
            );

            this.RecordTypeFuncs.Add(
                PriceMatrix.RecordTypeName.CustomerProduct,
                query =>
                    query.Where(
                        pm =>
                            pm.RecordType.Equals(
                                PriceMatrix.RecordTypeName.CustomerProduct,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.CustomerKeyPart.Equals(
                                parentCustomerId,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.ProductKeyPart.Equals(
                                this.Product.Id.ToString(),
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
            );
        }

        if (!customerPriceCode.IsBlank())
        {
            this.RecordTypeFuncs.Add(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProductPromotion,
                query =>
                    query.Where(
                        pm =>
                            pm.RecordType.Equals(
                                PriceMatrix.RecordTypeName.CustomerPriceCodeProductPromotion,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.CustomerKeyPart.Equals(
                                customerPriceCode,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.ProductKeyPart.Equals(
                                this.Product.Id.ToString(),
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
            );

            this.RecordTypeFuncs.Add(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProduct,
                query =>
                    query.Where(
                        pm =>
                            pm.RecordType.Equals(
                                PriceMatrix.RecordTypeName.CustomerPriceCodeProduct,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.CustomerKeyPart.Equals(
                                customerPriceCode,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.ProductKeyPart.Equals(
                                this.Product.Id.ToString(),
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
            );
        }

        if (!parentCustomerPriceCode.IsBlank() && customerPriceCode.IsBlank())
        {
            this.RecordTypeFuncs.Add(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProductPromotion,
                query =>
                    query.Where(
                        pm =>
                            pm.RecordType.Equals(
                                PriceMatrix.RecordTypeName.CustomerPriceCodeProductPromotion,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.CustomerKeyPart.Equals(
                                parentCustomerPriceCode,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.ProductKeyPart.Equals(
                                this.Product.Id.ToString(),
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
            );

            this.RecordTypeFuncs.Add(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProduct,
                query =>
                    query.Where(
                        pm =>
                            pm.RecordType.Equals(
                                PriceMatrix.RecordTypeName.CustomerPriceCodeProduct,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.CustomerKeyPart.Equals(
                                parentCustomerPriceCode,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && pm.ProductKeyPart.Equals(
                                this.Product.Id.ToString(),
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
            );
        }

        this.RecordTypeFuncs.Add(
            PriceMatrix.RecordTypeName.ProductPromotion,
            query =>
                query.Where(
                    pm =>
                        pm.RecordType.Equals(
                            PriceMatrix.RecordTypeName.ProductPromotion,
                            StringComparison.OrdinalIgnoreCase
                        )
                        && pm.CustomerKeyPart.Equals(
                            string.Empty,
                            StringComparison.OrdinalIgnoreCase
                        )
                        && pm.ProductKeyPart.Equals(
                            this.Product.Id.ToString(),
                            StringComparison.OrdinalIgnoreCase
                        )
                )
        );

        this.RecordTypeFuncs.Add(
            PriceMatrix.RecordTypeName.Product,
            query =>
                query.Where(
                    pm =>
                        pm.RecordType.Equals(
                            PriceMatrix.RecordTypeName.Product,
                            StringComparison.OrdinalIgnoreCase
                        )
                        && pm.CustomerKeyPart.Equals(
                            string.Empty,
                            StringComparison.OrdinalIgnoreCase
                        )
                        && pm.ProductKeyPart.Equals(
                            this.Product.Id.ToString(),
                            StringComparison.OrdinalIgnoreCase
                        )
                )
        );
    }

    protected override PriceMatrix GetPriceMatrix(
        IList<PriceMatrix> priceMatrixList,
        PricingServiceParameter pricingServiceParameter,
        bool isUnitListPrice
    )
    {
        var priceMatrixListRequestedUnitOfMeasure = priceMatrixList
            .Where(
                o =>
                    o.UnitOfMeasure.Equals(
                        pricingServiceParameter.UnitOfMeasure,
                        StringComparison.OrdinalIgnoreCase
                    )
            )
            .ToList();
        var priceMatrixListBaseUnitOfMeasure = priceMatrixList
            .Where(
                o =>
                    o.UnitOfMeasure.Equals(
                        this.Product.UnitOfMeasure,
                        StringComparison.OrdinalIgnoreCase
                    )
            )
            .ToList();

        return this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerProductPromotion,
                priceMatrixListRequestedUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerProduct,
                priceMatrixListRequestedUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerProductPromotion,
                priceMatrixListBaseUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerProduct,
                priceMatrixListBaseUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProductPromotion,
                priceMatrixListRequestedUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProduct,
                priceMatrixListRequestedUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProductPromotion,
                priceMatrixListBaseUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProduct,
                priceMatrixListBaseUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.ProductPromotion,
                priceMatrixListRequestedUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.Product,
                priceMatrixListRequestedUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.ProductPromotion,
                priceMatrixListBaseUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.Product,
                priceMatrixListBaseUnitOfMeasure,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerProductPromotion,
                priceMatrixList,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerProduct,
                priceMatrixList,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProductPromotion,
                priceMatrixList,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.CustomerPriceCodeProduct,
                priceMatrixList,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.ProductPromotion,
                priceMatrixList,
                pricingServiceParameter,
                isUnitListPrice
            )
            ?? this.CombPriceMatrix(
                PriceMatrix.RecordTypeName.Product,
                priceMatrixList,
                pricingServiceParameter,
                isUnitListPrice
            );
    }
}
