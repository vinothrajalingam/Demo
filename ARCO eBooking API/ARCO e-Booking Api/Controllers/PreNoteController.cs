using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using ARCO.EndPoint.eCard.VM;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using ARCO.EndPoint.Model;

namespace ARCO.EndPoint.eCard.Controllers
{
    
    [RoutePrefix("api/PreNote")]
    public class PreNoteController : ApiController
    {

        private static readonly ILog Log =
             LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
        public PreNoteController()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        [HttpGet]
        [Route("LoadPreNoteList")]
        public IHttpActionResult LoadPreNoteList()
        {

            try
            {
                var PreNoteSystemsList = (from note in arcodb.PreNoteSystems
                                          join cus in arcodb.Customers
                                          on note.CustomerNumber equals cus.CustomerNumber
                                          join site in arcodb.SiteLocations
                                          on note.SiteLocationId equals site.ID
                                          where note.Status == true && site.Status != false
                                          select new
                                          {
                                              ID = note.ID,
                                              CustomerNumber = note.CustomerNumber + "-" + cus.CompanyName,
                                              Body = note.Body,
                                              Notify = note.Notify,
                                              SiteLocation = note.SiteLocation.SiteLocation1,
                                              Status = note.Status,
                                              Subject = note.Subject,
                                              CreatedDate = note.CreatedDate,
                                              ModifiedDate = note.ModifiedDate

                                          }).ToList();
                return Content(HttpStatusCode.OK, PreNoteSystemsList);

            }
            catch (Exception ex)
            {
                Log.Info(String.Format("Request received from:{0}/{1}",
                                   ex.Message,
                                   ex.StackTrace));
                return Content(HttpStatusCode.InternalServerError, "There was an error while retrieveing prenote list.");
            }
        }

        [HttpPost]
        [Route("LoadFilteredPreNoteList")]
        public IHttpActionResult LoadFilteredPreNoteList([FromBody] FilteredPreNoteListVM filter)
        {
            if (filter == null || !ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, "Request body not valid");
            }
            int siteid = Convert.ToInt32(filter.Site);
            //SiteLocation sitelocation = arcodb.SiteLocations.Where(s => s.ID == siteid).FirstOrDefault();

            List<FilteredPreNoteListVM> PreNotes = (from note in arcodb.PreNoteSystems
                                                    join cus in arcodb.Customers
                                                    on note.CustomerNumber equals cus.CustomerNumber
                                                    join site in arcodb.SiteLocations
                                                    on note.SiteLocationId equals site.ID
                                                    join sale in arcodb.SalesOrders on note.ID equals sale.PreNoteId
                                                    into salesorders from sales in salesorders.DefaultIfEmpty()
                                                    where
                                                        ((note.Deleted.HasValue && note.Deleted.Value==false) || note.Deleted.HasValue==false) &&
                                                        (filter.Status==null || note.Status == filter.Status) &&
                                                        (String.IsNullOrEmpty(filter.CustomerName) || (cus.CompanyName.Contains(filter.CustomerName)||
                                                        cus.CustomerNumber.Equals(filter.CustomerNumber))) &&
                                                        (filter.Site == null || note.SiteLocationId == siteid) &&
                                                        (filter.Subject == null || note.Subject == filter.Subject)
                                                        && (site.Status != false)
                                                    //(filter.StartDate == null || note.CreatedDate.Date >= filter.StartDate.Value.Date) &&
                                                    //(filter.EndDate == null || note.CreatedDate.Date <= filter.EndDate.Value.Date)
                                                    select new FilteredPreNoteListVM
                                                    {
                                                        PreNoteId = note.ID,
                                                        CustomerNumber = note.CustomerNumber + "-" + cus.CompanyName,
                                                        Subject = note.Subject,
                                                        Site = note.SiteLocation.SiteLocation1,
                                                        StartDate = note.CreatedDate,
                                                        EndDate = note.CreatedDate,
                                                        LastUpdatedDate = note.ModifiedDate,
                                                        Status = note.Status,
                                                        Deleted =note.Deleted,
                                                        SaleOrderNumber= sales==null?String.Empty:sales.SalesOrderNumber,
                                                        CityNState=cus.City+ ","+cus.State
                                                    })
                                                    .ToList()
                                                    .Where(a => filter.StartDate == null || a.StartDate.Value.Date >= filter.StartDate.Value.Date)
                                                    .Where(a => filter.LastUpdatedDate == null || a.EndDate.Value.Date <= filter.LastUpdatedDate.Value.Date)
                                                    .ToList();



