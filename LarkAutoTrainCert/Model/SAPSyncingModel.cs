using Newtonsoft.Json;

namespace LarkAutoTrainCert.Model
{
    public class SAPSyncingModel
    {
        public string TableID { get; set; }
        public string RecordID { get; set; }
        public string? U_LarkReferenceID { get; set; }
        public string? TransactionType { get; set; }

        public class BPVendor
        {
            [JsonProperty("Employee/Payee Code")]
            public string CardCode { get; set; }

            [JsonProperty("Employee/Payee Name")]
            public string CardName { get; set; }

            [JsonProperty("GroupCode")]
            public string GroupCode { get; set; }

            [JsonProperty("CardType")]
            public string CardType { get; set; }

            [JsonProperty("Down Payment Clear Act")]
            public string DpmClear { get; set; }
            [JsonProperty("Status")]
            public string? validFor { get; set; }

        }

        public class ExpenseType
        {
            [JsonProperty("Expense Type")]
            public string ExpType { get; set; }

            [JsonProperty("Expense Name")]
            public string U_GL_Name { get; set; }

            [JsonProperty("G/L Account")]
            public string ExpAcct { get; set; }
        }
        public class TransType
        {
            [JsonProperty("Transaction Code")]
            public string Code { get; set; }

            [JsonProperty("Transaction Name")]
            public string Name { get; set; }

        }
        public class Showroom
        {
            [JsonProperty("Showroom")]
            public string PrcCode { get; set; }

            [JsonProperty("PrcName")]
            public string PrcName { get; set; }
            [JsonProperty("Status")]
            public string Active { get; set; }

        }
        public class Vehicle
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
}

