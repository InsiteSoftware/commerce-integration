﻿namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;

using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.WebService.Interfaces;

public class SubmitOrderLinesParameter : PipeParameterBase
{
    public string ErpOrderNumber { get; set; }

    public CustomerOrder CustomerOrder { get; set; }

    public IntegrationConnection IntegrationConnection { get; set; }

    public IJobLogger JobLogger { get; set; }
}
