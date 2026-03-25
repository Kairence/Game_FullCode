using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Server.Misc;

namespace Server.Engines.Reports
{
    public class HtmlRenderer
    {
        private static readonly string FtpHost = null;
        private static readonly string FtpUsername = null;
        private static readonly string FtpPassword = null;
        private static readonly string FtpStatsDirectory = null;
        private static readonly string FtpStaffDirectory = null;
        private readonly string m_Type;
        private readonly string m_Title;
        private readonly string m_OutputDirectory;
        private readonly DateTime m_TimeStamp;
        private readonly ObjectCollection m_Objects;

        public HtmlRenderer(string outputDirectory, Snapshot ss, SnapshotHistory history) : this(outputDirectory)
        {
            this.m_TimeStamp = ss.TimeStamp;
            this.m_Objects = new ObjectCollection();
            for (int i = 0; i < ss.Children.Count; ++i)
                this.m_Objects.Add(ss.Children[i]);

            this.m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Clients", 1, 100, 6));
            this.m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Items", 24, 9, 1));
            this.m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Players", 24, 9, 1));
            this.m_Objects.Add(BarGraph.OverTime(history, "General Stats", "NPCs", 24, 9, 1));
            this.m_Objects.Add(BarGraph.DailyAverage(history, "General Stats", "Clients"));
            this.m_Objects.Add(BarGraph.Growth(history, "General Stats", "Clients"));
        }

        public HtmlRenderer(string outputDirectory, StaffHistory history) : this(outputDirectory)
        {
            this.m_TimeStamp = DateTime.UtcNow;
            this.m_Objects = new ObjectCollection();
            history.Render(this.m_Objects);
        }

        private HtmlRenderer(string outputDirectory)
        {
            this.m_Type = outputDirectory;
            this.m_Title = (this.m_Type == "staff" ? "Staff" : "Stats");
            this.m_OutputDirectory = Path.Combine(Core.BaseDirectory, Config.Get("Reports.Path", "reports"));

            if (!Directory.Exists(this.m_OutputDirectory))
                Directory.CreateDirectory(this.m_OutputDirectory);

            this.m_OutputDirectory = Path.Combine(this.m_OutputDirectory, outputDirectory);

            if (!Directory.Exists(this.m_OutputDirectory))
                Directory.CreateDirectory(this.m_OutputDirectory);
        }

        public static string SafeFileName(string name) { return name.ToLower().Replace(' ', '_'); }

        public void Render()
        {
            Console.WriteLine("Reports: {0}: Render started", this.m_Title);
            this.RenderFull();
            for (int i = 0; i < this.m_Objects.Count; ++i)
                this.RenderSingle(this.m_Objects[i]);
            Console.WriteLine("Reports: {0}: Render complete", this.m_Title);
        }

        public void RenderFull()
        {
            string filePath = Path.Combine(this.m_OutputDirectory, "reports.html");
            StringBuilder sb = new StringBuilder();
            this.RenderFull(sb);
            File.WriteAllText(filePath, sb.ToString());

            string cssPath = Path.Combine(this.m_OutputDirectory, "styles.css");
            if (!File.Exists(cssPath))
            {
                File.WriteAllText(cssPath, "body { background-color: #FFFFFF; font-family: verdana, arial; font-size: 11px; }\n" +
                    "a { color: #28435E; }\ntd.header { background-color: #9696AA; font-weight: bold; font-size: 12px; }\n" +
                    "td.entry { background-color: #FFFFFF; }\n.tbl-border { background-color: #46465A; }");
            }
        }

        public void RenderFull(StringBuilder sb)
        {
            sb.Append("<html><head><title>").Append(ServerList.ServerName).Append(" Statistics</title>");
            sb.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"styles.css\"></head><body>");

            for (int i = 0; i < this.m_Objects.Count; ++i)
            {
                this.RenderDirect(this.m_Objects[i], sb);
                sb.Append("<br><br>");
            }

            sb.Append("<center>Snapshot taken at ").AppendFormat("{0:d} {0:t}", m_TimeStamp).Append("</center></body></html>");
        }

        public void RenderSingle(PersistableObject obj)
        {
            string filePath = Path.Combine(this.m_OutputDirectory, SafeFileName(this.FindNameFrom(obj)) + ".html");
            StringBuilder sb = new StringBuilder();
            this.RenderSingle(obj, sb);
            File.WriteAllText(filePath, sb.ToString());
        }

