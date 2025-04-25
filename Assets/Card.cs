using UnityEngine;
using UnityEngine.UI;
public class Card : MonoBehaviour
{
   

    public Sprite imagePng;
    public GameObject PrefabToInstantitate;
    public string name, subText;
    public TMPro.TextMeshProUGUI nameText, subNameText;

    private void Awake()
    {
     
    }

    public void SetCard()
    {
        GetComponent<Image>().sprite = imagePng;
        nameText.text = name;
        subNameText.text = subText;
        GetComponent<Button>().onClick.AddListener(() => {
            var a = Instantiate(PrefabToInstantitate);
            a.SetActive(true);
            a.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        });

       
    }
}
