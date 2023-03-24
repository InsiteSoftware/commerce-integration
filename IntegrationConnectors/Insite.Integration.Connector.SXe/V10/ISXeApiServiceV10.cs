namespace Insite.Integration.Connector.SXe.V10;

using Insite.Core.Interfaces.Dependency;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.ARCustomerMnt;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

internal interface ISXeApiServiceV10 : IMultiInstanceDependency
{
    ARCustomerMntResponse ARCustomerMnt(ARCustomerMntRequest arCustomerMntRequest);

    OEPricingMultipleV4Response OEPricingMultipleV4(
        OEPricingMultipleV4Request oePricingMultipleV4Request
    );

    SFCustomerSummaryResponse SFCustomerSummary(SFCustomerSummaryRequest sfCustomerSummaryRequest);

    SFOEOrderTotLoadV4Response SFOEOrderTotLoadV4(
        SFOEOrderTotLoadV4Request sfoeOrderTotLoadV4Request
    );
}
