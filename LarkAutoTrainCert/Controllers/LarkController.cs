using System.Data;
using System.Text.RegularExpressions;
using LarkAutoTrainCert.Helpers;
using LarkAutoTrainCert.Model;
using LarkAutoTrainCert.Service;
using LarkAutoTrainCert.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using static LarkAutoTrainCert.Model.APInvoiceModel;
using static LarkAutoTrainCert.Model.SAPPostingModel;
using static LarkAutoTrainCert.ViewModel.BPModel;
using static LarkAutoTrainCert.ViewModel.ProjectModel;

namespace LarkAutoTrainCert.Controllers;

[ApiController]
[Route("[controller]")]
public class LarkController : ControllerBase
{
    private readonly LarkService _larkService;
    private readonly SAPService _sapService;
    private readonly MSSQLHelper _sqlHelper;

    //public LarkController(LarkService larkService, GenerateCertificateService generateCertificateService)
    public LarkController(LarkService larkService, SAPService sapService, MSSQLHelper sqlHelper)
    {
        _larkService = larkService;
        _sapService = sapService;
        _sqlHelper = sqlHelper;
    }

    [HttpPost]
    [Route("PostBPnProject")]
    public async Task<IActionResult> Post([FromBody] SAPPostingModel data)
    {

        string accessToken = "";
        string msg = "";
        string cardCode = "";
        string projectCode = "";

        string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

        try
        {
            //CHECK BP IF EXISTING
            string checkTINifExisting = $"select \"CardCode\", \"CardName\", \"validFor\" from OCRD where CAST(\"AliasName\" as NVARCHAR(max)) = '{data.AliasName}'";
            var bpCode = _sqlHelper.GetData(checkTINifExisting, _sqlHelper.GetConnection());

            //GET LAST PROJECT CODE
            string getLastProjectCode = "select max(PrjCode) as \"Project Code\" from OPRJ";
            var pcode = _sqlHelper.GetData(getLastProjectCode, _sqlHelper.GetConnection());
            string code = pcode.Rows[0]["Project Code"].ToString();
            string prefix = code.Substring(0, 4);
            string numericPart = code.Substring(code.Length - 6);
            int incremented = int.Parse(numericPart) + 1;
            string newNumericPart = incremented.ToString("D6");
            string result = prefix + newNumericPart;

            // First 100 characters
            string address1 = data.Address.Length > 100 ? data.Address.Substring(0, 100) : data.Address;

            // Second 100 characters
            string adddress2 = data.Address.Length > 200 ? data.Address.Substring(100, 100) :
                          (data.Address.Length > 100 ? data.Address.Substring(100) : string.Empty);

            if (bpCode.Rows.Count < 1)
            {
                if (pcode.Rows.Count > 0)
                {
                    var contact = new BPModel.Contact
                    {
                        Name = data.ContactPerson
                    };

                    var address = new BPModel.Address
                    {
                        Street = address1,
                        City = adddress2,
                        AddressName = "From Lark"
                    };

                    var bp = new BPModel
                    {
                        CardName = data.CardName,
                        CardType = data.CardType,
                        Series = data.Series,
                        GroupCode = data.GroupCode,
                        Currency = data.Currency,
                        PayTermsGrpCode = data.PayTermsGrpCode,
                        VatGroup = data.VatGroup,
                        VatLiable = data.VatLiable,
                        DebitorAccount = data.DebitorAccount,
                        DownPaymentClearAct = data.DownPaymentClearAct,
                        FederalTaxID = data.TIN,
                        AliasName = data.AliasName,
                        ContactPerson = data.ContactPerson,
                        Notes = "Posted through Lark Sales CRM Workflow" + ": " + DateTime.Now.ToString(),

                        ContactEmployees = [contact],
                        BPAddresses = [address]
                    };

                    msg = await _sapService.Login();


                    if (!msg.Contains("error"))
                    {
                        cardCode = await _sapService.PostBP(bp);


                        if (!cardCode.Contains("error"))
                        {
                            var project = new ProjectModel
                            {
                                Code = result,
                                Name = data.Name,
                                Active = data.Active,
                                ValidFrom = DateTime.Now.ToString("yyyyMMdd"),
                                U_BPCode = cardCode,
                                U_BPName = data.CardName,
                                U_Remarks = "Posted through Lark Project Portfolio Workflow" + ": " + DateTime.Now.ToString()
                            };

                            projectCode = await _sapService.PostProject(project);

                            if (!projectCode.Contains("error"))
                            {
                                accessToken = await _larkService.GetAccessToken();
                                await _larkService.UpdateCardCode(data.RecordID, appToken, data.TableID, accessToken, cardCode, projectCode);
                            }
                            else
                            {
                                msg = cardCode;
                            }
                        }
                        else
                        {
                            msg = cardCode;
                        }
                    }
                }
            }
            else
            {
                msg = await _sapService.Login();

                if (!msg.Contains("error"))
                {
                    var inactiveBP = new InactiveBPModel
                    {
                        Frozen = "N",
                        Valid = "Y"
                    };

                    string bpValid = bpCode.Rows[0]["validFor"].ToString() == "N" ? await _sapService.UpdateBPasActive(inactiveBP, bpCode.Rows[0]["CardCode"].ToString()) : "";

                
                    var project = new ProjectModel
                    {
                        Code = result,
                        Name = data.Name,
                        Active = data.Active,
                        ValidFrom = DateTime.Now.ToString("yyyyMMdd"),
                        U_BPCode = bpCode.Rows[0]["CardCode"].ToString(),
                        U_BPName = bpCode.Rows[0]["CardName"].ToString(),
                        U_Remarks = "Posted through Lark Project Portfolio Workflow" + ": " + DateTime.Now.ToString()
                    };

                    projectCode = await _sapService.PostProject(project);

                    if (!projectCode.Contains("error"))
                    {
                        accessToken = await _larkService.GetAccessToken();
                        await _larkService.UpdateCardCode(data.RecordID, appToken, data.TableID, accessToken, bpCode.Rows[0]["CardCode"].ToString(), projectCode);
                    }
                    else
                    {
                        msg = projectCode;
                    }
                }

            }

            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(msg, data.RecordID, appToken, data.TableID, accessToken);

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }
    [HttpPost]
    [Route("UpdateBPasCustomer")]
    public async Task<IActionResult> UpdateBPasCustomer([FromBody] SAPPostingModel.UpdateBPModel data)
    {

        
        string accessToken = "";
        string msg = "";
        string cardCode = "";
        string projectCode = "";

        string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

        try
        {
            var updateBP = new BPModel.bpCustomer
            {
                CardCode = data.CardCode,
                CardType = data.CardType,
                Notes = "Updated through Lark Sales CRM Workflow" + ": " + DateTime.Now.ToString()
            };

            msg = await _sapService.Login();

            if (!msg.Contains("error"))
            {
                cardCode = await _sapService.UpdateBPasCustomer(updateBP);
            }

            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(cardCode, data.RecordID, appToken, data.TableID, accessToken);
            return Ok();

        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }
    [HttpPost]
    [Route("UpdateProjectStartDate")]
    public async Task<IActionResult> Post([FromBody] SAPPostingModel.ProjectStartDateModel data)
    {

        string accessToken = "";
        string msg = "";
        string returnMsg = "";

        string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

        try
        {
            var startDate = new StartDateModel
            {
                U_DateofStart = data.U_DateofStart,
                U_Remarks = "Updated through Lark Project Portfolio Workflow" + ": " + DateTime.Now.ToString()
            };

            msg = await _sapService.Login();

            if (!msg.Contains("error"))
            {
                returnMsg = await _sapService.UpdateProjectStartDate(data.Code, startDate);
            }

            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(returnMsg, data.RecordID, appToken, data.TableID, accessToken);

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("UpdateProjectFinishDate")]
    public async Task<IActionResult> Post([FromBody] ProjectFinishDateModel data)
    {

        string accessToken = "";
        string msg = "";
        string returnMsg = "";

        string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

        try
        {
            var finishDate = new EndDateModel
            {
                U_DateofFinish = data.U_DateofFinish,
                U_Remarks = "Updated through Lark Project Portfolio Workflow" + ": " + DateTime.Now.ToString()
            };

            msg = await _sapService.Login();

            if (!msg.Contains("error"))
            {
                returnMsg = await _sapService.UpdateProjectFinishDate(data.Code, finishDate);
            }

            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(returnMsg, data.RecordID, appToken, data.TableID, accessToken);

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("UpdateProjectCompletion")]
    public async Task<IActionResult> Post([FromBody] ProjectCompletionModel data)
    {

        string accessToken = "";
        string msg = "";
        string returnMsg = "";

        string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

        try
        {
            var completion = new CompletionModel
            {
                U_Completion = Convert.ToDouble(data.U_Completion),
                U_Remarks = "Updated through Lark Project Portfolio Workflow" + ": " + DateTime.Now.ToString()
            };

            msg = await _sapService.Login();

            if (!msg.Contains("error"))
            {
                returnMsg = await _sapService.UpdateProjectCompletion(data.Code, completion);
            }

            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(returnMsg, data.RecordID, appToken, data.TableID, accessToken);

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }
    [HttpPost]
    [Route("UpdateBPnProjectInactive")]
    public async Task<IActionResult> Post([FromBody] LostDealModel data)
    {

        string accessToken = "";
        string msg = "";
        string returnMsg = "";

        string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

        var inactiveBP = new InactiveBPModel
        {
            Frozen = data.Frozen,
            Valid = data.Valid,
            Notes = "Updated through Lark Sales CRM Workflow" + ": " + DateTime.Now.ToString(),
        };


        var inactiveProject = new InactiveProjectModel
        {
            Active = data.Active,
            U_Remarks = "Updated through Lark Sales CRM Workflow" + ": " + DateTime.Now.ToString()
        };

        try
        {
            if (data.Accomplishment == "")
            {
                //CHECK IF BP IS CUSTOMER
                string checkCustomer = $"select CardType from OCRD where CardCode = '{data.CardCode}'";
                var isCustomer = _sqlHelper.GetData(checkCustomer, _sqlHelper.GetConnection());

                var cardType = isCustomer.Rows[0]["CardType"].ToString();

                if (cardType == "C")
                {
                    msg = await _sapService.Login();

                    if (!msg.Contains("error"))
                    {
                        returnMsg = await _sapService.UpdateProjectasInactive(inactiveProject, data.Code);
                    }
                }
                else
                {
                    //CHECK IF BP HAS ACTIVE PROJECTS
                    string activeProj = $"select T0.PrjCode from OPRJ T0 inner join OCRD T1 ON T0.U_BPCode = T1.CardCode where T0.Active = 'Y' and T1.CardType = 'L' and T0.U_Completion is not null and isnull(T0.U_BPCode,'') = '{data.CardCode}'";
                    var activeExists = _sqlHelper.GetData(activeProj, _sqlHelper.GetConnection());

                    msg = await _sapService.Login();

                    if (activeExists.Rows.Count > 1)
                    {
                        if (!msg.Contains("error"))
                        {
                            returnMsg = await _sapService.UpdateProjectasInactive(inactiveProject, data.Code);
                        }
                    }
                    else
                    {
                        returnMsg = await _sapService.UpdateProjectasInactive(inactiveProject, data.Code);
                        returnMsg = await _sapService.UpdateBPasInactive(inactiveBP, data.CardCode);
                    }
                }
            }
            else
            {
                returnMsg = "Cannot update project as inactive due to ongoing progress";
            }

            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(returnMsg, data.RecordID, appToken, data.TableID, accessToken);

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError(ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //AP INVOICE POSTING
    [HttpPost]
    [Route("PostAPInvoice")]
    public async Task<IActionResult> PostAPInvoice([FromBody] APInvoiceModel data)
    {

        string accessToken = "";
        string msg = "";
        string cardCode = "";
        string projectCode = "";

        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        try
        {

            var records = data.Details.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var docLines = new List<DocumentLine>();

            int lineNum = 0;

            foreach (var record in records)
            {
                var line = new DocumentLine { LineNum = lineNum++ };
                var pairs = Regex.Matches(record, @"([^:,]+):\s*([^,]+)")
                                 .Cast<Match>();

                foreach (var match in pairs)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();

                    switch (key)
                    {
                        case "Vendor":
                            line.U_API_Vendor = value;
                            break;
                        case "TIN":
                            line.U_API_TIN = value;
                            break;
                        case "Address":
                            line.U_API_Address = value;
                            break;
                        case "G/L Account":
                            line.AccountCode = value;
                            break;
                        case "Particulars":
                            line.ItemDescription = value;
                            break;
                        case "Expense Name":
                            line.U_ExpenseType = value;
                            break;
                        case "Vehicle Plate No.":
                            line.U_PlateNo = value;
                            break;
                        case "Showroom":
                            line.CostingCode = value;
                            break;
                        case "Amount":
                            line.UnitPrice = value;
                            break;
                        case "Tax Code":
                            line.VatGroup = value;
                            break;
                    }
                }

                line.ProjectCode = data.Project; // sample static code
                docLines.Add(line);
            }

            var docWrapper = new DocumentWrapper
            {
                CardCode = data.CardCode,
                DocType = data.DocType,
                U_PurchType = data.U_PurchType,
                U_PrepBy = data.U_PrepBy,
                U_TrnsType = data.U_TransactionType,
                U_ProjectRelated = data.U_ProjectRelated,
                Project = data.Project,
                U_BPName = data.ProjectName,
                U_ReferenceID = data.U_ReferenceID,
                U_RegName = "000-675-315-005",
                U_Remarks = "Posted through Lark Finance Approval Workflow" + ": " + DateTime.Now.ToString(),
                Comments = "Posted through Lark Finance Approval Workflow" + ": " + DateTime.Now.ToString(),
                DocumentLines = docLines
            };

            string json = JsonConvert.SerializeObject(docWrapper, Formatting.Indented);
            Console.WriteLine(json);

            msg = await _sapService.FINLogin();

            var returnMsg = "";
            var docNum = "";
            var docEntry = "";

            if (!msg.Contains("error"))
            {
                returnMsg = await _sapService.PostAPInvoice(docWrapper);
            }

            accessToken = await _larkService.GetAccessToken();

            if (!returnMsg.Contains("error"))
            {
                var docDeets = returnMsg.Split(",");
                if (docDeets.Count() > 1)
                {
                    docEntry = docDeets[0].ToString();
                    docNum = docDeets[1].ToString();
                    returnMsg = "Successfully posted to SAP" + ": " + DateTime.Now.ToString();
                }
                await _larkService.UpdateSAPInvoiceDetails(data.RecordID, appToken, data.TableID, accessToken, returnMsg, docEntry, docNum);
            }
            else
            {
                await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + returnMsg, data.RecordID, appToken, data.TableID, accessToken);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //AP DP INVOICE POSTING
    [HttpPost]
    [Route("PostAPDPInvoice")]
    public async Task<IActionResult> PostAPDPInvoice([FromBody] APDPInvoiceModel data)
    {

        string accessToken = "";
        string msg = "";
        string cardCode = "";
        string projectCode = "";

        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        try
        {

            var records = data.Details.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var docLines = new List<APDPInvoiceModel.DocumentLine>();

            int lineNum = 0;


            foreach (var record in records)
            {
                var line = new APDPInvoiceModel.DocumentLine { LineNum = lineNum++ };
                var pairs = Regex.Matches(record, @"([^:,]+):\s*([^,]+)")
                                 .Cast<Match>();

                foreach (var match in pairs)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();

                    switch (key)
                    {
                        case "Vendor":
                            line.U_API_Vendor = value;
                            break;
                        case "TIN":
                            line.U_API_TIN = value;
                            break;
                        case "Address":
                            line.U_API_Address = value;
                            break;
                        case "G/L Account":
                            line.AccountCode = data.U_DPClearAct;
                            break;
                        case "Particulars":
                            line.ItemDescription = value;
                            break;
                        case "Expense Name":
                            line.U_ExpenseType = value;
                            break;
                        case "Vehicle Plate No.":
                            line.U_PlateNo = value;
                            break;
                        case "Showroom":
                            line.CostingCode = value;
                            break;
                        case "Amount":
                            line.UnitPrice = value;
                            break;
                        case "Tax Code":
                            line.VatGroup = value;
                            break;
                    }
                }

                line.ProjectCode = data.Project; // sample static code
                docLines.Add(line);
            }

            var docWrapper = new APDPInvoiceModel.DocumentWrapper
            {
                CardCode = data.CardCode,
                DocType = data.DocType,
                U_PurchType = data.U_PurchType,
                U_PrepBy = data.U_PrepBy,
                U_TrnsType = data.U_TransactionType,
                U_ProjectRelated = data.U_ProjectRelated,
                Project = data.Project,
                U_BPName = data.ProjectName,
                U_ReferenceID = data.U_ReferenceID,
                DownPaymentType = "dptInvoice",
                U_Remarks = "Posted through Lark Finance Approval Workflow" + ": " + DateTime.Now.ToString(),
                Comments = "Posted through Lark Finance Approval Workflow" + ": " + DateTime.Now.ToString(),
                U_RegName = "000-675-315-005",
                DocumentLines = docLines
            };

            string json = JsonConvert.SerializeObject(docWrapper, Formatting.Indented);
            Console.WriteLine(json);

            msg = await _sapService.FINLogin();

            var returnMsg = "";
            var docNum = "";
            var docEntry = "";

            if (!msg.Contains("error"))
            {
                returnMsg = await _sapService.PostAPDPInvoice(docWrapper);
            }

            accessToken = await _larkService.GetAccessToken();

            if (!returnMsg.Contains("error"))
            {
                var docDeets = returnMsg.Split(",");
                if (docDeets.Count() > 1)
                {
                    docEntry = docDeets[0].ToString();
                    docNum = docDeets[1].ToString();
                    returnMsg = "Successfully posted to SAP" + ": " + DateTime.Now.ToString();

                }
                await _larkService.UpdateSAPInvoiceDetails(data.RecordID, appToken, data.TableID, accessToken, returnMsg, docEntry, docNum);
            }
            else
            {
                await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + returnMsg, data.RecordID, appToken, data.TableID, accessToken);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }


    //AP INVOICE WITH APPLIED AP DP POSTING
    [HttpPost]
    [Route("PostAPInvoiceDP")]
    public async Task<IActionResult> PostAPInvoiceDP([FromBody] APInvoiceDPModel data)
    {

        string accessToken = "";
        string msg = "";
        string cardCode = "";
        string projectCode = "";

        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        try
        {

            var itemrecords = data.Details.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var apdprecords = data.APDPDetails.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var docLines = new List<DocumentLine>();
            var docDps = new List<DownPaymentsToDraw>();

            int lineNum = 0;
            var pCode = "";
            var pName = "";

            foreach (var record in itemrecords)
            {
                var line = new DocumentLine { LineNum = lineNum++ };
                var pairs = Regex.Matches(record, @"([^:,]+):\s*([^,]+)")
                                 .Cast<Match>();

                foreach (var match in pairs)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();

                    switch (key)
                    {
                        case "Vendor":
                            line.U_API_Vendor = value;
                            break;
                        case "TIN":
                            line.U_API_TIN = value;
                            break;
                        case "Address":
                            line.U_API_Address = value;
                            break;
                        case "G/L Account":
                            line.AccountCode = value;
                            break;
                        case "Particulars":
                            line.ItemDescription = value;
                            break;
                        case "Expense Name":
                            line.U_ExpenseType = value;
                            break;
                        case "Vehicle Plate No.":
                            line.U_PlateNo = value;
                            break;
                        case "Showroom":
                            line.CostingCode = value;
                            break;
                        case "Amount":
                            line.UnitPrice = value;
                            break;
                        case "Tax Code":
                            line.VatGroup = value;
                            break;
                    }
                }

                line.ProjectCode = data.Project; // sample static code
                docLines.Add(line);
            }

            foreach (var record in apdprecords)
            {
                var line = new DownPaymentsToDraw();
                var pairs = Regex.Matches(record, @"([^:,]+):\s*([^,]+)")
                                 .Cast<Match>();

                foreach (var match in pairs)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();

                    switch (key)
                    {
                        case "AP DP Entry":
                            line.DocEntry = value;
                            break;
                        case "Downpayment":
                            line.AmountToDraw = value;
                            break;
                        case "Project Code":
                            pCode = pCode == "" ? value : pCode;
                            break;
                        case "Project Name":
                            pName = pName == "" ? value : pName;
                            break;
                    }
                }

                docDps.Add(line);
            }

            var docWrapper = new DocumentWrapper
            {
                CardCode = data.CardCode,
                DocType = data.DocType,
                U_PurchType = data.U_PurchType,
                U_PrepBy = data.U_PrepBy,
                U_TrnsType = data.U_TransactionType,
                U_ProjectRelated = pCode != "" ? "Yes" : "No",
                Project = pCode,
                U_BPName = pName,
                U_ReferenceID = data.U_ReferenceID,
                U_RegName = "000-675-315-005",
                U_Remarks = "Posted through Lark Finance Approval Workflow" + ": " + DateTime.Now.ToString(),
                Comments = "Posted through Lark Finance Approval Workflow" + ": " + DateTime.Now.ToString(),
                DocumentLines = docLines,
                DownPaymentsToDraw = docDps
            };

            string json = JsonConvert.SerializeObject(docWrapper, Formatting.Indented);
            Console.WriteLine(json);

            msg = await _sapService.FINLogin();

            var returnMsg = "";
            var docNum = "";
            var docEntry = "";

            if (!msg.Contains("error"))
            {
                returnMsg = await _sapService.PostAPInvoice(docWrapper);
            }

            accessToken = await _larkService.GetAccessToken();

            if (!returnMsg.Contains("error"))
            {
                var docDeets = returnMsg.Split(",");

                if (docDeets.Count() > 1)
                {
                    docEntry = docDeets[0].ToString();
                    docNum = docDeets[1].ToString();
                    returnMsg = "Successfully posted to SAP" + ": " + DateTime.Now.ToString();
                }
                await _larkService.UpdateSAPInvoiceDetails(data.RecordID, appToken, data.TableID, accessToken, returnMsg, docEntry, docNum);
            }
            else
            {
                await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + returnMsg, data.RecordID, appToken, data.TableID, accessToken);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //SYNC BP VENDORS
    [HttpPost]
    [Route("SyncBPVendors")]
    public async Task<IActionResult> SyncBPVendors([FromBody] SAPSyncingModel data)
    {
        string accessToken = "";
        string msg = "";

        //GET BP VENDORS
        string getBPs = $"SELECT T0.CardCode, T0.CardName, T0.GroupCode, T0.CardType, T0.validFor, T0.frozenFor, T0.DpmClear FROM OCRD T0 WHERE T0.CardType = 'S' and T0.validFor = 'Y' and T0.frozenFor = 'N' and (T0.GroupCode != '119' or T0.GroupCode != '120')";

        var BPvendors = _sqlHelper.GetData(getBPs, _sqlHelper.GetConnection());

        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        accessToken = await _larkService.GetAccessToken();

        List<BPVendorModel> larkBPlist = await _larkService.GetLarkBPList(appToken, data.TableID, accessToken);

        try
        {
            List<SAPSyncingModel.BPVendor> bpVendorList = BPvendors.AsEnumerable()
                .Select(row => new SAPSyncingModel.BPVendor
                {
                    CardCode = row["CardCode"].ToString(),
                    CardName = row["CardName"].ToString(),
                    CardType = row["CardType"].ToString(),
                    GroupCode = row["GroupCode"].ToString(),
                    DpmClear = row["DpmClear"].ToString(),
                    validFor = row["validFor"].ToString(),
                })
                .ToList();

            if (larkBPlist != null)
            {
                var notInLarkList = bpVendorList
                .Where(bp => !larkBPlist.Any(lark => lark.fields.CardCode == bp.CardCode &&
                lark.fields.CardName == bp.CardName &&
                lark.fields.DpmClear == bp.DpmClear))
                .ToList();

                foreach (var bpv in notInLarkList)
                {
                    await _larkService.CreateRecord(appToken, data.TableID, accessToken, bpv);
                }

                var cardCodes = new HashSet<string>(bpVendorList.Select(v => v.CardCode));

                var toDelete = larkBPlist
                    .Where(lark => !cardCodes.Contains(lark.fields.CardCode))
                    .ToList();

                //ACTIVATED IN SAP BUT INACTIVE IN LARK
                var sapActivated = larkBPlist
                   .Where(et => bpVendorList.Any(lark => lark.CardCode == et.fields.CardCode &&
                   lark.CardName == et.fields.CardName && lark.validFor != et.fields.validFor))
                   .ToList();

                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in sapActivated)
                {
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "Y");
                }

                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in toDelete)
                {
                    //await _larkService.DeleteRecord(appToken, data.TableID, accessToken, bpv.record_id);
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "N");
                }
            }
            else
            {
                accessToken = await _larkService.GetAccessToken();

                foreach (var bpv in bpVendorList)
                {
                    await _larkService.CreateRecord(appToken, data.TableID, accessToken, bpv);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //SYNC BP VENDORS
    [HttpPost]
    [Route("SyncBPSubcon")]
    public async Task<IActionResult> SyncBPSubcon([FromBody] SAPSyncingModel data)
    {
        string accessToken = "";
        string msg = "";

        //GET BP VENDORS
        string getBPs = $"SELECT T0.CardCode, T0.CardName, T0.GroupCode, T0.CardType, T0.validFor, T0.frozenFor, T0.DpmClear FROM OCRD T0 INNER JOIN NNM1 T1 ON T0.Series = T1.Series Where T1.SeriesName = 'Subcon' and  T0.validFor = 'Y'";

        var BPvendors = _sqlHelper.GetData(getBPs, _sqlHelper.GetConnection());

        string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

        accessToken = await _larkService.GetAccessToken();

        List<BPVendorModel> larkBPlist = await _larkService.GetLarkBPList(appToken, data.TableID, accessToken);

        try
        {
            List<SAPSyncingModel.BPVendor> bpVendorList = BPvendors.AsEnumerable()
                .Select(row => new SAPSyncingModel.BPVendor
                {
                    CardCode = row["CardCode"].ToString(),
                    CardName = row["CardName"].ToString(),
                    CardType = row["CardType"].ToString(),
                    GroupCode = row["GroupCode"].ToString(),
                    DpmClear = row["DpmClear"].ToString(),
                    validFor = row["validFor"].ToString(),
                })
                .ToList();

            if (larkBPlist != null)
            {
                var notInLarkList = bpVendorList
                .Where(bp => !larkBPlist.Any(lark => lark.fields.CardCode == bp.CardCode &&
                lark.fields.CardName == bp.CardName &&
                lark.fields.DpmClear == bp.DpmClear))
                .ToList();

                foreach (var bpv in notInLarkList)
                {
                    await _larkService.CreateRecord(appToken, data.TableID, accessToken, bpv);
                }

                var cardCodes = new HashSet<string>(bpVendorList.Select(v => v.CardCode));

                var toDelete = larkBPlist
                    .Where(lark => !cardCodes.Contains(lark.fields.CardCode))
                    .ToList();

                //ACTIVATED IN SAP BUT INACTIVE IN LARK
                var sapActivated = larkBPlist
                   .Where(et => bpVendorList.Any(lark => lark.CardCode == et.fields.CardCode &&
                   lark.CardName == et.fields.CardName && lark.validFor != et.fields.validFor))
                   .ToList();

                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in sapActivated)
                {
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "Y");
                }

                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in toDelete)
                {
                    //await _larkService.DeleteRecord(appToken, data.TableID, accessToken, bpv.record_id);
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "N");
                }
            }
            else
            {
                accessToken = await _larkService.GetAccessToken();

                foreach (var bpv in bpVendorList)
                {
                    await _larkService.CreateRecord(appToken, data.TableID, accessToken, bpv);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //SYNC EXPENSE TYPES
    [HttpPost]
    [Route("SyncExpenseTypes")]
    public async Task<IActionResult> SyncExpenseTypes([FromBody] SAPSyncingModel data)
    {
        string accessToken = "";
        string msg = "";

        //GET BP VENDORS
        string getExpenseTypes = $"SELECT T0.ExpType, T0.U_GL_Name, T0.ExpAcct FROM OEXT T0 WHERE T0.U_Lark = 'Y'";

        var expenseTypes = _sqlHelper.GetData(getExpenseTypes, _sqlHelper.GetConnection());

        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        accessToken = await _larkService.GetAccessToken();

        List<ExpenseTypeModel> larkETlist = await _larkService.GetLarkETList(appToken, data.TableID, accessToken);

        try
        {
            List<SAPSyncingModel.ExpenseType> expenseTypeList = expenseTypes.AsEnumerable()
                .Select(row => new SAPSyncingModel.ExpenseType
                {
                    ExpType = row["ExpType"].ToString(),
                    U_GL_Name = row["U_GL_Name"].ToString(),
                    ExpAcct = row["ExpAcct"].ToString()
                })
                .ToList();

            if (larkETlist != null)
            {
                var notInLarkList = expenseTypeList
                    .Where(et => !larkETlist.Any(lark => lark.fields.ExpType == et.ExpType &&
                    lark.fields.U_GL_Name == et.U_GL_Name &&
                    lark.fields.ExpAcct == et.ExpAcct))
                    .ToList();

                foreach (var bpv in notInLarkList)
                {
                    await _larkService.CreateETRecord(appToken, data.TableID, accessToken, bpv);
                }

                var types = new HashSet<string>(expenseTypeList.Select(v => v.ExpType));

                var toDelete = larkETlist
                    .Where(lark => !types.Contains(lark.fields.ExpType))
                    .ToList();

                accessToken = await _larkService.GetAccessToken();

                foreach (var bpv in toDelete)
                {
                    //await _larkService.DeleteRecord(appToken, data.TableID, accessToken, bpv.record_id);
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "N");
                }
            }
            else
            {
                accessToken = await _larkService.GetAccessToken();

                foreach (var bpv in expenseTypeList)
                {
                    await _larkService.CreateETRecord(appToken, data.TableID, accessToken, bpv);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //SYNC TRANSACTION TYPES
    [HttpPost]
    [Route("SyncTransTypes")]
    public async Task<IActionResult> SyncTransTypes([FromBody] SAPSyncingModel data)
    {
        string accessToken = "";
        string msg = "";

        //GET TRANSACTION TYPES
        string getTransTypes = $"SELECT T0.Code, T0.Name FROM \"@TRANSTYPE\" T0 WHERE T0.U_Lark = 'Y'";

        var transTypes = _sqlHelper.GetData(getTransTypes, _sqlHelper.GetConnection());

        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        accessToken = await _larkService.GetAccessToken();

        List<TransactionTypeModel> larkTTlist = await _larkService.GetLarkTTList(appToken, data.TableID, accessToken);

        try
        {
            List<SAPSyncingModel.TransType> transTypeList = transTypes.AsEnumerable()
                .Select(row => new SAPSyncingModel.TransType
                {
                    Code = row["Code"].ToString(),
                    Name = row["Name"].ToString()
                })
                .ToList();

            if (larkTTlist != null)
            {
                var notInLarkList = transTypeList
                    .Where(et => !larkTTlist.Any(lark => lark.fields.Code == et.Code &&
                    lark.fields.Name == et.Name))
                    .ToList();

                foreach (var bpv in notInLarkList)
                {
                    await _larkService.CreateTTRecord(appToken, data.TableID, accessToken, bpv);
                }

                var types = new HashSet<string>(transTypeList.Select(v => v.Code));

                var toDelete = larkTTlist
                    .Where(lark => !types.Contains(lark.fields.Code))
                    .ToList();

                accessToken = await _larkService.GetAccessToken();

                foreach (var bpv in toDelete)
                {
                    //await _larkService.DeleteRecord(appToken, data.TableID, accessToken, bpv.record_id);
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "N");
                }
            }
            else
            {
                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in transTypeList)
                {
                    await _larkService.CreateTTRecord(appToken, data.TableID, accessToken, bpv);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //SYNC SHOWROOMS
    [HttpPost]
    [Route("SyncShowroom")]
    public async Task<IActionResult> SyncShowroom([FromBody] SAPSyncingModel data)
    {
        string accessToken = "";
        string msg = "";

        //GET BP VENDORS
        string getShowrooms = $"SELECT T0.PrcCode, T0.PrcName,T0.Active FROM OPRC T0 WHERE T0.CCTypeCode = 'Showroom' and T0.Active = 'Y'";

        var showrooms = _sqlHelper.GetData(getShowrooms, _sqlHelper.GetConnection());

        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        accessToken = await _larkService.GetAccessToken();

        List<ShowroomModel> larkSRlist = await _larkService.GetLarkSRList(appToken, data.TableID, accessToken);

        try
        {
            List<SAPSyncingModel.Showroom> showroomList = showrooms.AsEnumerable()
                .Select(row => new SAPSyncingModel.Showroom
                {
                    PrcCode = row["PrcCode"].ToString(),
                    PrcName = row["PrcName"].ToString(),
                    Active = row["Active"].ToString()
                })
                .ToList();

            if (larkSRlist != null)
            {
                var notInLarkList = showroomList
               .Where(et => !larkSRlist.Any(lark => lark.fields.PrcCode == et.PrcCode &&
               lark.fields.PrcName == et.PrcName))
               .ToList();

                foreach (var bpv in notInLarkList)
                {
                    await _larkService.CreateSRRecord(appToken, data.TableID, accessToken, bpv);
                }

                var types = new HashSet<string>(showroomList.Select(v => v.PrcCode));

                var toDelete = larkSRlist
                    .Where(lark => !types.Contains(lark.fields.PrcCode))
                    .ToList();

                //ACTIVATED IN SAP BUT INACTIVE IN LARK
                var sapActivated = larkSRlist
                   .Where(et => showroomList.Any(lark => lark.PrcCode == et.fields.PrcCode &&
                   lark.PrcCode == et.fields.PrcCode && lark.Active != et.fields.Active))
                   .ToList();

                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in sapActivated)
                {
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "Y");
                }

                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in toDelete)
                {
                    //await _larkService.DeleteRecord(appToken, data.TableID, accessToken, bpv.record_id);
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "N");
                }
            }
            else
            {
                foreach (var bpv in showroomList)
                {
                    await _larkService.CreateSRRecord(appToken, data.TableID, accessToken, bpv);
                }
            }
            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //SYNC VEHICLES
    [HttpPost]
    [Route("SyncVehicle")]
    public async Task<IActionResult> SyncVehicle([FromBody] SAPSyncingModel data)
    {
        string accessToken = "";
        string msg = "";

        //GET VEHICLES
        string getVehicles = $"SELECT T0.Code, T0.Name, T0.U_Description, T0.U_Active FROM \"@VEHICLES\" T0 WHERE T0.[U_Active] = 'Y'";

        var vehicles = _sqlHelper.GetData(getVehicles, _sqlHelper.GetConnection());

        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        accessToken = await _larkService.GetAccessToken();

        List<VehicleModel> larkVlist = await _larkService.GetLarkVList(appToken, data.TableID, accessToken);

        try
        {
            List<SAPSyncingModel.Vehicle> vehicleList = vehicles.AsEnumerable()
                .Select(row => new SAPSyncingModel.Vehicle
                {
                    Code = row["Code"].ToString(),
                    Name = row["Name"].ToString(),
                    U_Description = row["U_Description"].ToString(),
                    U_Active = row["U_Active"].ToString()
                })
                .ToList();

            if (larkVlist != null)
            {
                //SAP EXISTING BUT NOT IN LARK
                var notInLarkList = vehicleList
                    .Where(et => !larkVlist.Any(lark => lark.fields.Code == et.Code &&
                    lark.fields.Name == et.Name))
                    .ToList();

                foreach (var bpv in notInLarkList)
                {
                    await _larkService.CreateVRecord(appToken, data.TableID, accessToken, bpv);
                }

                //ACTIVATED IN SAP BUT INACTIVE IN LARK

                var sapActivated = larkVlist
                   .Where(et => vehicleList.Any(lark => lark.Code == et.fields.Code &&
                   lark.Name == et.fields.Name && lark.U_Active != et.fields.U_Active))
                   .ToList();

                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in sapActivated)
                {
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "Y");
                }

                //LARK EXISTING BUT NOT IS SAP

                var types = new HashSet<string>(vehicleList.Select(v => v.Code));

                var toDelete = larkVlist
                    .Where(lark => !types.Contains(lark.fields.Code))
                    .ToList();

                accessToken = await _larkService.GetAccessToken();
                foreach (var bpv in toDelete)
                {
                    //await _larkService.DeleteRecord(appToken, data.TableID, accessToken, bpv.record_id);
                    await _larkService.UpdateRecord(appToken, data.TableID, accessToken, bpv.record_id, "N");
                }
            }
            else
            {
                foreach (var bpv in vehicleList)
                {
                    await _larkService.CreateVRecord(appToken, data.TableID, accessToken, bpv);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //SYNC AP DP
    [HttpPost]
    [Route("SyncRecord")]
    public async Task<IActionResult> SyncRecord([FromBody] SAPSyncingModel data)
    {
        string accessToken = "";
        string msg = "";
        string sapModule = !data.TransactionType.Contains("Liquidation") ? "OPCH" : "ODPO";
        sapModule = data.TransactionType.Contains("Advance") ? "ODPO" : sapModule;

        //GET RECORD
        string getRecordDetails = $"SELECT T0.DocEntry, T0.DocNum FROM {sapModule} T0 WHERE T0.U_ReferenceID = '{data.U_LarkReferenceID}'";

        var record = _sqlHelper.GetData(getRecordDetails, _sqlHelper.GetConnection());

        var apdpBal = "";

        //GET AP DP BALANCE
        if (sapModule == "ODPO")
        {
            //string getAPDPBalance = $"SELECT \r\nT0.[DocNum], \r\nT0.[CardCode], \r\nT1.[BaseAbs], \r\nSUM(T2.DocTotal) - SUM(T1.[DrawnSum]) as \"Balance\"\r\nFROM OPCH T0  \r\nINNER JOIN PCH9 T1 ON T0.[DocEntry] = T1.[DocEntry]\r\nINNER JOIN ODPO T2 ON T2.DocEntry = T1.BaseAbs\r\nWHERE T1.[BaseAbs] = {record.Rows[0]["DocEntry"]} \r\nGROUP BY\r\nT0.[DocNum], \r\nT0.[CardCode], \r\nT1.[BaseAbs]";

            //var balance = _sqlHelper.GetData(getAPDPBalance, _sqlHelper.GetConnection());

            //apdpBal = Convert.ToDecimal(balance.Rows[0]["Balance"]).ToString("F2");
        }


        string appToken = "D4g8b0Fdpanr1msoCTslJ2IkgUg"; //_larkSettings.AppToken;

        try
        {
            if (record != null)
            {
                accessToken = await _larkService.GetAccessToken();

                if (sapModule == "ODPO")
                {
                    await _larkService.UpdateSAPDPDetails(data.RecordID, appToken, data.TableID, accessToken, "", record.Rows[0]["DocEntry"].ToString(), record.Rows[0]["DocNum"].ToString(), apdpBal);
                }
                else
                {
                    await _larkService.UpdateSAPInvoiceDetails(data.RecordID, appToken, data.TableID, accessToken, "", record.Rows[0]["DocEntry"].ToString(), record.Rows[0]["DocNum"].ToString());
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            accessToken = await _larkService.GetAccessToken();
            await _larkService.UpdateRecordError("ERROR: " + DateTime.Now.ToString() + Environment.NewLine + ex.Message, data.RecordID, appToken, data.TableID, accessToken);
            return BadRequest(ex.Message);
        }
    }

    //PRINT PROJCT REGISTRATION
    //[HttpPost]
    //[Route("PrintProjReg")]
    //public async Task<IActionResult> Post([FromBody] PDSFieldsModel data)
    //{

    //    string accessToken = "";
    //    string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

    //    try
    //    {
    //        accessToken = await _larkService.GetAccessToken();
    //        RecordModel record = await _larkService.GetRecord(appToken, data.TableId, data.RecordId, accessToken);
    //        string certificateFileName = await _larkService.GeneratePGF(record.fields, data.SiteLocation, data.ProjectName, data.DateofRegistration, data.Approver, data.Sales, data.Screener);
    //        Log.Information("PRD Generated");

    //        accessToken = await _larkService.GetAccessToken();
    //        string fileToken = await _larkService.UploadFile(certificateFileName, appToken, accessToken);

    //        Log.Information("File Uploaded");
    //        await _larkService.UpdateFile(fileToken, data.RecordId, appToken, data.TableId, accessToken);
    //        Log.Information("Record Updated");
    //        return Ok();
    //    }
    //    catch (Exception ex)
    //    {
    //        await _larkService.UpdateRecordError(ex.Message, data.RecordId, appToken, data.TableId, accessToken);
    //        return BadRequest(ex.Message);
    //    }
    //}

    //PRINT PDS
    //[HttpPost]
    //[Route("PrintPDS")]
    //public async Task<IActionResult> PostPDS([FromBody] PDSFieldsModel data)
    //{

    //    string accessToken = "";
    //    string appToken = "EumgbZGkyaX6cmsv8KOllAPSglg"; //_larkSettings.AppToken;

    //    try
    //    {
    //        accessToken = await _larkService.GetAccessToken();
    //        RecordModel record = await _larkService.GetRecord(appToken, data.TableId, data.RecordId, accessToken);
    //        string certificateFileName = await _larkService.GeneratePDS(record.fields);
    //        Log.Information("PDS Generated");

    //        accessToken = await _larkService.GetAccessToken();
    //        string fileToken = await _larkService.UploadFile(certificateFileName, appToken, accessToken);

    //        Log.Information("File Uploaded");
    //        await _larkService.UpdateRecord(fileToken, data.RecordId, appToken, data.TableId, accessToken);
    //        Log.Information("Record Updated");
    //        return Ok();
    //    }
    //    catch (Exception ex)
    //    {
    //        await _larkService.UpdateRecordError(ex.Message, data.RecordId, appToken, data.TableId, accessToken);
    //        return BadRequest(ex.Message);
    //    }
    //}

    //Test Login
    [HttpGet]
    [Route("TestLogin")]
    public async Task<IActionResult> TestLogin()
    {
        var resp = await _sapService.Login();

        if (!resp.Contains("error"))
        {
            return BadRequest(resp);
        }

        return Ok();
    }

    //SQL Quert Check
    [HttpGet]
    [Route("TestQuery")]
    public async Task<IActionResult> TestQuery()
    {
        var resp = await _sapService.Login();

        if (resp.Contains("error"))
        {
            return BadRequest(resp);
        }

        string checkTINifExisting = $"select \"CardCode\", \"CardName\", \"validFor\" from OCRD where CAST(\"AliasName\" as NVARCHAR(max)) = 'Metropolitan Waterworks Sewerage System (MWSS)'";
        var bpCode = _sqlHelper.GetData(checkTINifExisting, _sqlHelper.GetConnection());

        if (bpCode.Columns.Count == 0 || bpCode == null)
        {
            return BadRequest("Bad Query");
        }

        return Ok();
    }

    [HttpGet]
    [Route("TestGetBP")]
    public async Task<IActionResult> TestGetBP()
    {

        var resp = await _sapService.Login();

        //_larkService.LogError(resp);

        if (resp.Contains("error"))
            return BadRequest(resp);

        var getBPs = await _sapService.GetBP();

        if (getBPs.Contains("error"))
            return BadRequest(getBPs);

        return Ok(getBPs);
    }
}