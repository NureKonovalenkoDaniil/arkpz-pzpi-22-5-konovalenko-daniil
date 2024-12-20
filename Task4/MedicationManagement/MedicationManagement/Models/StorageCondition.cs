using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicationManagement.Models
{
    // StorageCondition class
    public class StorageCondition
    {
        // Condition property
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConditionID { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public DateTime Timestamp { get; set; }
        [ForeignKey("IoTDevice")]
        public int DeviceID { get; set; } // DeviceID property
        public IoTDevice? IoTDevice { get; set; } // IoTDevice property
    }
}
