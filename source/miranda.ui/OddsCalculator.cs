using System;
using System.Collections.Generic;

namespace miranda.ui
{
    public class OddsCalculator
    {
        public class PlayingCard
        {
            public PlayingCard(Rank r, Suit s)
            {
                Rank = r;
                Suit = s;
            }
            public readonly Rank Rank;
            public readonly Suit Suit;

            public override bool Equals(object obj)
            {
                if (obj is PlayingCard)
                {
                    return Rank.Equals((obj as PlayingCard).Rank) && Suit.Equals((obj as PlayingCard).Suit);
                }
                return false;
                //return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Rank.GetHashCode() + Suit.GetHashCode();
            }

            public override string ToString()
            {
                return Rank.ToString() + "-" + Suit.ToString();
            }
        }

        public class Container
        {
            public Container()
            {
                _cards = new List<PlayingCard>();

                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    foreach (Suit suit in Enum.GetValues(typeof (Suit)))
                    {
                        if(rank != Rank.NOT_RECOGNIZED && suit != Suit.NOT_RECOGNIZED)
                            _cards.Add(new PlayingCard(rank, suit));
                    }
                }
            }
            readonly List<PlayingCard> _cards;
            public PlayingCard this[Rank rank, Suit suit]
            {
                get 
                { 
                    var idx = _cards.IndexOf(new PlayingCard(rank, suit));
                    if (idx == -1)
                        return null;

                    _cards.RemoveAt(idx);
                    return new PlayingCard(rank, suit);
                }
            }

            public IList<PlayingCard> this[Rank rank]
            {
                get
                {
                    var res = new List<PlayingCard>();
                    var ls = _cards.FindAll(card => card.Rank == rank);
                    if (ls.Count == 0)
                    {
                        return res;
                    }

                    res.AddRange(ls);

                    foreach (var card in res)
                    {
                        _cards.Remove(card);
                    }
                    
                    return res;
                }
            }

            public IList<PlayingCard> this[Suit suit]
            {
                get
                {
                    var res = new List<PlayingCard>();

                    var ls = _cards.FindAll(card => card.Suit == suit);
                    if (ls.Count == 0)
                    {
                        return null;
                    }

                    
                    res.AddRange(ls);

                    foreach (var card in res)
                    {
                        _cards.Remove(card);
                    }

                    return res;
                }
            }
        }

        Dictionary<Rank, int> GetRankCount(IEnumerable<Card> input)
        {
            var ls = new List<Card>(input);

            var lsCnt = new Dictionary<Rank, int>();
            lsCnt.Add(Rank.Ace, ls.FindAll((card) => card.Rank == Rank.Ace).Count);
            lsCnt.Add(Rank.King, ls.FindAll((card) => card.Rank == Rank.King).Count);
            lsCnt.Add(Rank.Queen, ls.FindAll((card) => card.Rank == Rank.Queen).Count);
            lsCnt.Add(Rank.Jack, ls.FindAll((card) => card.Rank == Rank.Jack).Count);

            lsCnt.Add(Rank.Ten, ls.FindAll((card) => card.Rank == Rank.Ten).Count);
            lsCnt.Add(Rank.Nine, ls.FindAll((card) => card.Rank == Rank.Nine).Count);
            lsCnt.Add(Rank.Eight, ls.FindAll((card) => card.Rank == Rank.Eight).Count);
            lsCnt.Add(Rank.Seven, ls.FindAll((card) => card.Rank == Rank.Seven).Count);
            lsCnt.Add(Rank.Six, ls.FindAll((card) => card.Rank == Rank.Six).Count);
            lsCnt.Add(Rank.Five, ls.FindAll((card) => card.Rank == Rank.Five).Count);
            lsCnt.Add(Rank.Four, ls.FindAll((card) => card.Rank == Rank.Four).Count);
            lsCnt.Add(Rank.Three, ls.FindAll((card) => card.Rank == Rank.Three).Count);
            lsCnt.Add(Rank.Two, ls.FindAll((card) => card.Rank == Rank.Two).Count);

            lsCnt.Add(Rank.NOT_RECOGNIZED, ls.FindAll((card) => card.Rank == Rank.NOT_RECOGNIZED).Count);
            return lsCnt;
        }

        Dictionary<Suit, int> GetSuitCount(IEnumerable<Card> input)
        {
            var ls = new List<Card>(input);

            var lsCnt = new Dictionary<Suit, int>();
            lsCnt.Add(Suit.Hearts, ls.FindAll((card) => card.Suit == Suit.Hearts).Count);
            lsCnt.Add(Suit.Diamonds, ls.FindAll((card) => card.Suit == Suit.Diamonds).Count);
            lsCnt.Add(Suit.Clubs, ls.FindAll((card) => card.Suit == Suit.Clubs).Count);
            lsCnt.Add(Suit.Spades, ls.FindAll((card) => card.Suit == Suit.Spades).Count);

            lsCnt.Add(Suit.NOT_RECOGNIZED, ls.FindAll((card) => card.Suit == Suit.NOT_RECOGNIZED).Count);

            return lsCnt;
        }
 
        public int GetOutsForStreet(IEnumerable<Card> input)
        {
            var c = new Container();
            
            return 0;
        }
    }
}