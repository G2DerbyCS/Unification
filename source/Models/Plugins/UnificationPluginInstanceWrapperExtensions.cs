using IUnification.Models.Interfaces;

namespace Unification.Models.Plugins
{
    /// <summary>
    /// A static class containing extension methods for PluginInstanceWrapper(IUnification) 
    /// instances.
    /// </summary>
    internal static class UnificationPluginInstanceWrapperExtensions
    {
        /// <summary>
        /// Performs any checks and/or configuration serialization before 
        /// calling dispose on the IUnification plugin instance.
        /// </summary>
        /// <param name="InstanceEventArgs">IUnificationPlugin PluginInstanceWrapper instance.</param>
        public static void Dispose(this PluginInstanceWrapper<IUnificationPlugin> InstanceEventArgs)
        {
            if (InstanceEventArgs.Instance is IConfigurableUnificationPlugin)
            {
                System.Diagnostics.Debug.WriteLine("[UNIMPLIMENTED] : UnificationPluginInstanceWrapperExtensions");
                System.Diagnostics.Debug.WriteLine("Dispose(this PluginInstanceWrapper<IUnificationPlugin> InstanceEventArgs)");
            }

            InstanceEventArgs.Instance.Dispose();
        }
    }
}
