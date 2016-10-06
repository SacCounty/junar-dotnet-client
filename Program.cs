using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
        CommandOption FilePath = commandLineApplication.Option("-f |--file_path <file_path>", "Path to readable spreadsheet file", CommandOptionType.MultipleValue);
        CommandOption GUID = commandLineApplication.Option("-g |--guid <guid>", "Optional. use it to update existing data.", CommandOptionType.MultipleValue);
        CommandOption TableId = commandLineApplication.Option("-a |--table_id <table_id>", "Use table0 for the first sheet (or single csv file)", CommandOptionType.MultipleValue);

        commandLineApplication.HelpOption("-? | -h | --help");

        commandLineApplication.OnExecute(() =>
        {
            if (key.HasValue() | url.HasValue())
            {
                throw new Exception("Invalid options");
            }

            UploadOptions options = new UploadOptions()
            {
                Title = Title.Value(),
                Description = Description.Value(),
                Category = Category.Value(),
                Tags = Tags.Values,
                Notes = Notes.Value(),
                FilePath = FilePath.Value(),
                GUID = GUID.Value(),
                TableId = TableId.Value()
            };

            //I was toying with the JunarClient class for a more direct port of the Python to C# 
            Program upload = new Program();
            //upload.PostWebService();
            upload.PostRecord(options, key.Value(), url.Value());

            return 0;
        });
        commandLineApplication.Execute(args);
    }

    public string PostRecord(UploadOptions request, string key, string url)
    {
        using (HttpClient client = new HttpClient())
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                List<KeyValuePair<string, string>> kvps = new List<KeyValuePair<string, string>>();
                kvps.Add(new KeyValuePair<string, string>("auth_key", key));
                kvps.Add(new KeyValuePair<string, string>("title", request.Title));
                kvps.Add(new KeyValuePair<string, string>("description", request.Description));
                kvps.Add(new KeyValuePair<string, string>("category", request.Category));
                kvps.Add(new KeyValuePair<string, string>("tags", string.Join(",", request.Tags ?? new List<string>())));
                kvps.Add(new KeyValuePair<string, string>("meta_data", request.Notes));
                kvps.Add(new KeyValuePair<string, string>("table_id", request.TableId));
                if ((!string.IsNullOrWhiteSpace(request.GUID)))
                {
                    kvps.Add(new KeyValuePair<string, string>("guid", request.GUID));
                    kvps.Add(new KeyValuePair<string, string>("clone", "true"));
                }
                HttpContent data = new FormUrlEncodedContent(kvps);

                byte[] content = System.IO.File.ReadAllBytes(request.FilePath);
                HttpContent file = new ByteArrayContent(content);

                form.Add(data);
                form.Add(file, "file_data");

                dynamic response = client.PostAsync(url, form).Result;
                dynamic responseContent = new StreamReader(response.Content.ReadAsStreamAsync().Result).ReadToEnd();

                if (response.IsSuccessStatusCode == false)
                {
                    return null;
                }

                return responseContent;
            }
        }
    }

    private void PostWebService()
    {
        //Call JunarClient functions
    }

}
