# UI ArtLake — AI Agent System Prompt & Operational Guide

> **Purpose**: This document provides a compressed, high-density reference specifically optimized for AI LLMs, coding agents, and automated tools. When an AI agent reads this document, it acquires complete, deterministic operational knowledge of the `ArtLake.UI` package.

---

## 1. System Contract & Mental Model

### Core Axioms
1. **Single Entry Point**: All UI operations flow through `ArtLake.UI.UIManager.Instance`.
2. **Never Call `SetActive()`**: UI components remain active in the hierarchy. Showing and hiding is strictly handled by `CanvasGroup` properties (`alpha`, `interactable`, `blocksRaycasts`) via `BaseUI.Open()` and `BaseUI.Close()`.
3. **Dual Paradigm**:
   - `PanelType.Screen`: Exclusive primary views. Showing a Screen closes the previous screen, pushes it to `screenHistory`, and closes **all** popups.
   - `PanelType.Popup`: Stackable modal overlays. Showing a Popup pushes it onto `popupStack` on top of the current screen/popups.
4. **Type-Indexed Storage**: `UIManager` registers one instance per concrete C# `Type` in `Dictionary<Type, BaseUI>`. You cannot register two instances of the exact same class type.
5. **Unified Back Stack**: `UIManager.Instance.Back()` prioritizes popping popups; only when all popups are closed does it pop the previous screen.

---

## 2. API Signature Quick-Card

```csharp
namespace ArtLake.UI
{
    // --- ENUMS & INTERFACES ---
    public enum PanelType { Screen, Popup }
    public interface IGameUI { PanelType Type { get; } bool IsOpen { get; } void Open(); void Close(); }

    // --- BASE CLASS ---
    public abstract class BaseUI : MonoBehaviour, IGameUI
    {
        public PanelType Type { get; }
        public bool IsOpen { get; }
        public virtual void Open();
        public virtual void Close();
        public void SetActive(bool active);
        protected virtual void OnOpen();  // Virtual hook
        protected virtual void OnClose(); // Virtual hook
    }

    // --- MANAGER (SINGLETON) ---
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; }
        public BaseUI CurrentScreen { get; }
        public int ActivePopupCount { get; }

        public T Show<T>() where T : BaseUI;
        public void Show(BaseUI panel);
        public void Hide<T>() where T : BaseUI;
        public void Hide(BaseUI panel);
        public bool Back();
        public T Get<T>() where T : BaseUI;
        public bool IsOpen<T>() where T : BaseUI;
        public void CloseAllPopups();
        public void RegisterUI(BaseUI ui);
    }
}
```

---

## 3. Concrete Panels Index

| Class | Type | File Location | Key Responsibilities |
| :--- | :--- | :--- | :--- |
| `MainMenuUI` | `Screen` | `Assets/UI ArtLake/Scripts/MainMenuUI.cs` | Main title menu. Has `playButton`, `OptionsButton`, `ExitButton`, and `onPlay` UnityEvent. |
| `HudUI` | `Screen` | `Assets/UI ArtLake/Scripts/HudUI.cs` | In-game gameplay HUD. Listens for `KeyCode.Escape` to toggle `PauseUI`. |
| `PauseUI` | `Popup` | `Assets/UI ArtLake/Scripts/PauseUI.cs` | In-game pause menu. Automatically sets `Time.timeScale = 0f` on open, and restores `Time.timeScale = 1f` on close. Has Resume, Options, MainMenu buttons. |
| `OptionUI` | `Popup` | `Assets/UI ArtLake/Scripts/OptionUI.cs` | Options / Settings menu. Back button invokes `UIManager.Instance.Back()`. |
| `WinUI` | `Popup` | `Assets/UI ArtLake/Scripts/WinUI.cs` | Victory screen. Has Restart, NextLevel, and MainMenu buttons with respective UnityEvents. |
| `LoseUI` | `Popup` | `Assets/UI ArtLake/Scripts/LoseUI.cs` | Game Over / Defeat screen. Has Restart and MainMenu buttons with respective UnityEvents. |

---

## 4. Deterministic Decision Tree for AI Coding

Use this flowchart to determine the correct C# action:

```text
User asks to:
│
├── "Show UI / Open Screen"
│    └── Code: UIManager.Instance.Show<TPanel>();
│
├── "Close UI / Dismiss Popup"
│    ├── From external script: UIManager.Instance.Hide<TPanel>();
│    └── From inside the panel itself: UIManager.Instance.Hide(this);
│
├── "Handle Back Button / Android Escape Key / Cancel Action"
│    └── Code: UIManager.Instance.Back();
│
├── "Check if a UI is visible"
│    └── Code: UIManager.Instance.IsOpen<TPanel>();   (NEVER check gameObject.activeSelf!)
│
├── "Fetch UI reference to update health/ammo/text"
│    └── Code: var hud = UIManager.Instance.Get<HudUI>();
│
└── "Create a new UI panel"
     ├── Step 1: Create class inheriting from BaseUI.
     ├── Step 2: Define Reset() method setting panelType (Screen or Popup).
     ├── Step 3: Bind button listeners in OnEnable() and unbind in OnDisable().
     └── Step 4: Override OnOpen() / OnClose() calling base.OnOpen() / base.OnClose().
```

---

## 5. Ready-to-Generate Code Templates

### 5.1 New Screen Template
```csharp
using UnityEngine;
using ArtLake.UI;

namespace Game.UI
{
    public class CustomScreenUI : BaseUI
    {
        private void Reset()
        {
            panelType = PanelType.Screen;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            // Custom screen initialization
        }

        protected override void OnClose()
        {
            base.OnClose();
            // Custom screen cleanup
        }
    }
}
```

### 5.2 New Popup Template
```csharp
using UnityEngine;
using UnityEngine.UI;
using ArtLake.UI;

namespace Game.UI
{
    public class CustomPopupUI : BaseUI
    {
        [SerializeField] private Button closeButton;

        private void Reset()
        {
            panelType = PanelType.Popup;
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseSelf);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseSelf);
        }

        public void CloseSelf()
        {
            UIManager.Instance.Hide(this);
        }
    }
}
```

---

## 6. Critical Invariants to Preserve

1. **Hierarchy Integrity**: All panels in `_UI.prefab` are children of `_UI` and have a `CanvasGroup` component attached.
2. **TimeScale Hygiene**: Any popup modifying `Time.timeScale` must ensure it is reverted to `1f` on `OnClose()` and when navigating back to `MainMenuUI`.
3. **CanvasGroup State**:
   - Open: `alpha = 1`, `interactable = true`, `blocksRaycasts = true`.
   - Closed: `alpha = 0`, `interactable = false`, `blocksRaycasts = false`.
4. **No Multiple Instances of Same Type**: Subclass `BaseUI` for distinct views; never instantiate duplicates of the exact same `BaseUI` class under one `UIManager`.
