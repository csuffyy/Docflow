﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RapidDoc.Attributes;

namespace RapidDoc.Models.Repository
{
    public enum AllWSType : byte
    {
        [Display(Name = "Носимая")]
        Wearable = 0,

        [Display(Name = "Автомобильная")]
        Car = 1,

        [Display(Name = "Стационарная")]
        Stationary = 2
    }

    public enum WSType : byte
    {
        [Display(Name = "Автомобильная")]
        Car = 0,

        [Display(Name = "Стационарная")]
        Stationary = 1
    }

    public enum WSComponents : byte
    {
        [Display(Name = "Антенна автомобильная")]
        CarAntenna = 0,

        [Display(Name = "Антенна стационарная")]
        StationaryAntenna = 1,

        [Display(Name = "Преобразователь автомобильный")]
        CarInverter = 2,

        [Display(Name = "Преобразователь стационарный")]
        StationaryInverter = 3,

        [Display(Name = "Кабель RG-58")]
        Cable_RG58 = 4,

        [Display(Name = "Ручной микрофон HM-152")]
        Manual_Microphone = 5
    }

    public enum PhoneType : byte
    {
        [Display(Name = "Аналоговый")]
        Analog = 0,

        [Display(Name = "IP телефон")]
        IP = 1,

        [Display(Name = "Цифровой")]
        Digital = 2
    }

    public enum WorkPlaceMovement : byte
    {
        [Display(Name = "Перемещение")]
        Movement = 0,

        [Display(Name = "Освобождение")]
        Release = 1
    }

    public enum ActionsPhone : byte
    {
        [Display(Name = "Удаление")]
        Delete = 0,

        [Display(Name = "Резервирование")]
        Reservation = 1
    }

    public enum CTS_ServiceList : byte
    {
        [Display(Name = "Телефонная связь и АТС")]
        TelephonePBX = 0,

        [Display(Name = "Тарификатор PBX Avaya")]
        TarificatorPBXAvaya = 1,

        [Display(Name = "Радиосвязь")]
        RadioCommunication = 2,

        [Display(Name = "Радиорелейная связь")]
        MicrowaveCommunication = 3,

        [Display(Name = "Аудио конференцсвязь")]
        AudioConferencing = 4,

        [Display(Name = "IP телефония")]
        IPTelephony = 5,

        [Display(Name = "СКС")]
        SCS = 6,

        [Display(Name = "Другое")]
        Other = 7
    }

    public enum ForwardType : byte
    {
        [Display(Name = "Нет")]
        None = 0,

        [Display(Name = "Безусловная")]
        Unconditional = 1,

        [Display(Name = "По занятости")]
        Employment = 2,

        [Display(Name = "Нет ответа")]
        NoAnswer = 3
    }

    public enum ConferenceType : byte
    {
        [Display(Name = "Нет")]
        None = 0,

        [Display(Name = "Внутренняя")]
        Internal = 1,

        [Display(Name = "Внешняя")]
        External = 2,
    }

    public enum ServiceType : byte
    {
        [Display(Name = "Нет")]
        None = 0,

        [Display(Name = "Call-appr логическая линия")]
        CallAppr = 1,

        [Display(Name = "Autodial автоматический набор")]
        Autodial = 2,

        [Display(Name = "Busy-ind автоматический набор с функцией занятости абонента")]
        BusyInd = 3,

        [Display(Name = "Call-fwd самостоятельная настройка переадресации")]
        CallFwd = 4,

        [Display(Name = "Abr-prog самостоятельное программирование автоматического набора")]
        AbrProg = 5,

        [Display(Name = "Dn-dst функция не беспокоить")]
        DnDst = 6
    }

    public enum ProblemTypeCTS : byte
    {
        [Display(Name = "Другое")]
        Other = 0,

        [Display(Name = "Низкое качество звука")]
        PoorSoundQuality = 1,

        [Display(Name = "Односторонняя слышимость")]
        UnilateralAudibility = 2,

        [Display(Name = "Тишина в трубке")]
        SilenceTube = 3,

        [Display(Name = "Не проходят звонки")]
        CallsNotPass = 4,

        [Display(Name = "Нет исходящих вызовов")]
        NoOutgoingCalls = 5,

        [Display(Name = "Замена провода")]
        ReplacementCable = 6,
    }

    public enum AccessRight1C77 : byte
    {
        [Display(Name = "Просмотр")]
        View = 0,

        [Display(Name = "Изменение")]
        Edit = 1
    }

    public enum StorageType : byte
    {
        [Display(Name = "USB flash disk")]
        USBflashDisk = 0,

        [Display(Name = "DVD disk")]
        DVDdisk = 1,

        [Display(Name = "CD disk")]
        CDdisk = 2,

        [Display(Name = "USB HARD disk")]
        USBHARDdisk = 3
    }

    public enum StorageVolume : byte
    {
        [Display(Name = "4Gb")]
        V4Gb = 0,

        [Display(Name = "8Gb")]
        V8Gb = 1,

        [Display(Name = "160Gb")]
        V160Gb = 2,

        [Display(Name = "700Mb")]
        V700Mb = 3,

        [Display(Name = "250Gb")]
        V250Gb = 4,

        [Display(Name = "320Gb")]
        V320Gb = 5,

        [Display(Name = "500Gb")]
        V500Gb = 6,

        [Display(Name = "640Gb")]
        V640Gb = 7,

        [Display(Name = "750Gb")]
        V750Gb = 8,

        [Display(Name = "1Tb")]
        V1Tb = 9,

        [Display(Name = "1,5Tb")]
        V15Tb = 10,

        [Display(Name = "2Tb")]
        V2Tb = 11
    }

    public enum DeleteSignLotus : byte
    {
        [Display(Name = "Подпись")]
        Document = 0,

        [Display(Name = "Документ")]
        Signature = 1
    }

