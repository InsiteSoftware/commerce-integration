namespace Insite.Integration.Connector.SXe.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10;
using Insite.Integration.Connector.SXe.V11;
using Insite.Integration.Connector.SXe.V61;

internal interface ISXeApiServiceFactory : ISingletonLifetime
{
    ISXeApiServiceV61 GetSXeApiServiceV61(IntegrationConnection integrationConnection = null);

    ISXeApiServiceV10 GetSXeApiServiceV10(IntegrationConnection integrationConnection = null);

    ISXeApiServiceV11 GetSXeApiServiceV11(IntegrationConnection integrationConnection = null);
}
