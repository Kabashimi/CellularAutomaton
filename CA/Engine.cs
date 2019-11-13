using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA
{
    class Engine
    {
        public System.Drawing.Bitmap bitmap;
        List<List<Cell>> previousStep;
        List<List<Cell>> nextStep;
        List<Grain> grains;
        internal bool growthDone;
        public const int spaceDim = 300;
        public int neighborhoodType;
        public int probability;
        int emptyCells;
        List<Point> edgePoints;
        public List<int> selectedGrains;

        public Engine()
        {
            probability = 100;
            neighborhoodType = 0;
            emptyCells = spaceDim * spaceDim;
            growthDone = false;
            bitmap = new System.Drawing.Bitmap(spaceDim, spaceDim);
            grains = new List<Grain>();
            selectedGrains = new List<int>();
            previousStep = new List<List<Cell>>();
            for (int i = 0; i < spaceDim; i++)
            {
                previousStep.Add(new List<Cell>());
                for (int j = 0; j < spaceDim; j++)
                {
                    previousStep[i].Add(new Cell());
                }
            }
            //nextStep = new List<List<Cell>>(previousStep);
            nextStep = new List<List<Cell>>();
            for (int i = 0; i < spaceDim; i++)
            {
                nextStep.Add(new List<Cell>());
                for (int j = 0; j < spaceDim; j++)
                {
                    nextStep[i].Add(new Cell());
                }
            }

        }

        internal float CalculatePercentage(int grainID)
        {
            float sum = 0;
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    if (previousStep[i][j].grainID == grainID)
                        sum++;
                }
            }
            float perc = sum / (spaceDim * spaceDim);
            return perc * 100;
        }

        public bool AddSelectedGrain(int x, int y)
        {
            int grainID = previousStep[x][y].grainID;
            if (grainID >= 0)
            {
                if (!selectedGrains.Contains(grainID))
                {
                    selectedGrains.Add(grainID);
                    return true;
                }
            }
            return false;
        }

        public void DrawBitmap()
        {
            for (int i = 0; i < previousStep.Count; i++)
            {
                for (int j = 0; j < previousStep[i].Count; j++)
                {
                    if (previousStep[i][j].grainID >= 0)
                    {
                        bitmap.SetPixel(i, j, grains[previousStep[i][j].grainID].color);
                    }
                    else if (previousStep[i][j].grainID == -2)
                    {
                        bitmap.SetPixel(i, j, System.Drawing.Color.Black);
                    }
                    else if (previousStep[i][j].grainID == -3)
                    {
                        bitmap.SetPixel(i, j, System.Drawing.Color.Magenta);
                    }
                    else
                    {
                        bitmap.SetPixel(i, j, System.Drawing.Color.White);
                    }
                }
            }
        }

        private int getEmptyCells()
        {
            int cells = 0;
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    if(previousStep[i][j].grainID == -1)
                    {
                        cells++;
                    }
                }
            }
            return cells;
        }

        public void SetRandomNucleons(int nucleonsNumber)
        {
            Random rnd = new Random();
            int x;
            int y;
            bool done;
            for (int i = 0; i < nucleonsNumber; i++)
            {
                do
                {
                    done = false;
                    x = rnd.Next(0, spaceDim);
                    y = rnd.Next(0, spaceDim);
                    if (previousStep[x][y].grainID == -1)
                    {
                        done = true;
                        System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));
                        grains.Add(new Grain(grains.Count, randomColor));
                        previousStep[x][y].grainID = grains[grains.Count - 1].ID;
                        emptyCells--;
                        var dupa = grains[grains.Count - 1].color;
                        bitmap.SetPixel(x, y, grains[grains.Count - 1].color);
                    }
                } while (!done);
            }
        }

        internal void Grow()
        {

            if (neighborhoodType == 0)
            {
                Console.WriteLine("loop start");
                for (int i = 0; i < spaceDim; i++)
                {
                    for (int j = 0; j < spaceDim; j++)
                    {
                        int prevGrainID = previousStep[i][j].grainID;
                        if (prevGrainID == -1)
                        {
                            int newGrainID = -1;
                            var tmp = new List<Cell>();
                            var neighbors = getVonNeumannNeighbors(i, j).Where(c => c.grainID != -1 && c.grainID != -2).GroupBy(c => c.grainID).OrderByDescending(g => g.Count()).ToList();

                            if (neighbors.Count() > 0)
                            {
                                newGrainID = neighbors[0].ToList()[0].grainID;
                            }
                            if (newGrainID >= 0)
                            {
                                nextStep[i][j].grainID = newGrainID;
                                emptyCells--;
                                bitmap.SetPixel(i, j, grains[newGrainID].color);
                            }
                        }
                        else if (prevGrainID == -2)
                        {
                            nextStep[i][j].grainID = -2;
                        }
                        else if (prevGrainID == -3)
                        {
                            nextStep[i][j].grainID = -3;
                        }
                    }
                }
            }
            if (neighborhoodType == 1)
            {
                Random rnd = new Random();
                int randomNumber;
                for (int i = 0; i < spaceDim; i++)
                {
                    for (int j = 0; j < spaceDim; j++)
                    {
                        int prevGrainID = previousStep[i][j].grainID;
                        if (prevGrainID == -1)
                        {
                            int newGrainID = -1;
                            List<Cell> neighbors = getMooreNeighbors(i, j);
                            neighbors.Sort();
                            List<int> mostCommon = getMostCommon(neighbors);
                            if (mostCommon[0] == 8 && mostCommon[1] == -1)
                            {
                                continue;
                            }

                            if (mostCommon[0] >= 5)
                            {
                                if (mostCommon[1] > -1)
                                {
                                    nextStep[i][j].grainID = mostCommon[1];
                                    emptyCells--;
                                    bitmap.SetPixel(i, j, grains[mostCommon[1]].color);
                                    continue;
                                }
                            }
                            neighbors = getNearestMooreNeighbors(i, j);
                            neighbors.Sort();
                            mostCommon = getMostCommon(neighbors);
                            if (mostCommon[0] >= 3)
                            {
                                if (mostCommon[1] > -1)
                                {
                                    nextStep[i][j].grainID = mostCommon[1];
                                    emptyCells--;
                                    bitmap.SetPixel(i, j, grains[mostCommon[1]].color);
                                    continue;
                                }
                            }
                            neighbors = getFurtherMooreNeighbors(i, j);
                            neighbors.Sort();
                            mostCommon = getMostCommon(neighbors);
                            if (mostCommon[0] >= 3)
                            {
                                if (mostCommon[1] > -1)
                                {
                                    nextStep[i][j].grainID = mostCommon[1];
                                    emptyCells--;
                                    bitmap.SetPixel(i, j, grains[mostCommon[1]].color);
                                    continue;
                                }
                            }
                            randomNumber = rnd.Next(0, 100);
                            if (randomNumber > probability)
                            {
                                continue;
                            }

                            neighbors = getMooreNeighbors(i, j);
                            neighbors.Sort();
                            mostCommon = getMostCommonValid(neighbors);
                            if (mostCommon[0] > 0)
                            {
                                nextStep[i][j].grainID = mostCommon[1];
                                emptyCells--;
                                bitmap.SetPixel(i, j, grains[mostCommon[1]].color);
                            }
                        }
                        else if (prevGrainID == -2)
                        {
                            nextStep[i][j].grainID = -2;
                        }
                        else if (prevGrainID == -3)
                        {
                            nextStep[i][j].grainID = -3;
                        }
                    }
                }
            }

            Console.WriteLine("Growth iteration done");
            copyList();

            //Console.WriteLine(emptyCells);
            emptyCells = getEmptyCells();
            if (emptyCells <= 0)
            {
                growthDone = true;
                edgePoints = getEdgePoints();
            }
        }

        internal void SetDualStructure()
        {
            emptyCells = spaceDim * spaceDim;
            growthDone = false;
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    if (selectedGrains.Contains(previousStep[i][j].grainID))
                    {
                        //save this grain
                        nextStep[i][j].grainID = -3;
                        emptyCells--;

                    }
                    else
                    {
                        nextStep[i][j].grainID = -1;
                    }

                }
            }

            copyList();
            DrawBitmap();

        }

        private List<Point> getEdgePoints()
        {
            List<Point> result = new List<Point>();
            int newGrainID;
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    var neighbors = getVonNeumannNeighbors(i, j).Where(c => c.grainID != -1 && c.grainID != -2).GroupBy(c => c.grainID).OrderByDescending(g => g.Count()).ToList();

                    if (neighbors.Count() > 0)
                    {
                        newGrainID = neighbors[0].ToList()[0].grainID;
                        if (newGrainID != previousStep[i][j].grainID)
                        {
                            result.Add(new Point(i, j));
                        }

                    }
                }
            }

            return result;
        }

        private List<int> getMostCommon(List<Cell> input)
        {
            List<int> result = new List<int>();
            int maxCounter = 0;
            int maxID = -1;
            int counter = 1;
            int ID = input[input.Count - 1].grainID;
            for (int i = input.Count - 2; i > -1; i--)
            {
                if (input[i].grainID == ID)
                {
                    counter++;
                    if (counter > maxCounter)
                    {
                        maxCounter = counter;
                        maxID = input[i].grainID;
                    }
                }
                else
                {
                    counter = 1;
                    ID = input[i].grainID;
                }
            }
            result.Add(maxCounter);
            result.Add(maxID);
            return result;
        }

        private List<int> getMostCommonValid(List<Cell> input)
        {
            List<int> result = new List<int>();
            int maxCounter = 0;
            int maxID = -1;
            int counter = 0;
            int ID = input[input.Count - 1].grainID;
            if (ID > -1)
            {
                maxCounter = 1;
                counter = 1;
                maxID = ID;
            }
            for (int i = input.Count - 2; i > -1; i--)
            {
                if (input[i].grainID > -1)
                {
                    if (input[i].grainID == ID)
                    {
                        counter++;
                        if (counter > maxCounter)
                        {
                            maxCounter = counter;
                            maxID = input[i].grainID;
                        }
                    }
                    else
                    {
                        counter = 1;
                        ID = input[i].grainID;
                    }
                }

            }
            result.Add(maxCounter);
            result.Add(maxID);
            return result;
        }

        internal void ImportFromTxt(StreamReader sr)
        {
            String line = sr.ReadLine();
            int grainsNumber = System.Convert.ToInt32(line);
            String[] colorString;
            Console.WriteLine(grainsNumber);
            List<Grain> newGrains = new List<Grain>();
            int R, G, B;

            for (int i = 0; i < grainsNumber; i++)
            {
                line = sr.ReadLine();
                colorString = line.Split(';');
                R = System.Convert.ToInt32(colorString[0]);
                G = System.Convert.ToInt32(colorString[1]);
                B = System.Convert.ToInt32(colorString[2]);
                newGrains.Add(new Grain(i, System.Drawing.Color.FromArgb(R, G, B)));
            }

            line = sr.ReadLine();
            int newSpaceSize = System.Convert.ToInt32(line);
            List<List<Cell>> newStep = new List<List<Cell>>();

            for (int i = 0; i < newSpaceSize; i++)
            {
                newStep.Add(new List<Cell>());
                for (int j = 0; j < newSpaceSize; j++)
                {
                    newStep[i].Add(new Cell());
                }
            }

            int listLenght = newSpaceSize * newSpaceSize;
            String[] cellInfoArray;
            int x, y, id;
            for (int i = 0; i < listLenght; i++)
            {
                line = sr.ReadLine();
                cellInfoArray = line.Split(';');
                x = System.Convert.ToInt32(cellInfoArray[0]);
                y = System.Convert.ToInt32(cellInfoArray[1]);
                id = System.Convert.ToInt32(cellInfoArray[2]);
                newStep[x][y].grainID = id;
            }

            //swap old data with new ones, if no exception

            grains = newGrains;
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    previousStep[i][j].grainID = newStep[i][j].grainID;
                }
            }

            DrawBitmap();
            Console.WriteLine("Import done");
        }

        internal void SetRandomInclusions(int inclusionsNumber, int inclusionSize, int inclusionType)
        {
            Console.WriteLine("Start");
            Random rnd = new Random();
            int x;
            int y;
            int index;
            Point edgePoint;
            bool done = false;
            int halfSize = inclusionSize / 2;


            if (growthDone)
            {
                //inclusions on grains edge
                for (int i = 0; i < inclusionsNumber; i++)
                {
                    do
                    {
                        index = rnd.Next(0, edgePoints.Count - 1);
                        edgePoint = edgePoints[index];
                        done = CheckIfPerimeterInclusion(edgePoint.X, edgePoint.Y, halfSize);
                    } while (!done);
                    Console.WriteLine("draw inclusuion number: " + i);
                    DrawInclusion(edgePoint.X, edgePoint.Y, halfSize, inclusionType);
                    done = false;
                }
            }
            else
            {
                for (int i = 0; i < inclusionsNumber; i++)
                {
                    do
                    {
                        Console.WriteLine("Start random coords number: " + i);
                        x = rnd.Next(halfSize, spaceDim - halfSize);
                        y = rnd.Next(halfSize, spaceDim - halfSize);
                        done = CheckIfPerimeterEmpty(x, y, halfSize);
                    } while (!done);
                    Console.WriteLine("draw inclusuion number: " + i);
                    DrawInclusion(x, y, halfSize, inclusionType);
                    done = false;
                }
                //x = rnd.Next(0, spaceDim);
                //y = rnd.Next(0, spaceDim);
            }
            Console.WriteLine("draw bitmap");
            DrawBitmap();
            Console.WriteLine("Finish");
        }

        private void DrawInclusion(int x, int y, int diameter, int inclusionType)
        {

            for (int i = x - diameter; i < x + diameter; i++)
            {
                for (int j = y - diameter; j < y + diameter; j++)
                {
                    if (inclusionType == 0)
                    {
                        if (Math.Sqrt(Math.Pow((i - x), 2) + Math.Pow((j - y), 2)) > diameter)
                        {
                            continue;
                        }
                    }
                    previousStep[i][j].grainID = -2;
                    emptyCells--;
                }
            }
        }

        private bool CheckIfPerimeterEmpty(int x, int y, int diameter)
        {
            for (int i = x - diameter; i < x + diameter; i++)
            {
                for (int j = y - diameter; j < y + diameter; j++)
                {
                    if (i < 0 || i >= spaceDim || j < 0 || j >= spaceDim)
                    {
                        return false;
                    }
                    if (previousStep[i][j].grainID != -1)
                    {
                        return false;
                    }
                }
            }
            Console.WriteLine("Finish check perimeter loop ");
            return true;
        }

        private bool CheckIfPerimeterInclusion(int x, int y, int diameter)
        {
            for (int i = x - diameter; i < x + diameter; i++)
            {
                for (int j = y - diameter; j < y + diameter; j++)
                {
                    if (i < 0 || i >= spaceDim || j < 0 || j >= spaceDim)
                    {
                        return false;
                    }
                    if (previousStep[i][j].grainID == -2)
                    {
                        return false;
                    }
                }
            }
            Console.WriteLine("Finish check perimeter loop ");
            return true;
        }

        internal void ImportFromBmp(Bitmap bmp)
        {
            if (bmp.Height != spaceDim || bmp.Width != spaceDim)
            {
                throw new Exception();
            }

            //go around all pixels and check how many colors have been used on bitmap, then set cells

            throw new NotImplementedException();
        }

        internal void ExportToTxt(StreamWriter sw)
        {
            sw.WriteLine(grains.Count);
            for (int i = 0; i < grains.Count; i++)
            {
                sw.WriteLine(grains[i].color.R + ";" + grains[i].color.G + ";" + grains[i].color.B);
            }
            sw.WriteLine(spaceDim);
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    sw.WriteLine(i + ";" + j + ";" + previousStep[i][j].grainID);
                }
            }
        }

        private void copyList()
        {
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    previousStep[i][j] = new Cell(nextStep[i][j]);
                }
            }
        }

        private List<Cell> getVonNeumannNeighbors(int x, int y)
        {
            List<Cell> result = new List<Cell>();
            if (y - 1 < 0)
            {
                result.Add(previousStep[x][spaceDim - 1]);
            }
            else
            {
                result.Add(previousStep[x][y - 1]);
            }
            if (x - 1 < 0)
            {
                result.Add(previousStep[spaceDim - 1][y]);
            }
            else
            {
                result.Add(previousStep[x - 1][y]);
            }
            if (x + 1 >= spaceDim)
            {
                result.Add(previousStep[0][y]);
            }
            else
            {
                result.Add(previousStep[x + 1][y]);
            }
            if (y + 1 >= spaceDim)
            {
                result.Add(previousStep[x][0]);
            }
            else
            {
                result.Add(previousStep[x][y + 1]);
            }

            return result;
        }

        private List<Cell> getMooreNeighbors(int x, int y)
        {
            List<Cell> result = new List<Cell>();

            int xl, xr, yt, yb;
            if (y - 1 < 0)
            {
                yt = spaceDim - 1;
            }
            else
            {
                yt = y - 1;
            }
            if (y + 1 >= spaceDim)
            {
                yb = 0;
            }
            else
            {
                yb = y + 1;
            }
            if (x - 1 < 0)
            {
                xl = spaceDim - 1;
            }
            else
            {
                xl = x - 1;
            }
            if (x + 1 >= spaceDim)
            {
                xr = 0;
            }
            else
            {
                xr = x + 1;
            }
            result.Add(previousStep[xl][yt]);
            result.Add(previousStep[x][yt]);
            result.Add(previousStep[xr][yt]);
            result.Add(previousStep[xl][y]);
            result.Add(previousStep[xr][y]);
            result.Add(previousStep[xl][yb]);
            result.Add(previousStep[x][yb]);
            result.Add(previousStep[xr][yb]);
            return result;
        }

        private List<Cell> getNearestMooreNeighbors(int x, int y)
        {
            List<Cell> result = new List<Cell>();

            int xl, xr, yt, yb;
            if (y - 1 < 0)
            {
                yt = spaceDim - 1;
            }
            else
            {
                yt = y - 1;
            }
            if (y + 1 >= spaceDim)
            {
                yb = 0;
            }
            else
            {
                yb = y + 1;
            }
            if (x - 1 < 0)
            {
                xl = spaceDim - 1;
            }
            else
            {
                xl = x - 1;
            }
            if (x + 1 >= spaceDim)
            {
                xr = 0;
            }
            else
            {
                xr = x + 1;
            }
            result.Add(previousStep[x][yt]);
            result.Add(previousStep[xl][y]);
            result.Add(previousStep[xr][y]);
            result.Add(previousStep[x][yb]);
            return result;
        }

        private List<Cell> getFurtherMooreNeighbors(int x, int y)
        {
            List<Cell> result = new List<Cell>();

            int xl, xr, yt, yb;
            if (y - 1 < 0)
            {
                yt = spaceDim - 1;
            }
            else
            {
                yt = y - 1;
            }
            if (y + 1 >= spaceDim)
            {
                yb = 0;
            }
            else
            {
                yb = y + 1;
            }
            if (x - 1 < 0)
            {
                xl = spaceDim - 1;
            }
            else
            {
                xl = x - 1;
            }
            if (x + 1 >= spaceDim)
            {
                xr = 0;
            }
            else
            {
                xr = x + 1;
            }
            result.Add(previousStep[xl][yt]);
            result.Add(previousStep[xr][yt]);
            result.Add(previousStep[xl][yb]);
            result.Add(previousStep[xr][yb]);
            return result;
        }
    }
}
