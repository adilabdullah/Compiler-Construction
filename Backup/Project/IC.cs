using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Project
{
    class IC
    {
        static int TI, LI;
        static StreamWriter Sr;
        public static bool Enable;

        public static void InitIC()
        {
            Enable = true;
            TI = 1;
            LI = 1;
            Sr = new StreamWriter("IC.txt");
        }

        public static void DestIC()
        {
            Sr.Close();
        }

        public static string GetTemp()
        {
            if (!Enable)
                return "";
            return "T" + (TI++);
        }

        public static string GetLabel()
        {
            if (!Enable)
                return "";
            return "L" + (LI++);
        }

        public static void Gen(string S)
        {
            if (Enable)
                Sr.WriteLine(S);
        }
    }
}
