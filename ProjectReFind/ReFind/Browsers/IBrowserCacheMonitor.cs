using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReFind.Browsers
{
    /// <summary>
    /// IBrowserCacheMonitor interface
    /// </summary>
    public interface IBrowserCacheMonitor
    {
        /// <summary>
        /// Initiate browser cache monitoring
        /// </summary>
        void Start();

        /// <summary>
        /// Halt browser cache monitoring
        /// </summary>
        void Stop();
    }
}
