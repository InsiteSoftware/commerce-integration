namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

public sealed class AddInInLinesToRequest
    : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public int Order => 900;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddInInLinesToRequest)} Started.");

        result.SFOEOrderTotLoadV4Request.Ininline = GetInInLines(parameter);

        parameter.JobLogger?.Debug($"{nameof(AddInInLinesToRequest)} Finished.");

        return result;
    }

    private static List<Ininline3> GetInInLines(SFOEOrderTotLoadV4Parameter parameter)
    {
        var inInLine3s = new List<Ininline3>();

        inInLine3s.AddRange(GetNotes(parameter.CustomerOrder.Notes, "/"));

        foreach (var orderLine in parameter.CustomerOrder.OrderLines)
        {
            var inInLine3 = new Ininline3
            {
                ActualSellPrice = orderLine.UnitNetPrice,
                ItemDescription1 = orderLine.Description,
                ItemNumber = orderLine.Product.ErpNumber,
                LineItemType = "I",
                ListPrice = orderLine.UnitListPrice.ToString(),
                OrderQty = orderLine.QtyOrdered,
                UnitOfMeasure = orderLine.UnitOfMeasure,
                WarehouseID = GetWarehouseId(parameter.CustomerOrder, orderLine)
            };

            inInLine3s.Add(inInLine3);

            inInLine3s.AddRange(GetNotes(orderLine.Notes, "&"));
        }

        for (var i = 0; i < inInLine3s.Count; i++)
        {
            inInLine3s[i].SequenceNumber = i + 1;
        }

        return inInLine3s;
    }

    private static List<Ininline3> GetNotes(string notes, string itemNumber)
    {
        var inInLine3s = new List<Ininline3>();
        var splitNotes = SplitNotes(notes);

        foreach (var splitNote in splitNotes)
        {
            inInLine3s.Add(
                new Ininline3
                {
                    ItemNumber = itemNumber,
                    LineItemType = "cp",
                    ItemDescription1 = splitNote
                }
            );
        }

        return inInLine3s;
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
