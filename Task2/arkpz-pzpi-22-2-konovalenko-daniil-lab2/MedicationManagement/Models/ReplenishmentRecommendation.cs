namespace MedicationManagement.Models
{
    public class ReplenishmentRecommendation
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public int RecommendedQuantity { get; set; }
    }
}
