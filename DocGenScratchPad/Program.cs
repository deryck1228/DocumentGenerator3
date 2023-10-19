using DocGenScratchPad;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using NotesFor.HtmlToOpenXml;
using System.Data;
using NumberingFormat = DocumentFormat.OpenXml.Wordprocessing.NumberingFormat;

string docFilePath = "C:\\Users\\deryc\\source\\repos\\DocumentGenerator3\\DocGenScratchPad\\HES_FM0320C-WX v2 (1).docx";
string htmlText = @"
<p>A <strong>process</strong> document is a detailed and structured written guide that outlines the step-by-step procedures, instructions, and guidelines required to complete a specific task, activity, or workflow within an organization. It serves as a comprehensive reference for individuals involved in the process, providing them with a clear roadmap to achieve a desired outcome. Process documents are commonly used across various functions to ensure consistency, efficiency, and effective execution of tasks.</p> <ol> <li> <p><strong>Title and Purpose:</strong> The document begins with a clear and descriptive title that reflects the nature of the process. The purpose of the process is then outlined, explaining why the process is important, what it aims to achieve, and its relevance within the organization.</p> </li> <li> <p><strong>Scope and Applicability:</strong> The scope defines the boundaries of the process, indicating where it starts and ends. It also specifies the situations, scenarios, or conditions under which the process should be followed.</p> </li> <li> <p><strong>Roles and Responsibilities:</strong> The document identifies the individuals or roles involved in the process and outlines their specific responsibilities at each step. This section clarifies who is accountable for various actions within the process.</p> </li> <li> <p><strong>System Processes:&nbsp;</strong>The System Processes list includes all systems and applications utilized within the process workflow, along with their designated purpose and resulting outcomes.</p> </li> <li> <p><strong>Step-by-Step Instructions:</strong> The heart of the process document consists of a detailed sequence of steps that need to be followed to complete the process. Each step is clearly numbered or labeled and accompanied by precise instructions, guidelines, and any necessary information to ensure successful execution.</p>
</li> <li> <p><strong>Supporting Information:</strong> Process documents may include relevant visuals, diagrams, flowcharts, or screenshots to provide visual aids that enhance understanding. These visuals can help illustrate complex processes, decision points, and branching paths.</p> </li> <li> <p><strong>Decision Points and Branching:</strong> In more intricate processes, decision points may occur where the course of action depends on certain conditions or variables. Process documents outline these decision points and provide guidance on the appropriate choices to make.</p> </li> <li> <p><strong>Exceptions and Error Handling:</strong> It&#39;s important to address potential exceptions, errors, or unexpected situations that may arise during the process. The document should provide instructions on how to handle these situations to ensure a smooth and effective resolution.</p> </li> <li> <p><strong>Checklists and Forms:</strong> Process documents may include checklists, forms, or templates that need to be completed at specific stages to ensure that the process is on track and all required information is collected.</p> </li> <li> <p><strong>Automations:</strong>&nbsp;Processes within an organization may involve corresponding automated workflows that operate concurrently or are activated in response to specific decisions and outcomes.</p> </li> <li> <p><strong>Timeframes and Deadlines:</strong> If time is a critical factor, the document may specify timeframes or deadlines for completing each step of the process. This helps manage expectations and ensure timely execution.</p> </li> <li> <p><strong>Review and Approval:</strong> For processes that require oversight or approval from higher levels, the document may outline the review and approval process, indicating who needs to review and authorize each stage.</p> </li> <li> <p><strong>References and Resources:</strong> Process documents often include references to relevant policies, guidelines, tools, software, or other resources that individuals may need while following the process.</p> </li> <li> <p><strong>Version Control:</strong> To ensure accuracy and relevance, process documents may include a version control section that tracks revisions and updates to the document over time.</p> </li> <li> <p><strong>Contact Information:</strong> The document concludes with contact information for individuals who can provide further clarification, support, or assistance related to the process.</p> </li> </ol>
";

//InsertHtmlIntoWord(docFilePath, htmlText);
SetCheckboxRelativeToText(docFilePath, "Electric furnace", true, SearchDirection.Before);


void SetCheckboxRelativeToText(string docPath, string searchText, bool isChecked, SearchDirection direction)
{
    using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docPath, true))
    {
        var body = wordDoc.MainDocumentPart.Document.Body;

        // Find the run with the given text
        var runWithText = body.Descendants<Run>()
                              .FirstOrDefault(r => r.InnerText.Contains(searchText));

        if (runWithText == null)
        {
            Console.WriteLine($"No text '{searchText}' found");
            return;
        }
        // Depending on the direction, look for the checkbox before or after the text
        IEnumerable<Run> targetRuns = direction == SearchDirection.After
                                     ? runWithText.ElementsAfter().OfType<Run>()
                                     : runWithText.ElementsBefore().OfType<Run>().Reverse();

        SetCheckboxForRun(targetRuns, isChecked);
    }
}

