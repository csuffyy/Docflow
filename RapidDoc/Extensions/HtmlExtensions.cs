using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Web.Mvc.Html;
using System.Text.RegularExpressions;
using RapidDoc.Models.Infrastructure;
using Microsoft.AspNet.Identity;
using RapidDoc.Models.DomainModels;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Routing;
using RapidDoc.Models.Repository;
using System.Globalization;
using RapidDoc.Attributes;

namespace RapidDoc.Extensions
{
    public static class HtmlExtensions
    {
        private static Dictionary<string, string> enumNameCache = new Dictionary<string, string>();

        public static MvcHtmlString EnumDropDownList(this HtmlHelper helper, string enumName, string fieldName, object htmlAttribute = null)
        {
            Type type = Type.GetType(enumName);
            if (type != null)
            {
                List<SelectListItem> items = new List<SelectListItem>();
                var enumsList = System.Web.Mvc.Html.EnumHelper.GetSelectList(type);
                var enumArr = Enum.GetValues(type);
                int num = 0;
                foreach (var value in enumsList)
                {
                    FieldInfo fi = type.GetField(enumArr.GetValue(num).ToString());
                    var attribute = fi.GetCustomAttributes(typeof(DisabledAttribute), false).FirstOrDefault();
                    if (attribute == null)
                    {
                        var listItem = new SelectListItem
                        {
                            Text = value.Text,
                            Value = value.Value,
                            Selected = value.Selected
                        };

                        items.Add(listItem);
                    }
                    num++;
                }

                return helper.DropDownList(fieldName, items, htmlAttribute);
            }

            return new MvcHtmlString(String.Empty);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString LabelRequired(this HtmlHelper html, string htmlFieldName, string labelText = "")
        {
            return LabelHelper(html,
                ModelMetadata.FromStringExpression(htmlFieldName, html.ViewData), htmlFieldName, labelText);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString LabelForRequired<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText = "")
        {
            return LabelHelper(html,
                ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                ExpressionHelper.GetExpressionText(expression), labelText);
        }

        private static MvcHtmlString LabelHelper(HtmlHelper html,
           ModelMetadata metadata, string htmlFieldName, string labelText)
        {
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            }

            if (string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            bool isRequired = false;

            if (metadata.ContainerType != null)
            {
                isRequired = metadata.ContainerType.GetProperty(metadata.PropertyName)
                                .GetCustomAttributes(typeof(RequiredAttribute), false)
                                .Length == 1;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.Attributes.Add(
                "for",
                TagBuilder.CreateSanitizedId(
                    html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)
                )
            );

            if (isRequired)
                tag.Attributes.Add("class", "label-required");

            tag.SetInnerText(labelText);

            var output = tag.ToString(TagRenderMode.Normal);


            if (isRequired)
            {
                var asteriskTag = new TagBuilder("span");
                asteriskTag.Attributes.Add("class", "required");
                asteriskTag.SetInnerText("*");
                output += asteriskTag.ToString(TagRenderMode.Normal);
            }
            return MvcHtmlString.Create(output);
        }

        public static MvcHtmlString DateTimeLocalFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, string timeZone = "")  
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            if (String.IsNullOrEmpty(timeZone))
            {
                ApplicationDbContext context = new ApplicationDbContext();
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                ApplicationUser user = UserManager.FindById(HttpContext.Current.User.Identity.GetUserId());
                context.Dispose();
                timeZone = user.TimeZoneId;
            }

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            string convertedTime = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(metadata.Model), timeZoneInfo).ToString();

