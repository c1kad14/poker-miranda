using miranda.ui;

namespace miranda
{
    public class TableBack
    {
        //bool CheckAllFour(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        //{
        //    if (c4 == null && c5 == null)
        //    {
        //        return c1.Rank == c2.Rank && c2.Rank == c3.Rank
        //                    && (card1.Rank == c1.Rank || card2.Rank == c1.Rank);
        //    }
        //    if (c4 != null && c5 == null)
        //    {
        //        return
        //            c1.Rank == c2.Rank && c2.Rank == c3.Rank && (card1.Rank == c1.Rank || card2.Rank == c1.Rank)
        //            ||
        //            c4.Rank == c2.Rank && c2.Rank == c3.Rank && (card1.Rank == c4.Rank || card2.Rank == c4.Rank)
        //            ||
        //            c1.Rank == c4.Rank && c4.Rank == c3.Rank && (card1.Rank == c1.Rank || card2.Rank == c1.Rank)
        //            ||
        //            c1.Rank == c2.Rank && c2.Rank == c4.Rank && (card1.Rank == c1.Rank || card2.Rank == c1.Rank)
        //            ;
        //    }
        //    if (c4 != null && c5 != null)
        //    {
        //        return false; // не играем такое
        //    }
        //    return false;
        //}

        //bool CheckHouse(Card c1, Card c2, Card c3, Card card1, Card card2)
        //{
        //    var cnt = GetRankCount(new[] { c1, c2, c3, card1, card2 });
        //    return cnt.Exists((i) => i == 2) && cnt.Exists((i) => i == 3);
        //}

        //bool CheckAllHouse(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        //{

        //    if (c4 == null && c5 == null)
        //    {
        //        return CheckHouse(c1, c2, c3, card1, card2);
        //    }
        //    if (c4 != null && c5 == null)
        //    {
        //        var cnt1 = GetRankCount(new[] { c1, c2, c3, c4, card1 });
        //        var cnt2 = GetRankCount(new[] { c1, c2, c3, c4, card2 });
        //        return 
        //            cnt1.Exists((i) => i == 2) && cnt1.Exists((i) => i == 3)
        //            ||
        //            ;
        //    }
        //    if (c4 != null && c5 != null)
        //    {
        //        return false; // не играем такое
        //    }
        //    return false;
        //}

        bool CheckTriple_(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            if (c4 == null && c5 == null)
            {
                return
                    CheckTripleLine(c1, c2, card1)
                    ||
                    CheckTripleLine(c1, c3, card1)
                    ||
                    CheckTripleLine(c2, c3, card1)
                    ||
                    CheckTripleLine(c1, c2, card2)
                    ||
                    CheckTripleLine(c1, c3, card2)
                    ||
                    CheckTripleLine(c2, c3, card2)
                    ||
                    CheckTripleLine(c1, card1, card2)
                    ||
                    CheckTripleLine(c2, card1, card2)
                    ||
                    CheckTripleLine(c3, card1, card2)

                    ;

            }
            if (c4 != null && c5 == null)
            {
                return
                    CheckTripleLine(c1, c2, card1)
                    ||
                    CheckTripleLine(c1, c3, card1)
                    ||
                    CheckTripleLine(c2, c3, card1)
                    ||
                    CheckTripleLine(c1, c2, card2)
                    ||
                    CheckTripleLine(c1, c3, card2)
                    ||
                    CheckTripleLine(c2, c3, card2)
                    ||
                    CheckTripleLine(c1, card1, card2)
                    ||
                    CheckTripleLine(c2, card1, card2)
                    ||
                    CheckTripleLine(c3, card1, card2)
                    ||


                    CheckTripleLine(c1, c4, card1)
                    ||
                    CheckTripleLine(c4, c3, card1)
                    ||
                    CheckTripleLine(c1, c4, card2)
                    ||
                    CheckTripleLine(c4, c3, card2)
                    ||
                    CheckTripleLine(c1, card1, card2)
                    ||
                    CheckTripleLine(c4, card1, card2)
                    ||
                    CheckTripleLine(c3, card1, card2)

                    ||
                    CheckTripleLine(c4, c2, card1)
                    ||
                    CheckTripleLine(c4, c3, card1)
                    ||
                    CheckTripleLine(c2, c3, card1)
                    ||
                    CheckTripleLine(c4, c2, card2)
                    ||
                    CheckTripleLine(c4, c3, card2)
                    ||
                    CheckTripleLine(c2, c3, card2)
                    ||
                    CheckTripleLine(c4, card1, card2)
                    ||
                    CheckTripleLine(c2, card1, card2)
                    ||
                    CheckTripleLine(c3, card1, card2)


                    ||
                    CheckTripleLine(c1, c4, card1)
                    ||
                    CheckTripleLine(c2, c4, card1)
                    ||
                    CheckTripleLine(c1, c4, card2)
                    ||
                    CheckTripleLine(c2, c4, card2)
                    ||
                    CheckTripleLine(c1, card1, card2)
                    ||
                    CheckTripleLine(c2, card1, card2)
                    ||
                    CheckTripleLine(c4, card1, card2);
            }
            if (c4 != null && c5 != null)
            {
                return false;
            }
            return false;
        }

