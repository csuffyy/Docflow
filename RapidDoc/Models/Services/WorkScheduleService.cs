﻿using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity;

namespace RapidDoc.Models.Services
{
    public interface IWorkScheduleService
    {
        IEnumerable<WorkScheduleTable> GetAll();
        IEnumerable<WorkScheduleView> GetAllView();
        IEnumerable<WorkScheduleTable> GetPartial(Expression<Func<WorkScheduleTable, bool>> predicate);
        IEnumerable<WorkScheduleView> GetPartialView(Expression<Func<WorkScheduleTable, bool>> predicate);
        WorkScheduleTable FirstOrDefault(Expression<Func<WorkScheduleTable, bool>> predicate);
        WorkScheduleView FirstOrDefaultView(Expression<Func<WorkScheduleTable, bool>> predicate);
        void Save(WorkScheduleView viewTable);
        void SaveDomain(WorkScheduleTable domainTable);
        void Delete(Guid id);
        WorkScheduleTable Find(Guid id);
        WorkScheduleView FindView(Guid id);
        SelectList GetDropListWorkScheduleNull(Guid? id);
        SelectList GetDropListWorkSchedule(Guid? id);
        void SaveDayToCalendar(Guid workScheduleId, DateTime date);
        bool CheckDayType(Guid workScheduleId, DateTime date);
        DateTime[] GetDaysOff(Guid workScheduleId);
        bool CheckWorkTime(Guid? workScheduleId, DateTime date);
    }

    public class WorkScheduleService : IWorkScheduleService
    {
        private IRepository<WorkScheduleTable> repo;
        private IRepository<СalendarTable> repoCalendar;
        private IUnitOfWork _uow;

        public WorkScheduleService(IUnitOfWork uow)
        {
            _uow = uow;
            repo = uow.GetRepository<WorkScheduleTable>();
            repoCalendar = uow.GetRepository<СalendarTable>();
        }
        public IEnumerable<WorkScheduleTable> GetAll()
        {
            return repo.All();
        }
        public IEnumerable<WorkScheduleView> GetAllView()
        {
            var items = Mapper.Map<IEnumerable<WorkScheduleTable>, IEnumerable<WorkScheduleView>>(GetAll());
            return items;
        }
        public IEnumerable<WorkScheduleTable> GetPartial(Expression<Func<WorkScheduleTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<WorkScheduleView> GetPartialView(Expression<Func<WorkScheduleTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<WorkScheduleTable>, IEnumerable<WorkScheduleView>>(GetPartial(predicate));
            return items;
        }
        public WorkScheduleTable FirstOrDefault(Expression<Func<WorkScheduleTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }
        public WorkScheduleView FirstOrDefaultView(Expression<Func<WorkScheduleTable, bool>> predicate)
        {
            return Mapper.Map<WorkScheduleTable, WorkScheduleView>(FirstOrDefault(predicate));
        }
        public void Save(WorkScheduleView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new WorkScheduleTable();
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
            else
            {
                var domainTable = Find(viewTable.Id ?? Guid.Empty);
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
        }
        public void SaveDomain(WorkScheduleTable domainTable)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userId;
                domainTable.ApplicationUserModifiedId = userId;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userId;
                repo.Update(domainTable);
            }
            _uow.Commit();
        }
        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            _uow.Commit();
        }
        public WorkScheduleTable Find(Guid id)
        {
            return repo.GetById(id);
        }
        public WorkScheduleView FindView(Guid id)
        {
            return Mapper.Map<WorkScheduleTable, WorkScheduleView>(Find(id));
        }
        public SelectList GetDropListWorkScheduleNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new WorkScheduleView { WorkScheduleName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "WorkScheduleName", id);
        }
        public SelectList GetDropListWorkSchedule(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "WorkScheduleName", id);
        }

        // Calandar
        public void SaveDayToCalendar(Guid workScheduleId, DateTime date)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            СalendarTable calendar;

            if(repoCalendar.Contains(x => x.WorkScheduleTableId == workScheduleId && x.Date == date))
            {
                calendar = repoCalendar.Find(x => x.WorkScheduleTableId == workScheduleId && x.Date == date);
                calendar.ModifiedDate = DateTime.UtcNow;
                calendar.ApplicationUserModifiedId = userId;
                if (calendar.DateType == DateType.DayOff)
                {
                    calendar.DateType = DateType.WorkingDay;
                }
                else
                {
                    calendar.DateType = DateType.DayOff;
                }

                _uow.GetRepository<СalendarTable>().Update(calendar);
            }
            else
            {
                calendar = new СalendarTable();
                calendar.Date = date;
                calendar.DateType = DateType.DayOff;
                calendar.WorkScheduleTableId = workScheduleId;
                calendar.CreatedDate = DateTime.UtcNow;
                calendar.ModifiedDate = calendar.CreatedDate;
                calendar.ApplicationUserCreatedId = userId;
                calendar.ApplicationUserModifiedId = userId;
                _uow.GetRepository<СalendarTable>().Add(calendar);
            }

            _uow.Commit();
        }
        public bool CheckDayType(Guid workScheduleId, DateTime date)
        {
            if(repoCalendar.Contains(x => x.WorkScheduleTableId == workScheduleId && x.Date == date))
            {
                СalendarTable calendar = repoCalendar.Find(x => x.WorkScheduleTableId == workScheduleId && x.Date == date);
                if(calendar.DateType == DateType.DayOff)
                {
                    return true;
                }
            }

            return false;
        }
        public bool CheckWorkTime(Guid? workScheduleId, DateTime date)
        {
            WorkScheduleTable schedule = null;

            if(workScheduleId == null)
            {
                schedule = repo.Find(x => x.WorkScheduleName != null);
            }
            else
            {
                schedule = repo.GetById(workScheduleId ?? Guid.Empty);
            }

            if (schedule != null)
            {
                DateTime startTime = new DateTime(date.Year, date.Month, date.Day) + schedule.WorkStartTime;
                DateTime endTime = new DateTime(date.Year, date.Month, date.Day) + schedule.WorkEndTime;

                if (startTime > date || endTime < date)
                {
                    return false;
                }

                СalendarTable calendar = repoCalendar.Find(x => x.WorkScheduleTableId == schedule.Id && x.Date == date.Date);

                if (calendar != null && calendar.DateType == DateType.DayOff)
                {
                    return false;
                }
            }

            return true;
        }
        public DateTime[] GetDaysOff(Guid workScheduleId)
        {
            var items = repoCalendar.FindAll(x => x.WorkScheduleTableId == workScheduleId).Select(x => x.Date);
            return items.ToArray();
        }
    }
}