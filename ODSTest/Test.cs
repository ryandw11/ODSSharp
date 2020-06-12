using System;
using System.Collections.Generic;
using System.Text;
using ODS.Serializer;

namespace ODSTest
{
    public class Test
    {
        [ODSSerializeable]
        public int id;

        [ODSSerializeable]
        public string name;
    }
}
