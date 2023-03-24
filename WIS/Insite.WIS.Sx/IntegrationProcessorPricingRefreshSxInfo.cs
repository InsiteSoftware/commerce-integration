namespace Insite.WIS.Sx;

/// <summary>
/// Class to hold general information related to the Pricing Refresh. By default these values are populated in the PopulatePricingRefreshParameters method within
/// IntegrationProcessorPricingRefreshSx. They are passed in via integration job parameters.
/// </summary>
public class IntegrationProcessorPricingRefreshSxInfo
{
    /// <summary>The cono in context.</summary>
    public string CompanyNumber { get; set; }
}
