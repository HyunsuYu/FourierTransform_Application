using System;

namespace ConsoleApp1
{
    sealed public class Fourier : IDisposable
    {
        private struct FrequencyFunction
        {
            public double mamplitude;
            public double mperiod;
        }
        private struct NodeVector
        {
            public double mcurTime;
            public double mcenter_x;
            public double mcenter_y;
            public double mvector_x;
            public double mvector_y;
            public double mdot_x;
            public double mdot_y;
        }

        private FrequencyFunction[] mnodeInfos;
        private NodeVector[] mnodeVectors;

        private int[,] mmap;

        private int mmaxVectorNum;
        private int mmaxAmplitude;
        private int mmaxPeriod;
        private double mfrequency;
        private int mfirstN;

        public int mcoord_x, mcoord_y;

        private const int MAP_LENGTH = 50;
        private const double PI2 = Math.PI * 2.0f;

        Random random;

        //  public method
        public Fourier(int coord_x, int coord_y, int maxVectorNum, int maxAmplitude, int maxPeriod, double frequency, int firstN)
        {
            mmaxVectorNum = maxVectorNum;
            mmaxAmplitude = maxAmplitude;
            mmaxPeriod = maxPeriod;
            mfrequency = frequency;
            mfirstN = firstN;

            mcoord_x = coord_x;
            mcoord_y = coord_y;

            mnodeInfos = new FrequencyFunction[mmaxVectorNum];
            mnodeVectors = new NodeVector[mmaxVectorNum];
            mmap = new int[MAP_LENGTH, MAP_LENGTH];

            random = new Random();

            SetNodeInfos();
            MakeFourier(coord_x, coord_y);
        }
        public void Print()
        {
            for (int coord_y = 0; coord_y < MAP_LENGTH; coord_y++)
            {
                for (int coord_x = 0; coord_x < MAP_LENGTH; coord_x++)
                {
                    if (coord_x == 24 && coord_y == 24)
                    {
                        Console.Write("●");
                    }
                    else if (mmap[coord_x, coord_y] == 1)
                    {
                        Console.Write("■");
                    }
                    else
                    {
                        Console.Write("□");
                    }
                }
                Console.WriteLine("");
            }
        }
        public void NextDraw(double change)
        {
            System.Threading.Thread.Sleep(2000);
            Console.Clear();

            for (int i = 0; i < mmaxVectorNum; i++)
            {
                mnodeInfos[i].mamplitude += change;
                mnodeInfos[i].mperiod += change;

                mnodeVectors[i].mcenter_x = 0.0;
                mnodeVectors[i].mcenter_y = 0.0;
                mnodeVectors[i].mcurTime = 0.0;
                mnodeVectors[i].mdot_x = 0.0;
                mnodeVectors[i].mdot_y = 0.0;
                mnodeVectors[i].mvector_x = 0.0;
                mnodeVectors[i].mvector_y = 0.0;
            }

            for(int coord_y = 0; coord_y < MAP_LENGTH; coord_y++)
            {
                for(int coord_x = 0; coord_x < MAP_LENGTH; coord_x++)
                {
                    mmap[coord_y, coord_x] = 0;
                }
            }

            MakeFourier(mcoord_x, mcoord_y);

            Print();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        //  private method
        ~Fourier()
        {
            Dispose(false);
        }
        private void Dispose(bool flag)
        {
            mnodeInfos = null;
            mnodeVectors = null;
            mmap = null;
            random = null;
        }
        private void SetNodeInfos()
        {
            for (int i = 0; i < mmaxVectorNum; i++)
            {
                mnodeInfos[i].mamplitude = random.NextDouble() * mmaxAmplitude;
                mnodeInfos[i].mperiod = random.NextDouble() * mmaxPeriod;
            }
        }
        private void MakeFourier(int coord_x, int coord_y)
        {
            int flagIndex = Math.Abs(mfirstN) - 1;

            mnodeVectors[0].mcenter_x = coord_x;
            mnodeVectors[0].mcenter_y = coord_y;

            while (mnodeVectors[flagIndex].mcurTime <= PI2)
            {
                for (int index = 0; index < mmaxVectorNum; index++)
                {
                    if (mfirstN + index < 0)
                    {
                        mnodeVectors[index].mvector_x = GetFx(index) * GetCircle_x(index);
                        mnodeVectors[index].mvector_y = GetFx(index) * GetCircle_y(index, true);
                        mnodeVectors[index].mdot_x = mnodeVectors[index].mcenter_x + mnodeVectors[index].mvector_x;
                        mnodeVectors[index].mdot_y = mnodeVectors[index].mcenter_y + mnodeVectors[index].mvector_y;
                        mnodeVectors[index].mcurTime += mfrequency * Math.Abs(mfirstN + index);

                        MoveNestVectors(index);
                    }
                    else
                    {
                        mnodeVectors[index].mvector_x = GetFx(index) * GetCircle_x(index);
                        mnodeVectors[index].mvector_y = GetFx(index) * GetCircle_y(index, false);
                        mnodeVectors[index].mdot_x = mnodeVectors[index].mcenter_x + mnodeVectors[index].mvector_x;
                        mnodeVectors[index].mdot_y = mnodeVectors[index].mcenter_y + mnodeVectors[index].mvector_y;
                        mnodeVectors[index].mcurTime += mfrequency * Math.Abs(mfirstN + index);

                        MoveNestVectors(index);
                    }

                    if (index == mmaxVectorNum - 1 && (mnodeVectors[index].mdot_x >= 0 && mnodeVectors[index].mdot_x < MAP_LENGTH && mnodeVectors[index].mdot_y >= 0 && mnodeVectors[index].mdot_y < MAP_LENGTH))
                    {
                        mmap[(int)mnodeVectors[index].mdot_x, (int)mnodeVectors[index].mdot_y] = 1;
                    }
                }
            }
        }
        private double GetFx(int index)
        {
            return mnodeInfos[index].mamplitude * Math.Cos(mnodeVectors[index].mcurTime * mnodeInfos[index].mperiod);
        }
        private double GetCircle_x(int index)
        {
            return Math.Cos(mnodeVectors[index].mcurTime);
        }
        private double GetCircle_y(int index, bool isClockWay)
        {
            if (isClockWay == true)
            {
                return Math.Sin(mnodeVectors[index].mcurTime);
            }
            else
            {
                return (-1) * Math.Sin(mnodeVectors[index].mcurTime);
            }
        }
        private void MoveNestVectors(int index)
        {
            for (int i = index + 1; i < mmaxVectorNum; i++)
            {
                mnodeVectors[i].mcenter_x = mnodeVectors[i - 1].mdot_x;
                mnodeVectors[i].mcenter_y = mnodeVectors[i - 1].mdot_y;
                mnodeVectors[i].mdot_x = mnodeVectors[i].mcenter_x + mnodeVectors[i].mvector_x;
                mnodeVectors[i].mdot_y = mnodeVectors[i].mcenter_y + mnodeVectors[i].mvector_y;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Fourier fourier = new Fourier(25, 25, 7, 15, 5, 0.001, -3);
            fourier.Print();

            for(int i = 0; i < 20; i++)
            {
                fourier.NextDraw(0.1);
            }

            fourier.Dispose();
        }
    }
}