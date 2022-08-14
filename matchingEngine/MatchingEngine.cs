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

        public void AddOrder(Order order)
        {
            _tradeListener.OnAccept(order.OrderId);

            if(order is MarketOrder)
            {
                MarketOrder incomingOrder = (MarketOrder)order;

                MatchWithRestingOrders(incomingOrder);

                if (incomingOrder.IsPartiallyFilled)
                    _tradeListener.OnCancel(incomingOrder.OrderId, OrderCancelReason.InsufficientVolatilityOrExceededRangePartialCancel);
                    
                if (incomingOrder.IsNotFilledAtAll)
                    _tradeListener.OnCancel(incomingOrder.OrderId, OrderCancelReason.InsufficientVolatilityOrExceededRangeTotalCancel);
            }
            else
            {
                LimitOrder incomingOrder = (LimitOrder)order;

                MatchWithRestingOrders(incomingOrder);

                if (incomingOrder.IsPartiallyFilled || incomingOrder.IsNotFilledAtAll)
                {
                    _book.AddOrder(incomingOrder);
                    _tradeListener.OnAddLimitOrderToBook(incomingOrder.OrderId, incomingOrder.Price, incomingOrder.CurrentQuantity);
                }
            }
        }

        private void MatchWithRestingOrders(Order incomingOrder)
        {
            void TryFillOrder(Order incoming, LimitOrder resting)
            {
                uint fillQuantity = resting.CurrentQuantity >= incoming.InitialQuantity ? incoming.InitialQuantity : resting.CurrentQuantity;
                incoming.DecreaseQuantity(fillQuantity);
                resting.DecreaseQuantity(fillQuantity);
                MarketPrice = resting.Price;

                if (resting.IsTotallyFilled)
                    _book.RemoveFilledOrder(resting);
                
                _tradeListener.OnTrade(incoming.OrderId, resting.OrderId, resting.Price, fillQuantity);
            }

            
            double PercentageDifferenceBetweenMarketPriceAndRestingOrder(double restingOrderPrice)
            {
                return Math.Abs(MarketPrice - restingOrderPrice) / (MarketPrice + restingOrderPrice) / 2 * 100;
            }


            while (true)
            {
                LimitOrder? restingOrder = _book.GetBestOrderForSide(incomingOrder.Type == OrderType.BUY ? OrderType.SELL : OrderType.BUY);

                if (restingOrder == null || incomingOrder.IsTotallyFilled)
                    break;

                if(incomingOrder is LimitOrder)
                {
                    if(incomingOrder.Type == OrderType.BUY && restingOrder.Price <= (incomingOrder as LimitOrder)!.Price)
                        TryFillOrder(incomingOrder, restingOrder);
                        
                    if(incomingOrder.Type == OrderType.SELL && restingOrder.Price >= (incomingOrder as LimitOrder)!.Price)
                        TryFillOrder(incomingOrder, restingOrder);
                        
                }
                else
                {
                    if(PercentageDifferenceBetweenMarketPriceAndRestingOrder(restingOrder.Price) < 6)
                    {
                        TryFillOrder(incomingOrder, restingOrder);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
