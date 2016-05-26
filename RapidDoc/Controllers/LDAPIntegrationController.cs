using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Repository;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;

namespace RapidDoc.Controllers
{
    public class LDAPIntegrationController : ApiController
    {
        protected readonly ICompanyService _CompanyService;
        protected readonly IEmplService _EmplService;
        protected readonly IEmailService _Emailservice;
        protected readonly IWorkScheduleService _WorkScheduleService;
        protected readonly ITitleService _TitleService;
        protected readonly IDepartmentService _DepartmentService;
        protected readonly IItemCauseService _ItemCauseService;
        protected readonly IPortalParametersService _PortalParametersService;

        public LDAPIntegrationController(ICompanyService companyService, IEmailService emailservice, IEmplService emplService, IWorkScheduleService workScheduleService, ITitleService titleService, IDepartmentService departmentService, IItemCauseService itemCauseService, IPortalParametersService portalParametersService)
        {
            _CompanyService = companyService;
            _Emailservice = emailservice;
            _EmplService = emplService;
            _WorkScheduleService = workScheduleService;
            _TitleService = titleService;
            _DepartmentService = departmentService;
            _ItemCauseService = itemCauseService;
            _PortalParametersService = portalParametersService;
        }

        public void Get()
        {
            try
            {
                var companies = _CompanyService.GetAll().ToList();
                foreach (var company in companies)
                {
                    if (company.AliasCompanyName == "KZC")
                    {
                        getUsersDepartmentKZC(company, company.DomainTable.LDAPBaseDN);
                        getUsersDepartmentKZC(company, "OU=lgok,O=ustk,C=kz");
                        getUsersDepartmentKZC(company, "OU=zgok,O=ustk,C=kz");
                    }
                    else
                    {
                        BuildTreeLDAP(company, company.DomainTable.LDAPBaseDN, "");
                        BuildTreeLDAP(company, company.DomainTable.LDAPBaseDN, "", true);
                        CheckActiveUsers(company);
                    }

                    DeleteNotUsedDepartment(company);
                }

                DeleteNotUsedTitle();
            }
            catch (Exception e)
            {
                _Emailservice.SendStatusExecutionBatch(String.Format("Procedure LDAP Integration was failed. Message is ({0}).", e.Message), true);
            }
        }

        private void BuildTreeLDAP(CompanyTable _item, string _LDAPpath, string _parentDepartmentName = "", bool afterUpdate = false)
        {
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + _item.DomainTable.LDAPServer + ":" + Convert.ToString(_item.DomainTable.LDAPPort) + "/" + _LDAPpath, _item.DomainTable.LDAPLogin, _item.DomainTable.LDAPPassword, AuthenticationTypes.None);
            DirectorySearcher ds = new DirectorySearcher(entry);

            ds.SearchScope = SearchScope.OneLevel;
            ds.Filter = "(&(objectCategory=organizationalUnit)(!(name=Computers))(!(name=Disable))(!(name=Improvers))(!(name=Service&App))(!(name=Labs))(!(name=Servers))(!(name=Users))(!(name=User))(!(name=Groups))(!(name=Disabled)))";
            ds.PropertiesToLoad.Add("distinguishedname");
            ds.PropertiesToLoad.Add("name");

            foreach (SearchResult result in ds.FindAll())
            {
                if (result.Properties.Contains("distinguishedname") && result.Properties.Contains("name"))
                {
                    Guid depId = Guid.Empty;
                    if (afterUpdate == false)
                    {
                        depId = DepartmentIntegration(result.Properties["name"][0].ToString(), _parentDepartmentName, _item.Id);
                    }
                    getUsersDepartment(_item, result.Properties["distinguishedname"][0].ToString(), depId, afterUpdate);
                    BuildTreeLDAP(_item, result.Properties["distinguishedname"][0].ToString(), result.Properties["name"][0].ToString(), afterUpdate);
                }
            }
        }

        private void getUsersDepartment(CompanyTable _item, string _LDAPpath, Guid _department, bool afterUpdate)
        {
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + _item.DomainTable.LDAPServer + ":" 
                + Convert.ToString(_item.DomainTable.LDAPPort) + "/" 
                + _LDAPpath, _item.DomainTable.LDAPLogin, _item.DomainTable.LDAPPassword, AuthenticationTypes.None);
            DirectorySearcher ds = new DirectorySearcher(entry);
            string ldapData;
            Guid titleId = Guid.Empty;

