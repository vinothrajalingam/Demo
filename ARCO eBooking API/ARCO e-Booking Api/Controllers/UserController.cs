using ARCO.EndPoint.eCard.VM;
using log4net;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace ARCO.EndPoint.eCard.Controllers
{
    [Authorize]
    [RoutePrefix("api/User")]
    public class UserController: ApiController
    {

        private static readonly ILog Log =
             LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();
        public UserController()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        [HttpGet]
        [Route("LoadUserListFromAD")]
        public IHttpActionResult LoadUserListFromAD(String filter)
        {
            if (filter == null)
            {
                return Content(HttpStatusCode.BadRequest, "Request body not valid");
            }

            try
            {
                List<ADUserVM> ADUsers = new List<ADUserVM>();
                DirectoryEntry directoryEntry = Domain.GetCurrentDomain().GetDirectoryEntry();
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);

                directorySearcher.PropertiesToLoad.Add("samaccountname");
                directorySearcher.PropertiesToLoad.Add("mail");
                directorySearcher.PropertiesToLoad.Add("usergroup");
                directorySearcher.PropertiesToLoad.Add("displayname");
                directorySearcher.PropertiesToLoad.Add("firstname");
                directorySearcher.PropertiesToLoad.Add("lastname");
                
                directorySearcher.Filter = "(&(objectClass=User) (displayname=*" + filter + "*))";

                SearchResultCollection searchResultCollection = directorySearcher.FindAll();
                foreach (SearchResult u in searchResultCollection)
                {
                    var user = new ADUserVM()
                    {
                        UserName = u?.Properties?.Contains("samaccountname") == true ? u?.Properties["samaccountname"][0]?.ToString() : String.Empty,
                        DisplayName = u?.Properties?.Contains("displayname") == true ? u?.Properties["displayname"][0]?.ToString() : String.Empty,
                        FirstName = u?.Properties?.Contains("firstname") == true ? u?.Properties["firstname"][0]?.ToString() : String.Empty,
                        LastName = u?.Properties?.Contains("lastname") == true ? u?.Properties["lastname"][0]?.ToString() : String.Empty,
                        Email = u?.Properties?.Contains("mail") == true ? u?.Properties["mail"][0]?.ToString() : String.Empty          
                    };
                    ADUsers.Add(user);
                }
                ADUsers.RemoveAll(a => a.Email == "");
                return Content(HttpStatusCode.OK, ADUsers);
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of users from AD.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of users from AD.");
            }

        }


        [HttpPost]
        [Route("AddUsertoDB")]
        public IHttpActionResult AddUsertoDB([FromBody] AddPrenoteUserVM filter)
        {
            if (filter == null)
            {
                return Content(HttpStatusCode.BadRequest, "Request body not valid");
            }

            try
            {
                var username = (User.Identity.Name).Split('@')[0].Trim();
                PrenotesUser NewUser = new PrenotesUser();
                NewUser.Admin = filter.Admin;
                NewUser.Deleted = false;
                NewUser.Email = filter.user.Email;
                NewUser.Manager = filter.Manager;
                NewUser.Username = filter.user.Username;
                NewUser.DefaultSite = filter.defaultsite;
                PrenotesUser _User = new PrenotesUser();
                _User = arcodb.PrenotesUsers.Where(a => a.Username == filter.user.Username && a.Deleted==false).FirstOrDefault();
                if (_User!=null)
                {
                    if (_User.Username == NewUser.Username)
                    {
                        return Content(HttpStatusCode.Forbidden, "User Already exists.");
                    }
                }
                

                NewUser.DisplayName = filter.user.DisplayName;
                NewUser.CreatedDate = DateTime.Now;
                NewUser.ModifiedDate = DateTime.Now;
                NewUser.CreatedBy = username;
                NewUser.ModifiedBy = username;
                
                arcodb.PrenotesUsers.Add(NewUser);
                arcodb.SaveChanges();

                UserSiteLocation bridge1 = new UserSiteLocation();
                bridge1.SiteLocationId = filter.defaultsite;
                bridge1.UserId = NewUser.Id;
                arcodb.UserSiteLocations.Add(bridge1);


                foreach (var id in filter.othersites)
                {
                    if (id.selected == true)
                    {
                        UserSiteLocation bridge = new UserSiteLocation();
                        bridge.SiteLocationId = id.SiteID;
                        bridge.UserId = NewUser.Id;
                        arcodb.UserSiteLocations.Add(bridge);
                    }
                    
                }



                arcodb.SaveChanges();

                return Content(HttpStatusCode.OK, "User added successfully");
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of users from AD.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of users from AD.");
            }
            
        }

        [HttpPost]
        [Route("UpdateUsertoDB")]
        public IHttpActionResult UpdateUsertoDB([FromBody] UpdateUserVM filter)
        {
            if (filter == null)
            {
                return Content(HttpStatusCode.BadRequest, "Request body not valid");
            }

            try
            {
                PrenotesUser UpdateUser = arcodb.PrenotesUsers.Where(a => a.Username == filter.user && a.Deleted == false).FirstOrDefault();
                UpdateUser.Admin = filter.admin;
                UpdateUser.Manager = filter.manager;
                UpdateUser.DefaultSite = filter.defaultsite;
                List<UserSiteLocation> UserOldLocation = arcodb.UserSiteLocations.Where(a => a.UserId==UpdateUser.Id).ToList();
                arcodb.UserSiteLocations.RemoveRange(UserOldLocation);
                



                //arcodb.PrenotesUsers.Add(UpdateUser);
                arcodb.SaveChanges();

                UserSiteLocation bridge1 = new UserSiteLocation();
                bridge1.SiteLocationId = filter.defaultsite;
                bridge1.UserId = UpdateUser.Id;
                arcodb.UserSiteLocations.Add(bridge1);
                foreach (var item in filter.othersites)
                {
                    if (item.selected == true)
                    {
                        UserSiteLocation bridge = new UserSiteLocation();
                        bridge.SiteLocationId = item.SiteID;
                        bridge.UserId = UpdateUser.Id;
                        arcodb.UserSiteLocations.Add(bridge);
                    }
                    
                }

                arcodb.SaveChanges();

                return Content(HttpStatusCode.OK, "User added successfully");
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of users from AD.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of users from AD.");
            }



        }


        


        [HttpGet]
        [Route("LoadUserListFromDB")]
        public IHttpActionResult LoadUserListFromDB()
        {
           

            try
            {
                var UsersinDB = (from user in arcodb.PrenotesUsers
                                     select new
                                     {
                                         Id = user.Id,
                                         Email = user.Email,
                                         UserName = user.Username,
                                     }).OrderBy(a => a.Id).ToList();

                return Content(HttpStatusCode.OK, UsersinDB);
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of users from AD.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of users from AD.");
            }
        }

        [HttpPost]
        [Route("LoadUserList")]
        public IHttpActionResult LoadUserList([FromBody] UserListVM filter)
        {
            if (filter == null || !ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, "Request body not valid");
            }
            ARCOeCardEntities arcodb = new eCard.ARCOeCardEntities();

            int siteid = Convert.ToInt32(filter.SiteId);
            //SiteLocation sitelocation = arcodb.SiteLocations.Where(s => s.ID == siteid).FirstOrDefault();
            List<UserListVM> UserList = (from User in arcodb.PrenotesUsers
                                         
                                         join sitename in arcodb.SiteLocations
                                         on User.DefaultSite equals sitename.ID
                                         where
                                             (String.IsNullOrEmpty(filter.UserName) || (User.Username.Contains(filter.UserName))) &&
                                             (filter.SiteId == 0 || User.DefaultSite == siteid) && User.Deleted==false && sitename.Status != false
                                         // (filter.CreatedDate == null || orders.CreatedDate >= filter.CreatedDate) &&
                                         //(filter.EndDate == null || note.CreatedDate.Date <= filter.EndDate.Value.Date)
                                         select new UserListVM
                                         {
                                             UserName = User.Username,
                                             Id = User.Id,
                                             Email = User.Email,
                                             SiteName = sitename.SiteLocation1,
                                             CreatedDate = User.CreatedDate,
                                             LastUpdatedDate = User.ModifiedDate,
                                         })
                                                    .ToList()
                                                    //.Where(a => filter.StatusForDate == false ? (filter.StartDate == null || a.StartDate.Value.Date >= filter.StartDate.Value.Date) : filter.StartDate == null || a.LastStartDate.Value.Date >= filter.StartDate.Value.Date).DefaultIfEmpty()
                                                    //.Where(a => filter.StatusForDate == false ? (filter.LastUpdatedDate == null || a.LastUpdatedDate.Value.Date <= filter.LastUpdatedDate.Value.Date) : filter.LastUpdatedDate == null || a.LastEndUpdate.Value.Date <= filter.LastUpdatedDate.Value.Date).DefaultIfEmpty()
                                                    .ToList();
            

            // Filter the list according to the input filter
            try
            {

                #region Orders Sorting

                if (String.IsNullOrWhiteSpace(filter.Sorting))
                {
                    UserList = UserList.OrderByDescending(i => i.LastUpdatedDate).ToList();
                }
                else if (filter.Direction == "desc")
                {
                    switch (filter.Sorting)
                    {
                        case "UserName":
                            UserList = UserList.OrderByDescending(i => i.UserName).ToList();
                            break;
                        case "SiteName":
                            UserList = UserList.OrderByDescending(i => i.SiteName).ToList();
                            break;
                        
                        case "lastupdated":
                            UserList = UserList.OrderByDescending(i => i.LastUpdatedDate).ToList();
                            break;
                       
                        default:
                            UserList = UserList.OrderByDescending(i => i.LastUpdatedDate).ToList();
                            break;
                    }
                }
                else
                {
                    switch (filter.Sorting)
                    {
                        case "UserName":
                            UserList = UserList.OrderByDescending(i => i.UserName).ToList();
                            break;
                        case "SiteName":
                            UserList = UserList.OrderByDescending(i => i.SiteName).ToList();
                            break;

                        case "lastupdated":
                            UserList = UserList.OrderByDescending(i => i.LastUpdatedDate).ToList();
                            break;

                        default:
                            UserList = UserList.OrderByDescending(i => i.LastUpdatedDate).ToList();
                            break;
                    }
                }

                #endregion

                int UserListCount = UserList.Count();
                int pages = UserListCount / filter.PageSize + ((UserListCount % filter.PageSize > 0) ? 1 : 0);

                UserListReturnVM result = new UserListReturnVM();
                result.Pages = pages;
                result.Users = UserList.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

                return Content(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of Orders.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of Orders.");
            }

        }

        [HttpGet]
        [Route("GetSiteLocationsforUser")]
        public IHttpActionResult GetSiteLocationsforUser()
        {

            try
            {
                var Sitelocations = (from site in arcodb.SiteLocations
                                     where site.Status != false
                                     select new
                                     {
                                         SiteID = site.ID,
                                         Site = site.SiteLocation1,
                                         selected = false
                                     }).OrderBy(a => a.Site).ToList();

                return Content(HttpStatusCode.OK, Sitelocations);
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while getting list of site locations for prenote system", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an error while getting list of site locations for prenote system");
            }
        }

      

        [HttpGet]
        [Route("LoadUser")]
        public IHttpActionResult LoadUser(int UserId)
        {


            try
            {
                
                var Sites1 = arcodb.UserSiteLocations.Where(a => a.UserId == UserId).ToList();
                var SitesLocs = arcodb.SiteLocations.Where(a=> a.Status != false).ToList();
                var PrenotesUsers = arcodb.PrenotesUsers.Where(a => a.Id == UserId).ToList();
                var temp = (from site in SitesLocs
                            select new
                            {
                                SiteID = site.ID,
                                Site = site.SiteLocation1,
                                SiteNumber = site.SiteNumber,
                                selected = Sites1.Select(a=> a.SiteLocationId).Contains(site.ID) ? true : false
                            }).Distinct().OrderBy(a=> a.SiteNumber).ToList();
                var User = (from user in PrenotesUsers
                            select new
                            {
                                Id = user.Id,
                                UserName = user.Username,
                                Email = user.Email,
                                Admin = user.Admin,
                                Manager = user.Manager,
                                DefaultSite = user.DefaultSite,
                                Sites = temp
                            }).FirstOrDefault();

                return Content(HttpStatusCode.OK, User);
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of users from Database.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of users from Database.");
            }
        }

        [Authorize]
        [HttpGet]
        [Route("CheckUserExist")]
        public IHttpActionResult CheckUserExist()
        {


            try
            {
                string Name = User.Identity.Name;
                string username = Name.Split('@')[0].Trim();
                var check = (from user in arcodb.PrenotesUsers
                             where user.Username == username && user.Deleted != true
                             select new
                             {
                                 Id = user.Id,
                                 Manager = user.Manager,
                                 Admin = user.Admin,
                                 Email = user.Email,
                                 DisplayName = user.DisplayName,
                                 UserName = user.Username,
                             }).FirstOrDefault();

                
                return Content(HttpStatusCode.OK, check);
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of users from AD.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of users from AD.");
            }
        }

       
        [HttpGet]
        [Route("DeleteUser")]
        public IHttpActionResult DeleteUser(int UserId)
        {
            try
            {
                PrenotesUser User = arcodb.PrenotesUsers.Where(a => a.Id == UserId).FirstOrDefault();
                User.Deleted = true;
                arcodb.SaveChanges();
                return Content(HttpStatusCode.OK, "Deleted");
            }
            catch (Exception ex)
            {
                Log.Error("There was an unexpected error while filtering a list of users from AD.", ex);
                return Content(HttpStatusCode.InternalServerError, "There was an unexpected error while filtering a list of users from AD.");
            }
        }



    }
}