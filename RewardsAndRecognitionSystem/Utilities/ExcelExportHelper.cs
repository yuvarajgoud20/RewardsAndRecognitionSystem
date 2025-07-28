using System.Text;

namespace RewardsAndRecognitionSystem.Utilities
{
    public class ExcelExportHelper
    {
        public static byte[] ExportToExcel<T>(List<T> data, List<(string Header, Func<T, string> Selector)> columns, string sheetName = "Sheet1")
        {
            using var stream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                var contentTypes = archive.CreateEntry("[Content_Types].xml");
                using (var writer = new StreamWriter(contentTypes.Open()))
                {
                    writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
<Default Extension=""xml"" ContentType=""application/xml""/>
<Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
<Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
<Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/>
</Types>");
                }

                // _rels/.rels
                var rels = archive.CreateEntry("_rels/.rels");
                using (var writer = new StreamWriter(rels.Open()))
                {
                    writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>");
                }

                // xl/workbook.xml
                var workbook = archive.CreateEntry("xl/workbook.xml");
                using (var writer = new StreamWriter(workbook.Open()))
                {
                    writer.Write($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
<sheets>
<sheet name=""{sheetName}"" sheetId=""1"" r:id=""rId1""/>
</sheets>
</workbook>");
                }

                // xl/_rels/workbook.xml.rels
                var workbookRels = archive.CreateEntry("xl/_rels/workbook.xml.rels");
                using (var writer = new StreamWriter(workbookRels.Open()))
                {
                    writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>
</Relationships>");
                }

                // xl/styles.xml
                var styles = archive.CreateEntry("xl/styles.xml");
                using (var writer = new StreamWriter(styles.Open()))
                {
                    writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
<fonts count=""2"">
<font><sz val=""11""/><color rgb=""FF000000""/><name val=""Calibri""/></font>
<font><b/><sz val=""11""/><color rgb=""FF000000""/><name val=""Calibri""/></font>
</fonts>
<fills count=""1""><fill><patternFill patternType=""none""/></fill></fills>
<borders count=""1""><border><left/><right/><top/><bottom/><diagonal/></border></borders>
<cellStyleXfs count=""1""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/></cellStyleXfs>
<cellXfs count=""2"">
<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyFont=""1""/>
<xf numFmtId=""0"" fontId=""1"" fillId=""0"" borderId=""0"" xfId=""0"" applyFont=""1""/>
</cellXfs>
</styleSheet>");
                }

                // xl/worksheets/sheet1.xml (dynamic columns)
                var sheet = archive.CreateEntry("xl/worksheets/sheet1.xml");
                using (var writer = new StreamWriter(sheet.Open()))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
<cols>");

                    for (int i = 1; i <= columns.Count; i++)
                    {
                        sb.AppendLine($@"<col min=""{i}"" max=""{i}"" width=""25"" customWidth=""1""/>");
                    }

                    sb.AppendLine("</cols><sheetData>");

                    // Header
                    sb.AppendLine(@"<row r=""1"">");
                    foreach (var col in columns)
                    {
                        sb.AppendLine($@"<c s=""1"" t=""inlineStr""><is><t>{EscapeXml(col.Header)}</t></is></c>");
                    }
                    sb.AppendLine("</row>");

                    // Data
                    int row = 2;
                    foreach (var item in data)
                    {
                        sb.AppendLine($@"<row r=""{row}"">");
                        foreach (var col in columns)
                        {
                            string value = col.Selector(item) ?? "";
                            sb.AppendLine($@"<c s=""0"" t=""inlineStr""><is><t>{EscapeXml(value)}</t></is></c>");
                        }
                        sb.AppendLine("</row>");
                        row++;
                    }

                    sb.AppendLine("</sheetData></worksheet>");
                    writer.Write(sb.ToString());
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            return stream.ToArray();
        }

        private static string EscapeXml(string input)
        {
            return System.Security.SecurityElement.Escape(input) ?? "";
        }
    }
}
