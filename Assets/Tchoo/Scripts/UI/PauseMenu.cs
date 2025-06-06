using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    //[SerializeField] private UnityEngine.UIElements.VisualElement visualElement;
    [SerializeField] private Image imageBackground;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private CanvasGroup defaultCanvasGroup;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Button controlsMenuReturnArrow;
    [SerializeField] private Button controllerArrow;
    [SerializeField] private Button keyboardArrow;
    private CanvasGroup currentCanvasGroup;
    private EventSystem evtSystem;
    private void Start()
    {
        imageBackground.DOFade(0f, 0.25f).SetUpdate(true);
        evtSystem = EventSystem.current;
    }

    public void PauseGame()
    {
        playerInput.SwitchCurrentActionMap("UI");
        Time.timeScale = 0f;
        evtSystem.SetSelectedGameObject(evtSystem.firstSelectedGameObject);
        imageBackground.DOFade(0.6f, 0.25f).SetUpdate(true).OnComplete(() => 
        { 
        });
        FadeIn(defaultCanvasGroup);
    }

    public void ResumeGame()
    {
        playerInput.SwitchCurrentActionMap("Player");
        imageBackground.DOFade(0f, 0.25f).SetUpdate(true); 
        FadeOut(currentCanvasGroup).OnComplete(() =>
        {
            Time.timeScale = 1f;
        });
    }

    public void JumpToElement(Selectable selectable)
    {
        evtSystem.SetSelectedGameObject(selectable.gameObject);

        //Navigation nav = but.navigation;//.selectOnRight += QuitGame;

    }

    //public void OnNavigation(Navigation nav, float sliderValue)
    //{
    //    visualElement.RegisterCallback<UnityEngine.UIElements.NavigationMoveEvent>(OnNavMoveEvent);

    //}

    //private void OnNavMoveEvent(UnityEngine.UIElements.NavigationMoveEvent evt)
    //{
    //    Debug.Log($"OnNavMoveEvent {evt.propagationPhase} - move {evt.move} - direction {evt.direction}");
    //}

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        imageBackground.DOFade(0f, 0.25f).SetUpdate(true);
        FadeOut(currentCanvasGroup).OnComplete(() =>
        {
            playerInput.SwitchCurrentActionMap("Player");
        });
        LevelManager.Instance.ActiveConnection = null;
        LevelManager.Instance.ConnectionToNewLevel("Level_0", true);
    }

    public void SetCanvasGroupActive(CanvasGroup canvasGroup)
    {
        if(currentCanvasGroup != null && currentCanvasGroup.alpha > 0)
        {
            FadeOut(currentCanvasGroup).OnComplete(() => FadeIn(canvasGroup));
        }
        else
        {
            FadeIn(canvasGroup);
        }
    }

    private Tween FadeOut(CanvasGroup canvasGroup)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        return canvasGroup.DOFade(0, 0.25f).SetUpdate(true);
    }

    private void FadeIn(CanvasGroup canvasGroup)
    {
        canvasGroup.DOFade(1, 0.25f).SetUpdate(true).OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            currentCanvasGroup = canvasGroup;
        });
    }

    public void SetSliderValue(float value)
    {
        _scrollRect.DOHorizontalNormalizedPos(value, 0.5f).SetEase(Ease.InOutSine).SetUpdate(true);
        Navigation newNavigationDown = new() { mode = Navigation.Mode.Explicit };
        if (value == 1f)
        {
            newNavigationDown.selectOnDown = keyboardArrow;
        }
        else
        {
            newNavigationDown.selectOnDown = controllerArrow;
        }
        controlsMenuReturnArrow.navigation = newNavigationDown;
        
    }
}
