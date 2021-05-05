using System;
using System.Linq;
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

        public static bool ContainsSpecialCharacter(string str)
        {
            return str.Any(ch => !Char.IsLetterOrDigit(ch));
        }

        public static bool ContainsNumber(string str)
        {
            return str.Any(c => char.IsDigit(c));
        }

        public static bool ContainsUpperCase(string str)
        {
            return str.Any(c => char.IsUpper(c));
        }

        public static bool ContainsLowerCase(string str)
        {
            return str.Any(c => !char.IsUpper(c));
        }

        public static bool ContainsMinimumAmountOfCharacters(string str, int length)
        {
            return str.Length >= length;
        }
    }
}