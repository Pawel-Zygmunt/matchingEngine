using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace matchingEngine
{
    public class TimeProvider
    {
        private readonly DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSecondsFromEpoch()
        {
            return (int)DateTime.UtcNow.Subtract(Jan1970).TotalSeconds;
        }
    }
}


