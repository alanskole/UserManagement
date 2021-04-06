using System.Text.RegularExpressions;

namespace ManageUsers.Helper
{
    internal static class RegexChecker
    {
        public static Regex onlyLettersOneSpaceOrDash = new Regex(@"^(?=.{2,}$)([\p{Lu}\p{Ll}]+([\s-]?[\p{Lu}\p{Ll}]+))$");
        public static Regex onlyLettersNumbersOneSpaceOrDash = new Regex(@"^(?=.{2,}$)([\p{Lu}\p{Ll}0-9]([\s-]?[\p{Lu}\p{Ll}0-9]+)*)$");
        public static Regex addressNumber = new Regex(@"^[1-9]+[0-9]*[A-Za-z]?$");
        public static Regex isZipValidFormat = new Regex(@"^(?=.{2,10}$)([\p{Lu}\p{Ll}0-9]([\s-]?[\p{Lu}\p{Ll}0-9]+)*)$");
        public static Regex isEmailValidFormat = new Regex(@"^[\p{Lu}\p{Ll}0-9._%+-]+@[\p{Lu}\p{Ll}0-9.-]+\.[\p{Lu}\p{Ll}]{2,}$");
        public static Regex passwordMinimum8AtLeastOneNumberAndLetter = new Regex(@"^(?=.*\d)[\p{Lu}\p{Ll}\d@$!%*#?&/><+=)(}¤:;.,{_£§-]{8,}$");
        public static Regex passwordMinimum8AtLeastOneNumberAndLetterOneUpperAndLowerCase = new Regex(@"^(?=.*\d)(?=.*[\p{Ll}])(?=.*[\p{Lu}])[\p{Lu}\p{Ll}\d@$!%*#?&/><+=)(}¤:;.,{_£§-]{8,}$");
        public static Regex passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacter = new Regex(@"^(?=.*\d)(?=.*[@$!%*#?&/><+=)(}¤:;.,{_£§-])[\p{Lu}\p{Ll}\d@$!%*#?&/><+=)(}¤:;.,{_£§-]{8,}$");
        public static Regex passwordMinimum8AtLeastOneNumberAndLetterAndSpecialCharacterOneUpperAndLowerCase = new Regex(@"^(?=.*\d)(?=.*[\p{Ll}])(?=.*[\p{Lu}])(?=.*[@$!%*#?&/><+=)(}¤:;.,{_£§-])[\p{Lu}\p{Ll}\d@$!%*#?&/><+=)(}¤:;.,{_£§-]{8,}$");
    }
}