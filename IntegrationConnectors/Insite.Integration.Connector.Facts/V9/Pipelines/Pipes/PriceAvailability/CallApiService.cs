namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;

using System;
using System.Collections.Generic;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;

public sealed class CallApiService : IPipe<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private const int RequestItemPageSize = 20;

    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 700;

    public PriceAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        PriceAvailabilityParameter parameter,
        PriceAvailabilityResult result
    )
    {
        var factsApiService = this.dependencyLocator
            .GetInstance<IFactsApiServiceFactory>()
            .GetFactsApiService(parameter.IntegrationConnection);

        try
        {
            foreach (
                var priceAvailabilityRequestPage in GetPriceAvailabilityRequestPages(
                    result.PriceAvailabilityRequest
                )
            )
            {
                var priceAvailabilityResponse = factsApiService.PriceAvailability(
                    priceAvailabilityRequestPage
                );
                if (result.PriceAvailabilityResponse == null)
                {
                    result.PriceAvailabilityResponse = priceAvailabilityResponse;
                }
                else
                {
                    result.PriceAvailabilityResponse.Response.Items.AddRange(
                        priceAvailabilityResponse.Response.Items
                    );
                }
            }
        }
        catch (Exception exception)
        {
            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.GeneralFailure,
                exception.Message
            );
        }

        return result;
    }

    private static List<PriceAvailabilityRequest> GetPriceAvailabilityRequestPages(
        PriceAvailabilityRequest priceAvailabilityRequest
    )
    {
        var priceAvailabilityRequestPages = new List<PriceAvailabilityRequest>();

        foreach (var requestItemPage in GetRequestItemPages(priceAvailabilityRequest.Request.Items))
        {
            var priceAvailabilityRequestPage = FactsCloneService.Clone(priceAvailabilityRequest);
            priceAvailabilityRequestPage.Request.Items = requestItemPage;

            priceAvailabilityRequestPages.Add(priceAvailabilityRequestPage);
        }

        return priceAvailabilityRequestPages;
    }

    private static List<List<RequestItem>> GetRequestItemPages(List<RequestItem> requestItems)
    {
        var requestItemPages = new List<List<RequestItem>>();

        for (var i = 0; i < requestItems.Count; i += RequestItemPageSize)
        {
            requestItemPages.Add(
                requestItems.GetRange(i, Math.Min(RequestItemPageSize, requestItems.Count - i))
            );
        }

        return requestItemPages;
    }
}
