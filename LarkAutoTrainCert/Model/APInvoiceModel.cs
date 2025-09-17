using Newtonsoft.Json;

namespace LarkAutoTrainCert.Model
{
    public class APInvoiceModel
    {
        //Lark
        public string TableID { get; set; }
        public string RecordID { get; set; }
        public string U_ReferenceID { get; set; }

        //AP INVOICE
        public string CardCode { get; set; }
        public string DocType { get; set; }
        public string U_PurchType { get; set; }
        public string U_PrepBy { get; set; }
        public string U_TransactionType { get; set; }
        public string U_ProjectRelated { get; set; }
        public string Project { get; set; }
        public string ProjectName { get; set; }
        public string Details { get; set; }
        public class DocumentLine
        {
            public int LineNum { get; set; }
            public string U_API_Vendor { get; set; }
            public string U_API_TIN { get; set; }
            public string U_API_Address { get; set; }
            public string AccountCode { get; set; }
            public string ProjectCode { get; set; }
            public string ItemDescription { get; set; }
            public string U_ExpenseType { get; set; }
            public string CostingCode { get; set; }
            public string U_PlateNo { get; set; }
            public string UnitPrice { get; set; }
            public string VatGroup { get; set; }
        }
        public class DownPaymentsToDraw
        {
            public string DocEntry { get; set; }
            public string AmountToDraw { get; set; }
        }

        public class DocumentWrapper
        {
            public string CardCode { get; set; }
            public string DocType { get; set; }
            public string U_PurchType { get; set; }
            public string U_PrepBy { get; set; }
            public string U_TrnsType { get; set; }
            public string U_ProjectRelated { get; set; }
            public string Project { get; set; }
            public string U_BPName { get; set; }
            public string U_Remarks { get; set; }
            public string U_ReferenceID { get; set; }
            public string U_RegName { get; set; }

            public string Comments { get; set; }
            public List<DocumentLine> DocumentLines { get; set; }
            public List<DownPaymentsToDraw> DownPaymentsToDraw { get; set; }
        }

        //APInvoiceDocEntry
        public class APInvoiceDocEntryModel
        {
            public APDetails fields { get; set; }

        }
        public class APDetails
        {
            [JsonProperty("SAP Doc Entry")]
            public string DocEntry { get; set; }

            [JsonProperty("SAP Doc Num")]
            public string DocNum { get; set; }

            [JsonProperty("AP DP Entry")]
            public string APDPDocEntry { get; set; }

            [JsonProperty("AP DP Reference No.")]
            public string APDPDocNum { get; set; }

            [JsonProperty("AP Invoice Entry")]
            public string InvoiceDocEntry { get; set; }

            [JsonProperty("AP Invoice Reference No.")]
            public string InvoiceDocNum { get; set; }

            [JsonProperty("Balance")]
            public string Balance { get; set; }

            [JsonProperty("Error Message")]
            public string ErrorMessage { get; set; }
        }

    }
}
