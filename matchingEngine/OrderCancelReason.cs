using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matchingEngine
{
    public enum OrderCancelReason
    {
        InsufficientVolatilityOrExceededRangePartialCancel = 1,
        InsufficientVolatilityOrExceededRangeTotalCancel = 2,
    }
}
