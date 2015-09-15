using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using RapidDoc.Models.Repository;

namespace RapidDoc.Models.Services
{
    public interface ISystemService
    {
        DateTime ConvertDateTimeToLocal(ApplicationUser userTable, DateTime value);
        bool IsGUID(string expression);
        string[] GuidsFromText(string text);
        string RemoveColorFromText(string text);
    }

    public class SystemService : ISystemService
    {
        public SystemService()
        {
        }
        public DateTime ConvertDateTimeToLocal(ApplicationUser userTable, DateTime value)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTable.TimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(value, timeZoneInfo);
        }
        public bool IsGUID(string expression)
        {
            if (expression != null)
            {
                Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
                return isGuid.IsMatch(expression);
            }
            return false;
        }

        public string[] GuidsFromText(string text)
        {
            string[] arrayTempStructrue = text.Split(',');
            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            string[] arrayStructure = arrayTempStructrue.Where(a => isGuid.IsMatch(a) == true).ToArray();
            return arrayStructure;
        }

        public string RemoveColorFromText(string text)
        {
            Regex rgx = new Regex(@"color: rgb([(])\d+, \d+, \d+([)]);", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return rgx.Replace(text, "");
        }
    }
}