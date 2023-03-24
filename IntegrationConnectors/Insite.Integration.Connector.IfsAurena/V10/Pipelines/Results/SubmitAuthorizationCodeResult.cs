namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public class SubmitAuthorizationCodeResult : PipeResultBase
{
    public AuthorizationCode AuthorizationCodeRequest { get; set; }

    public string SerializedAuthorizationCodeRequest { get; set; }

    public string ErpOrderNumber { get; set; }
}
