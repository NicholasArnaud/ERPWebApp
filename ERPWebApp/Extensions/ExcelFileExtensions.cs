using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Reflection;
using System.Diagnostics;

namespace ERPWebApp.Extensions
{
    public static class ExcelFileExtensions
    {
        public static void AppendErrorMessage(Row row, string errorMessage, SpreadsheetDocument document, int? lastColumn = null)
        {
            int columnIndex = lastColumn ?? row.Elements<Cell>().Count();
            Cell cell = CreateCell(columnIndex, row.RowIndex.Value, errorMessage, CellValues.String);
            row.Append(cell);
            ApplyRedCellStyle(cell, document);
        }

        public static object GetCellValue(SpreadsheetDocument document, Cell cell, Type propertyType)
        {
            if (cell == null) return null;
            string cellValue = cell.CellValue?.Text;

            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                var sharedStringPart = document.WorkbookPart.SharedStringTablePart;
                cellValue = sharedStringPart.SharedStringTable.ElementAt(int.Parse(cell.CellValue.Text)).InnerText;
            }

            if (string.IsNullOrEmpty(cellValue)) return null;

            Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            try
            {
                if (underlyingType == typeof(bool)) return cellValue.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                if (underlyingType.IsEnum) return Enum.Parse(underlyingType, cellValue);
                if (underlyingType == typeof(DateTime)) return DateTime.FromOADate(double.Parse(cellValue));

                return Convert.ChangeType(cellValue, underlyingType);
            }
            catch (Exception ex)
            {
                // Handle conversion errors
                Debug.Print($"Error converting cell value: {ex.Message}");
            }

            return cellValue;
        }

        public static SpreadsheetDocument PrepareTemplate<T>(List<string> fieldArray, List<List<string>> sampleArray = null, SpreadsheetDocument doc = null) where T : class
        {
            SpreadsheetDocument document = doc ?? SpreadsheetDocument.Create("output.xlsx", SpreadsheetDocumentType.Workbook);
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            AddSheetToWorkbook(document, "Sheet1", worksheetPart);

            AddHeaderRow(worksheetPart, fieldArray);
            foreach (var item in sampleArray)
            {
                AddSampleRow(worksheetPart, item);
            }

            workbookPart.Workbook.Save();
            return document;
        }

        public static void PrepareWorkbook(
            Dictionary<string, Dictionary<string, string>> sheets,
            Dictionary<string, List<Dictionary<string, string>>> data,
            SpreadsheetDocument document
        )
        {
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            foreach (var sheetInfo in sheets)
            {
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                AddSheetToWorkbook(document, sheetInfo.Key, worksheetPart);

                AddHeaderRow(worksheetPart, [.. sheetInfo.Value.Keys]);
                AddDataRows(worksheetPart, [.. sheetInfo.Value.Keys], data[sheetInfo.Key]);
            }

            workbookPart.Workbook.Save();
        }

        // Helper method to handle both headers and values row
        private static void AddRow(WorksheetPart worksheetPart, List<string> values, uint rowIndex, CellValues cellType)
        {
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            Row row = new() { RowIndex = rowIndex };
            sheetData.Append(row);

            for (int i = 0; i < values.Count; i++)
            {
                row.Append(CreateCell(i, rowIndex, values[i], cellType));
            }
        }

        private static void AddSheetToWorkbook(SpreadsheetDocument document, string sheetName, WorksheetPart worksheetPart)
        {
            Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
            Sheet sheet = new() { Id = document.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = (uint)(sheets.Count() + 1), Name = sheetName };
            sheets.Append(sheet);
        }

        private static void AddHeaderRow(WorksheetPart worksheetPart, List<string> headers)
        {
            AddRow(worksheetPart, headers, 1, CellValues.String);
        }

        private static void AddSampleRow(WorksheetPart worksheetPart, List<string> sampleValues)
        {
            if (sampleValues == null) return;
            AddRow(worksheetPart, sampleValues, 2, CellValues.String);
        }

