namespace DocumentGenerator3.PdfConversion
{
    public class CloudConvertImportUrl : ICloudConvertTask
    {
        public string Operation { get; set; } = "import/url";
        public string Name { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Serialize()
        {
            return "\"" + Name + "\":{\"operation\":\"" + Operation + "\",\"url\":\"" + Url + "\",\"filename\":\"" + Filename + "\"}";
        }
    }
}
