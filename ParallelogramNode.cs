using System;
using Svg;
using System.Drawing;

namespace FlowChartSVG
{
    public class ParallelogramNode : Node
    {
        private const float DefaultSkewDistance = 10;

        public ParallelogramNode(FlowChart flowChart, float width, float height, float skewDistance = DefaultSkewDistance) : base(flowChart, width, height)
        {
            SkewDistance = skewDistance;
        }

        public ParallelogramNode(FlowChart flowChart, string nodeContent = "empty", float skewDistance = DefaultSkewDistance) : base(flowChart, nodeContent)
        {
            SkewDistance = skewDistance;
        }

        public ParallelogramNode(FlowChart flowChart, Node parent, string nodeContent = "empty", float skewDistance = DefaultSkewDistance) : base(flowChart, parent, nodeContent)
        {
            SkewDistance = skewDistance;
        }

        public float SkewDistance { get; set; }

        public override float Width
        {
            get
            {
                return constructorWidth == null
                    ? GetTextWidthAndHeight(Content, parentFlowChart.DocumentFont).Width +
                      parentFlowChart.TextPadding * HalfDivisor + SkewDistance
                    : (float)constructorWidth + SkewDistance;
            }

            set
            {
                constructorWidth = value;
            }
        }

        public override (float X, float Y) GetEdgePoint(NodeHeading nodeHeading)
        {
            return nodeHeading switch
            {
                NodeHeading.Top => (X + Width / HalfDivisor, Y),
                NodeHeading.Bottom => (X + Width / HalfDivisor, Y + Height),
                NodeHeading.Left => (X + SkewDistance / HalfDivisor, Y + Height / HalfDivisor),
                NodeHeading.Right => (X + Width - SkewDistance / HalfDivisor, Y + Height / HalfDivisor),
                _ => throw new ArgumentOutOfRangeException(nameof(nodeHeading), "Invalid heading value.")
            };
        }

        protected override SvgElement GenerateTextBox()
        {
            return new SvgPolygon
            {
                Points = new SvgPointCollection
                {
                    new SvgUnit(X + SkewDistance),
                    new SvgUnit(Y),
                    new SvgUnit(X + Width),
                    new SvgUnit(Y),
                    new SvgUnit(X + Width - SkewDistance),
                    new SvgUnit(Y + Height),
                    new SvgUnit(X),
                    new SvgUnit(Y + Height)
                },

                Stroke = new SvgColourServer(Color.Black),
                StrokeWidth = 1,
                Fill = new SvgColourServer(Color.LightGray)
            };
        }
    }
}