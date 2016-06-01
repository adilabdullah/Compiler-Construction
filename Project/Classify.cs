using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project
{
    class Classify
    {
        public static string Keyword(string Word)
        {
            string[] Classes = {   "if,if",
                                    "eif,eif",
                                    "else,else",
                                    "to,to",
                                    "by,by",
                                    "func,func",
                                    "ret,ret",
                                    "loop,loop",
                                    "repeat,repeat",
                                    "until,until",
                                    "cease,cease_chase",
                                    "chase,cease_chase",
                                    "create,create",
                                    "class,class",
                                    "self,self",
                                    "void,void",
                                    "int,dt",
                                    "real,dt",
                                    "string,dt",
                                    "bool,dt",
                                    "array,array",
                                    "object,object",
                                    "known,am",
                                    "unknown,am"};

            return Classes.ToList().Find(x => x.StartsWith(Word + ",")).Replace(Word + ",","");

        }
    }
}
