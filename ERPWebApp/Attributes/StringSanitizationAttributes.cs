using System.ComponentModel.DataAnnotations;
using System.Net;
namespace ERPWebApp.Attributes
{
    public class StringSanitizationAttributes : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            var input = value.ToString();
            input = WebUtility.HtmlEncode(input).ToString();
            var property = validationContext.ObjectType.GetProperty(validationContext.MemberName);
            if (property != null)
            {
                property.SetValue(validationContext.ObjectInstance, input);
            }
            return ValidationResult.Success;
        }
    }
}