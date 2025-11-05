using System.ComponentModel.DataAnnotations;

namespace WareHouse.Product
{
    public static class ProductValidationExtensions
    {
        public static IEnumerable<ValidationResult> ValidateAttributes(this Product product)
        {
            var validationContext = new ValidationContext(product);
            var errors = new List<ValidationResult>();
            Validator.TryValidateObject(product, validationContext, errors, true);
            return errors;
        }
    }
}
