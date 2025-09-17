using Newtonsoft.Json;

namespace LarkAutoTrainCert.ViewModel
{
    public class ExpenseTypeModel
    {
        public ExpenseFieldsModel fields { get; set; }
        public string id { get; set; }
        public string record_id { get; set; }
    }
    public class ExpenseFieldsModel
    {
        [JsonProperty("Expense Type")]
        public string ExpType { get; set; }

        [JsonProperty("Expense Name")]
        public string U_GL_Name { get; set; }

        [JsonProperty("G/L Account")]
        public string ExpAcct { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
    }
}
