using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using DocumentGenerator3.ImageHandling;
using System.Runtime.Serialization.Formatters.Binary;
using DocumentFormat.OpenXml;
using System.Drawing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using DocumentGenerator3.BulletedListData;

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

                if(fileData.bulletedListCollection.Count > 0)
                {
                    AddBulletedListsToDocument(wdDoc);
                }

                AddParentDataToDocument(wdDoc);
                Body bod = wdDoc.MainDocumentPart.Document.Body;

                if (fileData.listOfTableCSVs != null)
                {
                    AddChildDataToDocument(bod);
                }

                PopulateAllRuns(wdDoc);

                foreach (var img in fileData.originalPayload.image_locations)
                {
                    if (img.settings.image_bytes != null)
                    {
                        InsertPicture(wdDoc, img.settings); 
                    }
                }

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

        private void InsertPicture(WordprocessingDocument wordprocessingDocument, IImageSettings imageSettings)
        {
            MainDocumentPart mainPart = wordprocessingDocument.MainDocumentPart;

            //TODO update AddImagePart to use dynamic string of image type
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            //ImagePart ip = wordprocessingDocument.MainDocumentPart.AddImagePart(ImagePartType.Jpeg);

            MemoryStream memStream = new MemoryStream();

            memStream.Write(imageSettings.image_bytes, 0, imageSettings.image_bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            if (imageSettings.image_width is null || imageSettings.image_height is null)
            {
                var imageSize = GetSize(imageSettings.image_bytes); 
                imageSettings.image_width = imageSize.Width;
                imageSettings.image_height = imageSize.Height;
            }

            imagePart.FeedData(memStream);

            string imageSlug = $"~~{imageSettings.image_id}~~";

            AddImageToBody(wordprocessingDocument, mainPart.GetIdOfPart(imagePart), imageSlug, imageSettings.image_width, imageSettings.image_height);
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

        private static void AddImageToBody(WordprocessingDocument wordDoc, string relationshipId, string imageSlug, int? imageWidth = 50, int? imageHeight = 50)
        {
            Size size = new Size((int)imageWidth, (int)imageHeight);

            Int64Value width = size.Width * 9525;
            Int64Value height = size.Height * 9525;

            // Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         new DW.Extent() { Cx = width, Cy = height },
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = "Picture 1"
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
                         new A.Graphic(
                             new A.GraphicData(
                                 new PIC.Picture(
                                     new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = "New Bitmap Image.jpg"
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri =
                                                        "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = relationshipId,
                                             CompressionState =
                                             A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
                                             new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             new A.Extents() { Cx = 990000L, Cy = 792000L }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         EditId = "50D07946"
                     });

            //Paragraph paragraphWithSlug = wordDoc.MainDocumentPart.Document.Body.Descendants<Paragraph>().FirstOrDefault(r => r.InnerText == imageSlug) as Paragraph;
            //Run runWithSlug = paragraphWithSlug.Descendants().FirstOrDefault(r => r.InnerText == imageSlug) as Run;

            Run runWithSlug = wordDoc.MainDocumentPart.Document.Body.Descendants<Run>().FirstOrDefault(r => r.InnerText == imageSlug);
            //Paragraph paragraphWithSlug = (Paragraph)runWithSlug.Parent.First();
            if (runWithSlug is not null)
            {

                runWithSlug.Parent.AppendChild(new Run(element));

                runWithSlug.Remove(); 
            }

            wordDoc.MainDocumentPart.Document.Save();
        }

        private Size GetSize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var image = Image.FromStream(stream);

                return image.Size;
            }
        }

        private string AddGraph(WordprocessingDocument wpd, IImageSettings imageSettings, byte[] imageData)
        {
            ImagePart ip = wpd.MainDocumentPart.AddImagePart(imageSettings.image_extension);

            MemoryStream memStream = new MemoryStream();

            memStream.Write(imageData, 0, imageData.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            if (memStream.Length == 0) return string.Empty;
            ip.FeedData(memStream);

            return wpd.MainDocumentPart.GetIdOfPart(ip);
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
            if (m.Key == tableCaption && m.Value != "\n")
            {
                CreateArray tableData = new CreateArray();
                var tableArray = tableData.LoadCsv(m.Value);

                TableGrid tg = t.GetFirstChild<TableGrid>();

                RunProperties runProperties = ResizeTableAsNecessary(t, tableData, tg);
                var lastRowIndex = tableArray.GetUpperBound(0);
                AddCsvDataToTable(t, tableArray, lastRowIndex, runProperties);
            }
        }

        private static RunProperties ResizeTableAsNecessary(Table t, CreateArray tableData, TableGrid tg)
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
            var runProperties = GetRunPropertyFromTableCell(lastRow, 0);

            if (originalNumberOfRows < tableData.NumberOfRows)
            {
                int i = 1;
                while (originalNumberOfRows + i <= tableData.NumberOfRows)
                {
                    TableRow newRow = (TableRow)lastRow.CloneNode(true);
                    
                    var thisNewRow = t.Descendants<TableRow>().First().InsertAfterSelf(newRow);

                    foreach(Run run in thisNewRow.Elements<Run>())
                    {
                        run.PrependChild<RunProperties>(runProperties);
                        run.RunProperties = runProperties;
                    }

                    i++;
                }
            }

            return runProperties;
        }

        private static RunProperties GetRunPropertyFromTableCell(TableRow rowCopy, int cellIndex)
        {
            var runProperties = new RunProperties();

            var rowCopyProperties = rowCopy.Descendants<TableCell>()
                    .ElementAt(cellIndex)
                    .GetFirstChild<Paragraph>()
                    .GetFirstChild<ParagraphProperties>();

            if (rowCopyProperties is not null)
            {
                var runFontType = rowCopyProperties.GetFirstChild<ParagraphMarkRunProperties>().GetFirstChild<RunFonts>();
                if (runFontType is not null)
                {
                    var fontName = runFontType.Ascii;
                    if (fontName != "")
                    {
                        runProperties.AppendChild(new RunFonts() { Ascii = fontName });
                    }
                }

                var runFontSize = rowCopyProperties.GetFirstChild<ParagraphMarkRunProperties>().GetFirstChild<FontSize>();

                if (runFontSize is not null)
                {
                    var fontSize = runFontSize.Val;

                    if (fontSize != "")
                    {
                        runProperties.AppendChild(new FontSize() { Val = fontSize });
                    }
                } 
            }

            return runProperties;
        }

        private static void AddCsvDataToTable(Table t, string[,] tableArray, int lastRowIndex, RunProperties runProperties)
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
                    run.Append(new RunProperties(new RunFonts()));
                    //run.RunProperties = runProperties;

                    if (run.ChildElements.Count == 1)
                    {
                        run.Append(new Text());
                    }
                    Text text = run.Elements<Text>().First();


                    text.Text = cellValue; //.Replace("*|*","\n"); //Replace unusual character sequence with newline to reintroduce newlines when filling table cell
                }
            }

            AddFormattingToTable(t, runProperties);
        }

        private static void AddFormattingToTable(Table t, RunProperties runProperties)
        {
            foreach (var rows in t.Elements<TableRow>())
            {
                foreach (var cells in rows.Elements<TableCell>())
                {
                    foreach (var paragraphs in cells.Elements<Paragraph>())
                    {
                        foreach (var run in paragraphs.Elements<Run>())
                        {
                            if (run.RunProperties is not null)
                            {
                                if (run.RunProperties.RunFonts is not null)
                                {
                                    if (run.RunProperties.RunFonts.Ascii is not null)
                                    {
                                        run.RunProperties.RunFonts.Ascii = runProperties.RunFonts.Ascii; 
                                    }
                                }

                                if (run.RunProperties.RunStyle is not null)
                                {
                                    if (run.RunProperties.RunStyle is not null)
                                    {
                                        run.RunProperties.RunStyle.Val = runProperties.RunStyle.Val; 
                                    }
                                }
                            }
                        }
                    }
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
                    elementValue = r.Value.ToString();
                }

                if (elementValue != "")
                {
                    string elementSlug = "{{" + r.Key + "}}";

                    FindAndReplaceTextInHeader(wdDoc, elementValue, elementSlug);

                    FindAndReplaceTextInFooter(wdDoc, elementValue, elementSlug);

                    FindAndReplaceTextInMainBody(wdDoc, elementValue, elementSlug);
                }
            }

            wdDoc.MainDocumentPart.Document.Save();
        }

        private static void FindAndReplaceTextInMainBody(WordprocessingDocument wdDoc, string elementValue, string elementSlug)
        {
            Run runWithSlug = wdDoc.MainDocumentPart.Document.Descendants<Run>().FirstOrDefault(r => r.InnerText.Contains(elementSlug));

            if (runWithSlug != null)
            {
                var runText = runWithSlug.Descendants<Text>().FirstOrDefault();
                if (runText != null)
                {
                    runText.Text = runText.Text.Replace(elementSlug, elementValue);
                }
            }
        }

        private static void FindAndReplaceTextInFooter(WordprocessingDocument wdDoc, string elementValue, string elementSlug)
        {
            foreach (var footer in wdDoc.MainDocumentPart.FooterParts)
            {
                Run footerRunWithSlug = footer.RootElement.Descendants<Run>().FirstOrDefault(r => r.InnerText.Contains(elementSlug));
                if (footerRunWithSlug != null)
                {
                    var runText = footerRunWithSlug.Descendants<Text>().FirstOrDefault();
                    if (runText != null)
                    {
                        runText.Text = runText.Text.Replace(elementSlug, elementValue);
                    }
                }
            }
        }

        private static void FindAndReplaceTextInHeader(WordprocessingDocument wdDoc, string elementValue, string elementSlug)
        {
            foreach (var header in wdDoc.MainDocumentPart.HeaderParts)
            {
                Run headerRunWithSlug = header.RootElement.Descendants<Run>().FirstOrDefault(r => r.InnerText.Contains(elementSlug));
                if (headerRunWithSlug != null)
                {
                    var runText = headerRunWithSlug.Descendants<Text>().FirstOrDefault();
                    if (runText != null)
                    {
                        runText.Text = runText.Text.Replace(elementSlug, elementValue);
                    }
                }
            }
        }

        private void AddBulletedListsToDocument(WordprocessingDocument wdDoc)
        {
            NumberingDefinitionsPart numberingPart;

            if (wdDoc.MainDocumentPart.NumberingDefinitionsPart is null)
            {
                numberingPart =
                    wdDoc.MainDocumentPart.AddNewPart<NumberingDefinitionsPart>("someUniqueIdHere"); 
            }
            else
            {
                numberingPart = wdDoc.MainDocumentPart.NumberingDefinitionsPart as NumberingDefinitionsPart;
            }

            int numberId = 0;

            foreach (var bulletedList in fileData.bulletedListCollection)
            {
                numberId++;

                Numbering element =
                  new Numbering(
                    new AbstractNum(
                      //new Level(
                      //  new NumberingFormat() { Val = NumberFormatValues.Custom },
                      //  new LevelText() { Val = "·" }
                      //)
                      //{ LevelIndex = 0 },
                      //new Level(
                      //  new NumberingFormat() { Val = NumberFormatValues.Custom },
                      //  new LevelText() { Val = "·" },
                      //  new ParagraphProperties(
                      //      new Indentation() { Left = "1440"}
                      //      )
                      //)
                      //{ LevelIndex = 1 }
                    )
                    { AbstractNumberId = 1 },
                    new NumberingInstance(
                      new AbstractNumId() { Val = 1 }
                    )
                    { NumberID = numberId });

                var abstractNum = element.Descendants<AbstractNum>().FirstOrDefault();

                List<Paragraph> paragraphs = new List<Paragraph>();

                CreateBulletedListParagraph(abstractNum, paragraphs, bulletedList.Value.Lines, numberId);

                var elementSlug = bulletedList.Key;
                Run runWithSlug = wdDoc.MainDocumentPart.Document.Body.Descendants<Run>().FirstOrDefault(r => r.InnerText == elementSlug);

                if (runWithSlug is not null)
                {
                    foreach (var para in paragraphs)
                    {
                        runWithSlug.Parent.Parent.AppendChild<Paragraph>(para); 
                    }
                    runWithSlug.Remove();
                }

                element.Save(numberingPart);

                //wdDoc.MainDocumentPart.Document.Save();
            }
        }

        private void CreateBulletedListParagraph(AbstractNum abstractNum, List<Paragraph> paragraphs, List<BulletedListLine> lines, int numberingID, int indentLevel = 0)
        {
            var indentDistance = 720 * (indentLevel + 1);
            Level thisLevel = new Level(
                        new NumberingFormat() { Val = NumberFormatValues.Custom },
                        new ParagraphProperties(
                            new Indentation() { Left = indentDistance.ToString() }
                            )
                      )
            { LevelIndex = indentLevel };

            abstractNum.AppendChild<Level>(thisLevel);

            foreach (var line in lines)
            {
                var thisLineAsParagraph = new Paragraph(
                  new ParagraphProperties(
                    new NumberingProperties(
                      new NumberingLevelReference() { Val = indentLevel },
                      new NumberingId() { Val = numberingID }),
                    new ParagraphStyleId() { Val = "ListParagraph"}),
                  new Run(
                    new RunProperties(),
                    new Text(line.symbol + " " + line.text) { Space = SpaceProcessingModeValues.Preserve }));

                paragraphs.Add(thisLineAsParagraph);
                if(line.Lines != null)
                {
                    CreateBulletedListParagraph(abstractNum, paragraphs, line.Lines, numberingID, indentLevel + 1);
                }
            }
        }
    }
}
