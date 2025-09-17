using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using LarkAutoTrainCert.Model;
using LarkAutoTrainCert.ViewModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace LarkAutoTrainCert.Service;

public class LarkService
{
    private readonly HttpClient _client;
    private readonly LarkSettings _larkSettings;
    private readonly FileSettings _fileSettings;
    public LarkService(IOptions<LarkSettings> larkSettings, IOptions<FileSettings> fileSettings)
    {
        _client = new HttpClient();
        _larkSettings = larkSettings.Value;
        _fileSettings = fileSettings.Value;
    }
    public async Task<string> GetAccessToken()
    {
        string url = "https://open.larksuite.com/open-apis/auth/v3/app_access_token/internal";
        var postData = new
        {
            app_id = _larkSettings.AppId,
            app_secret = _larkSettings.AppSecret
        };

        try
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(url, jsonContent);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["app_access_token"] != null)
            {
                return jsonResponse["app_access_token"].ToString();
            }
            else
            {
                return "app_access_token not found in the response.";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    public async Task<RecordModel> GetRecord(string appToken, string tableId, string recordId, string accessToken)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordId}?with_shared_url=true";

        try
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(responseBody);
            var items = json["data"]?["record"]?.ToObject<RecordModel>();

            if (items != null)
            {
                return items;
            }
            else
            {
                throw new Exception("Record List is null");
            }
        }
        catch (HttpRequestException httpRequestEx)
        {
            throw new Exception("An error occurred while making the HTTP request.", httpRequestEx);
        }
        catch (JsonException jsonEx)
        {
            throw new Exception("An error occurred while parsing the JSON response.", jsonEx);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred.", ex);
        }
    }

    //public async Task UpdateRecord(string fileToken, string recordId, string appToken, string tableId, string accessToken)
    //{
    //    string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordId}";

    //    try
    //    {
    //        FileUploadModel body = new FileUploadModel
    //        {
    //            fields = new Fields
    //            {
    //                GeneratedCertificate = new List<GeneratedCertificate>
    //                {
    //                    new GeneratedCertificate
    //                    {
    //                        file_token = fileToken
    //                    }
    //                }
    //            }
    //        };
    //        string jsonBody = JsonConvert.SerializeObject(body);
    //        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    //        HttpResponseMessage response = await _client.PutAsync(url, content).ConfigureAwait(false);

    //        response.EnsureSuccessStatusCode();

    //        string responseBody = await response.Content.ReadAsStringAsync();
    //        var jsonResponse = JObject.Parse(responseBody);

    //        if (jsonResponse["msg"].ToString() != "success")
    //        {
    //            throw new Exception("Updating the record failed.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception("An unexpected error occurred while updating the record.", ex);
    //    }
    //}

    //GET BP RECORDS
    public async Task<List<BPVendorModel>> GetLarkBPList(string appToken, string TableId, string accessToken)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{TableId}/records";

        try
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(responseBody);
            var items = json["data"]?["items"]?.ToObject<List<BPVendorModel>>();

            return items;
            //if (items != null)
            //{
            //    //SignatureDetailsModel signatureDetails = items.Where(x => x.fields.Employee[0].en_name == facilitatorName).FirstOrDefault();
            //    //return signatureDetails;

            //    return items;
            //}
            //else
            //{
            //    throw new Exception("Record List is null");
            //}
        }
        catch (HttpRequestException httpRequestEx)
        {
            throw new Exception("An error occurred while making the HTTP request.", httpRequestEx);
        }
        catch (JsonException jsonEx)
        {
            throw new Exception("An error occurred while parsing the JSON response.", jsonEx);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred.", ex);
        }
    }

    //CREATE BP RECORD
    public async Task CreateRecord(string appToken, string tableId, string accessToken, SAPSyncingModel.BPVendor bp)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records";

        try
        {
            BPVendorModel body = new BPVendorModel
            {
                fields = new BPFieldsModel
                {
                    CardCode = bp.CardCode,
                    CardType = bp.CardType,
                    CardName = bp.CardName,
                    GroupCode = bp.GroupCode,
                    DpmClear = bp.DpmClear,
                    validFor = bp.validFor

                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PostAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while updating the record.", ex);
        }
    }

    //DELETE BP RECORD
    public async Task DeleteRecord(string appToken, string tableId, string accessToken, string recordID)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordID}";

        try
        {
            //string jsonBody = JsonConvert.SerializeObject(body);
            //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.DeleteAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while updating the record.", ex);
        }
    }

    //UPDATE BP RECORD
    public async Task UpdateRecord(string appToken, string tableId, string accessToken, string recordID, string status)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordID}";

        try
        {
            BPStatus body = new BPStatus
            {
                fields = new BPStatus.BPStatusModel
                {
                    Status = status
                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PutAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while updating the record.", ex);
        }
    }

    //GET ET RECORDS
    public async Task<List<ExpenseTypeModel>> GetLarkETList(string appToken, string TableId, string accessToken)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{TableId}/records";

        try
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(responseBody);
            var items = json["data"]?["items"]?.ToObject<List<ExpenseTypeModel>>();

            return items;

            //if (items != null)
            //{
            //    return items;
            //}
            //else
            //{
            //    throw new Exception("Record List is null");
            //}
        }
        catch (HttpRequestException httpRequestEx)
        {
            throw new Exception("An error occurred while making the HTTP request.", httpRequestEx);
        }
        catch (JsonException jsonEx)
        {
            throw new Exception("An error occurred while parsing the JSON response.", jsonEx);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred.", ex);
        }
    }

    //CREATE ET RECORD
    public async Task CreateETRecord(string appToken, string tableId, string accessToken, SAPSyncingModel.ExpenseType et)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records";

        try
        {
            ExpenseTypeModel body = new ExpenseTypeModel
            {
                fields = new ExpenseFieldsModel
                {
                    ExpType = et.ExpType,
                    ExpAcct = et.ExpAcct,
                    U_GL_Name = et.U_GL_Name,
                    Status = "Y"
                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PostAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while updating the record.", ex);
        }
    }

    //GET TT RECORDS
    public async Task<List<TransactionTypeModel>> GetLarkTTList(string appToken, string TableId, string accessToken)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{TableId}/records";

        try
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(responseBody);
            var items = json["data"]?["items"]?.ToObject<List<TransactionTypeModel>>();

            return items;

            //if (items != null)
            //{
            //    return items;
            //}
            //else
            //{
            //    throw new Exception("Record List is null");
            //}
        }
        catch (HttpRequestException httpRequestEx)
        {
            throw new Exception("An error occurred while making the HTTP request.", httpRequestEx);
        }
        catch (JsonException jsonEx)
        {
            throw new Exception("An error occurred while parsing the JSON response.", jsonEx);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred.", ex);
        }
    }

    //CREATE ET RECORD
    public async Task CreateTTRecord(string appToken, string tableId, string accessToken, SAPSyncingModel.TransType tt)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records";

        try
        {
            TransactionTypeModel body = new TransactionTypeModel
            {
                fields = new TransFieldsModel
                {
                    Code = tt.Code,
                    Name = tt.Name,
                    Status = "Y"
                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PostAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while updating the record.", ex);
        }
    }

    //GET SHOWROOM RECORDS
    public async Task<List<ShowroomModel>> GetLarkSRList(string appToken, string TableId, string accessToken)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{TableId}/records";

        try
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(responseBody);
            var items = json["data"]?["items"]?.ToObject<List<ShowroomModel>>();

            return items;

            //if (items != null)
            //{
            //    return items;
            //}
            //else
            //{
            //    throw new Exception("Record List is null");
            //}
        }
        catch (HttpRequestException httpRequestEx)
        {
            throw new Exception("An error occurred while making the HTTP request.", httpRequestEx);
        }
        catch (JsonException jsonEx)
        {
            throw new Exception("An error occurred while parsing the JSON response.", jsonEx);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred.", ex);
        }
    }

    //CREATE SHOWROOM RECORD
    public async Task CreateSRRecord(string appToken, string tableId, string accessToken, SAPSyncingModel.Showroom sr)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records";

        try
        {
            ShowroomModel body = new ShowroomModel
            {
                fields = new ShowroomFieldsModel
                {
                    PrcCode = sr.PrcCode,
                    PrcName = sr.PrcName,
                    Active = sr.Active
                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PostAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while updating the record.", ex);
        }
    }

    //GET VEHICLE RECORDS
    public async Task<List<VehicleModel>> GetLarkVList(string appToken, string TableId, string accessToken)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{TableId}/records";

        try
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(responseBody);
            var items = json["data"]?["items"]?.ToObject<List<VehicleModel>>();

            return items;
        }
        catch (HttpRequestException httpRequestEx)
        {
            throw new Exception("An error occurred while making the HTTP request.", httpRequestEx);
        }
        catch (JsonException jsonEx)
        {
            throw new Exception("An error occurred while parsing the JSON response.", jsonEx);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred.", ex);
        }
    }
    //CREATE VEHICLE RECORD
    public async Task CreateVRecord(string appToken, string tableId, string accessToken, SAPSyncingModel.Vehicle v)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records";

        try
        {
            VehicleModel body = new VehicleModel
            {
                fields = new VehicleFieldsModel
                {
                    Code = v.Code,
                    Name = v.Name,
                    U_Description = v.U_Description,
                    U_Active = v.U_Active
                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PostAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while updating the record.", ex);
        }
    }
    public async Task UpdateRecordError(string errorMessage, string recordId, string appToken, string tableId, string accessToken)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordId}";
        //"D:\\Documents\\CBS\\Projects\\VisitorManagement\\GeneratedQR\\test.png"
        try
        {
            ErrorMsgModel body = new ErrorMsgModel
            {
                fields = new Fields
                {
                    ErrorMessage = errorMessage
                }
            };
            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PutAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred.", ex);
        }
    }
    public async Task UpdateCardCode(string recordId, string appToken, string tableId, string accessToken, string cardCode, string projectCode)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordId}";

        try
        {

            CardCodeModel body = new CardCodeModel
            {
                fields = new Details
                {
                    CardCode = cardCode,
                    ProjectCode = projectCode
                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PutAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    public async Task UpdateSAPDPDetails(string recordId, string appToken, string tableId, string accessToken, string returnMsg, string docEntry, string docNum, string balance)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordId}";

        try
        {

            APInvoiceModel.APInvoiceDocEntryModel body = new APInvoiceModel.APInvoiceDocEntryModel
            {
                fields = new APInvoiceModel.APDetails
                {
                    DocEntry = docEntry,
                    DocNum = docNum,
                    APDPDocEntry = docEntry,
                    APDPDocNum = docNum,
                    ErrorMessage = returnMsg,
                    Balance = balance
                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PutAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    public async Task UpdateSAPInvoiceDetails(string recordId, string appToken, string tableId, string accessToken, string returnMsg, string docEntry, string docNum)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordId}";

        try
        {

            APInvoiceModel.APInvoiceDocEntryModel body = new APInvoiceModel.APInvoiceDocEntryModel
            {
                fields = new APInvoiceModel.APDetails
                {
                    DocEntry = docEntry,
                    DocNum = docNum,
                    InvoiceDocEntry = docEntry,
                    InvoiceDocNum = docNum,
                    ErrorMessage = returnMsg
                }
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PutAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<string> GeneratePGF(FieldsModel pdsDetails, string siteLocation, string projectName, string regDate, string approver, string sales, string screener)
    {
        try
        {
            string pdsPath = _fileSettings.GeneratedPgfPath + "_" + pdsDetails.ProjectCode + "_" + DateTime.Today.ToString("yyyyMMdd") + ".docx";

            File.Copy(_fileSettings.GeneratedPgfPath, pdsPath, true);

            using (var doc = WordprocessingDocument.Open(pdsPath, true))
            {
                MainDocumentPart mainPart = doc.MainDocumentPart;

                foreach (var text in mainPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                {
                    if (text.Text == "RegDate")
                        text.Text = text.Text.Replace("RegDate", regDate);
                    else if (text.Text == "Source")
                        text.Text = text.Text.Replace("Source", pdsDetails.Source);
                    else if (text.Text == "ProjectName")
                        text.Text = text.Text.Replace("ProjectName", projectName);
                    else if (text.Text == "ContactNo")
                        text.Text = text.Text.Replace("ContactNo", pdsDetails.ContactNo);
                    else if (text.Text == "Owner")
                        text.Text = text.Text.Replace("Owner", pdsDetails.Owner);
                    else if (text.Text == "Site")
                        text.Text = text.Text.Replace("Site", siteLocation);
                    else if (text.Text == "Architect")
                        text.Text = text.Text.Replace("Architect", pdsDetails.Architect);
                    else if (text.Text == "Contractor")
                        text.Text = text.Text.Replace("Contractor", pdsDetails.Contractor);
                    else if (text.Text == "Commi")
                        text.Text = text.Text.Replace("Commi", pdsDetails.Commi);
                    else if (text.Text == "Representation")
                        text.Text = text.Text.Replace("Representation", pdsDetails.Representation);
                    else if (text.Text == "Brand")
                        text.Text = text.Text.Replace("Brand", pdsDetails.Brand);
                    else if (text.Text == "Profile")
                        text.Text = text.Text.Replace("Profile", pdsDetails.Profile);
                    else if (text.Text == "Color")
                        text.Text = text.Text.Replace("Color", pdsDetails.Color);
                    else if (text.Text == "Note")
                        text.Text = text.Text.Replace("Note", pdsDetails.Note);
                    else if (text.Text == "Remarks")
                        text.Text = text.Text.Replace("Remarks", pdsDetails.Remarks);
                    else if (text.Text == "SalesExecutive")
                        text.Text = text.Text.Replace("SalesExecutive", sales);
                    else if (text.Text == "Screener")
                        text.Text = text.Text.Replace("Screener", screener);
                    else if (text.Text == "ScreeningStatus")
                        text.Text = text.Text.Replace("ScreeningStatus", pdsDetails.ScreeningStatus);
                    else if (text.Text == "ScreeningRemarks")
                        text.Text = text.Text.Replace("ScreeningRemarks", pdsDetails.ScreeningRemarks);
                    else if (text.Text == "Screener")
                        text.Text = text.Text.Replace("Screener", pdsDetails.ProjectName);
                    else if (text.Text == "Approver")
                        text.Text = text.Text.Replace("Approver", approver);
                }

                doc.Save();
                Log.Information("Document Saved");
            }

            string pdfcertificatePath = ConvertToPdf(pdsPath);
            if (File.Exists(pdfcertificatePath))
            {
                Log.Information("PDF Saved");
            }
            else
            {
                Log.Error("File does not exist.");
            }

            return System.IO.Path.GetFileName(pdfcertificatePath);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<string> GeneratePDS(FieldsModel pdsDetails)
    {
        try
        {
            string pdsPath = _fileSettings.GeneratedPdsPath + pdsDetails.ProjectName + "_" + DateTime.Today.ToString("yyyyMMdd") + ".docx";

            File.Copy(_fileSettings.GeneratedPdsPath, pdsPath, true);

            using (var doc = WordprocessingDocument.Open(_fileSettings.GeneratedPdsPath, true))
            {
                MainDocumentPart mainPart = doc.MainDocumentPart;

                //foreach (var placeholder in mainPart.Document.Descendants<Text>())
                //{
                //    if (placeholder.Text.Contains("ProjectName"))
                //    {
                //        placeholder.Text = placeholder.Text.Replace("ProjectName", pdsDetails.ProjectName);
                //    }
                //    else if (placeholder.Text.Contains("ProjectOwner"))
                //    {
                //        placeholder.Text = placeholder.Text.Replace("ProjectOwner", pdsDetails.ProjectOwner);
                //    }
                //}

                //foreach (var text in mainPart.Document.Descendants<Text>())
                //{
                //    if (text.Text == "LPROJECTNAME")
                //        text.Text = pdsDetails.ProjectName ?? "";

                //    else if (text.Text == "LOWNER")
                //        text.Text = pdsDetails.ProjectOwner ?? "";


                //}

                doc.Save();
                Log.Information("Document Saved");
            }
            string pdfcertificatePath = ConvertToPdf(pdsPath);
            if (File.Exists(pdfcertificatePath))
            {
                Log.Information("PDF Saved");
            }
            else
            {
                Log.Error("File does not exist.");
            }
            return System.IO.Path.GetFileName(pdfcertificatePath);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    private string ConvertToPdf(string sourceDocPath)
    {
        try
        {
            string pdfPath = System.IO.Path.ChangeExtension(sourceDocPath, ".pdf");

            // Path to LibreOffice
            string libreOfficePath = @"C:\Program Files\LibreOffice\program\soffice.exe"; // Adjust if necessary

            var startInfo = new ProcessStartInfo
            {
                FileName = libreOfficePath,
                Arguments = $"--headless --convert-to pdf --outdir \"{System.IO.Path.GetDirectoryName(pdfPath)}\" \"{sourceDocPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
            }

            return pdfPath;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while converting to PDF");
            throw;
        }
    }
    public async Task<string> UploadFile(string fileName, string appToken, string accessToken)
    {
        string url = "https://open.larksuite.com/open-apis/drive/v1/medias/upload_all";
        string filePath = System.IO.Path.Combine(_fileSettings.GeneratedCertPath, fileName);
        try
        {
            var fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            using (var content = new MultipartFormDataContent("----7MA4YWxkTrZu0gW"))
            {
                content.Add(new StringContent(fileName), "file_name");
                content.Add(new StringContent("bitable_file"), "parent_type");
                content.Add(new StringContent(appToken), "parent_node");
                content.Add(new StringContent(fileSize.ToString()), "size");

                byte[] fileData = await File.ReadAllBytesAsync(filePath);
                var fileContent = new ByteArrayContent(fileData);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content.Add(fileContent, "file", fileName);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await _client.PostAsync(url, content).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseBody);

                if (jsonResponse["data"]["file_token"] != null)
                {
                    return jsonResponse["data"]["file_token"].ToString();
                }
                else
                {
                    return "Upload failed.";
                }
            }

        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred.", ex);
        }
    }

    public void LogError(string message)
    {
        var logPath = "C:\\Users\\Administrator\\Downloads\\debug_log.txt";
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[{timestamp}] ERROR: {message}\n";

        File.AppendAllText(logPath, logEntry);
    }

    public void LogError(Exception ex)
    {
        var logPath = "C:\\Users\\Administrator\\Downloads\\debug_log.txt";
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[{timestamp}] ERROR: {ex.Message}\n{ex.StackTrace}\n\n";

        File.AppendAllText(logPath, logEntry);
    }
    //public async Task UpdateRecordPDS(string fileToken, string recordId, string appToken, string tableId, string accessToken)
    //{
    //    string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordId}";

    //    try
    //    {
    //        FileUploadModel body = new FileUploadModel
    //        {
    //            fields = new Fields
    //            {
    //                ErrorMessage = new List<GeneratedPDS>
    //                {
    //                    new GeneratedPDS
    //                    {
    //                        file_token = fileToken
    //                    }
    //                }
    //            }
    //        };
    //        string jsonBody = JsonConvert.SerializeObject(body);
    //        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    //        HttpResponseMessage response = await _client.PutAsync(url, content).ConfigureAwait(false);

    //        response.EnsureSuccessStatusCode();

    //        string responseBody = await response.Content.ReadAsStringAsync();
    //        var jsonResponse = JObject.Parse(responseBody);

    //        if (jsonResponse["msg"].ToString() != "success")
    //        {
    //            throw new Exception("Updating the record failed.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception("An unexpected error occurred while updating the record.", ex);
    //    }
    //}

    public static void ReplacePlaceholders(MainDocumentPart mainPart, Dictionary<string, string> replacements)
    {
        foreach (var paragraph in mainPart.Document.Descendants<Paragraph>())
        {
            string paragraphText = string.Concat(paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>().Select(t => t.Text));

            foreach (var kvp in replacements)
            {
                if (paragraphText.Contains(kvp.Key))
                {
                    paragraphText = paragraphText.Replace(kvp.Key, kvp.Value ?? "");
                    // remove all runs and rebuild paragraph
                    paragraph.RemoveAllChildren<Run>();
                    paragraph.AppendChild(new Run(new DocumentFormat.OpenXml.Drawing.Text(paragraphText)));
                }
            }
        }
    }

    public async Task UpdateFile(string fileToken, string recordId, string appToken, string tableId, string accessToken)
    {
        string url = $"https://open.larksuite.com/open-apis/bitable/v1/apps/{appToken}/tables/{tableId}/records/{recordId}";

        try
        {
            FileUploadModel body = new FileUploadModel
            {
                fields = new FieldsFile
                {
                    GeneratedPGF = new List<GeneratedPGF>
                    {
                        new GeneratedPGF
                        {
                            file_token = fileToken
                        }
                    }
                }
            };
            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.PutAsync(url, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["msg"].ToString() != "success")
            {
                throw new Exception("Updating the record failed.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while downloading the signature.", ex);
        }
    }
}