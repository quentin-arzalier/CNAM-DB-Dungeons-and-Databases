using Dungeons_Databases.Models.Entities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Dungeons_Databases.Data
{
    public class TrinketService : BaseService
    {
        public TrinketService(DatabaseService dbService, ProtectedSessionStorage sessionStorage, IConfiguration config) : base(dbService, sessionStorage, config)
        {
        }

        public async Task<int> CountTrinketsInPouch(string search)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return 0;
            search ??= "";


            var totalCount = _dbService.Get<int>(COUNT_TRINKETS_IN_POUCH, new { currAdv.AdventurerId, Search= $"%{search}%" });
            return totalCount;

        }
        public async Task<List<TrinketPouchModel>> GetTrinketPouchOfAdventurerPaginatedAsync(int pageNumber, string search)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return new();

            search ??= "";

            var trinkets = _dbService.GetList<TrinketPouchModel>(GET_TRINKET_POUCH_PAGINATED, new
            {
                AdventurerId = currAdv.AdventurerId,
                Search=$"%{search}%",
                Limit = ITEMS_PER_PAGE,
                Offset = ITEMS_PER_PAGE * pageNumber
            });

            foreach (var trinket in trinkets)
            {
                trinket.Trinket = _dbService.Get<TrinketModel>(GET_TRINKET_OF_POUCH, new { trinket.TrinketId });
            }

            return trinkets;
        }

        #region Queries 

        private const string COUNT_TRINKETS_IN_POUCH = @"
SELECT COUNT(*) FROM trinket_pouch tp
NATURAL JOIN trinket t
WHERE adventurer_id=@AdventurerId
AND t.""name"" ILIKE @Search;
";

        private const string GET_TRINKET_POUCH_PAGINATED = @"
SELECT * FROM trinket_pouch
NATURAL JOIN trinket t
WHERE adventurer_id=@AdventurerId
AND t.""name"" ILIKE @Search
ORDER BY t.""name""
LIMIT @Limit OFFSET @Offset;
";

        private const string GET_TRINKET_OF_POUCH = @"
SELECT * FROM trinket
WHERE trinket_id = @TrinketId;
";
        #endregion
    }
}
