using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class Uicontroller : MonoBehaviour
{
    public bool isMenuLooping;
    public int menuIndex;

    private int maxIndex;
    public List<Button> SelectionMenuPanelButtons;

    public List<Button> SubPanels;

    public UnityEvent OnMenuItemSelected,OnMenuItemHover;

    public bool menuSelected;
   public bool EnteredScene;

    public bool LoadPrevScene;
    private void Awake()
    {
        maxIndex =  SelectionMenuPanelButtons.Count;
        menuIndex = 0;
    }


    private void Start()
    {
      

        foreach (Button button in SelectionMenuPanelButtons)
        {
            button.OnPointerExit(null);
        }
        SelectionMenuPanelButtons[menuIndex].OnPointerEnter(null);
        CameraFade.FadeInComplete += (() => { if (EnteredScene) { if (LoadPrevScene) { { CardController.Instance.LoadPrevEnv(); } } else { CardController.Instance.LoadNextEnv(); } } });
    }


   public void OnJoystickRight()
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
            CameraFade.Instance.FadeInOut();

            SoundClipPlayer.Instance.PlayChangeSceneSound();

            LoadPrevScene = false;
            
        }
    }

    public void OnJoystickLeft()
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
            CameraFade.Instance.FadeInOut();
            SoundClipPlayer.Instance.PlayChangeSceneSound();

            LoadPrevScene = true;
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
        
        return menuIndex;
    }

    public void UpdateMaxIndex(int a)
    {
        maxIndex = a;
    }

}
