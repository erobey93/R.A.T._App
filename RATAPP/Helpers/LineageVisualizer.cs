using System.Drawing;
using System.Windows.Forms;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;

namespace RATAPP.Helpers
{
    public class LineageVisualizer
    {
        private const int NODE_WIDTH = 150;
        private const int NODE_HEIGHT = 60;
        private const int VERTICAL_SPACING = 80;
        private const int HORIZONTAL_SPACING = 40;
        private readonly Font NODE_FONT = new Font("Segoe UI", 9F);
        private readonly Pen LINE_PEN = new Pen(Color.Black, 1);
        private readonly Color NODE_COLOR = Color.LightBlue;
        private readonly Color NODE_BORDER_COLOR = Color.DarkBlue;

        private readonly LineageService _lineageService;
        private readonly Panel _targetPanel;
        private readonly Graphics _graphics;

        public LineageVisualizer(Panel targetPanel, LineageService lineageService)
        {
            _targetPanel = targetPanel;
            _lineageService = lineageService;
            _graphics = _targetPanel.CreateGraphics();
        }

        public async Task DrawLineage(Animal animal, int startX, int startY, int generation = 0, int maxGenerations = 3)
        {
            if (animal == null || generation >= maxGenerations) return;

            // Draw current animal node
            DrawAnimalNode(animal, startX, startY);

            // Get parents
            var dam = await _lineageService.GetDamByAnimalId(animal.Id);
            var sire = await _lineageService.GetSireByAnimalId(animal.Id);

            if (dam != null)
            {
                int damX = startX - (NODE_WIDTH + HORIZONTAL_SPACING) * (1 << (maxGenerations - generation - 1));
                int damY = startY - VERTICAL_SPACING;
                
                // Draw line to dam
                _graphics.DrawLine(LINE_PEN, 
                    startX + NODE_WIDTH / 2, startY,
                    damX + NODE_WIDTH / 2, damY + NODE_HEIGHT);

                // Recursively draw dam's lineage
                await DrawLineage(dam, damX, damY, generation + 1, maxGenerations);
            }

            if (sire != null)
            {
                int sireX = startX + (NODE_WIDTH + HORIZONTAL_SPACING) * (1 << (maxGenerations - generation - 1));
                int sireY = startY - VERTICAL_SPACING;
                
                // Draw line to sire
                _graphics.DrawLine(LINE_PEN,
                    startX + NODE_WIDTH / 2, startY,
                    sireX + NODE_WIDTH / 2, sireY + NODE_HEIGHT);

                // Recursively draw sire's lineage
                await DrawLineage(sire, sireX, sireY, generation + 1, maxGenerations);
            }
        }

        private void DrawAnimalNode(Animal animal, int x, int y)
        {
            using (var brush = new SolidBrush(NODE_COLOR))
            using (var pen = new Pen(NODE_BORDER_COLOR))
            {
                // Draw node background
                _graphics.FillRectangle(brush, x, y, NODE_WIDTH, NODE_HEIGHT);
                _graphics.DrawRectangle(pen, x, y, NODE_WIDTH, NODE_HEIGHT);

                // Draw animal information
                var textBrush = new SolidBrush(Color.Black);
                var nameRect = new RectangleF(x + 5, y + 5, NODE_WIDTH - 10, NODE_HEIGHT - 10);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                
                string displayText = $"{animal.Name}\n{animal.Gender}";
                _graphics.DrawString(displayText, NODE_FONT, textBrush, nameRect, sf);
            }
        }

        public void Clear()
        {
            _graphics.Clear(_targetPanel.BackColor);
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            LINE_PEN?.Dispose();
            NODE_FONT?.Dispose();
        }
    }
}
