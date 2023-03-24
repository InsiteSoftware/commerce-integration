namespace Insite.Integration.Connector.SXe.V61;

using Insite.Integration.Connector.SXe.V61.SXApi.Models.ARCustomerMnt;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

internal interface ISXeApiServiceV61
{
    ARCustomerMntResponse ARCustomerMnt(ARCustomerMntRequest arCustomerMntRequest);

    OEPricingMultipleV3Response OEPricingMultipleV3(
        OEPricingMultipleV3Request oePricingMultipleV3Request
    );

    SFCustomerSummaryResponse SFCustomerSummary(SFCustomerSummaryRequest sfCustomerSummaryRequest);

    SFOEOrderTotLoadV2Response SFOEOrderTotLoadV2(
        SFOEOrderTotLoadV2Request sfoeOrderTotLoadV2Request
    );
}