        bool CheckTripleLine(Card c1, Card c2, Card c3)
        {
            return c1.Rank == c2.Rank && c2.Rank == c3.Rank;
        }


        #region Flush
        bool CheckHalfFlushLine(Card c1, Card c2, Card card1, Card card2)
        {
            return c1.Suit == c2.Suit && c2.Suit == card1.Suit && c2.Suit == card2.Suit;
        }

        bool CheckHalfFlush(Card c1, Card c2, Card c3, Card c4, Card card1, Card card2)
        {
            if (c4 == null)
            {
                return
                    (
                    CheckHalfFlushLine(c1, c2, c3, card1)
                    ||
                    CheckHalfFlushLine(c1, c2, c3, card2)
                    ||
                    CheckHalfFlushLine(c1, c2, card1, card2)
                    ||
                    CheckHalfFlushLine(c2, c3, card1, card2)
                    ||
                    CheckHalfFlushLine(c1, c3, card1, card2)
                    );
            }
            if (c4 != null)
            {
                return
                    (
                    CheckHalfFlushLine(c1, c2, c3, card1)
                    ||
                    CheckHalfFlushLine(c1, c2, c3, card2)
                    ||
                    CheckHalfFlushLine(c1, c2, card1, card2)
                    ||
                    CheckHalfFlushLine(c2, c3, card1, card2)
                    ||
                    CheckHalfFlushLine(c1, c3, card1, card2)

                    ||
                    CheckHalfFlushLine(c1, c2, c4, card1)
                    ||
                    CheckHalfFlushLine(c1, c2, c4, card2)
                    ||
                    CheckHalfFlushLine(c1, c2, card1, card2)
                    ||
                    CheckHalfFlushLine(c2, c4, card1, card2)
                    ||
                    CheckHalfFlushLine(c1, c4, card1, card2)

                    ||
                    CheckHalfFlushLine(c1, c4, c3, card1)
                    ||
                    CheckHalfFlushLine(c1, c4, c3, card2)
                    ||
                    CheckHalfFlushLine(c1, c4, card1, card2)
                    ||
                    CheckHalfFlushLine(c4, c3, card1, card2)
                    ||
                    CheckHalfFlushLine(c1, c3, card1, card2)

                    ||
                    CheckHalfFlushLine(c4, c2, c3, card1)
                    ||
                    CheckHalfFlushLine(c4, c2, c3, card2)
                    ||
                    CheckHalfFlushLine(c4, c2, card1, card2)
                    ||
                    CheckHalfFlushLine(c2, c3, card1, card2)
                    ||
                    CheckHalfFlushLine(c4, c3, card1, card2)
                    );
            }
            return false;
        }
        bool CheckFlush(Card c1, Card c2, Card c3, Card card1, Card card2)
        {
            return c1.Suit == c2.Suit && c2.Suit == c3.Suit && c3.Suit == card1.Suit && c3.Suit == card2.Suit;
        }
        bool CheckAllFlush(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            if (c4 == null && c5 == null)
            {
                return CheckFlush(c1, c2, c3, card1, card2);
            }
            if (c4 != null && c5 == null)
            {
                return
                    (
                    CheckFlush(c1, c2, c3, card1, card2)
                    ||
                    CheckFlush(c1, c2, c4, card1, card2)
                    ||
                    CheckFlush(c2, c3, c4, card1, card2)
                    ||
                    CheckFlush(c1, c3, c4, card1, card2)
                    //||

                    //CheckFlush(c1, c2, c3, c4, card1)
                    //||
                    //CheckFlush(c1, c2, c3, c4, card2)
                    );
            }
            if (c4 != null && c5 != null)
            {
                return
                    (
                    CheckFlush(c1, c2, c3, card1, card2)
                    ||
                    CheckFlush(c1, c3, c4, card1, card2)
                    ||
                    CheckFlush(c1, c4, c5, card1, card2)
                    ||
                    CheckFlush(c2, c3, c4, card1, card2)
                    ||

                    CheckFlush(c2, c4, c5, card1, card2)
                    ||
                    CheckFlush(c3, c4, c5, card1, card2)
                    ||
                    CheckFlush(c1, c2, c4, card1, card2)
                    ||

                    CheckFlush(c1, c2, c5, card1, card2)
                    ||
                    CheckFlush(c2, c3, c5, card1, card2)
                    ||
                    CheckFlush(c2, c4, c5, card1, card2)
                    ||
                    CheckFlush(c1, c3, c5, card1, card2)
                    //||

                    //CheckFlush(c1, c2, c3, c4, card2)
                    //||
                    //CheckFlush(c1, c3, c4, c5, card2)
                    //||
                    //CheckFlush(c1, c2, c4, c5, card2)
                    //||
                    //CheckFlush(c1, c2, c3, c4, card2)
                    //||

                    //CheckFlush(c1, c2, c3, c4, card1)
                    //||
                    //CheckFlush(c1, c3, c4, c5, card1)
                    //||
                    //CheckFlush(c1, c2, c4, c5, card1)
                    //||
                    //CheckFlush(c1, c2, c3, c4, card1)
                    );
            }
            return false;
        }
        #endregion

