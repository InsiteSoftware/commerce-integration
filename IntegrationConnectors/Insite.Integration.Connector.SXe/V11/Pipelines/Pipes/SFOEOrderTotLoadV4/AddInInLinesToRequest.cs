namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;

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

        result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection =
            new InputLineDataCollection { InputLineDatas = GetInInLines(parameter) };

        parameter.JobLogger?.Debug($"{nameof(AddInInLinesToRequest)} Finished.");

        return result;
    }

    private static List<InputLineData> GetInInLines(SFOEOrderTotLoadV4Parameter parameter)
    {
        var inInLine3s = new List<InputLineData>();

        inInLine3s.AddRange(GetNotes(parameter.CustomerOrder.Notes, "/"));

        foreach (var orderLine in parameter.CustomerOrder.OrderLines)
        {
            var inInLine3 = new InputLineData
            {
                Actualsellprice = orderLine.UnitNetPrice,
                Itemdesc1 = orderLine.Description,
                Itemnumber = orderLine.Product.ErpNumber,
                Lineitemtype = "I",
                Listprice = orderLine.UnitListPrice.ToString(),
                Orderqty = orderLine.QtyOrdered,
                Unitofmeasure = orderLine.UnitOfMeasure,
                Warehouseid = GetWarehouseId(parameter.CustomerOrder, orderLine)
            };

            inInLine3s.Add(inInLine3);

            inInLine3s.AddRange(GetNotes(orderLine.Notes, "&"));
        }

        for (var i = 0; i < inInLine3s.Count; i++)
        {
            inInLine3s[i].Seqno = i + 1;
        }

        return inInLine3s;
    }

    private static List<InputLineData> GetNotes(string notes, string itemNumber)
    {
        var inInLine3s = new List<InputLineData>();
        var splitNotes = SplitNotes(notes);

        foreach (var splitNote in splitNotes)
        {
            inInLine3s.Add(
                new InputLineData
                {
                    Itemnumber = itemNumber,
                    Lineitemtype = "cp",
                    Itemdesc1 = splitNote
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
