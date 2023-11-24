namespace Dungeons_Databases.Models.Entities
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public bool IsAdmin { get; set; }
        public AdventurerModel? Adventurer { get; set; }
    }
}
