using Newtonsoft.Json;

namespace LarkAutoTrainCert.ViewModel
{
    public class ShowroomModel
    {
        public ShowroomFieldsModel fields { get; set; }
        public string id { get; set; }
        public string record_id { get; set; }
    }
    public class ShowroomFieldsModel
    {
        [JsonProperty("Showroom")]
        public string PrcCode { get; set; }

        [JsonProperty("PrcName")]
        public string PrcName { get; set; }
        [JsonProperty("Status")]
        public string Active { get; set; }
    }
}
