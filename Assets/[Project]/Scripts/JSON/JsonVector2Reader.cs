using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public static class JsonVector2Reader
{
    [System.Serializable]
    public class Path
    {
        public string name;
        public float area;
        public List<List<float>> points;
        public List<float> centroid;

        public List<Vector2> GetVectors()
        {
            return points.Select(p => new Vector2(p[0], p[1])).ToList();
        }
    }

    [System.Serializable]
    public class Layer
    {
        public string name;
        public int zOrderPosition;
        public List<Path> paths;
    }

    [System.Serializable]
    public class RootObject
    {
        public List<Layer> layers;
    }

    public static List<Vector2> GetVector2ListFromJson(string json)
    {
        RootObject root = JsonConvert.DeserializeObject<RootObject>(json);

        List<Vector2> vectors = new List<Vector2>();
        foreach (Layer layer in root.layers)
        {
            foreach (Path path in layer.paths)
            {
                vectors.AddRange(path.GetVectors());
            }
        }

        return vectors;
    }
}
