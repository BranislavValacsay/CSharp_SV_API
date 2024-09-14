namespace LARS.Interface
{
    public interface IStepperService
    {
        Task<bool> Lifecycle(string guid, string operation);
    }
}
