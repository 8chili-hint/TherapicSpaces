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

    bool menuSelected;
    private void Awake()
    {
      maxIndex =  SelectionMenuPanelButtons.Count;
        menuIndex = 0;
    }


    private void Start()
    {
        HoverOnMenuItem();
    }
    void CheckLoopingbehaviour() {
        
        
    }


   public void OnJoystickRight()
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
                menuIndex = maxIndex-1;
            }
        }
        HoverOnMenuItem();
    }

    public void OnJoystickLeft()
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

    void HoverOnMenuItem()
    {
        if (!menuSelected)
        {
            foreach (Button button in SelectionMenuPanelButtons)
            {
                button.OnPointerExit(null);
            }
            SelectionMenuPanelButtons[menuIndex].OnPointerEnter(null);


        }
        else
        {
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
            menuSelected = true;
            menuIndex = 0;
            HoverOnMenuItem();
            SelectionMenuPanelButtons[menuIndex].onClick?.Invoke();
            CardController.PushCurrentSelectionIndex?.Invoke(menuIndex);
            OnMenuItemSelected?.Invoke();
            return;
        }

        else
        {
            SubPanels[menuIndex].onClick?.Invoke();

        }
    }
}
