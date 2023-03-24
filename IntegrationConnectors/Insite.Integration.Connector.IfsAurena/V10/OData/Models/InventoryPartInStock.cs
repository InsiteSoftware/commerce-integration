namespace Insite.Integration.Connector.IfsAurena.V10.OData.Models;

using System;

public class InventoryPartInStock
{
    public string Contract { get; set; }

    public string PartNo { get; set; }

    public string ConfigurationId { get; set; }

    public string LocationNo { get; set; }

    public string LotBatchNo { get; set; }

    public string SerialNo { get; set; }

    public string EngChgLevel { get; set; }

    public string WaivDevRejNo { get; set; }

    public decimal? ActivitySeq { get; set; }

    public decimal? HandlingUnitId { get; set; }

    public decimal? AvgUnitTransitCost { get; set; }

    public decimal? CountVariance { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public DateTimeOffset? LastActivityDate { get; set; }

    public DateTime? LastCountDate { get; set; }

    public decimal? QtyInTransit { get; set; }

    public decimal? QtyOnhand { get; set; }

    public decimal? QtyReserved { get; set; }

    public DateTimeOffset? ReceiptDate { get; set; }

    public string Source { get; set; }

    public string Warehouse { get; set; }

    public string BayNo { get; set; }

    public string RowNo { get; set; }

    public string TierNo { get; set; }

    public string BinNo { get; set; }

    public string AvailabilityControlId { get; set; }

    public DateTime? CreateDate { get; set; }

    public string RotablePartPoolId { get; set; }

    public string ProjectId { get; set; }

    public decimal? CatchQtyInTransit { get; set; }

    public decimal? CatchQtyOnhand { get; set; }

    public string OwningCustomerNo { get; set; }

    public string OwningVendorNo { get; set; }

    public decimal? StructureLevel { get; set; }

    public string HandlingUnitTypeId { get; set; }

    public decimal? AvailableQty { get; set; }

    public string ConditionCode { get; set; }

    public decimal? TopParentHandlingUnitId { get; set; }

    public string TopParentHandlingUnitTypeId { get; set; }

    public string TopParentSscc { get; set; }

    public string TopParentAltHandlingUnitLabelId { get; set; }

    public string LocationDescription { get; set; }

    public string UoM { get; set; }

    public string CatchUoM { get; set; }

    public decimal? UnifiedOnHandQty { get; set; }

    public decimal? UnifiedReservedQty { get; set; }

    public decimal? UnifiedQtyinTransit { get; set; }

    public string UnifiedUoM { get; set; }

    public decimal? UnifiedCatchOnHandQty { get; set; }

    public decimal? UnifiedCatchQtyinTransit { get; set; }

    public string UnifiedCatchUoM { get; set; }

    public bool? FreezeFlagDb { get; set; }

    public decimal? UnitCost { get; set; }

    public decimal? TotalInventoryValue { get; set; }

    public string BaseCurr { get; set; }

    public string Owner { get; set; }

    public string OwnerName { get; set; }

    public decimal? PartAcquisitionValue { get; set; }

    public decimal? TotalAcquisitionValue { get; set; }

    public string AcquisitionCurrency { get; set; }

    public string PartOwnershipDb { get; set; }

    public string LocationTypeDb { get; set; }

    public decimal? AvailableQtytoMove { get; set; }

    public string OperationalCondition { get; set; }

    public string PartHandlingUnitTypeId { get; set; }

    public string Company { get; set; }

    public string InvPartBarcodeExist { get; set; }

    public string ProgramId { get; set; }

    public string ProgramDescription { get; set; }

    public string ProjectName { get; set; }

    public string SubProjectId { get; set; }

    public string SubProjectDescription { get; set; }

    public string ActivityNo { get; set; }

    public string ActivityDescription { get; set; }

    public string Cf_Commodity_Group_1 { get; set; }

    public string Cf_Commodity_Group_1_Desc { get; set; }

    public string Cf_Commodity_Group_2 { get; set; }

    public string Cf_Commodity_Group_2_Desc { get; set; }
}
