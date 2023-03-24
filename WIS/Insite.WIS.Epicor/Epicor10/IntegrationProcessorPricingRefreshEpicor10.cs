namespace Insite.WIS.Epicor.Epicor10;

using Insite.WIS.Epicor.Epicor9;

/// <summary>
/// Epicor 10 (Sql Server and Progress) Implementation of Pricing Refresh. Ultimately queries the PriceLst/PriceLstParts/CustomerPriceLst/CustomerGroupPriceLst tables
/// on the database and builds a dataset in the exact format of the ISC price matrix table.
/// </summary>
public class IntegrationProcessorPricingRefreshEpicor10 : IntegrationProcessorPricingRefreshEpicor9
{
    public IntegrationProcessorPricingRefreshEpicor10()
    {
        this.ErpSqlSchemaName = "Erp.";
    }
}
