namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class CreateInitialRequest
    : IPipe<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
{
    public int Order => 100;

    public GetMyAccountOpenARResult Execute(
        IUnitOfWork unitOfWork,
        GetMyAccountOpenARParameter parameter,
        GetMyAccountOpenARResult result
    )
    {
        result.GetMyAccountOpenARRequest = new GetMyAccountOpenAR
        {
            Request = new Request
            {
                B2BSellerVersion = new RequestB2BSellerVersion
                {
                    MajorVersion = "5",
                    MinorVersion = "11",
                    BuildNumber = "100"
                }
            }
        };

        return result;
    }
}
