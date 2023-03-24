﻿namespace Insite.Integration.Connector.SXe.V11.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.ARCustomerMnt;

public class ARCustomerMntResult : PipeResultBase
{
    public ARCustomerMntRequest ARCustomerMntRequest { get; set; }

    public ARCustomerMntResponse ARCustomerMntResponse { get; set; }

    public string ErpNumber { get; set; }

    public string ErpSequence { get; set; }
}
