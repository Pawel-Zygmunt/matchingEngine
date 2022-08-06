using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matchingEngine
{
    public class MatchingEngine
    {
        private readonly Book _book;
        private readonly ITradeListener _tradeListener;
        public double MarketPrice { get; private set; }

        public MatchingEngine(ITradeListener tradeListener)
        {
            _book = new Book();
            _tradeListener = tradeListener;
            MarketPrice = 0;
        }

        public OrderMatchingResult AddOrder(Order order)
        {
            _tradeListener.OnAccept(order.OrderId);

            if(order.GetType() == typeof(MarketOrder))
            {
                MarketOrder incomingOrder = (MarketOrder)order;

                MatchWithRestingOrders(incomingOrder);

                if (incomingOrder.IsPartiallyFilled)
                    return OrderMatchingResult.InsufficientVolatilityPartialCancel;

                if (incomingOrder.IsNotFilledAtAll)
                    return OrderMatchingResult.InsufficientVolatilityTotalCancel;
            }
            else
            {
                LimitOrder incomingOrder = (LimitOrder)order;

                MatchWithRestingOrders(incomingOrder);

                if (incomingOrder.IsPartiallyFilled || !incomingOrder.IsTotallyFilled)
                {
                    _book.AddOrder(incomingOrder);
                }
            }

            return OrderMatchingResult.OrderAccepted;
        }

        private bool MatchWithRestingOrders(Order incomingOrder)
        {
            bool anyMatchHappend = false;

            while (true)
            {
                LimitOrder? restingOrder = _book.GetBestOrderForSide(incomingOrder.Type == OrderType.BUY ? OrderType.SELL : OrderType.BUY);

                if (restingOrder == null)
                    break;

                double matchPrice = restingOrder.Price;

                if (incomingOrder.Type == OrderType.BUY && (restingOrder.Price <= incomingOrder.Price))
                {
                    uint fillQuantity = restingOrder.CurrentQuantity >= incomingOrder.InitialQuantity ? incomingOrder.InitialQuantity : restingOrder.CurrentQuantity;

                    incomingOrder.DecreaseQuantity(fillQuantity);

                    _book.FillLimitOrderAndRemovePriceLevelIfEmpty(restingOrder, fillQuantity);

                    _tradeListener.OnTrade(incomingOrder.OrderId, restingOrder.OrderId, matchPrice, fillQuantity);

                    MarketPrice = matchPrice;
                    anyMatchHappend = true;
                }

                if (incomingOrder.IsFilled)
                    break;
            }

            return anyMatchHappend;
        }
    }
}
