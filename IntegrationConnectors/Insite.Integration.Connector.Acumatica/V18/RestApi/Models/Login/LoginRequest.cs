namespace Insite.Integration.Connector.Acumatica.V18.RestApi.Models.Login;

public class LoginRequest
{
    public string Name { get; set; }

    public string Password { get; set; }

    public string Company { get; set; }

    public string Branch { get; set; }
}
