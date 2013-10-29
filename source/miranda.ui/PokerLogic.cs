using System;
using System.Text;

enum SUIT { None, Diamonds, Hearts, Clubs, Spades }
enum RANK { None, Ace, Two, Three, Four, Five, Six, Seven, Eight,
	Nine, Ten, Jack, Queen, King }
enum POKERSCORE { None, JacksOrBetter, TwoPair, ThreeOfAKind,
	Straight, Flush, FullHouse, FourOfAKind, StraightFlush,
	RoyalFlush }

class Card : IComparable
{
	private RANK _rank;
	private SUIT _suit;
	
	// IComparable interface method
	public int CompareTo(object o)
	{
		if (o is Card)
		{
			Card c = (Card)o;
			if (_rank < c.rank)
				return -1;
			else if (_rank > c.rank)
				return 1;
			return 0;
		}
		throw new ArgumentException("Object is not a Card");
	}
	
	public Card(RANK rank, SUIT suit)
	{
		this._rank = rank;
		this._suit = suit;
	}
	public Card() : this(RANK.None, SUIT.None)
	{
	}

	public override string ToString()
	{
		return this._rank + " " + this._suit;
	}

	public RANK rank
	{
		get { return _rank; }
	}

	public SUIT suit
	{
		get { return _suit; }
	}

	public bool isJacksOrBetter()
	{
		if (_rank == RANK.Ace)
			return true;
		if (_rank >= RANK.Jack)
			return true;
		return false;
	}
}

class Deck
{
	// array of Card of object (the real deck)
	private Card []d;
	// current card index
	private int cc = 0;
	// shuffle variable
	private Random rand = new Random();
	
	public Deck()
	{
		init();
	}

	private void init()
	{
		cc = 0;
		d = new Card[52];
		int counter = 0;
		// nice way to initialize the Deck, using
		// builtin functionality of Enum
		foreach(SUIT s in Enum.GetValues(typeof(SUIT)))
			foreach(RANK r in Enum.GetValues(typeof(RANK)))
				if (r!=RANK.None && s!=SUIT.None)
					d[counter++] = new Card(r,s);
	}

	public Card pullCard()
	{
		return d[cc++];
	}

	public Card peekCard()
	{
		return d[cc];
	}

	private void swapCards(int i, int j)
	{
		Card tmp = d[i];
		d[i] = d[j];
		d[j] = tmp;
	}

	/*
	 * shuffle the deck and reset the current card
	 * index to the beginning
	 */	
	public void shuffle(int count)
	{
		cc = 0;
		for (int i=0;i<count;++i)
		{
			for(int j=0;j<52;++j)
			{
				int idx = rand.Next(52);
				swapCards(j,idx);
			}
		}
	}

	/*
	 * 10 is overkill, 8 should be enough
	 */ 
	public void shuffle()
	{
		this.shuffle(10);
	}

	public void print()
	{
		foreach(Card c in d)
			Console.WriteLine(c);
	}
}

class PokerHand
{
	private Deck deck;
	private Card[] hand;
	
	public PokerHand(Deck deck)
	{
		this.deck = deck;
		this.hand = new Card[5];
	}

	public void pullCards()
	{
		for(int i=0;i<5;++i)
			hand[i] = deck.pullCard();
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		foreach(Card c in hand)
		{
			sb.Append(c);
			sb.Append(", ");
		}
		return sb.ToString();
	}

	public Card this [int index]
	{
		get
		{
			return hand[index];
		}
	}
	public void Sort()
	{
		Array.Sort(hand);
	}	
}

class PokerLogic
{
	// flush is when all of the suits are the same
	private static bool isFlush(PokerHand h)
	{
		if (h[0].suit == h[1].suit && 
		    h[1].suit == h[2].suit &&
		    h[2].suit == h[3].suit &&
		    h[3].suit == h[4].suit)
			return true;
		return false;
	}

	// make sure the rank differs by one
	// we can do this since the Hand is 
	// sorted by this point
	private static bool isStraight(PokerHand h)
	{
		if (h[0].rank == h[1].rank-1 &&
		    h[1].rank == h[2].rank-1 &&
		    h[2].rank == h[3].rank-1 &&
		    h[3].rank == h[4].rank-1)
			return true;
		// special case cause ace ranks lower
		// than 10 or higher
		if (h[1].rank == RANK.Ten &&
		    h[2].rank == RANK.Jack &&
		    h[3].rank == RANK.Queen &&
		    h[4].rank == RANK.King &&
		    h[0].rank == RANK.Ace)
			return true;
		return false;
	}
	
	// must be flush and straight and
	// be certain cards. No wonder I have
	private static bool isRoyalFlush(PokerHand h)
	{
		if (isStraight(h) && isFlush(h) &&
		      h[0].rank == RANK.Ace &&
		      h[1].rank == RANK.Ten &&
		      h[2].rank == RANK.Jack &&
		      h[3].rank == RANK.Queen &&
		      h[4].rank == RANK.King)
			return true;
		return false;
	}

	private static bool isStraightFlush(PokerHand h)
	{
		if (isStraight(h) && isFlush(h))
			return true;
		return false;
	}
	
