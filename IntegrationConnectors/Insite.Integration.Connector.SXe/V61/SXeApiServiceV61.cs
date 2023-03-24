namespace Insite.Integration.Connector.SXe.V61;

using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Insite.Common.Logging;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.ARCustomerMnt;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

internal sealed class SXeApiServiceV61 : ISXeApiServiceV61
{
    private const string SxApiServiceApiPath = "SxApiService.asmx";

    private readonly string url;

    private readonly int companyNumber;

    private readonly string connectionString;

    private readonly string operatorInitials;

    private readonly string operatorPassword;

    /// <summary>
    /// Initializes a new instance of the <see cref="SXeApiServiceV61"/> class.
    /// </summary>
    /// <param name="url">The url of the SXe web service.</param>
    /// <param name="connectionString">The connection string of the app server used call the SXe web service.</param>
    /// <param name="operatorInitials">The operator initials used call the SXe web service.</param>
    /// <param name="operatorPassword">The operator password used call the SXe web service.</param>
    /// <param name="companyNumber">The company number used call the SXe web service.</param>
    public SXeApiServiceV61(
        string url,
        string connectionString,
        string operatorInitials,
        string operatorPassword,
        int companyNumber
    )
    {
        this.url = url;
        this.connectionString = connectionString;
        this.operatorInitials = operatorInitials;
        this.operatorPassword = operatorPassword;
        this.companyNumber = companyNumber;
    }

    /// <summary>
    /// Executes the ARCustomerMnt with the provided <see cref="ARCustomerMntRequest"/>
    /// </summary>
    /// <param name="arCustomerMntRequest">The <see cref="ARCustomerMntRequest"/>.</param>
    /// <returns>The <see cref="ARCustomerMntResponse"/>.</returns>
    public ARCustomerMntResponse ARCustomerMnt(ARCustomerMntRequest arCustomerMntRequest)
    {
        LogHelper
            .For(this)
            .Debug($"{nameof(ARCustomerMntRequest)}: {GetSerializedValue(arCustomerMntRequest)}");

        ARCustomerMntResponse arCustomerMntResponse = null;

        try
        {
            var sXeSoapService = CreateSXeSoapService(this.url, SxApiServiceApiPath);
            var userCode = CreateUserCode(this.companyNumber, this.operatorInitials);

            arCustomerMntResponse = sXeSoapService.ARCustomerMnt(
                this.connectionString,
                userCode,
                this.operatorPassword,
                arCustomerMntRequest
            );
        }
        catch (Exception exception)
        {
            arCustomerMntResponse = new ARCustomerMntResponse { errorMessage = exception.Message };
        }

        LogHelper
            .For(this)
            .Debug($"{nameof(ARCustomerMntResponse)}: {GetSerializedValue(arCustomerMntResponse)}");

        return arCustomerMntResponse;
    }