        #region Street
        bool CheckAllStreet(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            if (c4 == null && c5 == null)
            {
                return CheckStreet(c1, c2, c3, card1, card2);
            }
            if (c4 != null && c5 == null)
            {
                return
                    (
                    CheckStreet(c1, c2, c3, card1, card2)
                    ||
                    CheckStreet(c1, c2, c4, card1, card2)
                    ||
                    CheckStreet(c2, c3, c4, card1, card2)
                    ||
                    CheckStreet(c1, c3, c4, card1, card2)
                    );
            }
            if (c4 != null && c5 != null)
            {
                return
                    (
                    CheckStreet(c1, c2, c3, card1, card2)
                    ||
                    CheckStreet(c1, c3, c4, card1, card2)
                    ||
                    CheckStreet(c1, c4, c5, card1, card2)
                    ||
                    CheckStreet(c2, c3, c4, card1, card2)
                    ||

                    CheckStreet(c2, c4, c5, card1, card2)
                    ||
                    CheckStreet(c3, c4, c5, card1, card2)
                    ||
                    CheckStreet(c1, c2, c4, card1, card2)
                    ||

                    CheckStreet(c1, c2, c5, card1, card2)
                    ||
                    CheckStreet(c2, c3, c5, card1, card2)
                    ||
                    CheckStreet(c2, c4, c5, card1, card2)
                    ||
                    CheckStreet(c1, c3, c5, card1, card2)
                    );
            }
            return false;
        }

        bool CheckStreet(Card c1, Card c2, Card c3, Card card1, Card card2)
        {
            return
            (
            CheckStreetLine(c1, c2, c3, card1, card2)
            ||
            CheckStreetLine(card1, card2, c1, c2, c3)
            ||
            CheckStreetLine(c1, card1, card2, c2, c3)
            ||
            CheckStreetLine(c1, c2, card1, card2, c3)

            ||
            CheckStreetLine(card1, c1, c2, c3, card2)
            ||
            CheckStreetLine(c1, card1, c2, c3, card2)
            ||
            CheckStreetLine(c1, c2, card1, c3, card2)
            ||
            CheckStreetLine(c1, card1, c2, card2, c3)
            );
        }
        bool CheckStreet(Card c1, Card c2, Card c3, Card c4, Card card1, Card card2)
        {
            return
            (
                            CheckStreetLine(c1, c2, c3, card1, card2)
                            ||
                            CheckStreetLine(card1, card2, c1, c2, c3)
                            ||
                            CheckStreetLine(c1, card1, card2, c2, c3)
                            ||
                            CheckStreetLine(c1, c2, card1, card2, c3)
                            ||

                            CheckStreetLine(c1, c2, c4, card1, card2)
                            ||
                            CheckStreetLine(card1, card2, c1, c2, c4)
                            ||
                            CheckStreetLine(c1, card1, card2, c2, c4)
                            ||
                            CheckStreetLine(c1, c2, card1, card2, c4)
                            ||

                            CheckStreetLine(c1, c3, c4, card1, card2)
                            ||
                            CheckStreetLine(card1, card2, c1, c3, c4)
                            ||
                            CheckStreetLine(c1, card1, card2, c3, c4)
                            ||
                            CheckStreetLine(c1, c3, card1, card2, c4)
                            ||

                            CheckStreetLine(c2, c3, c4, card1, card2)
                            ||
                            CheckStreetLine(card1, card2, c2, c3, c4)
                            ||
                            CheckStreetLine(c2, card1, card2, c3, c4)
                            ||
                            CheckStreetLine(c2, c3, card1, card2, c4)
                            );
        }

