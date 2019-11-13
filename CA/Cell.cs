using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA
{
    class Cell:IComparable
    {
        public int grainID;

        public Cell()
        {
            grainID = -1;
        }

        public Cell(Cell cell)
        {
            this.grainID = cell.grainID;
        }

        public Cell(int grainID)
        {
            this.grainID = grainID;
        }

        int IComparable.CompareTo(object obj)
        {
            Cell c = (Cell)obj;
            return this.grainID.CompareTo(c.grainID);

        }

    }
}
