using System;
namespace miranda.ui
{
    public class PlayerStat
    {
        public int CheckCount { get; set; }
        public int RaiseCount { get; set; }
        public int CallCount { get; set; }
        public int FoldCount { get; set; }

        public int TotalCount { get; set; }

        public string GetStat()
        {
            return TotalCount + "-" + FoldCount + "-" + CheckCount + "-" + CallCount + "-" + RaiseCount
                + ";"
                + Math.Round(RaiseCount/((float)CallCount) * 100.0, 2) + "-"
                + Math.Round(RaiseCount / ((float)CheckCount) * 100.0, 2) + "-"
                + Math.Round(RaiseCount / ((float)(CallCount+CheckCount)) * 100.0, 2) + "-"
                + Math.Round(RaiseCount / ((float)(CallCount + CheckCount + FoldCount)) * 100.0, 2) + "-"

                + Math.Round(CallCount / ((float)FoldCount) * 100.0, 2) + "-"
                + Math.Round(CallCount / ((float)CheckCount) * 100.0, 2) + "-"
                + Math.Round(CallCount / ((float)(FoldCount + CheckCount)) * 100.0, 2) + "-"
                ;
        }

        public void Clear()
        {
            FoldCount = 0;
            CallCount = 0;
            CheckCount = 0;
            RaiseCount = 0;

            TotalCount = 0;
        }

        public void Collect(Player player)
        {
            if (player.IsActive || (!player.IsActive && !player.IsFold))
            {
                FoldCount += player.IsFold ? 1 : 0;
                CallCount += player.Action == PlayerAction.Call ? 1 : 0;
                CheckCount += player.Action == PlayerAction.Check ? 1 : 0;
                RaiseCount += (player.Action == PlayerAction.Raise || (!player.IsActive && !player.IsFold)) ? 1 : 0;

                TotalCount += 1;
            }
        }

        public bool IsAggressive
        {
            get
            {
                if (TotalCount < 80)
                    return false;

                if (CallCount == 0)
                    return false;
                return ((double)RaiseCount)/((double)CallCount) > 5 && FoldCount/((double)TotalCount) < 0.4;
            }
        }

        public bool IsNotAggressive
        {
            get
            {
                if (TotalCount < 80)
                    return false;

                if (CallCount == 0)
                    return false;
                return ((double)RaiseCount) / ((double)CallCount) < 1.5;
            }
        }
    }
}