using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Better.StreamingAssets;
using System.Linq;
using System;
using UnityEngine.Scripting; // Add the BetterStreamingAssets namespace

public class Quest2AssetBundleLoader : MonoBehaviour
{
    public List<string> AssetBundleNames;
    public Vector3 spawnPoint;
    public GameObject PrevEnv { get; private set; }
    public GameObject CurrEnv { get; private set; }
    public GameObject NextEnv { get; private set; }
    private List<AssetBundle> loadedBundles = new List<AssetBundle>();
    private List<GameObject> LoadedEnvs = new();
    public List<string> AssetBundlePaths { get; private set; }

    private int CurrIndex = 0;
    private int PrevIndex = 0;
    private int NextIndex = 0;

    public GameObject FailedPrefab, DonePrefab;

    // Debug toggle
    public bool verbose = true;

    public static Quest2AssetBundleLoader Instance;

    public int globalIndex;

    public List<QueueStruct> Queue = new();

    void Awake()
    {
        Instance = this;
        AssetBundlePaths = new List<string>();
        if (spawnPoint == Vector3.zero)
        {
            spawnPoint = transform.position;
        }
        // Initialize Better Streaming Assets
        BetterStreamingAssets.Initialize();
    }

    void Start()
    {
        // Start the loading process
        StartCoroutine(GetAllFilePaths());
    }

    IEnumerator GetAllFilePaths()
    {
        yield return null;
        LogMessage("Starting Quest2AssetBundleLoader...");

        // Process each asset bundle name
        for (int i = 0; i < AssetBundleNames.Count; i++)
        {
            string bundleName = AssetBundleNames[i];
            string bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
            AssetBundlePaths.Add(bundlePath); // Store the file:// path

        }
    }

    IEnumerator LoadEnvironment(int index)
    {


        CurrIndex = index;
        string bundlePath = AssetBundlePaths[index];

        // Load the asset bundle
        LogMessage("Loading environment from: " + bundlePath);
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
        yield return request.SendWebRequest();

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        yield return null;

        if (bundle != null) // Changed to check if the bundle is valid
        {
            loadedBundles.Add(bundle);
            GameObject a = (GameObject)bundle.LoadAsset(AssetBundleNames[index]);
            if (a)
            {
                var b = Instantiate(a);
                b.transform.position = (Vector3.zero);
                b.SetActive(true);
                LoadedEnvs.Add(b);
                CameraFade.PitchToLight?.Invoke();
                Uicontroller.DisableRightStick = false;
            }
            else
            {
                Uicontroller.DisableRightStick = false;
                FindAnyObjectByType<Uicontroller>().OnJoystickRight();
            }
        }
        else
        {
            LogError("Failed to load environment: " + bundlePath);
            if (FailedPrefab) FailedPrefab.SetActive(true);
        }
    }
    public IEnumerator InitiateQueue(int index, int maxindex)
    {
        Queue.Clear();
        if (index == 0)
        {
            //forPreviousOne
            {

                QueueStruct a;
                a.GlobalIndex = index - 1;
                string bundlePath = AssetBundlePaths[a.GlobalIndex];
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
                yield return request.SendWebRequest();

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                yield return null;

                if (bundle != null) // Changed to check if the bundle is valid
                {
                    loadedBundles.Add(bundle);
                    GameObject assetBundlePrefab = (GameObject)bundle.LoadAsset(AssetBundleNames[index]);
                    a.PrefabInstantialted = Instantiate(assetBundlePrefab);
                    a.PrefabInstantialted.transform.position = (Vector3.zero);



                }
            }
            //ForSelected

            {

                QueueStruct a;
                a.GlobalIndex = index - 1;
                string bundlePath = AssetBundlePaths[a.GlobalIndex];
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
                yield return request.SendWebRequest();

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                yield return null;

                if (bundle != null) // Changed to check if the bundle is valid
                {
                    loadedBundles.Add(bundle);
                    GameObject assetBundlePrefab = (GameObject)bundle.LoadAsset(AssetBundleNames[index]);
                    a.PrefabInstantialted = Instantiate(assetBundlePrefab);
                    a.PrefabInstantialted.transform.position = (Vector3.zero);



                }
            }
        }

    }
   
    public void SwitchEnv(int GlobalIndex)
    {
     
        globalIndex = GlobalIndex;
      //  StartCoroutine(SwitchEnvironment(GlobalIndex));
    }
 
    IEnumerator SwitchEnvironment(int newIndex)
    {
        Uicontroller.DisableRightStick = true;
        // Clean up current environment
        UnloadEnvironments();

        // Load the new one
        yield return StartCoroutine(LoadEnvironment(newIndex));
    }

    public void UnloadEnvironments()
    {
        foreach(GameObject a in LoadedEnvs)
        {
            Destroy(a);
        }
        CurrEnv = null;
       
    }

    void OnDestroy()
    {
        UnloadEnvironments();
    }

    // Logging helpers
    void LogMessage(string message)
    {
        if (verbose) Debug.Log("[Quest2AssetBundleLoader] " + message);
    }

    void LogError(string message)
    {
        Debug.LogError("[Quest2AssetBundleLoader] " + message);
    }
}

public struct QueueStruct
{
    public AssetBundle bundle;
    public Index GlobalIndex;
    public GameObject PrefabInstantialted;
}