            ds.SearchScope = SearchScope.OneLevel;
            ds.Filter = "(&(objectCategory=user)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
            ds.PropertiesToLoad.Add("title");
            ds.PropertiesToLoad.Add("mail");
            ds.PropertiesToLoad.Add("telephonenumber");
            ds.PropertiesToLoad.Add("mobile");
            ds.PropertiesToLoad.Add("description");
            ds.PropertiesToLoad.Add("cn");
            ds.PropertiesToLoad.Add("sn");
            ds.PropertiesToLoad.Add("manager");
            ds.PropertiesToLoad.Add("sAMAccountName");
            ds.PropertiesToLoad.Add("objectGUID");

            foreach (SearchResult result in ds.FindAll())
            {
                if (result.Properties.Contains("title") && afterUpdate == false)
                {
                    ldapData = result.Properties["title"][0].ToString();
                    titleId = TitleIntegration(ldapData);
                }

                if (result.Properties.Contains("description") && result.Properties.Contains("mail"))
                {
                    string telephone = string.Empty;
                    string mobile = string.Empty;
                    string mail = string.Empty;
                    string userid = string.Empty;
                    string userName = string.Empty;
                    string manager = string.Empty;
                    string[] emplName = new string[3];
                    byte[] GlobalId = null;
                    Guid GlobalGuid = Guid.Empty;

                    ldapData = result.Properties["description"][0].ToString();
                    mail = result.Properties["mail"][0].ToString();
                    emplName = DecodeLDAPNameUsers(ldapData, 0);

                    if (result.Properties.Contains("telephonenumber"))
                    {
                        telephone = result.Properties["telephonenumber"][0].ToString();
                    }
                    if (result.Properties.Contains("mobile"))
                    {
                        mobile = result.Properties["mobile"][0].ToString();
                    }
                    if (result.Properties.Contains("cn"))
                    {
                        userName = result.Properties["cn"][0].ToString();
                    }
                    if (result.Properties.Contains("sAMAccountName"))
                    {
                        userid = result.Properties["sAMAccountName"][0].ToString();
                    }
                    if (result.Properties.Contains("manager"))
                    {
                        manager = result.Properties["manager"][0].ToString();
                    }
                    if (result.Properties.Contains("objectGUID"))
                    {
                        GlobalId = (byte[])result.Properties["objectGUID"][0];
                        GlobalGuid = new Guid(GlobalId);
                    }

                    if (emplName[0] != null && emplName[1] != null && !String.IsNullOrEmpty(userid) && GlobalGuid != Guid.Empty)
                    {
                        if (afterUpdate == true)
                        {
                            try
                            {
                                string managerUserId = GetManagerUserId(manager, _item);

                                if (!String.IsNullOrEmpty(managerUserId))
                                    EmplUpdateIntegration(GlobalGuid, emplName[1], emplName[0], emplName[2], _item.Id, managerUserId);
                            }
                            catch 
                            { 
                                continue; 
                            }
                        }
                        else
                        {
                            try
                            {
                                string managerUserId = GetManagerUserId(manager, _item);

                                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, _item.DomainTable.LDAPServer, _item.DomainTable.LDAPBaseDN, _item.DomainTable.LDAPLogin, _item.DomainTable.LDAPPassword);
                                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userid);
                                string domainSID = user.Sid.ToString();

                                String ApplicationUserId = UserIntegration(userid, mail, domainSID, _item);
                                EmplIntegration(GlobalGuid, emplName[1], emplName[0], emplName[2], telephone, mobile, ApplicationUserId, _department, titleId, _item.Id, managerUserId);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private string GetManagerUserId(string manager, CompanyTable _item)
        {
            DirectoryEntry entryManager = new DirectoryEntry("LDAP://" + _item.DomainTable.LDAPServer + ":"
                + Convert.ToString(_item.DomainTable.LDAPPort) + "/"
                + manager, _item.DomainTable.LDAPLogin, _item.DomainTable.LDAPPassword, AuthenticationTypes.None);

            if (entryManager.Properties.Contains("sAMAccountName"))
            {
                string ManagerUserLogin = entryManager.Properties["sAMAccountName"].Value.ToString();

                ApplicationDbContext context = new ApplicationDbContext();
                var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var userTable = um.FindByName(ManagerUserLogin);
                return userTable != null ? userTable.Id : String.Empty;
            }

            return String.Empty;
        }

        private void getUsersDepartmentKZC(CompanyTable _item, string _LDAPpath)
        {
            List<KZCInfoData_View> infoDataList = new List<KZCInfoData_View>();
            string connectionString = "Server = 192.168.22.20; Database = webinternal; Uid = userinfo; Pwd = U$er!nf0;";

            var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            connection.Open();
            using (MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM webinternal.view_User;", connection))
            {
                using (MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        infoDataList.Add(new KZCInfoData_View {
                            Login = reader["Login"].ToString().ToLower(),
                            Surname = reader["Surname"].ToString(),
                            Name = reader["Name"].ToString(),
                            SecondName = reader["SecondName"].ToString(),
                            Email = reader["Email"].ToString().ToLower(),
                            Subdivision = reader["Subdivision"].ToString(),
                            Position = reader["Position"].ToString()
                        });
                    }
                }
            }

            DirectoryEntry entry = new DirectoryEntry("LDAP://" + _item.DomainTable.LDAPServer + ":"
                + Convert.ToString(_item.DomainTable.LDAPPort) + "/"
                + _LDAPpath, _item.DomainTable.LDAPLogin, _item.DomainTable.LDAPPassword, AuthenticationTypes.None);
            DirectorySearcher ds = new DirectorySearcher(entry);
            string ldapData;

            ds.SearchScope = SearchScope.OneLevel;
            ds.Filter = "(&(objectClass=*))";
            ds.PropertiesToLoad.Add("mail");
            ds.PropertiesToLoad.Add("cn");
            ds.PropertiesToLoad.Add("uid");
            ds.PropertiesToLoad.Add("companyname");
            ds.PropertiesToLoad.Add("dominocertificate");

            foreach (SearchResult result in ds.FindAll())
            {
                if (result.Properties.Contains("cn") && result.Properties.Contains("mail") && result.Properties.Contains("companyname") && result.Properties.Contains("uid") && result.Properties.Contains("dominocertificate"))
                {
                    string mail = string.Empty;
                    string userid = string.Empty;
                    string userName = string.Empty;
                    string companyName = string.Empty;
                    string dominocertificate = string.Empty;
                    string GlobalId = String.Empty;
                    Guid GlobalGuid = Guid.Empty;

                    companyName = result.Properties["companyname"][0].ToString();

                    if (companyName == "Ustkam ID" || companyName == "Ustkam MK" || companyName == "Astana" || companyName == "Almaty"
                        || companyName == "Kazzinc-GEO" || companyName == "Ustkam ID, Kormin" || companyName == "KMTK" || companyName == "RGOK"
                        || companyName == "TOO Kazzinc-Remservice" || companyName == "PK Kazzinc-Avtomatica")
                    {
                        ldapData = result.Properties["cn"][0].ToString();
                        mail = result.Properties["mail"][0].ToString();
                        userid = result.Properties["uid"][0].ToString();
                        GlobalId = result.Properties["dominocertificate"][0].ToString();
                        GlobalId = GlobalId.Replace(" ", "");

                        using (MD5 md5 = MD5.Create())
                        {
                            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(GlobalId));
                            GlobalGuid = new Guid(hash);
                        }

                        if (!String.IsNullOrEmpty(userid))
                        {
                            String ApplicationUserId = UserIntegration(userid, mail, String.Empty, _item);

                            var infoItem = infoDataList.FirstOrDefault(x => x.Login == userid.ToLower());

                            if (infoItem == null) continue;

                            Guid? depId = null;
                            Guid titleId = Guid.Empty;

                            if (!String.IsNullOrEmpty(infoItem.Position))
                            {
                                if (!String.IsNullOrEmpty(infoItem.Subdivision))
                                    depId = DepartmentIntegration(infoItem.Subdivision, String.Empty, _item.Id);

                                titleId = TitleIntegration(infoItem.Position);
                                EmplIntegration(GlobalGuid, infoItem.Name, infoItem.Surname, infoItem.SecondName, String.Empty, String.Empty, ApplicationUserId, depId, titleId, _item.Id, String.Empty);
                            }
                        }
                    }
                }
            }
        }

        private void EmplUpdateIntegration(Guid _globalId, string _firstname, string _secondname, string _middlename, Guid _company, string _managerUserId)
        {
            Guid? manageId = null;

            if (_managerUserId != String.Empty)
            {
                if (_EmplService.Contains(x => x.CompanyTableId == _company
                    && x.ApplicationUserId == _managerUserId && x.Enable == true))
                {
                    manageId = _EmplService.FirstOrDefault(x => x.CompanyTableId == _company
                        && x.ApplicationUserId == _managerUserId && x.Enable == true).Id;

                    if (_EmplService.Contains(x => x.CompanyTableId == _company
                        && x.LDAPGlobalId == _globalId))
                    {
                        EmplTable empl = _EmplService.FirstOrDefault(x => x.CompanyTableId == _company
                            && x.LDAPGlobalId == _globalId);

                        empl.ManageId = manageId;
                        _EmplService.SaveDomain(empl, "Admin", _company);
                    }
                }
            }
        }

        private void EmplIntegration(Guid _globalId, string _firstname, string _secondname, string _middlename, string _workphone, string _mobilephone, string _userId, Guid? _departmentId, Guid _titleId, Guid _company, string _managerUserId)
        {
            Guid? manageId = null;

            if(_managerUserId != String.Empty)
            {
                if (_EmplService.Contains(x => x.CompanyTableId == _company
                    && x.ApplicationUserId == _managerUserId && x.Enable == true))
                {
                    manageId = _EmplService.FirstOrDefault(x => x.CompanyTableId == _company
                        && x.ApplicationUserId == _managerUserId && x.Enable == true).Id;
                }
            }

            //MIGRATION CODE
            /*
            if (_EmplService.Contains(x => x.CompanyTableId == _company
                && x.FirstName == _firstname && x.SecondName == _secondname && x.MiddleName == _middlename))
            {
                EmplTable empl = _EmplService.FirstOrDefault(x => x.CompanyTableId == _company
                    && x.FirstName == _firstname && x.SecondName == _secondname && x.MiddleName == _middlename);

                empl.LDAPGlobalId = _globalId;
                _EmplService.SaveDomain(empl, "Admin", _company);
            }
            */
            if (!_EmplService.Contains(x => x.CompanyTableId == _company
                && x.LDAPGlobalId == _globalId))
            {
                if (_titleId != Guid.Empty && !String.IsNullOrEmpty(_firstname))
                {
                    _EmplService.SaveDomain(new EmplTable()
                    {
                        isIntegratedLDAP = true,
                        LDAPGlobalId = _globalId,
                        FirstName = _firstname,
                        SecondName = _secondname,
                        MiddleName = _middlename,
                        ApplicationUserId = _userId,
                        DepartmentTableId = _departmentId,
                        TitleTableId = _titleId,
                        CompanyTableId = _company,
                        WorkScheduleTableId = _WorkScheduleService.FirstOrDefault(x => x.Id != null).Id,
                        ManageId = manageId,
                        Enable = true
                    }, "Admin", _company);
                }
            }
            else
            {
                if (_titleId != Guid.Empty)
                {
                    EmplTable empl = _EmplService.FirstOrDefault(x => x.CompanyTableId == _company
                        && x.LDAPGlobalId == _globalId);

                    empl.FirstName = _firstname;
                    empl.SecondName = _secondname;
                    empl.MiddleName = _middlename;
                    empl.DepartmentTableId = _departmentId;
                    empl.TitleTableId = _titleId;
                    empl.isIntegratedLDAP = true;
                    empl.ManageId = manageId;
                    _EmplService.SaveDomain(empl, "Admin", _company);
                }
            }
        }

        private string UserIntegration(string _userId, string _email, string _sid, CompanyTable _item)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var domainModel = context.Users.FirstOrDefault(x => x.DomainTableId == _item.DomainTableId && x.AccountDomainName == _userId && x.isDomainUser == true);
            var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if(domainModel == null)
            {
                string userNameLocal = _userId;
                if(context.Users.Any(x => x.UserName == _userId))
                {
                    userNameLocal = GetRightLocalUserName(context, userNameLocal);
                }

                try
                {
                    domainModel = new ApplicationUser();
                    domainModel.UserName = userNameLocal;
                    domainModel.Email = _email;
                    domainModel.TimeZoneId = "Central Asia Standard Time";
                    domainModel.Lang = "ru-RU";
                    domainModel.isDomainUser = true;
                    domainModel.CompanyTableId = _item.Id;
                    domainModel.DomainTableId = _item.DomainTableId;
                    domainModel.AccountDomainName = _userId;
                    domainModel.Enable = true;
                    um.Create(domainModel);

                    if (!String.IsNullOrEmpty(_sid))
                    {
                        var loginInfo = new UserLoginInfo("Windows", _sid);
                        um.AddLogin(domainModel.Id, loginInfo);
                    }
                    um.AddToRole(domainModel.Id, "ActiveUser");

                    domainModel = context.Users.FirstOrDefault(x => x.DomainTableId == _item.DomainTableId && x.AccountDomainName == _userId && x.isDomainUser == true);
                }
                catch
                {
                   
                }
            }
            else
            {
                try
                {
                    //MIGRATION CODE
                    /*
                    um.RemoveLogin(domainModel.Id, new UserLoginInfo("Windows", domainModel.Logins.FirstOrDefault().ProviderKey));
                    var loginInfo = new UserLoginInfo("Windows", _sid);
                    um.AddLogin(domainModel.Id, loginInfo);
                    */

                    domainModel.Email = _email;
                    um.Update(domainModel);
                }
                catch
                {
                    //domainModel = um.FindByName(_userId);
                    domainModel = context.Users.FirstOrDefault(x => x.DomainTableId == _item.DomainTableId && x.AccountDomainName == _userId && x.isDomainUser == true);
                }
            }
            return domainModel == null ? "" : domainModel.Id;
        }

        private string GetRightLocalUserName(ApplicationDbContext context, string userName)
        {
            int num = 1;
            if (context.Users.Any(x => x.UserName == userName))
            {
                num++;
                userName = userName + num.ToString();
                userName = GetRightLocalUserName(context, userName);
            }

            return userName;
        }

        private Guid TitleIntegration(string _title)
        {
            _title = _title.Replace(",", "");
            if (!_TitleService.Contains(x => x.TitleName == _title))
            {
                _TitleService.SaveDomain(new TitleTable() { TitleName = _title, isIntegratedLDAP = true }, "Admin");
            }

            TitleTable titleTable = _TitleService.FirstOrDefault(x => x.TitleName == _title);
            return titleTable == null ? Guid.Empty : titleTable.Id;
        }

        private Guid DepartmentIntegration(string _department, string _parentDepartmentName, Guid _companyId)
        {
            Guid? guid = null;
            _department = _department.Trim();

            if (!_DepartmentService.Contains(x => x.DepartmentName == _department && x.CompanyTableId == _companyId))
            {
                if (_parentDepartmentName != string.Empty)
                {
                    if (_DepartmentService.Contains(x => x.CompanyTableId == _companyId && x.DepartmentName == _parentDepartmentName))
                        guid = _DepartmentService.FirstOrDefault(x => x.CompanyTableId == _companyId && x.DepartmentName == _parentDepartmentName).Id;
                }

                _DepartmentService.SaveDomain(new DepartmentTable() { DepartmentName = _department, ParentDepartmentId = guid, CompanyTableId = _companyId }, "Admin", _companyId);
            }

            DepartmentTable department = _DepartmentService.FirstOrDefault(x => x.DepartmentName == _department && x.CompanyTableId == _companyId);

            if (department != null && department.ParentDepartmentName != _parentDepartmentName && _parentDepartmentName != string.Empty)
            {
                Guid parentGuid = _DepartmentService.FirstOrDefault(x => x.CompanyTableId == _companyId && x.DepartmentName == _parentDepartmentName).Id;
                department.ParentDepartmentId = parentGuid;
                _DepartmentService.SaveDomain(department, "Admin", _companyId);
            }

            guid = department.Id;
            return guid == null ? Guid.Empty : (Guid)guid;
        }

        private String[] DecodeLDAPNameUsers(string _name, int _type = 0)
        {
            String[] results = new string[3];

            if (_name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Count() > 1)
            {
                var list = new List<string>();
                list = _name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (list.Count == 2)
                {
                    list.Add(string.Empty);
                }

                switch (_type)
                {
                    case 0: //SecondName FirstName MiddleName
                        results[0] = list[0];
                        results[1] = list[1];
                        results[2] = list[2];
                        break;
                    case 1: //FirstName SecondName MiddleName 
                        results[0] = list[1];
                        results[1] = list[0];
                        results[2] = list[2];
                        break;
                }
            }

            return results;
        }

        private void CheckActiveUsers(CompanyTable _item)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            foreach (ApplicationUser user in um.Users.Where(x => x.isDomainUser == true && x.DomainTableId == _item.DomainTableId).ToList())
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + _item.DomainTable.LDAPServer + ":"
                + Convert.ToString(_item.DomainTable.LDAPPort)
                , _item.DomainTable.LDAPLogin, _item.DomainTable.LDAPPassword, AuthenticationTypes.None);
                DirectorySearcher ds = new DirectorySearcher(entry);

