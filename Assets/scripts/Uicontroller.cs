using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class Uicontroller : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject SubPanel;
    public GameObject OceanEnv;

    public bool isMenuLooping;
    public int menuIndex;

    private int maxIndex;
    public List<Button> SelectionMenuPanelButtons;

    public List<Button> SubPanels;

    public UnityEvent OnMenuItemSelected,OnMenuItemHover;

    public bool menuSelected;
   public bool EnteredScene;

    public bool LoadPrevScene;
    public  bool DisableRightStick;

    public static Uicontroller Instance;
    private void Awake()
    {
        Instance = this;
        maxIndex =  SelectionMenuPanelButtons.Count;
        menuIndex = 0;
    }
   
    public void SetActiveJoystick(bool a)
    {
        DisableRightStick = a;
        Debug.Log("Value of RightStick is " + DisableRightStick);
    }
    private void Start()
    {
      

        foreach (Button button in SelectionMenuPanelButtons)
        {
            button.OnPointerExit(null);
          
        }
        SelectionMenuPanelButtons[menuIndex].OnPointerEnter(null);
        CameraFade.FadeInComplete += (() => { if (EnteredScene) { if (LoadPrevScene) { { CardController.Instance.LoadPrevEnv();  } } else { CardController.Instance.LoadNextEnv(); } } });
    }
    [ContextMenu("GoBack")]
    public void GobackToMainMenu()
    {
        SetActiveJoystick(true);
        menuIndex = 0;
        maxIndex = SelectionMenuPanelButtons.Count;
        menuSelected = false;
        EnteredScene = false;
        MainPanel.GetComponent<CanvasGroup>().alpha = 1;
      //  MainPanel.SetActive(true);
        SubPanel.SetActive(false);

        Quest2AssetBundleLoader.Instance.UnloadEnvironments();
        OceanEnv.SetActive(true);

        foreach (Button button in SelectionMenuPanelButtons)
        {
            button.OnPointerExit(null);
        }

        CardController.Instance.ResetThisBehaviour();

        foreach (Button button in SelectionMenuPanelButtons)
        {
            button.OnPointerExit(null);
            button.OnDeselect(null);
            button.GetComponent<Animator>().ResetTrigger("Selected");
            button.GetComponent<Animator>().ResetTrigger("Pressed");
            button.GetComponent<Animator>().ResetTrigger("Highlighted");
            button.GetComponent<Animator>().SetTrigger("Normal");
        }
        foreach (Button button in SubPanels)
        {
            button.OnPointerExit(null);
            button.OnDeselect(null);
            button.GetComponent<Animator>().ResetTrigger("Selected");
            button.GetComponent<Animator>().ResetTrigger("Pressed");
            button.GetComponent<Animator>().ResetTrigger("Highlighted");
            button.GetComponent<Animator>().SetTrigger("Normal");
        }
        SelectionMenuPanelButtons[menuIndex].OnPointerEnter(null);


        DisableRightStick = false;
    }

    public void OnJoystickRight()
    {
        if (!DisableRightStick)
        {
            if (!EnteredScene)
            {
                menuIndex++;
                if (isMenuLooping)
                {
                    if (menuIndex > maxIndex - 1)
                    {
                        menuIndex = 0;
                    }
                }
                else
                {
                    if (menuIndex > maxIndex - 1)
                    {
                        menuIndex = maxIndex - 1;
                    }
                }
                HoverOnMenuItem();
            }
            else
            {
                CameraFade.PitchToDark?.Invoke();
                SoundClipPlayer.Instance.PlayChangeSceneSound();

                LoadPrevScene = false;
                CameraFade.FadeInComplete?.Invoke();
                
            }
        }
        
    }

  

    public void OnJoystickLeft()
    {

        if (!DisableRightStick)
        {
            if (!EnteredScene)
            {
                menuIndex--;
                if (isMenuLooping)
                {
                    if (menuIndex < 0)
                    {
                        menuIndex = maxIndex - 1;
                    }
                }

                else
                {
                    if (menuIndex < 0)
                    {

                        menuIndex = 0;
                    }
                }
                HoverOnMenuItem();
            }
            else
            {
                CameraFade.PitchToDark?.Invoke();
                SoundClipPlayer.Instance.PlayChangeSceneSound();
                LoadPrevScene = true;
                CameraFade.FadeInComplete?.Invoke();

               

            }
        }
     
    }

    void HoverOnMenuItem()
    {
        if (!menuSelected)
        {
            foreach (Button button in SelectionMenuPanelButtons)
            {
                button.OnPointerExit(null);
            }
            SelectionMenuPanelButtons[menuIndex].OnPointerEnter(null);
            SoundClipPlayer.Instance.PlaySelectSound();

        }
        else
        {
            SoundClipPlayer.Instance.PlaySelectSound();

            foreach (Button button in SubPanels)
            {
                button.OnPointerExit(null);
            }
            SubPanels[menuIndex].OnPointerEnter(null);
        }
        OnMenuItemHover?.Invoke();

    }
    public void OnMenuSelect()
    {
        if (!menuSelected)
        {
            CardController.Instance.ChangeSlectedCards(menuIndex);
            foreach (Button button in SelectionMenuPanelButtons)
            {
                button.OnPointerExit(null);
            }
            menuSelected = true;
            menuIndex = 0;
            HoverOnMenuItem();
            CardController.PushCurrentSelectionIndex?.Invoke(menuIndex);
            SelectionMenuPanelButtons[menuIndex].onClick?.Invoke();
            OnMenuItemSelected?.Invoke();
            SoundClipPlayer.Instance.PlayClickSound();

            return;
        }

        else if(!EnteredScene)
        {
            SoundClipPlayer.Instance.PlaySelectSound();
            foreach (Button button in SubPanels)
            {
                button.OnPointerExit(null);
            }
            SubPanels[menuIndex].onClick?.Invoke();
            CardController.Instance.CurrentSubSelectionIndex = menuIndex;
            EnteredScene = true;
        }
    }

    public int  IncrementMainUIIndex(int a)
    {
        menuIndex = a;
        menuIndex++;

        if (isMenuLooping)
        {
            if (menuIndex > maxIndex )
            {
                menuIndex = 0;
               
            }
        }
        else
        {
            if (menuIndex > maxIndex )
            {
                menuIndex = maxIndex ;
            }
        }
      //  SetActiveJoystick(false);
        return menuIndex;
    }

    public int decrementMainUIIndex(int a)
    {
        menuIndex = a;
      
            menuIndex--;
            if (isMenuLooping)
            {
                if (menuIndex < 0)
                {
                    menuIndex = maxIndex ;
                }
            }

            else
            {
                if (menuIndex < 0)
                {

                    menuIndex = 0;
                }
            }
       // SetActiveJoystick(false);

        return menuIndex;
    }

    public void UpdateMaxIndex(int a)
    {
        maxIndex = a;
    }

}
