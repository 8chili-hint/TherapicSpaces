using UnityEngine;
using UnityEngine.UI;
public class Card : MonoBehaviour
{
   

    public Texture imagePng;
    public GameObject PrefabToInstantitate;
    public string name, subText;
    public TMPro.TextMeshProUGUI nameText, subNameText;
    public AudioClip audioClip;
    public int GlobalIndex;

    public void SetCard()
    {
        transform.GetChild(0).GetChild(0).
            GetComponent<RawImage>().texture = imagePng;
        nameText.text = name;
        subNameText.text = subText;
        GetComponent<Button>().onClick.AddListener(() => {

          //  StartCoroutine(Quest2AssetBundleLoader.Instance.InitiateQueue(GlobalIndex));
            Quest2AssetBundleLoader.Instance.SwitchEnv(GlobalIndex);
        });
        
       
    }

    public void disableThisEnv()
    {
       // PrefabToInstantitate.SetActive(false);
    }

    public void enableThisEnv()
    {
      //  PrefabToInstantitate.SetActive(true);
        AmbientAudioClipManager.Instance.PlayAudio(audioClip);
    }
}
