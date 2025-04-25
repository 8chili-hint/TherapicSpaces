using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
public class CardController : MonoBehaviour
{

    public List<CardLinearStructure> AllCardsAndThemes;

    public List<CardFiller> SelectedCards;

    public int CurrentSelectionIndex, CurrentSubSelectionIndex;
    public int maxSelectionIndex = 6;
    public int maxSubSelectionIndex = 5;

    public static UnityEvent<int> PushCurrentSelectionIndex;
    public static UnityEvent<int> PushCurrentSubSelectionIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        for (int i = 0; i < AllCardsAndThemes.Count; i++)
        {
            int cardIndex = i; 
            AllCardsAndThemes[i].Button.onClick.AddListener(() => { ChangeSlectedCards(cardIndex); });
        }

      //  PushCurrentSelectionIndex.AddListener((arg0) => InjectCurrentSelection(arg0));

    }
    private void OnEnable()
    {

    }

    void ChangeSlectedCards(int Index)
    {
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
}

[System.Serializable]
public struct CardLinearStructure
{
    public Button Button;
    public Sprite imagePng;
    public string PriText, subText;
    public TMPro.TextMeshProUGUI nameText, subNameText;
    public List<CardFiller> CardDetails; 
}
[System.Serializable]

public struct CardFiller
{
    public Sprite imagePng;
    public string PriText, subText;
    public TMPro.TextMeshProUGUI nameText, subNameText;
    public GameObject PrefabToInstantitate;
}

