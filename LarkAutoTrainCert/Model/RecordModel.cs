using Newtonsoft.Json;

namespace LarkAutoTrainCert.Model;

public class RecordModel
{
    public FieldsModel fields { get; set; }
    public string id { get; set; }
    public string record_id { get; set; }
    public string record_url { get; set; }
}

public class FieldsModel
{
    [JsonProperty("ProjectCode")]
    public string ProjectCode { get; set; }

    [JsonProperty("Date of Registration")]
    public string RegDate { get; set; }

    [JsonProperty("Source")]
    public string Source { get; set; }

    [JsonProperty("Project Name")]
    public string ProjectName { get; set; }

    [JsonProperty("Contact Mobile No.")]
    public string ContactNo { get; set; }

    [JsonProperty("Project Owner")]
    public string Owner { get; set; }

    //[JsonProperty("Site Location")]
    //public string Site { get; set; }

    [JsonProperty("Architect")]
    public string Architect { get; set; }

    [JsonProperty("Contractor")]
    public string Contractor { get; set; }

    [JsonProperty("Architect's Commission")]
    public string Commi { get; set; }

    [JsonProperty("Representation")]
    public string Representation { get; set; }

    [JsonProperty("Brand")]
    public string Brand { get; set; }

    [JsonProperty("Profile")]
    public string Profile { get; set; }

    [JsonProperty("Color")]
    public string Color { get; set; }

    [JsonProperty("Notes")]
    public string Note { get; set; }

    [JsonProperty("Remarks/Special Instructions")]
    public string Remarks { get; set; }

    [JsonProperty("Pre-Screening Results")]
    public string ScreeningStatus { get; set; }

    [JsonProperty("Pre-Screening Comments")]
    public string ScreeningRemarks { get; set; }

    [JsonProperty("Screener")]
    public string Screener { get; set; }

    [JsonProperty("Approver")]
    public string Approver { get; set; }
}

public class LookupModel
{
    public List<LookupPersonModel> users { get; set; }
}

public class LookupPersonModel
{
    public string email { get; set; }
    public string enName { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public string userId { get; set; }
}

public class LookupTextModel
{
    public string text { get; set; }
    public string type { get; set; }
}
public class ErrorMsgModel
{
    public Fields fields { get; set; }
}
public class Fields
{

    [JsonProperty("Error Message")]
    public string ErrorMessage { get; set; }

}
