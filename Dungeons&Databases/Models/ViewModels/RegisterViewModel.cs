namespace Dungeons_Databases.Models.ViewModels
{
    public class RegisterViewModel
    {
        public bool CheckEmailValidity { get; set; }
        public bool CheckEmailUnicity { get; set; }

        public bool CheckPasswordLength { get; set; }
        public bool CheckPasswordSpecialCharacter { get; set; }
        public bool CheckPasswordUppercaseLetter { get; set; }
        public bool CheckPasswordLowercaseLetter { get; set; }
        public bool CheckPasswordNumericLetter { get; set; }

        public bool HasNoErrors => CheckEmailValidity && CheckPasswordLength && CheckPasswordSpecialCharacter && CheckPasswordUppercaseLetter
            && CheckPasswordLowercaseLetter && CheckPasswordNumericLetter;
    }
}
