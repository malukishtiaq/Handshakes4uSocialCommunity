﻿using System;
using System.Threading;
using WoWonder.Library.Anjo.Share.Abstractions;

namespace WoWonder.Library.Anjo.Share
{
    /// <summary>
    /// Cross platform Share implemenations
    /// </summary>
    public class CrossShare
    {
        static readonly Lazy<IShare> implementation = new Lazy<IShare>(() => CreateShare(), LazyThreadSafetyMode.PublicationOnly);
        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => implementation.Value == null ? false : true;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IShare Current
        {
            get
            {
                var ret = implementation.Value;
                return ret switch
                {
                    null => throw NotImplementedInReferenceAssembly(),
                    _ => ret
                };
            }
        }

        static IShare CreateShare()
        {
#if NETSTANDARD1_0
            return null;
#else
            return new ShareImplementation();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly() =>
            new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");

    }
}
