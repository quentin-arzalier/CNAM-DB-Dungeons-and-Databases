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

        public async Task<int> GetTotalAccessoryCountInShop(string search)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return 0;
            search ??= "";
            var totalCount = _dbService.Get<int>(SHOP_TOTAL_COUNT_QUERY, new { adventurerId = currAdv.AdventurerId, Search=$"%{search}%" });
            return totalCount;
        }

        public async Task<int> GetTotalAccessoryCountInInventory(string search)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return 0;
            search ??= "";
            var totalCount = _dbService.Get<int>(INVENTORY_TOTAL_COUNT_QUERY, new { adventurerId = currAdv.AdventurerId, Search = $"%{search}%" });
            return totalCount;
        }
        public async Task<List<InventoryModel>> GetListPaginatedForInventory(int pageNumber, string search)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return new();

            search ??= "";

            var inventories = _dbService.GetList<InventoryModel>(INVENTORY_ITEMS_QUERY, new
            {
                AdventurerId = currAdv.AdventurerId,
                Limit = ITEMS_PER_PAGE,
                Offset = ITEMS_PER_PAGE * pageNumber,
                Search = $"%{search}%"
            });

            foreach (var inv in inventories)
            {
                var acc = _dbService.Get<AccessoryModel>(GET_ACCESSORY_OF_INVENTORY, new
                {
                    inv.AccessoryId
                });
                acc.Boosts = _dbService.GetList<AccessoryBoostModel>(GET_BOOSTS_OF_ACCESSORY, new
                {
                    acc.AccessoryId
                });
                inv.Accessory = acc;
            }
            return inventories;
        }

        public async Task<List<AccessoryModel>> GetListPaginatedForShop(int pageNumber, string search)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return new();

            search ??= "";

            var accessories = _dbService.GetList<AccessoryModel>(SHOP_ITEMS_QUERY, new
            {
                AdventurerId = currAdv.AdventurerId,
                Limit = ITEMS_PER_PAGE,
                Offset = ITEMS_PER_PAGE * pageNumber,
                Search = $"%{search}%"
            });

            foreach (var acc in accessories)
            {
                acc.CraftingRequirements = _dbService.GetList<CraftingModel>(GET_CRAFTING_OF_ACCESSORY, new
                {
                    acc.AccessoryId
                });
                acc.Boosts = _dbService.GetList<AccessoryBoostModel>(GET_BOOSTS_OF_ACCESSORY, new
                {
                    acc.AccessoryId
                });
                foreach (var req in acc.CraftingRequirements)
                {
                    req.Trinket = _dbService.Get<TrinketModel>(GET_TRINKET_OF_CRAFT, new
                    {
                        req.TrinketId
                    });
                }
            }
            return accessories;
        }

        public async Task<bool> CraftAccessoryAsync(AccessoryModel accessory)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return new();

            return _dbService.ExecuteScalar<bool>("SELECT purchase_accessory(@AccessoryId, @AdventurerId);", new
            {
                accessory.AccessoryId,
                currAdv.AdventurerId
            });
        }

        public async Task<bool> ChangeEquipInventory(InventoryModel inventory, bool newEquip)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return new();


            var updatedLines = _dbService.ExecuteSql(CHANGE_EQUIP_STATUS, new
            {
                currAdv.AdventurerId,
                inventory.AccessoryId,
                Equipped = newEquip
            });

            // Le trigger empêche le changement des lignes.
            return updatedLines > 0;

        }

        #region Queries 

        private const string SHOP_TOTAL_COUNT_QUERY = @"SELECT COUNT(*)
FROM accessory a
WHERE @adventurerId IS NULL 
OR NOT EXISTS (
    SELECT 1
    FROM inventory i
    WHERE i.adventurer_id = @adventurerId
    AND i.accessory_id = a.accessory_id
) AND a.""name"" ILIKE @Search;";
        private const string SHOP_ITEMS_QUERY = @"
SELECT a.*, can_craft(a.accessory_id, @AdventurerId) AS can_craft
FROM accessory a
WHERE @adventurerId IS NULL
OR NOT EXISTS(
    SELECT 1
    FROM inventory i
    WHERE i.adventurer_id = @AdventurerId
    AND i.accessory_id = a.accessory_id
) AND a.""name"" ILIKE @Search
ORDER BY a.name
LIMIT @Limit OFFSET @Offset;";

        private const string INVENTORY_TOTAL_COUNT_QUERY = @"
SELECT COUNT(*) FROM inventory i
NATURAL JOIN accessory a
WHERE i.adventurer_id = @AdventurerId
AND a.""name"" ILIKE @Search;
";
        private const string INVENTORY_ITEMS_QUERY = @"
SELECT * FROM inventory i
NATURAL JOIN accessory a
WHERE i.adventurer_id = @AdventurerId
AND a.""name"" ILIKE @Search
ORDER BY is_equipped DESC, a.""name""
LIMIT @Limit OFFSET @Offset;
";
        private const string GET_ACCESSORY_OF_INVENTORY = @"
SELECT * FROM accessory
WHERE accessory_id = @AccessoryId;
";

        private const string GET_CRAFTING_OF_ACCESSORY = @"
SELECT * FROM crafting
WHERE accessory_id = @AccessoryId;
";

        private const string GET_BOOSTS_OF_ACCESSORY = @"
SELECT * FROM accessory_boost
WHERE accessory_id = @AccessoryId;
";
        private const string GET_TRINKET_OF_CRAFT = @"
SELECT * FROM trinket
WHERE trinket_id = @TrinketId;
";
        private const string CHANGE_EQUIP_STATUS = @"
UPDATE inventory
SET is_equipped=@Equipped
WHERE adventurer_id=@AdventurerId AND accessory_id=@AccessoryId;
";

        #endregion

    }
}