    public enum AccessRightBasic : byte
    {
        [Display(Name = "Просмотр")]
        View = 0,

        [Display(Name = "Изменение")]
        Edit = 1
    }

    public enum BlocksATK : byte
    {
        [Display(Name = "АДМИНИСТРАТИВНЫЙ БЛОК")]
        AdministrativeUnit = 0,

        [Display(Name = "БЛОК ПО ИНВЕСТИЦИЯМ и КАПИТАЛЬНОМУ СТРОИТЕЛЬСТВУ")]
        InvestBlock = 1,

        [Display(Name = "КОММЕРЧЕСКИЙ БЛОК")]
        ComerBlock = 2,

        [Display(Name = "ФИНАНСОВЫЙ БЛОК")]
        FinanceBlock = 3,

        [Display(Name = "БЛОК ПРОМЫШЛЕННОЙ БЕЗОПАСНОСТИ и ВСПОМОГАТЕЛЬНОГО ПРОИЗВОДСТВА")]
        AuxiliaryBlock = 4,

        [Display(Name = "БЛОК ГОРНОГО ПРОИЗВОДСТВА")]
        MiningBlock = 5,

        [Display(Name = "БЛОК ОБОГАЩЕНИЯ")]
        ConcetrationBlock = 6,

        [Display(Name = "ТОО Altyntau Kokshetau")]
        ATK = 8
    }

    public enum HardSoftwareMaintenance : byte
    {
        [Display(Name = "Сетевое оборудование")]
        NetworkEquipment = 0,

        [Display(Name = "Сервер")]
        Server = 1,

        [Display(Name = "Microsoft Exchange")]
        MicrosoftExchange = 2,

        [Display(Name = "Lotus Notes")]
        LotusNotes = 3,

        [Display(Name = "Microsoft DAX")]
        MicrosoftDAX = 4,

        [Display(Name = "1C")]
        ERP1C = 5,

        [Display(Name = "Microsoft SQL")]
        MicrosoftSQL = 6,

        [Display(Name = "Прочее программное обеспечение")]
        OtherSoftware = 7,

        [Display(Name = "Прочее оборудование")]
        Other = 8,

        [Display(Name = "Интернет")]
        Internet = 9,

        [Display(Name = "Active Directory")]
        AD = 10,

        [Display(Name = "VPN")]
        VPN = 11,

        [Display(Name = "Терминал-сервер")]
        Terminal = 12,

        [Display(Name = "Файловые ресурсы")]
        ShareFile = 13,

        [Display(Name = "СУД")]
        SUD = 14,

        [Display(Name = "Docflow")]
        Docflow = 15
    }

    public enum ServiceIncidientPriority : byte
    {
        [Display(Name = "Средний")]
        Medium = 0,

        [Display(Name = "Критический")]
        Critical = 1,

        [Display(Name = "Высокий")]
        High = 2,

        [Display(Name = "Низкий")]
        Low = 3
    }

    public enum ServiceIncidientLevel : byte
    {
        [Display(Name = "1 линия поддержки")]
        OneLevel = 0,

        [Display(Name = "2 линия поддержки")]
        TwoLevel = 1
    }

    public enum ServiceIncidientLocation : byte
    {
        [Display(Name = "ATK")]
        Element1 = 0,

        [Display(Name = "ЗИФ")]
        Element2 = 1
    }

    public enum AddOrChange : byte
    {
        [Display(Name = "Разработать")]
        Element1 = 0,

        [Display(Name = "Модифицировать")]
        Element2 = 1
    }

    public enum TranslateDirection : byte
    {
        [Display(Name = "С русского на английский")]
        Element1 = 0,

        [Display(Name = "С английского на русский")]
        Element2 = 1
    }

    public enum TranslateDirectionKAZ : byte
    {
        [Display(Name = "С русского на казахский")]
        Element1 = 0,

        [Display(Name = "С казахского на русский")]
        Element2 = 1
    }

    public enum TypeJUDocument : byte
    {
        [Display(Name = "Копия")]
        Element1 = 0,

        [Display(Name = "Оригинал")]
        Element2 = 1
    }

    public enum TypeJUPurposeRequest: byte
    {
        [Display(Name = "Принять в работу (активация договора и переход в JDE)")]
        Element1 = 0,

        [Display(Name = "Изменить статус")]
        Element2 = 1
    }

    public enum PipeName : byte
    {
        [Display(Name = "Хоз. Пит.")]
        Element1 = 0,

        [Display(Name = "Чаглинский пром. -хоз.водозабор")]
        Element2 = 1,

        [Display(Name = "Мырзакольсорский пром.водозабор")]
        Element3 = 2
    }

    public enum ContragentType : byte
    {
        [Display(Name = "Клиент")]
        Сustomer = 0,

        [Display(Name = "Поставщик")]
        Provider = 1,

        [Display(Name = "Клиент\\Поставщик")]
        Both = 2
    }

    public enum ObjectAccess : byte
    {
        [Display(Name = "АБК")]
        Element1 = 0,

        [Display(Name = "ЗИФ/РЗ")]
        Element2 = 1,

        [Display(Name = "ЗИФ/ОРЗ")]
        Element3 = 2,

        [Display(Name = "Пром. Площадка")]
        Element4 = 3
    }

    public enum Warehouse : byte
    {
        [Display(Name = "ГП ТМЦ (УГР, УТОР)")]
        Element1 = 0,

        [Display(Name = "ГСМ Управление техобслуживания и ремонта")]
        Element2 = 1,

        [Display(Name = "ГСМ_Участок буровзрывных работ")]
        Element3 = 2,

        [Display(Name = "ГСМ_Участок содержания дорог и отвалов")]
        Element4 = 3,

        [Display(Name = "ГСМ_Участок экскавации и транспортировки")]
        Element5 = 4,

        [Display(Name = "Забалансовый склад УТ Цех вспомогательного транспорта")]
        Element6 = 5,

