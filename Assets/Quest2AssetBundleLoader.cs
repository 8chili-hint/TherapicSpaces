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

    public void SwitchEnv(int newIndex,bool IsGoingPrev)
    {
        // if (newIndex == globalIndex) return;
      Uicontroller.Instance. SetActiveJoystick(true);

        previousIndex = globalIndex; // Store the previous index
        globalIndex = newIndex;
        StartCoroutine(UpdateEnvironmentQueue(IsGoingPrev));
    }

    IEnumerator UpdateEnvironmentQueue(bool IsGoingPrev)
    {
        LogMessage($"UpdateEnvironmentQueue started, newIndex: {globalIndex}, previousIndex: {previousIndex}");

        // Calculate indices for previous and next environments
        int prevIndex = (globalIndex - 1 + AssetBundleNames.Count) % AssetBundleNames.Count;
        int nextIndex = (globalIndex + 1) % AssetBundleNames.Count;

        // Ensure queue doesn't contain duplicates.
       // Queue.RemoveAll(item => item.GlobalIndex == globalIndex || item.GlobalIndex == prevIndex || item.GlobalIndex == nextIndex);


        // Load the new central environment
        yield return StartCoroutine(LoadEnvironment(globalIndex,true, IsGoingPrev));

        // Load the previous and next environments
        yield return StartCoroutine(LoadEnvironment(prevIndex, false, IsGoingPrev));
        yield return StartCoroutine(LoadEnvironment(nextIndex, false, IsGoingPrev));

        //Re-order Queue
        if (Queue.Count > 1)
        {
            QueueStruct[] tempArray = Queue.ToArray(); // Convert to array for easier manipulation.

            for (int i = 0; i < Queue.Count - 1; i++)
            {
                for (int j = i + 1; j < Queue.Count; j++)
                {
                    if (tempArray[i].GlobalIndex > tempArray[j].GlobalIndex)
                    {
                        // Swap elements
                        QueueStruct temp = tempArray[i];
                        tempArray[i] = tempArray[j];
                        tempArray[j] = temp;
                    }
                }
            }

            Queue.Clear(); // Clear the original queue.
            foreach (QueueStruct item in tempArray)
            {
                if(item.GlobalIndex == prevIndex)
                {
                    item.PrefabInstantialted.SetActive(false);
                Queue.Add(item); // Enqueue the sorted elements back into the queue.
                }
            }
            foreach (QueueStruct item in tempArray)
            {
                if (item.GlobalIndex == globalIndex)
                {
                    item.PrefabInstantialted.SetActive(true);

                    Queue.Add(item); // Enqueue the sorted elements back into the queue.
                }
            }
            foreach (QueueStruct item in tempArray)
            {
                if (item.GlobalIndex == nextIndex)
                {
                    item.PrefabInstantialted.SetActive(false);

                    Queue.Add(item); // Enqueue the sorted elements back into the queue.
                }
            }
        }
        LogMessage("Queue sorted");

        Uicontroller.Instance.SetActiveJoystick(false);
        CameraFade.PitchToLight?.Invoke();
    }

    IEnumerator LoadEnvironment(int index, bool enable, bool isprev)
    {
        LogMessage($"LoadEnvironment started for index: {index}");

        // Check if already in queue
        QueueStruct existing;
        foreach (var item in Queue)
        {
            if (item.GlobalIndex == index)
            {
                existing = item;
                if (existing.PrefabInstantialted)
                {
                  //  existing.PrefabInstantialted.SetActive(true);
                    yield break;
                }
                break; // Important: Exit the loop when found!
            }
            else
            {
                existing = item;
            }
        }
     


        // Queue shift (remove first if full)
        if (Queue.Count >= MaxQueueSize)
        {
            if (isprev)
            {
                LogMessage($"Queue is full, removing oldest entry");
                var toRemove = Queue[Queue.Count-1];
                Queue.RemoveAt(Queue.Count - 1);

                if (toRemove.PrefabInstantialted != null)
                {
                    Destroy(toRemove.PrefabInstantialted);
                }
                if (toRemove.bundle != null)
                {
                    toRemove.bundle.Unload(true);
                }
            }
            else
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
            //Falback To 1st
                path = AssetBundlePaths[0];
                LogMessage($"Loading AssetBundle from: {path}");
                 request = UnityWebRequestAssetBundle.GetAssetBundle(path);
                yield return request.SendWebRequest();

            }
         
        }

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        loadedBundles.Add(bundle);
        LogMessage($"AssetBundle loaded successfully.");

        AssetBundleRequest asyncRequest = bundle.LoadAssetAsync<GameObject>(AssetBundleNames[index]);
        yield return asyncRequest;

        GameObject prefab = asyncRequest.asset as GameObject;
        if (!prefab)
        {
            //Falback To 1st
            path = AssetBundlePaths[0];
            LogMessage($"Loading AssetBundle from: {path}");
            request = UnityWebRequestAssetBundle.GetAssetBundle(path);
            yield return request.SendWebRequest();


             bundle = DownloadHandlerAssetBundle.GetContent(request);
            loadedBundles.Add(bundle);
            LogMessage($"AssetBundle loaded successfully.");

             asyncRequest = bundle.LoadAssetAsync<GameObject>(AssetBundleNames[0]);
            yield return asyncRequest;

             prefab = asyncRequest.asset as GameObject;
            
        }

        // Instantiate on main thread
        GameObject instance = Instantiate(prefab, spawnPoint, Quaternion.identity);
        if (enable) 
        {
            instance.SetActive(true);
        } 
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
        LoadedEnvs.Clear();
        foreach (var obj in Queue)
        {
            Destroy(obj.PrefabInstantialted);
        }     
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
