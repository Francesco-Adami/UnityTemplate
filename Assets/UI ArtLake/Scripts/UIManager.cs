using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtLake.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Manager Settings")]
        [SerializeField] private bool isPersistent = true;
        [SerializeField] private Transform uiContainer;
        [SerializeField] private BaseUI startingUI;

        private readonly Dictionary<Type, BaseUI> registeredUIs = new Dictionary<Type, BaseUI>();
        private readonly Stack<BaseUI> popupStack = new Stack<BaseUI>();
        private readonly Stack<BaseUI> screenHistory = new Stack<BaseUI>();
        private BaseUI currentScreen;

        public BaseUI CurrentScreen => currentScreen;
        public int ActivePopupCount => popupStack.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (isPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializePanels();
        }

        private void InitializePanels()
        {
            if (uiContainer == null)
            {
                uiContainer = transform;
            }

            BaseUI[] foundPanels = uiContainer.GetComponentsInChildren<BaseUI>(true);
            foreach (BaseUI panel in foundPanels)
            {
                RegisterUI(panel);
                panel.Close();
            }

            if (startingUI != null)
            {
                Show(startingUI);
            }
        }

        public void RegisterUI(BaseUI ui)
        {
            if (ui == null) return;

            Type type = ui.GetType();
            registeredUIs[type] = ui;
        }

        public T Show<T>() where T : BaseUI
        {
            if (registeredUIs.TryGetValue(typeof(T), out BaseUI ui))
            {
                Show(ui);
                return (T)ui;
            }

            Debug.LogWarning($"[UIManager] UI of type '{typeof(T).Name}' is not registered.");
            return null;
        }

        public void Show(BaseUI panel)
        {
            if (panel == null) return;

            RegisterUI(panel);

            if (panel.Type == PanelType.Screen)
            {
                if (currentScreen != null && currentScreen != panel)
                {
                    currentScreen.Close();
                    screenHistory.Push(currentScreen);
                }

                CloseAllPopups();
                currentScreen = panel;
                panel.Open();
            }
            else if (panel.Type == PanelType.Popup)
            {
                if (!popupStack.Contains(panel))
                {
                    popupStack.Push(panel);
                }

                panel.Open();
            }
        }

        public void Hide<T>() where T : BaseUI
        {
            if (registeredUIs.TryGetValue(typeof(T), out BaseUI ui))
            {
                Hide(ui);
            }
        }

        public void Hide(BaseUI panel)
        {
            if (panel == null) return;

            panel.Close();

            if (panel.Type == PanelType.Popup && popupStack.Contains(panel))
            {
                if (popupStack.Count > 0 && popupStack.Peek() == panel)
                {
                    popupStack.Pop();
                }
                else
                {
                    RebuildPopupStackWithout(panel);
                }
            }
        }

        public bool Back()
        {
            if (popupStack.Count > 0)
            {
                BaseUI topPopup = popupStack.Pop();
                topPopup.Close();
                return true;
            }

            if (screenHistory.Count > 0)
            {
                BaseUI prevScreen = screenHistory.Pop();

                if (currentScreen != null)
                {
                    currentScreen.Close();
                }

                currentScreen = prevScreen;
                currentScreen.Open();
                return true;
            }

            return false;
        }

        public T Get<T>() where T : BaseUI
        {
            return registeredUIs.TryGetValue(typeof(T), out BaseUI ui) ? (T)ui : null;
        }

        public bool IsOpen<T>() where T : BaseUI
        {
            return registeredUIs.TryGetValue(typeof(T), out BaseUI ui) && ui.IsOpen;
        }

        public void CloseAllPopups()
        {
            while (popupStack.Count > 0)
            {
                BaseUI popup = popupStack.Pop();
                popup.Close();
            }
        }

        private void RebuildPopupStackWithout(BaseUI panelToRemove)
        {
            var tempList = new List<BaseUI>();
            while (popupStack.Count > 0)
            {
                BaseUI item = popupStack.Pop();
                if (item != panelToRemove)
                {
                    tempList.Add(item);
                }
            }

            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                popupStack.Push(tempList[i]);
            }
        }
    }
}