        [Display(Name = "Забалансовый склад УТ Цех легкового транспорта")]
        Element7 = 6,

        [Display(Name = "Забалансовый склад УТЦОиР")]
        Element8 = 7,

        [Display(Name = "Забалансовый склад УТ Цех пассажирского транспорта")]
        Element9 = 8,

        [Display(Name = "ЗИФ Служба АСУ ТП")]
        Element10 = 9,

        [Display(Name = "ЗИФ отделение гидрометалургии")]
        Element11 = 10,

        [Display(Name = "ЗИФ Дробильное отделение")]
        Element12 = 11,

        [Display(Name = "ЗИФ ДО (Среднее дробление)")]
        Element13 = 12,

        [Display(Name = "ЗИФ Отделение сгущения, произв. водоснабжения и хвостового хозяйства")]
        Element14 = 13,

        [Display(Name = "ЗИФ Отделение флотации и гравитации")]
        Element15 = 14,

        [Display(Name = "ЗИФ Реагентное отделение")]
        Element16 = 15,

        [Display(Name = "ЗИФ Служба главного энергетика")]
        Element17 = 16,

        [Display(Name = "ЗИФ Хозяйственная служба")]
        Element18 = 17,

        [Display(Name = "Склад неликвидов")]
        Element19 = 18,

        [Display(Name = "Склад отдела охраны здоровья и техники безопасности")]
        Element20 = 19,

        [Display(Name = "Организационно-контрольная служба")]
        Element21 = 20,

        [Display(Name = "ЗИФ Отделение тонкого дробления и измельчения Корпус измельчения")]
        Element22 = 21,

        [Display(Name = "ЗИФ Отделение тонкого дробления и измельчения Корпус тонкого дробления")]
        Element23 = 22,

        [Display(Name = "Пробирно-аналитическая лаборатория (золото)")]
        Element24 = 23,

        [Display(Name = "Реагентное отделение СУиХ")]
        Element25 = 24,

        [Display(Name = "Отдел технического контроля")]
        Element26 = 25,

        [Display(Name = "Пробирно-аналитическая лаборатория")]
        Element27 = 26,

        [Display(Name = "Центральный склад СДЯВ")]
        Element28 = 27,

        [Display(Name = "Служба по связям с общественностью")]
        Element29 = 28,

        [Display(Name = "Служба учета и хранения")]
        Element30 = 29,

        [Display(Name = "Управление безопасности")]
        Element31 = 30,

        [Display(Name = "Управление информационных технологий")]
        Element32 = 31,

        [Display(Name = "Участок кучного выщелачивания")]
        Element33 = 32,

        [Display(Name = "Управление по работе с персоналом")]
        Element34 = 33,

        [Display(Name = "Управление по развитию персонала СОР")]
        Element35 = 34,

        [Display(Name = "Управление транспорта")]
        Element36 = 35,

        [Display(Name = "УТОР ЦРМ")]
        Element37 = 36,

        [Display(Name = "Служба Охраны ОС")]
        Element38 = 37,

        [Display(Name = "Сектор переводов ОКС")]
        Element39 = 38,

        [Display(Name = "Управление безопасности_золото")]
        Element40 = 39,

        [Display(Name = "УКВ золото")]
        Element41 = 40,

        [Display(Name = "ГСМ Управления транспорта")]
        Element42 = 41,

        [Display(Name = "Цех железнодорожных перевозок")]
        Element43 = 42,

        [Display(Name = "Управление технического планирования горных работ")]
        Element44 = 43,

        [Display(Name = "Объединенная котельная")]
        Element45 = 44,

        [Display(Name = "Цех сетей и подстанций")]
        Element46 = 45,

        [Display(Name = "Цех тепло и водоснабжения")]
        Element47 = 46,

        [Display(Name = "ХУ Коттедж")]
        Element48 = 47,

        [Display(Name = "ХУ Отдел эксплуатации здания")]
        Element49 = 48,

        [Display(Name = "ХУ Фитнес клуб")]
        Element50 = 49,

        [Display(Name = "Центральный склад № 1/1")]
        Element51 = 50,

        [Display(Name = "Центральный склад №2")]
        Element52 = 51,

        [Display(Name = "Центральный склад №3")]
        Element53 = 52,

        [Display(Name = "Центральный склад ГСМ")]
        Element54 = 53
    }

    public enum SettlView : byte
    {
        [Display(Name = "Основное")]
        Element1 = 0,

        [Display(Name = "Дополнительное")]
        Element2 = 1
    }

    public enum SettlType : byte
    {
        [Display(Name = "Начисление")]
        Element1 = 0,

        [Display(Name = "Удержание")]
        Element2 = 1,

        [Display(Name = "Отчисление")]
        Element3 = 2
    }

    public enum СompositionFEP : byte
    {
        [Display(Name = "Не входит")]
        Element1 = 0,

        [Display(Name = "Тарифные ставки и должностные оклады")]
        Element2 = 1,

        [Display(Name = "Выплаты стимулирующего характера")]
        Element3 = 2,

        [Display(Name = "Надбавки по выслуге лет")]
        Element4 = 3,

        [Display(Name = "Вознагрождение по итогом работы за год")]
        Element5 = 4,

        [Display(Name = "Компенсационные выплаты связанные с условиеми труда")]
        Element6 = 5,

        [Display(Name = "Оплата трудовых отпусков и компенсации за неиспользованный отпуск")]
        Element7 = 6,

        [Display(Name = "Выплаты вынужденно работавшим неполное время")]
        Element8 = 7,

        [Display(Name = "Другие денежные суммы")]
        Element9 = 8
    }

    public enum ViewCostWorkforce : byte
    {
        [Display(Name = "нет")]
        Element1 = 0,

        [Display(Name = "Другие расходы на содержание раб. силы, не отнесенные к ранее приведенным квалификационным группам")]
        Element2 = 1,

