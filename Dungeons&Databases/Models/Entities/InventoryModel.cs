namespace Dungeons_Databases.Models.Entities
{
    public class InventoryModel
    {
        public int AdventurerId { get; set; }
        public int AccessoryId { get; set; }
        public AccessoryModel Accessory { get; set; }
        public bool IsEquipped { get; set; }
    }
}
