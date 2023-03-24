namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.EntityUtilities;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public sealed class AddInHeaderExtraDatasToRequest
    : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    private readonly ICustomerOrderUtilities customerOrderUtilities;

    public AddInHeaderExtraDatasToRequest(ICustomerOrderUtilities customerOrderUtilities)
    {
        this.customerOrderUtilities = customerOrderUtilities;
    }

    public int Order => 500;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddInHeaderExtraDatasToRequest)} Started.");

        result.SFOEOrderTotLoadV2Request.arrayHeaderextradata = this.GetInHeaderExtraDatas(
            parameter
        );

        parameter.JobLogger?.Debug($"{nameof(AddInHeaderExtraDatasToRequest)} Finished.");

        return result;
    }

    private List<SFOEOrderTotLoadV2inputHeaderextradata> GetInHeaderExtraDatas(
        SFOEOrderTotLoadV2Parameter parameter
    )
    {
        var inHeaderExtraDatas = new List<SFOEOrderTotLoadV2inputHeaderextradata>();

        inHeaderExtraDatas.AddRange(
            GetInHeaderExtraDataShippingCharges(
                parameter.CustomerOrder,
                inHeaderExtraDatas.Count + 1
            )
        );
        inHeaderExtraDatas.AddRange(
            GetInHeaderExtraDataHandlingCharges(
                parameter.CustomerOrder,
                inHeaderExtraDatas.Count + 1
            )
        );
        inHeaderExtraDatas.AddRange(
            GetInHeaderExtraDataOtherCharges(parameter.CustomerOrder, inHeaderExtraDatas.Count + 1)
        );

        inHeaderExtraDatas.Add(
            GetInHeaderExtraDataTermsCode(parameter.CustomerOrder, inHeaderExtraDatas.Count + 1)
        );
        inHeaderExtraDatas.Add(
            GetInHeaderExtraDataWebOrderNumber(
                parameter.CustomerOrder,
                inHeaderExtraDatas.Count + 1
            )
        );

        inHeaderExtraDatas.AddRange(
            this.GetInHeaderExtraDataDiscountAmount(
                parameter.CustomerOrder,
                inHeaderExtraDatas.Count + 1
            )
        );
        inHeaderExtraDatas.AddRange(
            GetInHeaderExtraDataDoNotRecalculatePrice(
                parameter.CustomerOrder,
                inHeaderExtraDatas.Count + 1
            )
        );

        return inHeaderExtraDatas;
    }

    private static List<SFOEOrderTotLoadV2inputHeaderextradata> GetInHeaderExtraDataShippingCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (customerOrder.ShippingCharges <= 0)
        {
            return new List<SFOEOrderTotLoadV2inputHeaderextradata>();
        }

        return new List<SFOEOrderTotLoadV2inputHeaderextradata>
        {
            new SFOEOrderTotLoadV2inputHeaderextradata
            {
                fieldName = "addon",
                fieldValue = $"addonno=2\taddonamt={customerOrder.ShippingCharges}\taddontype=$",
                sequenceNumber = sequenceNumber
            }
        };
    }

    private static List<SFOEOrderTotLoadV2inputHeaderextradata> GetInHeaderExtraDataHandlingCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (customerOrder.HandlingCharges <= 0)
        {
            return new List<SFOEOrderTotLoadV2inputHeaderextradata>();
        }

        return new List<SFOEOrderTotLoadV2inputHeaderextradata>
        {
            new SFOEOrderTotLoadV2inputHeaderextradata
            {
                fieldName = "addon",
                fieldValue = $"addonno=3\taddonamt={customerOrder.HandlingCharges}\taddontype=$",
                sequenceNumber = sequenceNumber
            }
        };
    }

    private static List<SFOEOrderTotLoadV2inputHeaderextradata> GetInHeaderExtraDataOtherCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (customerOrder.OtherCharges <= 0)
        {
            return new List<SFOEOrderTotLoadV2inputHeaderextradata>();
        }

        return new List<SFOEOrderTotLoadV2inputHeaderextradata>
        {
            new SFOEOrderTotLoadV2inputHeaderextradata
            {
                fieldName = "addon",
                fieldValue = $"addonno=10\taddonamt={customerOrder.OtherCharges}\taddontype=$",
                sequenceNumber = sequenceNumber
            }
        };
    }

    private static SFOEOrderTotLoadV2inputHeaderextradata GetInHeaderExtraDataTermsCode(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        return new SFOEOrderTotLoadV2inputHeaderextradata
        {
            fieldName = "termstype",
            fieldValue = customerOrder.TermsCode,
            sequenceNumber = sequenceNumber
        };
    }

    private static SFOEOrderTotLoadV2inputHeaderextradata GetInHeaderExtraDataWebOrderNumber(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        return new SFOEOrderTotLoadV2inputHeaderextradata
        {
            fieldName = "iondata",
            fieldValue = customerOrder.OrderNumber,
            sequenceNumber = sequenceNumber
        };
    }

    private List<SFOEOrderTotLoadV2inputHeaderextradata> GetInHeaderExtraDataDiscountAmount(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        var discountAmount = this.customerOrderUtilities.GetPromotionOrderDiscountTotal(
            customerOrder
        );
        if (discountAmount <= 0)
        {
            return new List<SFOEOrderTotLoadV2inputHeaderextradata>();
        }

        return new List<SFOEOrderTotLoadV2inputHeaderextradata>
        {
            new SFOEOrderTotLoadV2inputHeaderextradata
            {
                fieldName = "discountamt",
                fieldValue = discountAmount.ToString(),
                sequenceNumber = sequenceNumber
            }
        };
    }

    private static List<SFOEOrderTotLoadV2inputHeaderextradata> GetInHeaderExtraDataDoNotRecalculatePrice(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (!customerOrder.OrderLines.Any(o => o.CustomerOrderPromotions.Any()))
        {
            return new List<SFOEOrderTotLoadV2inputHeaderextradata>();
        }

        return new List<SFOEOrderTotLoadV2inputHeaderextradata>
        {
            new SFOEOrderTotLoadV2inputHeaderextradata
            {
                fieldName = "donotrecalculateprice",
                fieldValue = "true",
                sequenceNumber = sequenceNumber
            }
        };
    }
}