        [Display(Name = "Расходы на проведение культурных мероприятий, а также по организации отдыха и развлечений")]
        Element3 = 2,

        [Display(Name = "Расходы организации на социальную защиту работников")]
        Element4 = 3,

        [Display(Name = "Выходное пособие при расторжении трудового договора и суммы, начисленные при увольнении на период трудоустройства в связи с ликвидацией")]
        Element5 = 4,
        
        [Display(Name = "Материальная помощь, предоставленная отдельным работникам по семейным обстоятельствам, на медикаменты, погребение и т.п.")]
        Element6 = 5,

        [Display(Name = "Расходы организации по обеспечению работников жильем")]
        Element7 = 6,

        [Display(Name = "Расходы, связанные с обучением работников")]
        Element8 = 7,

        [Display(Name = "Оплата учебных отпусков, предоставляемых работникам, обучающимся в вечерних и заочных учебных организациях образования, в заочной аспирантуре")]
        Element9 = 8,

        [Display(Name = "Стипендии студентам и учащимся, направленным работодателем на обучение в учеб. заведениях, выплачиваемые за счет средств организации")]
        Element10 = 9,

        [Display(Name = "Суммы пособий по временной нетрудоспособности, выплачиваемые за счет средств организации в соответствии с законодательством")]
        Element11 = 10
    }

    public enum TypeCompensation : byte
    {
        [Display(Name = "нет")]
        Element1 = 0,

        [Display(Name = "Бесплатное лечебно-профилактическое питание")]
        Element2 = 1,

        [Display(Name = "Бесплатное получение молока или других равноценных продуктов")]
        Element3 = 2,

        [Display(Name = "Доплаты за вредные и другие неблагоприятные условия труда")]
        Element4 = 3,

        [Display(Name = "Дополнительный отпуск")]
        Element5 = 4,

        [Display(Name = "Сокращенный рабочий день")]
        Element6 = 5
    }

    public enum EmplTripType : byte
    {
        [Display(Name = "Генеральный директор")]
        GeneralManager = 0,

        [Display(Name = "Исполнительные Директора")]
        ExecutiveDirector = 1,

        [Display(Name = "Главный бухгалтер, Начальники управлений, зам. Начальников управлений, начальники самостоятельных служб, директор ЗИФ, иностранные специалисты (если условиями ТД не предусмотрено иное)")]
        HeadDepartment = 2,

        [Display(Name = "Начальники служб, секторов, цехов и участков")]
        HeadService = 3,

        [Display(Name = "Остальные ИТР, производственный персонал, рабочие и водители")]
        Other = 4,
    }

    public enum TripDirection : byte
    {
        [Display(Name = "Алматы")]
        Almaty = 0,

        [Display(Name = "Астана")]
        Astana = 1,

        [Display(Name = "Областные и районные центры")]
        DistrictCenter = 2,

        [Display(Name = "Усть-Каменогорск")]
        UstKamenogorsk = 3
    }

    public enum TripPassage : byte
    {
        [Display(Name = "Нет")]
        None = 0,

        [Display(Name = "Авиа бизнес-класс")]
        AirBussiness = 1,

        [Display(Name = "Авиа эконом-класс")]
        AirEconom = 2,

        [Display(Name = "Ж/Д (купе)")]
        Train = 3,

        [Disabled]
        [Display(Name = "Авиа по разрешению")]
        AirPermission = 4,
    }

    public enum TeachGroup : byte
    {
        [Display(Name = "Казахская")]
        Kazakh = 0,

        [Display(Name = "Русская")]
        Russian = 1
    }

    public enum RelateRate : byte
    {
        [Display(Name = "Сын/Дочь")]
        Son = 0,

        [Display(Name = "Внук/Внучка")]
        Grandson = 1,

        [Display(Name = "Племянник/Племянница")]
        Nephew = 2,

        [Display(Name = "Брат/Сестра")]
        Brother = 3
    }

    public enum HROpinion : byte
    {
        [Display(Name = "Благоприятное")]
        Fortunate = 0,

        [Display(Name = "Неблагоприятное")]
        Unfortunate = 1
    }

    public enum StatusResidence : byte
    {
        [Display(Name = "Непривлеченный")]
        Failure = 0,

        [Display(Name = "Привлеченный")]
        Recruiting = 1 
    }

    public enum HRGraphics : byte
    {
        [Display(Name = "№1 (день) (для JDE №2)")]
        Graphics1 = 0,

        [Display(Name = "№1 (день-ночь) (для JDE №1)")]
        Graphics2 = 1,

        [Display(Name = "№1 (ночь) (для JDE №8)")]
        Graphics3 = 2,

        [Display(Name = "№3 (день-ночь) (для JDE №3)")]
        Graphics4 = 3,

        [Display(Name = "№3 (день) (для JDE №5)")]
        Graphics5 = 4,

        [Display(Name = "№3 (ночь) (для JDE №9)")]
        Graphics6 = 5,

        [Display(Name = "№1 (день) для ПТО БГП (для JDE №6)")]
        Graphics7 = 6,

        [Display(Name = "№2 (для JDE №98)")]
        Graphics8 = 7,

        [Display(Name = "№4")]
        Graphics9 = 8,

        [Display(Name = "№5(ночь) (для JDE №7)")]
        Graphics10 = 9,

        [Display(Name = "№6 (день-ночь) (для JDE №10)")]
        Graphics11 = 10,
    }

    public enum HRDuration : byte
    {
        [Display(Name = "8 чаc")]
        Duration1 = 0,

        [Display(Name = "11 час")]
        Duration2 = 1,

        [Display(Name = "7.2 час")]
        Duration3 = 2
    }

    public enum HRDepartmentFirstDay : byte
    {
        [Display(Name = "Цех легкового транспорта УТ")]
        Department1 = 0,

        [Display(Name = "Цех пассажирского транспорта УТ")]
        Department2 = 1,

