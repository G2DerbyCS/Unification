using System;

namespace Unification.Models.Interfaces
{
    /// <summary>
    /// Generic interface for all progress indicators.
    /// </summary>
    public interface IProgressIndicator
    {
        float Progress { set; get; }

        event EventHandler<StateChangeEventArgs<float>> ProgressUpdatedEvent;
    }
}
