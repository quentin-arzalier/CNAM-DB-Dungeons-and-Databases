using Dungeons_Databases.Models.Entities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Dungeons_Databases.Data
{
    public class QuestService : BaseService
    {
        public QuestService(DatabaseService dbService, ProtectedSessionStorage sessionStorage, IConfiguration config) : base(dbService, sessionStorage, config)
        {
        }
        public async Task<int> GetTotalAvailableQuestCount()
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return 0;

            var totalCount = _dbService.Get<int>(COUNT_AVAILABLE_QUESTS_FOR_ADVENTURER, new { currAdv.AdventurerId });
            return totalCount;
        }
        public async Task<List<QuestModel>> GetAvailableQuestsPaginatedForCurrentAdventurer(int pageNumber)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return new();

            var quests = _dbService.GetList<QuestModel>(GET_AVAILABLE_QUESTS_FOR_ADVENTURER, new
            {
                AdventurerId = currAdv.AdventurerId,
                Limit = ITEMS_PER_PAGE,
                Offset = ITEMS_PER_PAGE * pageNumber
            });

            foreach (var quest in quests)
            {
                quest.Requirements = _dbService.GetList<QuestRequirementModel>(GET_REQUIREMENTS_OF_QUEST, new
                {
                    quest.QuestId
                });
                quest.Rewards = _dbService.GetList<QuestRewardModel>(GET_REWARDS_OF_QUEST, new
                {
                    quest.QuestId
                });
                foreach(var reward in quest.Rewards)
                {
                    reward.Trinket = _dbService.Get<TrinketModel>(GET_TRINKET_OF_QUEST_REWARD, new
                    {
                        reward.TrinketId
                    });
                }
            }
            return quests;
        }

        public async Task<bool> TryQuestAsync(int questId)
        {
            var currAdv = (await GetCurrentUserAsync())?.Adventurer;
            if (currAdv == null)
                return false;

            var success = _dbService.ExecuteScalar<bool>("SELECT complete_quest(@QuestId, @AdventurerId);", new
            {
                QuestId=questId,
                currAdv.AdventurerId
            });

            if (success)
                await RefreshUserInSessionAsync();

            return success;
        }

        #region Queries

        private const string COUNT_AVAILABLE_QUESTS_FOR_ADVENTURER = @"
SELECT COUNT(*) FROM quest
WHERE quest_id NOT IN 
(
   SELECT quest_id
   FROM quest_log
   WHERE adventurer_id = @AdventurerId
);
";
        private const string GET_AVAILABLE_QUESTS_FOR_ADVENTURER = @"
SELECT * FROM quest
WHERE quest_id NOT IN 
(
   SELECT quest_id
   FROM quest_log
   WHERE adventurer_id = @AdventurerId
)
LIMIT @Limit OFFSET @Offset;
";
        private const string GET_REWARDS_OF_QUEST = @"
SELECT * FROM quest_reward
WHERE quest_id = @QuestId;
";
        private const string GET_TRINKET_OF_QUEST_REWARD = @"
SELECT * FROM trinket
WHERE trinket_id = @TrinketId;
";
        private const string GET_REQUIREMENTS_OF_QUEST = @"
SELECT * FROM quest_requirement
WHERE quest_id = @QuestId;
";

        #endregion

    }
}
