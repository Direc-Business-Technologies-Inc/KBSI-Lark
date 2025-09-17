using Newtonsoft.Json;

namespace LarkAutoTrainCert.Model
{
    public class FileUploadModel
    {
        public FieldsFile fields { get; set; }
    }
    public class FieldsFile
    {
        //[JsonProperty("Generated PDS")]
        //public List<GeneratedPDS> GeneratedPDS { get; set; }

        [JsonProperty("GeneratedPGF")]
        public List<GeneratedPGF> GeneratedPGF { get; set; }



        [JsonProperty("Error Message")]
        public string ErrorMessage { get; set; }
    }

    public class GeneratedPDS
    {
        public string file_token { get; set; }
    }

    public class GeneratedPGF
    {
        public string file_token { get; set; }
    }


}
