using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class RunLSS : MonoBehaviour
{
    public Transform dataParent;

    private string outpath; // path to streamingassets folder

    private MyPointCloudImporter pointCloudImporter;

    public int N = 128;
    public float kb = 0.05f;
    public float slope = -4;
    public int seed = 8;

    public void SetNFromSlider(float val)
    {
        N = (int)Mathf.Pow(2, (int)val);
    }
    public void SetKbFromSlider(float val)
    {
        kb = val;
    }
    public void SetSlopeFromSlider(float val)
    {
        slope = val;
    }
    public void SetSeedFromSlider(float val)
    {
        seed = (int)val;
    }

    private void Start()
    {
        pointCloudImporter = GetComponent<MyPointCloudImporter>();
        outpath = @"C:\Users\Demon\Documents\VizLab\LSSPython\Assets\StreamingAssets\";//Application.streamingAssetsPath;

        //pointCloudObject = pointCloudImporter.CreateNewMesh(outpath + "/LSSModel.txt");
        //meshFilter = pointCloudObject.GetComponent<MeshFilter>();
    }

    public void Run()
    {
        // Run python
        RunPython();
        // Load new data
        LoadData();
    }

    private void RunPython()
    {
        // Set up the process start info
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = @"C:\Users\Demon\AppData\Local\Microsoft\WindowsApps\python.exe";  // Path to the Python interpreter executable
        startInfo.Arguments = @"C:\Users\Demon\Documents\VizLab\LSSPython\Assets\Scripts\run_LSS.py";  // Path to your Python script
        startInfo.Arguments += string.Format(" {0} {1} {2} {3} {4}", N, kb, slope, seed, outpath);
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        // Create and start the process
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        // Read the output of the process
        string output = process.StandardOutput.ReadToEnd();

        // Wait for the process to exit
        process.WaitForExit();

        // Display the output
        UnityEngine.Debug.Log("Python Script Output: " + output);
    }

    private void LoadData()
    {
        foreach (Transform child in dataParent)
        {
            Destroy(child.gameObject);
        }
        GameObject gameObject = pointCloudImporter.CreateNewComputeBuffer(outpath + "LSSModel.txt");
        UnityEngine.Debug.Log("read new point cloud");
        gameObject.transform.SetParent(dataParent);
    }
}
