namespace LarkAutoTrainCert.ViewModel
{
    public class ProjectModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Active { get; set; }
        public string U_Remarks { get; set; }
        public string U_BPCode { get; set; }
        public string U_BPName { get; set; }
        public string ValidFrom { get; set; }
        public class CompletionModel
        {
            public double U_Completion { get; set; }
            public string U_Remarks { get; set; }
        }
        public class StartDateModel
        {
            public string U_DateofStart { get; set; }
            public string U_Remarks { get; set; }
        }
        public class EndDateModel
        {
            public string U_DateofFinish { get; set; }
            public string U_Remarks { get; set; }

        }
        public class InactiveProjectModel
        {
            public string Active { get; set; }
            public string U_Remarks { get; set; }
        }
    }
}