                ds.Filter = "(SAMAccountName=" + user.UserName + ")";

                try
                {
                    SearchResult results = ds.FindOne();
                    if (results.ToString() != "")
                    {
                        int flags = Convert.ToInt32(results.Properties["userAccountControl"][0].ToString());
                        if(Convert.ToBoolean(flags & 0x0002))
                        {
                            if (user.Enable == true)
                            {
                                var allRoles = context.Roles.ToList();
                                foreach (var role in allRoles)
                                {
                                    um.RemoveFromRole(user.Id, role.Name);
                                }

                                user.Enable = false;
                                um.Update(user);
                            }

                            var emplTables = _EmplService.GetPartialIntercompany(x => x.Enable == true && x.ApplicationUserId == user.Id).ToList();

                            if (emplTables != null)
                            {
                                foreach (var empl in emplTables)
                                {
                                    empl.Enable = false;
                                    _EmplService.SaveDomain(empl, "Admin", empl.CompanyTableId);
                                }
                            }
                        }
                        /*
                        else
                        {
                            if (user.Enable == false)
                            {
                                um.AddToRole(user.Id, "ActiveUser");

                                user.Enable = true;
                                um.Update(user);

                                var emplTables = _EmplService.GetPartialIntercompany(x => x.Enable == false && x.ApplicationUserId == user.Id).ToList();
                                if (emplTables != null)
                                {
                                    foreach (var empl in emplTables)
                                    {
                                        empl.Enable = true;
                                        _EmplService.SaveDomain(empl, "Admin", empl.CompanyTableId);
                                    }
                                }
                            }
                        }
                        */
                    }
                    ds.Dispose();
                    entry.Dispose();
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void DeleteNotUsedTitle()
        {
            var titles = _TitleService.GetPartialIntercompany(x => x.isIntegratedLDAP == true).ToList();

            foreach (var title in titles)
            {
                if (!_EmplService.Contains(x => x.TitleTableId == title.Id))
                {
                    _TitleService.Delete(title.Id);
                }
            }
        }

        private void DeleteNotUsedDepartment(CompanyTable _item)
        {
            PortalParametersTable portalParameters = _PortalParametersService.GetPartialIntercompany(x => x.CompanyTableId == _item.Id).FirstOrDefault();
            var departments = _DepartmentService.GetPartialIntercompany(x => x.DepartmentName != null).ToList();

            foreach (var department in departments)
            {
                if (department.ParentDepartmentId == null && !_EmplService.Contains(x => x.DepartmentTableId == department.Id) && !_ItemCauseService.Contains(x => x.DepartmentTableId == department.Id) && 
                    (portalParameters == null || !portalParameters.ReportDepartments.Contains(department.Id.ToString())))
                {
                    _DepartmentService.Delete(department.Id);
                }
            }
        }
    }
}