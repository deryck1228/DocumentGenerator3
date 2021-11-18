namespace DocumentGenerator3.PdfConversion
{
    public class CloudConvertImportBase64 : ICloudConvertTask
    {
        public string Operation { get; set; } = "import/base64";
        public string Name { get; set; }
        public string File { get; set; }
        public string Filename { get; set; }
        public string Serialize()
        {
            return "\"" + Name + "\":{\"operation\":\"" + Operation + "\",\"file\":\"" + File + "\",\"filename\":\"" + Filename + "\"}";
        }
    }
}