        bool CheckStreetLine(Card c1, Card c2, Card c3, Card c4, Card c5)
        {
            return
            (
            c5.Rank - c4.Rank == 1 && c4.Rank - c3.Rank == 1 && c3.Rank - c2.Rank == 1 && c2.Rank - c1.Rank == 1
            ||
            c1.Rank - c2.Rank == 1 && c2.Rank - c3.Rank == 1 && c3.Rank - c4.Rank == 1 && c4.Rank - c5.Rank == 1
            );
        }

        bool CheckStreetDraw(Card c1, Card c2, Card c3, Card c4)
        {
            return
            (
            c4.Rank - c3.Rank == 1 && c3.Rank - c2.Rank == 1 && c2.Rank - c1.Rank == 1
            ||
            c1.Rank - c2.Rank == 1 && c2.Rank - c3.Rank == 1 && c3.Rank - c4.Rank == 1
            );
        }

        bool CheckHalfStreet(Card c1, Card c2, Card c3, Card c4, Card card1, Card card2)
        {
            if (c4 == null)
            {
                return
                    (
                        CheckStreetDraw(c1, c2, c3, card1)
                        ||
                        CheckStreetDraw(c1, c2, card1, c3)
                        ||
                        CheckStreetDraw(c1, card1, c2, c3)
                        ||
                        CheckStreetDraw(card1, c1, c2, c3)

                        ||
                        CheckStreetDraw(c1, c2, c3, card2)
                        ||
                        CheckStreetDraw(c1, c2, card2, c3)
                        ||
                        CheckStreetDraw(c1, card2, c2, c3)
                        ||
                        CheckStreetDraw(card2, c1, c2, c3)

                        ||
                        CheckStreetDraw(c1, c2, card1, card2)
                        ||
                        CheckStreetDraw(c1, card1, card2, c2)
                        ||
                        CheckStreetDraw(c1, card1, c2, card2)
                        ||
                        CheckStreetDraw(card1, card2, c1, c2)

                        ||
                        CheckStreetDraw(c1, c3, card1, card2)
                        ||
                        CheckStreetDraw(c1, card1, card2, c3)
                        ||
                        CheckStreetDraw(c1, card1, c3, card2)
                        ||
                        CheckStreetDraw(card1, card2, c1, c3)

                        ||
                        CheckStreetDraw(c2, c3, card1, card2)
                        ||
                        CheckStreetDraw(c2, card1, card2, c3)
                        ||
                        CheckStreetDraw(c2, card1, c3, card2)
                        ||
                        CheckStreetDraw(card1, card2, c2, c3)





                    );
            }
            if (c4 != null)
            {
                return
                    (
                    //=========================
                        CheckStreetDraw(c1, c2, c3, card1)
                        ||
                        CheckStreetDraw(c1, c2, card1, c3)
                        ||
                        CheckStreetDraw(c1, card1, c2, c3)
                        ||
                        CheckStreetDraw(card1, c1, c2, c3)

                        ||
                        CheckStreetDraw(c1, c2, c3, card2)
                        ||
                        CheckStreetDraw(c1, c2, card2, c3)
                        ||
                        CheckStreetDraw(c1, card2, c2, c3)
                        ||
                        CheckStreetDraw(card2, c1, c2, c3)

                        ||
                        CheckStreetDraw(c1, c2, c4, card2)
                        ||
                        CheckStreetDraw(c1, c2, card2, c4)
                        ||
                        CheckStreetDraw(c1, card2, c2, c4)
                        ||
                        CheckStreetDraw(card2, c1, c2, c4)

                        ||
                        CheckStreetDraw(c1, c3, c4, card2)
                        ||
                        CheckStreetDraw(c1, c3, card2, c4)
                        ||
                        CheckStreetDraw(c1, card2, c3, c4)
                        ||
                        CheckStreetDraw(card2, c1, c3, c4)

                        ||
                        CheckStreetDraw(c2, c3, c4, card2)
                        ||
                        CheckStreetDraw(c2, c3, card2, c4)
                        ||
                        CheckStreetDraw(c2, card2, c3, c4)
                        ||
                        CheckStreetDraw(card2, c2, c3, c4)

                        //===============
                        ||
                        CheckStreetDraw(c1, c2, card1, card2)
                        ||
                        CheckStreetDraw(c1, card1, card2, c2)
                        ||
                        CheckStreetDraw(c1, card1, c2, card2)
                        ||
                        CheckStreetDraw(card1, card2, c1, c2)

                        ||
                        CheckStreetDraw(c1, c3, card1, card2)
                        ||
                        CheckStreetDraw(c1, card1, card2, c3)
                        ||
                        CheckStreetDraw(c1, card1, c3, card2)
                        ||
                        CheckStreetDraw(card1, card2, c1, c3)

                        ||
                        CheckStreetDraw(c2, c3, card1, card2)
                        ||
                        CheckStreetDraw(c2, card1, card2, c3)
                        ||
                        CheckStreetDraw(c2, card1, c3, card2)
                        ||
                        CheckStreetDraw(card1, card2, c2, c3)

                        ||
                        CheckStreetDraw(c2, c4, card1, card2)
                        ||
                        CheckStreetDraw(c2, card1, card2, c4)
                        ||
                        CheckStreetDraw(c2, card1, c4, card2)
                        ||
                        CheckStreetDraw(card1, card2, c2, c4)

                        ||
                        CheckStreetDraw(c3, c4, card1, card2)
                        ||
                        CheckStreetDraw(c3, card1, card2, c4)
                        ||
                        CheckStreetDraw(c3, card1, c4, card2)
                        ||
                        CheckStreetDraw(card1, card2, c3, c4)
                    );
            }
            return false;
        }
        #endregion

