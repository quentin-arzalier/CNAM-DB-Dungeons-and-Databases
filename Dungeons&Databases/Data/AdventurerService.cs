using Dungeons_Databases.Models.Entities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;

namespace Dungeons_Databases.Data
{
	public class AdventurerService : BaseService
	{
		public AdventurerService(DatabaseService dbService, ProtectedSessionStorage sessionStorage, IConfiguration config) : base(dbService, sessionStorage, config)
		{
		}

		public async Task<AdventurerModel?> GetCurrentAdventurerAsync()
		{
			return (await GetCurrentUserAsync())?.Adventurer;
		}

		public async Task<bool> CreateAdventurerAsync(AdventurerModel model)
		{
			if (!model.IsModelValid)
				return false;

			var user = await GetCurrentUserAsync();
			if (user == null)
				return false;

			model.User = user;
			model.UserId = user.UserId;

			var id = _dbService.ExecuteInsert(ADVENTURER_CREATION_QUERY, model);
			model.AdventurerId = id;

			user.Adventurer = model;
			await UpdateUserInSessionAsync(user);

			return true;
		}

		public async Task<bool> UpdateAdventurerAsync(AdventurerModel model)
		{
			if (!model.IsModelValid)
				return false;

			var user = await GetCurrentUserAsync();
			if (user == null)
				return false;

			_dbService.ExecuteSql(ADVENTURER_UPDATE_QUERY, model);

			user.Adventurer = model;
			await UpdateUserInSessionAsync(user);

			return true;
		}


		#region Queries

		private const string ADVENTURER_CREATION_QUERY = @"
INSERT INTO public.adventurer
(user_id, ""name"", weapon_id, gold, description, ""level"", xp_count, ""class"", ""element"", max_health, strength, ether, dexterity, agility, charisma)
VALUES(@UserId, @Name, @WeaponId, @Gold, @Description::text, @Level, @XpCount, @Class, @Element, @MaxHealth, @Strength, @Ether, @Dexterity, @Agility, @Charisma)
RETURNING adventurer_id;
";

		private const string ADVENTURER_UPDATE_QUERY = @"
UPDATE public.adventurer
SET ""name""=@Name, weapon_id=@WeaponId, description=@Description::text, ""level""=@Level, xp_count=@XpCount, ""class""=@Class, gold=@Gold,
	""element""=@Element, max_health=@MaxHealth, strength=@Strength, ether=@Ether, dexterity=@Dexterity, agility=@Agility, charisma=@Charisma
WHERE adventurer_id=@AdventurerId;
";

		#endregion
	}
}
