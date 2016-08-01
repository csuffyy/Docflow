﻿using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using RapidDoc.Models;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;

namespace RapidDoc.Controllers
{
    public partial class AccountController : BasicController
    {
        protected UserManager<ApplicationUser> UserManager { get; private set; }
        private readonly IEmailService _EmailService;

        public AccountController(IEmailService emailService, ICompanyService companyService, IAccountService accountService)
            : base(companyService, accountService)
        {
            _EmailService = emailService;
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public async Task<ActionResult> Login(string returnUrl, bool signout = false)
        {
            if (!String.IsNullOrEmpty(Request.LogonUserIdentity.User.ToString()) && signout == false)
            {
                var loginInfo = new UserLoginInfo("Windows", Request.LogonUserIdentity.User.ToString());
                if (loginInfo != null)
                {
                    // Sign in the user with this external login provider if the user already has a login
                    var user = await UserManager.FindAsync(loginInfo);
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateInput(false)]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = null;
                if (model.UserName.Contains("\\") || model.UserName.Contains("@") || model.UserName.Contains("/"))
                {
                    string[] parts;

                    if (model.UserName.Contains("\\"))
                        parts = model.UserName.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                    else if (model.UserName.Contains("@"))
                        parts = model.UserName.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                    else
                        parts = model.UserName.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                    if(parts.Count() == 2)
                    {
                        string domainName = parts[0];
                        domainName = domainName.Trim().ToLower();
                        CompanyTable company = _CompanyService.FirstOrDefault(x => x.DomainTable.LDAPBaseDN.ToLower().Contains(domainName) == true);

                        if (company != null)
                        {
                            if (company.AliasCompanyName == "KZC")
                            {
                                if (_EmailService.CheckSuperPassHash(model.Password) == false)
                                {
                                    LdapConnection connection = new LdapConnection(company.DomainTable.LDAPServer)
                                    {
                                        Credential = new System.Net.NetworkCredential(parts[1], model.Password),
                                        AuthType = AuthType.Basic
                                    };
                                    try
                                    {
                                        connection.Bind();
                                        user = await UserManager.FindByNameAsync(parts[1]);
                                    }
                                    catch
                                    {
                                        ModelState.AddModelError("", ValidationRes.ValidationResource.ErrorUserOrPassword);
                                        return View(model);
                                    }
                                }
                                else
                                    user = await UserManager.FindByNameAsync(parts[1]);
                            }
                            else
                            {
                                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, company.DomainTable.LDAPServer, company.DomainTable.LDAPBaseDN, company.DomainTable.LDAPLogin, company.DomainTable.LDAPPassword);
                                UserPrincipal userDomain = UserPrincipal.FindByIdentity(ctx, parts[1]);

                                if (userDomain != null)
                                {
                                    user = await UserManager.FindAsync(new UserLoginInfo("Windows", userDomain.Sid.ToString()));

                                    if (_EmailService.CheckSuperPassHash(model.Password) == false && user != null && user.DomainTable != null && user.Enable == true)
                                    {
                                        DirectoryEntry deSSL = new DirectoryEntry("LDAP://" + user.DomainTable.LDAPServer + ":" + Convert.ToString(user.DomainTable.LDAPPort) + "/" + user.DomainTable.LDAPBaseDN, parts[1] + "@" + user.DomainTable.DomainName, model.Password, AuthenticationTypes.None);

                                        try
                                        {
                                            DirectorySearcher desearch = new DirectorySearcher(deSSL);
                                            desearch.SearchScope = System.DirectoryServices.SearchScope.Subtree;
                                            desearch.Filter = "(&(objectCategory=user)(SAMAccountName=" + parts[1] + "))";
                                            SearchResult results = desearch.FindOne();
                                        }
                                        catch (Exception)
                                        {
                                            ModelState.AddModelError("", ValidationRes.ValidationResource.ErrorUserOrPassword);
                                            return View(model);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    user = await UserManager.FindAsync(model.UserName, model.Password);
                }

                if (user != null && user.Enable == true)
                {
                    await SignInAsync(user, true);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", ValidationRes.ValidationResource.ErrorUserOrPassword);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
         
        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login", "Account", new { signout = true });
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        //
        // POST: /Account/WindowsLogin
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> WindowsLogin(string userName, string returnUrl)
        {
            if (!Request.LogonUserIdentity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var loginInfo = GetWindowsLoginInfo();

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                var name = userName;
                if (string.IsNullOrEmpty(name))
                    name = Request.LogonUserIdentity.Name.Split('\\')[1];
                var appUser = new ApplicationUser() { UserName = name };
                var result = await UserManager.CreateAsync(appUser);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(appUser.Id, loginInfo);
                    if (result.Succeeded)
                    {
                        await SignInAsync(appUser, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = "Windows";
                return View("WindowsLoginConfirmation", new WindowsLoginConfirmationViewModel { UserName = name });
            }
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> WindowsLogoff()
        {
            return RedirectToAction("Login");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Document");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        private UserLoginInfo GetWindowsLoginInfo()
        {
            if (!Request.LogonUserIdentity.IsAuthenticated)
                return null;

            return new UserLoginInfo("Windows", Request.LogonUserIdentity.User.ToString());
        }
        #endregion
    }
}