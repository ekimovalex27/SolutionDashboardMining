using System;

using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

public static class SharedLibraryDashboardMining
{
  #region Define vars
  private static string SimpleMiningNet = "https://simplemining.net";
  private static string SimpleMiningNet_Login = "https://simplemining.net/account/login";

  private static Uri SimpleMiningNet_URI_Main = new Uri(SimpleMiningNet);
  private static Uri SimpleMiningNet_URI_Login = new Uri(SimpleMiningNet_Login);

  //private static string SimpleMining_UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; WebView/3.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299"; //WebView
  private static string SimpleMining_UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299"; //Microsoft Edge
  #endregion Define vars

  public static string GetSimpleMiningNetMain()
  {
    return SimpleMiningNet;
  }

  public static Uri GetSimpleMiningNetURIMain()
  {
    return SimpleMiningNet_URI_Main;
  }

  public static Uri GetSimpleMiningNetURILogin()
  {
    return SimpleMiningNet_URI_Login;
  }

  private static string GetRandomString(int lengthString)
  {
    string randomString = "";

    var rand = new Random(DateTime.Now.Millisecond);
    for (int i = 0; i < lengthString; i++)
    {
      if (rand.Next(0, 100) < 50)
      {
        randomString += rand.Next(0, 9).ToString();
      }
      else
      {
        randomString += (char)rand.Next('a', 'z' + 1);
      }
    }

    return randomString;
  }

  public static string GetSimpleMining_cfduid()
  {
    //"d3048411731999302bc94903811588f721509015489"
    return GetRandomString(43);
  }

  public static string GetSimpleMining_cflb()
  {
    return "2312631544";
  }

  public static string GetSimpleMining_PHPSESSID()
  {
    //"hhp1nqm87b5ri2o0e56fte2m10"
    return GetRandomString(26);
  }

  public static async Task<Tuple<byte[], string, string, string, string>> GetCaptchaAsync(int Mining)
  {
    #region Define vars
    Tuple<byte[], string, string, string, string> returnValue;
    #endregion Define vars

    #region Try
    try
    {
      using (var client = new HttpClient(new HttpClientHandler { UseCookies = false }))
      {
        client.BaseAddress = new Uri(SimpleMiningNet);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        //client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 YaBrowser/17.9.1.768 Yowser/2.5 Safari/537.36");
        client.DefaultRequestHeaders.Add("X-Requested-With", "X-Requested-With");
        client.DefaultRequestHeaders.Referrer = SimpleMiningNet_URI_Main;
        //client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("max-age");

        var cfduid = GetSimpleMining_cfduid();
        var cflb = GetSimpleMining_cflb();
        var PHPSESSID = GetSimpleMining_PHPSESSID();

        client.DefaultRequestHeaders.Add("Cookie", "__cfduid=" + cfduid + "; __cflb=" + cflb + "; PHPSESSID=" + PHPSESSID);

        var response = await client.GetAsync("captcha");
        response.EnsureSuccessStatusCode();

        var resultPNG = await response.Content.ReadAsByteArrayAsync();

        returnValue = new Tuple<byte[], string, string, string, string>(resultPNG, cfduid, cflb, PHPSESSID, "");
      }
    }
    #endregion Try

    #region Catch
    catch (Exception ex)
    {
      returnValue = new Tuple<byte[], string, string, string, string>(null, "", "", "", ex.Message);
    }
    #endregion Catch

    return returnValue;
  }

