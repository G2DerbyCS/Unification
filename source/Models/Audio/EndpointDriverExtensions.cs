using NAudio.CoreAudioApi;
using System;

namespace Unification.Models.Audio
{
    /// <summary>
    /// A static class containing extension methods for use with IEndpointDriver implementations.
    /// </summary>
    internal static class EndpointDriverExtensions
    {
        /// <summary>
        /// Extends the MMDevice class allowing extraction of the device GUID from the MMDevice ID property.
        /// </summary>
        /// <param name="MMDevice">MMDevice instance.</param>
        /// <returns>Extracted GUID</returns>
        public static Guid GetGuid(this MMDevice MMDevice)
        {
            int    BraceCount = 0;
            string GUID       = "";

            for (int i = 0; i < MMDevice.ID.Length; i++)
            {
                if (MMDevice.ID[i].Equals('{')) BraceCount++;

                if (BraceCount == 2)
                {
                    GUID = MMDevice.ID.Substring((i + 1), (MMDevice.ID.Length - i - 2));

                    break;
                }
            }

            return new Guid(GUID);
        }
    }
}
