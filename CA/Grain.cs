using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CA
{
    class Grain
    {
        public int ID;
        public System.Drawing.Color color;
        public bool recristalized;

        public Grain(int id, Color color, bool rec = false)
        {
            this.color = color;
            ID = id;
            recristalized = rec;
        }
    }
}
