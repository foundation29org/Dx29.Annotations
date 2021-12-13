using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dx29.Services
{
    public class DocConverterService
    {
        public DocConverterService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task<string> ConvertAsync(Stream stream, string strategy = "Auto")
        {
            var docText = await HttpClient.PUTAsync<DocumentText>($"Document/Parse?strategy={strategy}&timeout=1200", stream);
            return docText.Content;
        }

        public class DocumentText
        {
            public string Status { get; set; }
            public string Strategy { get; set; }
            public string Content { get; set; }
        }
    }
}
