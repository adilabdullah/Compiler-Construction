using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project
{
    class Semantic
    {
        public static int Scope;
        public static Stack<int> ScopeStack;
        public static List<STL1> ST;
        public static int ST1Index;
        public static int ST2Index;

        public static void InitSementic()
        {
            ST = new List<STL1>();
            ScopeStack = new Stack<int>();
            Scope = -1;
        }

        public static void CreateScope()
        {
            Scope++;
            ScopeStack.Push(Scope);
        }

        public static void DestroyScope()
        {
            ScopeStack.Pop();
        }

        public static void Insert1(string N)
        {
            ST.Add(new STL1(N));
        }

        public static void Insert2(string N, string T, string AM)
        {
            if (AM == "")
                AM = "unknown";
            List<int> Temp = ScopeStack.ToList();
            int sc = Temp[Temp.Count - 1];
            ST[ST1Index].Link.Add(N, T, AM, sc);
        }

        public static void Insert3(string N, string T)
        {
            int c = ST1Index;
            int sc = ScopeStack.Peek();
            int c1 = ST2Index;
            if (c1 == -1)
                c1 = 0;

            ST[ST1Index].Link.Link[c1].Add(N, T, sc);
        }

        public static bool CDLookUp(string N)
        {
            if (N.EndsWith("[]"))
                N = N.Replace("[]", "");
            for (int i = 0; i < ST.Count; i++)
            {
                if (ST[i].Name == N)
                    return true;
            }
            return false;
        }

        public static bool FDLookUp(string N, string PL)
        {
            for (int i = 0; i < ST[ST1Index].Link.Name.Count; i++)
            {
                if (ST[ST1Index].Link.Name[i] == N && ST[ST1Index].Link.Type[i].Split('>')[0] == PL)
                    return true;
            }
            return false;
        }

        public static bool DLookUp(string N,bool CL)
        {
            int c = ST1Index;
            int c2 = ST2Index;
            if (c2 == -1)
                c2 = 0;

            if (ST[ST1Index].Link.Link.Count <= c2)
                return false;

            for (int i = 0; i < ST[ST1Index].Link.Link[c2].Name.Count; i++)
            {
                if (ST[ST1Index].Link.Link[ST2Index].Name[i] == N && ST[ST1Index].Link.Link[ST2Index].Scope[i] == ScopeStack.Peek())
                    return true;
            }
            if (!CL)
                return false;

            for (int i = 0; i < ST[ST1Index].Link.Name.Count; i++)
            {
                if (ST[ST1Index].Link.Name[i] == N && !ST[ST1Index].Link.Type[i].Contains('>'))
                    return true;
            }
            return false;
        }

        public static bool Comp(string T, string Op)
        {
            if (T == null)
                return false;

            if ((T == "int" || T == "double") && Op == "inc_dec")
                return true;

            if (T == "bool" && Op == "!")
                return true;
            return false;
        }

        public static string Comp(string T1, string T2, string Op)
        {
            if (T1 == null || T2 == null)
                return null;

            if (T1 == "int" && T2 == "char" && Op == "=")
                return "int";

            else if (T1 == T2 && Op == "=")
                return T1;

            if (T1 == "string" && Op == "+=")
                return "string";

            if ((T1 == T2) && (T1 == "int" || T1 == "real") && (Op == "+=" || Op == "-=" || Op == "*=" || Op == "/=" || Op == "%="))
                return T2;

            else if ((T1 == T2 && T1 == "string") && Op == "+")
                return T1;

            else if ((T1 == T2) && (T1 == "int" || T1 == "real") && (Op == "+" || Op == "-" || Op == "*" || Op == "/" || Op == "%"))
                return T1;

            else if (T1 == "int" && T2 == "char" && (Op == "+" || Op == "-" || Op == "*" || Op == "/" || Op == "%"))
                return "int";

            if (T1 == "int" && T2 == "char" && (Op == "+=" || Op == "-=" || Op == "*=" || Op == "/=" || Op == "%="))
                return "int";

            else if ((T1 == T2) && T1 == "char" && (Op == "+" || Op == "-" || Op == "*" || Op == "/" || Op == "%"))
                return "char";

            else if ((T1 == "string" || T2 == "string") && (Op == "+"))
                return "string";

            else if ((T1 == T2 && T1 == "bool") && (Op == "||" || Op == "&&"))
                return "bool";

            else if (((T1 == "int" && T2 == "real") || (T1 == "real" && T2 == "int")) && (Op == "+" || Op == "-" || Op == "*" || Op == "/" || Op == "%"))
                return "real";

            else if ((T1 == "int" || T1 == "real") && (T2 == "int" || T2 == "real") && (Op == "<" || Op == ">" || Op == ">=" || Op == "<="))
                return "bool";

            else if ((T1 == "int" || T1 == "char") && (T2 == "char" || T2 == "int") && (Op == "<" || Op == ">" || Op == ">=" || Op == "<="))
                return "bool";

            else if ((T1 == T2) && (Op == "==" || Op == "!="))
                return "bool";

            return null;
        }

        public static string SelfLookUp(string N)
        {
            if (N == null)
                return null;

            for (int i = 0; i < ST[ST1Index].Link.Name.Count; i++)
            {
                if (ST[ST1Index].Link.Name[i] == N)
                    //return new int[] { ST1Index, i };
                    return ST[ST1Index].Link.Type[i];
            }
            return null;
        }

        public static string SelfLookUp(string N, string PL)
        {
            if (N == null)
                return null;

            for (int i = 0; i < ST[ST1Index].Link.Name.Count; i++)
            {
                if (ST[ST1Index].Link.Name[i] == N && ST[ST1Index].Link.Type[i].Split('>')[0] == PL)
                    return ST[ST1Index].Link.Type[i].Split('>')[1];
            }
            return null;
        }

        public static bool RetLookUp(string T)
        {
            if (T == null)
                return false;

            for (int i = 0; i < ST[ST1Index].Link.Type.Count; i++)
            {
                if (ST[ST1Index].Link.Type[i].Contains('>'))
                {
                    if (ST[ST1Index].Link.Type[i].Split('>')[1] == T)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string LookUp(string N)
        {
            if (N == null)
                return null;

            for (int i = 0; i < ST[ST1Index].Link.Link[ST2Index].Name.Count; i++)
            {
                if (ST[ST1Index].Link.Link[ST2Index].Name[i] == N)
                    //return new int[] { 1, ST1Index, ST2Index, i };
                    return ST[ST1Index].Link.Link[ST2Index].Type[i];
            }
            for (int i = 0; i < ST[ST1Index].Link.Name.Count; i++)
            {
                if (ST[ST1Index].Link.Name[i] == N && !ST[ST1Index].Link.Type[i].Contains('>'))
                    //return new int[] { 2,ST1Index, i };
                    return ST[ST1Index].Link.Type[i];
            }
            return null;
        }

        public static string FLookUp(string N, string PL)
        {
            if (N == null)
                return null;

            for (int i = 0; i < ST[ST1Index].Link.Name.Count; i++)
            {
                if (ST[ST1Index].Link.Name[i] == N && ST[ST1Index].Link.Type[i].Split('>')[0] == PL)
                    return ST[ST1Index].Link.Type[i].Split('>')[1];
            }
            return null;
        }

        public static int[] AccessLookUp(string CN, string VN)
        {
            if (CN == null || VN == null)
                return null;

            CN = CN.Replace("[]", "");

            for (int i = 0; i < ST.Count; i++)
            {
                if (ST[i].Name == CN)
                {
                    for (int j = 0; j < ST[i].Link.Name.Count; j++)
                    {
                        if (ST[i].Link.Name[j] == VN && !ST[i].Link.Type[j].Contains('>'))
                            return new int[] { i, j };
                    }
                }
            }
            return null;
        }

        public static int[] AccessLookUp(string CN, string VN, string PL)
        {
            if (CN == null || VN == null)
                return null;

            CN = CN.Replace("[]", "");

            for (int i = 0; i < ST.Count; i++)
            {
                if (ST[i].Name == CN)
                {
                    for (int j = 0; j < ST[i].Link.Name.Count; j++)
                    {
                        if (ST[i].Link.Name[j] == VN && ST[i].Link.Type[j].Split('>')[0] == PL)
                            return new int[] { i, j };
                    }
                }
            }
            return null;
        }

        public static int HeadLookUp()
        {
            int c = 0;
            for (int i = 0; i < ST.Count; i++)
            {
                for (int j = 0; j < ST[i].Link.Name.Count; j++)
                {
                    if (ST[i].Link.Name[j] == "head" && ST[i].Link.Type[j].Contains('>'))
                        c++;
                }
            }
            return c;
        }
    }
}