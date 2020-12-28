namespace Homework1
{
    internal enum TaskState : byte
    {
        WaitingForDependency,
        Ready,
        Finished,
        Crashed
    }
}
