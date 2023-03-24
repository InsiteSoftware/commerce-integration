namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.GetOrderNotes;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class GetOrderNotes : IPipe<GetOrderNotesParameter, GetOrderNotesResult>
{
    public int Order => 100;

    public GetOrderNotesResult Execute(
        IUnitOfWork unitOfWork,
        GetOrderNotesParameter parameter,
        GetOrderNotesResult result
    )
    {
        result.LineItemInfos = this.GetOrderNotesLineItemInfos(parameter);

        return result;
    }

    private List<RequestLineItemInfo> GetOrderNotesLineItemInfos(GetOrderNotesParameter parameter)
    {
        // notes go in ItemDesc1 and ItemDesc2 in as many LineItemInfos as needed
        var lineItemInfos = new List<RequestLineItemInfo>();
        var notes = this.SplitOrderNotes(parameter);

        for (var i = 0; i < notes.Count; i += 2)
        {
            var lineItemInfo = new RequestLineItemInfo
            {
                ItemNumber = string.Empty,
                LineItemType = parameter.LineItemType,
                ItemDesc1 = notes[i]
            };

            if (notes.Count > i + 1)
            {
                lineItemInfo.ItemDesc2 = notes[i + 1];
            }

            lineItemInfos.Add(lineItemInfo);
        }

        return lineItemInfos;
    }

    private List<string> SplitOrderNotes(GetOrderNotesParameter parameter)
    {
        // notes are 31 character max and split on white space only
        var notes = new List<string>();
        var words = parameter.Notes.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (notes.Any() && notes[notes.Count - 1].Length + word.Length + 1 <= 31)
            {
                notes[notes.Count - 1] = $"{notes[notes.Count - 1]} {word}";
            }
            else
            {
                notes.Add(word);
            }
        }

        return notes;
    }
}
