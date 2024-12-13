using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Xml;
using System.Xml.Serialization;

Func<double, double> ParabolaGetY = (x) => x * x;
int processorCount = Environment.ProcessorCount;

double Integrate(Func<double, double> func, double a, double b, double tolerance)
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



double a = 0;
double b = 10;
double tolerance = 0.000001;

double IntegrateWithSegments(double a, double b, int segments)
{
    double segmentSize = (b - a) / segments;
    double areaSum = 0;

    for (int i = 0; i < segments; i++)
    {
        areaSum += Integrate(ParabolaGetY, i * segmentSize, i * segmentSize + segmentSize, tolerance);
    }

    return areaSum;
}

double IntegrateThreads(double a, double b, int segments)
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
            double tempSum = Integrate(ParabolaGetY, tempIndex * segmentSize, tempIndex * segmentSize + segmentSize, tolerance);
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

double IntegrateParallel(double a, double b, int segments)
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
            double tempSum = Integrate(ParabolaGetY, tempIndex * segmentSize, tempIndex * segmentSize + segmentSize, tolerance);
            lock (locker)
            {
                areaSum += tempSum;
            }
        }));
    }

    Task.WaitAll(tasks.ToArray());

    return areaSum;
}



Stopwatch stopwatch = new Stopwatch();
Console.WriteLine("Считаем...");

stopwatch.Start();
Console.WriteLine($"Значение интеграла обычно: {Integrate(ParabolaGetY, a, b, tolerance)}");
Console.WriteLine("За " + stopwatch.ElapsedMilliseconds + " миллисекунд");


stopwatch.Restart();
Console.WriteLine($"Значение интеграла с сегментами: {IntegrateWithSegments(a, b, 4)}");
Console.WriteLine("За " + stopwatch.ElapsedMilliseconds + " миллисекунд");


stopwatch.Restart();
Console.WriteLine($"Значение интеграла с сегментами (Thread): {IntegrateThreads(a, b, processorCount)}");
Console.WriteLine("За " + stopwatch.ElapsedMilliseconds + " миллисекунд");

stopwatch.Restart();
Console.WriteLine($"Значение интеграла с сегментами (Task): {IntegrateParallel(a, b, processorCount)}");
Console.WriteLine("За " + stopwatch.ElapsedMilliseconds + " миллисекунд");

Console.ReadLine();
