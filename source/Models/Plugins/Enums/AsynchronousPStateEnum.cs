namespace Unification.Models.Plugins.Enums
{
    /// <summary>
    /// An enum for use in indicating the process state of an asynchronous class, 
    /// or a class with asynchronous components.
    /// </summary>
    internal enum AsynchronousPState
    {
        Halted     = 0,
        Idling     = 1,
        Processing = 2
    }
}
