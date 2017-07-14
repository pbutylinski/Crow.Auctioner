using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crow.Auctioner.DataStorage
{
    [Serializable]
    public class AuctionItem
    {
        public AuctionItem()
        {
            StartingPrice = new Money();
            CurrentPrice = StartingPrice;
            Submissioner = new Attendee();
        }

        public string DisplayName { get; set; }
        public Money StartingPrice { get; set; }
        public Money CurrentPrice { get; set; }
        public Attendee Submissioner { get; set; }
        public Attendee AuctionWinner { get; set; }
        public decimal ForCharityPercentage { get; set; }
        public bool IsSold { get; set; }
        public string PhotoFileName { get; set; }

        public Money GetCharityAmount()
        {
            return new Money(CurrentPrice.Currency, CurrentPrice.Value * ForCharityPercentage / 100);
        }
    }
}
