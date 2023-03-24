namespace Insite.Integration.Connector.SXe.V10;

using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Insite.Common.Logging;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.ARCustomerMnt;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.Shared;

internal sealed class SXeApiServiceV10 : ISXeApiServiceV10
{
    private const string SxApiPath = "sxapi";

    private const string ServiceARApiPath = "ServiceAR.svc";

    private const string ServiceOEApiPath = "ServiceOE.svc";

    private const string ServiceSFApiPath = "ServiceSF.svc";

    private readonly string url;

    private readonly string connectionString;

    private readonly string operatorInitials;

    private readonly string operatorPassword;

    private readonly int companyNumber;

    /// <summary>
    /// Initializes a new instance of the <see cref="SXeApiServiceV10"/> class.
    /// </summary>
    /// <param name="url">The url of the SXe web service.</param>
    /// <param name="connectionString">The connection string of the app server used to populate the <see cref="CallConnection"/>.</param>
    /// <param name="operatorInitials">The operator initials used to populate the <see cref="CallConnection"/>.</param>
    /// <param name="operatorPassword">The operator password used to populate the <see cref="CallConnection"/>.</param>
    /// <param name="companyNumber">The company number used to populate the <see cref="CallConnection"/>.</param>
    public SXeApiServiceV10(
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
            var sXeSoapService = CreateSXeSoapService(this.url, SxApiPath, ServiceARApiPath);
            var callConnection = CreateCallConnection(
                this.companyNumber,
                this.connectionString,
                this.operatorInitials,
                this.operatorPassword
            );

            arCustomerMntResponse = sXeSoapService.ARCustomerMnt(
                callConnection,
                arCustomerMntRequest
            );
        }
        catch (Exception exception)
        {
            arCustomerMntResponse = new ARCustomerMntResponse { ErrorMessage = exception.Message };
        }

        LogHelper
            .For(this)
            .Debug($"{nameof(ARCustomerMntResponse)}: {GetSerializedValue(arCustomerMntResponse)}");

        return arCustomerMntResponse;
    }

    /// <summary>
    /// Executes the OEPricingMultipleV4 with the provided <see cref="OEPricingMultipleV4Request"/>
    /// </summary>
    /// <param name="oePricingMultipleV4Request">The <see cref="OEPricingMultipleV4Request"/>.</param>
    /// <returns>The <see cref="OEPricingMultipleV4Response"/>.</returns>
    public OEPricingMultipleV4Response OEPricingMultipleV4(
        OEPricingMultipleV4Request oePricingMultipleV4Request
    )
    {
        LogHelper
            .For(this)
            .Debug(
                $"{nameof(OEPricingMultipleV4Request)}: {GetSerializedValue(oePricingMultipleV4Request)}"
            );

        OEPricingMultipleV4Response oePricingMultipleV4Response = null;

        try
        {
            var sXeSoapService = CreateSXeSoapService(this.url, SxApiPath, ServiceOEApiPath);
            var callConnection = CreateCallConnection(
                this.companyNumber,
                this.connectionString,
                this.operatorInitials,
                this.operatorPassword
            );

            oePricingMultipleV4Response = sXeSoapService.OEPricingMultipleV4(
                callConnection,
                oePricingMultipleV4Request
            );
        }
        catch (Exception exception)
        {
            oePricingMultipleV4Response = new OEPricingMultipleV4Response
            {
                ErrorMessage = exception.Message
            };
        }

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(OEPricingMultipleV4Response)}: {GetSerializedValue(oePricingMultipleV4Response)}"
            );

        return oePricingMultipleV4Response;
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
            var sXeSoapService = CreateSXeSoapService(this.url, SxApiPath, ServiceSFApiPath);
            var callConnection = CreateCallConnection(
                this.companyNumber,
                this.connectionString,
                this.operatorInitials,
                this.operatorPassword
            );

            sfCustomerSummaryResponse = sXeSoapService.SFCustomerSummary(
                callConnection,
                sfCustomerSummaryRequest
            );
        }
        catch (Exception exception)
        {
            sfCustomerSummaryResponse = new SFCustomerSummaryResponse
            {
                ErrorMessage = exception.Message
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
    /// Executes the SFOEOrderTotLoadV4 with the provided <see cref="SFOEOrderTotLoadV4Request"/>
    /// </summary>
    /// <param name="sfoeOrderTotLoadV4Request">The <see cref="SFOEOrderTotLoadV4Request"/>.</param>
    /// <returns>The <see cref="SFOEOrderTotLoadV4Response"/>.</returns>
    public SFOEOrderTotLoadV4Response SFOEOrderTotLoadV4(
        SFOEOrderTotLoadV4Request sfoeOrderTotLoadV4Request
    )
    {
        LogHelper
            .For(this)
            .Debug(
                $"{nameof(SFOEOrderTotLoadV4Request)}: {GetSerializedValue(sfoeOrderTotLoadV4Request)}"
            );

        SFOEOrderTotLoadV4Response sfOEOrderTotLoadV4Response = null;

        try
        {
            var sXeSoapService = CreateSXeSoapService(this.url, SxApiPath, ServiceSFApiPath);
            var callConnection = CreateCallConnection(
                this.companyNumber,
                this.connectionString,
                this.operatorInitials,
                this.operatorPassword
            );

            sfOEOrderTotLoadV4Response = sXeSoapService.SFOEOrderTotLoadV4(
                callConnection,
                sfoeOrderTotLoadV4Request
            );
        }
        catch (Exception exception)
        {
            sfOEOrderTotLoadV4Response = new SFOEOrderTotLoadV4Response
            {
                ErrorMessage = exception.Message
            };
        }

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(SFOEOrderTotLoadV4Response)}: {GetSerializedValue(sfOEOrderTotLoadV4Response)}"
            );

        return sfOEOrderTotLoadV4Response;
    }

    private static SXeSoapServiceV10 CreateSXeSoapService(string baseUrl, params string[] apiPaths)
    {
        return new SXeSoapServiceV10(CreateUrl(baseUrl, apiPaths));
    }

    private static CallConnection CreateCallConnection(
        int companyNumber,
        string connectionString,
        string operatorInitials,
        string operatorPassword
    )
    {
        return new CallConnection
        {
            CompanyNumber = companyNumber,
            ConnectionString = connectionString,
            OperatorInitials = operatorInitials,
            OperatorPassword = operatorPassword,
            StateFreeAppserver = false
        };
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

    private static string GetSerializedValue(object value)
    {
        var serializer = new XmlSerializer(value.GetType());
        var writer = new StringWriter();

        serializer.Serialize(writer, value);

        return writer.ToString();
    }
}