bool SetCheckboxForRun(IEnumerable<Run> runs, bool isChecked)
{
    // Find the Run which contains the fldChar of type "begin"
    var fldCharRun = runs.FirstOrDefault(r => r.Descendants<FieldChar>()
                                               .Any(fc => fc.FieldCharType == FieldCharValues.Begin));

    if (fldCharRun == null)
        return false;

    // Locate the checkBox and modify its default value
    var checkBox = fldCharRun.Descendants<CheckBox>().FirstOrDefault();
    var checkBoxDefault = checkBox?.GetFirstChild<DocumentFormat.OpenXml.Wordprocessing.Checked>();

    if (checkBoxDefault != null)
    {
        checkBoxDefault.Val = isChecked;
        return true;
    }

    return false;
}

void InsertHtmlIntoWord(string filePath, string htmlContent)
{
    htmlContent = htmlContent.Replace("&#39;", "'").Replace("&nbsp;", " ");

    using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, true))
    {
        MainDocumentPart mainPart = wordDoc.MainDocumentPart;

        // Parse HTML
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        foreach (var node in htmlDoc.DocumentNode.ChildNodes)
        {
            ProcessHtmlNode(mainPart.Document.Body, node, wordDoc);
        }

        mainPart.Document.Save();
    }
}

void ProcessHtmlNode(OpenXmlElement parentElement, HtmlNode node, WordprocessingDocument wordDoc)
{
    int numberingId = 1;

    switch (node.Name)
    {
        case "p":
            Paragraph para = new Paragraph();
            foreach (var childNode in node.ChildNodes)
            {
                ProcessHtmlNode(para, childNode, wordDoc); // Recursive call for children of the paragraph
            }
            parentElement.AppendChild(para);
            break;

        case "strong":
            Run boldRun = new Run();
            RunProperties runProps = new RunProperties();
            Bold bold = new Bold();
            runProps.Append(bold);
            boldRun.Append(runProps);

            // Process the child nodes of the <strong> tag and append the processed children to this Run
            foreach (var childNode in node.ChildNodes)
            {
                ProcessHtmlNode(boldRun, childNode, wordDoc);
            }

            parentElement.AppendChild(boldRun);
            break;

        case "ol":
        case "ul":
            numberingId = CreateNumberingDefinition(wordDoc, node.Name == "ol" ? NumberFormatValues.Decimal : NumberFormatValues.Bullet);
            foreach (var liNode in node.ChildNodes)
            {
                if (liNode.Name == "li")
                {
                    Paragraph listItemPara = new Paragraph(
                        new ParagraphProperties(
                            new NumberingProperties(
                                new NumberingLevelReference() { Val = 0 },
                                new NumberingId() { Val = numberingId })),
                        new Run(new Text(liNode.InnerText)));

                    parentElement.AppendChild(listItemPara);
                }
            }
            break;

        case "#text":
            string decodedText = HtmlEntity.DeEntitize(node.InnerText);
            Run run = new Run(new Text(decodedText));
            if (parentElement is Paragraph paraTextParent)
            {
                paraTextParent.AppendChild(run);
            }
            else if (parentElement is Run runTextParent)
            {
                runTextParent.AppendChild(new Text(decodedText));
            }
            break;
    }
}

int CreateNumberingDefinition(WordprocessingDocument wordDoc, NumberFormatValues listType)
{
    NumberingDefinitionsPart numberingPart;

    if (wordDoc.MainDocumentPart.GetPartsCountOfType<NumberingDefinitionsPart>() > 0)
    {
        numberingPart = wordDoc.MainDocumentPart.GetPartsOfType<NumberingDefinitionsPart>().First();
    }
    else
    {
        numberingPart = wordDoc.MainDocumentPart.AddNewPart<NumberingDefinitionsPart>();
        numberingPart.Numbering = new Numbering();
    }

    var numbering = numberingPart.Numbering;

    int id = numbering.Elements<AbstractNum>().Count() + 1;

    AbstractNum abstractNum = new AbstractNum(new Level(
        new NumberingFormat() { Val = listType },
        new LevelText() { Val = listType == NumberFormatValues.Decimal ? "%1." : "•" })
    { LevelIndex = 0 })
    { AbstractNumberId = id };

    numbering.Append(abstractNum);

    NumberingInstance instance = new NumberingInstance(
        new AbstractNumId() { Val = id })
    { NumberID = id };

    numbering.Append(instance);

    return id;
}



