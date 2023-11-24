namespace Dungeons_Databases.Models.Entities
{
    public class QuestRewardModel
    {
        public int QuestId { get; set; }
        public QuestModel Quest { get; set; }
        public int TrinketId { get; set; }
        public TrinketModel Trinket { get; set; }
        public int Amount { get; set; }
    }
}
