using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project
{
    class STL3
    {
        public List<string> Name;
        public List<string> Type;
        public List<int> Scope;

        public STL3()
        {
            Name = new List<string>();
            Type = new List<string>();
            Scope = new List<int>();
        }

        public void Add(string N, string T,int S)
        {
            Name.Add(N);
            Type.Add(T);
            Scope.Add(S);
        }
    }
}
