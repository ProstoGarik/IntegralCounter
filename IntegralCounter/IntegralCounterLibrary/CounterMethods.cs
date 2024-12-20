using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace IntegralCounterLibrary
{
    public class CounterMethods
    {
        Func<double, double> ParabolaGetY = (x) => x * x;
        double tolerance = 0.000001;

        public double Tolerance { get => tolerance; set => tolerance = value; }
        public Func<double, double> ParabolaGetY1 { get => ParabolaGetY; set => ParabolaGetY = value; }

        public CounterMethods() { }

        public double Integrate(Func<double, double> func, double a, double b, double tolerance)
        {
            double n = 1;
            double previousArea = 0;

            while (true)
            {
                double width = (b - a) / n;
                double area = 0;

                for (int i = 0; i < n; i++)
                {
                    double x = a + i * width;
                    area += func(x) * width;
                }

                if (area != previousArea)
                {
                    if (Math.Abs(area - previousArea) < tolerance)
                    {
                        break;
                    }
                }

                previousArea = area;
                n *= 2;

            }

            return previousArea;
        }

        public double IntegrateWithSegments(double a, double b, int segments, double tolerance)
        {
            double segmentSize = (b - a) / segments;
            double areaSum = 0;

            for (int i = 0; i < segments; i++)
            {
                areaSum += Integrate(ParabolaGetY1, i * segmentSize, i * segmentSize + segmentSize, tolerance);
            }

            return areaSum;
        }
        public double IntegrateThreads(double a, double b, int segments, double tolerance)
        {
            double segmentSize = (b - a) / segments;
            double areaSum = 0;
            object locker = new object();
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < segments; i++)
            {
                int tempIndex = i;
                Thread thread = new Thread(() =>
                {
                    double tempSum = Integrate(ParabolaGetY1, tempIndex * segmentSize, tempIndex * segmentSize + segmentSize, tolerance);
                    lock (locker)
                    {
                        areaSum += tempSum;
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            return areaSum;
        }

        public double IntegrateParallel(double a, double b, int segments, double tolerance)
        {
            double segmentSize = (b - a) / segments;
            double areaSum = 0;
            object locker = new object();

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < segments; i++)
            {
                int tempIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    double tempSum = Integrate(ParabolaGetY1, tempIndex * segmentSize, tempIndex * segmentSize + segmentSize, tolerance);
                    lock (locker)
                    {
                        areaSum += tempSum;
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return areaSum;
        }


    }
}
