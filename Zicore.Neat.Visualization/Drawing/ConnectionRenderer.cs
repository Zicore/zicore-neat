using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Zicore.Neat.IO.Model;

namespace Zicore.Neat.Visualization.Drawing
{
    public class ConnectionRenderer : RenderingComponent
    {
        public ExportConnectionGene ExportConnectionGene { get; }

        public ConnectionRenderer(ExportConnectionGene exportConnectionGene)
        {
            ExportConnectionGene = exportConnectionGene;
        }

        public ExportConnectionGene ConnectionGene { get; set; }

        protected override void OnRender(DrawingContext e)
        {
            
        }
    }
}
