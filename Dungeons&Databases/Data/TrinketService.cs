using Dungeons_Databases.Models.Entities;

namespace Dungeons_Databases.Data
{
    public class TrinketService
    {
        private readonly DatabaseService _dbs;

        public TrinketService(DatabaseService dbs)
        {
            _dbs = dbs;
        }

        public TrinketModel GetById(int trinketId)
        {
            var trinket = _dbs.Get<TrinketModel>(@"SELECT * FROM trinket WHERE trinket_id = @trinketId", new { trinketId });
            return trinket; 
        }
    }
}
