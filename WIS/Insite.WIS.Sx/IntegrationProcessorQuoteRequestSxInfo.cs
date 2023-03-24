namespace Insite.WIS.Sx;

/// <summary>
/// Class to hold general information related to the Quote Request Submit. By default these values are populated in the PopulateGetQuoteParameters method within
/// IntegrationProcessorQuoteSubmitSx. They are passed in via integration job parameters.
/// </summary>
public class IntegrationProcessorQuoteRequestSxInfo
{
    /// <summary>The cono in context.</summary>
    public string CompanyNumber { get; set; }

    /// <summary>The default warehouse to use if its not defined in the ISC customer order.</summary>
    public string CustomerDefaultWarehouse { get; set; }
}
