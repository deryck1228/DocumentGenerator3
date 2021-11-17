using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepEqual.Syntax;
using DocumentGenerator3;
using DocumentGenerator3.TemplateData;
using Newtonsoft.Json;
using Xunit;

namespace DocumentGenerator3.Tests
{
    public class TemplateConverterTests
    {
        [Theory]
        [InlineData("{\"document_name\":\"2021213715 - Michelle Maiers - 11-15-2021\",\"document_type\":\".pdf\",\"template_location\":{\"settings\":{\"service\":\"quickbase\",\"app_dbid\":\"\",\"table_dbid\":\"brcui69xb\",\"realm\":\"peaksuite.quickbase.com\",\"apptoken\":\"\",\"usertoken\":\"b4dmad_nm59_5p292dccb6jjpdifut8xdjxsscs\",\"key_id\":\"1\",\"document_fid\":\"7\"}},\"parent_dataset\":{\"settings\":{\"service\":\"quickbase\",\"app_dbid\":\"\",\"table_dbid\":\"bqjqgprcc\",\"realm\":\"peaksuite.quickbase.com\",\"apptoken\":\"\",\"usertoken\":\"b4dmad_nm59_5p292dccb6jjpdifut8xdjxsscs\",\"rid\":\"2021213715-RNA-1\",\"merge_field_id\": \"11\"}},\"child_datasets\":[{\"service\":\"quickbase\",\"quickbase_settings\":{\"app_dbid\":\"\",\"table_dbid\":\"bqjqgprcc\",\"realm\":\"peaksuite.quickbase.com\",\"apptoken\":\"\",\"usertoken\":\"b4dmad_nm59_5p292dccb6jjpdifut8xdjxsscs\",\"id\":\"33007\",\"query\": \"{'3'.TV.'33007'}\",\"field_order\":\"3\",\"column_headers\":\"33007\"}}],\"delivery_method\":{\"service\":\"quickbase\",\"quickbase_settings\":{\"app_dbid\":\"\",\"table_dbid\":\"bqpzcxyet\",\"realm\": \"peaksuite.quickbase.com\",\"apptoken\":\"\",\"usertoken\":\"b4dmad_nm59_5p292dccb6jjpdifut8xdjxsscs\",\"rid\":\"\",\"key_id\":\"3\",\"document_field_data\":\"43:2021213715-RNA-1|28:1019564|54:2021213715 - Michelle Maiers - 11-15-2021 - Letter|53:US Biotek|77:1\",\"document_field_id\":\"10\"}}}")]
        public void ReadJson_ShouldDynamicallySerializeTemplateSettings_quickbase(string json)
        {
            //Arrange
            TemplateSettings_quickbase expected = new() 
            {   service = "quickbase", 
                app_dbid = "", 
                table_dbid = "brcui69xb", 
                realm = "peaksuite.quickbase.com" ,
                apptoken = "",
                usertoken = "b4dmad_nm59_5p292dccb6jjpdifut8xdjxsscs",
                key_id = "1",
                document_fid = "7"
            };

            //Act
            var deserializedJson = JsonConvert.DeserializeObject<DocumentGeneratorPayload>(json);
            var actual = deserializedJson.template_location.settings;

            //Assert
            expected.ShouldDeepEqual(actual);
            Assert.True(true);
        }
    }
}
