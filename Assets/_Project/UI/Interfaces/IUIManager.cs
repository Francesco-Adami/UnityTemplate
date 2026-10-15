using ArtLake.UI;

namespace Project.UI
{
    /// <summary>
    /// Facciata disaccoppiata su ArtLake.UI.UIManager esposta tramite ServiceLocator.
    /// I sistemi di gioco (GameStateManager, SceneLoader, ecc.) dipendono da questa
    /// interfaccia, non dall'implementazione concreta del package ArtLake.
    /// </summary>
    public interface IUIManager
    {
        BaseUI CurrentScreen { get; }
        int    ActivePopupCount { get; }

        T    Show<T>() where T : BaseUI;
        void Hide<T>() where T : BaseUI;
        bool Back();
        bool IsOpen<T>() where T : BaseUI;
        T    Get<T>() where T : BaseUI;
        void CloseAllPopups();
    }
}
