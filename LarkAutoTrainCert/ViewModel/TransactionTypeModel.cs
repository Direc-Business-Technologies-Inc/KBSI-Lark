using Newtonsoft.Json;

namespace LarkAutoTrainCert.ViewModel
{
    public class TransactionTypeModel
    {
        public TransFieldsModel fields { get; set; }
        public string id { get; set; }
        public string record_id { get; set; }
    }
    public class TransFieldsModel
    {
        [JsonProperty("Transaction Code")]
        public string Code { get; set; }

        [JsonProperty("Transaction Type")]
        public string Name { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
    }
}
