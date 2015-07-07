using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using RapidDoc.Models.Repository;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Data.Entity.Core;
using System.Transactions;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNet.Identity;

namespace RapidDoc.Models.Services
{
    public interface INumberSeqService
    {
        IEnumerable<NumberSeriesTable> GetAll();
        IEnumerable<NumberSeriesBookingTable> GetAllBooking();
        IEnumerable<NumberSeriesView> GetAllView();
        IEnumerable<NumberSeriesBookingView> GetAllViewBooking();
        IEnumerable<NumberSeriesTable> GetPartial(Expression<Func<NumberSeriesTable, bool>> predicate);
        IEnumerable<NumberSeriesBookingTable> GetPartialBooking(Expression<Func<NumberSeriesBookingTable, bool>> predicate);
        IEnumerable<NumberSeriesView> GetPartialView(Expression<Func<NumberSeriesTable, bool>> predicate);
        IEnumerable<NumberSeriesBookingView> GetPartialViewBooking(Expression<Func<NumberSeriesBookingTable, bool>> predicate);
        NumberSeriesTable FirstOrDefault(Expression<Func<NumberSeriesTable, bool>> predicate);
        NumberSeriesBookingTable FirstOrDefaultBooking(Expression<Func<NumberSeriesBookingTable, bool>> predicate);
        NumberSeriesView FirstOrDefaultView(Expression<Func<NumberSeriesTable, bool>> predicate);
        NumberSeriesBookingView FirstOrDefaultViewBooking(Expression<Func<NumberSeriesBookingTable, bool>> predicate);
        void Save(NumberSeriesView viewTable);
        void SaveBooking(NumberSeriesBookingView viewTable);
        void SaveDomain(NumberSeriesTable domainTable, string currentUserId = "");
        void SaveDomainBooking(NumberSeriesBookingTable domainTable, string currentUserId = "");
        void Delete(Guid id);
        void DeleteBooking(Guid id);
        NumberSeriesTable Find(Guid id);
        NumberSeriesBookingTable FindBooking(Guid id);
        NumberSeriesView FindView(Guid id);
        NumberSeriesBookingView FindViewBooking(Guid id);
        bool BookingContains(Expression<Func<NumberSeriesBookingTable, bool>> predicate);
        SelectList GetDropListNumberSeqNull(Guid? id);
        SelectList GetDropListNumberSeq(Guid? id);
        SelectList GetDropListNumberSeqBookingNull(Guid numberSeqId, Guid? id);
        string GetDocumentNum(Guid id, string currentUserId = "");
        string GetDocumentNumORD(Guid id, Guid? bookingNumberId, string currentUserId = "");
    }

    public class NumberSeqService : INumberSeqService
    {
        private IRepository<NumberSeriesTable> repo;
        private IRepository<NumberSeriesBookingTable> repoBooking;
        private IUnitOfWork _uow;

        public NumberSeqService(IUnitOfWork uow)
        {
            _uow = uow;
            repo = uow.GetRepository<NumberSeriesTable>();
            repoBooking = uow.GetRepository<NumberSeriesBookingTable>();
        }
        public IEnumerable<NumberSeriesTable> GetAll()
        {
            return repo.All();
        }
        public IEnumerable<NumberSeriesBookingTable> GetAllBooking()
        {
            return repoBooking.All();
        }
        public IEnumerable<NumberSeriesView> GetAllView()
        {
            var items = Mapper.Map<IEnumerable<NumberSeriesTable>, IEnumerable<NumberSeriesView>>(GetAll());
            return items;
        }
        public IEnumerable<NumberSeriesBookingView> GetAllViewBooking()
        {
            var items = Mapper.Map<IEnumerable<NumberSeriesBookingTable>, IEnumerable<NumberSeriesBookingView>>(GetAllBooking());
            return items;
        }
        public IEnumerable<NumberSeriesTable> GetPartial(Expression<Func<NumberSeriesTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<NumberSeriesBookingTable> GetPartialBooking(Expression<Func<NumberSeriesBookingTable, bool>> predicate)
        {
            return repoBooking.FindAll(predicate);
        }
        public IEnumerable<NumberSeriesView> GetPartialView(Expression<Func<NumberSeriesTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<NumberSeriesTable>, IEnumerable<NumberSeriesView>>(GetPartial(predicate));
            return items;
        }
        public IEnumerable<NumberSeriesBookingView> GetPartialViewBooking(Expression<Func<NumberSeriesBookingTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<NumberSeriesBookingTable>, IEnumerable<NumberSeriesBookingView>>(GetPartialBooking(predicate));
            return items;
        }
        public NumberSeriesTable FirstOrDefault(Expression<Func<NumberSeriesTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }
        public NumberSeriesBookingTable FirstOrDefaultBooking(Expression<Func<NumberSeriesBookingTable, bool>> predicate)
        {
            return repoBooking.Find(predicate);
        }
        public NumberSeriesView FirstOrDefaultView(Expression<Func<NumberSeriesTable, bool>> predicate)
        {
            return Mapper.Map<NumberSeriesTable, NumberSeriesView>(FirstOrDefault(predicate));
        }
        public NumberSeriesBookingView FirstOrDefaultViewBooking(Expression<Func<NumberSeriesBookingTable, bool>> predicate)
        {
            return Mapper.Map<NumberSeriesBookingTable, NumberSeriesBookingView>(FirstOrDefaultBooking(predicate));
        }
        public void Save(NumberSeriesView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new NumberSeriesTable();
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
        public void SaveBooking(NumberSeriesBookingView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new NumberSeriesBookingTable();
                Mapper.Map(viewTable, domainTable);
                SaveDomainBooking(domainTable);
            }
            else
            {
                var domainTable = FindBooking(viewTable.Id ?? Guid.Empty);
                Mapper.Map(viewTable, domainTable);
                SaveDomainBooking(domainTable);
            }
        }
        public void SaveDomain(NumberSeriesTable domainTable, string currentUserId = "")
        {
            string userid = getCurrentUserId(currentUserId);
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userid;
                domainTable.ApplicationUserModifiedId = userid;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userid;
                repo.Update(domainTable);
            }
            _uow.Commit();
        }

