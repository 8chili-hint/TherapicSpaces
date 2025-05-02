using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class Quest2AssetBundleLoader : MonoBehaviour
{
    public List<string> AssetBundleNames;
    public Vector3 spawnPoint;
    public GameObject PrevEnv { get; private set; }
    public GameObject CurrEnv { get; private set; }
    public GameObject NextEnv { get; private set; }
    private List<AssetBundle> loadedBundles = new List<AssetBundle>();
    public List<string> AssetBundlePaths { get; private set; }

    private int CurrIndex = 0;
    private int PrevIndex = 0;
    private int NextIndex = 0;

    public GameObject FailedPrefab, DonePrefab;

    // Debug toggle
    public bool verbose = true;

    void Awake()
    {
        AssetBundlePaths = new List<string>();
        if (spawnPoint == Vector3.zero)
        {
            spawnPoint = transform.position;
        }
    }

    void Start()
    {
        // Start the loading process
        StartCoroutine(LoadAssetBundles());
    }

    IEnumerator LoadAssetBundles()
    {
        LogMessage("Starting Quest2AssetBundleLoader...");

        // Process each asset bundle name
        for (int i = 0; i < AssetBundleNames.Count; i++)
        {
            string bundleName = AssetBundleNames[i];
            // Use the proper path format for Android/Quest
            string bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
            string androidPath = "file://" + bundlePath; // Important:  Use file:// for Android!

            LogMessage("Trying to load bundle: " + androidPath);
            AssetBundlePaths.Add(androidPath); // Store the file:// path

            // Try to load the asset bundle
           /* UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(androidPath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LogMessage("✓ Successfully yielded assetbundle: " + bundleName);

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                if (bundle != null)
                {
                    LogMessage("✓ Successfully loaded bundle: " + bundleName);
                }
                else
                {
                    LogError("Bundle loaded but is null: " + bundleName);
                    if (FailedPrefab) FailedPrefab.SetActive(true);
                }
                if (bundle != null)
                {
                    bundle.Unload(false);
                }
            }
            else
            {
                LogError("Failed to load bundle: " + bundleName + " - Error: " + request.error);
                if (FailedPrefab) FailedPrefab.SetActive(true);
            }*/
        }

        // Report results
        LogMessage("Found " + AssetBundlePaths.Count + " out of " + AssetBundleNames.Count + " asset bundles");

        // Load the first environment if we found any bundles
        if (AssetBundlePaths.Count > 0)
        {
            yield return StartCoroutine(LoadEnvironment(13));
        }
        else
        {
            LogError("No asset bundles could be loaded!");
            if (FailedPrefab) FailedPrefab.SetActive(true);
        }
    }

    IEnumerator LoadEnvironment(int index)
    {
        if (index < 0 || index >= AssetBundlePaths.Count)
        {
            LogError("Invalid environment index: " + index);
            if (FailedPrefab) FailedPrefab.SetActive(true);
            yield break;
        }

        CurrIndex = index;
        string bundlePath = AssetBundlePaths[index];

        // Load the asset bundle
        LogMessage("Loading environment from: " + bundlePath);
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle != null)
            {
                loadedBundles.Add(bundle);
                LogMessage("Asset bundle loaded successfully");

                // Get asset names
                string[] assetNames = bundle.GetAllAssetNames();
                LogMessage("Assets in bundle: " + string.Join(", ", assetNames));

                if (assetNames.Length > 0)
                {
                    // Load the first asset
                    string assetPath = assetNames[0];
                    ResourceRequest assetRequest = bundle.LoadAssetAsync<GameObject>(assetPath);
                    yield return assetRequest;
                    GameObject prefab = (GameObject)assetRequest.asset;


                    if (prefab != null)
                    {
                        // Instantiate the environment
                        CurrEnv = Instantiate(prefab, spawnPoint, Quaternion.identity);
                        CurrEnv.SetActive(true);
                        LogMessage("Environment loaded and instantiated successfully!");
                        if (DonePrefab) DonePrefab.SetActive(true);
                    }
                    else
                    {
                        LogError("Failed to load asset from bundle: " + assetPath);
                        if (FailedPrefab) FailedPrefab.SetActive(true);
                    }
                }
                else
                {
                    LogError("No assets found in bundle");
                    if (FailedPrefab) FailedPrefab.SetActive(true);
                }
            }
            else
            {
                LogError("Asset bundle content is null");
                if (FailedPrefab) FailedPrefab.SetActive(true);
            }
        }
        else
        {
            LogError("Failed to load environment: " + request.error);
            if (FailedPrefab) FailedPrefab.SetActive(true);
        }
    }

    public void MoveToNextEnvironment()
    {
        int nextIndex = (CurrIndex + 1) % AssetBundlePaths.Count;
        StartCoroutine(SwitchEnvironment(nextIndex));
    }

    public void MoveToPrevEnvironment()
    {
        int prevIndex = (CurrIndex - 1 + AssetBundlePaths.Count) % AssetBundlePaths.Count;
        StartCoroutine(SwitchEnvironment(prevIndex));
    }

    IEnumerator SwitchEnvironment(int newIndex)
    {
        // Clean up current environment
        UnloadEnvironments();

        // Load the new one
        yield return StartCoroutine(LoadEnvironment(newIndex));
    }

    void UnloadEnvironments()
    {
        // Destroy instantiated objects
        if (PrevEnv) Destroy(PrevEnv);
        if (CurrEnv) Destroy(CurrEnv);
        if (NextEnv) Destroy(NextEnv);

        PrevEnv = null;
        CurrEnv = null;
        NextEnv = null;

        // Unload all bundles
        foreach (AssetBundle bundle in loadedBundles)
        {
            if (bundle != null)
            {
                bundle.Unload(true);
            }
        }
        loadedBundles.Clear();
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