  public static async Task<string> LoginAsync(int Mining, string user, string password, string captcha, string SimpleMining_cfduid, string SimpleMining_cflb, string SimpleMining_PHPSESSID)
  {
    string returnValue;

    try
    {
      string content = string.Format("{0}={1}&{2}={3}&{4}={5}", WebUtility.UrlEncode("data[User][email]"), WebUtility.UrlEncode(user), WebUtility.UrlEncode("data[User][password]"), WebUtility.UrlEncode(password), WebUtility.UrlEncode("data[User][captcha]"), WebUtility.UrlEncode(captcha));

      HttpWebRequest request = WebRequest.Create(SimpleMiningNet + "/account/login") as HttpWebRequest;
      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded";
      request.Headers["Cache-Control"] = "max-age=0";
      request.Headers["Content-Length"] = content.Length.ToString();
      request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
      request.Headers["Accept-Encoding"] = "gzip, deflate, br";
      request.Headers["Accept-Language"] = "en;q=0.8";

      //request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 YaBrowser/17.9.1.768 Yowser/2.5 Safari/537.36";
      request.Headers["User-Agent"] = SimpleMining_UserAgent;

      request.Headers["Referer"] = SimpleMiningNet_Login;
      request.Headers["Origin"] = SimpleMiningNet;
      request.Headers["Upgrade-Insecure-Requests"] = "1";
      var CookieContainer = new CookieContainer();
      CookieContainer.Add(request.RequestUri, new Cookie("language", "us"));
      CookieContainer.Add(request.RequestUri, new Cookie("__cfduid", SimpleMining_cfduid));
      CookieContainer.Add(request.RequestUri, new Cookie("__cflb", SimpleMining_cflb));
      CookieContainer.Add(request.RequestUri, new Cookie("PHPSESSID", SimpleMining_PHPSESSID));      
      request.CookieContainer = CookieContainer;

      using (var writer = new StreamWriter(await request.GetRequestStreamAsync()))
      {
        await writer.WriteAsync(content);
      }

      var response = await request.GetResponseAsync() as HttpWebResponse;

      if (response != null)
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          if (response.Cookies.Count > 0)
          {
            if (response.Headers["Set-Cookie"] != null && response.ResponseUri.Segments.Length == 1)
            {
              returnValue = "";
            }
            else
            {
              returnValue = "Error authentification";
            }
          }
          else
          {
            returnValue = "Error Login";
          }          
        }
        else
        {
          returnValue = "Error: StatusCode=" + response.StatusCode.ToString();
        }
      }
      else
      {
        returnValue = "Response is null";
      }
    }
    catch (Exception ex)
    {
      returnValue = ex.Message;
    }

    return returnValue;
  }

  public static async Task<string> LogoutAsync(int Mining, string cfduid, string cflb, string PHPSESSID)
  {
    #region Define vars
    string returnValue;
    #endregion Define vars

    #region Try
    try
    {
      HttpWebRequest request = WebRequest.Create(SimpleMiningNet + "/account/logout") as HttpWebRequest;
      request.Method = "GET";
      request.ContentType = "application/json";
      request.Accept = "application/json, text/javascript, */*; q=0.01";
      request.Headers["Accept-Encoding"] = "gzip, deflate, br";
      request.Headers["Accept-Language"] = "ru,en;q=0.8";
      request.Headers["User-Agent"] = SimpleMining_UserAgent;
      request.Headers["X-Requested-With"] = "XMLHttpRequest";
      request.Headers["Referer"] = SimpleMiningNet;

      var CookieContainer = new CookieContainer();
      CookieContainer.Add(request.RequestUri, new Cookie("__cfduid", cfduid));
      CookieContainer.Add(request.RequestUri, new Cookie("__cflb", cflb));
      CookieContainer.Add(request.RequestUri, new Cookie("PHPSESSID", PHPSESSID));
      request.CookieContainer = CookieContainer;

      var response = await request.GetResponseAsync() as HttpWebResponse;

      if (response != null)
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          returnValue = "";
        }
        else
        {
          returnValue = "Error: StatusCode=" + response.StatusCode.ToString();
        }
      }
      else
      {
        returnValue = "Respoone is null";
      }
    }
    #endregion Try

    #region Catch
    catch (Exception ex)
    {
      returnValue = ex.ToString();
    }
    #endregion Catch

    return returnValue;
  }

  public static async Task<Tuple<string, string>> GetListRigsAsync(CancellationToken token, string cfduid, string cflb, string PHPSESSID)
  {
    #region Define vars
    Tuple<string, string> returnValue;
    #endregion Define vars

    #region Try
    try
    {
      using (var client = new HttpClient(new HttpClientHandler { UseCookies = false }))
      {
        client.BaseAddress = new Uri(SimpleMiningNet);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        //client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
        client.DefaultRequestHeaders.Add("User-Agent", SimpleMining_UserAgent);
        client.DefaultRequestHeaders.Add("X-Requested-With", "X-Requested-With");
        client.DefaultRequestHeaders.Referrer = SimpleMiningNet_URI_Main;
        //client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("max-age");        

        client.DefaultRequestHeaders.Add("Cookie", "__cfduid=" + cfduid + "; __cflb=" + cflb + "; PHPSESSID=" + PHPSESSID);

        var response = await client.GetAsync("json/getListRigs?sort=name&order=asc", token);
        response.EnsureSuccessStatusCode();

        var resultRead = await response.Content.ReadAsStringAsync();
        resultRead = WebUtility.HtmlDecode(resultRead);
        resultRead = WebUtility.UrlDecode(resultRead);
        resultRead = Uri.UnescapeDataString(resultRead);

        returnValue = new Tuple<string, string>(resultRead, "");
      }

      //HttpWebRequest request = WebRequest.Create(SimpleMiningNet + "/json/getListRigs?sort=name&order=asc") as HttpWebRequest;
      //request.Method = "GET";
      //request.ContentType = "application/json";
      //request.Accept = "application/json, text/javascript, */*; q=0.01";
      //request.Headers["Accept-Encoding"] = "gzip, deflate, br";
      //request.Headers["Accept-Language"] = "ru,en;q=0.8";
      //request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 YaBrowser/17.9.1.768 Yowser/2.5 Safari/537.36";
      //request.Headers["X-Requested-With"] = "XMLHttpRequest";
      //request.Headers["Referer"] = SimpleMiningNet;

      //var CookieContainer = new CookieContainer();
      //CookieContainer.Add(request.RequestUri, new Cookie("__cfduid", "d3048411731999302bc94903811588f721509015489"));
      //CookieContainer.Add(request.RequestUri, new Cookie("PHPSESSID", PHPSESSID()));
      //request.CookieContainer = CookieContainer;

      //var response = await request.GetResponseAsync() as HttpWebResponse;

      //string resultJson;
      //using (var responseStream = response.GetResponseStream())
      //{
      //  using (var readStream = new StreamReader(responseStream,true))
      //  {
      //    resultJson = await readStream.ReadToEndAsync();
      //  }
      //}

      //returnValue = new Tuple<string, string>(resultJson, "");      
    }
    #endregion Try

    #region Catch
    catch (Exception ex)
    {
      returnValue = new Tuple<string, string>("", ex.Message);
    }
    #endregion Catch

    return returnValue;
  }

  public static async Task<string> RebootAsync(int Mining, string id, string cfduid, string PHPSESSID)
  {
    string returnValue;

    try
    {
      string content = string.Format("id={0}", id);

      HttpWebRequest request = WebRequest.Create(SimpleMiningNet + "/json/rebootRig") as HttpWebRequest;
      request.Method = "POST";
      request.Headers["Origin"] = SimpleMiningNet;
      request.Headers["Referer"] = SimpleMiningNet;
      request.Headers["Accept-Language"] = "en;q=0.8";
      request.Headers["User-Agent"] = SimpleMining_UserAgent;
      request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
      request.Accept = "application/json, text/javascript, */*; q=0.01";
      request.Headers["X-Requested-With"] = "XMLHttpRequest";
      request.Headers["Accept-Encoding"] = "gzip, deflate, br";
      request.Headers["Cache-Control"] = "max-age=0";
      request.Headers["Content-Length"] = content.Length.ToString();

      var CookieContainer = new CookieContainer();
      CookieContainer.Add(request.RequestUri, new Cookie("language", "us"));
      CookieContainer.Add(request.RequestUri, new Cookie("__cfduid", cfduid));
      CookieContainer.Add(request.RequestUri, new Cookie("PHPSESSID", PHPSESSID));
      request.CookieContainer = CookieContainer;

      using (var writer = new StreamWriter(await request.GetRequestStreamAsync()))
      {
        await writer.WriteAsync(content);
      }

      //return null;
      var response = await request.GetResponseAsync() as HttpWebResponse;
      
      if (response != null)
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          if (response.Cookies.Count > 0)
          {
            returnValue = "";
          }
          else
          {
            returnValue = "Error reboot rig";
          }
        }
        else
        {
          returnValue = "Error: StatusCode=" + response.StatusCode.ToString();
        }
      }
      else
      {
        returnValue = "Response is null";
      }
    }
    catch (Exception ex)
    {
      returnValue = ex.ToString();
    }

    return returnValue;
  }

  public static async Task<Tuple<string, string>> GetVersionAsync(string cfduid, string PHPSESSID)
  {
#region Define vars
    Tuple<string, string> returnValue;
#endregion Define vars

#region Try
    try
    {
      HttpWebRequest request = WebRequest.Create(SimpleMiningNet + "/json/getVersion") as HttpWebRequest;
      request.Method = "GET";
      request.Accept = "application/json, text/javascript, */*; q=0.01";
      request.Headers["Accept-Encoding"] = "gzip, deflate, br";
      request.Headers["Accept-Language"] = "ru,en;q=0.8";
      request.Headers["User-Agent"] = SimpleMining_UserAgent;
      request.Headers["X-Requested-With"] = "XMLHttpRequest";
      request.Headers["Referer"] = SimpleMiningNet;

      var CookieContainer = new CookieContainer();
      CookieContainer.Add(request.RequestUri, new Cookie("__cfduid", cfduid));
      CookieContainer.Add(request.RequestUri, new Cookie("PHPSESSID", PHPSESSID));
      request.CookieContainer = CookieContainer;

      var response = await request.GetResponseAsync() as HttpWebResponse;

      if (response != null)
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          //returnValue = "";
        }
        else
        {
          //returnValue = "Error: StatusCode=" + response.StatusCode.ToString();
        }
      }
      else
      {
        //returnValue = "Respoone is null";
      }


      //using (var client = new HttpClient())
      //{
      //  client.BaseAddress = new Uri(SimpleMiningNet);
      //  client.DefaultRequestHeaders.Accept.Clear();
      //  client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      //  var response = await client.GetAsync("json/getVersion", HttpCompletionOption.ResponseContentRead);
      //  response.EnsureSuccessStatusCode();

      //  var resultJson = await response.Content.ReadAsStringAsync();
      //  var parseResultJson = Newtonsoft.Json.Linq.JObject.Parse(resultJson);

      //  var Version = parseResultJson["version"].ToString();

      //  if (string.IsNullOrEmpty(Version)) throw new ArgumentException("Version is empty");

      //  var ErrorMessage = "";
      //  returnValue = new Tuple<string, string>(Version, ErrorMessage);
      //}
    }
