namespace Insite.Integration.Connector.Base.Interfaces;

using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;

public interface IOAuthTokenProvider : IDependency
{
    string GetAccessToken(IntegrationConnection integrationConnection);
}
