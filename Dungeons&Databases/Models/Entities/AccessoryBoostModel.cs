namespace Dungeons_Databases.Models.Entities
{
    public class AccessoryBoostModel
    {
        public int AccessoryId { get; set; }
        public string StatType { get; set; }
        public int StatBoost { get; set; }

        public string StatTypeString => StatType switch
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
