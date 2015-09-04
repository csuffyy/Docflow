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
        private readonly ICountryService _CountryService;
        private readonly IOrganizationService _OrganizationService;
        private readonly IProcessService _ProcessService;

        public CustomController(IEmplService emplService, ISystemService systemService, IDocumentService documentService, IServiceIncidentService serviceIncidentService, ICompanyService companyService, IAccountService accountService, ITripSettingsService tripSettingsService, INumberSeqService numberSeqService, ICountryService countryService, IOrganizationService organizationService, IProcessService processService)
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
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult JsonEmpl()
        {
            var jsondata = _EmplService.GetJsonEmpl();
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
        public JsonResult JsonCreatingAccessRoles()
        {
            var jsondata = _EmplService.GetJsonCreatingAccessRoles();
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

        public ActionResult GetIncomingDocuments(RapidDoc.Models.ViewModels.USR_IND_IncomingDocuments_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "RegIncoming"))
                    {
                        return PartialView("USR_IND_IncomingDocuments_Edit", model);
                    }
                }
            }

            return PartialView("USR_IND_IncomingDocuments_View_Full", model);
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
        public ActionResult UpdateCalcTripUBUO(byte EmplTripType, byte TripDirection, int Day, int DayLive, int TicketSum)
        {
            EmplTripType emplTripType = (EmplTripType)EmplTripType;
            TripDirection tripDirection = (TripDirection)TripDirection;

            TripSettingsTable tripSettingsTable = _TripSettingsService.FirstOrDefault(x => x.EmplTripType == emplTripType && x.TripDirection == tripDirection);
            if (tripSettingsTable != null)
            {
                var model = new USR_REQ_UBUO_RequestCalcDriveTripCals_View(emplTripType, tripDirection, Day, DayLive, TicketSum, tripSettingsTable.DayRate, tripSettingsTable.ResidenceRate);
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
        public ActionResult GetManualOfficeMemoUIT(RapidDoc.Models.ViewModels.USR_OFM_UIT_OfficeMemo_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

            if ((document.DocumentState == RapidDoc.Models.Repository.DocumentState.Agreement || document.DocumentState == RapidDoc.Models.Repository.DocumentState.Execution) && _DocumentService.isSignDocument(document.Id))
            {
                var current = _DocumentService.GetCurrentSignStep(document.Id);
                if (current != null)
                {
                    if (current.Any(x => x.SystemName == "MidManager" || x.SystemName == "Manager"))
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


        public ActionResult GetManualORDMainActivity(RapidDoc.Models.ViewModels.USR_ORD_MainActivity_View model)
        {
            DocumentTable document = _DocumentService.Find(model.DocumentTableId);

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

        [HttpPost]
        public ActionResult UpdateCalcBTripPPTRIP(byte EmplTripType, byte TripDirection, int Day, int DayLive, int TicketSum)
        {
            EmplTripType emplTripType = (EmplTripType)EmplTripType;
            TripDirection tripDirection = (TripDirection)TripDirection;

            TripSettingsTable tripSettingsTable = _TripSettingsService.FirstOrDefault(x => x.EmplTripType == emplTripType && x.TripDirection == tripDirection);
            if (tripSettingsTable != null)
            {
                var model = new USR_REQ_TRIP_RequestCalcDriveBTripCalsPP_View(emplTripType, tripDirection, Day, DayLive, TicketSum, tripSettingsTable.DayRate, tripSettingsTable.ResidenceRate);
                return PartialView(@"~/Views/Custom/USR_REQ_TRIP_RegistrationBusinessTripPP_Calc.cshtml", model);
            }

            return PartialView("_Empty");
        }

        [HttpPost]
        public ActionResult UpdateCalcBTripPTY(byte EmplTripType, byte TripDirection, int Day, int DayLive, int TicketSum)
        {
            EmplTripType emplTripType = (EmplTripType)EmplTripType;
            TripDirection tripDirection = (TripDirection)TripDirection;

            TripSettingsTable tripSettingsTable = _TripSettingsService.FirstOrDefault(x => x.EmplTripType == emplTripType && x.TripDirection == tripDirection);
            if (tripSettingsTable != null)
            {
                var model = new USR_REQ_TRIP_RequestCalcDriveBTripCalsPTY_View(emplTripType, tripDirection, Day, DayLive, TicketSum, tripSettingsTable.DayRate, tripSettingsTable.ResidenceRate);
                return PartialView(@"~/Views/Custom/USR_REQ_TRIP_RegistrationBusinessTripPTY_Calc.cshtml", model);
            }

            return PartialView("_Empty");
        }

        [HttpPost]
        public ActionResult UpdateCalcBTripKZTRIP(byte EmplTripType, byte TripDirection, int Day, int DayLive, int TicketSum)
        {
            EmplTripType emplTripType = (EmplTripType)EmplTripType;
            TripDirection tripDirection = (TripDirection)TripDirection;

            TripSettingsTable tripSettingsTable = _TripSettingsService.FirstOrDefault(x => x.EmplTripType == emplTripType && x.TripDirection == tripDirection);
            if (tripSettingsTable != null)
            {
                var model = new USR_REQ_TRIP_RequestCalcDriveBTripCalsKZ_View(emplTripType, tripDirection, Day, DayLive, TicketSum, tripSettingsTable.DayRate, tripSettingsTable.ResidenceRate);
                return PartialView(@"~/Views/Custom/USR_REQ_TRIP_RegistrationBusinessTripKZ_Calc.cshtml", model);
            }

            return PartialView("_Empty");
        }
        public ActionResult GetWorkersOrdTrip(string Workers)
        {
            List<string> workersList = Workers.Split(',').ToList();
            ViewBag.WorkerSearchList = new SelectList(workersList);
            return PartialView("USR_ORD_Workers");
        }
        
	}
}