using Dungeons_Databases.Models.Entities;
using Dungeons_Databases.Models.ViewModels;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Dungeons_Databases.Data
{
    public class UserService : BaseService
    {
        public UserService(DatabaseService dbs, ProtectedSessionStorage ss, IConfiguration config) : base(dbs, ss, config)
        {
        }

        public async Task<UserModel?> GetUserInSessionAsync()
        {
            return await GetCurrentUserAsync();
        }

        public async Task Disconnect()
        {
            await RemoveUserInSessionAsync();
        }

        #region Registration

        private readonly Regex EMAIL_REGEX = new Regex("^(?:[a-z0-9!#$%&'*+\\/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+\\/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])$");

		public bool Register(string email, string password)
        {
            var vm = new RegisterViewModel();
            ValidateEmail(email, ref vm);
            ValidatePassword(password, ref vm);

            if (!vm.HasNoErrors)
                return false;

            var passwordHash = HashPassword(password);

            _dbService.ExecuteSql(USER_REGISTRATION_QUERY, new
            {
                email,
                passwordHash
            });

            return true;
        }


        public void ValidateEmail(string email, ref RegisterViewModel vm)
        {
            vm.CheckEmailValidity = EMAIL_REGEX.IsMatch(email);

            var potentialUser = _dbService.Get<UserModel>(USER_GET_BY_EMAIL_QUERY, new { email });
			vm.CheckEmailUnicity = potentialUser == null;

		}

        public void ValidatePassword(string password, ref RegisterViewModel vm)
        {
            vm.CheckPasswordLength = password.Length >= 8;

            var numericRegex = new Regex("^(?=.*[0-9]).*$");
            vm.CheckPasswordNumericLetter = numericRegex.IsMatch(password);

            var lowercaseRegex = new Regex("^(?=.*[a-z]).*$");
            vm.CheckPasswordLowercaseLetter = lowercaseRegex.IsMatch(password);

            var uppercaseRegex = new Regex("^(?=.*[A-Z]).*$");
            vm.CheckPasswordUppercaseLetter = uppercaseRegex.IsMatch(password);

            var specialRegex = new Regex("^(?=.*[!@#$&*]).*$");
            vm.CheckPasswordSpecialCharacter = specialRegex.IsMatch(password);
        }

        private static byte[] HashPassword(string password)
        {
            var salt = new byte[16];
            RandomNumberGenerator.Create().GetBytes(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10_000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return hashBytes;
        }

		#endregion

		#region Login

		public async Task<bool> LoginAsync(string email, string password)
        {
            var potentialUser = _dbService.Get<UserModel>(USER_GET_BY_EMAIL_QUERY, new { email });
            if (potentialUser == null || !VerifyPasswordHash(potentialUser.Password, password))
                return false;

            potentialUser.Adventurer = _dbService.Get<AdventurerModel>(USER_GET_RELATED_ADVENTURER_QUERY, new { userId = potentialUser.UserId });

            await UpdateUserInSessionAsync(potentialUser);

            return true;
        }

		private static bool VerifyPasswordHash(byte[] hashedPassword, string password)
        {
            byte[] salt = new byte[16];
            Array.Copy(hashedPassword, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10_000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashedPassword[i + 16] != hash[i])
                    return false;
            return true;
        }

		#endregion

		#region Queries

		private const string USER_REGISTRATION_QUERY = @"
INSERT INTO dndb_user(email, password)
VALUES
(@email, @passwordHash);
";
        private const string USER_GET_BY_EMAIL_QUERY = @"
SELECT * FROM dndb_user
WHERE email = @email;
";
        private const string USER_GET_RELATED_ADVENTURER_QUERY = @"
SELECT * FROM adventurer
WHERE user_id = @userId;
";
        #endregion

    }
}
