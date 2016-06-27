using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RapidDoc.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            if (!type.IsEnum) throw new ArgumentException(String.Format("Type '{0}' is not Enum", type));

            var members = type.GetMember(value.ToString());
            if (members.Length == 0) throw new ArgumentException(String.Format("Member '{0}' not found in type '{1}'", value, type.Name));

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length == 0) throw new ArgumentException(String.Format("'{0}.{1}' doesn't have DisplayAttribute", type.Name, value));

            var attribute = (DisplayAttribute)attributes[0];
            return attribute.GetName();
        }
    }
}