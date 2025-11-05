using System.ComponentModel.DataAnnotations;

namespace WareHouse.Product
{
    public class ProductValidationException : Exception
    {
        public IEnumerable<ValidationResult> Errors { get; }

        public ProductValidationException(IEnumerable<ValidationResult> errors)
            : base(string.Join("; ", errors.Select(e => e.ErrorMessage)))
        {
            Errors = errors;
        }
    }
}
