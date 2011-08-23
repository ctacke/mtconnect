using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OpenNETCF.MTConnect;

namespace SampleClient
{
    public partial class MainForm
    {
        private void InitializeDataPlotDragDrop()
        {
            dataPlot.DragEnter += new DragEventHandler(dataPlot_DragEnter);
            dataPlot.DragDrop += new DragEventHandler(dataPlot_DragDrop);
        }

        void dataPlot_DragDrop(object sender, DragEventArgs e)
        {
            var di = m_selectedNode.Tag as DataItem;
            if (di == null) return;

            if (dataPlot.Series.FindByName(di.Name) != null) return;

            var series = new Series(di.Name);
            series.ChartType = SeriesChartType.FastLine;
            series.Tag = di;

            dataPlot.Series.Add(series);
        }

        void dataPlot_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }
}
