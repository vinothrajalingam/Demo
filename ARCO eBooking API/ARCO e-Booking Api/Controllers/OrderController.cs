using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ARCO.EndPoint.Model;
using System.IO;
using System.Configuration;
using System.Web;
using System.Text;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Net.Mail;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using log4net;
using ARCO.EndPoint.eCard.VM;
using System.Data.Entity.Validation;

namespace ARCO.EndPoint.eCard.Controllers
{
    
    [RoutePrefix("api/Order")]
    public class OrderController : ApiController
    {
        private static readonly ILog Log =
             LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public OrderController()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        PreNoteController Prenotes = new PreNoteController();
        [HttpPost]
        [Route("PostOrder")]
        public IHttpActionResult PostOrder(ArcoOrder order)
        {
            if (Prenotes.CheckPreNoteSubmit(order.OrderFramework.PrenoteId))
            {
                return Content(HttpStatusCode.Conflict, "Prenote already submitted");
            }
            if (Prenotes.CheckPreNoteDelete(order.OrderFramework.PrenoteId))
            {
                return Content(HttpStatusCode.NotFound, "Prenote Deleted");
            }

            ArcoOrder _orderclonewithoutimages = new ArcoOrder()
            {
                InboundnPackaging = order.InboundnPackaging,
                OrderFramework = order.OrderFramework,
                SpecificationRepairs = order.SpecificationRepairs,
                VoiceNotesnPics = new VoiceNotesnPics()
                {
                    BookingCompletionDate = order.VoiceNotesnPics.BookingCompletionDate,
                    ImageAttachments = new List<string>(),
                    VoiceAndNotesComments = order.VoiceNotesnPics.VoiceAndNotesComments
                }
            };
            Log.Info(String.Format("Request received from:{0}/{1} with the following detail\r\n{2}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName,
                                   JsonConvert.SerializeObject(_orderclonewithoutimages)));
            try
            {
                String _CurrentYearFolder = String.Format(@"{0}\{1}",
                                                          ConfigurationManager.AppSettings["FileServerPath"].ToString(),
                                                          DateTime.Now.Year.ToString());
                if (!Directory.Exists(_CurrentYearFolder))//If Current year folder is not there create it
                {
                    Directory.CreateDirectory(_CurrentYearFolder);
                    Log.Info(String.Format("Year folder:{0} created", _CurrentYearFolder));
                }
                String pattern = @"[\\\/\:\*\?\""\<\>\|]";
                String _BillToCompanyWithLegalCharacters = Regex.Replace(order.OrderFramework.BilltoCompany,
                                                    pattern,
                                                    " ",
                                                    RegexOptions.CultureInvariant);
                String _ClientFolder = String.Format(@"{0}\{1}-{2}",
                                                   _CurrentYearFolder,
                                                   _BillToCompanyWithLegalCharacters,
                                                   order.OrderFramework.CustomerNumberBillTo);

                if (!Directory.Exists(_ClientFolder))//If Customer folder under Current year folder is not there create it
                {
                    Directory.CreateDirectory(_ClientFolder);
                    Log.Info(String.Format("Client folder:{0} created", _ClientFolder));
                }

                String orderfolder = String.Format(@"{0}\{1}",
                                                   _ClientFolder,
                                                   order.OrderFramework.SalesOrderNumber);

                String filename = String.Format(@"{0}\{1}.json", orderfolder, order.OrderFramework.SalesOrderNumber);

                if (!Directory.Exists(orderfolder))
                {
                    Directory.CreateDirectory(orderfolder);
                    CreateExcelDocument(order, String.Format(@"{0}\{1}.xlsm", orderfolder, order.OrderFramework.SalesOrderNumber));
                    Log.Info(String.Format(@"Data file:{0}\{1}.xlsm created", orderfolder, order.OrderFramework.SalesOrderNumber));
                    //int _imgcounter = 1;
                    //order
                    //    .VoiceNotesnPics
                    //    .ImageAttachments
                    //    .ForEach(Attachment =>
                    //    {
                    //        String _imagefilename = String.Format(@"{0}\{3}_Image_{1}.{2}", orderfolder, _imgcounter++, "jpeg", order.OrderFramework.SalesOrderNumber);
                    //        if (SaveImage(Attachment, _imagefilename))
                    //        {
                    //            Log.Info(String.Format(@"Image {0} created", _imagefilename));
                    //        }
                    //        else
                    //        {}
                    //    });
                    String Folderpath = String.Format(@"{0}\{1}-{2}\{3}", DateTime.Now.Year.ToString(),
                                                    _BillToCompanyWithLegalCharacters,
                                                    order.OrderFramework.CustomerNumberBillTo,
                                                    order.OrderFramework.SalesOrderNumber);

                    if (order.OrderFramework.PrenoteId > 0)
                    {

                        List<Attachment> Attachments = Prenotes.getAttachmentforPrenote(order.OrderFramework.PrenoteId);

                        String _SourceFolder = String.Format(@"{0}", ConfigurationManager.AppSettings["AttachmentsPath"].ToString());

                        String _DestFolder = String.Format(@"{0}\{1}", orderfolder, "Prenote Attachments");

                        if (!Directory.Exists(_DestFolder))
                        {
                            Directory.CreateDirectory(_DestFolder);
                            Log.Info(String.Format("Prenote folder:{0} created", _DestFolder));
                        }

                        foreach (Attachment item in Attachments)
                        {
                            var SourcefilePath = String.Format(@"{0}\{1}", _SourceFolder, item.ReferenceName + "." + item.Type);
                            var DestfilePath = String.Format(@"{0}\{1}", _DestFolder, item.FileName +"."+item.Type);
                            File.Copy(SourcefilePath, DestfilePath);
                        }

                        //String Folderpath = String.Format(@"{0}\{1}-{2}\{3}", DateTime.Now.Year.ToString(),
                        //                            _BillToCompanyWithLegalCharacters,
                        //                            order.OrderFramework.CustomerNumberBillTo,
                        //                            order.OrderFramework.SalesOrderNumber);     



                        Prenotes.PrenoteBooked(order.OrderFramework.PrenoteId);
                        Prenotes.MapSalesOrder1(order.OrderFramework, Folderpath);
                    }
                    else
                    {
                        Prenotes.MapSalesOrder(order.OrderFramework, Folderpath);
                    }
                    SmtpClient _emailclient = new SmtpClient();
                    MailMessage _message = new MailMessage();
                   // _message.From = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"].ToString());
                    MailAddressCollection _to = new MailAddressCollection();
                    List<String> _toaddresses = new List<String>(ConfigurationManager.AppSettings["EmailTo"].Split(';'));
                    _toaddresses.ForEach(_toaddress =>
                        {
                            _message.To.Add(new MailAddress(_toaddress));
                        });
                    _message.Subject = String.Format("Plant: {3} {0} - {1} Order {2}",
                                                     order.OrderFramework.BilltoCompany,
                                                     order.OrderFramework.CustomerNumberBillTo,
                                                     order.OrderFramework.SalesOrderNumber,
                                                     order.OrderFramework.SiteLocation);
                    _message.IsBodyHtml = true;
                    _message.Body = String.Format("Please find below the detail in the following File server<BR><A HREF=\"{0}\">{0}</A>", orderfolder);
                    //_emailclient.Send(_message);
                    //Log.Info("Mail Sent.");
                    return Content(HttpStatusCode.OK, "SUCCESS");
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, "Order already Exist");

                }

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                    Log.Error(string.Format("Err @ UpdatePrenoteSystem: Method:{0}, Entity Type: {1}, Entity State: {2}", "PrenoteSalesOrder", eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Log.Error(string.Format("Err @ UpdatePrenoteSystem: Method:{0}, {1}, {2}", "PrenoteSalesOrder", ve.PropertyName, ve.ErrorMessage));
                    }
                }
                Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                return Content(HttpStatusCode.InternalServerError, String.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));

            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                return Content(HttpStatusCode.InternalServerError, String.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }

        }

        private void CreateExcelDocument(ArcoOrder OrderDetail, String FileName)
        {
            String _TemplateFileName = "ARCOeCardTemplateV3.xlsm";
            String _TemplatePath = String.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), _TemplateFileName);
            FileInfo _templatefileinfo = new FileInfo(_TemplatePath);
            FileInfo _targetfileinfo = new FileInfo(FileName);
            ExcelPackage _excel = new ExcelPackage(_targetfileinfo, _templatefileinfo);

            ExcelWorksheet _ordersheet = _excel.Workbook.Worksheets["OrderFramework"];
            ExcelWorksheet _InboundAndPackagingSheet = _excel.Workbook.Worksheets["Inbound&Packaging"];
            ExcelWorksheet _VoiceAndNotesSheet = _excel.Workbook.Worksheets["Voice&Notes"];
            ExcelWorksheet _specandrepairsheet = _excel.Workbook.Worksheets["Specification&Repairs"];

            FillOrderSheet(_ordersheet, OrderDetail);

            FillInboundAndPackaging(_InboundAndPackagingSheet, OrderDetail);

            _specandrepairsheet.Name = "Specification&Repairs-1";
            if (OrderDetail.SpecificationRepairs.Count > 1)
            {
                for (int idx = 1; idx < OrderDetail.SpecificationRepairs.Count; idx++)
                {
                    _excel.Workbook.Worksheets.Add(String.Format("Specification&Repairs-{0}", idx + 1),
                                                   _excel.Workbook.Worksheets[String.Format("Specification&Repairs-{0}", idx)]);
                }
            }
            List<ExcelWorksheet> _specssheets = _excel.Workbook.Worksheets.Where(fil => fil.Name.StartsWith("Specification&Repairs-")).ToList();
            int _specidx = 0;
            _specssheets.ForEach(item =>
            {
                FillSpecsAndReapirs(item, OrderDetail.SpecificationRepairs[_specidx++]);
            });

            FillVoiceAndNotes(_VoiceAndNotesSheet, OrderDetail);
            _excel.Workbook.Worksheets.MoveToEnd("Voice&Notes");
            _ordersheet.Select();
            _excel.Save();
        }

        private void FillOrderSheet(ExcelWorksheet OrderSheet, ArcoOrder Orderdetail)
        {
            OrderSheet.Select();
            Regex regEx = new Regex("{{.*?}}");
            IEnumerable<ExcelRangeBase> _foundranges = OrderSheet.Cells.Where(fil => fil.Value != null).Where(fil => regEx.IsMatch(fil.Value.ToString()));

            Type _type = Orderdetail.OrderFramework.GetType();
            foreach (ExcelRangeBase _foundrange in _foundranges)
            {
                String _membername = ((String)_foundrange.Value).Replace("{{", String.Empty).Replace("}}", String.Empty);
                try
                {
                    _foundrange.Value = _type.GetProperty(_membername).GetValue(Orderdetail.OrderFramework, null);
                }
                catch (Exception ex)
                {
                    _foundrange.Value = "Member Name not found";
                }
            }
            OrderSheet.Select("A1");

        }

        public void FillInboundAndPackaging(ExcelWorksheet InboundAndPackagingSheet, ArcoOrder Orderdetail)
        {
            InboundAndPackagingSheet.Select();
            Regex regEx = new Regex("{{.*?}}");
            IEnumerable<ExcelRangeBase> _foundranges = InboundAndPackagingSheet.Cells.Where(fil => fil.Value != null).Where(fil => regEx.IsMatch(fil.Value.ToString()));

            Type _type = Orderdetail.InboundnPackaging.GetType();
            foreach (ExcelRangeBase _foundrange in _foundranges)
            {
                String _membername = ((String)_foundrange.Value).Replace("{{", String.Empty).Replace("}}", String.Empty);
                try
                {
                    _foundrange.Value = _type.GetProperty(_membername).GetValue(Orderdetail.InboundnPackaging, null);
                }
                catch (Exception ex)
                {
                    _foundrange.Value = "Member Name not found";
                }
            }
            InboundAndPackagingSheet.Select("A1");
        }

        private void FillVoiceAndNotes(ExcelWorksheet VoiceAndNotesSheet, ArcoOrder Orderdetail)
        {
            VoiceAndNotesSheet.Select();
            Regex regEx = new Regex("{{.*?}}");
            IEnumerable<ExcelRangeBase> _foundranges = VoiceAndNotesSheet.Cells.Where(fil => fil.Value != null).Where(fil => regEx.IsMatch(fil.Value.ToString()));

            Type _type = Orderdetail.VoiceNotesnPics.GetType();
            foreach (ExcelRangeBase _foundrange in _foundranges)
            {
                String _membername = ((String)_foundrange.Value).Replace("{{", String.Empty).Replace("}}", String.Empty);
                try
                {
                    _foundrange.Value = _type.GetProperty(_membername).GetValue(Orderdetail.VoiceNotesnPics, null);
                }
                catch (Exception ex)
                {
                    _foundrange.Value = "Member Name not found";
                }
            }
            regEx = new Regex("[[ImageList]]");
            ExcelRangeBase _imgrange = VoiceAndNotesSheet.Cells.Where(fil => fil.Value != null).Where(fil => regEx.IsMatch(fil.Value.ToString())).FirstOrDefault();

            if (_foundranges != null)
            {
                if (Orderdetail.VoiceNotesnPics.ImageAttachments.Count > 0)
                {
                    int _imgcount = 1;
                    String _imagelist = String.Empty;
                    Orderdetail.VoiceNotesnPics
                        .ImageAttachments
                        .ForEach(img =>
                        {
                            _imagelist = String.Concat(_imagelist,
                                                       String.Format("{0}_Image_{1}.jpeg", Orderdetail.OrderFramework.SalesOrderNumber, _imgcount++),
                                                       "\n");
                        });
                    _imgrange.Value = _imagelist;
                }
                else
                {
                    _imgrange.Value = String.Empty;
                }
            }
            VoiceAndNotesSheet.Select("A1");
        }

        private void FillSpecsAndReapirs(ExcelWorksheet SpecsSheet, SpecificationRepairs SpecsDetail)
        {
            SpecsSheet.Select();
            Regex regEx = new Regex("<<.*?>>");
            IEnumerable<ExcelRangeBase> _foundranges = SpecsSheet.Cells.Where(fil => fil.Value != null).Where(fil => regEx.IsMatch(fil.Value.ToString()));

            Type _type = SpecsDetail.ARCOSpecification.GetType();
            SpecsDetail.ARCOSpecification.SpecificationHeader = SpecsDetail.SpecificationHeader;
            foreach (ExcelRangeBase _foundrange in _foundranges)
            {
                String _membername = ((String)_foundrange.Value).Replace("<<", String.Empty).Replace(">>", String.Empty);
                
                try
                {
                    _foundrange.Value = _type.GetProperty(_membername).GetValue(SpecsDetail.ARCOSpecification, null);

                }
                catch (Exception ex)
                {
                    _foundrange.Value = "Member Name not found";
                }
            }

            regEx = new Regex("{{.*?}}");
            _foundranges = SpecsSheet.Cells.Where(fil => fil.Value != null).Where(fil => regEx.IsMatch(fil.Value.ToString()));

            _type = SpecsDetail.ARCORepairs.GetType();
            SpecsDetail.ARCORepairs.RepairsHeader = SpecsDetail.RepairsHeader;
            foreach (ExcelRangeBase _foundrange in _foundranges)
            {
                String _membername = ((String)_foundrange.Value).Replace("{{", String.Empty).Replace("}}", String.Empty);
                
                try
                {
                    _foundrange.Value = _type.GetProperty(_membername).GetValue(SpecsDetail.ARCORepairs, null);
                }
                catch (Exception ex)
                {
                    _foundrange.Value = "Member Name not found";
                }
            }
            SpecsSheet.Select("A1");
        }

        [HttpGet]
        [Route("GetCustomerList")]
        public HttpResponseMessage GetCustomerList()
        {
            Log.Info(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName));
            HttpResponseMessage resp;
            try
            {
                ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
                var customerlist = (from cus in arcodb.Customers
                                    select new
                                    {
                                        Id = cus.ID,
                                        CompanyName = cus.CompanyName,
                                        CustomerNumber = cus.CustomerNumber.Trim(),
                                        CompanyNameAndNumber = cus.CustomerNumber.Trim() + "-" + cus.CompanyName
                                    }).OrderBy(Cus => Cus.Id).ToList();

                String _JSONCustomerList = JsonConvert.SerializeObject(customerlist);

                resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(_JSONCustomerList, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                resp.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
        }

        [HttpGet]
        [Route("GetCustomer")]
        public HttpResponseMessage GetCustomer(String CustomerNumber)
        {
            Log.Info(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName));
            HttpResponseMessage resp;
            try
            {
                ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();

                var customerlist = (from cus in arcodb.Customers
                                    where cus.CustomerNumber.Trim() == CustomerNumber.Trim()
                                    select cus).FirstOrDefault();

                String _JSONCustomerList = JsonConvert.SerializeObject(customerlist);


                resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(_JSONCustomerList, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Request received from:{0}/{1}",
                                   HttpContext.Current.Request.UserHostAddress,
                                   HttpContext.Current.Request.UserHostName), ex);
                resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                resp.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
        }

        //private static bool SaveImage(string imageString, string location)
        //{
        //    try
        //    {
        //        imageString = imageString.Substring(imageString.IndexOf(',') + 1);
        //        byte[] bytes = Convert.FromBase64String(imageString);

        //        using (FileStream fs = new FileStream(location, FileMode.Create))
        //        {
        //            fs.Write(bytes, 0, bytes.Length);
        //        }
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}


        [HttpPost]
        [Route("SaveImage")]
        public IHttpActionResult SaveImage(UWPImageUpload Obj)
        {
            try
            {

                String SaveLocation = String.Format(@"{0}\{1}\{2}\{3}\{4}",
                                                          ConfigurationManager.AppSettings["FileServerPath"].ToString(),
                                                          DateTime.Now.Year.ToString(),
                                                          Obj.CompanyNameandNumber,
                                                          Obj.SalesOrderNumber,
                                                          Obj.FileName);
                byte[] bytes = Convert.FromBase64String(Obj.base64StringImage);

                using (FileStream fs = new FileStream(SaveLocation, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }

                //File.WriteAllBytes(SaveLocation, Convert.FromBase64String(Obj.base64StringImage));
                return Content(HttpStatusCode.OK, "SUCCESS");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, "FAILED");
            }
        }

        [HttpGet]
        [Route("SaveImage")]
        private static bool SaveImage(String ImageString, String SalesOrderNumber, String CompanyName,  String FileName)
        {
            try
            {

                String SaveLocation = String.Format(@"{0}\{1}\{2}\{3}\{4}",
                                                          ConfigurationManager.AppSettings["FileServerPath"].ToString(),
                                                          DateTime.Now.Year.ToString(),
                                                          CompanyName,
                                                          SalesOrderNumber,
                                                          FileName);

                File.WriteAllBytes(SaveLocation, Convert.FromBase64String(ImageString));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //[HttpGet]
        //[Route("GetOrderList")]
        //public IHttpActionResult GetOrderList()
        //{
        //    ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
        //    try
        //    {
        //        var OrderList = (from order in arcodb.SalesOrders
        //                         join cus in arcodb.Customers on order.CustomerNumber equals cus.CustomerNumber
        //                         join site in arcodb.SiteLocations on order.SiteLocationId equals site.ID
        //                         join orderstatus in arcodb.OrderStatus on order.Status equals orderstatus.Id
        //                         join user in arcodb.PrenotesUsers on order.AssignedTO equals user.Id
        //                         into userlist from PrenotesUser in userlist.DefaultIfEmpty()
        //                         where order.Status != 3
        //                             select new
        //                             {
        //                                 OrderId = order.ID,
        //                                 AssignedTo = order.AssignedTO,
        //                                 StartDate = order.CreatedDate,
        //                                 Customer  = cus.CustomerNumber + "-" + cus.CompanyName, 
        //                                 Site = site.SiteLocation1,
        //                                 FilePath = order.FolderPath,
        //                                 LastUpdate = order.ModifiedDate,
        //                                 SalesOrderNumber = order.SalesOrderNumber,
        //                                 CityNState = cus.City + "," + cus.State,
        //                                 Status = orderstatus.Status,
        //                                 UserName= PrenotesUser.Username!=null?PrenotesUser.Username:"----"
        //                             }).OrderBy(a => a.Site).ToList();

        //        return Content(HttpStatusCode.OK, OrderList);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("There was an error while getting list of site locations for prenote system", ex);
        //        return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of site locations for prenote system");
        //    }
        //}

        [HttpGet]
        [Route("LoadOrder")]
        public IHttpActionResult LoadOrder(int OrderId)
        {
            ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
            try
            {
                
                String _ServerFolder = String.Format(@"{0}\", ConfigurationManager.AppSettings["FileServerPath"].ToString());
                var Order = (from order in arcodb.SalesOrders
                               join Cus in arcodb.Customers on order.CustomerNumber equals Cus.CustomerNumber
                               join site in arcodb.SiteLocations on order.SiteLocationId equals site.ID
                               join status in arcodb.OrderStatus on order.Status equals status.Id
                               join user in arcodb.PrenotesUsers on order.AssignedTO equals user.Id
                               into userlist
                             from PrenotesUser in userlist.DefaultIfEmpty()
                             where order.ID == OrderId && site.Status != false
                               select new
                               {
                                   OrderId = order.ID,
                                   CustomerNumber = order.CustomerNumber + "-" + Cus.CompanyName,
                                   Site = site.SiteLocation1,
                                   SaleOrderNumber = order.SalesOrderNumber,
                                   Status = order.Status,
                                   FilePath = _ServerFolder + order.FolderPath,
                                   User = order.AssignedTO,
                                   ModifiedId=order.ModifiedBy,
                                   AssignedUser= userlist.Where(a=>a.Id==order.ModifiedBy).Select(a=>a.DisplayName).FirstOrDefault()
                               }).FirstOrDefault();

                if (Order.ModifiedId != null)
                {
                    Order = (from order in arcodb.SalesOrders
                             join Cus in arcodb.Customers on order.CustomerNumber equals Cus.CustomerNumber
                             join site in arcodb.SiteLocations on order.SiteLocationId equals site.ID
                             join status in arcodb.OrderStatus on order.Status equals status.Id
                             join username in arcodb.PrenotesUsers on order.ModifiedBy equals username.Id
                             join user in arcodb.PrenotesUsers on order.AssignedTO equals user.Id
                             into userlist
                             from PrenotesUser in userlist.DefaultIfEmpty()
                             where order.ID == OrderId && site.Status != false
                             select new
                             {
                                 OrderId = order.ID,
                                 CustomerNumber = order.CustomerNumber + "-" + Cus.CompanyName,
                                 Site = site.SiteLocation1,
                                 SaleOrderNumber = order.SalesOrderNumber,
                                 Status = order.Status,
                                 FilePath = _ServerFolder + order.FolderPath,
                                 User = order.AssignedTO,
                                 ModifiedId = order.ModifiedBy,
                                 AssignedUser = username.DisplayName
                             }).FirstOrDefault();
                }

                return Content(HttpStatusCode.OK, Order);

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while loading a order", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while loading a order");
            }


        }

        [HttpPost]
        [Route("UpdateOrder")]
        public IHttpActionResult UpdateOrder([FromBody] SalesOrder Order)
        {
            ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
            var OrdertoUpdate = arcodb.SalesOrders.Where(a => a.ID == Order.ID).FirstOrDefault();
            try
            {

                OrdertoUpdate.Status= Convert.ToInt32(Order.Status);
                OrdertoUpdate.AssignedTO = Order.AssignedTO;
                arcodb.SaveChanges();

                return Content(HttpStatusCode.OK, Order);

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while updating an order", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while updating an order");
            }


        }


        [HttpGet]
        [Route("GetStatusList")]
        public IHttpActionResult GetStatusList()
        {
            ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
            try
            {
                var StatusList = (from status in arcodb.OrderStatus
                                  select new
                                  {
                                      StatusId = status.Id,
                                      Status  = status.Status
                                  }).OrderBy(a => a.StatusId).ToList();

                return Content(HttpStatusCode.OK, StatusList);

            }
            catch (Exception ex)
            {
                Log.Error("There was an error while loading a order", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while loading a order");
            }


        }

        [HttpPost]
        [Route("SetUserName")]
        public IHttpActionResult SetUserName(int UserId, int orderitemid,int status,int assignuserid)
        {
            ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
            try
            {
                SalesOrder So = arcodb.SalesOrders.Where(a => a.ID == orderitemid).FirstOrDefault();
                PrenotesUser User = arcodb.PrenotesUsers.Where(a => a.Id == UserId).FirstOrDefault();
                So.AssignedTO = User.Id;
                So.ModifiedDate = DateTime.Now;
                So.Status = status;
                So.ModifiedBy = assignuserid;
                arcodb.SaveChanges();
                Prenotes.SendEmailonOrderAssignment(User, So);


                return Content(HttpStatusCode.OK, "SUCCESS");
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while assigning the user.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while assigning the user.");
            }


        }


        [HttpPost]
        [Route("SetUserClaimed")]
        public IHttpActionResult SetUserClaimed(int UserId, int orderitemid)
        {
            ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
            try
            {
                SalesOrder So = arcodb.SalesOrders.Where(a => a.ID == orderitemid).FirstOrDefault();
                PrenotesUser User = arcodb.PrenotesUsers.Where(a => a.Id == UserId).FirstOrDefault();
                So.ModifiedDate = DateTime.Now;
                So.Status = 4;
                So.ModifiedBy = UserId;
                So.AssignedTO = User.Id;
                arcodb.SaveChanges();


                return Content(HttpStatusCode.OK, "SUCCESS");
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while assigning the user.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while assigning the user.");
            }


        }

        

        [HttpPost]
        [Route("LoadFilteredOrderList")]
        public IHttpActionResult LoadFilteredOrderList([FromBody] FilteredOrderListVM filter)
        {
            if (filter == null || !ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, "Request body not valid");
            }
            ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
            PrenotesUser user1 = arcodb.PrenotesUsers.Where(a=> a.Username == User.Identity.Name).FirstOrDefault();
            //filter.Site = null;
            int siteid = Convert.ToInt32(filter.Site);
            //SiteLocation sitelocation = arcodb.SiteLocations.Where(s => s.ID == siteid).FirstOrDefault();

            List<FilteredOrderListVM> Orders=new List<FilteredOrderListVM>();
            
            if (user1.Admin == true || user1.Manager == true)
            {
                Orders = (from orders in arcodb.SalesOrders
                          join cus in arcodb.Customers
                          on orders.CustomerNumber equals cus.CustomerNumber
                          join site in arcodb.SiteLocations
                          on orders.SiteLocationId equals site.ID
                          join status in arcodb.OrderStatus on orders.Status equals status.Id
                          join user in arcodb.PrenotesUsers on orders.AssignedTO equals user.Id
                          into userlist
                          from PrenotesUser in userlist.DefaultIfEmpty()
                          where
                               (filter.StatusForProgress != 0 ? orders.Status == filter.StatusForProgress : orders.Status != 3) &&
                              (String.IsNullOrEmpty(filter.CustomerName) || (cus.CompanyName.Contains(filter.CustomerName) ||
                              cus.CustomerNumber.Contains(filter.CustomerName))) &&
                              (filter.Site == null || orders.SiteLocationId == siteid) &&
                              (filter.SalesOrderNumber == null || orders.SalesOrderNumber.Contains(filter.SalesOrderNumber))
                              && (site.Status != false) &&
                              (String.IsNullOrEmpty(filter.Assignusername) || (PrenotesUser.Username.Contains(filter.Assignusername)))

                          // (filter.CreatedDate == null || orders.CreatedDate >= filter.CreatedDate) &&
                          //(filter.EndDate == null || note.CreatedDate.Date <= filter.EndDate.Value.Date)
                          select new FilteredOrderListVM
                          {
                              OrderId = orders.ID,
                              Customer = orders.CustomerNumber + "-" + cus.CompanyName,
                              SalesOrderNumber = orders.SalesOrderNumber,
                              Site = orders.SiteLocation.SiteLocation1,
                              StartDate = orders.CreatedDate,
                              LastUpdatedDate = orders.CreatedDate,
                              LastStartDate = orders.ModifiedDate,
                              LastEndUpdate = orders.ModifiedDate,
                              Status = status.Status,
                              CityNState = cus.City + "," + cus.State,
                              UserName = PrenotesUser.Username != null ? PrenotesUser.Username : "----"
                          })
                                                        .ToList()
                                                        .Where(a => filter.StatusForDate == false ? (filter.StartDate == null || a.StartDate.Value.Date >= filter.StartDate.Value.Date) : filter.StartDate == null || a.LastStartDate.Value.Date >= filter.StartDate.Value.Date).DefaultIfEmpty()
                                                        .Where(a => filter.StatusForDate == false ? (filter.LastUpdatedDate == null || a.LastUpdatedDate.Value.Date <= filter.LastUpdatedDate.Value.Date) : filter.LastUpdatedDate == null || a.LastEndUpdate.Value.Date <= filter.LastUpdatedDate.Value.Date).DefaultIfEmpty()
                                                        .ToList();
            }




            else
            {


                Orders = (from orders in arcodb.SalesOrders
                          join cus in arcodb.Customers
                          on orders.CustomerNumber equals cus.CustomerNumber
                          join site in arcodb.SiteLocations
                          on orders.SiteLocationId equals site.ID
                          join status in arcodb.OrderStatus on orders.Status equals status.Id
                          join user in arcodb.PrenotesUsers on orders.AssignedTO equals (int?)user.Id
                          into userlist
                          from PrenotesUser in userlist.DefaultIfEmpty()
                          where
                          //(filter.StatusForProgress != 0 ? (orders.Status == filter.StatusForProgress || orders.AssignedTO == (int?)null || PrenotesUser.Id.Equals(user1.Id) ): (orders.AssignedTO == (int?)null || PrenotesUser.Id.Equals(user1.Id) && orders.Status != 3))
                          (filter.StatusForProgress == 0 ?
                            (orders.AssignedTO == (int?)null && orders.Status != 3 || PrenotesUser.Id.Equals(user1.Id) && orders.Status != 3) : 
                            filter.StatusForProgress == 1 ?
                            (orders.Status == filter.StatusForProgress && PrenotesUser.Id.Equals(user1.Id)) :
                            filter.StatusForProgress == 2 ?
                            (orders.Status == filter.StatusForProgress || orders.AssignedTO == (int?)null) : 
                            filter.StatusForProgress == 3 ?
                            (orders.Status == filter.StatusForProgress && PrenotesUser.Id.Equals(user1.Id)) :
                            (orders.Status == filter.StatusForProgress && PrenotesUser.Id.Equals(user1.Id)) )

                                &&
                              (String.IsNullOrEmpty(filter.CustomerName) || (cus.CompanyName.Contains(filter.CustomerName) ||
                              cus.CustomerNumber.Contains(filter.CustomerName))) &&
                              (filter.Site == null || orders.SiteLocationId == siteid) &&
                              (filter.SalesOrderNumber == null || orders.SalesOrderNumber.Contains(filter.SalesOrderNumber))
                              && (site.Status != false) &&
                              (String.IsNullOrEmpty(filter.Assignusername) || (PrenotesUser.Username.Contains(filter.Assignusername)))
                              
                              
                          // (filter.CreatedDate == null || orders.CreatedDate >= filter.CreatedDate) &&
                          //(filter.EndDate == null || note.CreatedDate.Date <= filter.EndDate.Value.Date)
                          select new FilteredOrderListVM
                          {
                              OrderId = orders.ID,
                              Customer = orders.CustomerNumber + "-" + cus.CompanyName,
                              SalesOrderNumber = orders.SalesOrderNumber,
                              Site = orders.SiteLocation.SiteLocation1,
                              StartDate = orders.CreatedDate,
                              LastUpdatedDate = orders.CreatedDate,
                              LastStartDate = orders.ModifiedDate,
                              LastEndUpdate = orders.ModifiedDate,
                              Status = status.Status,
                              CityNState = cus.City + "," + cus.State,
                              UserName = PrenotesUser.Username != null ? PrenotesUser.Username : "----"
                          })
                                                    .ToList()
                                                    .Where(a => filter.StatusForDate == false ? (filter.StartDate == null || a.StartDate.Value.Date >= filter.StartDate.Value.Date) : filter.StartDate == null || a.LastStartDate.Value.Date >= filter.StartDate.Value.Date).DefaultIfEmpty()
                                                    .Where(a => filter.StatusForDate == false ? (filter.LastUpdatedDate == null || a.LastUpdatedDate.Value.Date <= filter.LastUpdatedDate.Value.Date) : filter.LastUpdatedDate == null || a.LastEndUpdate.Value.Date <= filter.LastUpdatedDate.Value.Date).DefaultIfEmpty()
                                                    .ToList();
            }
            

            if (filter.DateForSearch != 0)
            {
                filter.Sorting = "datecreated";
            }
            if(Orders.Count==1 && Orders[0] == null)
            {
                return Content(HttpStatusCode.OK, Orders);
            }
            
            // Filter the list according to the input filter
            try
            {

                #region Orders Sorting

                if (String.IsNullOrWhiteSpace(filter.Sorting))
                {
                    Orders = Orders.OrderByDescending(i => i.LastEndUpdate).ToList();
                }
                else if (filter.Direction == "desc")
                {
                    switch (filter.Sorting)
                    {
                        case "customerNumber":
                            Orders = Orders.OrderByDescending(i => i.Customer).ToList();
                            break;
                        case "site":
                            Orders = Orders.OrderByDescending(i => i.Site).ToList();
                            break;
                        case "salesordernumber":
                            Orders = Orders.OrderByDescending(i => i.SalesOrderNumber).ToList();
                            break;
                        case "datecreated":
                            Orders = Orders.OrderByDescending(i => i.LastUpdatedDate).ToList();
                            break;
                        case "lastupdated":
                            Orders = Orders.OrderByDescending(i => i.LastEndUpdate).ToList();
                            break;
                        case "citynstate":
                            Orders = Orders.OrderByDescending(i => i.CityNState).ToList();
                            break;
                        case "username":
                            Orders = Orders.OrderByDescending(i => i.UserName).ToList();
                            break;

                        default:
                            Orders = Orders.OrderByDescending(i => i.OrderId).ToList();
                            break;
                    }
                }
                else
                {
                    switch (filter.Sorting)
                    {
                        case "customerNumber":
                            Orders = Orders.OrderBy(i => i.Customer).ToList();
                            break;
                        case "site":
                            Orders = Orders.OrderBy(i => i.Site).ToList();
                            break;
                        case "salesordernumber":
                            Orders = Orders.OrderBy(i => i.SalesOrderNumber).ToList();
                            break;
                        case "datecreated":
                            Orders = Orders.OrderBy(i => i.LastUpdatedDate).ToList();
                            break;
                        case "lastupdated":
                            Orders = Orders.OrderBy(i => i.LastEndUpdate).ToList();
                            break;
                        case "citynstate":
                            Orders = Orders.OrderByDescending(i => i.CityNState).ToList();
                            break;
                        case "username":
                            Orders = Orders.OrderByDescending(i => i.UserName).ToList();
                            break;
                        default:
                            Orders = Orders.OrderBy(i => i.OrderId).ToList();
                            break;
                    }
                }

                #endregion

                int OrdersCount = Orders.Count();
                int pages = OrdersCount / filter.PageSize + ((OrdersCount % filter.PageSize > 0) ? 1 : 0);

                FilteredOrdersListReturnVM result = new FilteredOrdersListReturnVM();
                result.Pages = pages;
                result.Orders = Orders.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

                return Content(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of Orders.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of Orders.");
            }

        }




    }
}
