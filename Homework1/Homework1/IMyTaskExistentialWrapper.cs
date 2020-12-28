namespace Homework1
{
    internal interface IMyTaskExistentialWrapper
    {
        void Execute();
        TaskState State { get; }
    }
}