        [Display(Name = "Цех вспомогательного транспорта УТ")]
        Department3 = 2
    }

    public enum HRDestinationFirstDay : byte
    {
        [Display(Name = "Астана")]
        Department1 = 0,

        [Display(Name = "Областные и районные центры")]
        Department2 = 1
    }

    public enum CategoryTrip : byte
    {
        [Display(Name = "Плановый")]
        Plan = 0,

        [Display(Name = "Внеплановый")]
        UnPlan = 1
    }

    public enum TypeTrip : byte
    {
        [Display(Name = "Производственная")]
        Production = 0,

        [Display(Name = "Учебная")]
        Training = 1,

        [Display(Name = "Для участия в конференции")]
        Conference = 2,

        [Display(Name = "Для участия в семинаре")]
        Workshop = 3,

        [Display(Name = "Другое")]
        Other = 4
    }

    public enum PassageTrip : byte
    {
        [Display(Name = "Поезд")]
        Train = 0,

        [Display(Name = "Автотранспорт")]
        Car = 1,

        [Display(Name = "Самолет")]
        Airplane = 2,

        [Display(Name = "Водный транспорт")]
        Water = 3
    }

    public enum PaymentTrip : byte
    {
        [Display(Name = "Компания")]
        Element1 = 0,

        [Display(Name = "Приглашающая сторона")]
        Element2 = 1,

        [Display(Name = "Не оплачивается")]
        Element3 = 2
    }

    public enum Months : byte
    {
        [Display(Name = "Не указано")]
        Indefinite = 0,

        [Display(Name = "Январь")]
        January = 1,

        [Display(Name = "Февраль")]
        February = 2,

        [Display(Name = "Март")]
        March = 3,

        [Display(Name = "Апрель")]
        April = 4,

        [Display(Name = "Май")]
        May = 5,

        [Display(Name = "Июнь")]
        June = 6,

        [Display(Name = "Июль")]
        July = 7,

        [Display(Name = "Август")]
        August = 8,

        [Display(Name = "Сентябрь")]
        September = 9,

        [Display(Name = "Октябрь")]
        October = 10,

        [Display(Name = "Ноябрь")]
        November = 11,

        [Display(Name = "Декабрь")]
        December = 12
    }

    public enum Payment : byte
    {
        [Display(Name = "Самостоятельно")]
        Element1 = 0,

        [Display(Name = "За счет Altyntau Kokshetau")]
        Element2 = 1,

        [Display(Name = "За счет Kazzinc Holding")]
        Element3 = 2,

        [Display(Name = "За счет Kazzinc")]
        Element4 = 3
    }

    public enum PurposeTrip : byte
    {
        [Display(Name = "Встретить")]
        Element1 = 0,

        [Display(Name = "Сопроводить")]
        Element2 = 1
    }

    public enum EmplTrip : byte
    {
        [Display(Name = "Kazzinc Holding")]
        Element1 = 0,

        [Disabled]
        [Display(Name = "ATV")]
        Element2 = 1,

        [Display(Name = "Kazzinc")]
        Element3 = 2,

        [Display(Name = "Казцинктех")]
        Element4 = 3,

        [Display(Name = "Приглашенные спец.\\подрядчики")]
        Element5 = 4,

        [Display(Name = "Altyntau Kokshetau")]
        Element6 = 5,

        [Display(Name = "Gleencore")]
        Element7 = 6
    }

    public enum PurposeAuxiliaryTransportTrip : byte
    {
        [Display(Name = "Плановыми")]
        Element1 = 0,

        [Display(Name = "Производственной необходимостью")]
        Element2 = 1,

        [Display(Name = "Ликвидацией аварии")]
        Element3 = 2
    }
    public enum PayType : byte
    {
        [Display(Name = "Самостоятельно")]
        Element1 = 0,

        [Display(Name = "За счет компании")]
        Element2 = 1
    }

    public enum Folder: byte
    {
        [Display(Name = "Служебная записка")]
        Element1 = 0,

        [Display(Name = "Заявление")]
        Element4 = 3,

        [Display(Name = "Объяснительная записка")]
        Element5 = 4
    }

    public enum PhotoType: byte
    {
        [Display(Name = "Фото")]
        Element1 = 0,

        [Display(Name = "Видео")]
        Element2 = 1,

        [Display(Name = "Фото и видео")]
        Element3 = 2
    }

    public enum RequestFilterType : byte
    {
        [Display(Name = "По инициатору")]
        Initiator = 0,

        [Display(Name = "По подразделению")]
        Department = 1,

        [Display(Name = "По процессу")]
        ProcessName = 2
    }

    public enum OfficeMemoFilterType : byte
    {
        [Display(Name = "По инициатору")]
        Initiator = 0,

        [Display(Name = "По подразделению")]
        Department = 1,

        [Display(Name = "По номенклатурному делу")]
        ItemCase = 2,

        [Display(Name = "По теме документа")]
        Topic = 3,

        [Display(Name = "По процессу")]
        ProcessName = 4,

        [Display(Name = "Папка")]
        Folder = 5,

        [Display(Name = "По дате")]
        CreateDate = 6

    }

    public enum TaskFilterType : byte
    {
        [Display(Name = "По инициатору")]
        Initiator = 0,

        [Display(Name = "По подразделению")]
        Department = 1,

        [Display(Name = "По исполнителям")]
        Executors = 2
    }

    public enum OrderFilterType : byte
    {
        [Display(Name = "По инициатору")]
        Initiator = 0,

        [Display(Name = "По подразделению")]
        Department = 1,

        [Display(Name = "По номеру приказа")]
        NumberOrder = 2,

        [Display(Name = "По теме документа")]
        Topic = 3,

        [Display(Name = "По процессу")]
        ProcessName = 4,

        [Display(Name = "По дате приказа")]
        OrderDate = 5
    }
    public enum IncomingFilterType : byte
    {
        [Display(Name = "По инициатору")]
        Initiator = 0,

