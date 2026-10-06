namespace ArtLake.UI
{
    public interface IGameUI
    {
        PanelType Type { get; }
        bool IsOpen { get; }
        void Open();
        void Close();
    }
}
