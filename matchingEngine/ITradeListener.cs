using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matchingEngine
{
    public interface ITradeListener
    {
        void OnAccept(Guid orderId);
        void OnTrade(Guid incomingOrderId, Guid restingOrderId, double matchPrice, uint matchQuantity);
        void OnCancel(Guid orderId);
    }
}
