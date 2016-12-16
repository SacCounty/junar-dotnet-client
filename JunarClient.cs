using System;
using System.Net.Http;

public class JunarClient
{
    private string app_key = "YOUR_API_KEY";
	private string guid = "";

	private string base_uri = "http://apisandbox.junar.com";

	//public static void Main(string[] args)
 //   {
 //       try
 //       {
 //           JunarClient client = new JunarClient("CHILE-INDIC-UF");
 //           Console.WriteLine("DS INFO : " + client.info());
 //       }
 //       catch (Exception e)
 //       {
 //           // TODO Auto-generated catch block
 //           Console.WriteLine(e.StackTrace);
 //       }
 //   }

    public JunarClient(string p_guid)
    {
        this.setGUID(p_guid);
    }

    public string getGUID()
    {
        return guid;
    }

    public string getAuthKey()
    {
        return this.app_key;
    }

    public void setGUID(string p_guid)
    {
        guid = p_guid;
    }

    public string invoke(string[] p_params)
    {
        string url = "/datastreams/invoke/" + getGUID() + "?";
        url += "auth_key=" + app_key;

        for (int i = 0; i <= p_params.Length; i++)
        {
            url += "&pArgument" + i + "=" + p_params[i];
        }
        return callURI(url);
    }

    public string info()
    {
        string url = "/datastreams/" + getGUID() + "?auth_key=" + getAuthKey();
        return callURI(url);
    }

    private string callURI(string p_url)
    {
        HttpClient http_client = new HttpClient();
        string response_body = "";
        dynamic final_url = this.base_uri + p_url;

        Console.WriteLine("URL: " + final_url);
        try
        {
            //Dim httpget As HttpGet = New HttpGet(final_url)

            //Dim responseHandler As ResponseHandler<String>= New BasicResponseHandler()
            //response_body = http_client.execute(httpget, responseHandler)
        }
        finally
        {
            //http_client.getConnectionManager().shutdown()
        }

        return response_body;
    }
}