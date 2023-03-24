namespace Insite.WIS.Sx.Sx55;

using Insite.Common.Helpers;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.WebIntegrationService;

/// <summary>SX 5.5 API Implementation of Order Submit</summary>
public class IntegrationProcessorOrderSubmitSx55
    : IntegrationProcessorOrderSubmitSx,
        IIntegrationProcessor
{
    private SxApiService SxApiService { get; set; }

    /// <summary>Retrieve a singleton instance of the SxApiService.</summary>
    /// <param name="url">The Url of the web service.</param>
    /// <returns>A singleton instance of the SxApiService.</returns>
    protected virtual SxApiService GetSxApiService(string url)
    {
        return this.SxApiService ?? (this.SxApiService = new SxApiService { Url = url });
    }

    protected override TU StandardApiCallWithLogging<T, TU>(
        SiteConnection siteConnection,
        IntegrationJob integrationJob,
        T request,
        string methodName,
        bool throwExceptionOnErrorMessage = true
    )
    {
        SxApiHelper.SerializeAndLog(
            siteConnection,
            integrationJob,
            request,
            methodName + " - Request"
        );

        var sxApiService = this.GetSxApiService(
            integrationJob.JobDefinition.IntegrationConnection.Url
        );
        var type = sxApiService.GetType();
        var methodInfo = type.GetMethod(methodName);
#pragma warning disable CS0618 // Type or member is obsolete
        var response = methodInfo.Invoke(
            sxApiService,
            new object[]
            {
                integrationJob.JobDefinition.IntegrationConnection.DataSource,
                integrationJob.JobDefinition.IntegrationConnection.LogOn,
                EncryptionHelper.DecryptAes(
                    integrationJob.JobDefinition.IntegrationConnection.Password
                ),
                request
            }
        );
#pragma warning restore

        SxApiHelper.ProcessResponse((TU)response, methodName, throwExceptionOnErrorMessage);
        SxApiHelper.SerializeAndLog(
            siteConnection,
            integrationJob,
            response,
            methodName + " - Response"
        );

        return (TU)response;
    }
}
