using Eto.Forms;

namespace PathFinder.Gui.Widgets
{
    public class StatsWidget : StackLayout
    {
        private readonly Label _status;
        private readonly Label _fps;
        private readonly Label _tps;
        private readonly Label _openPoints;
        private readonly Label _closedPoints;
        
        private double _lastTps;
        private double _lastFps;

        public StatsWidget()
        {
            _status = new Label {Text = "TPF: N/A"};
            _tps = new Label {Text = "TPS: N/A"};
            _fps = new Label {Text = "FPS: N/A"};
            _openPoints = new Label {Text = "Open Points: N/A"};
            _closedPoints = new Label {Text = "Closed Points: N/A"};


            static StackLayoutItem HStretched(Control c) =>
                new() {Control = c, HorizontalAlignment = HorizontalAlignment.Stretch};
            
            Items.Add(HStretched(_status));
            Items.Add(HStretched(_tps));
            Items.Add(HStretched(_fps));
            Items.Add(HStretched(_openPoints));
            Items.Add(HStretched(_closedPoints));
        }
        
        public void UpdateRunningStats(FrameData frameData)
        {
            _lastFps = 1 / frameData.FrameSeconds * 0.1d + _lastFps * 0.9d;
            _lastTps = frameData.ClosedCount / frameData.OverallSeconds;
            _status.Text = "Running";
            _tps.Text = $"TPS: {_lastTps:N0}";
            _fps.Text = $"FPS: {_lastFps:N0}";
            _openPoints.Text = $"Open Points: {frameData.OpenCount:N0}";
            _closedPoints.Text = $"Closed Points: {frameData.ClosedCount:N0}";
        }

        public void UpdateSuccessStats(FrameData frameData)
        {
            _status.Text = "Path Found";
            _fps.Text = $"Time: {frameData.OverallSeconds:N3}";
            _tps.Text =
                $"TPS: {frameData.ClosedCount / frameData.OverallSeconds:N2} ({frameData.ClosedCount:N0})";
            _openPoints.Text = $"Path Length {frameData.Path?.Count:N0}";
            _closedPoints.Text = $"Path Cost: {frameData.PathCost:N2}";
        }

        public void UpdateFailureStats(FrameData frameData)
        {
            _status.Text = "Failed to find a path";
            _fps.Text = $"Time: {frameData.OverallSeconds:N3}";
            _tps.Text = $"TPS: {frameData.ClosedCount / frameData.OverallSeconds:N2}";
            _openPoints.Text = $"Open Points: {frameData.OpenCount:N0}";
            _closedPoints.Text = $"Closed Points: {frameData.ClosedCount:N0}";
        }
    }
}