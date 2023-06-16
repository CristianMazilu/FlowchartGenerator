using System;

namespace FlowChartSVG
{
    public static class ProjectExtensions
    {
        public static FlowChartDirection Opposite(this FlowChartDirection direction)
        {
            return direction switch
            {
                FlowChartDirection.TopDown => FlowChartDirection.BottomUp,
                FlowChartDirection.BottomUp => FlowChartDirection.TopDown,
                FlowChartDirection.RightToLeft => FlowChartDirection.LeftToRight,
                FlowChartDirection.LeftToRight => FlowChartDirection.RightToLeft,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), "Invalid FlowChartDirection value.")
            };
        }

        public static NodeHeading ToHeading(this FlowChartDirection direction)
        {
            return direction switch
            {
                FlowChartDirection.TopDown => NodeHeading.Bottom,
                FlowChartDirection.BottomUp => NodeHeading.Top,
                FlowChartDirection.RightToLeft => NodeHeading.Left,
                FlowChartDirection.LeftToRight => NodeHeading.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), "Invalid FlowChartDirection value.")
            };
        }

        public static NodeHeading Opposite(this NodeHeading heading)
        {
            return heading switch
            {
                NodeHeading.Bottom => NodeHeading.Top,
                NodeHeading.Top => NodeHeading.Bottom,
                NodeHeading.Left => NodeHeading.Right,
                NodeHeading.Right => NodeHeading.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(heading), "Invalid NodeHeading value.")
            };
        }
    }
}