using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Project
{
    class Validate
    {
        public static string All(string Word)
        {
            if (Word.Length == 0 || Word == "\n") return "Empty";
            if (Keyword(Word)) return "Keyword";
            if (Bool(Word)) return "bool";
            if (Identifier(Word)) return "id";
            if (String(Word)) return "string";
            if (Character(Word)) return "char";
            if (Integer(Word)) return "int";
            if (Real(Word)) return "real";
            return null;
        }

        public static bool Keyword(string Word)
        {
            string[] Keywords = {   "loop",
                                    "repeat",
                                    "until",
                                    "to",
                                    "by",
                                    "if",
                                    "eif",
                                    "else",
                                    "create",
                                    "func",
                                    "ret",
                                    "void",
                                    "class",
                                    "self",
                                    "cease",
                                    "chase",
                                    "int",
                                    "real",
                                    "string",
                                    "bool",
                                    "array",
                                    "object",
                                    "known",
                                    "unknown"};
            return Keywords.Contains(Word);
        }

        public static bool Identifier(string Word)
        {
            return Regex.IsMatch(Word, "^[a-z_][a-z0-9_]*$", RegexOptions.IgnoreCase);
        }

        public static bool Bool(string Word)
        {
            return Regex.IsMatch(Word, "^(true|false)$");
        }

        public static bool String(string Word)
        {
            string Pattern = @"^""([^""\\]|((\\\"")|(\\\\)|(\\a)|(\\b)|(\\f)|(\\n)|(\\r)|(\\t)|(\\v)))*\""$";
            return Regex.IsMatch(Word, Pattern);
        }

        public static bool Character(string Word)
        {
            string Pattern = @"^'([^'\\]|((\\')|(\\\\)|(\\a)|(\\b)|(\\f)|(\\n)|(\\r)|(\\t)|(\\v)))'$";
            return Regex.IsMatch(Word, Pattern);
        }

        public static bool Integer(string Word)
        {
            return Regex.IsMatch(Word, "^(\\+|-|)[0-9]+$");
        }

        public static bool Real(string Word)
        {
            return Regex.IsMatch(Word, "^(\\+|-|)[0-9]*[.][0-9]+$");
        }
    }
}
