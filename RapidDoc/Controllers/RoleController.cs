using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Grids;
using RapidDoc.Models.Services;

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "Administrator, SetupAdministrator")]
    public class RoleController : BasicController
    {
        private readonly ISystemService _SystemService;
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public RoleManager<ApplicationRole> RoleManager { get; private set; }

        public RoleController(ISystemService systemService, ICompanyService companyService, IAccountService accountService)
            : base(companyService, accountService)
        {
            _SystemService = systemService;
            ApplicationDbContext dbContext = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
            RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(dbContext));
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Grid()
        {
            var items = Mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(RoleManager.Roles);
            var grid = new RoleAjaxPagingGrid(items.OrderByDescending(x => x.RoleType).ThenBy(y => y.Name), 1, false);
            return PartialView("_RoleGrid", grid);
        }

        public JsonResult GetRoleList(int page)
        {
            var items = Mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(RoleManager.Roles);
            var grid = new RoleAjaxPagingGrid(items.OrderByDescending(x => x.RoleType).ThenBy(y => y.Name), page, true);

            return Json(new
            {
                Html = RenderPartialViewToString("_RoleGrid", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            ViewBag.CompanyList = _CompanyService.GetDropListCompanyNull(null);
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(RoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var domainModel = new ApplicationRole(viewModel.Name, viewModel.Description, viewModel.RoleType, viewModel.CompanyTableId);
                var roleresult = await RoleManager.CreateAsync(domainModel);
                if (!roleresult.Succeeded)
                {
                    ModelState.AddModelError("", roleresult.Errors.First().ToString());
                    return View();
                }
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.CompanyList = _CompanyService.GetDropListCompanyNull(viewModel.CompanyTableId);
                return View();
            }
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var domainModel = await RoleManager.FindByIdAsync(id.ToString());
            if (domainModel == null)
            {
                return HttpNotFound();
            }

            var viewModel = Mapper.Map<ApplicationRole, RoleViewModel>(domainModel);
            ViewBag.CompanyList = _CompanyService.GetDropListCompanyNull(viewModel.CompanyTableId);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var domainModel = await RoleManager.FindByIdAsync(viewModel.Id);
                if (domainModel == null)
                {
                    return HttpNotFound();
                }

                domainModel.Name = viewModel.Name;
                domainModel.Description = viewModel.Description;
                domainModel.RoleType = viewModel.RoleType;
                domainModel.CompanyTableId = viewModel.CompanyTableId;
                var result = await RoleManager.UpdateAsync(domainModel);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First().ToString());
                    ViewBag.CompanyList = _CompanyService.GetDropListCompanyNull(viewModel.CompanyTableId);
                    return View();
                }
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.CompanyList = _CompanyService.GetDropListCompanyNull(viewModel.CompanyTableId);
                return View();
            }
        }

        public async Task<ActionResult> AddUsers(string id)
        {
            var roleTable = await RoleManager.FindByIdAsync(id);
            if (roleTable == null)
            {
                return HttpNotFound();
            }

            var users = Mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<UserViewModel>>(UserManager.Users);

            foreach (var user in users)
            {
                foreach (var userRole in roleTable.Users)
                {
                    if (userRole.UserId == user.Id)
                    {
                        user.isRoleUser = true;
                    }
                }
            }

            ViewBag.RoleId = id;
            return View(users);
        }

        public async Task<ActionResult> AddUsersManual(string id)
        {
            var roleTable = await RoleManager.FindByIdAsync(id);
            if (roleTable == null)
            {
                return HttpNotFound();
            }

            string usersManual = String.Empty;
            var roleUsers = roleTable.Users.Where(x => x.RoleId == id);
            foreach(var item in roleUsers)
            {
                var user = UserManager.FindById(item.UserId);
                usersManual += String.Format("{0},{1} ( {2} ),", user.Id, user.UserName, user.Email);
            }

            ViewBag.RoleId = id;
            ViewBag.Users = usersManual;
            ViewBag.RoleName = roleTable.Name;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddUsers(string id, string[] listdata, bool? isAjax)
        {
            var roleTable = await RoleManager.FindByIdAsync(id);

            if (roleTable == null)
                return HttpNotFound();

            if (isAjax == true)
            {
                string[] roleUsers = roleTable.Users.Select(x => x.UserId).ToArray();
                List<string> newUsers = new List<string>();
                List<string> removeUsers = new List<string>();

                if (listdata != null)
                {
                    newUsers.AddRange(listdata.Except(roleUsers));
                    removeUsers.AddRange(roleUsers.Except(listdata));
                }
                else
                {
                    removeUsers.AddRange(roleUsers);
                }

                foreach (var userid in newUsers)
                    UserManager.AddToRole(userid, roleTable.Name);

                foreach (var userid in removeUsers)
                    UserManager.RemoveFromRole(userid, roleTable.Name);
            }

            return Json(new { result = "Redirect", url = Url.Action("Index") });
        }

        [HttpPost]
        public async Task<ActionResult> AddUsersManual(string id, string Users)
        {
            var roleTable = await RoleManager.FindByIdAsync(id);
            List<UserViewModel> usersOfRole = new List<UserViewModel>();
            if (roleTable == null)
            {
                return HttpNotFound();
            }
            
            var allUsers = Mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<UserViewModel>>(UserManager.Users);

            RoleManager.Roles.Where(z => z.Id == id).FirstOrDefault().Users.ToList().ForEach( y => 
            usersOfRole.Add(allUsers.FirstOrDefault(x => x.Id == y.UserId))
                );

            if (usersOfRole.Count > 0)
            {
                foreach (var user in usersOfRole)
                {
                    UserManager.RemoveFromRole(user.Id, roleTable.Name);
                }
            }

            if(!String.IsNullOrEmpty(Users))
            {
                string[] listdata = _SystemService.GuidsFromText(Users);

                foreach (string userId in listdata)
                {
                    var userTable = await UserManager.FindByIdAsync(userId);
                    if (userTable == null)
                    {
                        return HttpNotFound();
                    }

                    UserManager.AddToRole(userTable.Id, roleTable.Name);
                }
            }

            string usersManual = String.Empty;
            var roleUsers = roleTable.Users.Where(x => x.RoleId == id);
            foreach (var item in roleUsers)
            {
                var user = UserManager.FindById(item.UserId);
                usersManual += String.Format("{0},{1} ({2}),", user.Id, user.UserName, user.Email);
            }

            ViewBag.RoleId = id;
            ViewBag.Users = usersManual;
            ViewBag.RoleName = roleTable.Name;
            return View();
        }

        [AllowAnonymous]
        public ActionResult RoleLookup(string prefix)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            var items = Mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(RoleManager.Roles.Where(x => x.CompanyTableId == user.CompanyTableId && (x.RoleType == Models.Repository.RoleType.Group || x.RoleType == Models.Repository.RoleType.GroupOrder)).OrderBy(x => x.Name));
            ViewBag.PrefixOfficeMemo = prefix;
            return PartialView("_RoleLookup", items);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            if (disposing && RoleManager != null)
            {
                RoleManager.Dispose();
                RoleManager = null;
            }
            base.Dispose(disposing);
        }
	}
}