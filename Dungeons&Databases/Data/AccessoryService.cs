using Dungeons_Databases.Models.Entities;
using Dungeons_Databases.Models.ViewModels;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Dungeons_Databases.Data
{
    public class AccessoryService : BaseService
    {
        public AccessoryService(DatabaseService dbs, ProtectedSessionStorage ss, IConfiguration config) : base(dbs, ss, config)
        { 
        }

        public async Task<int> GetTotalAccessoryCountInShop()
        {
            var currentUser = await GetCurrentUserAsync();
            var totalCount = _dbService.Get<int>(SHOP_TOTAL_COUNT_QUERY, new { adventurerId = currentUser?.Adventurer?.AdventurerId });
            return totalCount;
        }

        public async Task<List<AccessoryModel>> GetListPaginatedForShop(int pageNumber)
        {
            var currentUser = await GetCurrentUserAsync();
            var accessories = _dbService.GetList<AccessoryModel>(SHOP_ITEMS_QUERY, 
                new
                {
                    adventurerId = currentUser?.Adventurer?.AdventurerId,
                    limit = ITEMS_PER_PAGE,
                    offset = ITEMS_PER_PAGE * pageNumber
                }
            );

            return accessories;
        }

        #region Queries 

        #region Shop

        private const string SHOP_TOTAL_COUNT_QUERY = @"SELECT COUNT(*)
FROM accessory a
WHERE @adventurerId IS NULL 
OR NOT EXISTS (
    SELECT 1
    FROM inventory i
    WHERE i.adventurer_id = @adventurerId
    AND i.accessory_id = a.accessory_id
);";
        private const string SHOP_ITEMS_QUERY = @"
SELECT *
FROM accessory a
WHERE @adventurerId IS NULL
OR NOT EXISTS(
    SELECT 1
    FROM inventory i
    WHERE i.adventurer_id = @adventurerId
    AND i.accessory_id = a.accessory_id
)
ORDER BY a.name
LIMIT @limit OFFSET @offset;";

        #endregion

        #endregion

    }
}
