namespace Dungeons_Databases.Models.Entities
{
    public class CraftingModel
    {
        public int AccessoryId { get; set; }
        public int TrinketId { get; set; }
        public TrinketModel Trinket { get; set; }
        public int Amount { get; set; }
    }
}
