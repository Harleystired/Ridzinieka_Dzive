public interface IWorkComputer
{
    void Open();
    void Close();
    bool IsOpen { get; }
    bool CanInteract { get; }
}
