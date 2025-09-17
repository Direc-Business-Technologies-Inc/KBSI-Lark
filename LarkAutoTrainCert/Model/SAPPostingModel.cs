namespace LarkAutoTrainCert.Model
{
    public class SAPPostingModel
    {
        //Lark
        public string TableID { get; set; }
        public string RecordID { get; set; }

        //Customer
        public string CardName { get; set; }
        public string TIN { get; set; }
        public string AliasName { get; set; }
        public string CardType { get; set; }
        public int Series { get; set; }
        public int GroupCode { get; set; }
        public string Currency { get; set; }
        public int PayTermsGrpCode { get; set; }
        public string VatGroup { get; set; }
        public string VatLiable { get; set; }
        public string DebitorAccount { get; set; }
        public string DownPaymentClearAct { get; set; }
        public string ContactPerson { get; set; }
        public string Address { get; set; }
        public string BP { get; set; }

        //Project
        public string Code { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }

        public class UpdateBPModel
        {
            public string RecordID { get; set; }
            public string TableID { get; set; }
            public string CardCode { get; set; }
            public string CardType { get; set; }
            public string Notes { get; set; }
        }
        public class ProjectStartDateModel
        {
            public string Code { get; set; }
            public string RecordID { get; set; }
            public string TableID { get; set; }
            public string U_DateofStart { get; set; }
        }
        public class ProjectCompletionModel
        {
            public string Code { get; set; }
            public string U_Completion { get; set; }
            public string RecordID { get; set; }
            public string TableID { get; set; }
        }
        public class ProjectFinishDateModel
        {
            public string Code { get; set; }
            public string U_DateofFinish { get; set; }
            public string RecordID { get; set; }
            public string TableID { get; set; }
        }
    }
    public class LostDealModel
    {
        public string CardCode { get; set; }
        public string Code { get; set; }
        public string Active { get; set; }
        public string RecordID { get; set; }
        public string TableID { get; set; }
        public string Valid { get; set; }
        public string Frozen { get; set; }
        public string? Accomplishment { get; set; }
    }
}
