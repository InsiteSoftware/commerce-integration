namespace Insite.Integration.Connector.SXe.V11;

using Insite.Core.Interfaces.Dependency;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.ARCustomerMnt;

internal interface ISXeApiServiceV11 : IMultiInstanceDependency
{
    /// <summary>
    /// Executes the SFCustomerSummary with the provided <see cref="SFCustomerSummaryRequest"/>
    /// </summary>
    /// <param name="sfCustomerSummaryRequest">The <see cref="SFCustomerSummaryRequest"/>.</param>
    /// <returns>The <see cref="SFCustomerSummaryResponse"/>.</returns>
    SFCustomerSummaryResponse SFCustomerSummary(SFCustomerSummaryRequest sfCustomerSummaryRequest);

    /// <summary>
    /// Executes the OEPricingMultipleV4 with the provided <see cref="OEPricingMultipleV4Request"/>
    /// </summary>
    /// <param name="oePricingMultipleV4Request">The <see cref="OEPricingMultipleV4Request"/>.</param>
    /// <returns>The <see cref="OEPricingMultipleV4Response"/>.</returns>
    OEPricingMultipleV4Response OEPricingMultipleV4(
        OEPricingMultipleV4Request oePricingMultipleV4Request
    );

    /// <summary>
    /// Executes the SFOEOrderTotLoadV4 with the provided <see cref="SFOEOrderTotLoadV4Request"/>
    /// </summary>
    /// <param name="sfoeOrderTotLoadV4Request">The <see cref="SFOEOrderTotLoadV4Request"/>.</param>
    /// <returns>The <see cref="SFOEOrderTotLoadV4Response"/>.</returns>
    SFOEOrderTotLoadV4Response SFOEOrderTotLoadV4(
        SFOEOrderTotLoadV4Request sfoeOrderTotLoadV4Request
    );

    /// <summary>
    /// Executes the ARCustomerMnt with the provided <see cref="ARCustomerMntRequest"/>
    /// </summary>
    /// <param name="arCustomerMntRequest">The <see cref="ARCustomerMntRequest"/>.</param>
    /// <returns>The <see cref="ARCustomerMntResponse"/>.</returns>
    ARCustomerMntResponse ARCustomerMnt(ARCustomerMntRequest arCustomerMntRequest);
}
