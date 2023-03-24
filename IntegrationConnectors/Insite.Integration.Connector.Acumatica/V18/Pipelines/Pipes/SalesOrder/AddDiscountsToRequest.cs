namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;

using System.Collections.Generic;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.EntityUtilities;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;

public sealed class AddDiscountsToRequest : IPipe<SalesOrderParameter, SalesOrderResult>
{
    private readonly ICustomerOrderUtilities customerOrderUtilities;

    public AddDiscountsToRequest(ICustomerOrderUtilities customerOrderUtilities)
    {
        this.customerOrderUtilities = customerOrderUtilities;
    }

    public int Order => 600;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddDiscountsToRequest)} Started.");

        result.SalesOrderRequest.DiscountDetails = this.GetDiscountDetails(parameter.CustomerOrder);

        parameter.JobLogger?.Debug($"{nameof(AddDiscountsToRequest)} Finished.");

        return result;
    }

    private List<Discountdetail> GetDiscountDetails(CustomerOrder customerOrder)
    {
        var discountDetails = new List<Discountdetail>();
        var rowNumber = 1;

        foreach (var customerOrderPromotion in customerOrder.CustomerOrderPromotions)
        {
            discountDetails.Add(
                new Discountdetail
                {
                    rowNumber = rowNumber,
                    Description = customerOrderPromotion.Promotion.Description,
                    DiscountableAmount = this.GetDiscountableAmount(
                        customerOrder,
                        customerOrderPromotion
                    ),
                    DiscountableQty = customerOrderPromotion.OrderLine?.QtyOrdered ?? 1,
                    DiscountAmount = customerOrderPromotion.Amount ?? 0,
                    DiscountCode = customerOrderPromotion.Promotion.Name,
                    DiscountPercent = GetDiscountPercent(customerOrderPromotion),
                    FreeItem = customerOrderPromotion.PromotionResult.Product?.ErpNumber,
                    FreeItemQty = GetFreeItemQty(customerOrderPromotion),
                    ManualDiscount = true
                }
            );

            rowNumber++;
        }

        return discountDetails;
    }

    private decimal GetDiscountableAmount(
        CustomerOrder customerOrder,
        CustomerOrderPromotion customerOrderPromotion
    )
    {
        if (customerOrderPromotion.OrderLine == null)
        {
            return this.customerOrderUtilities.GetOrderSubTotalWithOutProductDiscounts(
                customerOrder
            );
        }
        else
        {
            return customerOrderPromotion.OrderLine.UnitNetPrice + customerOrderPromotion.Amount
                ?? 0;
        }
    }

    private static decimal GetDiscountPercent(CustomerOrderPromotion customerOrderPromotion)
    {
        if (!(customerOrderPromotion.PromotionResult.IsPercent ?? false))
        {
            return 0;
        }

        return customerOrderPromotion.Amount ?? 0;
    }

    private static decimal GetFreeItemQty(CustomerOrderPromotion customerOrderPromotion)
    {
        if (customerOrderPromotion.PromotionResult.Product == null)
        {
            return 0;
        }

        return customerOrderPromotion.PromotionResult.Amount ?? 0;
    }
}
