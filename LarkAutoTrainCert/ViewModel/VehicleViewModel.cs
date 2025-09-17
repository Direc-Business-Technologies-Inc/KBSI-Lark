using Newtonsoft.Json;

namespace LarkAutoTrainCert.ViewModel
{
    public class VehicleModel
    {
        public VehicleFieldsModel fields { get; set; }
        public string id { get; set; }
        public string record_id { get; set; }
    }
    public class VehicleFieldsModel
    {
        [JsonProperty("Vehicle Plate No.")]
        public string Code { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string U_Description { get; set; }

        [JsonProperty("Status")]
        public string U_Active { get; set; }
    }
}
