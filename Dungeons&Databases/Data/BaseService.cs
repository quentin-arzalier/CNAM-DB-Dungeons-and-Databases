using Dungeons_Databases.Models.Entities;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Dungeons_Databases.Data
{
    public abstract class BaseService
    {
        protected const int ITEMS_PER_PAGE = 20;
        private const string CURRENT_USER_SESS_KEY = "CurrentUser";

        protected readonly DatabaseService _dbService;
        protected readonly ProtectedSessionStorage _sessionStorage;
        protected readonly IConfiguration _config;
        public BaseService(DatabaseService dbService, ProtectedSessionStorage sessionStorage, IConfiguration config)
        {
            _dbService = dbService;
            _sessionStorage = sessionStorage;
            _config = config;
        }
        protected async Task UpdateUserInSessionAsync(UserModel user)
        {
            if (user == null)
            {
                await RemoveUserInSessionAsync();
                return;
            }
            if (user.Adventurer?.User != null)
                user.Adventurer.User = null;
            await _sessionStorage.SetAsync(CURRENT_USER_SESS_KEY, user);
        }
        protected async Task RemoveUserInSessionAsync()
        {
            await _sessionStorage.DeleteAsync(CURRENT_USER_SESS_KEY);
        }
        protected async Task<UserModel?> GetCurrentUserAsync()
        {
            var result = await _sessionStorage.GetAsync<UserModel>(CURRENT_USER_SESS_KEY);
            return result.Value;
        }

        protected async Task RefreshUserInSessionAsync()
        {
            var currUser = await GetCurrentUserAsync();
            if (currUser == null)
                return;
            currUser.Adventurer = _dbService.Get<AdventurerModel>(GET_ADVENTURER_OF_USER, new { currUser.UserId });
            await UpdateUserInSessionAsync(currUser);
        }

        #region Queries

        private const string GET_ADVENTURER_OF_USER = @"
SELECT * FROM adventurer
WHERE user_id=@UserId;
";
        #endregion
    }
}
