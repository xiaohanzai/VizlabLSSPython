using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MyPointCloudImporter : MonoBehaviour
{
    public Material pointCloudMaterial;
    public float scaling = 5;

    public List<Vector3> GetVertices(string filename)
    {
        List<Vector3> vertices = new List<Vector3>();

        using (StreamReader reader = new StreamReader(filename))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] elements = line.Split(' ');
                vertices.Add(new Vector3(float.Parse(elements[0]) * scaling, float.Parse(elements[1]) * scaling, float.Parse(elements[2]) * scaling));
            }
        }

        return vertices;
    }

    public List<Color32> GetColors(string filename)
    {
        List<Color32> colors = new List<Color32>();

        using (StreamReader reader = new StreamReader(filename))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] elements = line.Split(' ');
                Color32 color = new Color(float.Parse(elements[3]), float.Parse(elements[4]), float.Parse(elements[5]));
                colors.Add(color);
            }
        }

        return colors;
    }

    public GameObject CreateNewMesh(string filename)
    {
        List<Vector3> vertices = GetVertices(filename);
        List<Color32> colors = GetColors(filename);

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);

        mesh.SetIndices(
                    Enumerable.Range(0, vertices.Count).ToArray(),
                    MeshTopology.Points, 0
                );

        mesh.UploadMeshData(true);

        GameObject pointCloudObject = new GameObject("PointCloud");
        pointCloudObject.transform.position = Vector3.zero;
        pointCloudObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        pointCloudObject.AddComponent<MeshRenderer>().sharedMaterial = pointCloudMaterial;

        vertices = null;
        colors = null;

        return pointCloudObject;
    }

    static uint EncodeColor(Color c)
    {
        const float kMaxBrightness = 16;

        var y = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
        y = Mathf.Clamp(Mathf.Ceil(y * 255 / kMaxBrightness), 1, 255);

        var rgb = new Vector3(c.r, c.g, c.b);
        rgb *= 255 * 255 / (y * kMaxBrightness);

        return ((uint)rgb.x) |
                ((uint)rgb.y << 8) |
                ((uint)rgb.z << 16) |
                ((uint)y << 24);
    }
    struct Point
    {
        public Vector3 position;
        public uint color;
    }
    Point[] _pointData;
    public Shader pointShader, diskShader;
    private void Initialize(List<Vector3> positions, List<Color32> colors)
    {
        _pointData = new Point[positions.Count];
        for (var i = 0; i < _pointData.Length; i++)
        {
            _pointData[i] = new Point
            {
                position = positions[i],
                color = EncodeColor(colors[i])
            };
        }
    }
    public GameObject CreateNewComputeBuffer(string filename)
    {
        List<Vector3> vertices = GetVertices(filename);
        List<Color32> colors = GetColors(filename);
        var data = ScriptableObject.CreateInstance<MyPointCloudData>();
        data.Initialize(vertices, colors);
        GameObject gameObject = new GameObject();
        MyPointCloudRenderer renderer = gameObject.AddComponent<MyPointCloudRenderer>();
        renderer.InitShader(pointShader, diskShader);
        renderer.sourceData = data;
        //Initialize(vertices, colors);
        //ComputeBuffer pointBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 4); // Assumes Vector3 positions
        //pointBuffer.SetData(_pointData);
        //Material material = new Material(shader);
        //material.SetPass(0);
        //Graphics.DrawProceduralNow(MeshTopology.Points, vertices.Count);
        return gameObject;
    }
}
