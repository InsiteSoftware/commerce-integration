namespace Insite.WIS.InRiver.Models;

using System.Collections.Generic;

/// <summary>The in river channel (website).</summary>
public class InRiverGenericObject
{
    /// <summary>Gets or sets the entity id.</summary>
    public string UniqueIdentifier { get; set; }

    public string Action { get; set; }

    public string EntityType { get; set; }

    public IDictionary<string, IEnumerable<TranslationDictionary>> Fields { get; set; }

    /// <summary>Gets or sets children objects.  </summary>
    public IList<InRiverGenericObject> Children { get; set; }

    /// <summary>Gets or sets the parent object  </summary>
    public IList<InRiverGenericObject> Parents { get; set; }
}

public class TranslationDictionary
{
    public string LanguageCode { get; set; }

    public string TranslatedValue { get; set; }
}
