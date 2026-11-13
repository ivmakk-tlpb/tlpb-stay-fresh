using System.Collections.Generic;

namespace StayFresh
{
    // The place of the spoilage line among the tooltip lines: right after the weight line (a number with the weight
    // sprite, under the description), else above the hint at the end ("Right click for options", which starts with a
    // blank line). The lines are in their sibling order, with null for a line that is not shown.
    public static class TooltipOrder
    {
        // The sibling index for the own line, or -1 to leave it where it is.
        public static int Target(IReadOnlyList<string> lines, int own)
        {
            for (int i = 0; i < own; i++)
                if (lines[i] != null && lines[i].Contains("<sprite=")) return i + 1 == own ? -1 : i + 1;
            for (int i = 0; i < own; i++)
                if (lines[i] != null && lines[i].StartsWith("\n")) return i;
            return -1;
        }
    }
}