            // Filter the list according to the input filter
            try
            {
                #region Filter

                //if (filter.Status != false)
                //{
                //    PreNotes = PreNotes.Where(i=> i.Status == false).ToList();
                //}

                //if (!String.IsNullOrWhiteSpace(filter.CustomerNumber))
                //{
                //    Customer customer = arcodb.Customers.Where(c => c.CustomerNumber == filter.CustomerNumber).FirstOrDefault();
                //    string Companyname = customer.CustomerNumber + "-" + customer.CompanyName;
                //    PreNotes = PreNotes.Where(i => i.CustomerNumber.Contains(Companyname) && i.Status == true).ToList();
                //}

                //if (filter.Site != null)
                //{
                //    int siteid = Convert.ToInt32(filter.Site);
                //    SiteLocation sitelocation = arcodb.SiteLocations.Where(s => s.ID == siteid).FirstOrDefault();
                //    PreNotes = PreNotes.Where(i => i.Site.Contains(sitelocation.SiteLocation1) && i.Status == true).ToList();

                //}

                //if (filter.Subject != null)
                //{
                //    PreNotes = PreNotes.Where(i => i.Subject.Contains(filter.Subject) && i.Status == true).ToList();
                //}

                //if (filter.StartDate != null)
                //{
                //    PreNotes = PreNotes.Where(i => i.StartDate.Value.Date >= filter.StartDate.Value.Date && i.Status == true).ToList();
                //}

                //if (filter.EndDate != null)
                //{
                //    PreNotes = PreNotes.Where(i => i.EndDate.Value.Date <= filter.EndDate.Value.Date && i.Status == true).ToList();
                //}



                #endregion

                #region PreNote Sorting

                if (String.IsNullOrWhiteSpace(filter.Sorting))
                {
                    PreNotes = PreNotes.OrderByDescending(i => i.LastUpdatedDate).ToList();
                }
                else if (filter.Direction == "desc")
                {
                    switch (filter.Sorting)
                    {
                        case "customerNumber":
                            PreNotes = PreNotes.OrderByDescending(i => i.CustomerNumber).ToList();
                            break;
                        case "site":
                            PreNotes = PreNotes.OrderByDescending(i => i.Site).ToList();
                            break;
                        case "subject":
                            PreNotes = PreNotes.OrderByDescending(i => i.Subject).ToList();
                            break;
                        case "datecreated":
                            PreNotes = PreNotes.OrderByDescending(i => i.StartDate).ToList();
                            break;
                        case "citynstate":
                            PreNotes = PreNotes.OrderByDescending(i => i.CityNState).ToList();
                            break;
                        case "lastupdated":
                            PreNotes = PreNotes.OrderByDescending(i => i.LastUpdatedDate).ToList();
                            break;
                        default:
                            PreNotes = PreNotes.OrderByDescending(i => i.PreNoteId).ToList();
                            break;
                    }
                }
                else
                {
                    switch (filter.Sorting)
                    {
                        case "customerNumber":
                            PreNotes = PreNotes.OrderBy(i => i.CustomerNumber).ToList();
                            break;
                        case "site":
                            PreNotes = PreNotes.OrderBy(i => i.Site).ToList();
                            break;
                        case "subject":
                            PreNotes = PreNotes.OrderBy(i => i.Subject).ToList();
                            break;
                        case "datecreated":
                            PreNotes = PreNotes.OrderBy(i => i.StartDate).ToList();
                            break;
                        case "lastupdated":
                            PreNotes = PreNotes.OrderBy(i => i.LastUpdatedDate).ToList();
                            break;
                        case "citynstate":
                            PreNotes = PreNotes.OrderByDescending(i => i.CityNState).ToList();
                            break;
                        default:
                            PreNotes = PreNotes.OrderBy(i => i.PreNoteId).ToList();
                            break;
                    }
                }

                #endregion

                int PreNotesCount = PreNotes.Count();
                int pages = PreNotesCount / filter.PageSize + ((PreNotesCount % filter.PageSize > 0) ? 1 : 0);

                FilteredPreNoteListReturnVM result = new FilteredPreNoteListReturnVM();
                result.Pages = pages;
                result.PreNotes = PreNotes.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

                return Content(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of PreNotes.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of PreNotes.");
            }

        }


        [HttpGet]
        [Route("GetCustomerList")]
        public IHttpActionResult GetCustomerList()
        {

            try
            {

                List<CustomerList> CustomerList = arcodb.Database.SqlQuery<CustomerList>("Select a.CustomerNumber, a.CompanyName, a.ID from dbo.Customer as a ORDER BY a.ID").ToList();

                //CustomerList.OrderBy(a => a.ID);

                //var CustomerList1 = (from cus in arcodb.Customers
                //                    select new
                //                    {
                //                        CustomerNUmber = cus.CustomerNumber.Trim(),
                //                        CompanyNameAndNumber = cus.CompanyName
                //                    }).ToList();


                //CustomerList.OrderBy(a => a.Company);
                return Content(HttpStatusCode.OK, CustomerList);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while getting list of customers for prenote system", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of customers for prenote system");
            }
        }