        [Display(Name = "По подразделению")]
        Department = 1,

        [Display(Name = "По теме документа")]
        Topic = 2,

        [Display(Name = "По регистрационному номеру")]
        RegistrationNumber = 3,

        [Display(Name = "По дате регистрации")]
        RegistrationDate = 4,

        [Display(Name = "По корреспонденту")]
        Organization = 5,

        [Display(Name = "По номенклатурному делу")]
        ItemCase = 6
    }

    public enum OutcomingFilterType : byte
    {
        [Display(Name = "По инициатору")]
        Initiator = 0,

        [Display(Name = "По подразделению")]
        Department = 1,

        [Display(Name = "По теме документа")]
        Topic = 2,

        [Display(Name = "По регистрационному номеру")]
        RegistrationNumber = 3,

        [Display(Name = "По дате исходящего")]
        RegistrationDate = 4,

        [Display(Name = "По корреспонденту")]
        Organization = 5,

        [Display(Name = "По номенклатурному делу")]
        ItemCase = 6,

        [Display(Name = "По виду отправки")]
        OutDispatchType = 7
    }

    public enum AppealFilterType : byte
    {
        [Display(Name = "По инициатору")]
        Initiator = 0,

        [Display(Name = "По регистрационному номеру")]
        RegistrationNumber = 1,

        [Display(Name = "По дате регистрации")]
        RegistrationDate = 2,

        [Display(Name = "По теме документа")]
        Topic = 3,

        [Display(Name = "По корреспонденту")]
        Organization = 5,

        [Display(Name = "По номенклатурному делу")]
        ItemCase = 6
    }

    public enum ProtocolFilterType : byte
    {
        [Display(Name = "[Протокол] По инициатору")]
        Initiator = 0,

        [Display(Name = "[Протокол] По подразделению")]
        Department = 1,

        [Display(Name = "[Протокол] По теме документа")]
        Topic = 2,

        [Display(Name = "[Протокол] По папкам")]
        Folder = 3,

        [Display(Name = "[Протокол] По дате")]
        CreateDate = 4,

        [Display(Name = "[Поручения] По статусу")]
        TaskStatus = 5,

        [Display(Name = "[Поручения] По исполнителям")]
        TaskExecutor = 6,

        [Display(Name = "[Продление сроков] По исполнителям")]
        ProlongTaskExecutor = 7,

        [Display(Name = "[Продление сроков] По председателям")]
        ProlongTaskChairman = 8,

        [Display(Name = "[Продление сроков] По статусу")]
        ProlongTaskStatus = 9
    }

    public enum DiscussionFilterType : byte
    {
        [Display(Name = "По инициатору")]
        Initiator = 0,

        [Display(Name = "По подразделению")]
        Department = 1,

        [Display(Name = "По теме документа")]
        Topic = 2,

        [Display(Name = "По папкам")]
        Folder = 3,

        [Display(Name = "По дате")]
        CreateDate = 4
    }

    public enum ProtocolTaskDocumentBaseStatus : byte
    {
        [Display(Name = "В работе")]
        AtWork = 0,

        [Display(Name = "Выполненные")]
        Executed = 1,

        [Display(Name = "Просроченные")]
        Overdue = 2
    }

    public enum ProtocolProlongTaskDocumentBaseStatus : byte
    {
        [Display(Name = "В работе")]
        AtWork = 0,

        [Display(Name = "Выполненные")]
        Executed = 1
    }

    public enum BusinessTripType : byte
    {
        [Display(Name = "Производственная")]
        Element1 = 0,

        [Display(Name = "Учебная")]
        Element2 = 1,

        [Display(Name = "Для участия в конференции")]
        Element3 = 2,

        [Display(Name = "Для участия в семинаре")]
        Element4 = 3,

        [Display(Name = "Другое")]
        Element5 = 4
    }

    public enum BusinessTripCategory : byte
    {
        [Display(Name = "Плановая")]
        Element1 = 0,

        [Display(Name = "Внеплановая")]
        Element2 = 1
    }

    public enum BusinessTripDestination : byte
    {
        [Display(Name = "Республика Казахстан")]
        Element1 = 0,

        [Display(Name = "СНГ")]
        Element2 = 1,

        [Display(Name = "Дальнее зарубежье")]
        Element3 = 2,
    }

    public enum PaymentType : byte
    {
        [Display(Name = "Компания")]
        Element1 = 0,

        [Display(Name = "Приглашающая сторона")]
        Element2 = 1,

        [Display(Name = "Не оплачивается")]
        Element3 = 2,
    }

    public enum ControlType : byte
    {
        [Display(Name = "Контроль")]
        Element1 = 0,

        [Display(Name = "Особый контроль")]
        Element2 = 1,

        [Display(Name = "К исполнению")]
        Element3 = 2,

        [Display(Name = "Для ознакомления")]
        Element4 = 3,

        [Display(Name = "Ответ на запрос")]
        Element5 = 4,

        [Display(Name = "На рассмотрение")]
        Element6 = 5
    }

    public enum NatureIncomingQuestion : byte
    {
        [Display(Name = "Нет")]
        Element1 = 0,

        [Display(Name = "Вопрос закупки")]
        Element2= 1,

        [Display(Name = "Для сведения")]
        Element3 = 2,

        [Display(Name = "Запрос информации")]
        Element4 = 3,

        [Display(Name = "Кадровый")]
        Element5 = 4,

        [Display(Name = "Коммерческое предложение")]
        Element6 = 5,

        [Display(Name = "Личный")]
        Element7 = 6,

        [Display(Name = "Повестка в суд")]
        Element8 = 7,

        [Display(Name = "Предоставление докуметов")]
        Element9 = 8,

        [Display(Name = "Претензия")]
        Element10 = 9,

        [Display(Name = "Приглашение")]
        Element11 = 10,

