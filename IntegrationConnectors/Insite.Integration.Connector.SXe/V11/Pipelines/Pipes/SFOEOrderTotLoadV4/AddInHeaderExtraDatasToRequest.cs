namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.EntityUtilities;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;

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

        result.SFOEOrderTotLoadV4Request.Request.InputHeaderExtraDataCollection =
            new InputHeaderExtraDataCollection
            {
                InputHeaderExtraDatas = this.GetInHeaderExtraDatas(parameter)
            };

        parameter.JobLogger?.Debug($"{nameof(AddInHeaderExtraDatasToRequest)} Finished.");

        return result;
    }

    private List<InputHeaderExtraData> GetInHeaderExtraDatas(SFOEOrderTotLoadV4Parameter parameter)
    {
        var inHeaderExtraData2s = new List<InputHeaderExtraData>();

        inHeaderExtraData2s.AddRange(
            this.GetInHeaderExtraData2ShippingCharges(
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

    private List<InputHeaderExtraData> GetInHeaderExtraData2ShippingCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        var shippingDiscount = this.customerOrderUtilities.GetPromotionShippingDiscountTotal(
            customerOrder
        );
        var totalShippingCharges = customerOrder.ShippingCharges - shippingDiscount;
        if (totalShippingCharges <= 0)
        {
            return new List<InputHeaderExtraData>();
        }

        return new List<InputHeaderExtraData>
        {
            new InputHeaderExtraData
            {
                Fieldname = "addon",
                Fieldvalue = $"addonno=2\taddonamt={totalShippingCharges}\taddontype=$",
                Seqno = sequenceNumber
            }
        };
    }

    private static List<InputHeaderExtraData> GetInHeaderExtraData2HandlingCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (customerOrder.HandlingCharges <= 0)
        {
            return new List<InputHeaderExtraData>();
        }

        return new List<InputHeaderExtraData>
        {
            new InputHeaderExtraData
            {
                Fieldname = "addon",
                Fieldvalue = $"addonno=3\taddonamt={customerOrder.HandlingCharges}\taddontype=$",
                Seqno = sequenceNumber
            }
        };
    }

    private static List<InputHeaderExtraData> GetInHeaderExtraData2OtherCharges(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (customerOrder.OtherCharges <= 0)
        {
            return new List<InputHeaderExtraData>();
        }

        return new List<InputHeaderExtraData>
        {
            new InputHeaderExtraData
            {
                Fieldname = "addon",
                Fieldvalue = $"addonno=10\taddonamt={customerOrder.OtherCharges}\taddontype=$",
                Seqno = sequenceNumber
            }
        };
    }

    private static InputHeaderExtraData GetInHeaderExtraData2TermsCode(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        return new InputHeaderExtraData
        {
            Fieldname = "termstype",
            Fieldvalue = customerOrder.TermsCode,
            Seqno = sequenceNumber
        };
    }

    private static InputHeaderExtraData GetInHeaderExtraData2WebOrderNumber(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        return new InputHeaderExtraData
        {
            Fieldname = "iondata",
            Fieldvalue = customerOrder.OrderNumber,
            Seqno = sequenceNumber
        };
    }

    private List<InputHeaderExtraData> GetInHeaderExtraData2DiscountAmount(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        var discountAmount = this.customerOrderUtilities.GetPromotionOrderDiscountTotal(
            customerOrder
        );
        if (discountAmount <= 0)
        {
            return new List<InputHeaderExtraData>();
        }

        return new List<InputHeaderExtraData>
        {
            new InputHeaderExtraData
            {
                Fieldname = "discountamt",
                Fieldvalue = discountAmount.ToString(),
                Seqno = sequenceNumber
            }
        };
    }

    private static List<InputHeaderExtraData> GetInHeaderExtraData2DoNotRecalculatePrice(
        CustomerOrder customerOrder,
        int sequenceNumber
    )
    {
        if (!customerOrder.OrderLines.Any(o => o.CustomerOrderPromotions.Any()))
        {
            return new List<InputHeaderExtraData>();
        }

        return new List<InputHeaderExtraData>
        {
            new InputHeaderExtraData
            {
                Fieldname = "donotrecalculateprice",
                Fieldvalue = "true",
                Seqno = sequenceNumber
            }
        };
    }
}
