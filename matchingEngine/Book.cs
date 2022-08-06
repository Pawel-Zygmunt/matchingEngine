using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matchingEngine
{
    public class Book
    {
        private readonly SortedDictionary<double, PriceLevel> _bidSide;
        private readonly SortedDictionary<double, PriceLevel> _askSide;

        public PriceLevel? BestBidPriceLevel { get; private set; }
        public PriceLevel? BestAskPriceLevel { get; private set; }


        public Book()
        {
            var priceComparerAscending = new PriceComparerAscending();
            var priceComparerDescending = new PriceComparerDescending();

            _bidSide = new SortedDictionary<double, PriceLevel>(priceComparerDescending);
            _askSide = new SortedDictionary<double, PriceLevel>(priceComparerAscending);

            BestBidPriceLevel = null;
            BestAskPriceLevel = null;
        }

        public LimitOrder? GetBestOrderForSide(OrderType orderType)
        {
            var bestPriceLevel = orderType == OrderType.BUY ? BestBidPriceLevel : BestAskPriceLevel;

            if(bestPriceLevel == null)
                return null;

            return bestPriceLevel.FirstOrder;
        }

        public void AddOrder(LimitOrder order)
        {
            PriceLevel GetOrAddPriceLevel(double price, SortedDictionary<double, PriceLevel> side)
            {
                if (!side.TryGetValue(price, out PriceLevel? priceLevel))
                {
                    priceLevel = new PriceLevel(price);
                    side.Add(price, priceLevel);
                }
                return priceLevel;
            }

            if (order.Type == OrderType.BUY)
            {
                PriceLevel priceLevel = GetOrAddPriceLevel(order.Price, _bidSide);
                priceLevel.AddOrder(order);

                if(BestBidPriceLevel == null || order.Price > BestBidPriceLevel.Price)
                {
                    BestBidPriceLevel = priceLevel;
                }
            }
            else
            {
                PriceLevel priceLevel = GetOrAddPriceLevel(order.Price, _askSide);
                priceLevel.AddOrder(order);

                if(BestAskPriceLevel == null || order.Price < BestAskPriceLevel.Price)
                {
                    BestAskPriceLevel = priceLevel;
                }
            }
        }

        public void RemoveFilledOrderAndRemovePriceLevelIfEmpty(LimitOrder order)
        {
            if (!order.IsTotallyFilled)
                throw new InvalidOperationException($"PriceLevelError: trying to remove order which is not filled completely for OrderId={order.OrderId}");

            var side = order.Type == OrderType.BUY ? _bidSide : _askSide;

            if (side.TryGetValue(order.Price, out PriceLevel? priceLevel))
            {
                priceLevel.RemoveOrder(order);

                if (priceLevel.OrderCount == 0)
                    side.Remove(priceLevel.Price);
            }
            else
            {
                throw new InvalidOperationException("price level does not exists in order book");
            }
        }
    }
}
