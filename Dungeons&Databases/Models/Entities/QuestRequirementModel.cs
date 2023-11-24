namespace Dungeons_Databases.Models.Entities
{
    public class QuestRequirementModel
    {
        public int QuestId { get; set; }
        public string RequirementType { get; set; }
        public int Amount { get; set; }

        public string RequirementTypeString => RequirementType switch
        {
            "STR" => "Strength",
            "ETH" => "Ether",
            "DEX" => "Dexterity",
            "AGI" => "Agility",
            "MHP" => "Max Health",
            "GRD" => "Weapon Guard",
            "ETR" => "Weapon Ether Res.",
            "POW" => "Weapon Power",
            "ETP" => "Weapon Ether",
            _ => throw new NotImplementedException(),
        };
    }
}
