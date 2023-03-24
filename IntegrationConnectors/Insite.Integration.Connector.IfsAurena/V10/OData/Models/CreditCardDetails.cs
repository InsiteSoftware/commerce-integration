namespace Insite.Integration.Connector.IfsAurena.V10.OData.Models;

using System;
using Newtonsoft.Json;

public class CreditCardDetails
{
    public string OrderNo { get; set; }

    public string DisplayCardNumber { get; set; }

    public bool SingleOccurrence { get; set; }

    public string CreditExpMonth { get; set; }

    public int CreditExpYear { get; set; }

    public string CustomerNo { get; set; }

    public string CardType { get; set; }

    public string Company { get; set; }

    public string Currency { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}