            return new MvcHtmlString(convertedTime);
        }

        public static MvcHtmlString DateTimeLocal(this HtmlHelper helper, DateTime value, string timeZone = "")
        {
            if (String.IsNullOrEmpty(timeZone))
            {
                ApplicationDbContext context = new ApplicationDbContext();
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                ApplicationUser user = UserManager.FindById(HttpContext.Current.User.Identity.GetUserId());
                context.Dispose();
                timeZone = user.TimeZoneId;
            }

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            string convertedTime = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(value), timeZoneInfo).ToString();

            return new MvcHtmlString(convertedTime);
        }

        private static string GetWebApplicationUrl()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var urlHelper = new UrlHelper(request.RequestContext);
                var result = String.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority,
                    urlHelper.Content("~"));
                return result;
            }
            catch
            {
                return "/";
            }
        }

        public static MvcHtmlString HtmlDisplayTagsFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            string[] tags = html.Encode(metadata.Model).Split(',');

            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            string[] tagsResult = tags.Where(a => isGuid.IsMatch(a) == false).ToArray();
            var model = string.Join(",", tagsResult).Replace(",", ",<br />\n");

            if (String.IsNullOrEmpty(model))
                return MvcHtmlString.Empty;

            return MvcHtmlString.Create(model);
        }

        public static MvcHtmlString HtmlDisplayTagsFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            IDictionary<string, object> attributes = new RouteValueDictionary(htmlAttributes);

            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            string[] tags = html.Encode(metadata.Model).Split(',');

            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            string[] tagsResult = tags.Where(a => isGuid.IsMatch(a) == false).ToArray();

            if (attributes.ContainsKey("onlyName") && (bool)attributes["onlyName"] == true)
                tagsResult = tagsResult.Select(x => x.Split('-').First().Trim()).ToArray();
            
            var model = (attributes.ContainsKey("row") && (bool)attributes["row"] == true) ? string.Join(",", tagsResult).Replace(",", ",\n") : string.Join(",", tagsResult).Replace(",", ",<br />\n");

            if (String.IsNullOrEmpty(model))
                return MvcHtmlString.Empty;

            return MvcHtmlString.Create(model);
        }

        public static MvcHtmlString HtmlDisplayTags(this HtmlHelper html, string value)
        {
            string[] tags = html.Encode(value).Split(',');

            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
            string[] tagsResult = tags.Where(a => isGuid.IsMatch(a) == false).ToArray();
            var model = string.Join(",", tagsResult).Replace(",", ",<br />\n");

            if (String.IsNullOrEmpty(model))
                return MvcHtmlString.Empty;

            return MvcHtmlString.Create(model);
        }

        public static string EnumToDescription(this HtmlHelper html, Enum value)
        {
            string result = "";
            string key = String.Format("{0}/{1}/{2}", value.GetType().ToString(), value.ToString(), CultureInfo.CurrentCulture);
            if (enumNameCache.Any(x => x.Key == key))
            {
                result = enumNameCache.FirstOrDefault(x => x.Key == key).Value;
            }
            else
            {
                Type enumType = value.GetType();
                var enumValue = Enum.GetName(enumType, value);
                MemberInfo member = enumType.GetMember(enumValue)[0];

                var attrsDisplay = member.GetCustomAttributes(typeof(DisplayAttribute), false);
                var attrsDescription = member.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrsDisplay.Any())
                {
                    var displayAttr = ((DisplayAttribute)attrsDisplay[0]);
                    result = displayAttr.Name;

                    if (displayAttr.ResourceType != null)
                    {
                        result = displayAttr.GetName();
                    }
                }
                else if (attrsDescription.Any())
                {
                    var displayAttr = ((DescriptionAttribute)attrsDescription[0]);
                    result = displayAttr.Description;
                }
                else
                    result = enumValue.ToString();

                try
                {
                    enumNameCache.Add(key, result);
                }
                catch
                {
                    return result;
                }
            }

            return result;
        }

        public static string EnumToDescription(this HtmlHelper html, string enumName, string value)
        {
            string result = "";
            string key = String.Format("{0}/{1}/{2}", enumName, value.ToString(), CultureInfo.CurrentCulture);
            if (enumNameCache.Any(x => x.Key == key))
            {
                result = enumNameCache.FirstOrDefault(x => x.Key == key).Value;
            }
            else
            {
                Type enumType = Type.GetType(enumName);
                if (enumType != null)
                {
                    MemberInfo member = enumType.GetMember(value)[0];

                    var attrsDisplay = member.GetCustomAttributes(typeof(DisplayAttribute), false);
                    var attrsDescription = member.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attrsDisplay.Any())
                    {
                        var displayAttr = ((DisplayAttribute)attrsDisplay[0]);

                        result = displayAttr.Name;

                        if (displayAttr.ResourceType != null)
                        {
                            result = displayAttr.GetName();
                        }
                    }
                    else if (attrsDescription.Any())
                    {
                        var displayAttr = ((DescriptionAttribute)attrsDescription[0]);
                        result = displayAttr.Description;
                    }
                    else
                        result = value.ToString();
                    try
                    {
                        enumNameCache.Add(key, result);
                    }
                    catch
                    {
                        return result;
                    }
                }
            }

            return result;
        }
    }
}