        public void SaveDomainBooking(NumberSeriesBookingTable domainTable, string currentUserId = "")
        {
            string userid = getCurrentUserId(currentUserId);
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userid;
                domainTable.ApplicationUserModifiedId = userid;
                repoBooking.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userid;
                repoBooking.Update(domainTable);
            }
            _uow.Commit();
        }
        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            _uow.Commit();
        }
        public void DeleteBooking(Guid id)
        {
            repoBooking.Delete(a => a.Id == id);
            _uow.Commit();
        }
        public NumberSeriesTable Find(Guid id)
        {
            return repo.GetById(id);
        }
        public NumberSeriesBookingTable FindBooking(Guid id)
        {
            return repoBooking.GetById(id);
        }
        public NumberSeriesView FindView(Guid id)
        {
            return Mapper.Map<NumberSeriesTable, NumberSeriesView>(Find(id));
        }
        public NumberSeriesBookingView FindViewBooking(Guid id)
        {
            return Mapper.Map<NumberSeriesBookingTable, NumberSeriesBookingView>(FindBooking(id));
        }
        public bool BookingContains(Expression<Func<NumberSeriesBookingTable, bool>> predicate)
        {
            return repoBooking.Contains(predicate);
        }
        public SelectList GetDropListNumberSeqNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new NumberSeriesView { NumberSeriesName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "NumberSeriesName", id);
        }
        public SelectList GetDropListNumberSeq(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "NumberSeriesName", id);
        }
        public SelectList GetDropListNumberSeqBookingNull(Guid numberSeqId, Guid? id)
        {
            var items = GetPartialViewBooking(x => x.NumberSeriesTableId == numberSeqId && x.Enable == true).ToList();
            items.Insert(0, new NumberSeriesBookingView { NumberSeq = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "NumberSeq", id);
        }
        public string GetDocumentNum(Guid id, string currentUserId = "")
        {
            var numberSeq = Find(id);
            do
            {
                try
                {
                    numberSeq = Find(id);
                    numberSeq.LastNum++;
                    SaveDomain(numberSeq, currentUserId);
                    string num = numberSeq.Prefix + numberSeq.LastNum.ToString("D" + numberSeq.Size.ToString());
                    return num;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = (NumberSeriesTable)entry.GetDatabaseValues().ToObject();
                    var clientValues = (NumberSeriesTable)entry.Entity;
                    numberSeq.TimeStamp = databaseValues.TimeStamp;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            } while (true);
        }

        public string GetDocumentNumORD(Guid id, Guid? bookingNumberId, string currentUserId = "")
        {
            var numberSeq = Find(id);
            do
            {
                try
                {
                    numberSeq = Find(numberSeq.Id);

                    if (bookingNumberId != null && bookingNumberId != Guid.Empty)
                    {
                        NumberSeriesBookingTable numberSeqBookingTable = FirstOrDefaultBooking(x => x.Id == bookingNumberId);
                        numberSeqBookingTable.Enable = false;
                        SaveDomainBooking(numberSeqBookingTable, currentUserId);
                        return numberSeqBookingTable.LastNum + "-" + numberSeqBookingTable.Prefix;
                    }
                    else
                    {
                        do
                        {
                            numberSeq.LastNum++;
                        }
                        while (BookingContains(x => x.LastNum == numberSeq.LastNum && x.NumberSeriesTableId == numberSeq.Id));
                        
                        SaveDomain(numberSeq, currentUserId);
                        string num = numberSeq.LastNum + "-" + numberSeq.Prefix;
                        return num;
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = (NumberSeriesTable)entry.GetDatabaseValues().ToObject();
                    var clientValues = (NumberSeriesTable)entry.Entity;
                    numberSeq.TimeStamp = databaseValues.TimeStamp;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            } while (true);
        }

        private string getCurrentUserId(string currentUserId = "")
        {
            if (currentUserId != string.Empty)
            {
                return currentUserId;
            }
            else
            {
                return HttpContext.Current.User.Identity.GetUserId();
            }
        }
    }
}