namespace Insite.Integration.Connector.Facts.Tests.Services;

using System.Collections.Generic;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using NUnit.Framework;

[TestFixture]
public class FactsSerializationServiceTests
{
    private const string PriceAvailabilityResponseWithNamespace =
        @"<?xml version='1.0' encoding='UTF-8'?>
<ResponseBatch
               xmlns='http://www.infor.com/FactsResponseBatch'
               ConsumerKey='[eStoreFr]' Language='en' DateTime='2020071013' SerialID=''>
    <Response
           xmlns:rp='http://www.infor.com/FactsPriceAvailability'
           RequestID='PriceAvailability' Company='01' SerialID=''>
        <rp:Items>
            <rp:Item>
                <rp:ItemNumber>I100</rp:ItemNumber>
                <rp:QuantityAvailable>105.2</rp:QuantityAvailable>
                <rp:NonStockFlag>false</rp:NonStockFlag>
                <rp:OrderQuantity>500.0</rp:OrderQuantity>
                <rp:UnitOfMeasure>CT</rp:UnitOfMeasure>
                <rp:WarehouseID>01</rp:WarehouseID>
                <rp:Price>2280</rp:Price>
                <rp:PricingUnitOfMeasure>EA</rp:PricingUnitOfMeasure>
                <rp:PricingUnitPrice>228</rp:PricingUnitPrice>
                <rp:PricingUnitStdPrice>240.00</rp:PricingUnitStdPrice>
                <rp:PricingUnitL1Price>228.00</rp:PricingUnitL1Price>
                <rp:PricingUnitL2Price>216.00</rp:PricingUnitL2Price>
                <rp:PricingUnitL3Price>204.00</rp:PricingUnitL3Price>
                <rp:PricingUnitL4Price>192.00</rp:PricingUnitL4Price>
                <rp:PricingUnitL5Price>180.00</rp:PricingUnitL5Price>
                <rp:PricingUnitL6Price>168.00</rp:PricingUnitL6Price>
                <rp:ExtendedPrice>1140000</rp:ExtendedPrice>
                <rp:NextPODate>2019-06-15</rp:NextPODate>
                <rp:NextPOQuantity>20</rp:NextPOQuantity>
            </rp:Item>
        </rp:Items>
    </Response>
</ResponseBatch>";

    private const string PriceAvailabilityResponseWithoutNamespace =
        @"<?xml version='1.0' encoding='UTF-8'?>
<ResponseBatch ConsumerKey='ABC123' Language='en' DateTime='2020070716' SerialID=''>
   <Response RequestID='PriceAvailability' Company='01' SerialID=''>
      <Items>
         <Item>
            <ItemNumber>3D25</ItemNumber>
            <QuantityAvailable>1</QuantityAvailable>
            <NonStockFlag>false</NonStockFlag>
            <OrderQuantity>0</OrderQuantity>
            <UnitOfMeasure>GA</UnitOfMeasure>
            <Price>12</Price>
            <PricingUnitOfMeasure>GA</PricingUnitOfMeasure>
            <PricingUnitPrice>0</PricingUnitPrice>
            <ExtendedPrice>0</ExtendedPrice>
            <WarehouseID>0</WarehouseID>
         </Item>
      </Items>
   </Response>
</ResponseBatch>";

    [Test]
    public void Serialize_Should_Serialize_PriceAvailabilityResponse()
    {
        var priceAvailabilityResponse = new PriceAvailabilityResponse
        {
            ConsumerKey = "ABC123",
            Language = "en",
            DateTime = "2020070716",
            Response = new Response
            {
                Company = "01",
                Items = new List<ResponseItem>
                {
                    new ResponseItem
                    {
                        ItemNumber = "3D25",
                        UnitOfMeasure = "GA",
                        PricingUnitOfMeasure = "GA",
                        QuantityAvailable = 1,
                        Price = 12
                    }
                }
            }
        };

        Assert.DoesNotThrow(() => FactsSerializationService.Serialize(priceAvailabilityResponse));
    }

    [Test]
    public void Deserialize_Should_Deserialize_PriceAvailabilityResponseWithNamespace()
    {
        var result = FactsSerializationService.Deserialize<PriceAvailabilityResponse>(
            PriceAvailabilityResponseWithNamespace
        );

        CollectionAssert.IsNotEmpty(result.Response.Items);
    }

    [Test]
    public void Deserialize_Should_Deserialize_PriceAvailabilityResponseWithoutNamespace()
    {
        var result = FactsSerializationService.Deserialize<PriceAvailabilityResponse>(
            PriceAvailabilityResponseWithoutNamespace
        );

        CollectionAssert.IsNotEmpty(result.Response.Items);
    }
}
