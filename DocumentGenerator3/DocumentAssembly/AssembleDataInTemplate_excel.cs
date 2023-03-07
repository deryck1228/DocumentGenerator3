using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DocumentGenerator3.DocumentAssembly
{
    public class AssembleDataInTemplate_excel
    {
        public DocumentData fileData { get; set; }
        public string displayTimeZone { get; set; }
        public byte[] AssembleData()
        {
            //SpreadsheetDocument spreadsheetDocument;
            Stream stream = new MemoryStream(0);

            stream.Write(fileData.fileContents, 0, (int)fileData.fileContents.Length);
            using (SpreadsheetDocument spDoc = SpreadsheetDocument.Open(stream, true))
            {
                //Make changes to doc 
                AddParentDataToDocument(spDoc);

                if (fileData.listOfTableCSVs != null)
                {
                    AddChildDataToDocument(spDoc);
                }

                spDoc.Close();
                stream.Seek(0, SeekOrigin.Begin);

                var bytes = new Byte[(int)stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                fileData.fileContents = bytes;
            }

            stream.Close();

            return fileData.fileContents;
        }

        private void AddChildDataToDocument(SpreadsheetDocument spDoc)
        {
            DefinedNames existingDefinedNames = spDoc.WorkbookPart.Workbook.DefinedNames;

            foreach (var table in fileData.listOfTableCSVs)
            {
                foreach(DefinedName definedName in existingDefinedNames)
                {
                    //Resize named range based on number of rows and columns in table

                    string sheetName = definedName.InnerText.Split("!")[0];
                    Sheet sheet = spDoc.WorkbookPart.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName.Replace("'","")).FirstOrDefault();
                    if (sheet is not null)
                    {

                        Worksheet ws = ((WorksheetPart)(spDoc.WorkbookPart.GetPartById(sheet.Id))).Worksheet;
                        SheetData sheetData = ws.WorksheetPart.Worksheet.GetFirstChild<SheetData>();

                        AddChildDataToNamedRange(table, definedName, sheetData); 
                    }
                }
            }
        }

        private void AddParentDataToDocument(SpreadsheetDocument spDoc)
        {
            foreach (var r in fileData.parentData)
            {
                string elementValue = " ";
                if (!r.Equals(default(KeyValuePair<string, string>)))
                {
                    elementValue = r.Value.ToString();
                }

                string elementSlug = "{{" + r.Key + "}}";

                WorkbookPart wbPart = spDoc.WorkbookPart;

                foreach (Sheet sheet in spDoc.WorkbookPart.Workbook.Sheets)
                {
                    string elementValueAsDisplayDate = Helpers.DisplayValueConverter.ChangeDateToDisplayDate(elementValue, fileData.originalPayload.document_display_timezone);

                    if ( elementValueAsDisplayDate is not null)
                    {
                        elementValue = elementValueAsDisplayDate;
                    }
                    FindAndReplaceTextInSheet(wbPart, sheet, elementValue, elementSlug); 
                }

            }

            spDoc.Save();
        }

        private static void FindAndReplaceTextInSheet(WorkbookPart wbPart, Sheet sheet, string elementValue, string elementSlug)
        {

            string value = null;

            WorksheetPart wsPart =
                    (WorksheetPart)(wbPart.GetPartById(sheet.Id));

            var stringTable =
                wbPart.GetPartsOfType<SharedStringTablePart>()
                .FirstOrDefault();

            var CellsWithSlug = wsPart.Worksheet.Descendants<Cell>(); //.Where(c => c.CellValue.Text.Contains(elementSlug))

            foreach (var c in CellsWithSlug)
            {
                int elementIndex = 0;

                if (Int32.TryParse(c.InnerText, out elementIndex))
                {
                    if (c != null && (c.InnerText == elementSlug || (c.DataType != null && stringTable.SharedStringTable
                        .ElementAt(elementIndex).InnerText == elementSlug)))
                    {
                        var cellText = c.CellValue.Text.Replace(elementSlug, elementValue);

                        value = c.InnerText;


                        if (stringTable != null && c.DataType == CellValues.SharedString)
                        {
                            value =
                                stringTable.SharedStringTable
                                .ElementAt(int.Parse(value)).InnerText;

                            value = value.Replace(elementSlug, elementValue);

                            int index = InsertSharedStringItem(value, stringTable);

                            c.CellValue = new CellValue(index.ToString());
                            c.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                        }
                    } 
                }
            }
        }

        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }
                i++;
            }

            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }

        private void AddChildDataToNamedRange(KeyValuePair<string, CsvWithMetadata> table, DefinedName definedName, SheetData sheetData)
        {
            if (table.Key.Replace("[","_").Replace("]","_").Replace(".","_") == definedName.Name && table.Value.Csv != "\n")
            {
                CreateArray tableData = new CreateArray();
                var tableArray = tableData.LoadCsv(table.Value.Csv);

                if (table.Key.Contains("br9yj7p2c"))
                {
                    Console.WriteLine(table.Key);
                }

                //Get reference row index value of first row in named range
                //Add a new row if one doesn't exist
                Row refRow = sheetData.Descendants<Row>().Where(r => r.RowIndex.Value == 1).FirstOrDefault();
                //Add new cells for each element in the table data

                int startingColumn = GetExcelColumnNumber(definedName.InnerText.Split("!")[1].Split(":")[0].Split("$")[1]) - 1;
                int endingColumn = GetExcelColumnNumber(definedName.InnerText.Split("!")[1].Split(":")[1].Split("$")[1]) - 1;
                int startingRow = Convert.ToInt32(definedName.InnerText.Split("!")[1].Split(":")[0].Split("$")[2]) - 1;
                int endingRow = Convert.ToInt32(definedName.InnerText.Split("!")[1].Split(":")[1].Split("$")[2]) - 1;

                int csvRowIterator = 0;
                int csvColIterator = 0;

                for (int r = startingRow; r <= endingRow; r++)
                {
                    csvColIterator = 0;
                    Row newRow = new Row()
                    {
                        RowIndex = (uint)(r + 1)
                    };

                    for (int c = startingColumn; c <= endingColumn; c++)
                    {
                        var upperBoundRow = tableArray.GetUpperBound(0);
                        var upperBoundColumn = tableArray.GetUpperBound(1);

                        if(csvRowIterator > upperBoundRow || csvColIterator > upperBoundColumn)
                        {
                            continue;
                        }

                        var cellValueAsString = tableArray[csvRowIterator, csvColIterator];

                        var cellDataType = new EnumValue<CellValues>(CellValues.SharedString);

                        string elementValueAsDisplayDate = Helpers.DisplayValueConverter.ChangeDateToDisplayDate(cellValueAsString, fileData.originalPayload.document_display_timezone);

                        if(cellValueAsString == "2.5")
                        {
                            Console.WriteLine(cellValueAsString);
                        }

                        if (elementValueAsDisplayDate is not null)
                        {
                            cellValueAsString = elementValueAsDisplayDate;
                            cellDataType = new EnumValue<CellValues>(CellValues.Date);
                        }

                        CellValue cellValue = new CellValue(cellValueAsString);

                        int? elementValueAsInt = Helpers.DisplayValueConverter.ChangeStringToInt(cellValueAsString);
                        if(elementValueAsInt is not null)
                        {
                            cellValue = new CellValue((int)elementValueAsInt);
                            cellDataType = new EnumValue<CellValues>(CellValues.Number);
                        }


                        Cell cell1 = new Cell() { CellReference = GetExcelColumnName((c + 1)) + (uint)(r + 1), CellValue = cellValue, DataType = cellDataType  }; //{ Text = cellValue }
                        newRow.Append(cell1);

                        csvColIterator++;
                    }
                    csvRowIterator++;

                    sheetData.InsertBefore(newRow, refRow);
                }  


                
                //Insert the table element into new cell, if string add to sharedString table and add pointer :P

            }

        }

        private static string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }

        private static int GetExcelColumnNumber(string columnLetter)
        {
            if (string.IsNullOrEmpty(columnLetter)) throw new ArgumentNullException("columnName");

            columnLetter = columnLetter.ToUpperInvariant();

            int sum = 0;

            for (int i = 0; i < columnLetter.Length; i++)
            {
                sum *= 26;
                sum += (columnLetter[i] - 'A' + 1);
            }

            return sum;
        }
    }
}
