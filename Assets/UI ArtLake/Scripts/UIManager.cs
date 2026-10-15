using System;
using System.Collections.Generic;
using Project.Core;
using Project.GameState;
using Project.Input;
using Project.UI;
using UnityEngine;

namespace ArtLake.UI
{
    public class UIManager : MonoBehaviour, IUIManager
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

        private GameObject lastSelectedGameObject;
        private IInputReader _inputReader;
        private IGameStateManager _gsm;

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

            ServiceLocator.Register<IUIManager>(this);
            InitializePanels();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet<IInputReader>(out var inputReader))
            {
                _inputReader = inputReader;
                _inputReader.PausePerformed += HandleCancelOrPauseInput;
            }
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
            var tempList = new List<BaseUI>();
            while (popupStack.Count > 0)
            {
                BaseUI popup = popupStack.Pop();
                if (popup is LoadingUI)
                {
                    tempList.Add(popup);
                }
                else
                {
                    popup.Close();
                }
            }

            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                popupStack.Push(tempList[i]);
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

        private void OnDestroy()
        {
            if (_inputReader != null)
            {
                _inputReader.PausePerformed -= HandleCancelOrPauseInput;
            }

            if (Instance == this)
            {
                ServiceLocator.Unregister<IUIManager>();
            }
        }

        private void HandleCancelOrPauseInput()
        {
            if (popupStack.Count > 0)
            {
                var topPopup = popupStack.Peek();
                if (topPopup != null && topPopup.DismissOnCancel)
                {
                    Back();
                }
                return;
            }

            if (_gsm == null)
            {
                ServiceLocator.TryGet(out _gsm);
            }

            if (_gsm != null && (_gsm.CurrentState == GameState.Gameplay || _gsm.CurrentState == GameState.Lobby))
            {
                _gsm.PauseGame();
                Show<PauseUI>();
            }
        }

        private void Update()
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem != null)
            {
                if (eventSystem.currentSelectedGameObject != null)
                {
                    lastSelectedGameObject = eventSystem.currentSelectedGameObject;
                }
                else if (lastSelectedGameObject != null && lastSelectedGameObject.activeInHierarchy)
                {
                    if (UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.wasUpdatedThisFrame)
                    {
                        eventSystem.SetSelectedGameObject(lastSelectedGameObject);
                    }
                }
            }
        }
    }
}
