using UnityEngine;

namespace ArtLake.UI
{
    public enum PanelType
    {
        Screen,
        Popup
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class BaseUI : MonoBehaviour, IGameUI
    {
        [Header("Panel Configuration")]
        [SerializeField] protected PanelType panelType = PanelType.Screen;
        [Header("Controller Navigation")]
        [SerializeField] protected UnityEngine.UI.Selectable firstSelected;

        protected CanvasGroup canvasGroup;
        public PanelType Type => panelType;
        public bool IsOpen { get; private set; }
        public virtual bool DismissOnCancel => true;

        protected virtual void Awake()
        {
            EnsureCanvasGroup();
        }

        protected void EnsureCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        public virtual void Open()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            EnsureCanvasGroup();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            IsOpen = true;

            if (firstSelected != null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
            }

            OnOpen();
        }

        public virtual void Close()
        {
            EnsureCanvasGroup();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            IsOpen = false;

            OnClose();
        }

        public void SetActive(bool active)
        {
            if (active) Open();
            else Close();
        }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
    }
}
