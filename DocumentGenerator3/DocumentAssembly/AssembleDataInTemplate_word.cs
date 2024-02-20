using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using DocumentGenerator3.ImageHandling;
using DocumentFormat.OpenXml;
using System.Drawing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using DocumentGenerator3.BulletedListData;
using DocumentGenerator3.ChildDatasetData;
using DocumentFormat.OpenXml.Office2010.Word;
using System.Reflection;
using NumberingFormat = DocumentFormat.OpenXml.Wordprocessing.NumberingFormat;

namespace DocumentGenerator3.DocumentAssembly
{
    public class AssembleDataInTemplate_word
    {
        public DocumentData fileData { get; set; }
        public byte[] AssembleData()
        {
            Stream stream = new MemoryStream(0);

            stream.Write(fileData.fileContents, 0, (int)fileData.fileContents.Length);
            using (WordprocessingDocument wdDoc = WordprocessingDocument.Open(stream, true))
            {
                UpdateDocumentSettingsOnOpen(wdDoc);

                if (fileData.bulletedListCollection.Count > 0)
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
                wdDoc.MainDocumentPart.Document.Save();

                foreach (var child in fileData.originalPayload.child_datasets)
                {
                    if (child.settings is ChildSettings_quickbase)
                    {
                        ChildSettings_quickbase settings = (ChildSettings_quickbase)child.settings;
                        var groupBy = settings.groupBy;
                        if (groupBy != "")
                        {
                            AddGroupingRowsToChildTables(wdDoc, settings);
                        }
                    }
                }

                AddFormattingToChildTables(wdDoc);
                wdDoc.MainDocumentPart.Document.Save();

                AddImages(wdDoc);

                UpdateInteractableObjects(bod);
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

        private void UpdateInteractableObjects(Body bod)
        {
            foreach (var interactableObject in fileData.originalPayload.interactable_objects.interactables)
            {
                var direction = interactableObject.beforeOrAfterText == "before" ? SearchDirection.Before : SearchDirection.After;

                string methodName = $"Set{interactableObject.type}";

                MethodInfo method = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (method == null)
                {
                    throw new InvalidOperationException($"No method found for type '{interactableObject.type}'");
                }

                method.Invoke(this, new object[] { bod, interactableObject.searchText, interactableObject.value, direction, interactableObject.nthInstanceOfSearchText });
            }
        }

        private void AddImages(WordprocessingDocument wdDoc)
        {
            foreach (var img in fileData.originalPayload.image_locations)
            {
                if (img.settings.image_bytes == null) continue;
                InsertPicture(wdDoc, img.settings);
            }
        }

        private static void UpdateDocumentSettingsOnOpen(WordprocessingDocument wdDoc)
        {
            DocumentSettingsPart settingsPart =
                wdDoc.MainDocumentPart.GetPartsOfType<DocumentSettingsPart>().First();

            UpdateFieldsOnOpen updateFields = new UpdateFieldsOnOpen();
            updateFields.Val = new OnOffValue(true);

            settingsPart.Settings.PrependChild<UpdateFieldsOnOpen>(updateFields);
            settingsPart.Settings.Save();
        }

        private void SetRadioButton_FormField(Body bod, string searchText, bool isChecked, SearchDirection direction)
        {

        }

        private void SetCheckbox_ContentControl(Body body, string searchText, bool isChecked, SearchDirection direction, int nthInstanceOfSearchText)
        {
            var runWithText = body.Descendants<Run>()
                                    .Skip(nthInstanceOfSearchText-1)
                                    .FirstOrDefault(r => r.InnerText.Contains(searchText));

            if (runWithText == null) return;

            IEnumerable<SdtElement> targetElements = direction == SearchDirection.After
                                                        ? runWithText.ElementsAfter().OfType<SdtElement>()
                                                        : runWithText.ElementsBefore().OfType<SdtElement>().Reverse();

            SetContentControlCheckboxState(targetElements, isChecked);            
        }

        private void SetContentControlCheckboxState(IEnumerable<SdtElement> elements, bool isChecked)
        {
            var checkboxControl = elements.FirstOrDefault(s => s.SdtProperties.GetFirstChild<SdtContentCheckBox>() != null);
            if (checkboxControl == null) return;

            var checkbox = checkboxControl.SdtProperties.GetFirstChild<SdtContentCheckBox>();
            var contentControlContent = checkboxControl.GetFirstChild<SdtContentRun>();
            if (checkbox == null || contentControlContent == null) return;

            checkbox.Checked.Val = isChecked ? OnOffValues.True : OnOffValues.False;
            char checkboxSymbol = isChecked ? '\u2612' : '\u2610'; // Checked and unchecked symbols
            char oppositeCheckboxSymbol = isChecked ? '\u2610' : '\u2612';

            var run = (Run)contentControlContent.Descendants<Run>().Where(t => t.InnerText == "\u2612" || t.InnerText == "\u2610").FirstOrDefault();
            if (run == null) return;

            var textElement = (Text)run.Descendants<Text>().Where(t => t.Text == oppositeCheckboxSymbol.ToString()).FirstOrDefault();
            if (textElement == null) return;

            textElement.Text = checkboxSymbol.ToString();
        }

        private void SetCheckbox_FormField(Body body, string searchText, bool isChecked, SearchDirection direction, int nthInstanceOfSearchText)
        {
            //var runWithText = body.Descendants<Run>()
                                    //.Skip(nthInstanceOfSearchText - 1)
                                    //.FirstOrDefault(r => r.InnerText.Contains(searchText));

            var allRuns = body.Descendants<Run>().Where(r => r.InnerText.Contains(searchText)).ToList();
            if (allRuns.Count == 0) return;
            var runWithText = allRuns[nthInstanceOfSearchText - 1];

            if (runWithText == null)
            {
                Console.WriteLine($"No text '{searchText}' found");
                return;
            }

            IEnumerable<Run> targetRuns = direction == SearchDirection.After
                                            ? runWithText.ElementsAfter().OfType<Run>()
                                            : runWithText.ElementsBefore().OfType<Run>().Reverse();

            SetCheckboxForRun(targetRuns, isChecked, direction);  
        }

        private bool SetCheckboxForRun(IEnumerable<Run> runs, bool isChecked, SearchDirection direction)
        {
            var fldCharRun = runs.FirstOrDefault(r => r.Descendants<FieldChar>()
                           .Any(fc => fc.FieldCharType == FieldCharValues.Begin));

            if (fldCharRun == null) return false;

            var checkBox = fldCharRun.Descendants<CheckBox>().FirstOrDefault();
            var checkBoxDefault = checkBox?.GetFirstChild<DocumentFormat.OpenXml.Wordprocessing.Checked>();

            if (checkBoxDefault != null)
            {
                checkBoxDefault.Val = isChecked;
                return true;
            }

            checkBox.Append(new DocumentFormat.OpenXml.Wordprocessing.Checked() { Val = isChecked });

            return false;
        }

        private void AddGroupingRowsToChildTables(WordprocessingDocument wdDoc, ChildSettings_quickbase settings)
        {
            string settingsId = $"{settings.table_dbid}.{settings.id}";

            foreach (Table tbl in wdDoc.MainDocumentPart.Document.Body.Elements<Table>())
            {
                TableProperties tableProperties = tbl.Elements<TableProperties>().FirstOrDefault();
                TableCaption tableCaption = tableProperties.Elements<TableCaption>().FirstOrDefault();

                string caption = "";

                if (tableCaption is not null)
                {
                    caption = tableProperties.Elements<TableCaption>().FirstOrDefault().Val; 
                }

                if (tableProperties is not null && caption.Contains(settingsId))
                {
                    int groupingIndex = 1;
                    foreach (var line in fileData.listOfTableCSVs)
                    {
                        if (!line.Key.Contains(settingsId)) continue;
                        string previousEntry = "";
                        int offset = 0;

                        foreach (var grouping in line.Value.GroupByData)
                        {

                            if (previousEntry != grouping)
                            {
                                var allRows = tbl.Elements<TableRow>().ToList();

                                TableRow currentRow = allRows[groupingIndex + offset];
                                TableCellProperties cellOneProperties = new TableCellProperties();
                                cellOneProperties.Append(new HorizontalMerge()
                                {
                                    Val = MergedCellValues.Restart
                                });
                                cellOneProperties.Append(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
                                cellOneProperties.Append(new Shading() { Color = "auto", Fill = "#8A8A8D" });

                                ParagraphProperties paragraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });

                                TableRow newRow = new TableRow(new TableCell(cellOneProperties, new Paragraph(paragraphProperties, new Run(new RunProperties(new Text(grouping))))));

                                for (int i = 1; i < line.Value.CountOfColumns; i++)
                                {
                                    TableCellProperties additionalCellsProperties = new TableCellProperties();
                                    additionalCellsProperties.Append(new HorizontalMerge()
                                    {
                                        Val = MergedCellValues.Continue
                                    });
                                    additionalCellsProperties.Append(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
                                    ParagraphProperties cellParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                                    var newCell = new TableCell(additionalCellsProperties, new Paragraph(cellParagraphProperties, new Run(new Text(grouping))));
                                    newRow.AppendChild(newCell);
                                }

                                currentRow.InsertBeforeSelf(newRow);
                                previousEntry = grouping;
                                offset++;
                            }

                            groupingIndex++;
                        }

                    }
                }
            }
        }

        private void AddFormattingToChildTables(WordprocessingDocument wdDoc)
        {
            foreach (Table tbl in wdDoc.MainDocumentPart.Document.Body.Elements<Table>())
            {
                var thisTableProperties = tbl.Descendants<TableProperties>().FirstOrDefault();
                var thisTableCaption = thisTableProperties.Descendants<TableCaption>().FirstOrDefault();
                string thisTableCaptionVal = "";
                if (thisTableCaption is not null)
                {
                    thisTableCaptionVal = thisTableCaption.Val;
                }

                foreach (TableRow row in tbl.Descendants<TableRow>())
                {
                    foreach (TableCell cell in row.Descendants<TableCell>())
                    {
                        foreach (Paragraph paragraph in cell.Descendants<Paragraph>())
                        {
                            foreach (Run run in paragraph.Elements<Run>())
                            {
                                var thisTableChildDataset = fileData.originalPayload.child_datasets.Where(d => thisTableCaptionVal.Contains(d.settings.id)).FirstOrDefault();
                                if (thisTableChildDataset is not null)
                                {
                                    ChildSettings_quickbase thisTableSetting = (ChildSettings_quickbase)thisTableChildDataset.settings;

                                    RunProperties newRunProperties = new RunProperties(
                                        new RunFonts()
                                        {
                                            Ascii = thisTableSetting.table_font_family
                                        },
                                        new FontSize()
                                        {
                                            Val = (Convert.ToInt32(thisTableSetting.table_font_size) * 2).ToString()
                                        });

                                    var currentRunPropertiesList = run.Elements<RunProperties>().ToList();
                                    foreach (RunProperties currentRunProperties in currentRunPropertiesList)
                                    {
                                        if (currentRunProperties is not null)
                                        {
                                            var currentRunFont = currentRunProperties.Descendants<RunFonts>().FirstOrDefault();
                                            var currentFontSize = currentRunProperties.Descendants<FontSize>().FirstOrDefault();

                                            if (currentRunFont is not null)
                                            {
                                                currentRunFont = newRunProperties.RunFonts;
                                            }
                                            else
                                            {
                                                currentRunProperties.RunFonts = new RunFonts() { Ascii = newRunProperties.RunFonts.Ascii };
                                            }

                                            if (currentFontSize is not null)
                                            {
                                                currentFontSize = newRunProperties.FontSize;
                                            }
                                            else
                                            {
                                                currentRunProperties.FontSize = new FontSize() { Val = newRunProperties.FontSize.Val };
                                            }

                                            continue;
                                        }
                                    }
                                    run.PrependChild<RunProperties>(newRunProperties);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InsertPicture(WordprocessingDocument wordprocessingDocument, IImageSettings imageSettings)
        {
            MainDocumentPart mainPart = wordprocessingDocument.MainDocumentPart;

            //TODO update AddImagePart to use dynamic string of image type
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

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
            //TODO: Check if this is only necessary in Runs inside a table.
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
                        txt.Space = SpaceProcessingModeValues.Preserve;
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
                                             new A.Extents() { Cx = width, Cy = height }),
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

            Run runWithSlug = wordDoc.MainDocumentPart.Document.Body.Descendants<Run>().FirstOrDefault(r => r.InnerText == imageSlug);

            if (runWithSlug is not null)
            {

                runWithSlug.Parent.AppendChild(new Run(element));

                runWithSlug.Remove(); 
            }

            foreach (var header in wordDoc.MainDocumentPart.HeaderParts)
            {
                runWithSlug = header.RootElement.Descendants<Run>().FirstOrDefault(r => r.InnerText == imageSlug);

                if (runWithSlug is not null)
                {
                    runWithSlug.Parent.AppendChild(new Run(element));

                    runWithSlug.Remove();
                } 
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

        private static void PreconfigureTable(KeyValuePair<string, CsvWithMetadata> m, Table t, string tableCaption)
        {
            if (m.Key == tableCaption && m.Value.Csv != "\n")
            {
                CreateArray tableData = new CreateArray();
                var tableArray = tableData.LoadCsv(m.Value.Csv);

                TableGrid tg = t.GetFirstChild<TableGrid>();

                RunProperties runProperties = ResizeTableAsNecessary(t, tableData, tg);
                var lastRowIndex = tableArray.GetUpperBound(0);
                AddCsvDataToTable(t, tableArray, lastRowIndex, runProperties);
                //AddFormattingToTable(t, runProperties);
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

                TableRow header = (TableRow)t.GetFirstChild<TableRow>();
                foreach(Run run in header.Elements<Run>())
                    {
                    run.PrependChild<RunProperties>(runProperties);
                    run.RunProperties = runProperties;
                }

                while (originalNumberOfRows + i <= tableData.NumberOfRows)
                {
                    TableRow newRow = (TableRow)lastRow.CloneNode(true);
                    
                    var thisNewRow = t.Descendants<TableRow>().First().InsertAfterSelf(newRow);

                    foreach (TableCell cell in thisNewRow.Descendants<TableCell>())
                    {
                        var thisCell = cell.Descendants<Paragraph>().FirstOrDefault();

                        RunProperties clonedProperties = (RunProperties)runProperties.CloneNode(true);

                        thisCell.AppendChild(new Run(new RunProperties(clonedProperties)));
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
                var paragraphMarkRunProperties = rowCopyProperties.GetFirstChild<ParagraphMarkRunProperties>();
                
                if (paragraphMarkRunProperties is not null)
                {
                    var runFontType = paragraphMarkRunProperties.GetFirstChild<RunFonts>();
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
                else
                {
                    rowCopyProperties.ParagraphMarkRunProperties = new ParagraphMarkRunProperties(new RunFonts(), new FontSize() );
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
                        var currentRun = parag.Descendants<Run>().FirstOrDefault();
                        if (currentRun is null)
                        {
                            parag.Append(new Run()); 
                        }
                    }

                    Run run = parag.Elements<Run>().First();

                    run.Append(new Text());
                    
                    Text text = run.Elements<Text>().First();


                    text.Text = cellValue.Replace("*%*", ","); //.Replace("*|*","\n"); //Replace unusual character sequence with newline to reintroduce newlines when filling table cell; Replace unusual character sequence with comma to reintroduce commas when filling table cell
                }
            }
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
                                        if (run.RunProperties.RunFonts.Ascii is not null)
                                        {
                                            bool hasFontFamily = run.RunProperties.RunFonts.Ascii != "";
                                            run.RunProperties.RunFonts.Ascii = hasFontFamily ? runProperties.RunFonts.Ascii : "Times New Roman";
                                        }
                                    }
                                }
                                else
                                {
                                    run.RunProperties.AppendChild<RunFonts>(runProperties.RunFonts);
                                }

                                if (run.RunProperties.FontSize is not null)
                                {
                                    if (run.RunProperties.FontSize.Val is not null)
                                    {
                                        bool hasFontSize = run.RunProperties.FontSize.Val != "";
                                        run.RunProperties.FontSize.Val = hasFontSize ? runProperties.FontSize.Val : "20";
                                    }
                                }
                                else
                                {
                                    //run.RunProperties.AppendChild<FontSize>(runProperties.FontSize);
                                }

                                if (run.RunProperties.RunStyle is not null)
                                {
                                    if (run.RunProperties.RunStyle is not null)
                                    {
                                        run.RunProperties.RunStyle.Val = runProperties.RunStyle.Val;
                                    }
                                }
                            }
                            else
                            {
                                run.PrependChild<RunProperties>(runProperties);
                                run.RunProperties = runProperties;
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

                string elementSlug = "{{" + r.Key + "}}";

                FindAndReplaceTextInHeader(wdDoc, elementValue, elementSlug);

                FindAndReplaceTextInFooter(wdDoc, elementValue, elementSlug);

                FindAndReplaceTextInMainBody(wdDoc, elementValue, elementSlug);
                
            }

            wdDoc.MainDocumentPart.Document.Save();
        }

        private static void FindAndReplaceTextInMainBody(WordprocessingDocument wdDoc, string elementValue, string elementSlug)
        {
            var runWithSlugs = wdDoc.MainDocumentPart.Document.Descendants<Run>().Where(r => r.InnerText.Contains(elementSlug));

            foreach (var runWithSlug in runWithSlugs)
            {
                if (runWithSlug != null)
                {
                    var runText = runWithSlug.Descendants<Text>().FirstOrDefault();
                    if (runText != null)
                    {
                        runText.Text = runText.Text.Replace(elementSlug, elementValue);
                    }
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

        private static void EnsureSpacesArePreservedInTextElements(WordprocessingDocument wdDoc)
        {
            var allRuns = wdDoc.MainDocumentPart.Document.Descendants<Run>();

            foreach(Run run in allRuns)
            {
                var runText = run.Descendants<Text>().FirstOrDefault();
                if (runText != null)
                {
                    runText.Space = SpaceProcessingModeValues.Preserve;
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
                        runText.Space = SpaceProcessingModeValues.Preserve;
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
                    )
                    { AbstractNumberId = 1 },
                    new NumberingInstance(
                      new AbstractNumId() { Val = 1 }
                    )
                    { NumberID = numberId });

                var abstractNum = element.Descendants<AbstractNum>().FirstOrDefault();

                List<Paragraph> paragraphs = new List<Paragraph>();

                CreateBulletedListParagraph(abstractNum, paragraphs, bulletedList.Value.Lines, numberId, bulletedList.Value.font_family, bulletedList.Value.font_size);

                var elementSlug = bulletedList.Key;
                Run runWithSlug = wdDoc.MainDocumentPart.Document.Body.Descendants<Run>().FirstOrDefault(r => r.InnerText.Contains(elementSlug));

                if (runWithSlug is not null)
                {
                    var elementToInsertAfter = runWithSlug.Parent;
                    foreach (var para in paragraphs)
                    {
                        elementToInsertAfter.InsertAfterSelf<Paragraph>(para);
                        elementToInsertAfter = para;
                    }
                    runWithSlug.Remove();
                }

                element.Save(numberingPart);
            }
        }

        private void CreateBulletedListParagraph(AbstractNum abstractNum, List<Paragraph> paragraphs, List<BulletedListLine> lines, int numberingID, string fontFamily, string fontSize, int indentLevel = 0)
        {
            int doubleFontSize = Convert.ToInt32(fontSize) * 2;
            
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
                    new RunProperties(
                        new RunFonts() { Ascii = fontFamily },
                        new FontSize() { Val = doubleFontSize.ToString()}
                        ),
                    new Text(line.symbol + " " + line.text) { Space = SpaceProcessingModeValues.Preserve }));

                paragraphs.Add(thisLineAsParagraph);
                if(line.Lines != null)
                {
                    CreateBulletedListParagraph(abstractNum, paragraphs, line.Lines, numberingID, fontFamily, fontSize, indentLevel + 1);
                }
            }
        }
    }
}
