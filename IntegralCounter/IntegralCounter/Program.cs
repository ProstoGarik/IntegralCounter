using System.Xml;

Func<double, double> ParabolaGetY = (x) => x * x;

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
double b = 1;
double tolerance = 0.000001;

double result = Integrate(ParabolaGetY, a, b, tolerance);

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

Console.WriteLine($"Значение интеграла: {result}");
Console.WriteLine($"Значение интеграла: {IntegrateWithSegments(a, b, 4)}");


Console.ReadLine();