        [HttpGet]
        [Route("GetSiteLocations")]
        public IHttpActionResult GetSiteLocations()
        {

            try
            {
                var Sitelocations = (from site in arcodb.SiteLocations
                                     where site.Status != false
                                     select new
                                     {
                                         SiteID = site.ID,
                                         Site = site.SiteLocation1,
                                         SiteNumber=site.SiteNumber
                                     }).OrderBy(a => a.SiteNumber).ToList();

                return Content(HttpStatusCode.OK, Sitelocations);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while getting list of site locations for prenote system", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of site locations for prenote system");
            }
        }

        [HttpGet]
        [Route("GetPreNotesforUWP")]
        public IHttpActionResult GetPreNotesforUWP()
        {

            try
            {
                Log.Info(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName));
                var Prenotes = (from note in arcodb.PreNoteSystems
                                join site in arcodb.SiteLocations on note.SiteLocationId equals site.ID
                                join cus in arcodb.Customers on note.CustomerNumber equals cus.CustomerNumber
                                where note.Status == true && ((note.Deleted.HasValue && note.Deleted.Value == false) || note.Deleted.HasValue == false) && site.Status != false
                                select new
                                {
                                    PreNoteId = note.ID,
                                    SiteID = site.ID,
                                    Site = site.SiteLocation1,
                                    CustomerNumber = cus.CustomerNumber.Trim(),
                                    CustomerName = cus.CustomerNumber.Trim() + "-" + cus.CompanyName,
                                    CreatedDate = note.CreatedDate,
                                    CityNState = cus.ShipToCity.Trim() + "-" + cus.ShipToState.Trim(),
                                    Subject = note.Subject,
                                }).ToList();
                return Content(HttpStatusCode.OK, Prenotes);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Log : {0}/{1}", "There was an error while getting list of  prenotes for uwp", ex));
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of  prenotes for uwp");
            }
        }

        //[HttpGet]
        //[Route("CheckPrenoteMapped")]
        //public bool CheckPrenoteMapped(int CheckPrenoteId)
        //{

        //    try
        //    {
        //        Boolean status= arcodb.SalesOrders.Where(a => a.PreNoteId == CheckPrenoteId).Count()>0;
        //        return status;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("There was an error while getting list of sales order numbers for prenote system", ex);
        //        return false;
        //    }
        //}


        [HttpGet]
        [Route("DeletePreNote")]
        public IHttpActionResult DeletePreNote(int PreNoteID)
        {
            try
            {
                //Boolean status = arcodb.PreNoteSystems.Where(a => a.ID == PreNoteID).Count() > 0;
                //if(status)
                //{
                //    return Content(HttpStatusCode.BadRequest, "This prenote cannot be deleted as this is already mapped to an existing order.");
                //}
                //else
                //{
                    PreNoteSystem prenote = arcodb.PreNoteSystems.Where(fil => fil.ID == PreNoteID).FirstOrDefault();
                    prenote.Deleted = true;
                    arcodb.SaveChanges();
                    return Content(HttpStatusCode.OK, "Prenote deleted successfully.");
              //  }
            }
            catch (Exception ex)
            {
                Log.Error("There was an error when deleting this prenote.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error when deleting this prenote.");
            }
        }

        [HttpGet]
        [Route("DeleteAttachment")]
        public IHttpActionResult DeleteAttachment(int ID)
        {
            try
            {
                Attachment attachprenote = arcodb.Attachments.Where(fil => fil.ID == ID).FirstOrDefault();
                arcodb.Attachments.Remove(attachprenote);
                arcodb.SaveChanges();
                return Content(HttpStatusCode.OK, "Deleted Successfully");
                
            }
            catch (Exception ex)
            {
                Log.Error("There was an error when deleting this prenote.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error when deleting this prenote.");
            }
        }
        

        [HttpGet]
        [Route("GetSalesOrderNumbers")]
        public IHttpActionResult GetSalesOrderNumbers()
        {

            try
            {
                var salesordernumbers = (from sales in arcodb.SalesOrders
                                         select new
                                         {
                                             ID = sales.ID,
                                             SalesOrderNumber = sales.SalesOrderNumber
                                         }).ToList();

                return Content(HttpStatusCode.OK, salesordernumbers);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while getting list of sales order numbers for prenote system", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of sales order numbers for prenote system");
            }
        }

        [HttpGet]
        [Route("LoadPreNoteforUWP")]
        public IHttpActionResult LoadPreNoteforUWP(int PreNoteId)
        {

            try
            {
                var PreNote = (from prenote in arcodb.PreNoteSystems
                               where prenote.ID == PreNoteId
                               select new
                               {
                                   PreNoteId = prenote.ID,
                                   CustomerNumber = prenote.CustomerNumber,
                                   Site = prenote.SiteLocation.SiteLocation1,
                                   SalesOrderNumber = prenote.SalesOrderId,
                                   Subject = prenote.Subject,
                                   Body = prenote.Body,
                                   ShipToAddress=prenote.ShipToAddress,
                                   AttachmentDetail = (from att in prenote.Attachments
                                                       select new
                                                       {
                                                           ID = att.ID,
                                                           FileName = att.FileName,
                                                           Type = att.Type
                                                       })
                               }).FirstOrDefault();


                return Content(HttpStatusCode.OK, PreNote);

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while loading a prenote for uwp", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while loading a prenote for uwp");
            }


        }


        [HttpGet]
        [Route("CustomerAddressforUWP")]
        public IHttpActionResult CustomerAddressforUWP(String CustomerNumber)
        {

            try
            {
                var CustomerShipToAddress = (from CustomerShipToAddr in arcodb.CusAddresses
                               where CustomerShipToAddr.CustomerNumber.Trim() == CustomerNumber
                               select new
                               {
                                   CusAddr1 = CustomerShipToAddr.CustAddr1,
                                   CusAddr2 = CustomerShipToAddr.CustAddr2,
                                   CusAddr3 = CustomerShipToAddr.CustAddr3,
                                   CusAddr4 = CustomerShipToAddr.CustAddr4,
                                   CusShipToNameANDStreetAddresses= CustomerShipToAddr.CustAddr1.Trim() +", "+ CustomerShipToAddr.CustAddr2.Trim() + ", "
                                                                    + CustomerShipToAddr.CustAddr3.Trim() + ", "
                                                                    + CustomerShipToAddr.CustAddr4.Trim()

                               }).ToList();


                return Content(HttpStatusCode.OK, CustomerShipToAddress);

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while loading a prenote for uwp", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while loading a prenote for uwp");
            }


        }



        [HttpPost]
        [Route("CreatePreNote")]
        public IHttpActionResult CreatePreNote([FromBody] NewPreNoteVM PreNoteVM)
        {
            if (PreNoteVM.CustomerNumber == null || PreNoteVM.SiteLocationId==null || PreNoteVM.Subject==null || PreNoteVM.Notify==null)
            {
                Log.Error("Missing some mandatory fields.");
                return Content(HttpStatusCode.BadRequest, "Missing some mandatory fields.");
            }

            try
            {
                PreNoteSystem PreNote = new PreNoteSystem();
                PreNote.Body = PreNoteVM.Body;
                PreNote.CreatedDate = DateTime.Now;
                PreNote.CustomerNumber = PreNoteVM.CustomerNumber;
                PreNote.ModifiedDate = DateTime.Now;
                PreNote.Notify = PreNoteVM.Notify;
                //PreNote.SalesOrderId = PreNoteVM.SalesOrderId;
                PreNote.SiteLocationId = PreNoteVM.SiteLocationId;
                PreNote.Status = true;
                PreNote.ShipToAddress = PreNoteVM.ShipToAddress;
                PreNote.Subject = PreNoteVM.Subject;


                //arcodb.SaveChanges();
                if (PreNoteVM.Attachments != null)
                {
                    foreach (Attachment A in PreNoteVM.Attachments)
                    {
                        Attachment file = new Attachment();

                        //String ext = Path.GetExtension(A.FileName);

                        file.CreatedDate = DateTime.Now;
                        file.FileName = A.FileName;
                        file.ModifiedDate = DateTime.Now;
                        file.PreNoteId = PreNote.ID;
                        file.Type = Path.GetExtension(A.FileName).Replace(".", String.Empty);
                        file.ReferenceName = A.ReferenceName;
                        file.FilePath = A.FilePath;
                        PreNote.Attachments.Add(file);
                    }
                }
                arcodb.PreNoteSystems.Add(PreNote);
                arcodb.Attachments.AddRange(PreNote.Attachments);

                String customername = arcodb.Customers.Where(fil => fil.CustomerNumber == PreNote.CustomerNumber).SingleOrDefault().CompanyName;
                String sitelocation = arcodb.SiteLocations.Where(fil => fil.ID == PreNote.SiteLocationId).SingleOrDefault().SiteLocation1;
                SmtpClient _emailclient = new SmtpClient();
                MailMessage _message = new MailMessage();
                _message.From = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"].ToString());
                List<String> _toaddresses = new List<String>(PreNote.Notify.Split(';'));
                _toaddresses.ForEach(_toaddress =>
                {
                    _message.To.Add(new MailAddress(_toaddress));
                });
                _message.Subject = PreNote.CustomerNumber + "-" + customername + ": " + sitelocation + ": " + PreNote.Subject;
                _message.Body = "New Prenote:" + Environment.NewLine + PreNote.Body;
                _emailclient.Send(_message);
                Log.Info("Mail Sent.");
                arcodb.SaveChanges();
                return Content(HttpStatusCode.OK, PreNote);
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                    Log.Error(string.Format("Err @ PrenoteSystem: Method:{0}, Entity Type: {1}, Entity State: {2}", "CreatePrenote", eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Log.Error(string.Format("Err @ PrenoteSystem: Method:{0}, {1}, {2}", "CreatePrenote", ve.PropertyName, ve.ErrorMessage));
                    }
                }
                return Content(HttpStatusCode.BadRequest, "There was an error while creating a prenote");

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while creating a prenote", ex);
                return Content(HttpStatusCode.BadRequest, "There was an error while creating a prenote");
            }
            
            }

        [HttpPost]
        [Route("UploadAttachment")]
        public IHttpActionResult UploadAttachment()
        {
            List<Attachment> returndetail = new List<Attachment>();

            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        String _Folder = String.Format(@"{0}",
                                                              ConfigurationManager.AppSettings["AttachmentsPath"].ToString());
                        if (!Directory.Exists(_Folder))
                        {
                            Directory.CreateDirectory(_Folder);
                        }
                        Guid NewFileName = Guid.NewGuid();
                        //String Type = postedFile.ContentType.Split('/').Last();

                        var filePath = String.Format(@"{0}\{1}", _Folder, NewFileName + "." + Path.GetExtension(postedFile.FileName).Replace(".", String.Empty));
                        postedFile.SaveAs(filePath);
                        Attachment result = new Attachment();
                        result.FileName = postedFile.FileName;
                        result.ReferenceName = NewFileName;
                        result.Type = Path.GetExtension(postedFile.FileName).Replace(".", String.Empty);
                        result.FilePath = NewFileName + "." + Path.GetExtension(postedFile.FileName).Replace(".", String.Empty); ;
                        returndetail.Add(result);
                    }
                }
                return Content(HttpStatusCode.OK, returndetail);

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while adding an attachment", ex);
                return Content(HttpStatusCode.BadRequest, "There was an error while adding an attachment");
            }
        }

        [HttpPost]
        [Route("RemoveAttachment")]
        public HttpResponseMessage RemoveAttachment()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        String _Folder = String.Format(@"{0}\{1}",
                                                              ConfigurationManager.AppSettings["AttachmentsPath"].ToString(),
                                                              DateTime.Now.Year.ToString());

                        var filePath = String.Format(@"{0}\{1}", _Folder, postedFile.FileName);

                        var uri = new Uri(filePath, UriKind.Absolute);
                        System.IO.File.Delete(uri.LocalPath);
                    }
                }
                response.Content = new StringContent("Attachment removed", System.Text.Encoding.UTF8, "text/plain");
                response = new HttpResponseMessage(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while deleting an attachment", ex);
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/plain");
                return response;
            }
        }

        [HttpGet]
        [Route("RemoveSingleAttachment")]
        public HttpResponseMessage RemoveSingleAttachment(string filename)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            String _Folder = String.Format(@"{0}",
                                                             ConfigurationManager.AppSettings["AttachmentsPath"].ToString());

            var filePath = String.Format(@"{0}\{1}", _Folder, filename);

            var uri = new Uri(filePath, UriKind.Absolute);
            System.IO.File.Delete(uri.LocalPath);
            response.Content = new StringContent("Attachment removed", System.Text.Encoding.UTF8, "text/plain");
            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;

        }

        [HttpGet]
        [Route("RemoveAllAttachment")]
        public HttpResponseMessage RemoveAllAttachment(List<string> filenames)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            foreach (string file in filenames)
            {
                String _Folder = String.Format(@"{0}",
                                                             ConfigurationManager.AppSettings["AttachmentsPath"].ToString());

                var filePath = String.Format(@"{0}\{1}", _Folder, file);

                var uri = new Uri(filePath, UriKind.Absolute);
                System.IO.File.Delete(uri.LocalPath);
            }
            response.Content = new StringContent("Attachment removed", System.Text.Encoding.UTF8, "text/plain");
            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;

        }

        [HttpGet]
        [Route("GetAttachment")]
        public HttpResponseMessage GetAttachmentById(int Id)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                Attachment FileAtt = arcodb.Attachments.Where(a => a.ID == Id).FirstOrDefault();

                if (FileAtt.ReferenceName != Guid.Empty)
                {
                    string filePath = ConfigurationManager.AppSettings["AttachmentsPath"];
                    string fullPath = filePath + "\\" + FileAtt.FilePath;
                    if (File.Exists(fullPath))
                    {

                        response = new HttpResponseMessage(HttpStatusCode.OK);
                        var fileStream = new FileStream(fullPath, FileMode.Open);
                        response.Content = new StreamContent(fileStream);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                        response.Content.Headers.ContentDisposition.FileName = FileAtt.FileName;
                        return response;
                    }
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while reading the attachment", ex);
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/plain");
                return response;
            }
        }

        [HttpGet]
        [Route("DownloadOutlookAddin")]
        public HttpResponseMessage DownloadOutlookAddin(String fileName)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                   
                    string filePath = ConfigurationManager.AppSettings["DownloadPath"];
                    //fileName = ConfigurationManager.AppSettings["OutlookAddInFileName"] + fileName + ".msi";
                    string fullPath = filePath + "\\" + fileName;
                    if (File.Exists(fullPath))
                    {

                        response = new HttpResponseMessage(HttpStatusCode.OK);
                        var fileStream = new FileStream(fullPath, FileMode.Open);
                        response.Content = new StreamContent(fileStream);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-msdownload");
                        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                        response.Content.Headers.ContentDisposition.FileName = fileName;
                        return response;
                    }
                    else
                    {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                    }
                
                
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while downloading Outlook AddIn", ex);
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/plain");
                return response;
            }
        }

        [HttpGet]
        [Route("GetCityStateforCustomer")]
        public IHttpActionResult GetCityStateforCustomer(String Id)
        {
            Log.Info(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName));
            try
            {
                var CityState = (from cus in arcodb.Customers
                                 where cus.CustomerNumber == Id
                                 select new
                                 { CityNState = cus.ShipToCity + "-" + cus.ShipToState,
                                   CustomershiptoAddress = cus.ShiptoAddr1.Trim()+", "+ cus.ShiptoAddr2.Trim() + ", "  + cus.ShiptoAddr3.Trim() + ", " + cus.ShiptoAddr4.Trim()
                                 }).FirstOrDefault();
                return Content(HttpStatusCode.OK, CityState);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while getting city and state for a customer", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting city and state for a customer");
            }
        }


        [HttpGet]
        [Route("LoadPreNote")]
        public IHttpActionResult LoadPreNote(int PreNoteId)
        {

            try
            {
                var PreNote = (from prenote in arcodb.PreNoteSystems
                               join Cus in arcodb.Customers on prenote.CustomerNumber equals Cus.CustomerNumber
                               join sale in arcodb.SalesOrders on prenote.ID equals sale.PreNoteId
                               into salesorders from sales in salesorders.DefaultIfEmpty()
                               where prenote.ID == PreNoteId
                               select new
                               {
                                   PreNoteId = prenote.ID,
                                   CustomerNumber = prenote.CustomerNumber + "-" + Cus.CompanyName,
                                   CustomerNumberForShip=prenote.CustomerNumber,
                                   CityNState = Cus.ShipToCity+"-"+Cus.ShipToState,
                                   Site = prenote.SiteLocation.ID,
                                   notify = prenote.Notify,
                                   Subject = prenote.Subject,
                                   Body = prenote.Body,
                                   SaleOrderNumber = sales == null ? String.Empty : sales.SalesOrderNumber,
                                   Deleted=prenote.Deleted,
                                   Status=prenote.Status,
                                   ShipToAddress=prenote.ShipToAddress,
                                   lastupadated=prenote.ModifiedDate,
                                   AttachmentDetail = (from att in prenote.Attachments
                                                       select new
                                                       {
                                                           ID = att.ID,
                                                           FileName = att.FileName,
                                                           Type = att.Type,
                                                           ReferenceName = att.ReferenceName
                                                       })
                               }).FirstOrDefault();
                
                return Content(HttpStatusCode.OK, PreNote);

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while loading a prenote", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while loading a prenote");
            }


        }

        [HttpGet]
        [Route("AssignTo")]
        public IHttpActionResult AssignTo(int AssignToId)
        {

            try
            {
                var UserName = (from sale in arcodb.SalesOrders
                               join site in arcodb.UserSiteLocations on sale.SiteLocationId equals site.SiteLocationId
                               join user in arcodb.PrenotesUsers on site.UserId equals user.Id
                               into salesorders
                               from sales in salesorders.ToList()
                               where sale.ID == AssignToId
                                select new
                               {
                                   id=sales.Id,
                                   User = sales.Username
                               }).Distinct().OrderBy(a=> a.User).ToList();

                return Content(HttpStatusCode.OK, UserName);

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while loading a prenote", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while loading a prenote");
            }


        }

        public bool CheckPreNoteDelete(int PrenoteId)
        {
            return arcodb.PreNoteSystems.Where(a => a.ID == PrenoteId && a.Deleted == true).Count()>0;
        }


        public bool CheckPreNoteSubmit(int PrenoteId)
        {
            return arcodb.PreNoteSystems.Where(a => a.ID == PrenoteId && a.Status == false).Count() > 0;
        }

        public void PrenoteBooked(int PrenoteId)
        {
            PreNoteSystem Prenote = arcodb.PreNoteSystems.Where(a => a.ID == PrenoteId).FirstOrDefault();
            Prenote.Status = false;
            arcodb.SaveChanges();
        }

        public List<Attachment> getAttachmentforPrenote(int PrenoteId)
        {
            List<Attachment> Attachments = arcodb.Attachments.Where(a => a.PreNoteId == PrenoteId).ToList();
            return Attachments;
        }

        [HttpPost]
        [Route("UpdatePreNote")]
        public IHttpActionResult UpdatePreNote([FromBody] UpdatePreNoteVM PreNoteVM)
        {
            if (PreNoteVM.CustomerNumber == null || PreNoteVM.SiteLocationId == null || PreNoteVM.Subject == null || PreNoteVM.Notify == null)
            {
                Log.Error("Missing some mandatory fields.");
                return Content(HttpStatusCode.BadRequest, "Missing some mandatory fields.");
            }

            try
            {
                String EmailMessage = String.Empty;
                PreNoteSystem PreNote = arcodb.PreNoteSystems.Include(a => a.Attachments).Where(b => b.ID == PreNoteVM.ID).FirstOrDefault();
                PreNote.Body = PreNoteVM.Body;
                PreNote.CustomerNumber = PreNoteVM.CustomerNumber;
                PreNote.ModifiedDate = DateTime.Now;
                PreNote.Notify = PreNoteVM.Notify;
                PreNote.SiteLocationId = PreNoteVM.SiteLocationId;
                PreNote.ShipToAddress = PreNoteVM.ShipToAddress;
                PreNote.Subject = PreNoteVM.Subject;
                var Sales = (from prenote in arcodb.PreNoteSystems
                               join sale in arcodb.SalesOrders on prenote.ID equals sale.PreNoteId
                               into salesorders
                               from sales in salesorders.DefaultIfEmpty()
                               where prenote.ID == PreNoteVM.ID
                               select new {
                                   SaleOrderNumber = sales == null ? String.Empty : sales.SalesOrderNumber,
                               }).FirstOrDefault();
                
                if (Sales.SaleOrderNumber != PreNoteVM.SaleOrderNumber && PreNoteVM.SaleOrderNumber != "")
                {
                    SalesOrder Saledb = arcodb.SalesOrders.Where(p => p.SalesOrderNumber==PreNoteVM.SaleOrderNumber).FirstOrDefault();
                    if (Saledb != null)
                    {
                        if (Saledb.PreNoteId == 0 ||Saledb.PreNoteId==null)
                        {
                            Saledb.PreNoteId = PreNoteVM.ID;
                            PreNote.Status = false;
                            Saledb.SalesOrderNumber = PreNoteVM.SaleOrderNumber;
                            List<Attachment> Attachments = PreNote.Attachments.ToList();
                            String _SourceFolder = String.Format(@"{0}", ConfigurationManager.AppSettings["AttachmentsPath"].ToString());
                            String _DestFolder = String.Format(@"{0}\{1}\{2}", ConfigurationManager.AppSettings["FileServerPath"].ToString(), Saledb.FolderPath, "Prenote Attachments");

                            if (!Directory.Exists(_DestFolder))
                            {
                                Directory.CreateDirectory(_DestFolder);
                                Log.Info(String.Format("Prenote folder:{0} created", _DestFolder));
                            }
                            foreach (Attachment item in Attachments)
                            {
                                var SourcefilePath = String.Format(@"{0}\{1}", _SourceFolder, item.ReferenceName + "." + item.Type);
                                var DestfilePath = String.Format(@"{0}\{1}", _DestFolder, item.FileName + "." + item.Type);
                                File.Copy(SourcefilePath, DestfilePath);
                            }
                        }
                        else
                        {
                            //Prenote is matched 
                            return Content(HttpStatusCode.BadRequest, "Prenote is already mapped.");
                        }
                    }
                    else
                    {
                        // no such Sale order
                        return Content(HttpStatusCode.BadRequest, "No Sales Order Exits.");
                    }
                }
                else if (PreNoteVM.SaleOrderNumber == "" && Sales.SaleOrderNumber != PreNoteVM.SaleOrderNumber)
                {
                    SalesOrder Saledb = arcodb.SalesOrders.Where(p => p.PreNoteId==PreNoteVM.ID).FirstOrDefault();
                    Saledb.PreNoteId = null;
                    PreNote.Status = true;
                    String _DestFolder = String.Format(@"{0}\{1}\{2}", ConfigurationManager.AppSettings["FileServerPath"].ToString(), Saledb.FolderPath, "Prenote Attachments");
                    EmailMessage = "This prenote has been reactivated.";
                    if (Directory.Exists(_DestFolder))
                    {
                        Directory.Delete(_DestFolder,true);
                        Log.Info(String.Format("Prenote folder:{0} PrenoteFolderDeleted", _DestFolder));
                    }
                }
                if (PreNoteVM.Attachments.Count !=0 || PreNoteVM.Attachments !=null)
                {
                    foreach (Attachment A in PreNoteVM.Attachments)
                    {
                        Attachment file = arcodb.Attachments.Where(a => a.ID == A.ID).FirstOrDefault();

                        if (file != null)
                        {
                            file.FileName = A.FileName;
                            file.ModifiedDate = DateTime.Now;
                            file.Type = A.Type;
                            file.ReferenceName = A.ReferenceName;

                        }
                        else
                        {
                            Attachment Att = new Attachment();
                            Att.CreatedDate = DateTime.Now;
                            Att.FileName = A.FileName;
                            Att.ModifiedDate = DateTime.Now;
                            Att.PreNoteId = PreNote.ID;
                            Att.Type = A.Type;
                            Att.ReferenceName = A.ReferenceName;
                            Att.FilePath = A.FilePath;
                            PreNote.Attachments.Add(Att);
                        }
                    }

                   // arcodb.Attachments.AddRange(PreNote.Attachments);
                    
                }
                
                SmtpClient _emailclient = new SmtpClient();
                MailMessage _message = new MailMessage();
                //_message.From = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"].ToString());
                String customername = arcodb.Customers.Where(fil => fil.CustomerNumber == PreNote.CustomerNumber).SingleOrDefault().CompanyName;
                String sitelocation = arcodb.SiteLocations.Where(fil => fil.ID == PreNote.SiteLocationId).SingleOrDefault().SiteLocation1;
                List<String> _toaddresses = new List<String>(PreNote.Notify.Split(';'));
                _toaddresses.ForEach(_toaddress =>
                {
                    _message.To.Add(new MailAddress(_toaddress));
                });
                _message.Subject = PreNote.CustomerNumber + "-" + customername + ": " + sitelocation + ": " + PreNote.Subject;
                if (EmailMessage != String.Empty)
                {
                    _message.Body = "Updated Prenote- " + EmailMessage + "\r" + PreNote.Body;
                }
                else
                {
                    _message.Body = "Updated Prenote-" + "\r" + PreNote.Body;
                }
                _emailclient.Send(_message);
                Log.Info("Mail Sent.");
                arcodb.SaveChanges();
                return Content(HttpStatusCode.OK, PreNote);
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                    Log.Error(string.Format("Err @ UpdatePrenoteSystem: Method:{0}, Entity Type: {1}, Entity State: {2}", "UpdatePrenote", eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Log.Error(string.Format("Err @ UpdatePrenoteSystem: Method:{0}, {1}, {2}", "UpdatePrenote", ve.PropertyName, ve.ErrorMessage));
                    }
                }
                return Content(HttpStatusCode.BadRequest, "There was an error while creating a prenote");

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while creating a prenote", ex);
                return Content(HttpStatusCode.BadRequest, "There was an error while creating a prenote");
            }
        }


        public void MapSalesOrder1( OrderFramework OrderFramework ,String FolderPath)
        {
            //SalesOrder Sale = arcodb.SalesOrders.Where(a => a.PreNoteId.Equals(PrenoteId)).FirstOrDefault();
            List<SalesOrder> SalesList = arcodb.SalesOrders.Where(a => a.PreNoteId == OrderFramework.PrenoteId).ToList();
            SiteLocation Site = arcodb.SiteLocations.Where(a => a.SiteLocation1 == OrderFramework.SiteLocation).FirstOrDefault();
            CSRAssignment CSR = arcodb.CSRAssignments.Where(a => a.SiteLocation == Site.SiteNumber && a.CustomerNumber.Trim() == OrderFramework.CustomerNumberBillTo
                                ).FirstOrDefault();
            PrenotesUser user=new PrenotesUser();
            if (CSR != null)
            {
                user = arcodb.PrenotesUsers.Where(a => a.Email.Equals(CSR.CSREmail)).FirstOrDefault();
            }
            else
            {
                user.Id = 0;
            }
            
            SalesOrder SalesOrderMap = new SalesOrder();

            if (SalesList != null)
            {
                foreach (SalesOrder Sale in SalesList)
                {
                    Sale.PreNoteId = null;
                }
            }

            if (user.Id > 0)
            {
                SalesOrderMap.SalesOrderNumber = OrderFramework.SalesOrderNumber;
                SalesOrderMap.PreNoteId = OrderFramework.PrenoteId < 0 ? (int?)null : OrderFramework.PrenoteId;
                SalesOrderMap.FolderPath = FolderPath;
                SalesOrderMap.CreatedDate = DateTime.Now;
                SalesOrderMap.ModifiedDate = DateTime.Now;
                SalesOrderMap.SiteLocation = Site;
                SalesOrderMap.SiteLocationId = Site.ID;
                SalesOrderMap.CustomerNumber = OrderFramework.CustomerNumberBillTo;
                SalesOrderMap.Status = 1;
                SalesOrderMap.AssignedTO = user.Id;
                SendEmailonOrderAssignment(user, SalesOrderMap);
            }
            else
            {
                SalesOrderMap.SalesOrderNumber = OrderFramework.SalesOrderNumber;
                SalesOrderMap.PreNoteId = OrderFramework.PrenoteId < 0 ? (int?)null : OrderFramework.PrenoteId;
                SalesOrderMap.FolderPath = FolderPath;
                SalesOrderMap.CreatedDate = DateTime.Now;
                SalesOrderMap.ModifiedDate = DateTime.Now;
                SalesOrderMap.SiteLocation = Site;
                SalesOrderMap.SiteLocationId = Site.ID;
                SalesOrderMap.CustomerNumber = OrderFramework.CustomerNumberBillTo;
                SalesOrderMap.Status = 2;
            }
            arcodb.SalesOrders.Add(SalesOrderMap);
            arcodb.SaveChanges();

        }

        public void MapSalesOrder(OrderFramework OrderFramework, String FolderPath)
        {
            SiteLocation Site = arcodb.SiteLocations.Where(a => a.SiteLocation1 == OrderFramework.SiteLocation).FirstOrDefault();
            CSRAssignment CSR = arcodb.CSRAssignments.Where(a => a.SiteLocation == Site.SiteNumber &&  a.CustomerNumber.Trim() == OrderFramework.CustomerNumberBillTo
                                ).FirstOrDefault();
            PrenotesUser user = new PrenotesUser();
            if (CSR != null)
            {
                user = arcodb.PrenotesUsers.Where(a => a.Email.Equals(CSR.CSREmail)).FirstOrDefault();
            }
            else
            {
                user.Id = 0;
            }
            SalesOrder SalesOrderMap = new SalesOrder();
            if (CSR != null && user !=null)
            {
                SalesOrderMap.SalesOrderNumber = OrderFramework.SalesOrderNumber;
                SalesOrderMap.PreNoteId = null;
                SalesOrderMap.FolderPath = FolderPath;
                SalesOrderMap.CreatedDate = DateTime.Now;
                SalesOrderMap.SiteLocation = Site; 
                SalesOrderMap.SiteLocationId = Site.ID;
                SalesOrderMap.CustomerNumber = OrderFramework.CustomerNumberBillTo;
                SalesOrderMap.Status = 1;
                SalesOrderMap.AssignedTO = user.Id;
                SendEmailonOrderAssignment(user, SalesOrderMap);
            }
            else
            {
                SalesOrderMap.SalesOrderNumber = OrderFramework.SalesOrderNumber;
                SalesOrderMap.PreNoteId = null;
                SalesOrderMap.FolderPath = FolderPath;
                SalesOrderMap.CreatedDate = DateTime.Now;
                SalesOrderMap.SiteLocation = Site;
                SalesOrderMap.SiteLocationId = Site.ID;
                SalesOrderMap.CustomerNumber = OrderFramework.CustomerNumberBillTo;
                SalesOrderMap.Status = 2;
            }
           
            arcodb.SalesOrders.Add(SalesOrderMap);
            arcodb.SaveChanges();
        }

        public void SendEmailonOrderAssignment(PrenotesUser user, SalesOrder salesorder)
        {
            SmtpClient _emailclient = new SmtpClient();
            MailMessage _message = new MailMessage();
            string CustomerName = arcodb.Customers.Where(a => a.CustomerNumber == salesorder.CustomerNumber).Select(a => a.CompanyName).FirstOrDefault();
            _message.From = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"].ToString());
            //MailAddressCollection _to = new MailAddressCollection();
            _message.To.Add(new MailAddress(user.Email));
           
            _message.Subject = String.Format("Plant: {3} {0} - {1} Order {2}",
                                             CustomerName,
                                             salesorder.CustomerNumber,
                                             salesorder.SalesOrderNumber,
                                             salesorder.SiteLocation.SiteLocation1);
            _message.IsBodyHtml = true;
            if (salesorder.Status == 1)
            {
                _message.Body = String.Format("The below SalesOrder has been assigned to you <br/> SO : {0}", salesorder.SalesOrderNumber);
            }
            else if(salesorder.Status == 3)
            {
                _message.Body = String.Format("The below SalesOrder has been completed. <br/> SO : {0}", salesorder.SalesOrderNumber);
            }
           else if (salesorder.Status == 4)
            {
                _message.Body = String.Format("The below salesOrder is in Inprogress. <br/> SO : {0}", salesorder.SalesOrderNumber);
            }
              
            _emailclient.Send(_message);
            Log.Info("Mail Sent.");
        }

        [HttpGet]
        [Route("GetEmails")]
        public IHttpActionResult GetEmails()
        {
            try
            {
                var Emails = (from email in arcodb.NotifyEmails
                              select new
                              {
                                  ID = email.Id,
                                  EmailId = email.Email
                              }).ToList();

                return Content(HttpStatusCode.OK, Emails);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while getting list of emails for prenote system", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of emails for prenote system");
            }

        }

        [HttpGet]
        [Route("GetAllEmails")]
        public IHttpActionResult GetAllEmails()
        {
            try
            {
                var Emails = (from email in arcodb.NotifyEmails
                              select email.Email).ToList();

                return Content(HttpStatusCode.OK, Emails);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while getting list of emails for prenote system", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of emails for prenote system");
            }

        }

        [HttpGet]
        [Route("GetEmailsForPrenote")]
        public IHttpActionResult GetEmails(int PrenoteId)
        {
            try
            {
                List<String> emails = arcodb.PreNoteSystems.Where(fil => fil.ID == PrenoteId)
                                      .FirstOrDefault().Notify.Split(new char[] { ';' })
                                      .ToList();
                //var Emails = (from email in arcodb.NotifyEmails
                //              where emails.Contains(email.Email)
                //              select new
                //              {
                //                  ID = email.Id,
                //                  EmailId = email.Email
                //              }).ToList();

                return Content(HttpStatusCode.OK, emails);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while getting list of emails for prenote system", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of emails for prenote system");
            }
        }

    }
}