using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Epsilon.OpenXml
{
    public class Excel
    {
        public const string Version = "0";

        public class Document
        {
            private SpreadsheetDocument _document;

            internal Document(SpreadsheetDocument d)
            {
                _document = d;
            }

            public void Save()
            {
                _document.Save();
            }

            public Sheet AddSheet(string name)
            {
                var s = InsertWorksheet(_document.WorkbookPart, name);
                return new Sheet(_document, s);
            }

            public class Sheet
            {

                private SpreadsheetDocument _document;
                private WorksheetPart _sheet;

                internal Sheet(SpreadsheetDocument d, WorksheetPart s)
                {
                    _document = d;
                    _sheet = s;
                }

                public void InsertVal(string col, uint row, object value, string dataType)
                {
                    if (value is DBNull) return;
                    if (dataType == "integer")
                        Excel.InsertInteger(_document, _sheet, col, row, (int)value);
                    else if (dataType == "decimal")
                        Excel.InsertDecimal(_document, _sheet, col, row, (decimal)value);
                    else if (dataType == "datetime")
                        Excel.InsertDate(_document, _sheet, col, row, (DateTime)value);
                    else if (dataType == "bool")
                        Excel.InsertBool(_document, _sheet, col, row, (byte)value == 1);
                    else
                        Excel.InsertText(_document, _sheet, col, row, (string)value);
                }

                public void InsertVal(string col, uint row, string value, string dataType)
                {
                    if (dataType == "integer" && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i))
                        Excel.InsertInteger(_document, _sheet, col, row, (int)i);
                    if (dataType == "decimal" && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var n))
                        Excel.InsertDecimal(_document, _sheet, col, row, n);
                    else if (dataType == "datetime" && DateTime.TryParse(value, out var d))
                        Excel.InsertDate(_document, _sheet, col, row, d);
                    else
                        Excel.InsertText(_document, _sheet, col, row, value);
                }

                public void InsertText(string col, uint row, string value)
                {
                    Excel.InsertText(_document, _sheet, col, row, value);
                }
            }
        }

        public static void Generate(Stream stream, Action<Document> act)
        {
            CreateSpreadsheetWorkbook(stream, (d) =>
            {
                var doc = new Document(d);
                act(doc);
                doc.Save();
            });
        }

        public static void CreateSpreadsheetWorkbook(Stream stream, Action<SpreadsheetDocument> action)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            using (var spreadsheetStream = new MemoryStream())
            {
                SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(spreadsheetStream, SpreadsheetDocumentType.Workbook);

                // Add a WorkbookPart to the document.
                WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

                var workbookStylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
                workbookStylesPart.Stylesheet = new Stylesheet(styles);

                action(spreadsheetDocument);

                workbookpart.Workbook.Save();
                spreadsheetDocument.Close();
                spreadsheetStream.Seek(0, SeekOrigin.Begin);
                spreadsheetStream.CopyTo(stream);
            }
        }


        public static void InsertText(SpreadsheetDocument spreadSheet, WorksheetPart worksheetPart, string col, uint row, string text)
        {
            // Get the SharedStringTablePart. If it does not exist, create a new one.
            var shareStringPart = spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Any()
                ? spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First()
                : spreadSheet.WorkbookPart.AddNewPart<SharedStringTablePart>();

            // Insert the text into the SharedStringTablePart.
            int index = InsertSharedStringItem(text, shareStringPart);


            // Insert cell A1 into the new worksheet.
            Cell cell = InsertCellInWorksheet(col, row, worksheetPart);

            // Set the value of cell A1.
            cell.CellValue = new CellValue(index.ToString());
            cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
        }

        public static void InsertInteger(SpreadsheetDocument spreadSheet, WorksheetPart worksheetPart, string col, uint row, int val)
        {
            Cell cell = InsertCellInWorksheet(col, row, worksheetPart);

            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
            cell.CellValue = new CellValue(val.ToString("D", CultureInfo.InvariantCulture));
            cell.StyleIndex = new UInt32Value { Value = 1 };
        }

        public static void InsertBool(SpreadsheetDocument spreadSheet, WorksheetPart worksheetPart, string col, uint row, bool val)
        {
            Cell cell = InsertCellInWorksheet(col, row, worksheetPart);

            cell.DataType = new EnumValue<CellValues>(CellValues.Boolean);
            cell.CellValue = new CellValue(val ? "1" : "0");
            cell.StyleIndex = new UInt32Value { Value = 1 };
        }

        public static void InsertDecimal(SpreadsheetDocument spreadSheet, WorksheetPart worksheetPart, string col, uint row, decimal val)
        {
            Cell cell = InsertCellInWorksheet(col, row, worksheetPart);

            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
            cell.CellValue = new CellValue(val.ToString("F4", CultureInfo.InvariantCulture));
        }

        public static void InsertDate(SpreadsheetDocument spreadSheet, WorksheetPart worksheetPart, string col, uint row, DateTime val)
        {
            Cell cell = InsertCellInWorksheet(col, row, worksheetPart);

            cell.CellValue = new CellValue(val);
            cell.DataType = new EnumValue<CellValues>(CellValues.Date);
            cell.StyleIndex = new UInt32Value { Value = 3 };
        }

        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null) shareStringPart.SharedStringTable = new SharedStringTable();

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text) return i;
                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }


        private static WorksheetPart InsertWorksheet(WorkbookPart workbookPart, string name)
        {
            // Add a new worksheet part to the workbook.
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(new SheetData());
            newWorksheetPart.Worksheet.Save();

            Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
            string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

            // Get a unique ID for the new sheet.
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Any())
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }


            // Append the new worksheet and associate it with the workbook.
            var sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = name };
            sheets.Append(sheet);

            return newWorksheetPart;
        }

        private static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName.ToUpper() + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);
            if (row == null)
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            var foundCell = row.Elements<Cell>().FirstOrDefault(c => c.CellReference.Value == cellReference);
            if (foundCell != null) return foundCell;

            Cell refCell = row.Elements<Cell>().FirstOrDefault(cell => string.Compare(cell.CellReference.Value.PadLeft(16, '0'), cellReference.PadLeft(16, '0'), true, CultureInfo.InvariantCulture) > 0);

            Cell newCell = new Cell() { CellReference = cellReference };
            row.InsertBefore(newCell, refCell);


            return newCell;
        }

        //https://stackoverflow.com/questions/181596/how-to-convert-a-column-number-e-g-127-into-an-excel-column-e-g-aa
        public static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = string.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        public class Options
        {
            public long Timestamp { get; set; }
        }

        private const string styles = @"
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""x14ac x16r2 xr"" xmlns:x14ac=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"" xmlns:x16r2=""http://schemas.microsoft.com/office/spreadsheetml/2015/02/main"" xmlns:xr=""http://schemas.microsoft.com/office/spreadsheetml/2014/revision"">
  <numFmts count=""1"">
    <numFmt numFmtId=""164"" formatCode=""#,##0.0""/>
  </numFmts>
  <fonts count=""1"" x14ac:knownFonts=""1"">
		<font>
			<sz val=""11""/>
			<color theme=""1""/>
			<name val=""Calibri""/>
			<family val=""2""/>
			<scheme val=""minor""/>
		</font>
	</fonts>
	<fills count=""2"">
		<fill>
			<patternFill patternType=""none""/>
		</fill>
		<fill>
			<patternFill patternType=""gray125""/>
		</fill>
	</fills>
	<borders count=""1"">
		<border>
			<left/>
			<right/>
			<top/>
			<bottom/>
			<diagonal/>
		</border>
	</borders>
	<cellStyleXfs count=""1"">
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/>
	</cellStyleXfs>
	<cellXfs count=""3"">
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0""/>
    <!--1=integer-->
    <xf numFmtId=""1"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/>
    <!--2=decimal-->
    <xf numFmtId=""164"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/>
    <!--3=datetime-->
    <xf numFmtId=""14"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0"" applyNumberFormat=""1""/>
	</cellXfs>
	<cellStyles count=""1"">
		<cellStyle name=""Normal"" xfId=""0"" builtinId=""0""/>
	</cellStyles>
	<dxfs count=""0""/>
	<tableStyles count=""0"" defaultTableStyle=""TableStyleMedium2"" defaultPivotStyle=""PivotStyleLight16""/>
	<extLst>
		<ext uri=""{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}"" xmlns:x14=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/main"">
			<x14:slicerStyles defaultSlicerStyle=""SlicerStyleLight1""/>
		</ext>
		<ext uri=""{9260A510-F301-46a8-8635-F512D64BE5F5}"" xmlns:x15=""http://schemas.microsoft.com/office/spreadsheetml/2010/11/main"">
			<x15:timelineStyles defaultTimelineStyle=""TimeSlicerStyleLight1""/>
		</ext>
	</extLst>
</styleSheet>
        ";
    }
}
