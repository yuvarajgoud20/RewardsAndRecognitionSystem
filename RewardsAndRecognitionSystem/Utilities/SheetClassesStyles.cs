
using DocumentFormat.OpenXml.Spreadsheet;

namespace RewardsAndRecognitionSystem.Utilities
{
    public class SheetClassesStyles
    {
        public static Cell CreateStyledCell(string text, uint styleIndex)
        {
            return new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(text),
                StyleIndex = styleIndex
            };
        }

        // Utility: Stylesheet
        public static Stylesheet CreateStylesheet()
        {
            return new Stylesheet(
                new Fonts(
                    new Font(), // 0 - Default
                    new Font(   // 1 - Bold white
                        new Bold(),
                        new Color() { Rgb = "FFFFFF" },
                        new FontSize() { Val = 11 })
                ),
                new Fills(
                    new Fill(new PatternFill() { PatternType = PatternValues.None }), // 0
                    new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }), // 1
                    new Fill(new PatternFill( // 2 - Indigo background
                        new ForegroundColor { Rgb = "3F51B5" },
                        new BackgroundColor { Indexed = 64 }
                    )
                    { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    new Border(), // 0 - Default
                    new Border(   // 1 - Thin border
                        new LeftBorder { Style = BorderStyleValues.Thin },
                        new RightBorder { Style = BorderStyleValues.Thin },
                        new TopBorder { Style = BorderStyleValues.Thin },
                        new BottomBorder { Style = BorderStyleValues.Thin })
                ),
                new CellFormats(
                    new CellFormat(), // 0 - Default
                    new CellFormat // 1 - Data cell with border
                    {
                        BorderId = 1,
                        ApplyBorder = true
                    },
                    new CellFormat // 2 - Header with fill, font, and border
                    {
                        FontId = 1,
                        FillId = 2,
                        BorderId = 1,
                        ApplyFill = true,
                        ApplyFont = true,
                        ApplyBorder = true
                    }
                )
            );
        }
    }
}
