using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

/// <summary>
/// Structure that represents a road graph with intersection and roads with possible different directedness (directed/undirected).
/// </summary>
public class Map
{
    public string name;
    public List<Intersection> intersections =  new List<Intersection>();
    public List<Road> roads = new List<Road>();

    public float length;

    float x = 0;
    float y = 0;
    float width = 0;
    float height = 0;

    public Map(string name)
    {
        this.name = name;
    }

    /// <summary>
    /// Updates enclosing bounds and calculate total road length.
    /// </summary>
    public void Refresh()
    {
        UpdateBounds();
        CalcRoadLength();
    }

    public void CalcRoadLength()
    {
        this.length = 0;
        foreach (Road road in roads) this.length += road.length;
    }

    public void UpdateBounds()
    {
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        foreach(Intersection i in intersections)
        {
            if (i.position.X < minX) minX = i.position.X;
            if (i.position.X > maxX) maxX = i.position.X;
            if (i.position.Z < minY) minY = i.position.Z;
            if (i.position.Z > maxY) maxY = i.position.Z;
        }

        x = minX;
        y = minY;
        width = maxX - minX;
        height = maxY - minY;
    }

    /// <summary>
    /// Prunes away sections of the map that are outside of the a provided bounding box. Can be used for a smaller map.
    /// </summary>
    /// <param name="b">The rectangle of the map we want to keep.</param>
    public void SetBounds(float x, float y, float width, float height)
    {
        float minX = x, maxX = minX + width;
        float minY = y, maxY = minY + height;

        List<Road> redundantRoads = roads.FindAll(y => y.roadPoints.Any(x => x.X < minX || x.X > maxX || x.Z < minY || x.Z > maxY));

        RemoveRoads(redundantRoads);

        Refresh();
    }

    /// <summary>
    /// Removes roads from the map.
    /// </summary>
    /// <param name="trashRoads">Roads that we want to delete from our map.</param>
    public void RemoveRoads(List<Road> trashRoads)
    {
        foreach (Road road in trashRoads)
        {
            roads.Remove(road);

            if (!roads.Any(r => r.start == road.start || r.end == road.start))
                intersections.Remove(road.start);
            if (!roads.Any(r => r.start == road.end || r.end == road.end))
                intersections.Remove(road.end);
        }

        foreach (Intersection intersect in intersections)
        {
            if (intersect.outgoing != null)
            {
                intersect.outgoing.RemoveAll(y => trashRoads.Contains(y));
            }
        }
    }

    /// <summary>
    /// Get center point of map, usefull for moving the camera.
    /// </summary>
    /// <returns>The center of the bounding box in which the map is contained.</returns>
    public Vector3 GetCenter()
    {
        return new Vector3(x + 0.5f * width, 0, y + 0.5f * height);
    }    

    /// <summary>
    /// Get coordinate withing the map that is the closest to the provided point.
    /// </summary>
    /// <param name="pos">The point for which we are trying to find the closes point to in the map.</param>
    /// <returns>A coordinate of the closest point on the map.</returns>
    public Coordinate GetClosestPoint(Vector3 pos)
    {
        Coordinate shortest = roads[0].GetNearestCoordinate(pos);

        foreach (Road road in roads)
        {
            Coordinate current = road.GetNearestCoordinate(pos);

            if(Vector3.Distance(current.location, pos) < Vector3.Distance(shortest.location, pos))
            {
                shortest = current;
            }
        }

        return shortest;
    }
}