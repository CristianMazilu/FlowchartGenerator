using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Xml.XPath;
using FlowChartSVG;
using SkiaSharp;
using Svg;

namespace FlowChartSVG
{
    public enum NodeHeading
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public class Node : IEnumerable<Node>
    {
        protected const int HalfDivisor = 2;
        protected readonly FlowChart parentFlowChart;
        protected float? constructorHeight;
        protected float? constructorWidth;

        private const int DefaultEdgePointRadius = 2;
        private const double DefaultFontDisplacement = 0.3;

        private float x;
        private float y;

        public Node(FlowChart flowChart, float width, float height)
        {
            ValidateParameter(flowChart);
            ValidateParameter(width);
            ValidateParameter(height);
            parentFlowChart = flowChart;
            X = flowChart.DocumentPadding;
            Y = flowChart.DocumentPadding;
            Width = width;
            Height = height;
        }

        public Node(FlowChart flowChart, string nodeContent = "empty")
        {
            ValidateParameter(flowChart);
            ValidateParameter(nodeContent);
            parentFlowChart = flowChart;
            X = flowChart.DocumentPadding;
            Y = flowChart.DocumentPadding;
            Content = nodeContent;
        }

        public Node(FlowChart flowChart, Node parent, string nodeContent = "empty")
        {
            ValidateParameter(flowChart);
            ValidateParameter(parent);
            ValidateParameter(nodeContent);
            parentFlowChart = flowChart;
            Parent = parent;
            Content = nodeContent;
        }

        public Node Parent { get; }

        public string Content { get; set; }

        public float X
        {
            get
            {
                if (Parent == null)
                {
                    return x;
                }

                return parentFlowChart.Direction switch
                {
                    FlowChartDirection.TopDown => Parent.DefaultParentAnchor.X +
                                                  -Parent.TotalWidth / HalfDivisor +
                                                  TotalWidth / HalfDivisor -
                                                  Width / HalfDivisor +
                                                  XBleedForCenterAlignment() +
                                                  SumPreviousSiblingsSecondaryDimension(),
                    FlowChartDirection.BottomUp => Parent.DefaultParentAnchor.X +
                                                   -Parent.TotalWidth / HalfDivisor +
                                                   TotalWidth / HalfDivisor -
                                                   Width / HalfDivisor +
                                                   XBleedForCenterAlignment() +
                                                   SumPreviousSiblingsSecondaryDimension(),
                    FlowChartDirection.LeftToRight => Parent.DefaultParentAnchor.X + parentFlowChart.NeighborPadding,
                    FlowChartDirection.RightToLeft => Parent.DefaultParentAnchor.X - parentFlowChart.NeighborPadding -
                                                      Width
                };
            }

            set
            {
                x = value;
            }
        }

        public float Y
        {
            get
            {
                if (Parent == null)
                {
                    return y;
                }

                return parentFlowChart.Direction switch
                {
                    FlowChartDirection.LeftToRight => Parent.DefaultParentAnchor.Y +
                                                      -Parent.TotalHeight / HalfDivisor +
                                                      TotalHeight / HalfDivisor -
                                                      Height / HalfDivisor +
                                                      YBleedForCenterAlignment() +
                                                      SumPreviousSiblingsSecondaryDimension(),
                    FlowChartDirection.RightToLeft => Parent.DefaultParentAnchor.Y +
                                                      -Parent.TotalHeight / HalfDivisor +
                                                      TotalHeight / HalfDivisor -
                                                      Height / HalfDivisor +
                                                      YBleedForCenterAlignment() +
                                                      SumPreviousSiblingsSecondaryDimension(),
                    FlowChartDirection.TopDown => Parent.DefaultParentAnchor.Y +
                                                  parentFlowChart.NeighborPadding,
                    FlowChartDirection.BottomUp => Parent.DefaultParentAnchor.Y -
                                                   parentFlowChart.NeighborPadding -
                                                   Height
                };
            }

            set
            {
                y = value;
            }
        }

        public virtual float Width
        {
            get
            {
                return constructorWidth == null ? GetTextWidthAndHeight(Content, parentFlowChart.DocumentFont).Width + parentFlowChart.TextPadding * HalfDivisor : (float)constructorWidth;
            }

            set
            {
                constructorWidth = value;
            }
        }

        public float TotalWidth
        {
            get
            {
                return parentFlowChart.Direction switch
                {
                    FlowChartDirection.LeftToRight =>
                        LastChild == null ?
                            Width + parentFlowChart.NeighborPadding :
                            Width + HalfDivisor * parentFlowChart.NeighborPadding +
                            this.OrderByDescending(node => node.TotalWidth).First().TotalWidth,
                    FlowChartDirection.RightToLeft =>
                        LastChild == null ?
                            Width + parentFlowChart.NeighborPadding :
                            Width + HalfDivisor * parentFlowChart.NeighborPadding +
                            this.OrderByDescending(node => node.TotalWidth).First().TotalWidth,
                    FlowChartDirection.TopDown =>
                        LastChild == null ?
                            Width + parentFlowChart.NeighborPadding :
                            Math.Max(
                                this.SumChildrenWidths(),
                                Width + parentFlowChart.NeighborPadding),
                    FlowChartDirection.BottomUp =>
                        LastChild == null ?
                            Width + parentFlowChart.NeighborPadding :
                            Math.Max(
                                this.SumChildrenWidths(),
                                Width + parentFlowChart.NeighborPadding),
                };
            }
        }

        public float Height
        {
            get
            {
                return constructorHeight == null ? GetTextWidthAndHeight(Content, parentFlowChart.DocumentFont).Height + parentFlowChart.TextPadding : (float)constructorHeight;
            }

            set
            {
                constructorHeight = value;
            }
        }

        public float TotalHeight
        {
            get
            {
                return parentFlowChart.Direction switch
                {
                    FlowChartDirection.LeftToRight =>
                        LastChild == null ?
                            Height + parentFlowChart.NeighborPadding :
                            Math.Max(
                                this.SumChildrenHeights(),
                                Height + parentFlowChart.NeighborPadding),
                    FlowChartDirection.RightToLeft =>
                        LastChild == null ?
                            Height + parentFlowChart.NeighborPadding :
                            Math.Max(
                                this.SumChildrenHeights(),
                                Height + parentFlowChart.NeighborPadding),
                    FlowChartDirection.TopDown => LastChild == null ?
                        Height + parentFlowChart.NeighborPadding :
                        Height + HalfDivisor * parentFlowChart.NeighborPadding +
                        this.OrderByDescending(node => node.TotalHeight).First().TotalHeight,
                    FlowChartDirection.BottomUp =>
                        LastChild == null ?
                            Height + parentFlowChart.NeighborPadding :
                            Height + HalfDivisor * parentFlowChart.NeighborPadding +
                            this.OrderByDescending(node => node.TotalHeight).First().TotalHeight
                };
            }
        }

        public (float X, float Y) DefaultParentAnchor => GetAnchor(parentFlowChart.Direction);

        public (float X, float Y) DefaultChildAnchor => GetAnchor(parentFlowChart.Direction.Opposite());

        public (float X, float Y) DefaultParentEdgePoint => GetEdgePoint(parentFlowChart.Direction.ToHeading());

        public (float X, float Y) DefaultChildEdgePoint => GetEdgePoint(parentFlowChart.Direction.Opposite().ToHeading());

        protected Node LastChild { get; set; }

        protected Node NextSibling { get; set; }

        protected Node PreviousSibling { get; set; }

        public IEnumerator<Node> GetEnumerator()
        {
            for (var child = LastChild; child != null; child = child.PreviousSibling)
            {
                yield return child;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Node AddChild(Node childNode)
        {
            if (LastChild == null)
            {
                LastChild = childNode;
                return childNode;
            }

            LastChild.NextSibling = childNode;
            childNode.PreviousSibling = LastChild;
            LastChild = childNode;
            return childNode;
        }

        public Node FindNode(string content)
        {
            if (Content == content)
            {
                return this;
            }

            foreach (var child in this)
            {
                if (child.Content == content)
                {
                    return child;
                }

                var foundNode = child.FindNode(content);

                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }

        public Node AddChild(string childNodeContent)
        {
            // Create a new child node with the specified content
            var childNode = new Node(parentFlowChart, this, childNodeContent);

            return AddChild(childNode);
        }

        public (float X, float Y) GetAnchor(FlowChartDirection direction)
        {
            return direction switch
            {
                FlowChartDirection.TopDown => (
                    X + Width / HalfDivisor,
                    Y + Height + parentFlowChart.NeighborPadding),
                FlowChartDirection.BottomUp => (
                    X + Width / HalfDivisor,
                    Y - parentFlowChart.NeighborPadding),
                FlowChartDirection.LeftToRight => (
                    X + Width + parentFlowChart.NeighborPadding,
                    Y + Height / HalfDivisor),
                FlowChartDirection.RightToLeft => (
                    X - parentFlowChart.NeighborPadding,
                    Y + Height / HalfDivisor),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }

        public virtual (float X, float Y) GetEdgePoint(NodeHeading nodeHeading)
        {
            return nodeHeading switch
            {
                NodeHeading.Top => (X + Width / HalfDivisor, Y),
                NodeHeading.Bottom => (X + Width / HalfDivisor, Y + Height),
                NodeHeading.Left => (X, Y + Height / HalfDivisor),
                NodeHeading.Right => (X + Width, Y + Height / HalfDivisor),
                _ => throw new ArgumentOutOfRangeException(nameof(nodeHeading), "Invalid heading value.")
            };
        }

        public SvgGroup GenerateSvgGroup()
        {
            var svgGroup = new SvgGroup();

            foreach (var edgePoint in GenerateEdgePoints())
            {
                svgGroup.Children.Add(edgePoint);
            }

            svgGroup.Children.Add(GenerateAnchor());

            foreach (var connection in GenerateConnections())
            {
                svgGroup.Children.Add(connection);
            }

            foreach (var childNode in this)
            {
                svgGroup.Children.Add(childNode.GenerateSvgGroup());
            }

            svgGroup.Children.Add(GenerateTextBox());
            svgGroup.Children.Add(GenerateText());

            return svgGroup;
        }

        protected float SumChildrenWidths()
        {
            return this.Aggregate(
                (float)0,
                (total, next) => total + next.TotalWidth);
        }

        protected float SumChildrenHeights()
        {
            return this.Aggregate(
                (float)0,
                (total, next) => total + next.TotalHeight);
        }

        protected (float Width, float Height) GetTextWidthAndHeight(string text, SKFont font)
        {
            font ??= parentFlowChart.DocumentFont;

            using (var paint = new SKPaint())
            {
                paint.Typeface = font.Typeface;
                paint.TextSize = font.Size;

                // Measure the text's width and height
                var widthBounds = new SKRect();
                var heightBounds = new SKRect();
                float measuredWidth = paint.MeasureText(text, ref widthBounds);
                paint.MeasureText("childafgefrgaer2", ref heightBounds); // This is to all boxes have the same height
                float measuredHeight = heightBounds.Height;

                return (measuredWidth, measuredHeight);
            }
        }

        protected virtual SvgElement GenerateTextBox()
        {
            return new SvgRectangle
            {
                X = new SvgUnit(X),
                Y = new SvgUnit(Y),
                Width = new SvgUnit(Width),
                Height = new SvgUnit(Height),
                Stroke = new SvgColourServer(Color.Black),
                StrokeWidth = 1,
                Fill = new SvgColourServer(Color.LightGray)
            };
        }

        private static void ValidateParameter(object parameter)
        {
            if (parameter != null)
            {
                return;
            }

            throw new ArgumentNullException(nameof(parameter));
        }

        private float SumPreviousSiblingsSecondaryDimension()
        {
            float total = 0;
            for (var previousSibling = PreviousSibling;
                 previousSibling != null;
                 previousSibling = previousSibling.PreviousSibling)
            {
                total +=
                    (parentFlowChart.Direction == FlowChartDirection.BottomUp ||
                     parentFlowChart.Direction == FlowChartDirection.TopDown)
                        ? previousSibling.TotalWidth
                        : previousSibling.TotalHeight;
            }

            return total <= 0 ? 0 : total;
        }

        private float XBleedForCenterAlignment()
        {
            if (Parent == null)
            {
                return 0;
            }

            if (Parent.TotalWidth < Parent.SumChildrenWidths())
            {
                return 0;
            }

            return (Parent.TotalWidth - Parent.SumChildrenWidths()) / HalfDivisor;
        }

        private float YBleedForCenterAlignment()
        {
            if (Parent == null)
            {
                return 0;
            }

            if (Parent.TotalHeight < Parent.SumChildrenHeights())
            {
                return 0;
            }

            return (Parent.TotalHeight - Parent.SumChildrenHeights()) / HalfDivisor;
        }

        private SvgText GenerateText()
        {
            return new SvgText(Content)
            {
                X = { new SvgUnit(X + Width / HalfDivisor) },
                Y = { new SvgUnit(Y + Height / HalfDivisor) },
                TextAnchor = SvgTextAnchor.Middle,
                FontFamily = parentFlowChart.DocumentFont.Typeface.ToString(),
                FontSize = parentFlowChart.DocumentFont.Size,
                Dy = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Em, (float)DefaultFontDisplacement) }
            };
        }

        private SvgCircle GenerateAnchor()
        {
            return new SvgCircle()
            {
                CenterX = DefaultParentAnchor.X,
                CenterY = DefaultParentAnchor.Y,
                Radius = DefaultEdgePointRadius,
                Fill = new SvgColourServer(Color.OrangeRed)
            };
        }

        private IEnumerable<SvgCircle> GenerateEdgePoints()
        {
            foreach (var iterateHeading in Enum.GetValues(typeof(NodeHeading)))
            {
                var edgePointCoordinates = this.GetEdgePoint((NodeHeading)iterateHeading);
                yield return new SvgCircle()
                {
                    CenterX = edgePointCoordinates.X,
                    CenterY = edgePointCoordinates.Y,
                    Radius = DefaultEdgePointRadius,
                    Fill = new SvgColourServer(Color.OrangeRed)
                };
            }
        }

        private IEnumerable<SvgPolyline> GenerateConnections()
        {
            foreach (var child in this)
            {
                var polylinePoints = new SvgPointCollection
                {
                    new SvgUnit(DefaultParentEdgePoint.X),
                    new SvgUnit(DefaultParentEdgePoint.Y),
                    new SvgUnit(DefaultParentAnchor.X),
                    new SvgUnit(DefaultParentAnchor.Y),
                    new SvgUnit(child.DefaultChildAnchor.X),
                    new SvgUnit(child.DefaultChildAnchor.Y),
                    new SvgUnit(child.DefaultChildEdgePoint.X),
                    new SvgUnit(child.DefaultChildEdgePoint.Y)
                };

                yield return new SvgPolyline
                {
                    Points = polylinePoints,
                    Stroke = new SvgColourServer(Color.Black),
                    StrokeWidth = 1,
                    Fill = SvgPaintServer.None
                };
            }
        }
    }
}