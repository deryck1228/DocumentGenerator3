using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;

namespace DocumentGenerator3.DocumentAssembly
{
    public class AssembleDataInTemplate_word
    {
        public DocumentData fileData { get; set; }
        public byte[] AssembleData()
        {
            WordprocessingDocument wDocument;
            Stream stream = new MemoryStream(0);

            stream.Write(fileData.fileContents, 0, (int)fileData.fileContents.Length);
            using (WordprocessingDocument wdDoc = WordprocessingDocument.Open(stream, true))
            {
                DocumentSettingsPart settingsPart =
                    wdDoc.MainDocumentPart.GetPartsOfType<DocumentSettingsPart>().First();

                UpdateFieldsOnOpen updateFields = new UpdateFieldsOnOpen();
                updateFields.Val = new DocumentFormat.OpenXml.OnOffValue(true);

                settingsPart.Settings.PrependChild<UpdateFieldsOnOpen>(updateFields);
                settingsPart.Settings.Save();

                wDocument = wdDoc;
                string docText = null;
                using (StreamReader sr = new StreamReader(wdDoc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                AddParentDataToDocument(wdDoc);
                Body bod = wdDoc.MainDocumentPart.Document.Body;
                if (fileData.listOfTableCSVs != null)
                {
                    AddChildDataToDocument(bod);
                }

                PopulateAllRuns(wdDoc);

                wdDoc.MainDocumentPart.Document.Save();
                wdDoc.Close();
                stream.Seek(0, SeekOrigin.Begin);

                var bytes = new Byte[(int)stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                fileData.fileContents = bytes;

            }
            stream.Close();

            return fileData.fileContents;
        }

        private static void PopulateAllRuns(WordprocessingDocument wdDoc)
        {
            foreach (Run r in wdDoc.MainDocumentPart.Document.Descendants<Run>())
            {
                if (r.Elements<Text>().Count() > 0)
                {
                    Text text = r.Elements<Text>().First();
                    if (text == null) continue;
                    //Replace unusual character sequence with new Break() to reintroduce newlines in doc
                    string[] newLineArray = { Environment.NewLine, "*|*" };
                    string[] textArray = text.Text.Split(newLineArray, StringSplitOptions.None);
                    text.Text = "";

                    bool first = true;

                    foreach (string line in textArray)
                    {
                        if (!first)
                        {
                            r.Append(new Break());
                        }

                        first = false;

                        Text txt = new Text();
                        txt.Text = line;
                        r.Append(txt);
                    }       
                }
            }
        }

        private void AddChildDataToDocument(Body bod)
        {
            foreach (var m in fileData.listOfTableCSVs)
            {
                foreach (Table t in bod.Descendants<Table>())
                {
                    var tableCaption = "";
                    if (t.GetFirstChild<TableProperties>().TableCaption != null)
                    {
                        tableCaption = t.GetFirstChild<TableProperties>().TableCaption.Val.Value;
                    }
                    var tableProperties = t.GetFirstChild<TableProperties>();

                    PreconfigureTable(m, t, tableCaption);
                }
            }
        }

        private static void PreconfigureTable(KeyValuePair<string, string> m, Table t, string tableCaption)
        {
            if (m.Key == tableCaption)
            {
                CreateArray tableData = new CreateArray();
                var tableArray = tableData.LoadCsv(m.Value);

                TableGrid tg = t.GetFirstChild<TableGrid>();

                ResizeTableAsNecessary(t, tableData, tg);
                var lastRowIndex = tableArray.GetUpperBound(0);
                AddCsvDataToTable(t, tableArray, lastRowIndex);
            }
        }

        private static void ResizeTableAsNecessary(Table t, CreateArray tableData, TableGrid tg)
        {
            int originalNumberOfColumns = tg.ChildElements.Count();
            if (originalNumberOfColumns < tableData.numberOfColumns)
            {
                int i = 1;
                while (originalNumberOfColumns + i <= tableData.numberOfColumns)
                {
                    tg.AppendChild(new GridColumn());
                    i++;
                }
            }
            int originalNumberOfRows = t.Elements<TableRow>().Count();

            TableRow lastRow = t.Elements<TableRow>().Last();

            if (originalNumberOfRows < tableData.NumberOfRows)
            {
                int i = 1;
                while (originalNumberOfRows + i <= tableData.NumberOfRows)
                {
                    TableRow newRow = (TableRow)lastRow.CloneNode(true);
                    t.Descendants<TableRow>().First().InsertAfterSelf(newRow);

                    i++;
                }
            }
        }

        private static void AddCsvDataToTable(Table t, string[,] tableArray, int lastRowIndex)
        {
            for (int r = 0; r <= lastRowIndex; r++)
            {
                var lastColumnIndex = tableArray.GetUpperBound(1);
                for (int c = 0; c <= lastColumnIndex; c++)
                {
                    var cellValue = tableArray[r, c];
  
                    TableRow row = t.Elements<TableRow>().ElementAt(r); 
                    TableCell cell = row.Elements<TableCell>().ElementAt(c);

                    if (cell.ChildElements.Count <= 1)
                    {
                        cell.Append(new Paragraph(new Run()));
                    }
                    Paragraph parag = cell.Elements<Paragraph>().First();

                    if (parag.ChildElements.Count <= 1)
                    {
                        parag.Append(new Run());
                    }
                    Run run = parag.Elements<Run>().First();
 
                    if (run.ChildElements.Count == 0)
                    {
                        run.Append(new Text());
                    }
                    Text text = run.Elements<Text>().First();

                    text.Text = cellValue; //.Replace("*|*","\n"); //Replace unusual character sequence with newline to reintroduce newlines when filling table cell
                }
            }
        }

        private void AddParentDataToDocument(WordprocessingDocument wdDoc)
        {
            foreach (var r in fileData.parentData)
            {
                string elementValue = " ";
                if (!r.Equals(default(KeyValuePair<string, string>)))
                {
                    elementValue = r.Value.ToString(); //.Replace("*|*", "\n"); //Replace unusual character sequence with newline to reintroduce newlines when filling table cell
                }

                if (elementValue != "")
                {
                    SearchAndReplacer.SearchAndReplace(wdDoc, "{{" + r.Key + "}}", elementValue, true);
                }
                else
                {
                    SearchAndReplacer.SearchAndReplace(wdDoc, "{{" + r.Key + "}}", " ", true);
                }
            }
        }
    }
}
