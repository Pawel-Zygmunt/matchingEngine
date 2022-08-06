using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matchingEngine
{
    public enum OrderMatchingResult : byte
    {
        //Success Result
        OrderCancelled = 1,
        OrderAccepted = 2,
        InsufficientVolatilityPartialCancel = 3,
        InsufficientVolatilityTotalCancel = 4,


        //Failure Result
        OrderDoesNotExists = 11,
        InvalidPriceOrInitialQuantity = 13,
        //QuantityAndTotalQuantityShouldBeMultipleOfStepSize = 24

    }
}
