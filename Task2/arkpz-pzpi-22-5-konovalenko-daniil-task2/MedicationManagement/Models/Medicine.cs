using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicationManagement.Models
{
    // Medicine class
    public class Medicine
    {
        // Medicine property
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedicineID { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Type { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        [Required]
        public int Quantity { get; set; }
        public string Category { get; set; }
    }
}
