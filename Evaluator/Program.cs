using System;
using System.Collections.Generic;
using System.Text;

class Program
{  
    static void Main(string[] args)
    {
        ImportMap importMap = new ImportMap();
        MapTransformer mapTrans = new MapTransformer();
        ImportTraj importTraj = new ImportTraj();
        RandomMap rand = new RandomMap();
        TrajGenerator trajGenerator = new TrajGenerator();
        TrajSaver trajSaver = new TrajSaver();
        MapPruner mapPruner = new MapPruner();
        SampleNeighbourhood sampleNeighbourhood = new SampleNeighbourhood();
        MapSaver mapSaver = new MapSaver();
        Evaluator eval = new Evaluator(rand, sampleNeighbourhood);

        Map gt = importMap.ReadMap($"Chicago/Chicago-200-directed-50-100");
        Map cm = importMap.ReadMap($"Kharita/Directed/Chicago-200-50-100");

        (float, float) precall = eval.EvalNeighbourhood(gt, cm);

        Console.WriteLine(precall);
    }
}
