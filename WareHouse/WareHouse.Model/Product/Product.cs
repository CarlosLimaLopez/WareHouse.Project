using System.ComponentModel.DataAnnotations;

namespace WareHouse.Product
{
    public class Product : IValidatableObject
    {
        /// <summary>
        /// Product unique identifier
        /// </summary>
        [Key]
        public Guid Id { get; init; }

        /// <summary>
        /// Product code (exactly 20 characters)
        /// </summary>
        [Required]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Code must be exactly 8 characters.")]
        public required string Code { get; init; }

        /// <summary>
        /// Current stock level
        /// </summary>
        public int Stock { get; private set; } = 0;

        /// <summary>
        /// Update stock to a specific value
        /// </summary>
        public void UpdateStock(int stock)
        {
            Stock = stock;
        }

        /// <summary>
        /// Increment stock by 1
        /// </summary>
        public void AddStock()
        {
            Stock++;
        }

        /// <summary>
        /// Decrement stock by 1 if possible
        /// </summary>
        public void RemoveStock()
        {
            if (Stock <= 0)
                throw new InvalidOperationException("Cannot remove stock when stock level is zero.");

            Stock--;
        }

        /// <summary>
        /// Validates the Product entity
        /// </summary>
        /// <param name="validationContext"><see cref="ValidationContext"/></param>
        /// <returns>List of <see cref="ValidationResult"/></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Array.Empty<ValidationResult>();
        }

        /// <summary>
        /// Validates if stock can be removed
        /// </summary>
        /// <returns>List of <see cref="ValidationResult"/></returns>
        public IEnumerable<ValidationResult> ValidateRemoveStock()
        {
            if (Stock <= 0)
            {
                yield return new ValidationResult("Cannot remove stock when stock level is zero.", [nameof(Stock)]);
            }
        }

        /// <summary>
        /// Validates if it can be removed
        /// </summary>
        /// <returns>List of <see cref="ValidationResult"/></returns>
        public IEnumerable<ValidationResult> ValidateRemove()
        {
            if (Stock != 0)
            {
                yield return new ValidationResult("Cannot remove a product when stock level is zero.", [nameof(Stock)]);
            }
        }

        /// <summary>
        /// Validates if stock can be removed
        /// </summary>
        /// <returns>List of <see cref="ValidationResult"/></returns>
        public IEnumerable<ValidationResult> ValidateUpdateStock(int stock)
        {
            if (stock < 0)
            {
                yield return new ValidationResult("Stock cannot be negative.", [nameof(Stock)]);
            }
        }
    }
}
