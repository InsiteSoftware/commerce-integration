namespace Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;

using Newtonsoft.Json;

public class InventoryAllocationInquiry
{
    public string Id { get; set; }

    public int RowNumber { get; set; }

    public string Note { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal Available { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal AvailableForIssue { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal AvailableForShipping { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public string BaseUnit { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal InTransit { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal InTransitToSO { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public string InventoryID { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal InventoryIssues { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal InventoryReceipts { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal KitAssemblyDemand { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal KitAssemblySupply { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public string Location { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal OnHand { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal OnLocationNotAvailable { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal PurchaseForSO { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal PurchaseForSOPrepared { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal PurchaseOrders { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal PurchasePrepared { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal PurchaseReceipts { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal ReceiptsForSO { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal SOAllocated { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal SOBackOrdered { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal SOBooked { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal SOPrepared { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal SOShipped { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal SOToPurchase { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal TotalAddition { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public decimal TotalDeduction { get; set; }

    [JsonConverter(typeof(ValuePropertyJsonConverter))]
    public string WarehouseID { get; set; }
}