#endregion Try

#region Catch
    catch (Exception ex)
    {
      returnValue = new Tuple<string, string>("", ex.Message);
    }
#endregion Catch

    return returnValue=null;
  }

  public static async Task<string> RigsAsync(string cfduid, string PHPSESSID)
  {
#region Define vars
    string returnValue;
#endregion Define vars

#region Try
    try
    {
      HttpWebRequest request = WebRequest.Create(SimpleMiningNet + "/account/rigs") as HttpWebRequest;
      request.Method = "GET";
      request.Accept = "*/*";
      //request.Headers["Accept-Encoding"] = "gzip, deflate, br";
      //request.Headers["Accept-Language"] = "ru,en;q=0.8";
      request.Headers["User-Agent"] = SimpleMining_UserAgent;
      request.Headers["X-Requested-With"] = "XMLHttpRequest";
      request.Headers["Referer"] = SimpleMiningNet;

      var CookieContainer = new CookieContainer();      
      CookieContainer.Add(request.RequestUri, new Cookie("__cfduid", cfduid));
      CookieContainer.Add(request.RequestUri, new Cookie("language", "us"));
      CookieContainer.Add(request.RequestUri, new Cookie("PHPSESSID", PHPSESSID));
      request.CookieContainer = CookieContainer;

      var response = await request.GetResponseAsync() as HttpWebResponse;

      //if (response != null)
      //{
      //  if (response.StatusCode == HttpStatusCode.OK)
      //  {
      //    returnValue = "";
      //  }
      //  else
      //  {
      //    returnValue = "Error: StatusCode=" + response.StatusCode.ToString();
      //  }
      //}
      //else
      //{
      //  returnValue = "Respoone is null";
      //}
    }
#endregion Try

#region Catch
    catch (Exception ex)
    {
      returnValue = ex.Message;
    }
#endregion Catch

    return returnValue=null;
  }
}

namespace SharedLibrary
{
  //public class SharedLibrary : ContentPage
  //{
  //  public SharedLibrary()
  //  {
  //    var button = new Button
  //    {
  //      Text = "Click Me!",
  //      VerticalOptions = LayoutOptions.CenterAndExpand,
  //      HorizontalOptions = LayoutOptions.CenterAndExpand,
  //    };

  //    int clicked = 0;
  //    button.Clicked += (s, e) => button.Text = "Clicked: " + clicked++;

  //    Content = button;
  //  }
  //}
}
