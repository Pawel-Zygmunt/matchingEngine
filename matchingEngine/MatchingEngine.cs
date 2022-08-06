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
                    _tradeListener.OnAddLimitOrderToBook(incomingOrder.OrderId, incomingOrder.Price, incomingOrder.InitialQuantity);
                }
            }

            return OrderMatchingResult.OrderAccepted;
        }

        private void MatchWithRestingOrders(Order incomingOrder)
        {
            void FillOrderWith(Order incoming, LimitOrder resting)
            {
                uint fillQuantity = resting.CurrentQuantity >= incoming.InitialQuantity ? incoming.InitialQuantity : resting.CurrentQuantity;
                incoming.DecreaseQuantity(fillQuantity);
                _tradeListener.OnTrade(incoming.OrderId, resting.OrderId, resting.Price, fillQuantity);
            }

            //double PercentageDifference(double val1, double val2)
            //{
            //    return Math.Abs(val1 - val2) / (val1 + val2) / 2 * 100;
            //}


            while (true)
            {
                LimitOrder? restingOrder = _book.GetBestOrderForSide(incomingOrder.Type == OrderType.BUY ? OrderType.SELL : OrderType.BUY);

                if (restingOrder == null || incomingOrder.IsTotallyFilled)
                    break;

                if(incomingOrder.GetType() == typeof(LimitOrder) && incomingOrder.Type == OrderType.BUY && restingOrder.Price <= (incomingOrder as LimitOrder).Price)
                {
                    FillOrderWith(incomingOrder, restingOrder);
                    MarketPrice = restingOrder.Price;
                }
            }
        }
    }
}
