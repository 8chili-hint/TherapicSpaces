using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class CardController : MonoBehaviour
{
    public static CardController Instance;
    public List<CardLinearStructure> AllCardsAndThemes;

    public List<CardFiller> SelectedCards;

    public int CurrentSelectionIndex, CurrentSubSelectionIndex;
    public int maxSelectionIndex;
    public int maxCurrentSubSelectionIndex;

    public static UnityEvent<int> PushCurrentSelectionIndex;
    public static UnityEvent<int> PushCurrentSubSelectionIndex;

    // Pool of environment instances
    private Dictionary<string, Queue<GameObject>> envPool = new Dictionary<string, Queue<GameObject>>();
    // Currently active environments
    private List<GameObject> activeEnvironments = new List<GameObject>();
    // Max number of environments to keep loaded at a time (current + next + previous)
    private const int MAX_ACTIVE_ENVIRONMENTS = 3;

    private void Awake()
    {
        Instance = this;
        maxSelectionIndex = AllCardsAndThemes.Count - 1;

        SetInitialMenu();
    }

    void SetInitialMenu()
    {
        foreach (var ab in AllCardsAndThemes)
        {
            ab.Button.transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = ab.imagePng;
            ab.nameText.text = ab.PriText;
            ab.subNameText.text = ab.subText;
        }
    }

    public void ChangeSlectedCards(int Index)
    {
        CurrentSelectionIndex = Index;
        CurrentSubSelectionIndex = 0;
        maxCurrentSubSelectionIndex = AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count - 1;
        FindAnyObjectByType<Uicontroller>().UpdateMaxIndex(AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count);

        SelectedCards.Clear(); // Clear previous selected cards

        for (int i = 0; i < AllCardsAndThemes[Index].CardDetails.Count; i++)
        {
            SelectedCards.Add(AllCardsAndThemes[Index].CardDetails[i]);
        }

        foreach (var a in SelectedCards)
        {
            var Card = a.nameText.transform.parent.GetComponent<Card>();
            Debug.LogError(Card.name);
            if (a.imagePng)
            {
                Card.imagePng = a.imagePng;
            }
            if (a.PrefabToInstantitate)
            {
                Card.PrefabToInstantitate = a.PrefabToInstantitate;
            }
            Card.name = a.PriText;
            Card.subText = a.subText;
            Card.nameText = a.nameText;
            Card.subNameText = a.subNameText;
            Card.audioClip = a.audioClip;
            Card.enabled = true;
            Card.SetCard();
        }

        // When changing selection, manage environment pool
        ManageEnvironmentPool();
    }

    void InjectCurrentSelection(int i)
    {
        CurrentSelectionIndex = i;
    }
    void InjectCurrentSubSelection(int i)
    {
        CurrentSubSelectionIndex = i;
    }

    public bool LoadNextEnv()
    {
        AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().disableThisEnv();
        CurrentSubSelectionIndex++;

        if (CurrentSubSelectionIndex > maxCurrentSubSelectionIndex)
        {
            CurrentSelectionIndex = FindAnyObjectByType<Uicontroller>().IncrementMainUIIndex(CurrentSelectionIndex);
            CurrentSubSelectionIndex = 0;
            maxCurrentSubSelectionIndex = AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count - 1;
            ChangeSlectedCards(CurrentSelectionIndex);
            AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().enableThisEnv();
            Debug.LogError(AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].PrefabToInstantitate.name);

            // Update environment pool when changing main selection
            ManageEnvironmentPool();
            return true;
        }
        else
        {
            AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().enableThisEnv();

            // Update environment pool when changing sub-selection
            ManageEnvironmentPool();
            return false;
        }
    }

    public bool LoadPrevEnv()
    {
        AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().disableThisEnv();
        CurrentSubSelectionIndex--;

        if (CurrentSubSelectionIndex < 0)
        {
            CurrentSelectionIndex = FindAnyObjectByType<Uicontroller>().decrementMainUIIndex(CurrentSelectionIndex);
            maxCurrentSubSelectionIndex = AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count - 1;
            CurrentSubSelectionIndex = maxCurrentSubSelectionIndex;
            ChangeSlectedCards(CurrentSelectionIndex);

            AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().enableThisEnv();

            // Update environment pool when changing main selection
            ManageEnvironmentPool();
            return true;
        }
        else
        {
            AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().enableThisEnv();

            // Update environment pool when changing sub-selection
            ManageEnvironmentPool();
            return false;
        }
    }

    // ENVIRONMENT POOLING SYSTEM

    // Get an environment from the pool or instantiate a new one
    private GameObject GetEnvironmentFromPool(GameObject prefab)
    {
        string prefabName = prefab.name;

        // If this type doesn't have a pool queue yet, create one
        if (!envPool.ContainsKey(prefabName))
        {
            envPool[prefabName] = new Queue<GameObject>();
        }

        // Check if we have an available instance in the pool
        if (envPool[prefabName].Count > 0)
        {
            GameObject pooledEnv = envPool[prefabName].Dequeue();
            pooledEnv.SetActive(true);
            return pooledEnv;
        }

        // If no pooled instance is available, instantiate a new one
        GameObject newEnv = Instantiate(prefab);
        return newEnv;
    }

    // Return an environment to the pool (deactivate and queue)
    private void ReturnEnvironmentToPool(GameObject env)
    {
        if (env == null) return;

        string prefabName = env.name.Replace("(Clone)", "").Trim();

        // Make sure we have a queue for this prefab type
        if (!envPool.ContainsKey(prefabName))
        {
            envPool[prefabName] = new Queue<GameObject>();
        }

        // Deactivate and add to pool
        env.SetActive(false);
        envPool[prefabName].Enqueue(env);

        // Remove from active environments
        if (activeEnvironments.Contains(env))
        {
            activeEnvironments.Remove(env);
        }
    }

    // Get the environment prefab for a specific index
    private GameObject GetEnvironmentPrefabAt(int selectionIndex, int subSelectionIndex)
    {
        // Safety checks
        if (selectionIndex < 0 || selectionIndex >= AllCardsAndThemes.Count) return null;
        if (subSelectionIndex < 0 || subSelectionIndex >= AllCardsAndThemes[selectionIndex].CardDetails.Count) return null;

        return AllCardsAndThemes[selectionIndex].CardDetails[subSelectionIndex].PrefabToInstantitate;
    }

    // Manage which environments should be active (current, next, previous)
    private void ManageEnvironmentPool()
    {
        // Step 1: Determine which environments should be active
        List<(int selIndex, int subIndex)> envsToLoad = new List<(int, int)>();

        // Current environment
        envsToLoad.Add((CurrentSelectionIndex, CurrentSubSelectionIndex));

        // Next environment
        int nextSubIndex = CurrentSubSelectionIndex + 1;
        int nextSelIndex = CurrentSelectionIndex;

        if (nextSubIndex > maxCurrentSubSelectionIndex)
        {
            nextSelIndex = (CurrentSelectionIndex + 1) % AllCardsAndThemes.Count;
            nextSubIndex = 0;
        }

        envsToLoad.Add((nextSelIndex, nextSubIndex));

        // Previous environment
        int prevSubIndex = CurrentSubSelectionIndex - 1;
        int prevSelIndex = CurrentSelectionIndex;

        if (prevSubIndex < 0)
        {
            prevSelIndex = (CurrentSelectionIndex - 1 + AllCardsAndThemes.Count) % AllCardsAndThemes.Count;
            prevSubIndex = AllCardsAndThemes[prevSelIndex].CardDetails.Count - 1;
        }

        envsToLoad.Add((prevSelIndex, prevSubIndex));

        // Step 2: Return all current active environments to pool
        foreach (var env in activeEnvironments.ToList())
        {
            ReturnEnvironmentToPool(env);
        }

        activeEnvironments.Clear();

        // Step 3: Load and activate the needed environments
        foreach (var (selIndex, subIndex) in envsToLoad)
        {
            // Get the prefab to instantiate
            GameObject prefab = GetEnvironmentPrefabAt(selIndex, subIndex);
            if (prefab == null) continue;

            // Get from pool or instantiate
            GameObject instance = GetEnvironmentFromPool(prefab);

            // Only activate the current environment - keep others ready but inactive
            if (selIndex == CurrentSelectionIndex && subIndex == CurrentSubSelectionIndex)
            {
                instance.SetActive(true);
            }
            else
            {
                instance.SetActive(false);
            }

            activeEnvironments.Add(instance);
        }
    }
}

[System.Serializable]
public struct CardLinearStructure
{
    public Button Button;
    public Texture imagePng;
    public string PriText, subText;
    public TMPro.TextMeshProUGUI nameText, subNameText;
    public List<CardFiller> CardDetails;
}

[System.Serializable]
public struct CardFiller
{
    public Texture imagePng;
    public string PriText, subText;
    public TMPro.TextMeshProUGUI nameText, subNameText;
    public GameObject PrefabToInstantitate;
    public AudioClip audioClip;
}