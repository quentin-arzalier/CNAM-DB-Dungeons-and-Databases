using System.Diagnostics.Metrics;
using System.Reflection;

namespace Dungeons_Databases.Models.Entities
{
    public class AdventurerModel
    {
        public AdventurerModel()
		{
			Name = "";
			Description = "";
			Class = "";
			Element = "";
            Gold = 0;
			Level = 1;
			XpCount = 0;
			MaxHealth = 5;
			Strength = 1;
			Ether = 1;
			Dexterity = 1;
			Agility = 1;
			Charisma = 1;
		}
		public AdventurerModel(UserModel user) : this()
        {
            User = user;
            UserId = user.UserId;
		}
        public int AdventurerId { get; set; }
        public int UserId { get; set; }
        public UserModel? User { get; set; }
        public string Name { get; set; }
        public int WeaponId { get; set; }
        public int Gold { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public int XpCount { get; set; }
        public string Class { get; set; }
        public string Element { get; set; }
        public int MaxHealth { get; set; }
        public int Strength { get; set; }
        public int Ether { get; set; }
        public int Dexterity { get; set; }
        public int Agility { get; set; }
        public int Charisma { get; set; }

        public int RemainingSkillPoints => (Level - 1) * 4 + 16 - (MaxHealth / 5) - Strength - Ether - Dexterity - Agility - Charisma;

        public bool IsModelValid =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Description) &&
            !string.IsNullOrWhiteSpace(Class) &&
            !string.IsNullOrWhiteSpace(Element) &&
            WeaponId != 0 && RemainingSkillPoints == 0;

        public int ExpUntilLevelUp => 1000 + 250 * Level*Level - XpCount;

	}
}
