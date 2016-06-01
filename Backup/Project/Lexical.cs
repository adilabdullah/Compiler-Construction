using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Project
{
    class Lexical
    {
        public static List<string> Analyze(string Code)
        {
            List<string> Errors = new List<string>();
            List<string> Tokens = new List<string>();

            int Line = 1;
            string Word = "";
            for (int i = 0; i < Code.Length; i++)
            {
                if (Word.StartsWith("\""))
                {
                    Word += Code[i];
                    if (Code[i] == '\n' || (Code[i] == '"' && Code[i - 1] != '\\'))
                    {
                        if (Validate.String(Word))
                            Tokens.Add(TA("string", Word, Line));
                        else
                            Tokens.Add(TA("InvalidLexeme", Word, Line));

                        Word = "";
                    }
                }

                else if (Word.StartsWith("'"))
                {
                    Word += Code[i];
                    if (Code[i] == '\n' || (Code[i] == '\'' && Code[i - 1] != '\\'))
                    {
                        if (Validate.Character(Word))
                            Tokens.Add(TA("char", Word, Line));
                        else
                            Tokens.Add(TA("InvalidLexeme", Word, Line));

                        Word = "";
                    }
                }

                else if (Code[i] == '+')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '+' && Code[i + 1] != '=' && !char.IsDigit(Code[i + 1])&&Code[i+1]!='.')
                            Tokens.Add(TA("pm", "+", Line));

                        else if (Code[i + 1] == '+')
                        {
                            Tokens.Add(TA("inc_dec", "++", Line));
                            i++;
                        }
                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("assign_op", "+=", Line));
                            i++;
                        }
                        else if (char.IsDigit(Code[i + 1]) || Code[i + 1] == '.')
                        {
                            Word += Code[i];
                            i++;
                            for (; i < Code.Length - 1; i++)
                            {
                                Word += Code[i];
                                if (Code[i] == ' ' ||
                                    Code[i] == '\n' ||
                                    Code[i] == '\r' ||
                                    Code[i] == '\t' ||
                                    Code[i] == '+' ||
                                    Code[i] == '-' ||
                                    Code[i] == '*' ||
                                    Code[i] == '/' ||
                                    Code[i] == '%' ||
                                    Code[i] == '(' ||
                                    Code[i] == ')' ||
                                    Code[i] == '{' ||
                                    Code[i] == '}' ||
                                    Code[i] == '[' ||
                                    Code[i] == ']' ||
                                    Code[i] == '&' ||
                                    Code[i] == '|' ||
                                    Code[i] == '=' ||
                                    Code[i] == '<' ||
                                    Code[i] == '>' ||
                                    Code[i] == '!' ||
                                    Code[i] == ';' ||
                                    Code[i] == ',' ||
                                    Code[i] == '"' ||
                                    Code[i] == '\'')
                                {
                                    Word = Word.Substring(0, Word.Length - 1);
                                    i -= 1;
                                    if (ValidateWord(Word, Line) != null)
                                        Tokens.Add(ValidateWord(Word, Line));
                                    Word = "";
                                    break;
                                }
                            }
                        }
                    }
                    else
                        Tokens.Add(TA("pm", "+", Line));
                }

                else if (Code[i] == '-')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '-' && Code[i + 1] != '=' && !char.IsDigit(Code[i + 1]) && Code[i + 1] != '.')
                            Tokens.Add(TA("pm", "-", Line));

                        else if (Code[i + 1] == '-')
                        {
                            Tokens.Add(TA("inc_dec", "--", Line));
                            i++;
                        }
                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("assign_op", "-=", Line));
                            i++;
                        }
                        else if (char.IsDigit(Code[i + 1]) || Code[i + 1]=='.')
                        {
                            Word += Code[i];
                            i++;
                            for (; i < Code.Length - 1; i++)
                            {
                                Word += Code[i];
                                if (Code[i] == ' ' ||
                                    Code[i] == '\n' ||
                                    Code[i] == '\r' ||
                                    Code[i] == '\t' ||
                                    Code[i] == '+' ||
                                    Code[i] == '-' ||
                                    Code[i] == '*' ||
                                    Code[i] == '/' ||
                                    Code[i] == '%' ||
                                    Code[i] == '(' ||
                                    Code[i] == ')' ||
                                    Code[i] == '{' ||
                                    Code[i] == '}' ||
                                    Code[i] == '[' ||
                                    Code[i] == ']' ||
                                    Code[i] == '&' ||
                                    Code[i] == '|' ||
                                    Code[i] == '=' ||
                                    Code[i] == '<' ||
                                    Code[i] == '>' ||
                                    Code[i] == '!' ||
                                    Code[i] == ';' ||
                                    Code[i] == ',' ||
                                    Code[i] == '"' ||
                                    Code[i] == '\'')
                                {
                                    Word = Word.Substring(0, Word.Length - 1);
                                    i -= 1;
                                    if (ValidateWord(Word, Line) != null)
                                        Tokens.Add(ValidateWord(Word, Line));
                                    Word = "";
                                    break;
                                }
                            }
                        }
                    }
                    else
                        Tokens.Add(TA("pm", "-", Line));
                }

                else if (Code[i] == '*')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '=')
                            Tokens.Add(TA("mdm", "*", Line));

                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("assign_op", "*=", Line));
                            i++;
                        }
                    }
                    else
                        Tokens.Add(TA("mdm", "+", Line));
                }

                else if (Code[i] == '/')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '=')
                            Tokens.Add(TA("mdm", "/", Line));

                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("assign_op", "/=", Line));
                            i++;
                        }
                    }
                    else
                        Tokens.Add(TA("mdm", "/", Line));
                }

                else if (Code[i] == '%')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '=')
                            Tokens.Add(TA("mdm", "%", Line));

                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("assign_op", "%=", Line));
                            i++;
                        }
                    }
                    else
                        Tokens.Add(TA("mdm", "%", Line));
                }

                else if (Code[i] == '=')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '=' && Code[i + 1] != '>')
                            Tokens.Add(TA("=", "=", Line));

                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("rel_op", "==", Line));
                            i++;
                        }
                        else if (Code[i + 1] == '>')
                        {
                            Tokens.Add(TA("=>", "=>", Line));
                            i++;
                        }
                    }
                    else
                        Tokens.Add(TA("=", "=", Line));
                }

                else if (Code[i] == '<')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '=' && Code[i + 1] != '?')
                            Tokens.Add(TA("rel_op", "<", Line));

                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("rel_op", "<=", Line));
                            i++;
                        }
                        else if (Code[i + 1] == '?')
                        {
                            for (; i < Code.Length - 1; i++)
                            {
                                if (Code[i] == '\n') Line++;
                                if (Code[i] == '?' && Code[i + 1] == '>') { i++; break; }
                            }
                        }
                    }
                    else
                        Tokens.Add(TA("rel_op", "<", Line));
                }

                else if (Code[i] == '>')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '=' && Code[i + 1] != '>')
                            Tokens.Add(TA("rel_op", ">", Line));

                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("rel_op", ">=", Line));
                            i++;
                        }
                        else if (Code[i + 1] == '>')
                        {
                            for (; i < Code.Length; i++)
                                if (Code[i] == '\n') break;
                        }
                    }
                    else
                        Tokens.Add(TA("rel_op", ">", Line));
                }

                else if (Code[i] == '!')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] != '=')
                            Tokens.Add(TA("!", "!", Line));

                        else if (Code[i + 1] == '=')
                        {
                            Tokens.Add(TA("rel_op", "!=", Line));
                            i++;
                        }
                    }
                    else
                        Tokens.Add(TA("!", "!", Line));
                }

                else if (Code[i] == '&')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] == '&')
                        {
                            Tokens.Add(TA("&&", "&&", Line));
                            i++;
                        }
                        else
                            Tokens.Add(TA("InvalidLexeme", "&", Line));
                    }
                    else
                        Tokens.Add(TA("InvalidLexeme", "&", Line));
                }

                else if (Code[i] == '|')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    if (i < Code.Length - 1)
                    {
                        if (Code[i + 1] == '|')
                        {
                            Tokens.Add(TA("||", "||", Line));
                            i++;
                        }
                        else
                            Tokens.Add(TA("InvalidLexeme", "|", Line));
                    }
                    else
                        Tokens.Add(TA("InvalidLexeme", "|", Line));
                }

                else if (Code[i] == '"' || Code[i] == '\'')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));

                    Word = Code[i].ToString();
                }

                else if (Code[i] == '.')
                {
                    if (Word.Contains('.'))
                    {
                        if (Validate.Real(Word))
                        {
                            Tokens.Add(TA("real", Word, Line));
                            Word = "";
                        }
                        else
                        {
                            Tokens.Add(TA("InvalidLexeme", Word, Line));
                            Word = "";
                        }
                    }
                    else if (Validate.Keyword(Word))
                    {
                        Tokens.Add(TA(Classify.Keyword(Word), Word, Line));
                        Word = "";
                    }
                    else if (Validate.Identifier(Word))
                    {
                        Tokens.Add(TA("id", Word, Line));
                        Word = "";
                    }
                    Word += Code[i];
                }

                else if (Code[i] == '(' ||
                            Code[i] == ')' ||
                            Code[i] == '{' ||
                            Code[i] == '}' ||
                            Code[i] == '[' ||
                            Code[i] == ']' ||
                            Code[i] == ';' ||
                            Code[i] == ',')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";

                    Tokens.Add(TA(Code[i].ToString(), Code[i].ToString(), Line));
                }

                else if (Code[i] == ' ' || Code[i] == '\n' || Code[i] == '\t' || Code[i] == '\r')
                {
                    if (ValidateWord(Word, Line) != null)
                        Tokens.Add(ValidateWord(Word, Line));
                    Word = "";
                }

                else
                    Word += Code[i];

                if (i < Code.Length)
                    if (Code[i] == '\n')
                        Line++;

            }

            if (ValidateWord(Word, Line) != null)
                Tokens.Add(ValidateWord(Word, Line));

            return Tokens;
        }

        public static string ValidateWord(string Word, int Line)
        {
            string MainClass = Validate.All(Word);
            if (MainClass != null && MainClass != "Empty")
            {
                if (MainClass == "Keyword")
                    return TA(Classify.Keyword(Word), Word, Line);
                else
                    return TA(MainClass, Word, Line);
            }

            else if (MainClass == null)
                return TA("InvalidLexeme", Word, Line);

            else return null;
        }

        public static string TA(string C, string V, int L)
        {
            return "( " + C + " , " + V + " , " + L + " )";
        }
    }
}