namespace Devesprit.Core
{
    public partial interface IStartupTask
    {
        void Execute();

        int Order { get; }
    }
}
