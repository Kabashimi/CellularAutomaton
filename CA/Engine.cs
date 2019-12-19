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
        public System.Drawing.Bitmap bitmapEnergy;
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
        public bool showEnergy;
        double maxEnergy;


        public Engine()
        {
            maxEnergy = 0;
            showEnergy = false;
            probability = 100;
            neighborhoodType = 0;
            emptyCells = spaceDim * spaceDim;
            growthDone = false;
            bitmap = new System.Drawing.Bitmap(spaceDim, spaceDim);
            bitmapEnergy = new System.Drawing.Bitmap(spaceDim, spaceDim);
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
            double colorShift;
            Color energyColor;
            for (int i = 0; i < previousStep.Count; i++)
            {
                for (int j = 0; j < previousStep[i].Count; j++)
                {
                    if (previousStep[i][j].grainID >= 0)
                    {
                        double energy = previousStep[i][j].energy;
                        bitmap.SetPixel(i, j, grains[previousStep[i][j].grainID].color);
                        colorShift = 254.0 * ((double)previousStep[i][j].energy / (double)maxEnergy);
                        if (colorShift > 255) { colorShift = 255; }
                        if (colorShift < 0) { colorShift = 0; }
                        if (colorShift != colorShift) { colorShift = 0; }
                        energyColor = Color.FromArgb((int)colorShift, 254 - (int)colorShift, 0);
                        bitmapEnergy.SetPixel(i, j, energyColor);
                    }
                    else if (previousStep[i][j].grainID == -2)
                    {
                        bitmap.SetPixel(i, j, System.Drawing.Color.Black);
                        bitmapEnergy.SetPixel(i, j, Color.FromArgb(255, 0, 0));
                    }
                    else if (previousStep[i][j].grainID == -3)
                    {
                        bitmap.SetPixel(i, j, System.Drawing.Color.Magenta);
                        bitmapEnergy.SetPixel(i, j, Color.FromArgb(255, 0, 0));
                    }
                    else
                    {
                        bitmap.SetPixel(i, j, System.Drawing.Color.White);
                        bitmapEnergy.SetPixel(i, j, Color.White);
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
                    if (previousStep[i][j].grainID == -1)
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

        internal void Recrystallize(int nucleonsLocation, int nucleationType, int nucleonsNumber)
        {
            int iterator = 0;
            int nucleonsToAdd = 0;
            List<Point> pointsAvailable;
            Random rnd = new Random();


            //pre-loop

            if (nucleationType == 0)    //constant
            {
                nucleonsToAdd = nucleonsNumber;
            }
            else if (nucleationType == 1)    //increasing
            {
                nucleonsToAdd += nucleonsNumber;
            }
            else if (nucleationType == 2)    //all at the begining
            {
                if (iterator == 0)
                {
                    nucleonsToAdd = nucleonsNumber;
                }
                else
                {
                    nucleonsToAdd = 0;
                }
            }

            pointsAvailable = new List<Point>();
            if (nucleonsLocation == 0) // Grain boundaries
            {
                for (int i = 0; i < spaceDim; i++)
                {
                    for (int j = 0; j < spaceDim; j++)
                    {
                        var tmpNeighbors = getVonNeumannNeighbors(i, j).Where(c => c.grainID != -1 && c.grainID != -2).GroupBy(c => c.grainID).OrderByDescending(g => g.Count()).ToList();
                        if (tmpNeighbors.Count() > 1)
                        {
                            pointsAvailable.Add(new Point(i, j));
                        }
                    }
                }
            }
            else    //Anywhere
            {
                for (int i = 0; i < spaceDim; i++)
                {
                    for (int j = 0; j < spaceDim; j++)
                    {
                        pointsAvailable.Add(new Point(i, j));
                    }
                }
            }


            //loop

            //      nucleation:
            if (nucleonsToAdd > pointsAvailable.Count)
            {
                nucleonsToAdd = pointsAvailable.Count;
            }
            pointsAvailable = ShuffleList<Point>(pointsAvailable);
            int x, y;
            Color color;
            for (int i = 0; i < nucleonsToAdd; i++)
            {
                x = pointsAvailable[i].X;
                y = pointsAvailable[i].Y;
                color = Color.FromArgb(rnd.Next(100, 255), 0, 0);
                grains.Add(new Grain(grains.Count, color, true));
                previousStep[x][y].grainID = grains.Count - 1;
                previousStep[x][y].recristalized = true;
                previousStep[x][y].energy = 0;
                bitmapEnergy.SetPixel(x, y, Color.FromArgb(0, 255, 0));
                bitmap.SetPixel(x, y, color);
            }
            //      growth:

            List<Point> points = new List<Point>();
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    points.Add(new Point(i, j));
                }
            }
            points = ShuffleList<Point>(points);
            List<Cell> neighbors;
            double oldCellEnergy, newCellEnergy;
            int newCellID;
            Color energyColor;

            for (int i = 0; i < points.Count; i++)
            {
                x = points[i].X;
                y = points[i].Y;
                //neighbors = RemoveInactiveCells(getMooreNeighbors(x, y));
                neighbors = getMooreNeighbors(x, y);
                oldCellEnergy = GetCellEnergy(neighbors, previousStep[x][y].grainID) + previousStep[x][y].energy;

                newCellID = getNewRecrystalizedNeighbor(neighbors);
                if (newCellID == -1)
                    continue;
                newCellEnergy = GetCellEnergy(neighbors, newCellID);
                if (newCellEnergy < oldCellEnergy)
                {
                    previousStep[x][y].grainID = newCellID;
                    previousStep[x][y].recristalized = true;
                    previousStep[x][y].energy = 0;
                    energyColor = Color.FromArgb(0, 255, 0);
                    bitmapEnergy.SetPixel(x, y, energyColor);
                    bitmap.SetPixel(x, y, grains[newCellID].color);
                }
            }
            Console.WriteLine("Iteration done");

        }

        private int getNewRecrystalizedNeighbor(List<Cell> list)
        {
            int index;
            Random rnd = new Random();
            do
            {
                index = rnd.Next(0, list.Count);
                if (list[index].recristalized)
                {
                    return list[index].grainID;
                }
                list.RemoveAt(index);
            } while (list.Count() > 0);
            return -1;
        }

        internal void DistributeEnergy(int distributionType, int minEnergy, int maxEnergy, int deviation)
        {
            this.maxEnergy = maxEnergy + maxEnergy * (Double)((double)deviation / 100.0);
            Random rnd = new Random();
            double energyDeviation;
            if (distributionType == 0)
            {
                for (int i = 0; i < spaceDim; i++)
                {
                    for (int j = 0; j < spaceDim; j++)
                    {
                        energyDeviation = ((double)rnd.Next(-deviation, deviation) / 100) * minEnergy;
                        nextStep[i][j].energy = minEnergy + (double)energyDeviation;
                    }
                }
            }
            else if (distributionType == 1)
            {
                for (int i = 0; i < spaceDim; i++)
                {
                    for (int j = 0; j < spaceDim; j++)
                    {
                        var neighbors = getVonNeumannNeighbors(i, j).Where(c => c.grainID != -1 && c.grainID != -2).GroupBy(c => c.grainID).OrderByDescending(g => g.Count()).ToList();

                        if (neighbors.Count() > 1)
                        {
                            energyDeviation = ((double)rnd.Next(-deviation, deviation) / 100) * maxEnergy;
                            nextStep[i][j].energy = maxEnergy + (double)energyDeviation;
                        }
                        else
                        {
                            energyDeviation = ((double)rnd.Next(-deviation, deviation) / 100) * minEnergy;
                            nextStep[i][j].energy = minEnergy + (double)energyDeviation;
                        }
                    }
                }
            }

            copyList();
            DrawBitmap();
        }

        internal void MCGrow(System.Windows.Forms.PictureBox pictureBox1)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<Point> points = new List<Point>();
            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    points.Add(new Point(i, j));
                }
            }
            Random rnd = new Random();
            int pointID;
            int newCellID;
            List<Cell> neighbors;
            int x, y;
            int oldCellEnergy, newCellEnergy;
            do
            {
                pointID = rnd.Next(0, points.Count);
                x = points[pointID].X;
                y = points[pointID].Y;
                if (previousStep[x][y].grainID < -1)
                {
                    points.Remove(points[pointID]);
                    continue;
                }
                //do operations on nextStep[points[pointID.x][points[pointID.y]]
                neighbors = RemoveInactiveCells(getMooreNeighbors(x, y));
                if (neighbors.Count == 0)
                {
                    points.Remove(points[pointID]);
                    continue;
                }
                oldCellEnergy = GetCellEnergy(neighbors, previousStep[x][y].grainID);
                if (oldCellEnergy <= 4)
                {
                    points.Remove(points[pointID]);
                    continue;
                }
                newCellID = neighbors[rnd.Next(0, neighbors.Count)].grainID;
                newCellEnergy = GetCellEnergy(neighbors, newCellID);

                if (newCellID < oldCellEnergy)
                {
                    nextStep[x][y].grainID = newCellID;
                    bitmap.SetPixel(x, y, grains[newCellID].color);
                }

                points.Remove(points[pointID]);
            } while (points.Count > 0);

            Console.WriteLine("Iteration done!");

            watch.Stop();
            Console.WriteLine("Iteartion time: " + watch.ElapsedMilliseconds / 1000);
            copyList();
        }

        private List<Cell> RemoveInactiveCells(List<Cell> list)
        {
            List<Cell> modifiedList = new List<Cell>();
            foreach (Cell cell in list)
            {
                if (cell.grainID >= 0)
                {
                    modifiedList.Add(new Cell(cell.grainID));
                }
            }
            return modifiedList;
        }

        private int GetCellEnergy(List<Cell> neighbors, int cellID)
        {
            int energy = 0;
            for (int i = 0; i < neighbors.Count - 1; i++)
            {
                if (neighbors[i].grainID != cellID && neighbors[i].grainID >= 0)
                {
                    energy++;
                }
            }
            return energy;
        }

        internal void setMCGrains(int grainsNumber)
        {
            Random rnd = new Random();
            grains = new List<Grain>();

            for (int i = 0; i < grainsNumber; i++)
            {
                System.Drawing.Color randomColor = System.Drawing.Color.FromArgb(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));
                grains.Add(new Grain(i, randomColor));
            }

            for (int i = 0; i < spaceDim; i++)
            {
                for (int j = 0; j < spaceDim; j++)
                {
                    if (previousStep[i][j].grainID < -1)
                    {
                        nextStep[i][j].grainID = previousStep[i][j].grainID;

                        continue;
                    }
                    nextStep[i][j].grainID = rnd.Next(0, grainsNumber);
                }
            }
            copyList();
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

        private List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }
    }
}
