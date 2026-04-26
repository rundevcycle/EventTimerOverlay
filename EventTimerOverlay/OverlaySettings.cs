using System.Collections.Generic;

namespace EventTimerOverlay
{
    public class OverlaySettings
    {
        public Dictionary<string, OverlayPosition> Positions { get; set; } = new();
    }

    public class OverlayPosition
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsVertical { get; set; }
    }
}