﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using RapidDoc.Models.DomainModels;

namespace RapidDoc.Models.Services
{
    public interface ISystemService
    {
        DateTime ConvertDateTimeToLocal(ApplicationUser userTable, DateTime value);

        DateTime ConvertDateTimeToLocal(TimeZoneInfo timeZoneInfo, DateTime value);

        bool IsGUID(string expression);

        string[] GuidsFromText(string text);

        string RemoveColorFromText(string text);

        string DeleteAllTags(string text);

        string DeleteAllSpecialCharacters(string text);

        bool CheckTextExists(string text);

        string DeleteGuidText(string text);

        string DeleteLastTagSegment(string text);

        string DeleteEmptyTag(string text);

        string ReplaceLastOccurrence(string source, string find, string replace);

        Guid GuidNull2Guid(Guid? value);
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

        public DateTime ConvertDateTimeToLocal(TimeZoneInfo timeZoneInfo, DateTime value)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(value, timeZoneInfo);
        }

        public bool IsGUID(string expression)
        {
            if (String.IsNullOrEmpty(expression))
                return false;

            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            return isGuid.IsMatch(expression);
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

        public string DeleteAllTags(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            return Regex.Replace(text, @"<[^>]*>", String.Empty, RegexOptions.Compiled);
        }

        public string DeleteAllSpecialCharacters(string text)
        {
            text = text.Replace("&nbsp;", String.Empty);
            return Regex.Replace(text, @"&\w+;", String.Empty, RegexOptions.Compiled);
        }

        public string DeleteGuidText(string text)
        {
            string[] tags = text.Split(',');
            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            string[] tagsResult = tags.Where(a => isGuid.IsMatch(a) == false).ToArray();
            var model = string.Join(",", tagsResult);

            return model;
        }

        public string DeleteLastTagSegment(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                while (text.StartsWith("<p>") && text.EndsWith("</p>"))
                {
                    text = text.Substring(3);
                    text = text.Substring(0, text.Length - 4);
                }

                while (text.StartsWith("<br>"))
                    text = text.Substring(4);
                while (text.EndsWith("<br>"))
                    text = text.Substring(0, text.Length - 4);
            }

            return text.Trim();
        }

        public string DeleteEmptyTag(string text)
        {
            return Regex.Replace(text, "<[^>/][^>]*></[^>]*>", "");
        }

        public bool CheckTextExists(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrWhiteSpace(text))
                return false;

            string prepare = DeleteAllTags(text);
            prepare = DeleteAllSpecialCharacters(prepare);
            prepare = prepare.Trim();

            if (String.IsNullOrEmpty(prepare) || String.IsNullOrWhiteSpace(prepare))
                return false;

            return true;
        }

        public string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if (place == -1)
                return source;

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public Guid GuidNull2Guid(Guid? value)
        {
            return value ?? Guid.Empty;
        }
    }
}