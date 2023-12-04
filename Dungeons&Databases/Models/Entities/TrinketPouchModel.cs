namespace Dungeons_Databases.Models.Entities
{
    public class TrinketPouchModel
    {
        public int AdventurerId { get; set; }
        public int TrinketId { get; set; }
        public TrinketModel Trinket { get; set; }
        public int Amount { get; set; }
    }
}
