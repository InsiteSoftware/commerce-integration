namespace Insite.WIS.Epicor.Epicor9;

/// <summary>
/// Class to hold general information related to the Pricing Refresh. By default these values are populated in the PopulatePricingRefreshParameters method within
/// IntegrationProcessorPricingRefreshEpicor9. They are passed in via integration job parameters.
/// </summary>
public class IntegrationProcessorPricingRefreshEpicor9Info
{
    /// <summary>The cono in context.</summary>
    public string CompanyNumber { get; set; }

    /// <summary>The default warehouse to use.</summary>
    public string CustomerDefaultWarehouse { get; set; }

    /// <summary>Will be either Sql or Progress so i know which type of call (sql/odbc) to make.</summary>
    public string DbType { get; set; }
}
