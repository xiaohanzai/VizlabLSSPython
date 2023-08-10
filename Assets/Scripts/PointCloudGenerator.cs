using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.VFX;

public class PointCloudGenerator : MonoBehaviour
{
    public VisualEffect vfx;
    uint resolution = 2048;
    public float particleSize = 0.05f;
    public float scale = 5;

    public void GeneratePointCloud(string filename)
    {
        vfx.Reinit();
        List<Vector3> positions = GetVertices(filename);
        List<Color> colors = GetColors(filename);
        SetParticles(positions, colors, vfx);
        positions = null;
        colors = null;
    }

    public List<Vector3> GetVertices(string filename)
    {
        List<Vector3> vertices = new List<Vector3>();

        using (StreamReader reader = new StreamReader(filename))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] elements = line.Split(' ');
                vertices.Add(new Vector3(float.Parse(elements[0]), float.Parse(elements[1]), float.Parse(elements[2])));
            }
        }

        return vertices;
    }

    public List<Color> GetColors(string filename)
    {
        List<Color> colors = new List<Color>();

        using (StreamReader reader = new StreamReader(filename))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] elements = line.Split(' ');
                Color color = new Color(float.Parse(elements[3]), float.Parse(elements[4]), float.Parse(elements[5]));
                colors.Add(color);
            }
        }

        return colors;
    }

    private void SetParticles(List<Vector3> positions, List<Color> colors, VisualEffect vfx)
    {
        Texture2D texColor = new Texture2D(positions.Count > (int)resolution ? (int)resolution : positions.Count, Mathf.Clamp(positions.Count / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
        Texture2D texPosScale = new Texture2D(positions.Count > (int)resolution ? (int)resolution : positions.Count, Mathf.Clamp(positions.Count / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
        int texWidth = texColor.width;
        int texHeight = texColor.height;

        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                int index = x + y * texWidth;
                texColor.SetPixel(x, y, colors[index]);
                var data = new Color(positions[index].x * scale, positions[index].y * scale, positions[index].z * scale, particleSize);
                texPosScale.SetPixel(x, y, data);
            }
        }

        texColor.Apply();
        texPosScale.Apply();

        vfx.SetUInt(Shader.PropertyToID("ParticleCount"), (uint)positions.Count);
        vfx.SetTexture(Shader.PropertyToID("TexColor"), texColor);
        vfx.SetTexture(Shader.PropertyToID("TexPosScale"), texPosScale);
        vfx.SetUInt(Shader.PropertyToID("Resolution"), resolution);
    }
}
