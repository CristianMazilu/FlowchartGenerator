using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using SkiaSharp;
using Svg;
using Svg.Transforms;

namespace FlowChartSVG
{
    public enum FlowChartDirection
    {
        TopDown,
        BottomUp,
        RightToLeft,
        LeftToRight
    }

    public class FlowChart
    {
        public int DocumentPadding;
        public int NeighborPadding;
        public int TextPadding;
        public SKFont DocumentFont = new SKFont(SKTypeface.Default, 24);
        private const int HalfDivisor = 2;

        public FlowChart(FlowChartDirection direction = FlowChartDirection.TopDown, int documentPadding = 40, int neighborPadding = 20, int textPadding = 10, SKFont font = null)
        {
            Direction = direction;
            DocumentPadding = documentPadding;
            NeighborPadding = neighborPadding;
            TextPadding = textPadding;
            if (font == null)
            {
                return;
            }

            DocumentFont = font;
        }

        public Node Head { get; set; }

        public List<Connection> Connections { get; } = new List<Connection>();

        public FlowChartDirection Direction { get; }

        public Node AddNode(Node node)
        {
            if (Head == null)
            {
                Head = node;
                return Head;
            }

            Head.AddChild(node);
            return node;
        }

        public void UpdateNode(Node node)
        {
            throw new NotImplementedException();
        }

        public void RemoveNode(string nodeId)
        {
            throw new NotImplementedException();
        }

        public void AddConnection(Connection connection)
        {
            throw new NotImplementedException();
        }

        public void UpdateConnection(Connection connection)
        {
            throw new NotImplementedException();
        }

        public void RemoveConnection(string connectionId)
        {
            throw new NotImplementedException();
        }

        public string GenerateSvgContent(Stream fs = null)
        {
            var svgDocument = new SvgDocument();
            var svgGroup = new SvgGroup();

            // Render nodes
            svgDocument.Children.Add(Head.GenerateSvgGroup());
            foreach (var node in Head)
            {
                svgDocument.Children.Add(node.GenerateSvgGroup());
            }

            svgDocument.ViewBox = Direction switch
            {
                FlowChartDirection.TopDown =>
                    new SvgViewBox(
                        Head.X - Head.TotalWidth / HalfDivisor + Head.Width / HalfDivisor - DocumentPadding,
                        Head.Y - DocumentPadding,
                        Head.TotalWidth + DocumentPadding * HalfDivisor,
                        Head.TotalHeight + DocumentPadding * HalfDivisor),
                FlowChartDirection.LeftToRight =>
                    new SvgViewBox(
                        Head.X - DocumentPadding,
                        Head.Y - Head.TotalHeight / HalfDivisor + Head.Height / HalfDivisor - DocumentPadding,
                        Head.TotalWidth + DocumentPadding * HalfDivisor,
                        Head.TotalHeight + DocumentPadding * HalfDivisor),
                FlowChartDirection.BottomUp =>
                    new SvgViewBox(
                        Head.X - Head.TotalWidth / HalfDivisor + Head.Width / HalfDivisor - DocumentPadding,
                        Head.Y - Head.TotalHeight - DocumentPadding,
                        Head.TotalWidth + DocumentPadding * HalfDivisor,
                        Head.TotalHeight + DocumentPadding * HalfDivisor),
                FlowChartDirection.RightToLeft =>
                    new SvgViewBox(
                        Head.X - Head.TotalWidth - DocumentPadding,
                        Head.Y - Head.TotalHeight / HalfDivisor + Head.Height / HalfDivisor - DocumentPadding,
                        Head.TotalWidth + DocumentPadding * HalfDivisor,
                        Head.TotalHeight + DocumentPadding * HalfDivisor)
            };

            using var stream = new MemoryStream();
            svgDocument.Write(stream);
            svgDocument.Write(fs);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}