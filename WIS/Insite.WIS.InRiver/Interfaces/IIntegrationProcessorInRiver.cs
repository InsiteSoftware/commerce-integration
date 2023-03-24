namespace Insite.WIS.InRiver.Interfaces;

using System.Collections.Generic;
using System.Data;
using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Models;
using Insite.WIS.Broker.Interfaces;

public interface IIntegrationProcessorInRiver : IIntegrationProcessor
{
    IEnumerable<InRiverGenericObject> ParseXmlFile(
        string processingFileName,
        IntegrationConnection integrationConnection
    );

    DataTable ProcessInRiverCollection(
        IEnumerable<InRiverGenericObject> inRiverCollection,
        DataTable dataTableSchema,
        IntegrationJob integrationJob,
        SiteConnection siteConnection,
        JobDefinitionStep integrationJobStep
    );
}
