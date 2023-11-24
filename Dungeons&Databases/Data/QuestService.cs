using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Dungeons_Databases.Data
{
    public class QuestService : BaseService
    {
        public QuestService(DatabaseService dbService, ProtectedSessionStorage sessionStorage, IConfiguration config) : base(dbService, sessionStorage, config)
        {
        }


    }
}
