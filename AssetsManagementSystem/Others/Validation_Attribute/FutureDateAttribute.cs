namespace AssetsManagementSystem.Others.Validation_Attribute
{
    public class FutureDateAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public FutureDateAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (comparisonProperty == null)
            {
                return new ValidationResult($"Unknown property: {_comparisonProperty}");
            }

            var comparisonValue = (DateOnly)comparisonProperty.GetValue(validationContext.ObjectInstance);

            if (value == null || (DateOnly)value > comparisonValue)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

}
