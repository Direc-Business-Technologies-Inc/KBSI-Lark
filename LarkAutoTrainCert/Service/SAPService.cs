using System.Text;
using LarkAutoTrainCert.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using LarkAutoTrainCert.ViewModel;
using static LarkAutoTrainCert.ViewModel.ProjectModel;
using static LarkAutoTrainCert.ViewModel.BPModel;

namespace LarkAutoTrainCert.Service
{
    public class SAPService
    {

        private readonly HttpClient _client;
        private readonly LoginModel _sapcreds;
        //private readonly string slURL = "192.168.2.211";
        private readonly string slURL = "192.168.1.77";
        public SAPService(IOptions<LoginModel> larkSettings)
        {
            _client = new HttpClient();
            _sapcreds = larkSettings.Value;
        }
        public async Task<string> Login()
        {
            string url = $"http://{slURL}:50001/b1s/v2/Login";
            var loginCreds = new LoginModel
            {
                CompanyDB = _sapcreds.CompanyDB,

                UserName = _sapcreds.UserName,
                Password = _sapcreds.Password
            };

            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(loginCreds), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (responseBody.Contains("error"))
                {
                    response.EnsureSuccessStatusCode();

                }
                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> FINLogin()
        {
            string url = $"http://{slURL}:50001/b1s/v2/Login";
            var loginCreds = new LoginModel
            {
                CompanyDB = _sapcreds.CompanyDB,

                UserName = "fin02",
                Password = "Nov041988"
            };

            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(loginCreds), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (responseBody.Contains("error"))
                {
                    response.EnsureSuccessStatusCode();

                }
                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> PostBP(BPModel sapCustomer)
        {
            string url = $"http://{slURL}:50001/b1s/v2/BusinessPartners";

            try
            {
                string json = JsonConvert.SerializeObject(sapCustomer);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(sapCustomer), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(responseBody);
                var jsonDoc = JsonDocument.Parse(responseBody);

                if (!responseBody.Contains("error"))
                {
                    responseBody = jsonDoc.RootElement.GetProperty("CardCode").GetString();
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> GetBP()
        {
            string url = $"http://{slURL}:50001/b1s/v2/BusinessPartners('CEM00005')";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(responseBody);
                var jsonDoc = JsonDocument.Parse(responseBody);

                if (!responseBody.Contains("error"))
                {
                    responseBody = jsonDoc.RootElement.GetProperty("CardCode").GetString();
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> PostProject(ProjectModel sapProject)
        {
            string url = $"http://{slURL}:50001/b1s/v2/Projects";

            try
            {
                string json = JsonConvert.SerializeObject(sapProject);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(sapProject), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(responseBody);
                var jsonDoc = JsonDocument.Parse(responseBody);

                if (!responseBody.Contains("error"))
                {
                    responseBody = jsonDoc.RootElement.GetProperty("Code").GetString();
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<string> UpdateBPasCustomer(BPModel.bpCustomer sapCustomer)
        {
            string url = $"http://{slURL}:50001/b1s/v2/BusinessPartners('{sapCustomer.CardCode}')";

            try
            {
                string json = JsonConvert.SerializeObject(sapCustomer);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(sapCustomer), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                //var jsonResponse = JObject.Parse(responseBody);

                //var jsonDoc = JsonDocument.Parse(responseBody);
                //var cardCode = jsonDoc.RootElement.GetProperty("CardCode").GetString();

                return "Ok";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<string> UpdateBPasActive(InactiveBPModel bp, string cardCode)
        {
            string url = $"http://{slURL}:50001/b1s/v2/BusinessPartners('{cardCode}')";

            try
            {
                string json = JsonConvert.SerializeObject(bp);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(bp), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                return "Ok";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> UpdateProjectStartDate(string code, StartDateModel startDate)
        {
            string url = $"http://{slURL}:50001/b1s/v2/Projects('{code}')";

            try
            {
                string json = JsonConvert.SerializeObject(startDate);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(startDate), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<string> UpdateProjectFinishDate(string code, EndDateModel finishDate)
        {
            string url = $"http://{slURL}:50001/b1s/v2/Projects('{code}')";

            try
            {
                string json = JsonConvert.SerializeObject(finishDate);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(finishDate), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<string> UpdateProjectCompletion(string code, CompletionModel completion)
        {
            string url = $"http://{slURL}:50001/b1s/v2/Projects('{code}')";

            try
            {
                string json = JsonConvert.SerializeObject(completion);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(completion), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<string> UpdateBPasInactive(InactiveBPModel bp, string cardCode)
        {
            string url = $"http://{slURL}:50001/b1s/v2/BusinessPartners('{cardCode}')";

            try
            {
                string json = JsonConvert.SerializeObject(bp);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(bp), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<string> UpdateProjectasInactive(InactiveProjectModel project, string projectCode)
        {
            string url = $"http://{slURL}:50001/b1s/v2/Projects('{projectCode}')";

            try
            {
                string json = JsonConvert.SerializeObject(project);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(project), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PatchAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<string> PostAPInvoice(APInvoiceModel.DocumentWrapper sapApInvoice)
        {
            string url = $"http://{slURL}:50001/b1s/v2/PurchaseInvoices";

            try
            {
                string json = JsonConvert.SerializeObject(sapApInvoice);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(sapApInvoice), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(responseBody);
                var jsonDoc = JsonDocument.Parse(responseBody);

                if (!responseBody.Contains("error"))
                {
                    responseBody = jsonDoc.RootElement.GetProperty("DocEntry").GetDouble().ToString() + "," + jsonDoc.RootElement.GetProperty("DocNum").GetDouble().ToString();
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public async Task<string> PostAPDPInvoice(APDPInvoiceModel.DocumentWrapper sapApdpInvoice)
        {
            string url = $"http://{slURL}:50001/b1s/v2/PurchaseDownPayments";

            try
            {
                string json = JsonConvert.SerializeObject(sapApdpInvoice);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(sapApdpInvoice), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(url, jsonContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(responseBody);
                var jsonDoc = JsonDocument.Parse(responseBody);


                if (!responseBody.Contains("error"))
                {
                    responseBody = jsonDoc.RootElement.GetProperty("DocEntry").GetDouble().ToString() + "," + jsonDoc.RootElement.GetProperty("DocNum").GetDouble().ToString();
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
