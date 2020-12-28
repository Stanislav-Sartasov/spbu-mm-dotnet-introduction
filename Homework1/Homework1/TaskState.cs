namespace Homework1
{
    internal enum TaskState : byte
    {
        WaitingForDependency,
        Ready,
        Executing,
        Finished,
        Crashed
    }
}
