namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.EntityUtilities;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

public sealed class AddInHeaderExtraDatasToRequest
    : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private readonly ICustomerOrderUtilities customerOrderUtilities;

    public AddInHeaderExtraDatasToRequest(ICustomerOrderUtilities customerOrderUtilities)
    {
        this.customerOrderUtilities = customerOrderUtilities;
    }

    public int Order => 500;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddInHeaderExtraDatasToRequest)} Started.");

        result.SFOEOrderTotLoadV4Request.Inheaderextradata = this.GetInHeaderExtraDatas(parameter);

        parameter.JobLogger?.Debug($"{nameof(AddInHeaderExtraDatasToRequest)} Finished.");

        return result;
    }

    private List<Inheaderextradata2> GetInHeaderExtraDatas(SFOEOrderTotLoadV4Parameter parameter)
    {
        var inHeaderExtraData2s = new List<Inheaderextradata2>();

        inHeaderExtraData2s.AddRange(
            GetInHeaderExtraData2ShippingCharges(
                parameter.CustomerOrder,
                inHeaderExtraData2s.Count + 1
            )
        );
        inHeaderExtraData2s.AddRange(
            GetInHeaderExtraData2HandlingCharges(
                parameter.CustomerOrder,
                inHeaderExtraData2s.Count + 1
            )
        );
        inHeaderExtraData2s.AddRange(
            GetInHeaderExtraData2OtherCharges(
                parameter.CustomerOrder,
                inHeaderExtraData2s.Count + 1
            )
        );

        inHeaderExtraData2s.Add(
            GetInHeaderExtraData2TermsCode(parameter.CustomerOrder, inHeaderExtraData2s.Count + 1)
        );
        inHeaderExtraData2s.Add(
            GetInHeaderExtraData2WebOrderNumber(
                parameter.CustomerOrder,
                inHeaderExtraData2s.Count + 1
            )
        );

        inHeaderExtraData2s.AddRange(
            this.GetInHeaderExtraData2DiscountAmount(
                parameter.CustomerOrder,
                inHeaderExtraData2s.Count + 1
            )
        );
        inHeaderExtraData2s.AddRange(
            GetInHeaderExtraData2DoNotRecalculatePrice(
                parameter.CustomerOrder,
                inHeaderExtraData2s.Count + 1
            )
        );

        return inHeaderExtraData2s;
    }

    private static List<Inheaderextradata2> GetInHeaderExtraData2ShippingCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (customerOrder.ShippingCharges <= 0)
        {
            return new List<Inheaderextradata2>();
        }

        return new List<Inheaderextradata2>
        {
            new Inheaderextradata2
            {
                FieldName = "addon",
                FieldValue = $"addonno=2\taddonamt={customerOrder.ShippingCharges}\taddontype=$",
                SequenceNumber = sequenceNumber
            }
        };
    }

    private static List<Inheaderextradata2> GetInHeaderExtraData2HandlingCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (customerOrder.HandlingCharges <= 0)
        {
            return new List<Inheaderextradata2>();
        }

        return new List<Inheaderextradata2>
        {
            new Inheaderextradata2
            {
                FieldName = "addon",
                FieldValue = $"addonno=3\taddonamt={customerOrder.HandlingCharges}\taddontype=$",
                SequenceNumber = sequenceNumber
            }
        };
    }

    private static List<Inheaderextradata2> GetInHeaderExtraData2OtherCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (customerOrder.OtherCharges <= 0)
        {
            return new List<Inheaderextradata2>();
        }

        return new List<Inheaderextradata2>
        {
            new Inheaderextradata2
            {
                FieldName = "addon",
                FieldValue = $"addonno=10\taddonamt={customerOrder.OtherCharges}\taddontype=$",
                SequenceNumber = sequenceNumber
            }
        };
    }

    private static Inheaderextradata2 GetInHeaderExtraData2TermsCode(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        return new Inheaderextradata2
        {
            FieldName = "termstype",
            FieldValue = customerOrder.TermsCode,
            SequenceNumber = sequenceNumber
        };
    }

    private static Inheaderextradata2 GetInHeaderExtraData2WebOrderNumber(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        return new Inheaderextradata2
        {
            FieldName = "iondata",
            FieldValue = customerOrder.OrderNumber,
            SequenceNumber = sequenceNumber
        };
    }

    private List<Inheaderextradata2> GetInHeaderExtraData2DiscountAmount(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        var discountAmount = this.customerOrderUtilities.GetPromotionOrderDiscountTotal(
            customerOrder
        );
        if (discountAmount <= 0)
        {
            return new List<Inheaderextradata2>();
        }

        return new List<Inheaderextradata2>
        {
            new Inheaderextradata2
            {
                FieldName = "discountamt",
                FieldValue = discountAmount.ToString(),
                SequenceNumber = sequenceNumber
            }
        };
    }

    private static List<Inheaderextradata2> GetInHeaderExtraData2DoNotRecalculatePrice(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (!customerOrder.OrderLines.Any(o => o.CustomerOrderPromotions.Any()))
        {
            return new List<Inheaderextradata2>();
        }

        return new List<Inheaderextradata2>
        {
            new Inheaderextradata2
            {
                FieldName = "donotrecalculateprice",
                FieldValue = "true",
                SequenceNumber = sequenceNumber
            }
        };
    }
}
