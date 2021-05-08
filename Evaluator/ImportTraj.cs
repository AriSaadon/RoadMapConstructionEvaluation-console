using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

public class ImportTraj
{
    /// <summary>
    /// Import a single trajectory defined in a trip_x file.
    /// </summary>
    /// <param name="map">The name of the folder in the Data folder in which the trajectories are located.</param>
    /// <param name="index">The index of the trajectory that will be imported.</param>
    /// <returns>A list of points representing the trajectory.</returns>
    public Vector3[] Import(string map, int index)
    {
        string[] lines = File.ReadAllLines($"../Data/{map}/trip_{index}.txt");
        Vector3[] trajectory = new Vector3[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string[] sample = lines[i].Split(' ');
            trajectory[i] = new Vector3(float.Parse(sample[0], CultureInfo.InvariantCulture), 0, float.Parse(sample[1], CultureInfo.InvariantCulture));
        }

        return trajectory;
    }
}
