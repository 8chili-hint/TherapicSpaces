using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Quest2AssetBundleLoader : MonoBehaviour
{
    public List<string> AssetBundleNames;
    public Vector3 spawnPoint;
    public GameObject FailedPrefab;

    public List<GameObject> LoadedEnvs = new();
    public List<QueueStruct> Queue = new();
    private List<AssetBundle> loadedBundles = new();

    public static Quest2AssetBundleLoader Instance;

    private int globalIndex = 0;
    private const int MaxQueueSize = 3; // Keep this at 3
    private int previousIndex = -1;

    public bool verbose = true;

    public List<string> AssetBundlePaths { get; private set; }

    void Awake()
    {
        Instance = this;
        AssetBundlePaths = new List<string>();
        if (spawnPoint == Vector3.zero)
            spawnPoint = transform.position;
        BetterStreamingAssets.Initialize();
    }

    void Start()
    {
        StartCoroutine(GetAllFilePaths());
    }

    IEnumerator GetAllFilePaths()
    {
        yield return null;
        LogMessage("Starting Quest2AssetBundleLoader...");
        for (int i = 0; i < AssetBundleNames.Count; i++)
        {
            string bundlePath = Path.Combine(Application.streamingAssetsPath, AssetBundleNames[i]);
            AssetBundlePaths.Add(bundlePath);
        }
        //yield return StartCoroutine(LoadEnvironment(0)); // Load initial index
    }

    public void SwitchEnv(int newIndex)
    {
       // if (newIndex == globalIndex) return;

        previousIndex = globalIndex; // Store the previous index
        globalIndex = newIndex;
        StartCoroutine(UpdateEnvironmentQueue());
    }

    IEnumerator UpdateEnvironmentQueue()
    {
        LogMessage($"UpdateEnvironmentQueue started, newIndex: {globalIndex}, previousIndex: {previousIndex}");

        // Calculate indices for previous and next environments
        int prevIndex = (globalIndex - 1 + AssetBundleNames.Count) % AssetBundleNames.Count;
        int nextIndex = (globalIndex + 1) % AssetBundleNames.Count;

        // Ensure queue doesn't contain duplicates.
        Queue.RemoveAll(item => item.GlobalIndex == globalIndex || item.GlobalIndex == prevIndex || item.GlobalIndex == nextIndex);


        // Load the new central environment
        yield return StartCoroutine(LoadEnvironment(globalIndex));

        // Load the previous and next environments
        yield return StartCoroutine(LoadEnvironment(prevIndex));
        yield return StartCoroutine(LoadEnvironment(nextIndex));

        //Re-order Queue
        Queue.Sort((a, b) =>
        {
            if (a.GlobalIndex == globalIndex) return -1;
            if (b.GlobalIndex == globalIndex) return 1;
            if (a.GlobalIndex == prevIndex) return -1;
            if (b.GlobalIndex == prevIndex) return 1;
            return 0;
        });
        LogMessage("Queue sorted");

        // Deactivate environments that are not the current or adjacent
        foreach (var env in Queue)
        {
            if (env.PrefabInstantialted != null && env.GlobalIndex != globalIndex && env.GlobalIndex != prevIndex && env.GlobalIndex != nextIndex)
            {
                env.PrefabInstantialted.SetActive(false);
            }
            else if (env.PrefabInstantialted != null)
            {
                env.PrefabInstantialted.SetActive(true); //make the current and adjacent active
            }
        }
    }

    IEnumerator LoadEnvironment(int index)
    {
        LogMessage($"LoadEnvironment started for index: {index}");

        // Check if already in queue
        QueueStruct? existing = null;
        foreach (var item in Queue)
        {
            if (item.GlobalIndex == index)
            {
                existing = item;
                break; // Important: Exit the loop when found!
            }
        }

        if (existing.HasValue)
        {
            LogMessage($"Environment found in queue, index: {index}");
            // ... (Code to show the existing environment)
            yield break;
        }
        // Queue shift (remove first if full)
        if (Queue.Count >= MaxQueueSize)
        {
            LogMessage($"Queue is full, removing oldest entry");
            var toRemove = Queue[0];
            Queue.RemoveAt(0);

            if (toRemove.PrefabInstantialted != null)
            {
                Destroy(toRemove.PrefabInstantialted);
            }
            if (toRemove.bundle != null)
            {
                toRemove.bundle.Unload(true);
            }
        }

        // Async Load
        string path = AssetBundlePaths[index];
        LogMessage($"Loading AssetBundle from: {path}");
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            LogError("Failed to load AssetBundle at: " + path + " Error: " + request.error);
            if (FailedPrefab)
            {
                GameObject failedInstance = Instantiate(FailedPrefab, spawnPoint, Quaternion.identity);
                yield return new WaitForSeconds(5);
                Destroy(failedInstance);
            }
            yield break;
        }

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        loadedBundles.Add(bundle);
        LogMessage($"AssetBundle loaded successfully.");

        AssetBundleRequest asyncRequest = bundle.LoadAssetAsync<GameObject>(AssetBundleNames[index]);
        yield return asyncRequest;

        GameObject prefab = asyncRequest.asset as GameObject;
        if (!prefab)
        {
            LogError("Failed to load prefab from bundle: " + AssetBundleNames[index]);
            yield break;
        }

        // Instantiate on main thread
        GameObject instance = Instantiate(prefab, spawnPoint, Quaternion.identity);
        instance.SetActive(true);
        LoadedEnvs.Add(instance);


        Queue.Add(new QueueStruct
        {
            GlobalIndex = index,
            bundle = bundle,
            PrefabInstantialted = instance
        });

        LogMessage("Environment Loaded and Instantiated: " + index);
    }

    public void UnloadEnvironments()
    {
        foreach (GameObject obj in LoadedEnvs)
        {
            Destroy(obj);
        }
        LoadedEnvs.Clear();
        Queue.Clear();
    }

    void OnDestroy()
    {
        UnloadEnvironments();
        foreach (var bundle in loadedBundles)
        {
            bundle?.Unload(true);
        }
        loadedBundles.Clear();
    }

    void LogMessage(string msg)
    {
        if (verbose) Debug.Log("[Quest2AssetBundleLoader] " + msg);
    }

    void LogError(string msg)
    {
        Debug.LogError("[Quest2AssetBundleLoader] " + msg);
    }
}

[Serializable]
public struct QueueStruct
{
    public AssetBundle bundle;
    public int GlobalIndex;
    public GameObject PrefabInstantialted;
}
