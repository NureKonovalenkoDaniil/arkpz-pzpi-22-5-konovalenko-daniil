using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicationManagement.Models
{
    // IoTDevice class
    public class IoTDevice
    {
        // Device property
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceID { get; set; }
        [Required]
        [StringLength(50)]
        public string Location { get; set; }
        [Required]
        [StringLength(50)]
        public string Type { get; set; }
        public string Parameters { get; set; }
        public bool IsActive { get; set; } = false;
        public float MinTemperature { get; set; }
        public float MaxTemperature { get; set; }
        public float MinHumidity { get; set; }
        public float MaxHumidity { get; set; }
    }
}