	/*
	 * Two choices here, the first four cards
	 * must match in rank, or the second four
	 * must match in rank. Only because the hand
	 * is sorted
	 */
	private static bool isFourOfAKind(PokerHand h)
	{
		if (h[0].rank == h[1].rank &&
			h[1].rank == h[2].rank &&
			h[2].rank == h[3].rank)
			return true;
		if (h[1].rank == h[2].rank &&
			h[2].rank == h[3].rank &&
			h[3].rank == h[4].rank)
			return true;
		return false;
	}
	
	/*
	 * two choices here, the pair is in the
	 * front of the hand or in the back of the
	 * hand, because it is sorted
	 */
	private static bool isFullHouse(PokerHand h)
	{
		if (h[0].rank == h[1].rank &&
			h[2].rank == h[3].rank &&
			h[3].rank == h[4].rank)
			return true;
		if (h[0].rank == h[1].rank &&
			h[1].rank == h[2].rank &&
			h[3].rank == h[4].rank)
			return true;
		return false;
	}

	/*
	 * three choices here, first three cards match
	 * middle three cards match or last three cards
	 * match
	 */
	private static bool isThreeOfAKind(PokerHand h)
	{
		if (h[0].rank == h[1].rank &&
			h[1].rank == h[2].rank)
			return true;
		if (h[1].rank == h[2].rank &&
			h[2].rank == h[3].rank)
			return true;
		if (h[2].rank == h[3].rank &&
			h[3].rank == h[4].rank)
			return true;
		return false;
	}
	  
	/*
	 * three choices, two pair in the front,
	 * separated by a single card or
	 * two pair in the back
	 */
	private static bool isTwoPair(PokerHand h)
	{
		if (h[0].rank == h[1].rank &&
			h[2].rank == h[3].rank)
			return true;
		if (h[0].rank == h[1].rank &&
			h[3].rank == h[4].rank)
			return true;
		if (h[1].rank == h[2].rank &&
			h[3].rank == h[4].rank)
			return true;
		return false;
	}

	/*
	 * 4 choices here
	 */
	private static bool isJacksOrBetter(PokerHand h)
	{
		if (h[0].rank == h[1].rank &&
			h[0].isJacksOrBetter())
			return true;
		if (h[1].rank == h[2].rank &&
			h[1].isJacksOrBetter())
			return true;
		if (h[2].rank == h[3].rank &&
			h[2].isJacksOrBetter())
			return true;
		if (h[3].rank == h[4].rank &&
			h[3].isJacksOrBetter())
			return true;
		return false;
	}
	
	// must be in order of hands and must be
	// mutually exclusive choices
	public static POKERSCORE score(PokerHand h)
	{
		if (isRoyalFlush(h))
			return POKERSCORE.RoyalFlush;
		else if (isStraightFlush(h))
			return POKERSCORE.StraightFlush;
		else if (isFourOfAKind(h))
			return POKERSCORE.FourOfAKind;
		else if (isFullHouse(h))
			return POKERSCORE.FullHouse;
		else if (isFlush(h))
			return POKERSCORE.Flush;
		else if (isStraight(h))
			return POKERSCORE.Straight;
		else if (isThreeOfAKind(h))
			return POKERSCORE.ThreeOfAKind;
		else if (isTwoPair(h))
			return POKERSCORE.TwoPair;
		else if (isJacksOrBetter(h))
			return POKERSCORE.JacksOrBetter;
		else		
			return POKERSCORE.None;
	}
}

class Stats
{
	private int _simCount;

	private int[] counts = new int[Enum.GetValues(typeof(POKERSCORE)).Length];

	public void Report()
	{
		Console.WriteLine("{0,10}\t{1,10}\t{2,10}",
				"Hand", "Count", "Percent");
		for(int i=0;i<counts.Length;++i)
		{
			Console.WriteLine("{0,-10}\t{1,10}\t{2,10:p4}", 
					Enum.GetName(typeof(POKERSCORE),i),
					counts[i], 
					counts[i]/(double)_simCount);
		}
		Console.WriteLine("{0,10}\t{1,10}", "Total Hands", _simCount);	
	}
	
	public void Append(POKERSCORE ps)
	{
		counts[(int)ps]++;
	}

	public void reset()
	{
		for(int i=0;i<counts.Length;++i)
			counts[i] = 0;
	}

	public Stats()
	{
		reset();
	}
	public int simCount
	{
		set { _simCount = value; }
		get { return _simCount; }
	}
}

public class PokerApp
{
	public static void Main(string[] args)
	{
		int simCount = 5000;
		if (args.Length == 1)
			simCount = int.Parse(args[0]);
		
		Deck d = new Deck();
		PokerHand hand = new PokerHand(d);
		
		Stats stats = new Stats();
		stats.simCount = simCount;
		for (int i=0;i<simCount;i++)
		{
			// worry counter
			if ((i%1000)==0)
				Console.Write("*");
			d.shuffle();
			hand.pullCards();
			hand.Sort();
			POKERSCORE ps = PokerLogic.score(hand);
			stats.Append(ps);
		}
		Console.WriteLine();
		stats.Report();
	}
}
