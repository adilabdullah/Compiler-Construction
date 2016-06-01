using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Project
{
    class Syntax : Semantic
    {
        static List<string> CP;
        static List<string> VP;
        static List<string> Line;
        static List<string> Errors;
        static int i;
        static int OpenLoops;
        static bool Returned;
        static string CRet;
        static string ClassName;
        static bool FirstTime;
        //static int Errors;

        public static List<string> Analyze(List<string> Tokens)
        {
            InitSementic();
            IC.InitIC();

            Errors = new List<string>();
            CP = new List<string>();
            VP = new List<string>();
            Line = new List<string>();
            i = 0;
            OpenLoops = 0;
            Returned = false;
            CRet = "";
            //Errors = 0;

            foreach (string Token in Tokens)
            {
                if (Token.StartsWith("( , , ,"))
                {
                    CP.Add(",");
                    VP.Add(",");
                }
                else
                {
                    CP.Add(Token.Split(',')[0].Substring(2).Replace(" ", ""));
                    VP.Add(Token.Split(',')[1].Replace(" ", ""));
                }
                Line.Add(Token.Split(',')[2].Replace(" )", ""));
            }

            ST1Index = 0;
            ST2Index = -1;
            FirstTime = true;

            bool SyntaxTree = false;

            try
            {
                SyntaxTree = S();
            }
            catch (Exception)
            {
                Errors.Add("$$Exception");
            }

            //IC.DestIC();

            if (SyntaxTree)
            {
                ST1Index = 0;
                ST2Index = -1;
                i = 0;
                OpenLoops = 0;
                Returned = false;
                CRet = "";
                FirstTime = false;
                try
                {
                    SyntaxTree = S();
                }
                catch (Exception)
                {
                    Errors.Add("$$Exception");
                }
            }
            IC.DestIC();

            if (SyntaxTree && i == CP.Count && Errors.Where(x => x.StartsWith("Semantic")).ToList().Count == 0)
                return new List<string>();

            if (Errors.Count != 0 && Errors.Where(x => x.StartsWith("Syntax") && x.EndsWith(" Expected")).ToList().Count == 0)
                return Errors;

            for (; i < CP.Count; i++)
                Errors.Add("Syntax$" + Line[i] + "$Unexpected " + CP[i]);

            return Errors;
        }


        static bool S()
        {
            if (Class_Def())
            {
                int c = HeadLookUp();
                if (FirstTime && c != 1)
                {
                    List<string> Temp = new List<string>();
                    if (c == 0)
                        Temp.Add("Semantic$$Program does not contain 'head' method for an entry point");
                    else
                        Temp.Add("Semantic$$Program contains more than one 'head' method for an entry point");
                    Errors = Temp.Concat(Errors).ToList();
                }
                return true;
            }

            if (Errors.Count == 0)
                Errors.Add("Syntax$$");
            return false;
        }


        static bool Func_Def(string AM)
        {
            if (i < CP.Count && CP[i] == "func")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    string N = VP[i];
                    i++;
                    if (i < CP.Count && CP[i] == "(")
                    {
                        i++;
                        string PL = "";
                        string CL = "";
                        CreateScope();
                        if (Para_Def(ref PL, ref CL))
                        {
                            if (i < CP.Count && CP[i] == ")")
                            {
                                i++;
                                string RT = "";
                                if (Ret_Def(ref RT))
                                {
                                    CRet = RT;
                                    Returned = false;
                                    if (!FirstTime)
                                        IC.Gen(ClassName + "_" + N + "_" + PL + " proc");
                                    if (FirstTime)
                                    {
                                        if (!FDLookUp(N, PL))
                                            Insert2(N, PL + ">" + RT, AM);
                                        else
                                            Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared function - " + N);
                                    }
                                    string[] Temp = CL.Split(',');
                                    if (CL != "")
                                    {
                                        for (int k = 0; k < Temp.Length; k++)
                                        {
                                            if (!FirstTime)
                                            {
                                                if (!DLookUp(Temp[k].Split(' ')[1], false))
                                                    Insert3(Temp[k].Split(' ')[1], Temp[k].Split(' ')[0]);
                                                else
                                                    Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + Temp[k].Split(' ')[1]);
                                            }
                                        }
                                    }

                                    if (Func_Body())
                                    {
                                        if (!FirstTime)
                                            IC.Gen(ClassName + "_" + N + "_" + PL + " endp\n");
                                        DestroyScope();
                                        return true;

                                    }
                                }
                            }
                            else
                                Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");

                return false;
            }

            return true;
        }


        static bool Para_Def(ref string PL, ref string CL)
        {
            if (i < CP.Count && CP[i] == "void")
            {
                PL = "void";
                i++;
                return true;
            }

            else if (i < CP.Count && (CP[i] == "dt" || CP[i] == "id"))
            {
                string T = "";
                if (DT_ID(ref T))
                {
                    PL = T;
                    CL = T;
                    if (i < CP.Count && CP[i] == "id")
                    {
                        string N = VP[i];
                        CL += " " + N;
                        //Insert3(N, T);
                        i++;
                        if (Para_Def2(ref PL, ref CL))
                        {
                            return true;
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                    return false;
                }
            }

            PL = "void";
            return true;
        }


        static bool Para_Def2(ref string PL, ref string CL)
        {
            if (i < CP.Count && CP[i] == ",")
            {
                i++;
                string T = "";
                if (DT_ID(ref T))
                {
                    PL += "," + T;
                    CL += "," + T;
                    if (i < CP.Count && CP[i] == "id")
                    {
                        string N = VP[i];
                        CL += " " + N;
                        //   if (!DLookUp(N))
                        // Insert3(N, T);
                        //   else
                        // Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);

                        i++;
                        if (Para_Def2(ref PL, ref CL))
                        {
                            return true;
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Datatype or identifier expected1");
                return false;
            }

            return true;
        }


        static bool Ret_Def(ref string RT)
        {
            if (i < CP.Count && CP[i] == "ret")
            {
                i++;
                if (Ret_Def2(ref RT))
                {
                    return true;
                }
                Errors.Add("Syntax$" + Line[i - 1] + "$Datatype or identifier expected");
                return false;
            }

            RT = "void";
            return true;
        }


        static bool Ret_Def2(ref string RT)
        {
            if (i < CP.Count && CP[i] == "void")
            {
                RT = "void";
                i++;
                return true;
            }

            else if (i < CP.Count && (CP[i] == "dt" || CP[i] == "id"))
            {
                if (DT_ID(ref RT))
                {
                    return true;
                }
            }

            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Return type expected");

            return false;
        }


        static bool Func_Body()
        {
            if (i < CP.Count && CP[i] == ";")
            {
                i++;
                return true;
            }

            else if (i < CP.Count && CP[i] == "{")
            {
                i++;
                if (M_St())
                {
                    if (!FirstTime && Returned == false && CRet != "void")
                        Errors.Add("Semantic$" + Line[i - 1] + "$Function did not return a value");

                    if (i < CP.Count && CP[i] == "}")
                    {
                        i++;
                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$} expected");
                }
            }

            else
                Errors.Add("Syntax$" + Line[i - 1] + "$; or { expected");

            return false;
        }


        static bool DT_ID(ref string T)
        {
            if (i < CP.Count && CP[i] == "dt")
            {
                T = VP[i];
                i++;
                if (DT_ID2(ref T))
                {
                    return true;
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "id")
            {
                T = VP[i];
                i++;
                if (DT_ID2(ref T))
                {
                    return true;
                }
            }

            return false;
        }


        static bool DT_ID2(ref string T)
        {
            if (i < CP.Count && CP[i] == "[")
            {
                i++;
                if (i < CP.Count && CP[i] == "]")
                {
                    T += "[]";
                    i++;
                    return true;
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$] expected");
                return false;
            }

            return true;
        }


        static bool Class_Def()
        {
            if (i < CP.Count && CP[i] == "class")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    string N = VP[i];
                    ClassName = N;
                    i++;

                    CreateScope();
                    if (FirstTime)
                    {
                        if (!CDLookUp(N))
                            Insert1(N);
                        else
                            Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared class - " + N);
                    }
                    if (i < CP.Count && CP[i] == "{")
                    {
                        i++;
                        if (FirstTime)
                            IC.Gen(N + "_" + N + "_void proc");
                        IC.Enable = true;
                        if (Class_Body())
                        {
                            IC.Enable = true;
                            if (FirstTime)
                                IC.Gen(N + "_" + N + "_void endp\n");
                            if (i < CP.Count && CP[i] == "}")
                            {
                                i++;
                                ST1Index++;
                                ST2Index = -1;
                                DestroyScope();
                                if (Class_Def())
                                {
                                    return true;
                                }
                            }
                            else
                                Errors.Add("Syntax$" + Line[i - 1] + "$} expected");
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "${ expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                return false;
            }

            return true;
        }


        static void AccessModifier(ref string AM)
        {
            if (i < CP.Count && CP[i] == "am")
            {
                AM = VP[i];
                i++;
            }
        }


        static bool Class_Body()
        {
            ST2Index++;
            string AM = "";
            AccessModifier(ref AM);
            if (i < CP.Count && CP[i] == "func")
            {
                IC.Enable = !FirstTime;
                if (Func_Def(AM))
                {
                    if (Class_Body())
                    {
                        return true;
                    }
                }
                return false;
            }

            if (i < CP.Count && (CP[i] == "dt" || CP[i] == "id"))
            {
                IC.Enable = FirstTime;
                if (All_Dec(AM, true))
                {
                    if (Class_Body())
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        static bool All_Dec(string AM, bool CL)
        {
            if (i < CP.Count && CP[i] == "dt")
            {
                string T = VP[i];
                i++;
                if (All_Dec2(ref T, AM, true))
                {
                    return true;
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "id")
            {
                string T = VP[i];
                i++;

                if (!FirstTime)
                {
                    if (!CDLookUp(T))
                        Errors.Add("Semantic$" + Line[i - 1] + "$Undeclared class - " + T);
                }

                if (All_Dec3(ref T, AM, true))
                {
                    return true;
                }
            }

            return false;
        }


        static bool All_Dec2(ref string T, string AM, bool CL)
        {
            if (i < CP.Count && CP[i] == "id")
            {
                string N = VP[i];
                i++;

                if (CL)
                {
                    if (FirstTime)
                    {
                        if (!DLookUp(N, CL))
                            Insert2(N, T, AM);
                        else
                            Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                    }
                }
                else
                {
                    if (!FirstTime)
                    {
                        if (!DLookUp(N, CL))
                            Insert3(N, T);
                        else
                            Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                    }
                }

                if (DT_Init(ref T, ref N))
                {
                    if (DT_Dec2(ref T, AM, CL))
                    {
                        return true;
                    }
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "[")
            {
                i++;
                if (i < CP.Count && CP[i] == "]")
                {
                    i++;
                    T += "[]";
                    if (i < CP.Count && CP[i] == "id")
                    {
                        string N = VP[i];
                        i++;

                        if (CL)
                        {
                            if (FirstTime)
                            {
                                if (!DLookUp(N, CL))
                                    Insert2(N, T, AM);
                                else
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                            }
                        }
                        else
                        {
                            if (!FirstTime)
                            {
                                if (!DLookUp(N, CL))
                                    Insert3(N, T);
                                else
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                            }
                        }

                        if (DT_A_Init(ref T, ref N))
                        {
                            if (DT_A_Dec2(ref T, AM, CL))
                            {
                                return true;
                            }
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$] expected");
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Identifier or [ expected");

            return false;
        }


        static bool All_Dec3(ref string T, string AM, bool CL)
        {
            if (i < CP.Count && CP[i] == "id")
            {
                string N = VP[i];
                i++;

                if (CL)
                {
                    if (FirstTime)
                    {
                        if (!DLookUp(N, CL))
                            Insert2(N, T, AM);
                        else
                            Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                    }
                }
                else
                {
                    if (!FirstTime)
                    {
                        if (!DLookUp(N, CL))
                            Insert3(N, T);
                        else
                            Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                    }
                }

                if (Ob_Init(ref T, ref N))
                {
                    if (Ob_Dec2(ref T, AM, CL))
                    {
                        return true;
                    }
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "[")
            {
                i++;
                if (i < CP.Count && CP[i] == "]")
                {
                    i++;
                    T += "[]";
                    if (i < CP.Count && CP[i] == "id")
                    {
                        string N = VP[i];
                        i++;

                        if (CL)
                        {
                            if (FirstTime)
                            {
                                if (!DLookUp(N, CL))
                                    Insert2(N, T, AM);
                                else
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                            }
                        }
                        else
                        {
                            if (!FirstTime)
                            {
                                if (!DLookUp(N, CL))
                                    Insert3(N, T);
                                else
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                            }
                        }

                        if (Ob_A_Init(ref T, ref N))
                        {
                            if (Ob_A_Dec2(ref T, AM, CL))
                            {
                                return true;
                            }
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$] expected");
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Identifier or [ expected");

            return false;
        }


        static bool DT_Dec2(ref string T, string AM, bool CL)
        {
            if (i < CP.Count && CP[i] == ";")
            {
                i++;
                return true;
            }

            else if (i < CP.Count && CP[i] == ",")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    string N = VP[i];
                    i++;

                    if (CL)
                    {
                        if (FirstTime)
                        {
                            if (!DLookUp(N, CL))
                                Insert2(N, T, AM);
                            else
                                Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                        }
                    }
                    else
                    {
                        if (!FirstTime)
                        {
                            if (!DLookUp(N, CL))
                                Insert3(N, T);
                            else
                                Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                        }
                    }

                    if (DT_Init(ref T, ref N))
                    {
                        if (DT_Dec2(ref T, AM, CL))
                        {
                            return true;
                        }
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$; or , expected");

            return false;
        }


        static bool DT_Init(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "=")
            {
                i++;
                string T2 = "";
                string N2 = "";
                if (Exp(ref T2, ref N2))
                {
                    if (!FirstTime)
                    {
                        if (!FirstTime && T != null && T2 != null)
                        {
                            if (Comp(T, T2, "=") == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");
                        }
                    }
                    IC.Gen(N + "=" + N2);

                    return true;
                }
                return false;
            }

            return true;
        }


        static bool DT_A_Dec2(ref string T, string AM, bool CL)
        {
            if (i < CP.Count && CP[i] == ";")
            {
                i++;
                return true;
            }

            else if (i < CP.Count && CP[i] == ",")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    string N = VP[i];
                    i++;

                    if (CL)
                    {
                        if (FirstTime)
                        {
                            if (!DLookUp(N, CL))
                                Insert2(N, T, AM);
                            else
                                Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                        }
                    }
                    else
                    {
                        if (!FirstTime)
                        {
                            if (!DLookUp(N, CL))
                                Insert3(N, T);
                            else
                                Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                        }
                    }

                    if (DT_A_Init(ref T, ref N))
                    {
                        if (DT_A_Dec2(ref T, AM, CL))
                        {
                            return true;
                        }
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$; or , expected");

            return false;
        }


        static bool DT_A_Init(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "=")
            {
                i++;
                if (DT_A_Init2(ref T, ref N))
                {
                    return true;
                }
                return false;
            }

            return true;
        }


        static bool DT_A_Init2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "create")
            {
                i++;
                if (i < CP.Count && CP[i] == "array")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "(")
                    {
                        i++;
                        if (i < CP.Count && CP[i] == "dt")
                        {
                            string T2 = VP[i] + "[]";
                            i++;

                            if (!FirstTime)
                            {
                                if (!FirstTime && T != null && T2 != null && T != T2)
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");
                            }
                            if (i < CP.Count && CP[i] == ",")
                            {
                                i++;
                                string T3 = "";
                                string Temp = "";
                                if (Exp(ref T3, ref Temp))
                                {
                                    if (!FirstTime)
                                    {
                                        if (!FirstTime && T3 != null && T3 != "int")
                                            Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T3 + "' to 'int'");
                                    }
                                    if (i < CP.Count && CP[i] == ")")
                                    {
                                        i++;
                                        return true;
                                    }
                                    else
                                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                                }
                            }
                            else
                                Errors.Add("Syntax$" + Line[i - 1] + "$, expected");
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$Datatype expected");
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
                }
                return false;
            }

            string T4 = "";
            string N4 = "";
            if (Exp(ref T4, ref N4))
            {
                if (!FirstTime)
                {
                    if (!FirstTime && T != null && T4 != null && T != T4)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T4 + "' to '" + T + "'");
                }
                IC.Gen(N + "=" + N4);

                return true;
            }

            else if (i < CP.Count && CP[i] == "{")
            {
                i++;
                if (DT_A_Elem(ref T))
                {
                    if (i < CP.Count && CP[i] == "}")
                    {
                        i++;
                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$} expected");
                }
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Array initialization expected");

            return false;
        }


        static bool DT_A_Elem(ref string T)
        {
            string T2 = "";
            string Temp = "";
            if (Exp(ref T2, ref Temp))
            {
                if (!FirstTime)
                {
                    if (!FirstTime && T != null && T2 != null)
                    {
                        if (Comp(T, T2, "=") == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");
                    }
                }

                if (DT_A_Elem2(ref T))
                {
                    return true;
                }
            }

            return false;
        }


        static bool DT_A_Elem2(ref string T)
        {
            if (i < CP.Count && CP[i] == ",")
            {
                i++;
                string T2 = "";
                string Temp = "";
                if (Exp(ref T2, ref Temp))
                {
                    if (!FirstTime)
                    {
                        if (!FirstTime && T != null && T2 != null)
                        {
                            if (Comp(T, T2, "=") == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");
                        }
                    }

                    if (DT_A_Elem2(ref T))
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }


        static bool Ob_Dec2(ref string T, string AM, bool CL)
        {
            if (i < CP.Count && CP[i] == ";")
            {
                i++;
                return true;
            }

            else if (i < CP.Count && CP[i] == ",")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    string N = VP[i];
                    i++;

                    if (CL)
                    {
                        if (FirstTime)
                        {
                            if (!DLookUp(N, CL))
                                Insert2(N, T, AM);
                            else
                                Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                        }
                    }
                    else
                    {
                        if (!FirstTime)
                        {
                            if (!DLookUp(N, CL))
                                Insert3(N, T);
                            else
                                Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                        }
                    }

                    if (Ob_Init(ref T, ref N))
                    {
                        if (Ob_Dec2(ref T, AM, CL))
                        {
                            return true;
                        }
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$; or , expected");

            return false;
        }


        static bool Ob_Init(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "=")
            {
                i++;
                if (Ob_Init2(ref T, ref N))
                {
                    return true;
                }
                return false;
            }

            return true;
        }


        static bool Ob_Init2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "create")
            {
                i++;
                if (i < CP.Count && CP[i] == "object")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "(")
                    {
                        i++;
                        if (i < CP.Count && CP[i] == "id")
                        {
                            string T2 = VP[i];
                            i++;

                            if (!FirstTime)
                            {
                                if (!FirstTime && T != null && T2 != null && T != T2)
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");
                            }
                            if (i < CP.Count && CP[i] == ")")
                            {
                                i++;
                                return true;
                            }
                            else
                                Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Keyword object expected");

                return false;
            }

            string T3 = "";
            string N3 = "";
            if (Exp(ref T3, ref N3))
            {
                if (!FirstTime)
                {
                    if (!FirstTime && T != null && T3 != null && T != T3)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T3 + "' to '" + T + "'");
                }
                IC.Gen(N + "=" + N3);

                return true;
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Object Initialization expected");

            return false;
        }


        static bool Ob_A_Dec2(ref string T, string AM, bool CL)
        {
            if (i < CP.Count && CP[i] == ";")
            {
                i++;
                return true;
            }

            else if (i < CP.Count && CP[i] == ",")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    string N = VP[i];
                    i++;

                    if (CL)
                    {
                        if (FirstTime)
                        {
                            if (!DLookUp(N, CL))
                                Insert2(N, T, AM);
                            else
                                Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                        }
                    }
                    else
                    {
                        if (!FirstTime)
                        {
                            if (!DLookUp(N, CL))
                                Insert3(N, T);
                            else
                                Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                        }
                    }

                    if (Ob_A_Init(ref T, ref N))
                    {
                        if (Ob_A_Dec2(ref T, AM, CL))
                        {
                            return true;
                        }
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$; or , expected");

            return false;
        }


        static bool Ob_A_Init(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "=")
            {
                i++;
                if (Ob_A_Init2(ref T, ref N))
                {
                    return true;
                }
                return false;
            }

            return true;
        }


        static bool Ob_A_Init2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "create")
            {
                i++;
                if (i < CP.Count && CP[i] == "array")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "(")
                    {
                        i++;
                        if (i < CP.Count && CP[i] == "id")
                        {
                            string T2 = VP[i] + "[]";
                            i++;

                            if (!FirstTime)
                            {
                                if (!FirstTime && T != null && T2 != null && T != T2)
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");
                            }
                            if (i < CP.Count && CP[i] == ",")
                            {
                                i++;
                                string T3 = "";
                                string Temp = "";
                                if (Exp(ref T3, ref Temp))
                                {
                                    if (!FirstTime)
                                    {
                                        if (!FirstTime && T3 != null && T3 != "int")
                                            Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T3 + "' to 'int'");
                                    }
                                    if (i < CP.Count && CP[i] == ")")
                                    {
                                        i++;
                                        return true;
                                    }
                                    else
                                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                                }
                            }
                            else
                                Errors.Add("Syntax$" + Line[i - 1] + "$, expected");
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Keyword array expected");

                return false;
            }

            string T4 = "";
            string N4 = "";
            if (Exp(ref T4, ref N4))
            {
                if (!FirstTime)
                {
                    if (!FirstTime && T != null && T4 != null && T != T4)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T4 + "' to '" + T + "'");
                }
                IC.Gen(N + "=" + N4);

                return true;
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Array initialization expected");

            return false;
        }


        static bool Loop()
        {
            if (i < CP.Count && CP[i] == "loop")
            {
                i++;
                if (i < CP.Count && CP[i] == "(")
                {
                    i++;
                    CreateScope();
                    string N = "";
                    if (Loop_C(ref N))
                    {
                        if (i < CP.Count && CP[i] == "to")
                        {
                            i++;

                            string L1 = IC.GetLabel();
                            IC.Gen(L1 + ":");

                            string T2 = "";
                            string N2 = "";
                            if (Exp(ref T2, ref N2))
                            {
                                if (!FirstTime)
                                {
                                    if (!FirstTime && T2 != null && T2 != "bool")
                                        Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to 'bool'");
                                }
                                string L2 = IC.GetLabel();
                                IC.Gen("if(" + N2 + "==false) jmp " + L2);
                                string L3 = IC.GetLabel();
                                IC.Gen("jmp " + L3);
                                string L4 = IC.GetLabel();
                                IC.Gen(L4 + ":");

                                if (i < CP.Count && CP[i] == "by")
                                {
                                    i++;
                                    string T3 = "";
                                    string N3 = "";
                                    if (Exp(ref T3, ref N3))
                                    {
                                        if (!FirstTime)
                                        {
                                            if (!FirstTime && T3 != null && T3 != "int")
                                                Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T3 + "' to 'int'");
                                        }
                                        string N4 = IC.GetTemp();
                                        IC.Gen(N4 + "=" + N + "+" + N3);
                                        IC.Gen(N + "=" + N4);
                                        IC.Gen("jmp " + L1);
                                        IC.Gen(L3 + ":");

                                        if (i < CP.Count && CP[i] == ")")
                                        {
                                            i++;
                                            OpenLoops++;
                                            if (Body())
                                            {
                                                IC.Gen("jmp " + L4);
                                                IC.Gen(L2 + ":");
                                                OpenLoops--;
                                                DestroyScope();
                                                return true;
                                            }
                                        }
                                        else
                                            Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                                    }
                                }
                                else
                                    Errors.Add("Syntax$" + Line[i - 1] + "$by expected");
                            }
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$to expected");
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
            }

            return false;
        }


        static bool Loop_C(ref string N)
        {
            if (i < CP.Count && CP[i] == "dt")
            {
                string T = VP[i];
                i++;

                if (!FirstTime && T != null && T != "int")
                    Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T + "' to 'int'");

                if (i < CP.Count && CP[i] == "id")
                {
                    N = VP[i];
                    i++;
                    if (!FirstTime)
                    {
                        if (!DLookUp(N, false))
                            Insert3(N, T);
                        else
                            Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N);
                    }
                    if (i < CP.Count && CP[i] == "=")
                    {
                        i++;
                        string T2 = "";
                        string N2 = "";
                        if (Exp(ref T2, ref N2))
                        {
                            if (!FirstTime && T != null && T2 != null && T != T2)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T + "' to '" + T2 + "'");

                            IC.Gen(N + "=" + N2);

                            return true;
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$= expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
            }

            string T3 = "";
            if (Var(ref T3, ref N))
            {
                if (!FirstTime && T3 != null && T3 != "int")
                    Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T3 + "' to 'int'");

                return true;
            }

            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Variable or datatype expected");

            return false;
        }


        static bool Repeat()
        {
            if (i < CP.Count && CP[i] == "repeat")
            {
                i++;

                string L1 = IC.GetLabel();
                IC.Gen(L1 + ":");

                OpenLoops++;
                CreateScope();
                if (Body())
                {
                    OpenLoops--;
                    if (i < CP.Count && CP[i] == "until")
                    {
                        i++;
                        if (i < CP.Count && CP[i] == "(")
                        {
                            i++;
                            string T = "";
                            string N = "";
                            if (Exp(ref T, ref N))
                            {
                                if (!FirstTime && T != null && T != "bool")
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T + "' to 'bool'");

                                IC.Gen("if(" + N + "==true) jmp " + L1);

                                if (i < CP.Count && CP[i] == ")")
                                {
                                    i++;
                                    if (i < CP.Count && CP[i] == ";")
                                    {
                                        i++;
                                        DestroyScope();
                                        return true;
                                    }
                                    else
                                        Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
                                }
                                else
                                    Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                            }
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Keyword until expected");
                }
            }

            return false;
        }


        static bool If()
        {
            if (i < CP.Count && CP[i] == "if")
            {
                i++;

                string L1 = IC.GetLabel();

                if (i < CP.Count && CP[i] == "(")
                {
                    i++;
                    CreateScope();
                    string T = "";
                    string N = "";
                    if (Exp(ref T, ref N))
                    {
                        if (!FirstTime && T != null && T != "bool")
                            Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T + "' to 'bool'");

                        string L2 = IC.GetLabel();
                        IC.Gen("if(" + N + "==false) jmp " + L2);

                        if (i < CP.Count && CP[i] == ")")
                        {
                            i++;
                            if (Body())
                            {
                                DestroyScope();
                                if (Eif(L1, ref L2))
                                {
                                    if (Else(L1, ref L2))
                                    {
                                        IC.Gen(L1 + ":");

                                        return true;
                                    }
                                }
                            }
                            else
                                Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                        }
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
            }

            return false;
        }


        static bool Eif(string L1, ref string L2)
        {
            if (i < CP.Count && CP[i] == "eif")
            {
                i++;

                IC.Gen("jmp " + L1);
                IC.Gen(L2 + ":");

                CreateScope();
                if (i < CP.Count && CP[i] == "(")
                {
                    i++;
                    string T = "";
                    string N = "";
                    if (Exp(ref T, ref N))
                    {
                        if (!FirstTime && T != null && T != "bool")
                            Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T + "' to 'bool'");

                        L2 = IC.GetLabel();
                        IC.Gen("if(" + N + "==false) jmp " + L2);

                        if (i < CP.Count && CP[i] == ")")
                        {
                            i++;
                            if (Body())
                            {
                                DestroyScope();
                                if (Eif(L1, ref L2))
                                {
                                    return true;
                                }
                            }
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
                return false;
            }

            return true;
        }


        static bool Else(string L1, ref string L2)
        {
            if (i < CP.Count && CP[i] == "else")
            {
                i++;

                IC.Gen("jmp " + L1);
                IC.Gen(L2 + ":");

                CreateScope();
                if (Body())
                {
                    DestroyScope();
                    return true;
                }
                return false;
            }

            IC.Gen(L2 + ":");
            return true;
        }


        static bool Body()
        {
            if (i < CP.Count && CP[i] == ";")
            {
                i++;
                return true;
            }

            else if (i < CP.Count && CP[i] == "{")
            {
                i++;
                if (M_St())
                {
                    if (i < CP.Count && CP[i] == "}")
                    {
                        i++;
                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$} expected");
                }
                return false;
            }

            else if (S_St())
            {
                return true;
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$; or { or single statement expected");

            return false;
        }


        static bool Array(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "[")
            {
                i++;
                if (!FirstTime && T != null && !T.EndsWith("[]"))
                    Errors.Add("Semantic$" + Line[i - 1] + "$Variable is not an array");

                string T2 = "";
                string N2 = "";
                if (Exp(ref T2, ref N2))
                {
                    if (!FirstTime && T2 != null && T2 != "int")
                        Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T + "' to 'int'");

                    if (i < CP.Count && CP[i] == "]")
                    {
                        i++;
                        if (!FirstTime)
                        {
                            T = T.Replace("[]", "");

                            N += "[" + N2 + "]";
                        }
                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$] expected");
                }
                return false;
            }

            return true;
        }


        static bool Inc_Dec(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "inc_dec")
            {
                if (!FirstTime && T != null && !Comp(T, "inc_dec"))
                    Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + VP[i] + "' cannot be applied to operand of type '" + T + "'");

                string T1 = IC.GetTemp();
                IC.Gen(T1 + "=" + N + VP[i][0] + 1);
                IC.Gen(N + "=" + T1);

                i++;
                return true;
            }

            return true;
        }


        static bool Para_Call(ref string PL)
        {
            string T = "";
            string N = "";
            if (Exp(ref T, ref N))
            {
                PL = T;
                IC.Gen("param " + N);
                if (Para_Call2(ref PL))
                {
                    return true;
                }
                return false;
            }

            PL = "void";
            return true;
        }


        static bool Para_Call2(ref string PL)
        {
            if (i < CP.Count && CP[i] == ",")
            {
                i++;
                string T = "";
                string N = "";
                if (Exp(ref T, ref N))
                {
                    PL += "," + T;
                    IC.Gen("param " + N);
                    if (Para_Call2(ref PL))
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        static bool Ret_St()
        {
            if (i < CP.Count && CP[i] == "ret")
            {
                i++;
                string RT = "";
                if (Ret_St2(ref RT))
                {
                    if (!FirstTime && CRet != RT)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Function dont return value of type - " + RT);
                    Returned = true;

                    return true;
                }
            }

            return false;
        }


        static bool Ret_St2(ref string T)
        {
            if (i < CP.Count && CP[i] == ";")
            {
                i++;
                T = "void";
                return true;
            }

            string N = "";
            if (Exp(ref T, ref N))
            {
                if (i < CP.Count && CP[i] == ";")
                {
                    i++;
                    return true;
                }

            }
            return false;
        }


        static bool Var_Func(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "inc_dec")
            {
                string Op = VP[i];
                i++;
                if (Var_Func7(ref T, ref N))
                {
                    if (!FirstTime && T != null && !Comp(T, "inc_dec"))
                        Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + Op + "' cannot be applied to operand of type '" + T + "'");

                    return true;
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "self")
            {
                i++;
                if (i < CP.Count && CP[i] == "=>")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "id")
                    {
                        N = VP[i];
                        i++;
                        if (Var_Func3(ref N, ref T))
                        {
                            return true;
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$=> expected");
                return false;
            }

            else if (i < CP.Count && CP[i] == "id")
            {
                N = VP[i];
                i++;
                if (Var_Func4(ref N, ref T))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Var_Func2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "=>")
            {
                N = VP[i];
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    i++;

                    if (!FirstTime)
                    {
                        int[] Temp = AccessLookUp(T, N);
                        if (Temp == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist in class '" + T + "'");
                        else
                        {
                            if (ST[Temp[0]].Link.AM[Temp[1]] == "unknown")
                                Errors.Add("Semantic$" + Line[i - 1] + "$Variable  '" + T + "=>" + N + "' is inaccessible outside class");
                            else
                                T = ST[Temp[0]].Link.Type[Temp[1]];
                        }
                    }

                    if (Array(ref T, ref N))
                    {
                        return true;
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                return false;
            }

            return true;
        }


        static bool Var_Func3(ref string N, ref string T)
        {
            if (i < CP.Count && CP[i] == "(")
            {
                i++;
                string PL = "";
                if (Para_Call(ref PL))
                {
                    if (i < CP.Count && CP[i] == ")")
                    {
                        i++;
                        if (!FirstTime)
                        {
                            T = FLookUp(N, PL);
                            if (T == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N + "'(" + PL + ")' does not exist");
                        }
                        if (PL != null && PL == "void")
                            IC.Gen("call " + ClassName + "_" + N + "_" + PL + ", 0");
                        else if (PL != null && PL != "void")
                        {
                            string Temp = IC.GetTemp();
                            IC.Gen(Temp + " = call " + ClassName + "_" + N + "_" + PL + ", " + (PL.ToCharArray().Count(x => x == ',') + 1));
                            N = Temp;
                        }

                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                }
                return false;
            }

            if (!FirstTime)
            {
                T = SelfLookUp(N);
                if (T == null)
                    Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");
            }
            if (Array(ref T, ref N))
            {
                if (Inc_Dec(ref T, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Var_Func4(ref string N, ref string T)
        {
            if (i < CP.Count && CP[i] == "(")
            {
                i++;
                string PL = "";
                if (Para_Call(ref PL))
                {
                    if (i < CP.Count && CP[i] == ")")
                    {
                        i++;
                        if (!FirstTime)
                        {
                            T = FLookUp(N, PL);
                            if (T == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N + "(" + PL + ")' does not exist");
                        }
                        if (PL != null && PL == "void")
                            IC.Gen("call " + ClassName + "_" + N + "_" + PL + ", 0");
                        else if (PL != null && PL != "void")
                        {
                            string Temp = IC.GetTemp();
                            IC.Gen(Temp + " = call " + ClassName + "_" + N + "_" + PL + ", " + (PL.ToCharArray().Count(x => x == ',') + 1));
                            N = Temp;
                        }

                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                }
                return false;
            }

            if (!FirstTime)
            {
                T = LookUp(N);
                if (T == null)
                    Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");
            }
            if (Array(ref T, ref N))
            {
                if (Var_Func5(ref N, ref T))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Var_Func5(ref string N, ref string T)
        {
            if (i < CP.Count && CP[i] == "=>")
            {
                i++;

                if (i < CP.Count && CP[i] == "id")
                {
                    N = VP[i];
                    i++;
                    if (Var_Func6(ref N, ref T))
                    {
                        return true;
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                return false;
            }

            else if (Inc_Dec(ref T, ref N))
            {
                return true;
            }

            return false;
        }


        static bool Var_Func6(ref string N, ref string T)
        {
            if (i < CP.Count && CP[i] == "(")
            {
                i++;
                string PL = "";
                if (Para_Call(ref PL))
                {
                    if (i < CP.Count && CP[i] == ")")
                    {
                        if (!FirstTime)
                        {
                            int[] Temp = AccessLookUp(T, N, PL);
                            if (Temp == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N + "'(" + PL + ")' does not exist in class '" + T + "'");
                            else
                            {
                                if (ST[Temp[0]].Link.AM[Temp[1]] == "unknown")
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Function  '" + T + "=>" + N + "(" + PL + ")' is inaccessible outside class");
                                else
                                    T = ST[Temp[0]].Link.Type[Temp[1]].Split('>')[1];
                            }
                        }
                        if (PL != null && PL == "void")
                            IC.Gen("call " + ClassName + "_" + N + "_" + PL + ", 0");
                        else if (PL != null && PL != "void")
                        {
                            string Temp = IC.GetTemp();
                            IC.Gen(Temp+" = call " + ClassName + "_" + N + "_" + PL + ", " + (PL.ToCharArray().Count(x => x == ',') + 1));
                            N = Temp;
                        }

                        i++;
                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                }
                return false;
            }

            if (!FirstTime)
            {
                int[] Temp = AccessLookUp(T, N);
                if (Temp == null)
                    Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist in class '" + T + "'");
                else
                {
                    if (ST[Temp[0]].Link.AM[Temp[1]] == "unknown")
                        Errors.Add("Semantic$" + Line[i - 1] + "$Variable  '" + T + "=>" + N + "' is inaccessible outside class");
                    else
                        T = ST[Temp[0]].Link.Type[Temp[1]];
                }
            }

            if (Array(ref T, ref N))
            {
                if (Inc_Dec(ref T, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Var_Func7(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "self")
            {
                i++;
                if (i < CP.Count && CP[i] == "=>")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "id")
                    {
                        N = VP[i];
                        if (!FirstTime)
                        {
                            T = SelfLookUp(N);
                            if (T == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$5Variable '" + N + "' does not exist");
                        }
                        i++;
                        if (Array(ref T, ref N))
                        {
                            return true;
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$=> expected");
                return false;
            }

            else if (i < CP.Count && CP[i] == "id")
            {
                N = VP[i];
                if (!FirstTime)
                {
                    T = LookUp(N);
                    if (T == null)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");
                }
                i++;
                if (Array(ref T, ref N))
                {
                    if (Var_Func2(ref T, ref N))
                    {
                        return true;
                    }
                }
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Identifier or self expected");

            return false;
        }


        static bool Var(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "inc_dec")
            {
                string Op = VP[i];
                i++;
                if (Var4(ref T, ref N))
                {
                    if (!FirstTime && !Comp(T, "inc_dec"))
                        Errors.Add("Semantic$" + Line[i - 1] + "$'" + Op + "' cannot be applied to operand of type '" + T + "'");

                    string T1 = IC.GetTemp();
                    IC.Gen(T1 + "=" + N + Op[0] + 1);
                    IC.Gen(N + "=" + T1);

                    return true;
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "self")
            {
                i++;
                if (i < CP.Count && CP[i] == "=>")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "id")
                    {
                        N = VP[i];
                        i++;

                        if (!FirstTime)
                        {
                            T = SelfLookUp(N);
                            if (T == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$6Variable '" + N + "' does not exist");
                        }
                        if (Array(ref T, ref N))
                        {
                            if (Inc_Dec(ref T, ref N))
                            {
                                return true;
                            }
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$=> expected");
                return false;
            }

            else if (i < CP.Count && CP[i] == "id")
            {
                N = VP[i];
                i++;

                if (!FirstTime)
                {
                    T = LookUp(N);
                    if (T == null)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");
                }
                if (Array(ref T, ref N))
                {
                    if (Var3(ref T, ref N))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        static bool Var2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "=>")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    N = VP[i];
                    i++;

                    if (!FirstTime)
                    {
                        int[] Temp = AccessLookUp(T, N);
                        if (Temp == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist in class '" + T + "'");
                        else
                        {
                            if (ST[Temp[0]].Link.AM[Temp[1]] == "unknown")
                                Errors.Add("Semantic$" + Line[i - 1] + "$Variable  '" + T + "=>" + N + "' is inaccessible outside class");
                            else
                                T = ST[Temp[0]].Link.Type[Temp[1]];
                        }
                    }

                    if (Array(ref T, ref N))
                    {
                        return true;
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                return false;
            }

            return true;
        }


        static bool Var3(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "=>")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    N = VP[i];
                    i++;

                    if (!FirstTime)
                    {
                        int[] Temp = AccessLookUp(T, N);
                        if (Temp == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist in class '" + T + "'");
                        else
                        {
                            if (ST[Temp[0]].Link.AM[Temp[1]] == "unknown")
                                Errors.Add("Semantic$" + Line[i - 1] + "$Variable  '" + T + "=>" + N + "' is inaccessible outside class");
                            else
                                T = ST[Temp[0]].Link.Type[Temp[1]];
                        }
                    }

                    if (Array(ref T, ref N))
                    {
                        if (Inc_Dec(ref T, ref N))
                        {
                            return true;
                        }
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                return false;
            }

            else if (Inc_Dec(ref T, ref N))
            {
                return true;
            }

            return false;
        }


        static bool Var4(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "self")
            {
                i++;
                if (i < CP.Count && CP[i] == "=>")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "id")
                    {
                        N = VP[i];
                        if (!FirstTime)
                        {
                            T = SelfLookUp(N);
                            if (T == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");
                        }
                        i++;
                        if (Array(ref T, ref N))
                        {
                            return true;
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$=> expected");
                return false;
            }

            else if (i < CP.Count && CP[i] == "id")
            {
                N = VP[i];
                if (!FirstTime)
                {
                    T = LookUp(N);
                    if (T == null)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");
                }
                i++;
                if (Array(ref T, ref N))
                {
                    if (Var2(ref T, ref N))
                    {
                        return true;
                    }
                }
            }
            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Identifier or Self expected");

            return false;
        }


        static bool S_St()
        {
            if (i < CP.Count && CP[i] == "cease_chase")
            {
                i++;
                if (OpenLoops == 0)
                    Errors.Add("Semantic$" + Line[i - 1] + "$No open loop to cease or chase");
                if (i < CP.Count && CP[i] == ";")
                {
                    i++;
                    return true;
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
                return false;
            }

            else if (i < CP.Count && CP[i] == "self")
            {
                i++;
                if (i < CP.Count && CP[i] == "=>")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "id")
                    {
                        string N = VP[i];
                        i++;

                        if (S_St3(ref N))
                        {
                            if (i < CP.Count && CP[i] == ";")
                            {
                                i++;
                                return true;
                            }
                            else
                                Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
                        }
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$=> expected");
                return false;
            }

            else if (i < CP.Count && CP[i] == "id")
            {
                string N = VP[i];
                i++;
                if (S_St4(ref N))
                {
                    return true;
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "dt")
            {
                string T = VP[i];
                i++;
                if (All_Dec2(ref T, "", false))
                {
                    return true;
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "inc_dec")
            {
                string Op = VP[i];
                i++;
                string T = "";
                string N = "";
                if (Var4(ref T, ref N))
                {
                    if (!FirstTime && T != null && !Comp(T, "inc_dec"))
                        Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + Op + "' cannot be applied to operand of type '" + T + "'");

                    if (S_St8(ref T, ref N))
                    {
                        if (i < CP.Count && CP[i] == ";")
                        {
                            i++;
                            return true;
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Assignment operator expected");
                }
            }

            else if (Loop())
            {
                return true;
            }

            else if (Repeat())
            {
                return true;
            }

            else if (Ret_St())
            {
                return true;
            }

            else if (If())
            {
                return true;
            }

            return false;
        }


        static bool S_St2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "assign_op")
            {
                string Op = VP[i];
                i++;
                string T2 = "";
                string N2 = "";
                if (Exp(ref T2, ref N2))
                {
                    if (!FirstTime && T != null && T2 != null)
                    {
                        if (Comp(T, T2, Op) == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + Op + "' cannot be applied to type '" + T + "' and '" + T2 + "'");
                    }

                    IC.Gen(N + Op + N2);

                    return true;
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "=")
            {
                i++;
                if (S_St10(ref T, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool S_St3(ref string N)
        {
            if (i < CP.Count && CP[i] == "(")
            {
                i++;
                string PL = "";
                if (Para_Call(ref PL))
                {
                    if (i < CP.Count && CP[i] == ")")
                    {
                        i++;
                        if (!FirstTime)
                        {
                            string T = FLookUp(N, PL);
                            if (T == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N + "'(" + PL + ")' does not exist");

                            else if (T != "void")
                                Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N + "'(" + PL + ")' does not returns void");
                        }
                        if (PL != null && PL == "void")
                            IC.Gen("call " + ClassName + "_" + N + "_" + PL + ", 0");
                        else if (PL != null && PL != "void")
                        {
                            string Temp = IC.GetTemp();
                            IC.Gen(Temp + " = call " + ClassName + "_" + N + "_" + PL + ", " + (PL.ToCharArray().Count(x => x == ',') + 1));
                            N = Temp;
                        }

                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                }
                return false;
            }

            string T2 = null;
            if (!FirstTime)
            {
                T2 = SelfLookUp(N);
                if (T2 == null)
                    Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");
            }
            if (Array(ref T2, ref N))
            {
                if (S_St9(ref T2, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool S_St4(ref string N)
        {
            if (i < CP.Count && CP[i] == "id")
            {
                string N2 = VP[i];

                i++;

                if (!FirstTime)
                {
                    if (!CDLookUp(N))
                        Errors.Add("Semantic$" + Line[i - 1] + "$Undeclared class - " + N);

                    if (!DLookUp(N2, false))
                        Insert3(N2, N);
                    else
                        Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N2);
                }
                if (Ob_Init(ref N, ref N2))
                {
                    if (Ob_A_Dec2(ref N, "", false))
                    {
                        return true;
                    }
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "(")
            {
                i++;
                string PL = "";
                if (Para_Call(ref PL))
                {
                    if (i < CP.Count && CP[i] == ")")
                    {
                        i++;
                        if (!FirstTime)
                        {
                            string T = FLookUp(N, PL);
                            if (T == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N + "'(" + PL + ")' does not exist");

                            else if (T != "void")
                                Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N + "'(" + PL + ")' does not returns void");
                        }
                        if (PL != null && PL == "void")
                            IC.Gen("call " + ClassName + "_" + N + "_" + PL + ", 0");
                        else if (PL != null && PL != "void")
                        {
                            string Temp = IC.GetTemp();
                            IC.Gen(Temp + " = call " + ClassName + "_" + N + "_" + PL + ", " + (PL.ToCharArray().Count(x => x == ',') + 1));
                            N = Temp;
                        }

                        if (i < CP.Count && CP[i] == ";")
                        {
                            i++;
                            return true;
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                }
                return false;
            }

            else if (i < CP.Count && CP[i] == "[")
            {
                i++;
                if (S_St6(ref N))
                {
                    return true;
                }
                return false;
            }

            string T2 = null;
            if (!FirstTime)
            {
                T2 = LookUp(N);
                if (T2 == null)
                    Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");
            }
            if (i < CP.Count && CP[i] == "=>")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    string N2 = VP[i];
                    i++;
                    if (S_St5(ref T2, ref N2))
                    {
                        if (i < CP.Count && CP[i] == ";")
                        {
                            i++;
                            return true;
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                return false;
            }

            if (S_St9(ref T2, ref N))
            {
                if (i < CP.Count && CP[i] == ";")
                {
                    i++;
                    return true;
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
            }

            return false;
        }


        static bool S_St5(ref string N, ref string N2)
        {
            if (i < CP.Count && CP[i] == "(")
            {
                i++;
                string PL = "";
                if (Para_Call(ref PL))
                {
                    if (i < CP.Count && CP[i] == ")")
                    {
                        i++;
                        string T2 = null;
                        if (!FirstTime)
                        {
                            int[] Temp = AccessLookUp(N, N2, PL);
                            if (Temp == null)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N2 + "'(" + PL + ")' does not exist in class '" + N + "'");
                            else
                            {
                                if (ST[Temp[0]].Link.AM[Temp[1]] == "unknown")
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Function  '" + N + "=>" + N2 + "(" + PL + ")' is inaccessible outside class");
                                else
                                {
                                    T2 = ST[Temp[0]].Link.Type[Temp[1]];
                                    if (T2.Split('>')[1] != "void")
                                        Errors.Add("Semantic$" + Line[i - 1] + "$Function '" + N + "=>" + N2 + "(" + PL + ")' does not returns void");
                                }
                            }
                        }
                        if (PL != null && PL == "void")
                            IC.Gen("call " + N + "_" + N2 + "_" + PL + ", 0");
                        else if (PL != null && PL != "void")
                        {
                            string Temp = IC.GetTemp();
                            IC.Gen(Temp + " = call " + N + "_" + N2 + "_" + PL + ", " + (PL.ToCharArray().Count(x => x == ',') + 1));
                            N2 = Temp;
                        }

                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                }
                return false;
            }

            string T = null;
            if (!FirstTime)
            {
                int[] Temp = AccessLookUp(N, N2);
                if (Temp == null)
                    Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N2 + "' does not exist in class '" + N + "'");
                else
                {
                    if (ST[Temp[0]].Link.AM[Temp[1]] == "unknown")
                        Errors.Add("Semantic$" + Line[i - 1] + "$Variable  '" + N + "=>" + N2 + "' is inaccessible outside class");
                    else
                        T = ST[Temp[0]].Link.Type[Temp[1]];
                }
            }

            if (Array(ref T, ref N2))
            {
                if (S_St9(ref T, ref N2))
                {
                    return true;
                }
            }

            return false;
        }


        static bool S_St6(ref string N)
        {
            if (i < CP.Count && CP[i] == "]")
            {
                i++;
                N += "[]";
                if (i < CP.Count && CP[i] == "id")
                {
                    string N2 = VP[i];
                    i++;
                    if (!FirstTime)
                    {
                        if (!CDLookUp(N))
                            Errors.Add("Semantic$" + Line[i - 1] + "$Undeclared class - " + N);

                        if (!DLookUp(N2, false))
                            Insert3(N2, N);
                        else
                            Errors.Add("Semantic$" + Line[i - 1] + "$Redeclared variable - " + N2);
                    }
                    if (Ob_A_Init(ref N, ref N2))
                    {
                        if (Ob_A_Dec2(ref N, "", false))
                        {
                            return true;
                        }
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");

                return false;
            }

            string T2 = "";
            string N3 = "";
            if (Exp(ref T2, ref N3))
            {
                string T3 = null;
                if (!FirstTime)
                {
                    T3 = LookUp(N);
                    if (T3 == null)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Variable '" + N + "' does not exist");

                    else if (!T3.EndsWith("[]"))
                        Errors.Add("Semantic$" + Line[i - 1] + "$Variable is not array");

                    if (!FirstTime && T2 != null && T2 != "int")
                        Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to 'int'");
                }
                N += "[" + N3 + "]";

                if (i < CP.Count && CP[i] == "]")
                {
                    if (!FirstTime && T3 != null)
                        T3 = T3.Replace("[]", "");

                    i++;
                    if (S_St7(ref T3, ref N))
                    {
                        return true;
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$] expected");
                return false;
            }

            return false;
        }


        static bool S_St7(ref string N, ref string N2)
        {
            if (i < CP.Count && CP[i] == "=>")
            {
                i++;
                if (i < CP.Count && CP[i] == "id")
                {
                    N2 = VP[i];
                    i++;

                    if (S_St5(ref N, ref N2))
                    {
                        if (i < CP.Count && CP[i] == ";")
                        {
                            i++;
                            return true;
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
                    }
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                return false;
            }

            else if (S_St9(ref N, ref N2))
            {
                if (i < CP.Count && CP[i] == ";")
                {
                    i++;
                    return true;
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$; expected");
            }

            return false;
        }


        static bool S_St8(ref string T, ref string N)
        {
            if (S_St2(ref T, ref N))
            {
                return true;
            }
            return true;
        }


        static bool S_St9(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "inc_dec")
            {
                if (!FirstTime && T != null && !Comp(T, "inc_dec"))
                    Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + VP[i] + "' cannot be applied to operand of type '" + T + "'");

                string T1 = IC.GetTemp();
                IC.Gen(T1 + "=" + N + VP[i][0] + 1);
                IC.Gen(N + "=" + T1);

                i++;

                if (S_St8(ref T, ref N))
                {
                    return true;
                }
            }

            else if (S_St2(ref T, ref N))
            {
                return true;
            }

            return false;
        }


        static bool S_St10(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "create")
            {
                i++;
                if (S_St11(ref T))
                {
                    return true;
                }

                return false;
            }

            string T2 = "";
            string N2 = "";
            if (Exp(ref T2, ref N2))
            {
                if (!FirstTime && T != null && T2 != null)
                {
                    if (Comp(T, T2, "=") == null)
                        Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");
                }

                IC.Gen(N + "=" + N2);

                return true;
            }
            return false;
        }


        static bool S_St11(ref string T)
        {
            if (i < CP.Count && CP[i] == "object")
            {
                i++;
                if (i < CP.Count && CP[i] == "(")
                {
                    i++;
                    if (i < CP.Count && CP[i] == "id")
                    {
                        string T2 = VP[i];
                        i++;

                        if (!FirstTime)
                        {
                            if (!CDLookUp(T2))
                                Errors.Add("Semantic$" + Line[i - 1] + "$Undeclared class '" + T2 + "'");

                            if (!FirstTime && T != null && T2 != null && T != T2)
                                Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");
                        }
                        if (i < CP.Count && CP[i] == ")")
                        {
                            i++;
                            return true;
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
            }

            else if (i < CP.Count && CP[i] == "array")
            {
                i++;
                if (i < CP.Count && CP[i] == "(")
                {
                    i++;
                    string T2 = "";
                    if (DT_ID(ref T2))
                    {
                        T2 += "[]";

                        if (!FirstTime && T == null && T2 != null && T != T2)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T2 + "' to '" + T + "'");

                        if (i < CP.Count && CP[i] == ",")
                        {
                            i++;
                            string T3 = "";
                            string Temp = "";
                            if (Exp(ref T3, ref Temp))
                            {
                                if (!FirstTime && T3 != null && T3 != "int")
                                    Errors.Add("Semantic$" + Line[i - 1] + "$Cannot convert type '" + T3 + "' to 'int'");

                                if (i < CP.Count && CP[i] == ")")
                                {
                                    i++;
                                    return true;
                                }
                                else
                                    Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                            }
                        }
                        else
                            Errors.Add("Syntax$" + Line[i - 1] + "$, expected");
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$Identifier expected");
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$( expected");
            }

            else
                Errors.Add("Syntax$" + Line[i - 1] + "$Keyword object or array expected");

            return false;
        }


        static bool M_St()
        {
            if (i < CP.Count && (CP[i] == "loop" ||
            CP[i] == "repeat" ||
            CP[i] == "ret" ||
            CP[i] == "cease_chase" ||
            CP[i] == "if" ||
            CP[i] == "self" ||
            CP[i] == "id" ||
            CP[i] == "dt" ||
            CP[i] == "inc_dec"))
            {
                if (S_St())
                {
                    if (M_St())
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        static bool Exp(ref string T, ref string N)
        {
            if (Exp_And(ref T, ref N))
            {
                if (Exp2(ref T, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Exp2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "||")
            {
                string Op = VP[i];
                i++;
                string T2 = "";
                string N2 = "";
                if (Exp_And(ref T2, ref N2))
                {
                    if (!FirstTime && T != null && T2 != null)
                    {
                        string Temp1 = Comp(T, T2, Op);
                        if (!FirstTime && Temp1 == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + Op + "' cannot be applied to '" + T + "' and '" + T2 + "'");
                        T = Temp1;
                    }

                    string Temp = IC.GetTemp();
                    IC.Gen(Temp + "=" + N + Op + N2);

                    if (Exp2(ref T, ref Temp))
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        static bool Exp_And(ref string T, ref string N)
        {
            if (Exp_Rel(ref T, ref N))
            {
                if (Exp_And2(ref T, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Exp_And2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "&&")
            {
                string Op = VP[i];
                i++;
                string T2 = "";
                string N2 = "";
                if (Exp_Rel(ref T2, ref N2))
                {
                    if (!FirstTime && T != null && T2 != null)
                    {
                        string Temp1 = Comp(T, T2, Op);
                        if (!FirstTime && Temp1 == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + Op + "' cannot be applied to '" + T + "' and '" + T2 + "'");
                        T = Temp1;
                    }

                    string Temp = IC.GetTemp();
                    IC.Gen(Temp + "=" + N + Op + N2);
                    N = Temp;

                    if (Exp_And2(ref T, ref N))
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        static bool Exp_Rel(ref string T, ref string N)
        {
            if (Exp_PM(ref T, ref N))
            {
                if (Exp_Rel2(ref T, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Exp_Rel2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "rel_op")
            {
                string Op = VP[i];
                i++;
                string T2 = "";
                string N2 = "";
                if (Exp_PM(ref T2, ref N2))
                {
                    if (!FirstTime && T != null && T2 != null)
                    {
                        string Temp1 = Comp(T, T2, Op);
                        if (!FirstTime && Temp1 == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + Op + "' cannot be applied to '" + T + "' and '" + T2 + "'");
                        T = Temp1;
                    }

                    string Temp = IC.GetTemp();
                    IC.Gen(Temp + "=" + N + Op + N2);
                    N = Temp;

                    if (Exp_Rel2(ref T, ref N))
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        static bool Exp_PM(ref string T, ref string N)
        {
            if (Exp_MDM(ref T, ref N))
            {
                if (Exp_PM2(ref T, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Exp_PM2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "pm")
            {
                string Op = VP[i];
                i++;
                string T2 = "";
                string N2 = "";
                if (Exp_MDM(ref T2, ref N2))
                {
                    if (!FirstTime && T != null && T2 != null)
                    {
                        string Temp1 = Comp(T, T2, Op);
                        if (!FirstTime && Temp1 == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + Op + "' cannot be applied to '" + T + "' and '" + T2 + "'");
                        T = Temp1;
                    }

                    string Temp = IC.GetTemp();
                    IC.Gen(Temp + "=" + N + Op + N2);
                    N = Temp;

                    if (Exp_PM2(ref T, ref N))
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        static bool Exp_MDM(ref string T, ref string N)
        {
            if (Operand(ref T, ref N))
            {
                if (Exp_MDM2(ref T, ref N))
                {
                    return true;
                }
            }

            return false;
        }


        static bool Exp_MDM2(ref string T, ref string N)
        {
            if (i < CP.Count && CP[i] == "mdm")
            {
                string Op = VP[i];
                i++;
                string T2 = "";
                string N2 = "";
                if (Operand(ref T2, ref N2))
                {
                    if (!FirstTime && T != null && T2 != null)
                    {
                        string Temp1 = Comp(T, T2, Op);
                        if (!FirstTime && Temp1 == null)
                            Errors.Add("Semantic$" + Line[i - 1] + "$Operator '" + Op + "' cannot be applied to '" + T + "' and '" + T2 + "'");
                        T = Temp1;
                    }

                    string Temp = IC.GetTemp();
                    IC.Gen(Temp + "=" + N + Op + N2);
                    N = Temp;

                    if (Exp_MDM2(ref T, ref N))
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        static bool Operand(ref string T, ref string N)
        {
            if (Var_Func(ref T, ref N))
            {
                return true;
            }

            else if (Const(ref T, ref N))
            {
                return true;
            }

            else if (i < CP.Count && CP[i] == "(")
            {
                i++;
                if (Exp(ref T, ref N))
                {
                    if (i < CP.Count && CP[i] == ")")
                    {
                        i++;
                        return true;
                    }
                    else
                        Errors.Add("Syntax$" + Line[i - 1] + "$) expected");
                }

                return false;
            }

            else if (i < CP.Count && CP[i] == "!")
            {
                i++;
                if (Operand(ref T, ref N))
                {
                    if (!FirstTime && T != null && !Comp(T, "!"))
                        Errors.Add("Semantic$" + Line[i - 1] + "$Operator '!' cannot be applied to operand of type '" + T + "'");

                    IC.Gen(N + "=!" + N);

                    return true;
                }
                else
                    Errors.Add("Syntax$" + Line[i - 1] + "$Operand expected");
            }

            return false;
        }


        static bool Const(ref string T, ref string N)
        {
            if (i < CP.Count && (CP[i] == "int" ||
            CP[i] == "real" ||
            CP[i] == "string" ||
            CP[i] == "bool" ||
            CP[i] == "char"))
            {
                T = CP[i];
                N = VP[i];
                i++;
                return true;
            }

            return false;
        }

    }
}