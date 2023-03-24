namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetInventoryStock;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetInventoryStock;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class DeserializeResponseTests
    : BaseForPipeTests<GetInventoryStockParameter, GetInventoryStockResult>
{
    public override Type PipeType => typeof(DeserializeResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_300()
    {
        this.pipe.Order.Should().Be(300);
    }

    [Test]
    public void Execute_Should_Deserialize_Response()
    {
        var inventoryPartInStocks = new List<InventoryPartInStock>
        {
            new InventoryPartInStock { Contract = "200", PartNo = "1" },
            new InventoryPartInStock { Contract = "210", PartNo = "2" }
        };

        var mockODataResult =
            "{ \"value\" : " + IfsAurenaSerializationService.Serialize(inventoryPartInStocks) + "}";

        var result = new GetInventoryStockResult
        {
            SerializedInventoryPartInStockResponse = mockODataResult
        };

        result = this.RunExecute(result);

        result.InventoryPartInStockResponses.Should().BeEquivalentTo(inventoryPartInStocks);
    }
}
