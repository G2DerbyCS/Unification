using NAudio.CoreAudioApi;
using System;

namespace Unification.Models.Audio
{
    internal static class EndpointDriverExtensions
    {
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
