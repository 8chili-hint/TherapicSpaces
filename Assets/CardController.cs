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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this; 
           maxSelectionIndex = AllCardsAndThemes.Count - 1;
       
        SetInitialMenu();
      //  PushCurrentSelectionIndex.AddListener((arg0) => InjectCurrentSelection(arg0));

    }

 
    void SetInitialMenu()
    {
        foreach(var ab in AllCardsAndThemes)
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
        maxCurrentSubSelectionIndex = AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count-1;
        FindAnyObjectByType<Uicontroller>().UpdateMaxIndex(AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count);
        for(int i=0;i<AllCardsAndThemes[Index].CardDetails.Count ; i++)
        {
            SelectedCards.Add(AllCardsAndThemes[Index].CardDetails[i]);
        }


        foreach(var a in SelectedCards)
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
            maxCurrentSubSelectionIndex = AllCardsAndThemes[CurrentSelectionIndex].CardDetails.Count-1;
            ChangeSlectedCards(CurrentSelectionIndex);
            AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().enableThisEnv();
            Debug.LogError(AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].PrefabToInstantitate.name);
            return true;
        }

        else
        {

            AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().enableThisEnv();

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

            return true;
        }

        else
        {

            AllCardsAndThemes[CurrentSelectionIndex].CardDetails[CurrentSubSelectionIndex].nameText.transform.parent.GetComponent<Card>().enableThisEnv();

            return false;
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

