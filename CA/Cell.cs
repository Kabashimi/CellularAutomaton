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
        public double energy;
        public bool recristalized;

        public Cell()
        {
            grainID = -1;
            energy = 0;
            recristalized = false;
        }

        public Cell(Cell cell)
        {
            this.grainID = cell.grainID;
            this.energy = cell.energy;
            recristalized = cell.recristalized;
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
