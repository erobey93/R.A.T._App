using PdfSharp;
namespace RATAPPLibrary.Services
{
    public class PdfTemplate
    {
        public string Name { get; set; }
        public PageSize PageSize { get; set; } = PageSize.A4;
        public PdfMargin Margins { get; set; } = new PdfMargin();
        public List<PdfElement> Elements { get; set; } = new List<PdfElement>();
        public Dictionary<string, string> DefaultValues { get; set; } = new Dictionary<string, string>();
    }

    public class PdfMargin
    {
        public float Left { get; set; } = 20;
        public float Right { get; set; } = 20;
        public float Top { get; set; } = 20;
        public float Bottom { get; set; } = 20;
    }

    public abstract class PdfElement
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public class TextElement : PdfElement
    {
        public string Text { get; set; }
        public string FontFamily { get; set; } = "Times New Roman";
        public float FontSize { get; set; } = 12;
        public bool IsBold { get; set; }
        public string Color { get; set; } = "#000000";
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        public string DataField { get; set; }
    }

    public class ImageElement : PdfElement
    {
        public string ImagePath { get; set; }
        public string DataField { get; set; } // For dynamic images
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }
}