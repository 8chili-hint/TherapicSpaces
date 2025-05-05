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

    public void ResetThisBehaviour()
    {
        SelectedCards.Clear();
        CurrentSelectionIndex = 0;
        CurrentSubSelectionIndex = 0; 
        maxSelectionIndex = 0;
        maxCurrentSubSelectionIndex = 0;
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
      /*  CurrentSubSelectionIndex = 0;*/
      //  maxCurrentSubSelectionIndex = AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count - 1;
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
            Card.GlobalIndex = a.GlobalIndex;
            Card.SetCard();
        }

       
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
     
       //     CameraFade.PitchToDark?.Invoke();

        CurrentSubSelectionIndex++;

        if (CurrentSubSelectionIndex > maxCurrentSubSelectionIndex)
        {
            CurrentSelectionIndex = FindAnyObjectByType<Uicontroller>().IncrementMainUIIndex(CurrentSelectionIndex);
            CurrentSubSelectionIndex = 0;
            maxCurrentSubSelectionIndex = AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count-1 ;
            ChangeSlectedCards(CurrentSelectionIndex);
            Quest2AssetBundleLoader.Instance.SwitchEnv(AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().GlobalIndex);
          

            return true;
        }
        else
        {
            Quest2AssetBundleLoader.Instance.SwitchEnv(AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().GlobalIndex);

            return false;
        }


    }

    public bool LoadPrevEnv()
    {
     //   CameraFade.PitchToDark?.Invoke();

        CurrentSubSelectionIndex--;

        if (CurrentSubSelectionIndex < 0)
        {
            CurrentSelectionIndex = FindAnyObjectByType<Uicontroller>().decrementMainUIIndex(CurrentSelectionIndex);
            maxCurrentSubSelectionIndex = AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count-1;
            CurrentSubSelectionIndex = maxCurrentSubSelectionIndex;
            ChangeSlectedCards(CurrentSelectionIndex);

             Quest2AssetBundleLoader.Instance.SwitchEnv( AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().GlobalIndex);


            return true;
        }
        else
        {


            Quest2AssetBundleLoader.Instance.SwitchEnv(AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().GlobalIndex);



            return false;
        }

        //return true;

    }

    // ENVIRONMENT POOLING SYSTEM

    // Get an environment from the pool or instantiate a new one


    // Return an environment to the pool (deactivate and queue)

    // Get the environment prefab for a specific index


    // Manage which environments should be active (current, next, previous)

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
    public int GlobalIndex;
}