using System;
using System.Collections.Generic;
using System.Numerics;

public class Evaluator
{
    RandomMap rand;
    SampleNeighbourhood sampleNeighbourhood;

    public Evaluator(RandomMap rand, SampleNeighbourhood sample)
    {
        this.rand = rand;
        this.sampleNeighbourhood = sample;
    }

    /// <summary>
    /// Evaluates the similarity between two maps using the graph sampling evaluation of biagioni and eriksson.
    /// Parameters are hardcoded, based on the ones we used in the corresponding research.
    /// </summary>
    /// <param name="GT">The ground truth map</param>
    /// <param name="CT">the constructed map.</param>
    /// <param name="amount">the amount of neighbourhood evaluations we average over</param>
    /// <returns>A precision and recall value indicating the similarity between the maps.</returns>
    public (float, float) EvalMap(Map GT, Map CT, int amount)
    {
        float precision = 0;
        float recall = 0;

        for (int k = 0; k < amount; k++)
        {
            Coordinate originGT = rand.GetRandomPointOnRoad(rand.GetWeightedRandomRoad(GT));
            Coordinate originCT = CT.GetClosestPoint(originGT.location);

            /* An extra step that is sometimes used in the literature when ground truth map is not pruned.
            while (Vector3.Distance(originCT.location, originGT.location) > 50)
            {
                originGT = rand.GetRandomCoordinate(rand.GetWeightedRandomRoad(GT));
                originCT = CT.GetClosestPoint(originGT.location);
            }
            */

            List<Vector3> pointsGT = sampleNeighbourhood.GetNeighbourhood(GT, originGT, 500, 30);
            List<Vector3> pointsCT = sampleNeighbourhood.GetNeighbourhood(CT, originCT, 500, 30);

            MaxFlow flow = new MaxFlow(pointsGT.Count + pointsCT.Count + 2);

            for (int i = 0; i < pointsCT.Count; i++) flow.AddEdge(0, i + 1, 1);

            for (int i = 0; i < pointsCT.Count; i++)
            {
                Vector3 pointCT = pointsCT[i];

                for (int j = 0; j < pointsGT.Count; j++)
                {
                    Vector3 pointGT = pointsGT[j];
                    if (Vector3.Distance(pointCT, pointGT) < 20) flow.AddEdge(i + 1, j + pointsCT.Count + 1, 1);
                }
            }

            for (int i = 0; i < pointsGT.Count; i++) flow.AddEdge(i + pointsCT.Count + 1, 1 + pointsCT.Count + pointsGT.Count, 1);

            float matching = flow.FindMaximumFlow(0, 1 + pointsCT.Count + pointsGT.Count).Item1;
            precision += pointsCT.Count > 0 ? matching / pointsCT.Count : 0;
            recall += pointsGT.Count > 0 ? matching / pointsGT.Count : 0;
        }

        return (precision / amount, recall / amount);
    }


    /// <summary>
    /// Visualizes and evaluates a single random neighbourhood.
    /// </summary>
    /// <param name="GT">Ground truth map</param>
    /// <param name="CT">contructed map</param>
    /// <param name="originDistanceCondition"></param>
    /// <param name="matchDistance"></param>
    /// <returns></returns>
    public (float, float) EvalNeighbourhood(Map GT, Map CT)
    {
        Coordinate originGT = rand.GetRandomPointOnRoad(rand.GetWeightedRandomRoad(GT));
        Coordinate originCT = CT.GetClosestPoint(originGT.location);

        /* An extra step that is sometimes used in the literature when ground truth map is not pruned.
        while (Vector3.Distance(originCT.location, originGT.location) > 50)
        {
            originGT = rand.GetRandomCoordinate(rand.GetWeightedRandomRoad(GT));
            originCT = CT.GetClosestPoint(originGT.location);
        }
        */

        List<Vector3> pointsGT = sampleNeighbourhood.GetNeighbourhood(GT, originGT, 150, 30);
        List<Vector3> pointsCT = sampleNeighbourhood.GetNeighbourhood(CT, originCT, 150, 30);

        MaxFlow flow = new MaxFlow(pointsGT.Count + pointsCT.Count + 2);

        for (int i = 0; i < pointsCT.Count; i++) flow.AddEdge(0, i + 1, 1); // source edges

        for (int i = 0; i < pointsCT.Count; i++) // edges between CT and GT
        {
            Vector3 pointCT = pointsCT[i];

            for (int j = 0; j < pointsGT.Count; j++)
            {
                Vector3 pointGT = pointsGT[j];
                if (Vector3.Distance(pointCT, pointGT) < 20) flow.AddEdge(i + 1, j + pointsCT.Count + 1, 1);
            }
        }

        for (int i = 0; i < pointsGT.Count; i++) flow.AddEdge(i + pointsCT.Count + 1, 1 + pointsCT.Count + pointsGT.Count, 1); // sink edges

        float matching; 
        int[,] graph;
        (matching, graph) = flow.FindMaximumFlow(0, 1 + pointsCT.Count + pointsGT.Count);
    
        float prec = pointsCT.Count > 0 ? matching / pointsCT.Count : 0;
        float recall = pointsGT.Count > 0 ? matching / pointsGT.Count : 0;
        return (prec, recall);
    }
}
