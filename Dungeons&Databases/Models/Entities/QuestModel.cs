namespace Dungeons_Databases.Models.Entities
{
    public class QuestModel
    {
        public int QuestId { get; set; }
        public string Name { get; set; }
        public int Gold_Reward { get; set; }
        public int XpReward { get; set; }
        public List<QuestRequirementModel> Requirements { get; set; }
        public List<QuestRewardModel> Rewards { get; set; }

    }
}
