using Newtonsoft.Json;

namespace LarkAutoTrainCert.ViewModel
{
    public class BPModel
    {
        public string CardName { get; set; }
        public string CardType { get; set; }
        public int Series { get; set; }
        public int GroupCode { get; set; }
        public string Currency { get; set; }
        public int PayTermsGrpCode { get; set; }
        public string VatGroup { get; set; }
        public string VatLiable { get; set; }
        public string DebitorAccount { get; set; }
        public string DownPaymentClearAct { get; set; }
        public string FederalTaxID { get; set; }
        public string AliasName { get; set; }
        public string Notes { get; set; }
        public string ContactPerson { get; set; }
        public List<Contact> ContactEmployees { get; set; }

        public class Contact
        {
            public string Name { get; set; }
        }
        public List<Address> BPAddresses { get; set; }

        public class Address
        {
            public string AddressName { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
        }

        public class bpCustomer
        {
            public string CardCode { get; set; }
            public string CardType { get; set; }
            public string Notes { get; set; }
        }
        public class InactiveBPModel
        {
            public string Valid { get; set; }
            public string Frozen { get; set; }
            public string Notes { get; set; }
        }
        public class BPContactsModel
        {
            public ContactEmployees contacts { get; set; }
            public class ContactEmployees
            {
                public string CardCode { get; set; }
                public string Name { get; set; }
            }
        }
    }
    public class CardCodeModel
    {
        public Details fields { get; set; }

    }
    public class Details
    {
        [JsonProperty("CardCode")]
        public string CardCode { get; set; }

        [JsonProperty("ProjectCode")]
        public string ProjectCode { get; set; }

        [JsonProperty("Error Message")]
        public string ErrorMessage { get; set; }
    }
    public class BPVendorModel
    {
        public BPFieldsModel fields { get; set; }
        public string id { get; set; }
        public string record_id { get; set; }
    }
    public class BPFieldsModel
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
        public string validFor { get; set; }
    }
    public class BPStatus
    {
        public BPStatusModel fields { get; set; }
        public class BPStatusModel
        {
            public string Status { get; set; }

            [JsonProperty("Error Message")]
            public string ErrorMessage { get; set; }
        }
    }
}
