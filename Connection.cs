using System.Collections.Generic;
using System.Drawing;

namespace FlowChartSVG
{
    public class Connection
    {
        public string Id { get; set; }

        public string SourceNodeId { get; set; }

        public string TargetNodeId { get; set; }

        public List<PointF> PathPoints { get; }
    }
}