        [Display(Name = "Производственный")]
        Element12 = 11,

        [Display(Name = "Спонсорская помощь")]
        Element13 = 12,

        [Display(Name = "Технический")]
        Element14 = 13,

        [Display(Name = "Финансовый")]
        Element15 = 14
    }

    public enum NatureIncomingQuestionKZC : byte
    {
        [Display(Name = "Нет")]
        Element1 = 0,

        [Display(Name = "Комплексный запрос")]
        Element2 = 1,

        [Display(Name = "Вопросы, связанные с приобретением либо продажей Компанией доли в юридических лицах")]
        Element3 = 2,

        [Display(Name = "Геологоразведка")]
        Element4 = 3,

        [Display(Name = "Горное производство")]
        Element5 = 4,

        [Display(Name = "Деятельность профсоюзной организации")]
        Element6 = 5,

        [Display(Name = "Инвестиционные проекты")]
        Element7 = 6,

        [Display(Name = "Инновационная деятельность")]
        Element8 = 7,

        [Display(Name = "Исполнение обязательств казахстанского содержания")]
        Element9 = 8,

        [Display(Name = "Кадровые вопросы")]
        Element10 = 9,

        [Display(Name = "Контакты с международными организациями")]
        Element11 = 10,

        [Display(Name = "Лизензирование, стандартизация и сертификация")]
        Element12 = 11,

        [Display(Name = "Налогообложение")]
        Element13 = 12,

        [Display(Name = "Обогатительное производство")]
        Element14 = 13,

        [Display(Name = "Объемы производства")]
        Element15 = 14,

        [Display(Name = "Приобретение материалов и оборудования")]
        Element16 = 15,

        [Display(Name = "Промышленная безопасность")]
        Element17 = 16,

        [Display(Name = "Разрешение проблемных вопросов в деятельности дочерних предприятий")]
        Element18 = 17,

        [Display(Name = "Рынки сбыта, экспорт")]
        Element19 = 18,

        [Display(Name = "Социальные вопросы")]
        Element20 = 19,

        [Display(Name = "Стоимость и себестоимость  продукции")]
        Element21 = 20,

        [Display(Name = "Страхование")]
        Element22 = 21,

        [Display(Name = "Таможенное оформление и регулирование")]
        Element23 = 22,

        [Display(Name = "Тендерная площадка")]
        Element24 = 23,

        [Display(Name = "Технологическое развитие")]
        Element25 = 24,

        [Display(Name = "Трудовые отношения (заработная плата, численность работников, потребность в персонале и т.п.)")]
        Element26 = 25,

        [Display(Name = "Финансирование проектов")]
        Element27 = 26,

        [Display(Name = "Экология (отходы, вода, воздух)")]
        Element28 = 27,

        [Display(Name = "Энергосбережение, энергоснабжение и энергоэффективность")]
        Element29 = 28
    }

    public enum IncomingDocumentType : byte
    {
        [Display(Name = "Нет")]
        Element1 = 0,

        [Display(Name = "Акт")]
        Element2 = 1,

        [Display(Name = "Доверенность")]
        Element3 = 2,

        [Display(Name = "Накладная")]
        Element4 = 3,

        [Display(Name = "Переписка с акиматом")]
        Element5 = 4,

        [Display(Name = "Письмо")]
        Element6 = 5,

        [Display(Name = "Постановление")]
        Element7 = 6,

        [Display(Name = "Приказ")]
        Element8 = 7,

        [Display(Name = "Протокол")]
        Element9 = 8,

        [Display(Name = "Распоряжение")]
        Element10 = 9,

        [Display(Name = "Служебная записка")]
        Element11 = 10,

        [Display(Name = "Справка")]
        Element12 = 11,

        [Display(Name = "Телефонограмма")]
        Element13 =12,

        [Display(Name = "Уведомление")]
        Element14 = 13
    }

    public enum IncomingDocumentKZHCType : byte
    {
        [Display(Name = "Нет")]
        Element1 = 0,

        [Display(Name = "Запрос информации")]
        Element49 = 50,

        [Display(Name = "Приглашение")]
        Element50 = 51,

        [Display(Name = "НПА")]
        Element51 = 52,

        [Display(Name = "Коммерческое предложение")]
        Element52 = 53,

        [Display(Name = "Спонсорская помощь")]
        Element53 = 54,

        [Display(Name = "Предложение о сотрудничестве")]
        Element54 = 55,

        [Display(Name = "Просьба о содействии")]
        Element55 = 56,

        [Display(Name = "Повестка")]
        Element56 = 57,

        [Display(Name = "Предписание")]
        Element57 = 58,

        [Display(Name = "Уведомление")]
        Element14 = 13
    }

    public enum OutcomingDocumentKZHCType : byte
    {
        [Display(Name = "Нет")]
        Element1 = 0,

        [Display(Name = "Запрос информации")]
        Element49 = 50,

        [Display(Name = "Приглашение")]
        Element50 = 51,

        [Display(Name = "Ответ на запрос")]
        Element51 = 52,

        [Display(Name = "Коммерческое предложение")]
        Element52 = 53,

        [Display(Name = "Спонсорская помощь")]
        Element53 = 54,

        [Display(Name = "Предложение о сотрудничестве")]
        Element54 = 55,

        [Display(Name = "Просьба о содействии")]
        Element55 = 56,

        [Display(Name = "Результат рассмотрения")]
        Element56 = 57,

        [Display(Name = "Согласие/отказ")]
        Element57 = 58,

        [Display(Name = "Заявка")]
        Element58 = 59,

        [Display(Name = "Подтверждение участия")]
        Element59 = 60,

        [Display(Name = "Рассмотрение проектов законов и нормативных актов")]
        Element60 = 61,

        [Display(Name = "Уведомление")]
        Element14 = 13
    }

    public enum IncomingAdmissionMethodType : byte
    {
        [Display(Name = "Почта")]
        Element1 = 0,

