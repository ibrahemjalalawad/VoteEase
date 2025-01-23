using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Face.Detection.API
{
    public class Analyse
    {
        const string subscriptionKey = "<Subscription Key>";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";
        static async Task<string> MakeAnalysisRequest(string imageFilePath)
        {
            string resultat = string.Empty;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" + "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," + "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";
            string uri = uriBase + "?" + requestParameters;
            HttpResponseMessage httpResponseMessage;

            byte[] byteDataArray = PathImageAsByteArray(imageFilePath);
            using (ByteArrayContent content = new ByteArrayContent(byteDataArray))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                httpResponseMessage = await client.PostAsync(uri, content);
                string contentAsync = await httpResponseMessage.Content.ReadAsStringAsync();
                resultat = ToJson(contentAsync);
            }
            return resultat;
        }
        static byte[] PathImageAsByteArray(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
        static string ToJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder stringBuilder = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char item in json)
            {
                switch (item)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    stringBuilder.Append(item);
                else
                {
                    switch (item)
                    {
                        case '{':
                        case '[':
                            stringBuilder.Append(item);
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(new string(' ', --offset * indentLength));
                            stringBuilder.Append(item);
                            break;
                        case ',':
                            stringBuilder.Append(item);
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            stringBuilder.Append(item);
                            stringBuilder.Append(' ');
                            break;
                        default:
                            if (item != ' ') stringBuilder.Append(item);
                            break;
                    }
                }
            }

            return stringBuilder.ToString().Trim();
        }
    }
}
