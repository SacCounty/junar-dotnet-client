using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using JunarUpload;

public class Program
{
    public static void Main(string[] args)
    {
        CommandLineApplication commandLineApplication = new CommandLineApplication(true);
        CommandArgument names = null;
        commandLineApplication.Command("key", (target) =>
                names = target.Argument("app_key", "Enter the app_key for publishing to Junar.", false));

        CommandOption key = commandLineApplication.Option("-k |--key <key>", "The app key for Junar.", CommandOptionType.SingleValue);
        CommandOption url = commandLineApplication.Option("-u |--url <url>", "The URL of the dataset.", CommandOptionType.SingleValue);
        CommandOption Title = commandLineApplication.Option("-t |--title <title>", "Name for your data. Up to 100 characters", CommandOptionType.SingleValue);
        CommandOption Description = commandLineApplication.Option("-d |--description <description>", "Information about this file. Up to 140 characters", CommandOptionType.SingleValue);
        CommandOption Category = commandLineApplication.Option("-c |--category <category>", "A valid category name", CommandOptionType.SingleValue);
        CommandOption Tags = commandLineApplication.Option("-ta |--tags <tags>", "A colon separated values list like: tag1:tag2:tag3", CommandOptionType.MultipleValue);
        CommandOption Notes = commandLineApplication.Option("-n |--notes <notes>", "Do not use if you don't need it", CommandOptionType.MultipleValue);
        CommandOption License = commandLineApplication.Option("-l |--license <license>", "License for use of open data; Optional", CommandOptionType.MultipleValue);
        CommandOption FilePath = commandLineApplication.Option("-f |--file_path <file_path>", "Path to readable spreadsheet file", CommandOptionType.MultipleValue);
        CommandOption ContentType = commandLineApplication.Option("-ct |--ContentType <ContentType>", "Content Type for uploading file", CommandOptionType.MultipleValue);
        CommandOption GUID = commandLineApplication.Option("-g |--guid <guid>", "Optional. use it to update existing data.", CommandOptionType.MultipleValue);
        CommandOption TableId = commandLineApplication.Option("-a |--table_id <table_id>", "Use table0 for the first sheet (or single csv file)", CommandOptionType.MultipleValue);
        CommandOption Get = commandLineApplication.Option("-G |--g <get>", "Http Get response instead of POST.", CommandOptionType.SingleValue);

        commandLineApplication.HelpOption("-? | -h | --help");
        commandLineApplication.OnExecute(async () =>
        //commandLineApplication.OnExecute(() =>
        {
            UploadOptions options = new UploadOptions()
            {
                Title = Title.Value(),
                Description = Description.Value(),
                Category = Category.Value(),
                Tags = Tags.Values,
                Notes = Notes.Value(),
                License = License.Value(),
                FilePath = FilePath.Value(),
                ContentType = ContentType.Value(),
                GUID = GUID.Value(),
                TableId = TableId.Value(),
                Get = Get.Value()
            };

            //Program.AppSettings appset = new Program.AppSettings();
            //string guey = AppSettings["GUID"];

            string sKey = key.Value();
            string sUrl = url.Value();

            if (sKey.Length == 0 || sUrl.Length == 0)
            {
                throw new Exception("Invalid options");
            }

            //IApplicationEnvironment app = null;

            //Program upload = new Program(app);
            Program upload = new Program();

            if (options.Get != null)
                upload.GetResponse(sKey, sUrl);
            else
                await upload.SubmitFormViaPOST(sUrl, "file_data", options, sKey);

            return 0;
        });
        commandLineApplication.Execute(args);
    }

