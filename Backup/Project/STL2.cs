using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project
{
    class STL2
    {
        public List<string> Name;
        public List<string> Type;
        public List<string> AM;
        public List<int> Scope;
        public List<STL3> Link;

        public STL2()
        {
            Name = new List<string>();
            Type = new List<string>();
            AM = new List<string>();
            Scope = new List<int>();
            Link = new List<STL3>();
        }

        public void Add(string N, string T, string A,int S)
        {
            Name.Add(N);
            Type.Add(T);
            AM.Add(A);
            Scope.Add(S);
            Link.Add(new STL3());
        }
    }
}
