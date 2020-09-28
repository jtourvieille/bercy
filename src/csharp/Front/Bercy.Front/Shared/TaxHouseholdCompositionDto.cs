namespace Bercy.Front.Shared
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The household composition definition
    /// </summary>
    public class TaxHouseholdCompositionDto
    {
        /// <summary>
        /// Gets or sets the count of adults in the household
        /// </summary>
        [Range(1, 2, ErrorMessage = "1 ou 2 adultes seulement")]
        public int NbAdults { get; set; }

        /// <summary>
        /// Gets or sets the count of children in the household
        /// </summary>
        [Range(0, 100, ErrorMessage = "Entre 0 et 100 enfants")]
        public int NbChildren { get; set; }
    }
}