        bool CheckTwoPairOld(Card c1, Card c2, Card c3, Card c4, Card c5, Card card1, Card card2)
        {
            if (c4 == null && c5 == null)
            {
                return c1.Rank == card1.Rank && (c2.Rank == card2.Rank || c3.Rank == card2.Rank)
                        ||
                        c2.Rank == card1.Rank && (c1.Rank == card2.Rank || c3.Rank == card2.Rank)
                        ||
                        c3.Rank == card1.Rank && (c1.Rank == card2.Rank || c2.Rank == card2.Rank);
            }
            if (c4 != null && c5 == null)
            {
                return
                    c1.Rank == card1.Rank && (c2.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c1.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c1.Rank == card2.Rank || c2.Rank == card2.Rank)
                    ||

                    c4.Rank == card1.Rank && (c2.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c4.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c4.Rank == card2.Rank || c2.Rank == card2.Rank)
                    ||

                    c1.Rank == card1.Rank && (c4.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c4.Rank == card1.Rank && (c1.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c1.Rank == card2.Rank || c4.Rank == card2.Rank)
                    ||

                    c1.Rank == card1.Rank && (c2.Rank == card2.Rank || c4.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c1.Rank == card2.Rank || c4.Rank == card2.Rank)
                    ||
                    c4.Rank == card1.Rank && (c1.Rank == card2.Rank || c2.Rank == card2.Rank);
            }
            if (c4 != null && c5 != null)
            {
                return
                    c1.Rank == card1.Rank && (c2.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c1.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c1.Rank == card2.Rank || c2.Rank == card2.Rank)
                    ||

                    c4.Rank == card1.Rank && (c2.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c4.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c4.Rank == card2.Rank || c2.Rank == card2.Rank)
                    ||

                    c1.Rank == card1.Rank && (c4.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c4.Rank == card1.Rank && (c1.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c1.Rank == card2.Rank || c4.Rank == card2.Rank)
                    ||

                    c1.Rank == card1.Rank && (c2.Rank == card2.Rank || c4.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c1.Rank == card2.Rank || c4.Rank == card2.Rank)
                    ||
                    c4.Rank == card1.Rank && (c1.Rank == card2.Rank || c2.Rank == card2.Rank)
                    ||

                    c1.Rank == card1.Rank && (c2.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c1.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c1.Rank == card2.Rank || c2.Rank == card2.Rank)
                    ||

                    c5.Rank == card1.Rank && (c2.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c5.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c5.Rank == card2.Rank || c2.Rank == card2.Rank)
                    ||

                    c1.Rank == card1.Rank && (c5.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c5.Rank == card1.Rank && (c1.Rank == card2.Rank || c3.Rank == card2.Rank)
                    ||
                    c3.Rank == card1.Rank && (c1.Rank == card2.Rank || c5.Rank == card2.Rank)
                    ||

                    c1.Rank == card1.Rank && (c2.Rank == card2.Rank || c5.Rank == card2.Rank)
                    ||
                    c2.Rank == card1.Rank && (c1.Rank == card2.Rank || c5.Rank == card2.Rank)
                    ||
                    c5.Rank == card1.Rank && (c1.Rank == card2.Rank || c2.Rank == card2.Rank)
                    ;
            }
            return false;
        }


    }
}