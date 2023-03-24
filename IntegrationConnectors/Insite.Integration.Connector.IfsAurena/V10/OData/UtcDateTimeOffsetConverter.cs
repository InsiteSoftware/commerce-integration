namespace Insite.Integration.Connector.IfsAurena.V10.OData;

using System.Globalization;
using Newtonsoft.Json.Converters;

internal class UtcDateTimeOffsetConverter : IsoDateTimeConverter
{
    public UtcDateTimeOffsetConverter()
    {
        this.DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        this.DateTimeStyles = DateTimeStyles.AdjustToUniversal;
    }
}