        [Display(Name = "Электронная почта")]
        Element2 = 1,

        [Display(Name = "Факс")]
        Element3 = 2,

        [Display(Name = "Нарочно")]
        Element4 = 3,

        [Display(Name = "Телефонограмма")]
        Element5 = 4
    }

    public enum OutcomingDispatchType : byte
    {
        [Display(Name = "Нет")]
        Element1 = 0,

        [Display(Name = "Заказное письмо (с уведомлением)")]
        Element2 = 1,

        [Display(Name = "На руки")]
        Element3 = 2,

        [Display(Name = "Нарочно (в журнале)")]
        Element4 = 3,

        [Display(Name = "Письмо")]
        Element5 = 4,

        [Display(Name = "По системе документооборота")]
        Element6 = 5,

        [Display(Name = "Почта DHL")]
        Element7 = 6,

        [Display(Name = "Почта спецсвязи")]
        Element8 = 7,

        [Display(Name = "Телеграмма")]
        Element9 = 8,

        [Display(Name = "Факс")]
        Element10 = 9,

        [Display(Name = "Фельдсвязь")]
        Element11 = 10
    }

    public enum OutcomingDispatchTypeKZHC : byte
    {
        [Display(Name = "Нет")]
        Element1 = 0,

        [Display(Name = "Почта")]
        Element2 = 1,

        [Display(Name = "Электронная почта")]
        Element3 = 2,

        [Display(Name = "Факс")]
        Element4 = 3,

        [Display(Name = "Нарочно")]
        Element5 = 4,

        [Display(Name = "Телефонограмма")]
        Element6 = 5
    }

    public enum OutcomingTopicTypeKZHC : byte
    {
        [Display(Name = "Иное")]
        Element1 = 0,

        [Display(Name = "Корреспонденция по вопросам законотворческой деятельности")]
        Element2 = 1,

        [Display(Name = "Корреспонденция по вопросам недропользования")]
        Element3 = 2
    }

    public enum CategoryPerson : byte
    {
        [Display(Name = "Физическое лицо")]
        Element1 = 0,

        [Display(Name = "Юридическое лицо")]
        Element2 = 1
    }

    public enum StatusPerson : byte
    {
        [Display(Name = "Гражданин")]
        Element1 = 0,

        [Display(Name = "Пенсионер")]
        Element2 = 1,

        [Display(Name = "Сотрудник")]
        Element3 = 2
    }

    public enum FormAppeal : byte
    {
        [Display(Name = "Бумажная")]
        Element1 = 0,

        [Display(Name = "Электронная")]
        Element2 = 1,

        [Display(Name = "Устная")]
        Element3 = 2,

        [Display(Name = "На личном приеме")]
        Element4 = 3
    }

    public enum TypeAppeal : byte
    {
        [Display(Name = "Не выбрано")]
        Element1 = 0,

        [Display(Name = "Жалоба")]
        Element2 = 1,

        [Display(Name = "Заявление")]
        Element3 = 2,

        [Display(Name = "Коммерческое предложение")]
        Element4 = 3,

        [Display(Name = "Материальная помощь")]
        Element5 = 4,

        [Display(Name = "О предоставлении справки")]
        Element6 = 5,

        [Display(Name = "Обращение")]
        Element7 = 6,

        [Display(Name = "Претензия")]
        Element8 = 7,

        [Display(Name = "Спонсорская помощь")]
        Element9 = 8
    }

    public enum CharacterAppeal : byte
    {
        [Display(Name = "Коллективное")]
        Element1 = 0,

        [Display(Name = "Индивидуальное")]
        Element2 = 1
    }

    public enum CharacterQuestion : byte
    {
        [Display(Name = "Не выбрано")]
        Element1 = 0,

        [Display(Name = "Вопрос закупки")]
        Element2 = 1,

        [Display(Name = "Для сведения")]
        Element3 = 2,

        [Display(Name = "Запрос информации")]
        Element4 = 3,

        [Display(Name = "Кадровый")]
        Element5 = 4,

        [Display(Name = "Коммерческое предложение")]
        Element6 = 5,

        [Display(Name = "Личный")]
        Element7 = 6,

        [Display(Name = "Повестка в суд")]
        Element8 = 7,

        [Display(Name = "Предоставление документов")]
        Element9 = 8,

        [Display(Name = "Претензия")]
        Element10 = 9,

        [Display(Name = "Приглашение")]
        Element11 = 10,

        [Display(Name = "Производственный")]
        Element12 = 11,

        [Display(Name = "Спонсорская помощь")]
        Element13 = 12,

        [Display(Name = "Технический")]
        Element14 = 13,

        [Display(Name = "Финансовый")]
        Element15 = 14
    }

    public enum TypeRequestTrip : byte
    {
        [Display(Name = "Командировка")]
        Trip = 0,

        [Display(Name = "Продление")]
        Continents = 1
    }

    public enum ReportExecutionType : byte
    {     
        Done = 0,       
        NoneDone = 1,
        OverDate = 2
    }

    public enum ChangeOrderType : byte
    {
        [Display(Name = "Кадровый приказ")]
        Element1 = 0,

        [Display(Name = "По основной деятельности")]
        Element2 = 1
    }

    public enum UMMDays : byte
    {
        [Display(Name = "В будние дни")]
        Weekdays = 0,

        [Display(Name = "В ночные, выходные и праздничные дни")]
        Weekends = 1
    }

    public enum UMMItem : byte
    {
        [Display(Name = "Услуга")]
        Element1 = 0,

        [Display(Name = "Изделие")]
        Element2 = 1
    }

    public enum TaskScheduleTypePeriod : byte
    {
        //[Display(Name = "Ежедневно")]
        //Daily = 0,

        [Display(Name = "Еженедельно")]
        Weekly = 0,

        [Display(Name = "Ежемесячно")]
        Monthly = 1
    } 
}
