using Newtonsoft.Json;
using RapidDoc.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using RapidDoc.Models.Infrastructure;
using System.Globalization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RapidDoc.Controllers
{
    public class CustomController : BasicController
    {
        private readonly IEmplService _EmplService;
        private readonly ISystemService _SystemService;
        private readonly IDocumentService _DocumentService;
        private readonly INumberSeqService _NumberSeqService;
        private readonly IServiceIncidentService _ServiceIncidentService;
        private readonly ITripSettingsService _TripSettingsService;
        private readonly ITripMRPService _ITripMRPService;
        private readonly ICountryService _CountryService;
        private readonly IOrganizationService _OrganizationService;
        private readonly IReasonRequestService _ReasonRequestService;
        private readonly IQuestionRequestService _QuestionRequestService;
        private readonly IProtocolFoldersService _ProtocolFoldersService;
        private readonly IProcessService _ProcessService;
        private readonly IDepartmentService _DepartmentService;

        public CustomController(IEmplService emplService, ISystemService systemService, IDocumentService documentService, IServiceIncidentService serviceIncidentService, ICompanyService companyService, IAccountService accountService, ITripSettingsService tripSettingsService, INumberSeqService numberSeqService, ICountryService countryService, IOrganizationService organizationService, IProcessService processService,
            IReasonRequestService reasonRequestService, IQuestionRequestService questionRequestService, IProtocolFoldersService protocolFoldersService, ITripMRPService iTripMRPService, IDepartmentService departmentService)
            : base(companyService, accountService)
        {
            _EmplService = emplService;
            _SystemService = systemService;
            _DocumentService = documentService;
            _ServiceIncidentService = serviceIncidentService;
            _TripSettingsService = tripSettingsService;
            _NumberSeqService = numberSeqService;
            _CountryService = countryService;
            _OrganizationService = organizationService;
            _ProcessService = processService;
            _ReasonRequestService = reasonRequestService;
            _QuestionRequestService = questionRequestService;
            _ProtocolFoldersService = protocolFoldersService;
            _ITripMRPService = iTripMRPService;
            _DepartmentService = departmentService;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonEmpl()
        {
            var jsondata = _EmplService.GetJsonEmpl();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonEmplKZ()
        {
            var jsondata = _EmplService.GetJsonEmplKZ();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonGroup()
        {
            var jsondata = _EmplService.GetJsonGroup();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonEmplIntercompany()
        {
            var jsondata = _EmplService.GetJsonEmplIntercompany();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonRoles()
        {
            var jsondata = _EmplService.GetJsonRoles();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetJsonOnlyGroup()
        {
            var jsondata = _EmplService.GetJsonOnlyGroup();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetJsonTripEmpl()
        {
            var jsondata = _EmplService.GetJsonTripEmpl();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonUsers()
        {
            var jsondata = _EmplService.GetJsonUsers();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonEmplBothOption()
        {
            var jsondata = _EmplService.GetJsonEmplBothOption();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonDepartmentCompany()
        {
            var jsondata = _DepartmentService.GetJsonDepartmentCompany();
            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        //Custom
        public ActionResult GetIncidentAdminData(RapidDoc.Models.ViewModels.USR_REQ_IT_CTP_IncidentIT_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if(current != null)
                {
                    if(current.Any(x => x.ActivityName == "Администратор" || x.SystemName == "Administrator"))
                    {
                        ViewBag.ServiceIncidentList = _ServiceIncidentService.GetDropListServiceIncident(String.Empty);
                        return PartialView("USR_REQ_IT_CTP_IncidentIT_Edit_Administrator", model);
                    }
                }
            }

            if (document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution
                || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Closed || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Cancelled)
            {
                if (model.ServiceName != null)
                {
                    return PartialView("USR_REQ_IT_CTP_IncidentIT_View_Administrator", model);
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetBookingNumberORD(Guid documentId)
        {
            DocumentTable document = _DocumentService.Find(documentId);
            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var numberSeqTable = _NumberSeqService.FirstOrDefault(x => x.TableName == document.ProcessTable.TableName);
                ViewBag.NumberSeqBookingList = _NumberSeqService.GetDropListNumberSeqBookingNull(numberSeqTable.Id, Guid.Empty);
                return PartialView("_BookingNumbers");
            }
            return PartialView("_Empty");
        }

        public ActionResult GetBookingIncomingDoc(Type type)
        {
            var numberSeqTable = _NumberSeqService.FirstOrDefault(x => x.TableName == type.Name.Replace("_View", ""));
            ViewBag.NumberSeqBookingList = _NumberSeqService.GetDropListNumberSeqBookingNull(numberSeqTable.Id, Guid.Empty);
            return PartialView("_BookingNumbers");
        }

        public ActionResult GetRevocationORD(Guid? id, bool edit)
        {
            ViewBag.ORDRevocationList = _DocumentService.RevocationORDList(id, edit);
            ViewBag.EditMode = edit;
            return PartialView("USR_ORD_Revocation");
        }

        public ActionResult GetRevocationORDKZH(Guid? id, bool edit)
        {
            ViewBag.ORDRevocationList = _DocumentService.RevocationORDKZHList(id, edit);
            ViewBag.EditMode = edit;
            return PartialView("USR_ORD_Revocation");
        }

        public ActionResult GetAdditionORD(Guid? id, bool edit)
        {
            ViewBag.ORDAdditionList = _DocumentService.AdditionORDList(id, edit);
            ViewBag.EditMode = edit;
            return PartialView("USR_ORD_Addition");
        }

        public ActionResult GetAdditionORDKZH(Guid? id, bool edit)
        {
            ViewBag.ORDAdditionList = _DocumentService.AdditionORDKZHList(id, edit);
            ViewBag.EditMode = edit;
            return PartialView("USR_ORD_Addition");
        }

        public ActionResult GetCountryORD(Guid? id = null, bool selected = false)
        {
            ViewBag.Selected = selected;
            ViewBag.CountryList = selected == true ? _CountryService.GetDropListCountry(id) : _CountryService.GetDropListCountryNull(id);
            return PartialView("USR_ORD_Country");
        }

        public ActionResult GetOrganizationORD(Guid? id = null, bool selected = false)
        {
            ViewBag.Selected = selected;
            ViewBag.OrganizationList = selected == true ? _OrganizationService.GetDropListOrganization(id) :_OrganizationService.GetDropListOrganizationNull(id);
            return PartialView("USR_ORD_Organization");
        }

        public ActionResult GetReasonRequest(Guid? id = null, bool selected = false)
        {
            ViewBag.Selected = selected;
            ViewBag.ReasonRequestList = selected == true ? _ReasonRequestService.GetDropListReasonRequest(id) : _ReasonRequestService.GetDropListReasonRequestNull(id);
            return PartialView("_ReasonRequest");
        }

        public ActionResult GetQuestionRequest(Guid? id = null, bool selected = false)
        {
            ViewBag.Selected = selected;
            ViewBag.QuestionRequestList = selected == true ? _QuestionRequestService.GetDropListQuestionRequest(id) : _QuestionRequestService.GetDropListQuestionRequestNull(id);
            return PartialView("_QuestionRequest");
        }

        public ActionResult GetIncomingDoc(Guid? id = null)
        {
            ViewBag.IncomingDocList = _DocumentService.IncomingDocList<USR_IND_IncomingDocuments_Table>(id);
            return PartialView("USR_IND_IncomingDocList");
        }
        public ActionResult GetIncomingDocKZH(Guid? id = null)
        {
            ViewBag.IncomingDocList = _DocumentService.IncomingDocList<USK_IND_IncomingDocuments_Table>(id);
            return PartialView("USR_IND_IncomingDocList");
        }
        public ActionResult GetIncomingDocKZC(Guid? id = null)
        {
            ViewBag.IncomingDocList = _DocumentService.IncomingDocList<USC_IND_IncomingDocuments_Table>(id);
            return PartialView("USR_IND_IncomingDocList");
        }

        public ActionResult GetOutcomingDoc(Guid? id = null)
        {
            ViewBag.OutcomingDocList = _DocumentService.OutcomingDocList<USR_OND_OutcomingDocuments_Table>(id);
            return PartialView("USR_OND_OutcomingDocList");
        }

        public ActionResult GetOutcomingDocKZH(Guid? id = null)
        {
            ViewBag.OutcomingDocList = _DocumentService.OutcomingDocList<USK_OND_OutcomingDocuments_Table>(id);
            return PartialView("USR_OND_OutcomingDocList");
        }

        public ActionResult GetOutcomingDocKZC(Guid? id = null)
        {
            ViewBag.OutcomingDocList = _DocumentService.OutcomingDocList<USC_OND_OutcomingDocuments_Table>(id);
            return PartialView("USR_OND_OutcomingDocList");
        }

        public ActionResult GetPRTFolderORD(Guid processId, Guid? id = null, bool selected = false)
        {
            ViewBag.Selected = selected;
            ViewBag.PRTFolderList = _ProtocolFoldersService.GetDropListProtocolFoldersFullPath(processId, id);
            return PartialView("USR_PRT_FolderList");
        }

        public ActionResult GetManualRequest(RapidDoc.Models.ViewModels.USR_REQ_KD_RequestForCompetitonProc_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_KD_RequestForCompetitonProc_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualRequest1(RapidDoc.Models.ViewModels.USR_REQ_KD_RequestForCompetitonProcUZL_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_KD_RequestForCompetitonProcUZL_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualRequest2(RapidDoc.Models.ViewModels.USR_REQ_KD_RequestForCompetitonProcServices_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_KD_RequestForCompetitonProcServices_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualRequest3(RapidDoc.Models.ViewModels.USR_REQ_KD_RequestForCompetitonProcServicesBGP_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_KD_RequestForCompetitonProcServicesBGP_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }
        public ActionResult GetRequestForOpenCompetition(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForOpenCompetition_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForOpenCompetition_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestForPriceOffers(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForPriceOffers_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForPriceOffers_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestForOneSource(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForOneSource_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForOneSource_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestForElectronicTrading(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForElectronicTrading_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForElectronicTrading_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestForVoiceBids(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForVoiceBids_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForVoiceBids_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetReissueComputerData(RapidDoc.Models.ViewModels.USR_REQ_IT_CTP_ReissueComputer_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Проверка данных" || x.SystemName == "CheckData"))
                    {
                        return PartialView("USR_REQ_IT_CTP_ReissueComputer_Edit_TableCheck", model);
                    }
                }
            }

            if (document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution
                || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Closed || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Cancelled)
            {
                return PartialView("USR_REQ_IT_CTP_ReissueComputer_View_TableView", model);
            }

            return PartialView("_Empty");
        }
        
        //Запрос на выделение сотрудника для приемки ТМЦ-->
        public ActionResult GetManualAcceptanceItems1(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForPeopleAcceptanceItems_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Руководитель 1" || x.SystemName == "Manager1"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForPeopleAcceptanceItems_Edit_Manual1", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualAcceptanceItems2(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForPeopleAcceptanceItems_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Руководитель 2" || x.SystemName == "Manager2"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForPeopleAcceptanceItems_Edit_Manual2", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualAcceptanceItems3(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForPeopleAcceptanceItems_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Руководитель 3" || x.SystemName == "Manager3"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForPeopleAcceptanceItems_Edit_Manual3", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualAcceptanceItems4(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForPeopleAcceptanceItems_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Руководитель 4" || x.SystemName == "Manager4"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForPeopleAcceptanceItems_Edit_Manual4", model);
                    }
                }
            }

            return PartialView("_Empty");
        }
        //<--Запрос на выделение сотрудника для приемки ТМЦ

        //Запрос на передачу договоров в ССД (договора с нерезидентами)-->

        public ActionResult GetManualContractNoneresident(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForContractNoneresident_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник ССД" || x.SystemName == "ManagerSSD"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForContractNoneresident_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualContractNoneresidentCustoms(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForContractNoneresidentCustoms_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник ССД" || x.SystemName == "ManagerSSD"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForContractNoneresidentCustoms_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }
        //<--Запрос на передачу договоров в ССД (договора с нерезидентами)

        //Запрос на передачу договоров в ССД (договора с нерезидентами)-->

        public ActionResult GetManualContractResident(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForContractResident_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник ССД" || x.SystemName == "ManagerSSD"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForContractResident_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        //<--Запрос на передачу договоров в ССД (договора с нерезидентами)

        //Запрос на предоставление КП-->

        public ActionResult GetManualRepresentationKD(RapidDoc.Models.ViewModels.USR_REQ_UZL_RequestForrepresentationKD_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "УЗЛ" || x.SystemName == "UZL"))
                    {
                        return PartialView("USR_REQ_UZL_RequestForrepresentationKD_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }
        //<--Запрос на предоставление КП

        public ActionResult GetRequestCreateSettlViewData(RapidDoc.Models.ViewModels.USR_REQ_UBUO_RequestCreateSettlView_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник СТЗП" || x.SystemName == "ManagerCTZP"))
                    {
                        return PartialView("USR_REQ_UBUO_RequestCreateSettlView_Edit_StatAcxcounting", model);
                    }
                    if (current.Any(x => x.ActivityName == "Начальник СНУ" || x.SystemName == "ManagerCNY"))
                    {
                        return PartialView("USR_REQ_UBUO_RequestCreateSettlView_Edit_BeginAcxcounting", model);
                    }
                }
            }

            return PartialView("USR_REQ_UBUO_RequestCreateSettlView_View_Full", model);
        }

        [HttpPost]
        public ActionResult UpdateCalcTripUBUO(byte EmplTripType, byte TripDirection, byte TypeRequestTrip, int Day, int DayLive, int TicketSum)
        {
            EmplTripType emplTripType = (EmplTripType)EmplTripType;
            TripDirection tripDirection = (TripDirection)TripDirection;
            TypeRequestTrip typeRequestTrip = (TypeRequestTrip)TypeRequestTrip;

            DateTime dateNow = DateTime.UtcNow.Date;
            TripMRPTable mrp = _ITripMRPService.FirstOrDefault(x => x.FromDate <= dateNow && x.ToDate >= dateNow);
            TripSettingsTable tripSettingsTable = _TripSettingsService.FirstOrDefault(x => x.EmplTripType == emplTripType && x.TripDirection == tripDirection);
            if (tripSettingsTable != null && mrp != null)
            {
                double residenceRate = Double.Parse(tripSettingsTable.ResidenceRate, CultureInfo.InvariantCulture);
                double dayRate = Double.Parse(tripSettingsTable.DayRate, CultureInfo.InvariantCulture) * Double.Parse(mrp.Amount, CultureInfo.InvariantCulture);

                var model = new USR_REQ_UBUO_RequestCalcDriveTripCals_View(emplTripType, tripDirection, typeRequestTrip, Day, DayLive, TicketSum, (int)Math.Ceiling(dayRate), (int)residenceRate);
                return PartialView(@"~/Views/Custom/USR_REQ_UBUO_RequestCalcDriveTrip_Calc.cshtml", model);
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualURPInstruction(RapidDoc.Models.ViewModels.USR_REQ_UKR_RequestForExpertiseInstruction_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УКР" || x.SystemName == "ManagerYKR"))
                    {
                        return PartialView("USR_REQ_UKR_RequestForExpertiseInstruction_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualURPDepartment(RapidDoc.Models.ViewModels.USR_REQ_UKR_RequestForExpertiseDepartment_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник УКР" || x.SystemName == "ManagerYKR"))
                    {
                        return PartialView("USR_REQ_UKR_RequestForExpertiseDepartment_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }
        //Запрос на рекруткарты-->
        public ActionResult GetManualHRCardITR11(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITR1_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Непосредственный руководитель" || x.SystemName == "DirectManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITR1_Edit_Manual1", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITR12(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITR1_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "ИД" || x.SystemName == "TopManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITR1_Edit_Manual2", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITR13(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITR1_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Генеральный директор/и.о. ГД" || x.SystemName == "GeneralDirector"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITR1_Edit_Manual3", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITR14(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITR1_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник службы ОЗиТБ" || x.SystemName == "ManagerOZTB"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITR1_Edit_Manual4", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITR21(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITR2_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Непосредственный руководитель" || x.SystemName == "DirectManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITR2_Edit_Manual1", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITR22(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITR2_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник управление" || x.ActivityName == "Начальник управления" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITR2_Edit_Manual2", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualRequestForITWeekend(RapidDoc.Models.ViewModels.USR_REQ_IT_CAP_RequestForITWeekend_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальники служб УИТ" || x.SystemName == "ManagerIT"))
                    {
                        return PartialView("USR_REQ_IT_CAP_RequestForITWeekend_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITR23(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITR2_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "ИД" || x.SystemName == "TopManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITR2_Edit_Manual3", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITR24(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITR2_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник службы ОЗиТБ" || x.SystemName == "ManagerOZTB"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITR2_Edit_Manual4", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITRZIF1(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITRZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Непосредственный руководитель" || x.SystemName == "DirectManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITRZIF_Edit_Manual1", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITRZIF2(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITRZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Директор фабрики" || x.SystemName == "DirectorZIF"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITRZIF_Edit_Manual2", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardITRZIF3(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardITRZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник службы ОЗиТБ" || x.SystemName == "ManagerOZTB"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardITRZIF_Edit_Manual3", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardWork1(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardWork_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Непосредственный руководитель" || x.SystemName == "DirectManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardWork_Edit_Manual1", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardWork2(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardWork_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник управления" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardWork_Edit_Manual2", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardWork3(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardWork_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Исполнительный директор" || x.SystemName == "TopManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardWork_Edit_Manual3", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardWork4(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardWork_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник службы ОЗиТБ" || x.SystemName == "ManagerOZTB"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardWork_Edit_Manual4", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardWorkZIF1(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardWorkZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Непосредственный руководитель" || x.SystemName == "DirectManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardWorkZIF_Edit_Manual1", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardWorkZIF2(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardWorkZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Исполнительный директор" || x.SystemName == "TopManager"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardWorkZIF_Edit_Manual2", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetManualHRCardWorkZIF3(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForHRCardWorkZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник службы ОЗиТБ" || x.SystemName == "ManagerOZTB"))
                    {
                        return PartialView("USR_REQ_URP_RequestForHRCardWorkZIF_Edit_Manual3", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestForProvisionGraphVac(RapidDoc.Models.ViewModels.USR_REQ_URP_RequestForProvisionGraphVac_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Исполнитель" || x.SystemName == "Executor"))
                    {
                        return PartialView("USR_REQ_URP_RequestForProvisionGraphVac_Edit_Exec", model);
                    }
                }
            }

            return PartialView("_Empty");
        }
        //<--Запрос на рекруткарты

        //УТ-->
        public ActionResult GetRequestAuxiliaryTransportOCTMCInvest(RapidDoc.Models.ViewModels.USR_REQ_YT_AuxiliaryTransportOCTMCInvest_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_AuxiliaryTransportOCTMCInvest_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_AuxiliaryTransportOCTMCInvest_View_Show", model);
        }

        public ActionResult GetRequestAuxiliaryTransportOCTMCOper(RapidDoc.Models.ViewModels.USR_REQ_YT_AuxiliaryTransportOCTMCOper_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_AuxiliaryTransportOCTMCOper_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_AuxiliaryTransportOCTMCOper_View_Show", model);
        }

        public ActionResult GetRequestAuxiliaryTransportDayOff(RapidDoc.Models.ViewModels.USR_REQ_YT_AuxiliaryTransportDayOff_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ"
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_AuxiliaryTransportDayOff_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_AuxiliaryTransportDayOff_View_Show", model);
        }

        public ActionResult GetRequestAuxiliaryTransportWorkDays(RapidDoc.Models.ViewModels.USR_REQ_YT_AuxiliaryTransportWorkDays_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_AuxiliaryTransportWorkDays_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_AuxiliaryTransportWorkDays_View_Show", model);
        }

        public ActionResult GetRequestAuxiliaryTransportOutABK(RapidDoc.Models.ViewModels.USR_REQ_YT_AuxiliaryTransportOutABK_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ"
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_AuxiliaryTransportOutABK_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_AuxiliaryTransportOutABK_View_Show", model);
        }

        public ActionResult GetRequestStandbyTransport(RapidDoc.Models.ViewModels.USR_REQ_YT_StandbyTransport_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_StandbyTransport_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_StandbyTransport_View_Show", model);
        }

        public ActionResult GetRequestStandbyTransportUIT(RapidDoc.Models.ViewModels.USR_REQ_YT_StandbyTransportUIT_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_StandbyTransportUIT_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_StandbyTransportUIT_View_Show", model);
        }

        public ActionResult GetRequestLightTransportTripManage(RapidDoc.Models.ViewModels.USR_REQ_YT_LightTransportTripManage_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_LightTransportTripManage_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_LightTransportTripManage_View_Show", model);
        }

        public ActionResult GetRequestLightTransportTripATK(RapidDoc.Models.ViewModels.USR_REQ_YT_LightTransportTripATK_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_LightTransportTripATK_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_LightTransportTripATK_View_Show", model);
        }

        public ActionResult GetRequestLightTransportOCTMCInvest(RapidDoc.Models.ViewModels.USR_REQ_YT_LightTransportOCTMCInvest_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_LightTransportOCTMCInvest_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_LightTransportOCTMCInvest_View_Show", model);
        }

        public ActionResult GetRequestLightTransportOCTMCOper(RapidDoc.Models.ViewModels.USR_REQ_YT_LightTransportOCTMCOper_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_LightTransportOCTMCOper_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_LightTransportOCTMCOper_View_Show", model);
        }

        public ActionResult GetRequestLightTransportOutOrganizationInvest(RapidDoc.Models.ViewModels.USR_REQ_YT_LightTransportOutOrganizationInvest_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_LightTransportOutOrganizationInvest_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_LightTransportOutOrganizationInvest_View_Show", model);
        }

        public ActionResult GetRequestLightTransportOutOrganizationOper(RapidDoc.Models.ViewModels.USR_REQ_YT_LightTransportOutOrganizationOper_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_LightTransportOutOrganizationOper_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_LightTransportOutOrganizationOper_View_Show", model);
        }

        public ActionResult GetRequestLightTransportTripDayOff(RapidDoc.Models.ViewModels.USR_REQ_YT_LightTransportTripDayOff_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_LightTransportTripDayOff_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_LightTransportTripDayOff_View_Show", model);
        }

        public ActionResult GetRequestPassangerTransportTrip(RapidDoc.Models.ViewModels.USR_REQ_YT_PassangerTransportTrip_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_PassangerTransportTrip_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_PassangerTransportTrip_View_Show", model);
        }

        public ActionResult GetRequestProvisionOfWelder(RapidDoc.Models.ViewModels.USR_REQ_UMM_ProvisionOfWelder_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
         
            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MasterUMM"))
                    {
                        return PartialView("USR_REQ_UMM_ProvisionOfWelder_Edit_UMM", model);
                    }
                    if (current.Any(x => x.SystemName == "DispUMM"))
                    {
                        return PartialView("USR_REQ_UMM_ProvisionOfWelder_Edit_Disp", model);
                    }
                }
            }

            return PartialView("USR_REQ_UMM_ProvisionOfWelder_View_Full", model);
        }

        public ActionResult GetRequestPassangerTransportTripManage(RapidDoc.Models.ViewModels.USR_REQ_YT_PassangerTransportTripManage_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_PassangerTransportTripManage_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_PassangerTransportTripManage_View_Show", model);
        }

        public ActionResult GetRequestPassangerTransportTripATK(RapidDoc.Models.ViewModels.USR_REQ_YT_PassangerTransportTripATK_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_PassangerTransportTripATK_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_PassangerTransportTripATK_View_Show", model);
        }

        public ActionResult GetRequestPassangerTransportOutOrganizationInvest(RapidDoc.Models.ViewModels.USR_REQ_YT_PassangerTransportOutOrganizationInvest_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_PassangerTransportOutOrganizationInvest_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_PassangerTransportOutOrganizationInvest_View_Show", model);
        }

        public ActionResult GetRequestPassangerTransportOutOrganizationOper(RapidDoc.Models.ViewModels.USR_REQ_YT_PassangerTransportOutOrganizationOper_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_PassangerTransportOutOrganizationOper_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_PassangerTransportOutOrganizationOper_View_Show", model);
        }

        public ActionResult GetRequestPassangerTransportDayOff(RapidDoc.Models.ViewModels.USR_REQ_YT_PassangerTransportDayOff_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_PassangerTransportDayOff_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_PassangerTransportDayOff_View_Show", model);
        }

        public ActionResult GetRequestPassangerTransportDayOffZIF(RapidDoc.Models.ViewModels.USR_REQ_YT_PassangerTransportDayOffZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_PassangerTransportDayOffZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_PassangerTransportDayOffZIF_View_Show", model);
        }

        public ActionResult GetRequestPassangerTransportCorporate(RapidDoc.Models.ViewModels.USR_REQ_YT_PassangerTransportCorporate_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_PassangerTransportCorporate_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_PassangerTransportCorporate_View_Show", model);
        }

        public ActionResult GetRequestEmergAuxiliaryTransportZIF(RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForEmergAuxiliaryTransportZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForEmergAuxiliaryTransportZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForEmergAuxiliaryTransportZIF_View_Show", model);
        }

        public ActionResult GetRequestAuxiliaryTransportDayOffZIF (RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForAuxiliaryTransportDayOffZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForAuxiliaryTransportDayOffZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForAuxiliaryTransportDayOffZIF_View_Show", model);
        }

        public ActionResult GetRequestAuxiliaryTransportWorkDaysZIF  (RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForAuxiliaryTransportWorkDaysZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForAuxiliaryTransportWorkDaysZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForAuxiliaryTransportWorkDaysZIF_View_Show", model);
        }

        public ActionResult GetRequestStandbyTransportZIF (RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForStandbyTransportZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForStandbyTransportZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForStandbyTransportZIF_View_Show", model);
        }

        public ActionResult GetRequestLightTransportTripManageZIF(RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForLightTransportTripManageZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForLightTransportTripManageZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForLightTransportTripManageZIF_View_Show", model);
        }

        public ActionResult GetRequestLightTransportTripDayOffZIF   (RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForLightTransportTripDayOffZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForLightTransportTripDayOffZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForLightTransportTripDayOffZIF_View_Show", model);
        }

        public ActionResult GetRequestLightPassangerTransportZIF(RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForPassangerTransportZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForPassangerTransportZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForPassangerTransportZIF_View_Show", model);
        }

        public ActionResult GetRequestPassangerTransportTripZIF(RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForPassangerTransportTripZIF_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForPassangerTransportTripZIF_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForPassangerTransportTripZIF_View_Show", model);
        }

        public ActionResult GetRequestStandbyTransportUZL(RapidDoc.Models.ViewModels.USR_REQ_YT_RequestForStandbyTransportUZL_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Диспетчер УТ" || x.ActivityName == "ЦВТ" || x.ActivityName == "ЦЛТ" || x.ActivityName == "ЦПТ" 
                        || x.SystemName == "DispatcherYT" || x.SystemName == "CVT" || x.SystemName == "CLT" || x.SystemName == "CPT"))
                    {
                        return PartialView("USR_REQ_YT_RequestForStandbyTransportUZL_View_Edit", model);
                    }
                }
            }

            return PartialView("USR_REQ_YT_RequestForStandbyTransportUZL_View_Show", model);
        }
        //<--УТ

        public ActionResult GetRequestBookingRoom(RapidDoc.Models.ViewModels.USR_REQ_HY_BookingRoom_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник ХУ" || x.SystemName == "ManagerHY"))
                    {
                        return PartialView("USR_REQ_HY_BookingRoom_Edit_HY", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestFindApartment(RapidDoc.Models.ViewModels.USR_REQ_HY_FindApartment_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник ХУ" || x.SystemName == "ManagerHY"))
                    {
                        return PartialView("USR_REQ_HY_FindApartment_Edit_HY", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestRequestRepair(RapidDoc.Models.ViewModels.USR_REQ_HY_RequestRepair_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник ХУ" || x.SystemName == "ManagerHY"))
                    {
                        return PartialView("USR_REQ_HY_RequestRepair_Edit_HY", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestEmergencyPurposeTRU(RapidDoc.Models.ViewModels.USR_REQ_HY_EmergencyPurposeTRU_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "СХО" || x.ActivityName == "Начальник СХО"
                        || x.SystemName == "CHO" || x.SystemName == "ManagerCHO"))
                    {
                        return PartialView("USR_REQ_HY_EmergencyPurposeTRU_Edit_CXO", model);
                    }
                }
            }

            return PartialView("USR_REQ_HY_EmergencyPurposeTRU_View_Full", model);
        }

        public ActionResult GetRequestTRU(RapidDoc.Models.ViewModels.USR_REQ_HY_RequestTRU_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "СХО" || x.ActivityName == "Начальник СХО" 
                        || x.SystemName == "CHO" || x.SystemName == "ManagerCHO"))
                    {
                        return PartialView("USR_REQ_HY_RequestTRU_Edit_CXO", model);
                    }
                }
            }

            return PartialView("USR_REQ_HY_RequestTRU_View_Full", model);
        }
        public ActionResult GetManualOfficeMemo(BasicDocumantOfficeMemoView model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (document == null)
                return RedirectToAction("PageNotFound", "Error");

            if (User.IsInRole("Administrator"))
                return PartialView(document.ProcessTable.TableName + "_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {

                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView(document.ProcessTable.TableName + "_Edit_Part", model);
                    }
                }
            }

            return PartialView(document.ProcessTable.TableName + "_View_Full", model);
        }


        public ActionResult GetManualOfficeMemoUIT(RapidDoc.Models.ViewModels.USR_OFM_UIT_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UIT_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {

                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager") )
                    {
                        return PartialView("USR_OFM_UIT_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UIT_OfficeMemo_View_Full", model);
        }

        public ActionResult GetManualOfficeMemoVIP(RapidDoc.Models.ViewModels.USR_OFM_VIP_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_VIP_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_VIP_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_VIP_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoBMK(RapidDoc.Models.ViewModels.USR_OFM_BMK_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_BMK_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_BMK_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_BMK_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoBY(RapidDoc.Models.ViewModels.USR_OFM_BY_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_BY_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_BY_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_BY_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUKR(RapidDoc.Models.ViewModels.USR_OFM_UKR_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UKR_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UKR_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UKR_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoDS(RapidDoc.Models.ViewModels.USR_OFM_DS_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_DS_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_DS_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_DS_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoZIF(RapidDoc.Models.ViewModels.USR_OFM_ZIF_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_ZIF_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_ZIF_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_ZIF_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoOKS(RapidDoc.Models.ViewModels.USR_OFM_OKS_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_OKS_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_OKS_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_OKS_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoOTK(RapidDoc.Models.ViewModels.USR_OFM_OTK_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_OTK_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_OTK_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_OTK_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoPAL(RapidDoc.Models.ViewModels.USR_OFM_PAL_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_PAL_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_PAL_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_PAL_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoProfKom(RapidDoc.Models.ViewModels.USR_OFM_ProfKom_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_ProfKom_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_ProfKom_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_ProfKom_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoPTO(RapidDoc.Models.ViewModels.USR_OFM_PTO_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_PTO_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_PTO_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_PTO_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoPTU(RapidDoc.Models.ViewModels.USR_OFM_PTU_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_PTU_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_PTU_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_PTU_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoROGR(RapidDoc.Models.ViewModels.USR_OFM_ROGR_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_ROGR_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_ROGR_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_ROGR_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoSK(RapidDoc.Models.ViewModels.USR_OFM_SK_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_SK_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_SK_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_SK_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoSKS(RapidDoc.Models.ViewModels.USR_OFM_SKS_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_SKS_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_SKS_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_SKS_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoSM(RapidDoc.Models.ViewModels.USR_OFM_SM_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_SM_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_SM_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_SM_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoSFK(RapidDoc.Models.ViewModels.USR_OFM_SFK_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_SFK_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_SFK_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_SFK_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUB(RapidDoc.Models.ViewModels.USR_OFM_UB_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UB_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UB_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UB_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUBUO(RapidDoc.Models.ViewModels.USR_OFM_UBUO_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UBUO_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UBUO_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UBUO_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUZL(RapidDoc.Models.ViewModels.USR_OFM_UZL_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UZL_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UZL_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UZL_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUKV(RapidDoc.Models.ViewModels.USR_OFM_UKV_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UKV_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UKV_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UKV_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUMM(RapidDoc.Models.ViewModels.USR_OFM_UMM_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UMM_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UMM_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UMM_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUPB(RapidDoc.Models.ViewModels.USR_OFM_UPB_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UPB_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UPB_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UPB_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoURP(RapidDoc.Models.ViewModels.USR_OFM_URP_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_URP_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_URP_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_URP_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUSH(RapidDoc.Models.ViewModels.USR_OFM_USH_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_USH_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_USH_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_USH_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUT(RapidDoc.Models.ViewModels.USR_OFM_UT_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UT_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UT_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UT_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUTOR(RapidDoc.Models.ViewModels.USR_OFM_UTOR_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UTOR_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UTOR_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UTOR_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoUE(RapidDoc.Models.ViewModels.USR_OFM_UE_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_UE_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_UE_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_UE_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoFS(RapidDoc.Models.ViewModels.USR_OFM_FS_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_FS_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_FS_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_FS_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoHU(RapidDoc.Models.ViewModels.USR_OFM_HU_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_HU_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_HU_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_HU_OfficeMemo_View_Full", model);
        }
        public ActionResult GetManualOfficeMemoJU(RapidDoc.Models.ViewModels.USR_OFM_JU_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_OFM_JU_OfficeMemo_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
                    {
                        return PartialView("USR_OFM_JU_OfficeMemo_Edit_Part", model);
                    }
                }
            }

            return PartialView("USR_OFM_JU_OfficeMemo_View_Full", model);
        }

        public ActionResult GetManualORD(dynamic model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView(document.ProcessTable.TableName + "_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor1") || current.Any(x => x.SystemName == "Censor2") || current.Any(x => x.SystemName == "Censor"))
                    {
                        return PartialView(document.ProcessTable.TableName + "_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView(document.ProcessTable.TableName + "_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView(document.ProcessTable.TableName + "_Edit_Registration", model);
                    }
                }
            }

            return PartialView(document.ProcessTable.TableName + "_View_Full", model);
        }

        public ActionResult GetManualORDMainActivity(RapidDoc.Models.ViewModels.USR_ORD_MainActivity_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_MainActivity_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor1") || current.Any(x => x.SystemName == "Censor2"))
                    {
                        return PartialView("USR_ORD_MainActivity_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_MainActivity_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_MainActivity_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_MainActivity_View_Full", model);
        }

        public ActionResult GetManualORDBusinessTrip(RapidDoc.Models.ViewModels.USR_ORD_BusinessTrip_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_BusinessTrip_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor1") || current.Any(x => x.SystemName == "Censor2"))
                    {
                        return PartialView("USR_ORD_BusinessTrip_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_BusinessTrip_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_BusinessTrip_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_BusinessTrip_View_Full", model);
        }

        public ActionResult GetManualORDStaff(RapidDoc.Models.ViewModels.USR_ORD_Staff_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_Staff_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor"))
                    {
                        return PartialView("USR_ORD_Staff_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_Staff_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_Staff_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_Staff_View_Full", model);
        }

        public ActionResult GetManualORDReception(RapidDoc.Models.ViewModels.USR_ORD_Reception_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_Reception_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor"))
                    {
                        return PartialView("USR_ORD_Reception_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_Reception_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_Reception_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_Reception_View_Full", model);
        }

        public ActionResult GetManualORDDismissal(RapidDoc.Models.ViewModels.USR_ORD_Dismissal_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_Dismissal_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor"))
                    {
                        return PartialView("USR_ORD_Dismissal_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_Dismissal_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_Dismissal_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_Dismissal_View_Full", model);
        }

        public ActionResult GetManualORDTransfer(RapidDoc.Models.ViewModels.USR_ORD_Transfer_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_Transfer_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor"))
                    {
                        return PartialView("USR_ORD_Transfer_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_Transfer_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_Transfer_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_Transfer_View_Full", model);
        }

        public ActionResult GetManualORDHoliday(RapidDoc.Models.ViewModels.USR_ORD_Holiday_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_Holiday_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor"))
                    {
                        return PartialView("USR_ORD_Holiday_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_Holiday_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_Holiday_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_Holiday_View_Full", model);
        }

        public ActionResult GetManualORDChangeStaff(RapidDoc.Models.ViewModels.USR_ORD_ChangeStaff_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_ChangeStaff_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor"))
                    {
                        return PartialView("USR_ORD_ChangeStaff_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_ChangeStaff_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_ChangeStaff_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_ChangeStaff_View_Full", model);
        }

        public ActionResult GetManualORDSanction(RapidDoc.Models.ViewModels.USR_ORD_Sanction_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if (User.IsInRole("Administrator"))
                return PartialView("USR_ORD_Sanction_Edit_Part", model);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "Censor"))
                    {
                        return PartialView("USR_ORD_Sanction_Edit_Part", model);
                    }
                    if (current.Any(x => x.SystemName == "Translator"))
                    {
                        return PartialView("USR_ORD_Sanction_Edit_Translator", model);
                    }
                    if (current.Any(x => x.SystemName == "Registration"))
                    {
                        return PartialView("USR_ORD_Sanction_Edit_Registration", model);
                    }
                }
            }

            return PartialView("USR_ORD_Sanction_View_Full", model);
        }

        public ActionResult GetRequestCTPTRU(RapidDoc.Models.ViewModels.USR_REQ_IT_CTP_RequestTRU_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "СТП" || x.SystemName == "CTP"))
                    {
                        return PartialView("USR_REQ_IT_CTP_RequestTRU_Edit_CTP", model);
                    }

                    if (current.Any(x => x.ActivityName == "Заведующий складом СТП" || x.SystemName == "ManagerWarehouse"))
                    {
                        return PartialView("USR_REQ_IT_CTP_RequestTRU_Edit_StockMan", model);
                    }
                }
            }

            return PartialView("USR_REQ_IT_CTP_RequestTRU_View_Full", model);
        }

        public ActionResult GetEmergencyRequestTRU(RapidDoc.Models.ViewModels.USR_REQ_HY_EmergencyRequestTRU_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник СХО" || x.SystemName == "ManagerCHO"))
                    {
                        return PartialView("USR_REQ_HY_EmergencyRequestTRU_Edit_CXO", model);
                    }
                }
            }

            return PartialView("USR_REQ_HY_EmergencyRequestTRU_View_Full", model);
        }

        public ActionResult GetOutcomingDocuments(dynamic model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "ONDRegistration"))
                    {
                        return PartialView(document.ProcessTable.TableName + "_Registration", model);
                    }
                }
            }

            return PartialView(document.ProcessTable.TableName + "_View_Full", model);
        }

        public ActionResult GetManualRequestForExplanationNormalAct(RapidDoc.Models.ViewModels.USR_REQ_JU_RequestForExplanationNormalAct_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.ActivityName == "Начальник ЮУ" || x.SystemName == "ChiefNormalAct"))
                    {
                        return PartialView("USR_REQ_JU_RequestForExplanationNormalAct_Edit_Manual", model);
                    }
                }
            }

            return PartialView("_Empty");
        }

        public ActionResult GetRequestManufactureItemsBGP(RapidDoc.Models.ViewModels.USR_REQ_UMM_ManufactureItemsBGP_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            string curUser = User.Identity.GetUserId();
            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "UMMBGP") && current.FirstOrDefault(x => x.SystemName == "UMMBGP").Users.Any(x => x.UserId == curUser))
                    {
                        return PartialView("USR_REQ_UMM_ManufactureItemsBGP_Edit_TOIR", model);
                    }

                    if (current.Any(x => x.SystemName == "HighMasterUMM"))
                    {
                        return PartialView("USR_REQ_UMM_ManufactureItemsBGP_Edit_UMM", model);
                    }
                }
            }

            return PartialView("USR_REQ_UMM_ManufactureItemsBGP_View_Full", model);
        }

        public ActionResult GetRequestManufactureWeldingItemsBGP(RapidDoc.Models.ViewModels.USR_REQ_UMM_ManufactureWeldingItemsBGP_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            string curUser = User.Identity.GetUserId();
            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "HighMasterUMM"))
                    {
                        return PartialView("USR_REQ_UMM_ManufactureWeldingItemsBGP_Edit_Executor", model);
                    }
                }
            }

            return PartialView("USR_REQ_UMM_ManufactureWeldingItemsBGP_View_Full", model);
        }

        public ActionResult GetRequestManufactureItemsWeekends(RapidDoc.Models.ViewModels.USR_REQ_UMM_ManufactureItemsWeekends_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            string curUser = User.Identity.GetUserId();
            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "HighMasterUMM"))
                    {
                        return PartialView("USR_REQ_UMM_ManufactureItemsWeekends_Edit_Executor", model);
                    }
                }
            }

            return PartialView("USR_REQ_UMM_ManufactureItemsWeekends_View_Full", model);
        }

        public ActionResult GetRequestWeldingItemsBGPWeekends(RapidDoc.Models.ViewModels.USR_REQ_UMM_WeldingItemsBGPWeekends_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            string curUser = User.Identity.GetUserId();
            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "HighMasterUMM"))
                    {
                        return PartialView("USR_REQ_UMM_WeldingItemsBGPWeekends_Edit_Executor", model);
                    }
                }
            }

            return PartialView("USR_REQ_UMM_WeldingItemsBGPWeekends_View_Full", model);
        }

        public ActionResult GetRequestFluxingWork(RapidDoc.Models.ViewModels.USR_REQ_UMM_FluxingWork_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            string curUser = User.Identity.GetUserId();
            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "HighMasterUMM"))
                    {
                        return PartialView("USR_REQ_UMM_FluxingWork_Edit_Executor", model);
                    }
                }
            }

            return PartialView("USR_REQ_UMM_FluxingWork_View_Full", model);
        }

        public ActionResult GetPRTTechCommitteeDocuments(RapidDoc.Models.ViewModels.USR_PRT_TechCommitteeDocuments_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            ViewBag.ProcessId = document.ProcessTableId;
            if (User.IsInRole("Administrator"))
                return PartialView("USR_PRT_TechCommitteeDocuments_Edit", model);

            return PartialView("USR_PRT_TechCommitteeDocuments_View_Full", model);
        }

        public ActionResult GetPRTProtocolDocuments(RapidDoc.Models.ViewModels.USR_PRT_ProtocolDocuments_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            ViewBag.ProcessId = document.ProcessTableId;
            if (User.IsInRole("Administrator"))
                return PartialView("USR_PRT_ProtocolDocuments_Edit", model);

            return PartialView("USR_PRT_ProtocolDocuments_View_Full", model);
        }
        public ActionResult GetPRTManagementDocuments(RapidDoc.Models.ViewModels.USR_PRT_ManagementDocuments_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            ViewBag.ProcessId = document.ProcessTableId;
            if (User.IsInRole("Administrator"))
                return PartialView("USR_PRT_ManagementDocuments_Edit", model);

            return PartialView("USR_PRT_ManagementDocuments_View_Full", model);
        }

        public ActionResult GetPRTDirectorateDocuments(RapidDoc.Models.ViewModels.USR_PRT_DirectorateDocuments_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            ViewBag.ProcessId = document.ProcessTableId;
            if (User.IsInRole("Administrator"))
                return PartialView("USR_PRT_DirectorateDocuments_Edit", model);

            return PartialView("USR_PRT_DirectorateDocuments_View_Full", model);
        }

        public ActionResult GetPRTBalanceCommissionDocuments(RapidDoc.Models.ViewModels.USR_PRT_BalanceCommissionDocuments_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            ViewBag.ProcessId = document.ProcessTableId;
            if (User.IsInRole("Administrator"))
                return PartialView("USR_PRT_BalanceCommissionDocuments_Edit", model);

            return PartialView("USR_PRT_BalanceCommissionDocuments_View_Full", model);
        }

        public ActionResult GetPRTDocuments(dynamic model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);
            ViewBag.ProcessId = document.ProcessTableId;
            if (User.IsInRole("Administrator"))
                return PartialView(document.ProcessTable.TableName + "_Edit", model);

            return PartialView(document.ProcessTable.TableName + "_View_Full", model);
        }

        [HttpPost]
        public ActionResult UpdateCalcBTripPPTRIP(byte EmplTripType, byte TripDirection, byte TypeRequestTrip, int Day, int DayLive, int TicketSum)
        {
            EmplTripType emplTripType = (EmplTripType)EmplTripType;
            TripDirection tripDirection = (TripDirection)TripDirection;
            TypeRequestTrip typeRequestTrip = (TypeRequestTrip)TypeRequestTrip;

            DateTime dateNow = DateTime.UtcNow.Date;
            TripMRPTable mrp = _ITripMRPService.FirstOrDefault(x => x.FromDate <= dateNow && x.ToDate >= dateNow);
            TripSettingsTable tripSettingsTable = _TripSettingsService.FirstOrDefault(x => x.EmplTripType == emplTripType && x.TripDirection == tripDirection);
            if (tripSettingsTable != null && mrp != null)
            {
                double residenceRate = Double.Parse(tripSettingsTable.ResidenceRate, CultureInfo.InvariantCulture);
                double dayRate = Double.Parse(tripSettingsTable.DayRate, CultureInfo.InvariantCulture) * Double.Parse(mrp.Amount, CultureInfo.InvariantCulture);

                var model = new USR_REQ_TRIP_RequestCalcDriveBTripCalsPP_View(emplTripType, tripDirection, typeRequestTrip, Day, DayLive, TicketSum, (int)Math.Ceiling(dayRate), (int)residenceRate);
                return PartialView(@"~/Views/Custom/USR_REQ_TRIP_RegistrationBusinessTripPP_Calc.cshtml", model);
            }

            return PartialView("_Empty");
        }

        [HttpPost]
        public ActionResult UpdateCalcBTripPTY(byte EmplTripType, byte TripDirection, byte TypeRequestTrip, int Day, int DayLive, int TicketSum)
        {
            EmplTripType emplTripType = (EmplTripType)EmplTripType;
            TripDirection tripDirection = (TripDirection)TripDirection;
            TypeRequestTrip typeRequestTrip = (TypeRequestTrip)TypeRequestTrip;

            DateTime dateNow = DateTime.UtcNow.Date;
            TripMRPTable mrp = _ITripMRPService.FirstOrDefault(x => x.FromDate <= dateNow && x.ToDate >= dateNow);
            TripSettingsTable tripSettingsTable = _TripSettingsService.FirstOrDefault(x => x.EmplTripType == emplTripType && x.TripDirection == tripDirection);
            if (tripSettingsTable != null && mrp != null)
            {
                double residenceRate = Double.Parse(tripSettingsTable.ResidenceRate, CultureInfo.InvariantCulture);
                double dayRate = Double.Parse(tripSettingsTable.DayRate, CultureInfo.InvariantCulture) * Double.Parse(mrp.Amount, CultureInfo.InvariantCulture);

                var model = new USR_REQ_TRIP_RequestCalcDriveBTripCalsPTY_View(emplTripType, tripDirection, typeRequestTrip, Day, DayLive, TicketSum, (int)Math.Ceiling(dayRate), (int)residenceRate);
                return PartialView(@"~/Views/Custom/USR_REQ_TRIP_RegistrationBusinessTripPTY_Calc.cshtml", model);
            }

            return PartialView("_Empty");
        }

        [HttpPost]
        public ActionResult UpdateCalcBTripKZTRIP(byte EmplTripType, byte TripDirection, byte TypeRequestTrip, int Day, int DayLive, int TicketSum)
        {
            EmplTripType emplTripType = (EmplTripType)EmplTripType;
            TripDirection tripDirection = (TripDirection)TripDirection;
            TypeRequestTrip typeRequestTrip = (TypeRequestTrip)TypeRequestTrip;

            DateTime dateNow = DateTime.UtcNow.Date;
            TripMRPTable mrp = _ITripMRPService.FirstOrDefault(x => x.FromDate <= dateNow && x.ToDate >= dateNow);
            TripSettingsTable tripSettingsTable = _TripSettingsService.FirstOrDefault(x => x.EmplTripType == emplTripType && x.TripDirection == tripDirection);
            if (tripSettingsTable != null && mrp != null)
            {
                double residenceRate = Double.Parse(tripSettingsTable.ResidenceRate, CultureInfo.InvariantCulture);
                double dayRate = Double.Parse(tripSettingsTable.DayRate, CultureInfo.InvariantCulture) * Double.Parse(mrp.Amount, CultureInfo.InvariantCulture);

                var model = new USR_REQ_TRIP_RequestCalcDriveBTripCalsKZ_View(emplTripType, tripDirection, typeRequestTrip, Day, DayLive, TicketSum, (int)Math.Ceiling(dayRate), (int)residenceRate);
                return PartialView(@"~/Views/Custom/USR_REQ_TRIP_RegistrationBusinessTripKZ_Calc.cshtml", model);
            }

            return PartialView("_Empty");
        }

        [HttpPost]
        public ActionResult CheckIncomeDocument(Guid OrganizationId, string OutgoingNumber, DateTime OutgoingDate)
        {
            var model = _DocumentService.CheckIncomeDublicateDocument(OrganizationId, OutgoingNumber, OutgoingDate);
            return PartialView("_IncomeDocumentDublicate", model);
        }
        public ActionResult GetWorkersOrdTrip(string Workers)
        {
            List<string> workersList = Workers.Split(',').ToList();
            ViewBag.WorkerSearchList = new SelectList(workersList);
            return PartialView("USR_ORD_Workers");
        }

        public ViewResult CreateNewQuestionProtocol()
        {
            //var model = new PRT_QuestionList_Table() { DecisionList = new List<PRT_DecisionList_Table>() };
            //model.DecisionList.Add(new PRT_DecisionList_Table());
            var model = new PRT_QuestionList_Table();
            ViewData["counter"] = Guid.NewGuid().ToString("N");
            return View("_QuestionList", model);
        }

        public ViewResult CreateNewDecisionProtocol(string counter)
        {
            ViewData["counter"] = counter;
            return View("_DecisionList", new PRT_DecisionList_Table());
        }

        public ActionResult ProtocolDecisionText(PRT_DecisionList_Table model)
        {
            string result = String.Empty;

            if (model.Users != null)
            {
                string[] tmp = _SystemService.GuidsFromText(model.Users);

                if (tmp != null)
                {
                    foreach (var item in tmp)
                    {
                        EmplTable empl = _EmplService.Find(Guid.Parse(item));

                        if (empl != null)
                            result = result + String.Format("{0}, ", empl.ShortFullNameType2);
                    }
                }

                string decisionText = _SystemService.DeleteLastTagSegment(model.Decision);

                while (decisionText.EndsWith(".") || decisionText.EndsWith(",") || (decisionText.EndsWith(";") && !decisionText.EndsWith("&nbsp;")))
                    decisionText = decisionText.Substring(0, decisionText.Length - 1).TrimEnd();

                string decisionCleanText = _SystemService.DeleteAllTags(decisionText);
                
                while (decisionCleanText.EndsWith("&nbsp;"))
                {
                    decisionText = _SystemService.ReplaceLastOccurrence(decisionText, "&nbsp;", String.Empty).TrimEnd();
                    decisionCleanText = _SystemService.ReplaceLastOccurrence(decisionCleanText, "&nbsp;", String.Empty).TrimEnd();
                }

                decisionCleanText = _SystemService.DeleteAllTags(decisionCleanText);

                while (decisionCleanText.EndsWith(".") || decisionCleanText.EndsWith(",") || (decisionCleanText.EndsWith(";") && !decisionCleanText.EndsWith("&nbsp;")))
                {
                    decisionText = _SystemService.ReplaceLastOccurrence(decisionText, decisionCleanText.Last().ToString(), "").TrimEnd();
                    decisionCleanText = _SystemService.ReplaceLastOccurrence(decisionCleanText, decisionCleanText.Last().ToString(), "").TrimEnd();
                }

                decisionText = _SystemService.DeleteEmptyTag(decisionText);

                if (!String.IsNullOrEmpty(result) && !decisionText.EndsWith(">"))
                    result = String.Format("{2}, <strong>ответ. {0}срок {1}г.</strong>", result, model.ControlDate != null ? model.ControlDate.Value.ToShortDateString() : String.Empty, decisionText);
                else if(!String.IsNullOrEmpty(result) && decisionText.EndsWith(">"))
                    result = String.Format("{2} <strong>ответ. {0}срок {1}г.</strong>", result, model.ControlDate != null ? model.ControlDate.Value.ToShortDateString() : String.Empty, decisionText);
                else if (!decisionText.EndsWith(">"))
                    result = decisionText + '.';
                else
                    result = decisionText;
            }

            return PartialView("USR_PRT_DecisionText", result);
        }
	}
}