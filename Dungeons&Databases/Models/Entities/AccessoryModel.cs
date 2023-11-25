namespace Dungeons_Databases.Models.Entities
{
    public class AccessoryModel
    {
        public int AccessoryId { get; set; }
        public string Name { get; set; }
        public int GoldCost { get; set; }
        public List<AccessoryBoostModel> Boosts { get; set; }
        public List<CraftingModel> CraftingRequirements { get; set; }

        public bool CanCraft { get; set; }
        public bool Crafted { get; set; } = false;
    }
}