        private static void AddDataRows(WorksheetPart worksheetPart, List<string> columns, List<Dictionary<string, string>> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                var rowData = columns.Select(col => data[i].TryGetValue(col, out string value) ? value : string.Empty).ToList();
                AddRow(worksheetPart, rowData, (uint)(i + 2), CellValues.String);
            }
        }

        private static Cell CreateCell(int columnIndex, uint rowIndex, string text, CellValues type)
        {
            Cell cell = new() { CellReference = GetCellReference(columnIndex, rowIndex), DataType = type };
            cell.CellValue = new CellValue(text);
            return cell;
        }

        public static string GetCellReference(int columnIndex, uint rowIndex)
        {
            return $"{GetColumnName(columnIndex)}{rowIndex}";
        }

        private static string GetColumnName(int columnIndex)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string columnName = "";
            while (columnIndex >= 0)
            {
                columnName = letters[columnIndex % 26] + columnName;
                columnIndex = columnIndex / 26 - 1;
            }
            return columnName;
        }

        private static void ApplyRedCellStyle(Cell cell, SpreadsheetDocument document)
        {
            WorkbookPart workbookPart = document.WorkbookPart;

            if (workbookPart.WorkbookStylesPart == null)
            {
                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet();
            }

            var stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

            stylesheet.Fills ??= new Fills();

            stylesheet.CellFormats ??= new CellFormats();

            Fill fill = new(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue { Value = "FFFF0000" } })
            {
                PatternType = PatternValues.Solid
            });
            stylesheet.Fills.Append(fill);

            CellFormat cellFormat = new() { FillId = (uint)(stylesheet.Fills.Count - 1), ApplyFill = true };
            stylesheet.CellFormats.Append(cellFormat);
            stylesheet.Save();

            cell.StyleIndex = (uint)(stylesheet.CellFormats.Count - 1);
        }


        public static Cell CreateTextCell(int columnIndex, uint rowIndex, string text)
        {
            return CreateCell(columnIndex, rowIndex, text, CellValues.String);
        }

        public static SheetData GetSheetData(SpreadsheetDocument document)
        {
            var workbookPart = document.WorkbookPart;
            var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
            return worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
        }

        public static (Dictionary<string, int>, int) GetHeaders(SpreadsheetDocument document, SheetData sheetData, List<string> properties)
        {
            Row headerRow = sheetData.Elements<Row>().FirstOrDefault();
            var headers = new Dictionary<string, int>();
            int columnIndex = 0;
            foreach (var cell in headerRow.Elements<Cell>())
            {
                string headerValue = GetCellValue(document, cell, typeof(string))?.ToString();
                string cleanedHeader = headerValue.Replace("(Required)", "").Replace(" ", "");
                if (properties.Contains(cleanedHeader, StringComparer.CurrentCultureIgnoreCase))
                    headers[cleanedHeader] = columnIndex;
                columnIndex++;
            }
            return (headers, columnIndex + 1);
        }

        public static Dictionary<string, object> ProcessRow(SpreadsheetDocument document, Row row, string[] fieldArray, PropertyInfo[] properties)
        {
            var result = new Dictionary<string, object>();

            for (int columnIndex = 0; columnIndex < fieldArray.Length; columnIndex++)
            {
                Cell cell = row.Elements<Cell>().ElementAt(columnIndex);
                string columnName = fieldArray[columnIndex];

                object cellValue = GetCellValue(document, cell, typeof(string)); // Defaulting to string

                result.Add(columnName, cellValue);
            }

            return result;
        }

        public static void ApplyHeaderStyle(Cell cell, WorkbookPart workbookPart)
        {
            // Ensure the Stylesheet exists
            if (workbookPart.WorkbookStylesPart == null)
            {
                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateStylesheet();
                stylesPart.Stylesheet.Save();
            }

            // Apply the header style (assuming style index 1 is the header style)
            cell.StyleIndex = 1U;
        }

        private static Stylesheet CreateStylesheet()
        {
            // Create Fonts
            Fonts fonts = new(
                new Font( // Index 0 - Default font
                    new FontSize { Val = 11 },
                    new Color { Rgb = new HexBinaryValue() { Value = "000000" } }),
                new Font( // Index 1 - Bold font for header
                    new Bold(),
                    new FontSize { Val = 11 },
                    new Color { Rgb = new HexBinaryValue() { Value = "FFFFFF" } })
            );

            // Create Fills (background color for header)
            Fills fills = new(
                new Fill(new PatternFill { PatternType = PatternValues.None }), // Index 0 - No fill
                new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue { Value = "FF0000" } }) // Red background
                { PatternType = PatternValues.Solid }) // Index 1 - Solid red fill
            );

            // Create Borders
            Borders borders = new(
                new Border() // Default border
            );

            // Create CellFormats
            CellFormats cellFormats = new(
                new CellFormat(),  // Index 0 - Default style
                new CellFormat
                {
                    FontId = 1, // Bold font
                    FillId = 1, // Red fill
                    BorderId = 0,
                    ApplyFont = true,
                    ApplyFill = true,
                    ApplyBorder = true,
                    Alignment = new Alignment
                    {
                        Horizontal = HorizontalAlignmentValues.Center,
                        Vertical = VerticalAlignmentValues.Center
                    }
                } // Index 1 - Header style (bold, red fill, center alignment)
            );

            return new Stylesheet(fonts, fills, borders, cellFormats);
        }

        public static Cell GetCellByColumnIndex(Row row, int columnIndex)
        {
            string columnName = GetExcelColumnName(columnIndex);
            return row.Elements<Cell>().FirstOrDefault(c => c.CellReference.Value.StartsWith(columnName));
        }

        private static string GetExcelColumnName(int columnIndex)
        {
            string columnName = string.Empty;
            while (columnIndex >= 0)
            {
                columnName = (char)('A' + (columnIndex % 26)) + columnName;
                columnIndex = (columnIndex / 26) - 1;
            }
            return columnName;
        }
    }
}
