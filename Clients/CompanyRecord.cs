namespace SkinApi.Gui.Clients;

public class CompanyRecord
{
    public int? CompanyId { get; set; }

    public string Company { get; set; }

    public DateTime? CompanyDateEntry { get; set; }

    public bool? CompanyActive { get; set; }

    public DateTime? CompanyDateChange { get; set; }

    public string CompanyStreetname1 { get; set; }
    
    public string CompanyStreetname2 { get; set; }

    public string CompanyPostalcode { get; set; }

    public string CompanyCity { get; set; }

    public string CompanyCountry { get; set; }

    public string GeneralEmailaddress { get; set; }

    public string GeneralPhonenumber { get; set; }

    public string CompanyWebsite { get; set; }

    public int? CompanyHubspotId { get; set; }
}