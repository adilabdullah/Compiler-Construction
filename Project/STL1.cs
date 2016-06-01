using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project
{
    class STL1
    {
        public string Name;
        public STL2 Link;

        public STL1(string N)
        {
            Name = N;
            Link = new STL2();
        }
    }
}