    public string GetResponse(string key, string url)
    {
        using (HttpClient client = new HttpClient())
        {
            url = url + "?auth_key=" + key;
            dynamic response = client.GetAsync(url).Result;
            dynamic responseContent = new StreamReader(response.Content.ReadAsStreamAsync().Result).ReadToEnd();

            Console.Out.Write(responseContent);

            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }

            return responseContent;
        }
    }

    public async Task SubmitFormViaPOST(string serverUrl,
            string fileControlName,
            UploadOptions request,
            string key)
    {
        FileInfo fileInfo = new FileInfo(request.FilePath);
        Uri RequestUri = new Uri(serverUrl);

        // Create HTTP transport objects
        HttpRequestMessage httpRequest = null;
        try
        {
            #region Build Request Content

            // 27 dashes then current ticks as hexa decimal
            string partBoundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] partBoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + partBoundary + "\r\n");
            string formContentType = "multipart/form-data; boundary=" + partBoundary;

            MemoryStream requestStream = new MemoryStream();

            List<KeyValuePair<string, string>> kvps = new List<KeyValuePair<string, string>>();
            kvps.Add(new KeyValuePair<string, string>("auth_key", key));
            kvps.Add(new KeyValuePair<string, string>("title", request.Title));
            kvps.Add(new KeyValuePair<string, string>("description", request.Description));
            kvps.Add(new KeyValuePair<string, string>("notes", request.Notes));
            kvps.Add(new KeyValuePair<string, string>("meta_data", request.Notes));
            kvps.Add(new KeyValuePair<string, string>("category", request.Category));
            kvps.Add(new KeyValuePair<string, string>("tags", string.Join(",", request.Tags ?? new List<string>())));
            if (!string.IsNullOrWhiteSpace(request.License))
                kvps.Add(new KeyValuePair<string, string>("license", request.License));
            if (!string.IsNullOrWhiteSpace(request.TableId))
                kvps.Add(new KeyValuePair<string, string>("table_id", request.TableId));
            else
                kvps.Add(new KeyValuePair<string, string>("table_id", "0"));

            if ((!string.IsNullOrWhiteSpace(request.GUID)))
            {
                kvps.Add(new KeyValuePair<string, string>("guid", request.GUID));
                kvps.Add(new KeyValuePair<string, string>("clone", "True"));
            }


            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (KeyValuePair<string, string> kv in kvps)
            {
                requestStream.Write(partBoundaryBytes, 0, partBoundaryBytes.Length);
                string formItem = string.Format(formdataTemplate, kv.Key, kv.Value);
                byte[] formItemBytes = System.Text.Encoding.ASCII.GetBytes(formItem);
                requestStream.Write(formItemBytes, 0, formItemBytes.Length);
            }

            requestStream.Write(partBoundaryBytes, 0, partBoundaryBytes.Length);

            FileInfo uplFile = new FileInfo(request.FilePath);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, fileControlName, uplFile.Name, request.ContentType);
            byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(header);

            requestStream.Write(headerBytes, 0, headerBytes.Length);

            FileStream fileStream = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Dispose();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + partBoundary + "--\r\n");
            requestStream.Write(trailer, 0, trailer.Length);

            #endregion

            #region creates HttpRequestMessage object

            httpRequest = new HttpRequestMessage();
            httpRequest.Method = HttpMethod.Post;
            httpRequest.RequestUri = RequestUri;
            requestStream.Position = 0;
            httpRequest.Content = new StreamContent(requestStream);
            httpRequest.Content.Headers.Remove("Content-Type");
            httpRequest.Content.Headers.TryAddWithoutValidation("Content-Type", formContentType);

            #endregion

            #region Send the request and process response
            // Send Request

            HttpResponseMessage httpResponse = null;
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(300);
                httpResponse = await httpClient.SendAsync(httpRequest);

                System.Net.HttpStatusCode statusCode = httpResponse.StatusCode;
                string errorResponse =
                    await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                Console.Out.Write(errorResponse);
                if (statusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(errorResponse);
                }

            }
            finally
            {
                if (httpResponse != null)
                {
                    httpResponse.Dispose();
                }
            }

            #endregion
        }
        finally
        {
            if (httpRequest != null)
            {
                httpRequest.Dispose();
            }
        }
    }

}
