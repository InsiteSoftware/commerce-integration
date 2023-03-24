namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public sealed class AddInInLinesToRequest
    : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public int Order => 800;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddInInLinesToRequest)} Started.");

        result.SFOEOrderTotLoadV2Request.arrayInline = GetInInLines(parameter);

        parameter.JobLogger?.Debug($"{nameof(AddInInLinesToRequest)} Finished.");

        return result;
    }

    private static List<SFOEOrderTotLoadV2inputInline> GetInInLines(
        SFOEOrderTotLoadV2Parameter parameter
    )
    {
        var inInLine1s = new List<SFOEOrderTotLoadV2inputInline>();

        inInLine1s.AddRange(GetNotes(parameter.CustomerOrder.Notes, "/"));

        foreach (var orderLine in parameter.CustomerOrder.OrderLines)
        {
            var inInLine3 = new SFOEOrderTotLoadV2inputInline
            {
                actualSellPrice = orderLine.UnitNetPrice,
                itemDescription1 = orderLine.Description,
                itemNumber = orderLine.Product.ErpNumber,
                lineItemType = "I",
                listPrice = orderLine.UnitListPrice.ToString(),
                orderQty = orderLine.QtyOrdered,
                unitOfMeasure = orderLine.UnitOfMeasure,
                warehouseID = GetWarehouseId(parameter.CustomerOrder, orderLine)
            };

            inInLine1s.Add(inInLine3);

            inInLine1s.AddRange(GetNotes(orderLine.Notes, "&"));
        }

        for (var i = 0; i < inInLine1s.Count; i++)
        {
            inInLine1s[i].sequenceNumber = i + 1;
        }

        return inInLine1s;
    }

    private static List<SFOEOrderTotLoadV2inputInline> GetNotes(string notes, string itemNumber)
    {
        var inInLine1s = new List<SFOEOrderTotLoadV2inputInline>();
        var splitNotes = SplitNotes(notes);

        foreach (var splitNote in splitNotes)
        {
            inInLine1s.Add(
                new SFOEOrderTotLoadV2inputInline
                {
                    itemNumber = itemNumber,
                    lineItemType = "cp",
                    itemDescription1 = splitNote
                }
            );
        }

        return inInLine1s;
    }

    private static List<string> SplitNotes(string notes)
    {
        // notes are 60 character max and split on white space only
        var splitNotes = new List<string>();
        var words = notes.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (splitNotes.Any() && splitNotes[splitNotes.Count - 1].Length + word.Length + 1 <= 31)
            {
                splitNotes[splitNotes.Count - 1] = $"{splitNotes[splitNotes.Count - 1]} {word}";
            }
            else
            {
                splitNotes.Add(word);
            }
        }

        return splitNotes;
    }

    private static string GetWarehouseId(CustomerOrder customerOrder, OrderLine orderLine)
    {
        return orderLine.Warehouse?.Name ?? customerOrder.DefaultWarehouse?.Name ?? string.Empty;
    }
}