    /// <summary>
    /// Executes the OEPricingMultipleV3 with the provided <see cref="OEPricingMultipleV3Request"/>
    /// </summary>
    /// <param name="oePricingMultipleV3Request">The <see cref="OEPricingMultipleV3Request"/>.</param>
    /// <returns>The <see cref="OEPricingMultipleV3Response"/>.</returns>
    public OEPricingMultipleV3Response OEPricingMultipleV3(
        OEPricingMultipleV3Request oePricingMultipleV3Request
    )
    {
        LogHelper
            .For(this)
            .Debug(
                $"{nameof(OEPricingMultipleV3Request)}: {GetSerializedValue(oePricingMultipleV3Request)}"
            );

        OEPricingMultipleV3Response oePricingMultipleV3Response = null;

        try
        {
            var sXeSoapService = CreateSXeSoapService(this.url, SxApiServiceApiPath);
            var userCode = CreateUserCode(this.companyNumber, this.operatorInitials);

            oePricingMultipleV3Response = sXeSoapService.OEPricingMultipleV3(
                this.connectionString,
                userCode,
                this.operatorPassword,
                oePricingMultipleV3Request
            );
        }
        catch (Exception exception)
        {
            oePricingMultipleV3Response = new OEPricingMultipleV3Response
            {
                errorMessage = exception.Message
            };
        }

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(OEPricingMultipleV3Response)}: {GetSerializedValue(oePricingMultipleV3Response)}"
            );

        return oePricingMultipleV3Response;
    }

    /// <summary>
    /// Executes the SFCustomerSummary with the provided <see cref="SFCustomerSummaryRequest"/>
    /// </summary>
    /// <param name="sfCustomerSummaryRequest">The <see cref="SFCustomerSummaryRequest"/>.</param>
    /// <returns>The <see cref="SFCustomerSummaryResponse"/>.</returns>
    public SFCustomerSummaryResponse SFCustomerSummary(
        SFCustomerSummaryRequest sfCustomerSummaryRequest
    )
    {
        LogHelper
            .For(this)
            .Debug(
                $"{nameof(SFCustomerSummaryRequest)}: {GetSerializedValue(sfCustomerSummaryRequest)}"
            );

        SFCustomerSummaryResponse sfCustomerSummaryResponse = null;

        try
        {
            var sXeSoapService = CreateSXeSoapService(this.url, SxApiServiceApiPath);
            var userCode = CreateUserCode(this.companyNumber, this.operatorInitials);

            sfCustomerSummaryResponse = sXeSoapService.SFCustomerSummary(
                this.connectionString,
                userCode,
                this.operatorPassword,
                sfCustomerSummaryRequest
            );
        }
        catch (Exception exception)
        {
            sfCustomerSummaryResponse = new SFCustomerSummaryResponse
            {
                errorMessage = exception.Message
            };
        }

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(SFCustomerSummaryResponse)}: {GetSerializedValue(sfCustomerSummaryResponse)}"
            );

        return sfCustomerSummaryResponse;
    }

    /// <summary>
    /// Executes the SFOEOrderTotLoadV2 with the provided <see cref="SFOEOrderTotLoadV2Request"/>
    /// </summary>
    /// <param name="sfoeOrderTotLoadV2Request">The <see cref="SFOEOrderTotLoadV2Request"/>.</param>
    /// <returns>The <see cref="SFOEOrderTotLoadV2Response"/>.</returns>
    public SFOEOrderTotLoadV2Response SFOEOrderTotLoadV2(
        SFOEOrderTotLoadV2Request sfoeOrderTotLoadV2Request
    )
    {
        LogHelper
            .For(this)
            .Debug(
                $"{nameof(SFOEOrderTotLoadV2Request)}: {GetSerializedValue(sfoeOrderTotLoadV2Request)}"
            );

        SFOEOrderTotLoadV2Response sfOEOrderTotLoadV2Response = null;

        try
        {
            var sXeSoapService = CreateSXeSoapService(this.url, SxApiServiceApiPath);
            var userCode = CreateUserCode(this.companyNumber, this.operatorInitials);

            sfOEOrderTotLoadV2Response = sXeSoapService.SFOEOrderTotLoadV2(
                this.connectionString,
                userCode,
                this.operatorPassword,
                sfoeOrderTotLoadV2Request
            );
        }
        catch (Exception exception)
        {
            sfOEOrderTotLoadV2Response = new SFOEOrderTotLoadV2Response
            {
                errorMessage = exception.Message
            };
        }

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(SFOEOrderTotLoadV2Response)}: {GetSerializedValue(sfOEOrderTotLoadV2Response)}"
            );

        return sfOEOrderTotLoadV2Response;
    }

    private static SXeSoapServiceV61 CreateSXeSoapService(string baseUrl, params string[] apiPaths)
    {
        return new SXeSoapServiceV61(CreateUrl(baseUrl, apiPaths));
    }

    private static string CreateUrl(string baseUrl, params string[] apiPaths)
    {
        var url = string.Empty;

        baseUrl = baseUrl.TrimEnd('/');

        foreach (var apiPath in apiPaths.Reverse())
        {
            if (baseUrl.EndsWith(apiPath, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            url = $"{apiPath}/{url}";
        }

        url = $"{baseUrl}/{url}";

        return url.TrimEnd('/');
    }

    private static string CreateUserCode(int companyNumber, string operatorInitials)
    {
        return $"cono={companyNumber}|oper={operatorInitials}";
    }

    private static string GetSerializedValue(object value)
    {
        var serializer = new XmlSerializer(value.GetType());
        var writer = new StringWriter();

        serializer.Serialize(writer, value);

        return writer.ToString();
    }
}