        public void RenderSingle(PersistableObject obj, StringBuilder sb)
        {
            sb.Append("<html><head><title>").Append(ServerList.ServerName).Append(" - ").Append(FindNameFrom(obj)).Append("</title>");
            sb.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"styles.css\"></head><body><center>");
            this.RenderDirect(obj, sb);
            sb.Append("<br>Snapshot taken at ").AppendFormat("{0:d} {0:t}", m_TimeStamp).Append("</center></body></html>");
        }

        public void RenderDirect(PersistableObject obj, StringBuilder sb)
        {
            if (obj is Report) this.RenderReport(obj as Report, sb);
            else if (obj is BarGraph) this.RenderBarGraph(obj as BarGraph, sb);
            else if (obj is PieChart) this.RenderPieChart(obj as PieChart, sb);
        }

        private string FindNameFrom(PersistableObject obj)
        {
            if (obj is Report) return (obj as Report).Name;
            if (obj is Chart) return (obj as Chart).Name;
            return "Invalid";
        }

        private void RenderPieChart(PieChart chart, StringBuilder sb)
        {
            PieChartRenderer pieChart = new PieChartRenderer(Color.White);
            pieChart.ShowPercents = chart.ShowPercents;
            string[] labels = new string[chart.Items.Count];
            string[] values = new string[chart.Items.Count];
            for (int i = 0; i < chart.Items.Count; ++i) { labels[i] = chart.Items[i].Name; values[i] = chart.Items[i].Value.ToString(); }
            pieChart.CollectDataPoints(labels, values);
            using (Bitmap bmp = pieChart.Draw())
            {
                string fileName = chart.FileName + ".png";
                bmp.Save(Path.Combine(this.m_OutputDirectory, fileName), ImageFormat.Png);
                sb.AppendFormat("<table cellpadding=0 cellspacing=0 border=0><tr><td class=\"tbl-border\"><table cellpadding=4 cellspacing=1>");
                sb.AppendFormat("<tr><td colspan=10 align=center class=\"header\">{0}</td></tr>", chart.Name);
                sb.AppendFormat("<tr><td class=\"entry\"><img src=\"{0}\" width={1} height={2}></td></tr></table></td></tr></table>", fileName, bmp.Width, bmp.Height);
            }
        }

        private void RenderBarGraph(BarGraph graph, StringBuilder sb)
        {
            BarGraphRenderer barGraph = new BarGraphRenderer(Color.White);
            barGraph.RenderMode = graph.RenderMode;
            barGraph._regions = graph.Regions;
            barGraph.SetTitles(graph.xTitle, null);
            if (graph.yTitle != null) barGraph.VerticalLabel = graph.yTitle;
            barGraph.FontColor = Color.Black;
            barGraph.ShowData = (graph.Interval == 1);
            barGraph.VerticalTickCount = graph.Ticks;
            string[] labels = new string[graph.Items.Count];
            string[] values = new string[graph.Items.Count];
            for (int i = 0; i < graph.Items.Count; ++i) { labels[i] = graph.Items[i].Name; values[i] = graph.Items[i].Value.ToString(); }
            barGraph._interval = graph.Interval;
            barGraph.CollectDataPoints(labels, values);
            using (Bitmap bmp = barGraph.Draw())
            {
                string fileName = graph.FileName + ".png";
                bmp.Save(Path.Combine(this.m_OutputDirectory, fileName), ImageFormat.Png);
                sb.AppendFormat("<table cellpadding=0 cellspacing=0 border=0><tr><td class=\"tbl-border\"><table cellpadding=4 cellspacing=1>");
                sb.AppendFormat("<tr><td colspan=10 align=center class=\"header\">{0}</td></tr>", graph.Name);
                sb.AppendFormat("<tr><td class=\"entry\"><img src=\"{0}\" width={1} height={2}></td></tr></table></td></tr></table>", fileName, bmp.Width, bmp.Height);
            }
        }

        private void RenderReport(Report report, StringBuilder sb)
        {
            sb.AppendFormat("<table width=\"{0}\" cellpadding=0 cellspacing=0 border=0><tr><td class=\"tbl-border\"><table width=\"100%\" cellpadding=4 cellspacing=1>", report.Width);
            sb.AppendFormat("<tr><td colspan=10 align=center class=\"header\">{0}</td></tr>", report.Name);

            bool isNamed = false;
            for (int i = 0; i < report.Columns.Count && !isNamed; ++i) isNamed = (report.Columns[i].Name != null);

            if (isNamed)
            {
                sb.Append("<tr>");
                foreach (var col in report.Columns)
                    sb.AppendFormat("<td class=\"header\" width=\"{0}\" align=\"{1}\">{2}</td>", col.Width, col.Align, col.Name);
                sb.Append("</tr>");
            }

            foreach (var item in report.Items)
            {
                sb.Append("<tr>");
                for (int j = 0; j < item.Values.Count; ++j)
                    sb.AppendFormat("<td class=\"entry\" align=\"{0}\">{1}</td>", report.Columns[j].Align, item.Values[j].Value);
                sb.Append("</tr>");
            }
            sb.Append("</table></td></tr></table>");
        }

		public void Upload()
        {
            if (FtpHost == null)
                return;

            Console.WriteLine("Reports: {0}: Upload started", this.m_Title);

            string filePath = Path.Combine(this.m_OutputDirectory, "upload.ftp");

            try
            {
                using (StreamWriter op = new StreamWriter(filePath))
                {
                    op.WriteLine("open \"{0}\"", FtpHost);
                    op.WriteLine(FtpUsername);
                    op.WriteLine(FtpPassword);
                    op.WriteLine("cd \"{0}\"", (this.m_Type == "staff" ? FtpStaffDirectory : FtpStatsDirectory));
                    op.WriteLine("mput \"{0}\"", Path.Combine(this.m_OutputDirectory, "*.html"));
                    op.WriteLine("mput \"{0}\"", Path.Combine(this.m_OutputDirectory, "*.css"));
                    op.WriteLine("binary");
                    op.WriteLine("mput \"{0}\"", Path.Combine(this.m_OutputDirectory, "*.png"));
                    op.WriteLine("disconnect");
                    op.Write("quit");
                }

                ProcessStartInfo psi = new ProcessStartInfo();

                psi.FileName = "ftp";
                psi.Arguments = String.Format("-i -s:\"{0}\"", filePath);

                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                using (Process p = Process.Start(psi))
                {
                    if (p != null)
                        p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Reports: Upload Error: {0}", ex.Message);
            }
            finally
            {
                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch { }
            }

            Console.WriteLine("Reports: {0}: Upload complete", this.m_Title);
        }
    }
}
