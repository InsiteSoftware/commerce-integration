namespace Insite.WIS.DistributorDataSolutions.Services;

using System;
using Insite.WIS.Broker.Parsers;
using Insite.WIS.Broker.Services;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.DistributorDataSolutions.Parsers;

public class FlatFileServiceDDS : FlatFileService
{
    public FlatFileServiceDDS(IntegrationJob integrationJob, JobDefinitionStep jobDefinitionStep)
        : base(integrationJob, jobDefinitionStep) { }

    protected override FileParserBase GetFileParser()
    {
        return
            this.JobDefinitionStep.FromClause.IndexOf(".json", StringComparison.OrdinalIgnoreCase)
            >= 0
            ? new FileParserDDS(this.IntegrationJob, this.JobDefinitionStep)
            : base.GetFileParser();
    }
}
