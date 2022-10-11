using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace DashboardMining
{
  public enum EnumDuringShow : int { Seconds = 0, Minuts = 1, Hours = 2, Days = 3 };

  public enum EnumMining : int { SimpleMiningNet = 0 };
  public enum EnumFarmProblem : int
  {
    FarmNotAvailable = 0,
    TheVideocardDoesNotWorkFor = 1,
    NoVideocardSpeedInfo = 2,
    TheVideocardIsRunningSlowly = 3,
    NoVideocardTemperatureInformation = 4,
    VideocardTemperatureLow = 5,
    VideocardTemperatureHigh = 6,
    NoFanDataOnVideocards = 7,
    TheFanOnVideocardIsWeak = 8,
    TheFanOnVideocardSpinsHard = 9,
    ErrorRig = 10
  };

  public enum EnumFarmAction : int { TakeNoAction = 0, Notification = 1, RebootTheRig = 2 };

  public class FarmActionObject
  {
    public EnumFarmAction enumFarmAction { get; set; }
    public string FarmAction { get => SharedClass.GetFarmAction(enumFarmAction); }
  }

  public class LoginMining
  {
    public string Id { get; set; }
    public EnumMining Mining { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string SimpleMining_cfduid { get; set; }
    public string SimpleMining_cflb { get; set; }
    public string SimpleMining_PHPSESSID { get; set; }
    public string SimpleMining_Captcha { get; set; }
    public bool IsLogged { get; set; }
    //public bool IsLogged { get => !string.IsNullOrEmpty(SimpleMining_cfduid) && !string.IsNullOrEmpty(SimpleMining_PHPSESSID) && !string.IsNullOrEmpty(SimpleMining_cflb) && !string.IsNullOrEmpty(SimpleMining_Captcha); }
  }

  public class MiningObject
  {
    public EnumMining Mining { get; set; }
    public string Title { get; set; }
    public string URL { get; set; }
  }

  public class RigObject
  {
    public string Login { get; set; }
    public string id { get; set; }
    public string group { get; set; }
    public string name { get; set; }
    public string ip { get; set; }
    public string state { get; set; }
    public double[] speed { get; set; }
    public double[] temps { get; set; }
    public double[] cooler { get; set; }
    public string speedAll { get; set; }
    public string tempsAll { get; set; }
    public string coolerAll { get; set; }
    public DateTime LastUpdate { get; set; }    
    public byte[] ipShow { get => System.Net.IPAddress.Parse(ip).GetAddressBytes(); }
    public DateTime LastRestart { get; set; }
    public int CountRestart { get; set; }
  }

  public class SandboxObject
  {
    public string id { get; set; }
    public DateTime CreateDateTime { get; set; }
    public TimeSpan during { get => DateTime.Now.Subtract(CreateDateTime); }
    public string duringShow { get => SharedClass.GetTimeSpanShow(during, EnumDuringShow.Seconds); }
    public EnumFarmProblem FarmProblem { get; set; }
    public string FarmProblemShow { get => SharedClass.GetFarmProblem(FarmProblem); }
    public string details { get; set; }
    public DateTime LastNotification { get; set; }
  }

  public class EventObject
  {
    public DateTime datetime { get; set; }
    public string id { get; set; }
    public string group { get; set; }
    public string name { get; set; }
    public EnumFarmProblem FarmProblem { get; set; }
    public string FarmProblemShow { get => SharedClass.GetFarmProblem(FarmProblem); }
    public TimeSpan during { get; set; }
    public string duringShow { get => SharedClass.GetTimeSpanShow(during, EnumDuringShow.Minuts); }    
    public EnumFarmAction FarmAction { get; set; }
    public string actionShow { get => SharedClass.GetFarmAction(FarmAction, CountRestart); }
    public string ip { get; set; }
    public string details { get; set; }
    public int CountRestart { get; set; }
  }

  public class TelegramBotObject
  {
    public int ChatId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Fullname { get => Firstname + " " + Lastname; }
    public string Username { get; set; }
    public string Language { get; set; }
    public string Text { get; set; }
    public string PhoneNumber { get; set; }
    public int MessageId { get; set; }
  }

  public enum NotifyType{StatusMessage, ErrorMessage};

  public class StoreItemDetails
  {
    public string Title { get; private set; }
    public string Price { get; private set; }
    public bool InCollection { get; private set; }
    public string ProductKind { get; private set; }
    public string StoreId { get; private set; }
    public string FormattedTitle => $"{Title} ({ProductKind}) {Price}, InUserCollection:{InCollection}";

    public StoreItemDetails(Windows.Services.Store.StoreProduct product)
    {
      Title = product.Title;
      Price = product.Price.FormattedPrice;
      InCollection = product.IsInUserCollection;
      ProductKind = product.ProductKind;
      StoreId = product.StoreId;
    }
  }

  public static class BindingUtils
  {
    // Helper function for binding.
    public static bool IsNonNull(object o)
    {
      if (o == null)
      {
        return false;
      }
      else
      {
        if ((o as StoreItemDetails).InCollection)
        {
          return false;
        }
        else
        {
          return true;
        }
      }

      //return o != null;
    }
  }

  class SharedClass
  {
    //private static ResourceLoader resourceLoader = new ResourceLoader();
    //private static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

    private static ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

    private static ApplicationDataContainer appSettings = ApplicationData.Current.LocalSettings;
    //private static ApplicationDataContainer appSettingsRoaming = ApplicationData.Current.RoamingSettings;

    private static ApplicationDataContainer containerLogin = appSettings.CreateContainer("Login", ApplicationDataCreateDisposition.Always);
    private static ApplicationDataContainer containerEvent = appSettings.CreateContainer("Event", ApplicationDataCreateDisposition.Always);
    private static ApplicationDataContainer containerTelegramBot = appSettings.CreateContainer("TelegramBot", ApplicationDataCreateDisposition.Always);

    public static string Main_ToastNotification_Background1 { get { return resourceLoader.GetString("Main_ToastNotification_Background1") ?? ""; } }
    public static string Main_ToastNotification_Background2 { get { return resourceLoader.GetString("Main_ToastNotification_Background2") ?? ""; } }
    public static string Main_ToastNotification_Background3 { get { return resourceLoader.GetString("Main_ToastNotification_Background3") ?? ""; } }

    public static string Main_LicenseText { get { return resourceLoader.GetString("Main_LicenseText") ?? ""; } }

    public static string DashboardOrderColumn
    {
      get
      {
        if (appSettings.Values["DashboardOrderColumn"] != null)
        {
          return (string)appSettings.Values["DashboardOrderColumn"];
        }
        else
        {
          return "group";
        }
      }

      set
      {
        appSettings.Values["DashboardOrderColumn"] = value;
      }
    }

    public static string DashboardOrderDirection
    {
      get
      {
        if (appSettings.Values["DashboardOrderDirection"] != null)
        {
          return (string)appSettings.Values["DashboardOrderDirection"];
        }
        else
        {
          return "";
        }

      }

      set
      {
        appSettings.Values["DashboardOrderDirection"] = value;
      }
    }

    public static DateTime StartDashboardMining
    {
      get
      {
        var year = appSettings.Values["StartDashboardMining_Year"];

        return new DateTime(
          Convert.ToInt32(appSettings.Values["StartDashboardMining_Year"]),
          Convert.ToInt32(appSettings.Values["StartDashboardMining_Month"]),
          Convert.ToInt32(appSettings.Values["StartDashboardMining_Day"]),
          Convert.ToInt32(appSettings.Values["StartDashboardMining_Hour"]),
          Convert.ToInt32(appSettings.Values["StartDashboardMining_Minute"]),
          Convert.ToInt32(appSettings.Values["StartDashboardMining_Second"]));
      }

      set
      {
        appSettings.Values["StartDashboardMining_Year"] = value.Year.ToString();
        appSettings.Values["StartDashboardMining_Month"] = value.Month.ToString();
        appSettings.Values["StartDashboardMining_Day"] = value.Day.ToString();
        appSettings.Values["StartDashboardMining_Hour"] = value.Hour.ToString();
        appSettings.Values["StartDashboardMining_Minute"] = value.Minute.ToString();
        appSettings.Values["StartDashboardMining_Second"] = value.Second.ToString();
      }
    }

    public static string Main_BottomAppBar_CheckLastNotPerformed { get { return resourceLoader.GetString("Main_BottomAppBar_CheckLastNotPerformed") ?? ""; } }

    public static string Time_Seconds { get { return resourceLoader.GetString("Time_Seconds") ?? ""; } }
    public static string Time_Minutes { get { return resourceLoader.GetString("Time_Minutes") ?? ""; } }
    public static string Time_Hours { get { return resourceLoader.GetString("Time_Hours") ?? ""; } }
    public static string Time_Days { get { return resourceLoader.GetString("Time_Days") ?? ""; } }

    public static int Main_Rigs_DelayCheck
    {
      get { if (appSettings.Values["Main_Rigs_DelayCheck"] != null) return Convert.ToInt32(appSettings.Values["Main_Rigs_DelayCheck"]); else return 1000; }
      set { appSettings.Values["Main_Rigs_DelayCheck"] = value; }
    }

    public static string Main_Rigs_Error_Parse { get { return resourceLoader.GetString("Main_Rigs_Error_Parse") ?? ""; } }
    public static string Main_Rigs_Error_Get { get { return resourceLoader.GetString("Main_Rigs_Error_Get") ?? ""; } }
    public static string Main_Rigs_Error_Check { get { return resourceLoader.GetString("Main_Rigs_Error_Check") ?? ""; } }

    public static string Dialog_Yes { get { return resourceLoader.GetString("Dialog_Yes") ?? ""; } }
    public static string Dialog_No { get { return resourceLoader.GetString("Dialog_No") ?? ""; } }

    public static string GetTimeSpanShow(TimeSpan during, EnumDuringShow DuringShow)
    {
      string ResultShow;

      string format_dhms = string.Format("d' {0} 'hh' {1} 'mm' {2} 'ss' {3}'", Time_Days, Time_Hours, Time_Minutes, Time_Seconds);
      string format_dhm = string.Format("d' {0} 'hh' {1} 'mm' {2}'", Time_Days, Time_Hours, Time_Minutes);
      string format_dh = string.Format("d' {0} 'hh' {1}'", Time_Days, Time_Hours);
      string format_d = string.Format("d' {0}'", Time_Days);
      string format_d0 = string.Format("0 {0}", Time_Days);

      string format_hms = string.Format("hh' {0} 'mm' {1} 'ss' {2}'", Time_Hours, Time_Minutes, Time_Seconds);
      string format_hm = string.Format("hh' {0} 'mm' {1}'", Time_Hours, Time_Minutes);
      string format_h = string.Format("hh' {0}'", Time_Hours);
      string format_h0 = string.Format("0 {0}", Time_Hours);

      string format_ms = string.Format("mm' {0} 'ss' {1}'", Time_Minutes, Time_Seconds);
      string format_m = string.Format("mm' {0}'", Time_Minutes);
      string format_m0 = string.Format("0 {0}", Time_Minutes);

      string format_s = string.Format("ss' {0}'", Time_Seconds);

      if (during.Days > 0)
      {
        switch (DuringShow)
        {
          #region EnumDuringShow.Seconds
          case EnumDuringShow.Seconds:
            if (during.Seconds > 0)
              ResultShow = during.ToString(format_dhms);
            else if (during.Minutes > 0)
              ResultShow = during.ToString(format_dhm);
            else if (during.Hours > 0)
              ResultShow = during.ToString(format_dh);
            else
              ResultShow = during.ToString(format_d);
            break;
          #endregion EnumDuringShow.Seconds

          #region EnumDuringShow.Minuts
          case EnumDuringShow.Minuts:
            if (during.Minutes > 0)
              ResultShow = during.ToString(format_dhm);
            else if (during.Hours > 0)
              ResultShow = during.ToString(format_dh);
            else
              ResultShow = during.ToString(format_d);
            break;
          #endregion EnumDuringShow.Minuts

          #region EnumDuringShow.Hours
          case EnumDuringShow.Hours:
            if (during.Hours > 0)
              ResultShow = during.ToString(format_dh);
            else
              ResultShow = during.ToString(format_d);
            break;
          #endregion EnumDuringShow.Hours

          #region EnumDuringShow.Days:
          case EnumDuringShow.Days:
            ResultShow = during.ToString(format_d);
            break;
          #endregion EnumDuringShow.Days:

          #region Default
          default:
            ResultShow = during.ToString(format_dhms);
            break;
            #endregion Default
        }
      }
      else if (during.Hours > 0)
      {
        switch (DuringShow)
        {
          #region EnumDuringShow.Seconds
          case EnumDuringShow.Seconds:
            if (during.Seconds > 0)
              ResultShow = during.ToString(format_hms);
            else if (during.Minutes > 0)
              ResultShow = during.ToString(format_hm);
            else if (during.Hours > 0)
              ResultShow = during.ToString(format_h);
            else
              ResultShow = during.ToString(format_hms);
            break;
          #endregion EnumDuringShow.Seconds

          #region EnumDuringShow.Minuts
          case EnumDuringShow.Minuts:
            if (during.Minutes > 0)
              ResultShow = during.ToString(format_hm);
            else if (during.Hours > 0)
              ResultShow = during.ToString(format_h);
            else
              ResultShow = during.ToString(format_hm);
            break;
          #endregion EnumDuringShow.Minuts

          #region EnumDuringShow.Hours
          case EnumDuringShow.Hours:
            ResultShow = during.ToString(format_h);
            break;
          #endregion EnumDuringShow.Hours

          #region EnumDuringShow.Days
          case EnumDuringShow.Days:
            ResultShow = "format_d0";
            break;
          #endregion EnumDuringShow.Days

          #region Default
          default:
            ResultShow = during.ToString(format_hms);
            break;
            #endregion Default
        }
      }
      else if (during.Minutes > 0)
      {
        switch (DuringShow)
        {
          case EnumDuringShow.Seconds:
            if (during.Seconds > 0)
              ResultShow = during.ToString(format_ms);
            else if (during.Minutes > 0)
              ResultShow = during.ToString(format_m);
            else
              ResultShow = during.ToString(format_ms);
            break;
          case EnumDuringShow.Minuts:
            ResultShow = during.ToString(format_m);
            break;
          case EnumDuringShow.Hours:
            ResultShow = format_h0;
            break;
          case EnumDuringShow.Days:
            ResultShow = format_d0;
            break;
          default:
            ResultShow = during.ToString(format_ms);
            break;
        }
      }
      else
        switch (DuringShow)
        {
          case EnumDuringShow.Seconds:
            if (during.Seconds > 0) ResultShow = during.ToString(format_s); else ResultShow = "";
            break;
          case EnumDuringShow.Minuts:
            ResultShow = format_m0;
            break;
          case EnumDuringShow.Hours:
            ResultShow = format_h0;
            break;
          case EnumDuringShow.Days:
            ResultShow = format_d0;
            break;
          default:
            ResultShow = during.ToString(format_s);
            break;
        }

      return ResultShow;
    }

    #region Telegram
    public static string TelegramBot_AddUser { get { return resourceLoader.GetString("TelegramBot_AddUser") ?? ""; } }
    public static string TelegramBot_DeleteUser { get { return resourceLoader.GetString("TelegramBot_DeleteUser") ?? ""; } }

    public static bool IsTelegramBot
    {
      get { if (appSettings.Values["IsTelegramBot"] != null) return (bool)appSettings.Values["IsTelegramBot"]; else return false; }
      set { appSettings.Values["IsTelegramBot"] = value; }
    }

    public static string TelegramBotDashboardName
    {
      get { if (appSettings.Values["TelegramBotDashboardName"] != null) return (string)appSettings.Values["TelegramBotDashboardName"]; else return ""; }
      set { appSettings.Values["TelegramBotDashboardName"] = value; }
    }

    public static string TelegramBotToken
    {
      get { if (appSettings.Values["TelegramBotToken"] != null) return (string)appSettings.Values["TelegramBotToken"]; else return ""; }
      set { appSettings.Values["TelegramBotToken"] = value; }
    }

    public static string TelegramBotPassword
    {
      get { if (appSettings.Values["TelegramBotPassword"] != null) return (string)appSettings.Values["TelegramBotPassword"]; else return ""; }
      set { appSettings.Values["TelegramBotPassword"] = value; }
    }

    public static string TelegramBotAllow
    {
      get { if (appSettings.Values["TelegramBotAllow"] != null) return (string)appSettings.Values["TelegramBotAllow"]; else return ""; }
      set { appSettings.Values["TelegramBotAllow"] = value; }
    }

    public static int TelegramBotUpdateId
    {
      get { if (appSettings.Values["TelegramBotUpdateId"] != null) return (int)appSettings.Values["TelegramBotUpdateId"]; else return 0; }
      set { appSettings.Values["TelegramBotUpdateId"] = value; }
    }

    public static List<TelegramBotObject> ListTelegramBot
    {
      get
      {
        // Для отладки - удаления контейнера для теста
        //appSettings.DeleteContainer("TelegramBot");
        //containerTelegramBot = appSettings.CreateContainer("TelegramBot", ApplicationDataCreateDisposition.Always);

        CultureInfo provider = CultureInfo.InvariantCulture;

        var _ListTelegramBot = new List<TelegramBotObject>();

        foreach (var item in containerTelegramBot.Values)
        {
          var itemEvent = item.Value as ApplicationDataCompositeValue;

          _ListTelegramBot.Add(new TelegramBotObject
          {
            ChatId= Convert.ToInt32(itemEvent["ChatId"]),
            Firstname= (string)itemEvent["Firstname"],
            Lastname = (string)itemEvent["Lastname"],
            Username = (string)itemEvent["Username"],
            Language = (string)itemEvent["Language"],
            PhoneNumber = (string)itemEvent["PhoneNumber"]
          });
        }

        return _ListTelegramBot;
      }
    }

    public static void TelegramBotSave(string PhoneNumber, int ChatId = 0, string Firstname = "", string Lastname = "", string Username = "", string Language = "")
    {
      foreach (var item in containerTelegramBot.Values)
      {
        var itemTelegramBot = item.Value as ApplicationDataCompositeValue;

        if ((string)itemTelegramBot["PhoneNumber"] == PhoneNumber)
        {
          containerTelegramBot.Values.Remove(item.Key);
        }
      }

      var composite = new ApplicationDataCompositeValue
      {
        ["ChatId"] = ChatId,
        ["Firstname"] = Firstname,
        ["Lastname"] = Lastname,
        ["Username"] = Username,
        ["Language"] = Language,
        ["PhoneNumber"] = PhoneNumber
      };

      containerTelegramBot.Values.Add("TelegramBotItem" + PhoneNumber, composite);
    }

    public static void TelegramBotRemove(string PhoneNumber)
    {
      containerTelegramBot.Values.Remove("TelegramBotItem" + PhoneNumber);
    }

    //public static bool IsTelegramBotReady
    //{
    //  get { return IsTelegramBot && !string.IsNullOrEmpty(TelegramBotDashboardName) && !string.IsNullOrEmpty(TelegramBotToken) && !string.IsNullOrEmpty(TelegramBotAllow); }
    //}

    #endregion Telegram

    #region Problem

    public static string GetFarmProblem(EnumFarmProblem enumFarmProblem)
    {
      string FarmProblem;

      switch (enumFarmProblem)
      {
        case EnumFarmProblem.FarmNotAvailable:
          FarmProblem = resourceLoader.GetString("Problem_FarmNotAvailable") ?? "Ферма недоступна";
          break;
        case EnumFarmProblem.TheVideocardDoesNotWorkFor:
          FarmProblem = resourceLoader.GetString("Problem_NotAllVideocardsWork") ?? "Работают не все видеокарты";
          break;
        case EnumFarmProblem.NoVideocardSpeedInfo:
          FarmProblem = resourceLoader.GetString("Problem_NoVideocardSpeedInfo") ?? "Нет данных о скорости видеокарт";
          break;
        case EnumFarmProblem.TheVideocardIsRunningSlowly:
          FarmProblem = resourceLoader.GetString("Problem_VideocardSlow") ?? "Видеокарта работает медленно";
          break;
        case EnumFarmProblem.NoVideocardTemperatureInformation:
          FarmProblem = resourceLoader.GetString("Problem_NoVideocardTemperatureInformation") ?? "Нет данных о температуре видеокарт";
          break;
        case EnumFarmProblem.VideocardTemperatureLow:
          FarmProblem = resourceLoader.GetString("Problem_VideocardTemperatureLow") ?? "Температура видеокарты низкая";
          break;
        case EnumFarmProblem.VideocardTemperatureHigh:
          FarmProblem = resourceLoader.GetString("Problem_VideocardTemperatureHigh") ?? "Температура видеокарты высокая";
          break;
        case EnumFarmProblem.NoFanDataOnVideocards:
          FarmProblem = resourceLoader.GetString("Problem_NoFanDataOnVideocards") ?? "Нет данных о вентиляторах на видеокартах";
          break;
        case EnumFarmProblem.TheFanOnVideocardIsWeak:
          FarmProblem = resourceLoader.GetString("Problem_FanOnVideocardIsWeak") ?? "Вентилятор видеокарты крутится слабо";
          break;
        case EnumFarmProblem.TheFanOnVideocardSpinsHard:
          FarmProblem = resourceLoader.GetString("Problem_FanOnVideocardSpinsHard") ?? "Вентилятор видеокарты крутится сильно";
          break;
        default:
          FarmProblem = "";
          break;
      }
      return FarmProblem;
    }
    #endregion Problem

    #region Action

    //private static string ActionListTakeNoAction => resourceLoader.GetString("ActionListTakeNoAction") ?? "Нет действия";
    //private static string ActionListNotification => resourceLoader.GetString("ActionListNotification") ?? "Уведомление";
    //private static string ActionListRebootTheRig => resourceLoader.GetString("ActionListRebootTheRig") ?? "Перезагрузка фермы";

    //private static string ActionListTakeNoAction => "Нет действия";
    //private static string ActionListNotification => "Уведомление";
    //private static string ActionListRebootTheRig => "Перезагрузка фермы";

    public static string GetFarmAction(EnumFarmAction enumFarmAction, int CountRestart=0)
    {
      string FarmAction;

      switch (enumFarmAction)
      {
        case EnumFarmAction.TakeNoAction:
          FarmAction = resourceLoader.GetString("FarmAction_TakeNoAction") ?? "Нет действия";
          break;
        case EnumFarmAction.Notification:
          FarmAction = resourceLoader.GetString("FarmAction_Notification") ?? "Уведомление";
          break;
        case EnumFarmAction.RebootTheRig:
          if (CountRestart == 0)
          {
            FarmAction = resourceLoader.GetString("FarmAction_RebootTheRig") ?? "Перезагрузка";
          }
          else
          {
            FarmAction = (resourceLoader.GetString("FarmAction_RebootTheRig") ?? "Перезагрузка") + " (" + CountRestart.ToString() + ")";
          }          
          break;
        default:
          FarmAction = "";
          break;
      }
      return FarmAction;
    }

    public static EnumFarmAction ActionRigDuring
    {
      get
      {
        if (appSettings.Values["ActionRigDuring"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionRigDuring"]);
        }
        else
        {
          return EnumFarmAction.Notification;
        }
      }

      set
      {
        appSettings.Values["ActionRigDuring"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionTheVideocardDoesNotWorkFor
    {
      get
      {
        if (appSettings.Values["ActionTheVideocardDoesNotWorkFor"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionTheVideocardDoesNotWorkFor"]);
        }
        else
        {
          return EnumFarmAction.Notification;
        }
      }

      set
      {
        appSettings.Values["ActionTheVideocardDoesNotWorkFor"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionNoVideocardSpeedInfo
    {
      get
      {
        if (appSettings.Values["ActionNoVideocardSpeedInfo"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionNoVideocardSpeedInfo"]);
        }
        else
        {
          return EnumFarmAction.TakeNoAction;
        }
      }

      set
      {
        appSettings.Values["ActionNoVideocardSpeedInfo"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionTheVideocardIsRunningSlowly
    {
      get
      {
        if (appSettings.Values["ActionTheVideocardIsRunningSlowly"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionTheVideocardIsRunningSlowly"]);
        }
        else
        {
          return EnumFarmAction.RebootTheRig;
        }
      }

      set
      {
        appSettings.Values["ActionTheVideocardIsRunningSlowly"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionNoVideocardTemperatureInformation
    {
      get
      {
        if (appSettings.Values["ActionNoVideocardTemperatureInformation"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionNoVideocardTemperatureInformation"]);
        }
        else
        {
          return EnumFarmAction.TakeNoAction;
        }
      }

      set
      {
        appSettings.Values["ActionNoVideocardTemperatureInformation"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionVideocardTemperatureLow
    {
      get
      {
        if (appSettings.Values["ActionVideocardTemperatureLow"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionVideocardTemperatureLow"]);
        }
        else
        {
          return EnumFarmAction.TakeNoAction;
        }
      }

      set
      {
        appSettings.Values["ActionVideocardTemperatureLow"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionVideocardTemperatureHigh
    {
      get
      {
        if (appSettings.Values["ActionVideocardTemperatureHigh"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionVideocardTemperatureHigh"]);
        }
        else
        {
          return EnumFarmAction.Notification;
        }
      }

      set
      {
        appSettings.Values["ActionVideocardTemperatureHigh"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionNoFanDataOnVideocards
    {
      get
      {
        if (appSettings.Values["ActionNoFanDataOnVideocards"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionNoFanDataOnVideocards"]);
        }
        else
        {
          return EnumFarmAction.TakeNoAction;
        }
      }

      set
      {
        appSettings.Values["ActionNoFanDataOnVideocards"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionTheFanOnVideocardIsWeak
    {
      get
      {
        if (appSettings.Values["ActionTheFanOnVideocardIsWeak"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionTheFanOnVideocardIsWeak"]);
        }
        else
        {
          return EnumFarmAction.TakeNoAction;
        }
      }

      set
      {
        appSettings.Values["ActionTheFanOnVideocardIsWeak"] = Convert.ToInt32(value);
      }
    }

    public static EnumFarmAction ActionTheFanOnVideocardSpinsHard
    {
      get
      {
        if (appSettings.Values["ActionTheFanOnVideocardSpinsHard"] != null)
        {
          return (EnumFarmAction)Convert.ToInt32(appSettings.Values["ActionTheFanOnVideocardSpinsHard"]);
        }
        else
        {
          return EnumFarmAction.Notification;
        }
      }

      set
      {
        appSettings.Values["ActionTheFanOnVideocardSpinsHard"] = Convert.ToInt32(value);
      }
    }
    #endregion Action

    #region Settings
    public static int SettingsCheckDelay
    {
      get {if (appSettings.Values["SettingsCheckDelay"] != null) return (int)appSettings.Values["SettingsCheckDelay"]; else return 60;}
      set {appSettings.Values["SettingsCheckDelay"] = value;}
    }

    public static int SettingsCheckRestart
    {
      get { if (appSettings.Values["SettingsCheckReboot"] != null) return (int)appSettings.Values["SettingsCheckReboot"]; else return 10; }
      set { appSettings.Values["SettingsCheckReboot"] = value; }
    }

    public static int SettingsCheckNotification
    {
      get { if (appSettings.Values["SettingsCheckNotification"] != null) return (int)appSettings.Values["SettingsCheckNotification"]; else return 24; }
      set { appSettings.Values["SettingsCheckNotification"] = value; }
    }

    public static int SettingsCountRestart
    {
      get { if (appSettings.Values["SettingsCountRestart"] != null) return (int)appSettings.Values["SettingsCountRestart"]; else return 3; }
      set { appSettings.Values["SettingsCountRestart"] = value; }
    }

    public static int SettingsCountResetRestart
    {
      get { if (appSettings.Values["SettingsCountResetRestart"] != null) return (int)appSettings.Values["SettingsCountResetRestart"]; else return 12; }
      set { appSettings.Values["SettingsCountResetRestart"] = value; }
    }

    public static int SettingsRigDuring
    {
      get { if (appSettings.Values["SettingsRigDuring"] != null) return (int)appSettings.Values["SettingsRigDuring"]; else return 10; }
      set { appSettings.Values["SettingsRigDuring"] = value; }
    }

    public static int SettingsTheVideocardDoesNotWorkDuring
    {
      get { if (appSettings.Values["SettingsTheVideocardDoesNotWorkDuring"] != null) return (int)appSettings.Values["SettingsTheVideocardDoesNotWorkDuring"]; else return 10; }
      set { appSettings.Values["SettingsTheVideocardDoesNotWorkDuring"] = value; }
    }

    public static int SettingsSpeedSlow
    {
      get { if (appSettings.Values["SettingsSpeed"] != null) return (int)appSettings.Values["SettingsSpeed"]; else return 15; }
      set { appSettings.Values["SettingsSpeed"] = value; }
    }

    public static int SettingsSpeedSlowDuring
    {
      get { if (appSettings.Values["SettingsSpeedSlowDuring"] != null) return (int)appSettings.Values["SettingsSpeedSlowDuring"]; else return 5; }
      set { appSettings.Values["SettingsSpeedSlowDuring"] = value; }
    }

    public static int SettingsTempMin
    {
      get { if (appSettings.Values["SettingsTempMin"] != null) return (int)appSettings.Values["SettingsTempMin"]; else return 15; }
      set { appSettings.Values["SettingsTempMin"] = value; }
    }

    public static int SettingsTempMinDuring
    {
      get { if (appSettings.Values["SettingsTempMinDuring"] != null) return (int)appSettings.Values["SettingsTempMinDuring"]; else return 20; }
      set { appSettings.Values["SettingsTempMinDuring"] = value; }
    }

    public static int SettingsTempMax
    {
      get { if (appSettings.Values["SettingsTempMax"] != null) return (int)appSettings.Values["SettingsTempMax"]; else return 80; }
      set { appSettings.Values["SettingsTempMax"] = value; }
    }

    public static int SettingsTempMaxDuring
    {
      get { if (appSettings.Values["SettingsTempMaxDuring"] != null) return (int)appSettings.Values["SettingsTempMaxDuring"]; else return 10; }
      set { appSettings.Values["SettingsTempMaxDuring"] = value; }
    }

    public static int SettingsFanMin
    {
      get { if (appSettings.Values["SettingsFanMin"] != null) return (int)appSettings.Values["SettingsFanMin"]; else return 70; }
      set { appSettings.Values["SettingsFanMin"] = value; }
    }

    public static int SettingsFanMinDuring
    {
      get { if (appSettings.Values["SettingsFanMinDuring"] != null) return (int)appSettings.Values["SettingsFanMinDuring"]; else return 15; }
      set { appSettings.Values["SettingsFanMinDuring"] = value; }
    }

    public static int SettingsFanMax
    {
      get { if (appSettings.Values["SettingsFanMax"] != null) return (int)appSettings.Values["SettingsFanMax"]; else return 90; }
      set { appSettings.Values["SettingsFanMax"] = value; }
    }

    public static int SettingsFanMaxDuring
    {
      get { if (appSettings.Values["SettingsFanMaxDuring"] != null) return (int)appSettings.Values["SettingsFanMaxDuring"]; else return 10; }
      set { appSettings.Values["SettingsFanMaxDuring"] = value; }
    }

    public static List<FarmActionObject> ListFarmAction
    {
      get
      {
        var _ListFarmAction = new List<FarmActionObject>();

        //_ListFarmAction.Insert(0, new FarmActionObject { enumFarmAction = EnumFarmAction.TakeNoAction });
        //_ListFarmAction.Insert(0, new FarmActionObject { enumFarmAction = EnumFarmAction.Notification });
        //_ListFarmAction.Insert(0, new FarmActionObject { enumFarmAction = EnumFarmAction.RebootTheRig });

        _ListFarmAction.Add(new FarmActionObject { enumFarmAction = EnumFarmAction.TakeNoAction });
        _ListFarmAction.Add(new FarmActionObject { enumFarmAction = EnumFarmAction.Notification });
        _ListFarmAction.Add(new FarmActionObject { enumFarmAction = EnumFarmAction.RebootTheRig });

        return _ListFarmAction;
      }
    }

    #endregion Settings

    public static List<MiningObject> ListMining
    {
      get
      {
        var listMining = new List<MiningObject>();

        listMining.Add(new MiningObject
        {
          Mining = EnumMining.SimpleMiningNet,
          Title = "SimpleMining.net",
          URL = "https://simplemining.net"
        });

        return listMining;
      }
    }

    #region Notification
    public static string Notification_Format1 { get { return resourceLoader.GetString("Notification_Format1") ?? ""; } }
    public static string Notification_Format2 { get { return resourceLoader.GetString("Notification_Format2") ?? ""; } }
    public static string Notification_Format3 { get { return resourceLoader.GetString("Notification_Format3") ?? ""; } }
    public static string Notification_Format4 { get { return resourceLoader.GetString("Notification_Format4") ?? ""; } }
    public static string Notification_Format5 { get { return resourceLoader.GetString("Notification_Format5") ?? ""; } }

    #endregion Notification

    #region Login
    public static string Error_Captcha { get { return resourceLoader.GetString("Error_Captcha") ?? ""; } }
    public static string Error_Login { get { return resourceLoader.GetString("Error_Login") ?? ""; } }
    public static string Error_Logout { get { return resourceLoader.GetString("Error_Logout") ?? ""; } }

    public static List<LoginMining> ListLoginMining
    {
      get
      {
        bool _IsLogged;
        // Для отладки - удаления контейнера для теста
        //appSettings.DeleteContainer("Login");
        //loginContainer = appSettings.CreateContainer("Login", ApplicationDataCreateDisposition.Always);

        var listLoginMining = new List<LoginMining>();

        if (containerLogin.Values.Count == 2)
        {
          foreach (var item in containerLogin.Values)
          {
            var loginItem = item.Value as ApplicationDataCompositeValue;

            bool.TryParse((string)loginItem["IsLogged"], out _IsLogged);

            listLoginMining.Insert(0, new LoginMining
            {
              Id = (string)loginItem["Id"],
              Login = (string)loginItem["Login"],
              Password = (string)loginItem["Password"],
              Mining = (EnumMining)loginItem["Mining"],
              SimpleMining_cfduid = (string)loginItem["SimpleMining_cfduid"],
              SimpleMining_cflb = (string)loginItem["SimpleMining_cflb"],
              SimpleMining_PHPSESSID = (string)loginItem["SimpleMining_PHPSESSID"],
              SimpleMining_Captcha = (string)loginItem["SimpleMining_Captcha"],
              //IsLogged=(!string.IsNullOrEmpty((string)loginItem["SimpleMining_cfduid"]) && !string.IsNullOrEmpty((string)loginItem["SimpleMining_PHPSESSID"]) && !string.IsNullOrEmpty((string)loginItem["SimpleMining_cflb"]) && !string.IsNullOrEmpty((string)loginItem["SimpleMining_Captcha"]))
              //IsLogged=(!string.IsNullOrEmpty((string)loginItem["SimpleMining_cfduid"]) && !string.IsNullOrEmpty((string)loginItem["SimpleMining_PHPSESSID"]) && !string.IsNullOrEmpty((string)loginItem["SimpleMining_cflb"]))
              IsLogged = _IsLogged
            });
          }
        }
        else
        {
          appSettings.DeleteContainer("Login");
          containerLogin = appSettings.CreateContainer("Login", ApplicationDataCreateDisposition.Always);

          SaveLogin("0", EnumMining.SimpleMiningNet, "", "", "", "", "", "", false);
          SaveLogin("1", EnumMining.SimpleMiningNet, "", "", "", "", "", "", false);

          listLoginMining = ListLoginMining;
        }

        return listLoginMining;
      }
    }

    public static void SaveLoginCookie(string LoginId, EnumMining Mining, string SimpleMining_cfduid, string SimpleMining_cflb, string SimpleMining_PHPSESSID)
    {
      var foundLogin = containerLogin.Values.FirstOrDefault(item => (string)(item.Value as ApplicationDataCompositeValue)["Id"] == LoginId);

      var loginItem = foundLogin.Value as ApplicationDataCompositeValue;
      if (loginItem == null)
      {
        var composite = new ApplicationDataCompositeValue
        {
          ["Id"] = LoginId,
          ["Mining"] = (int)Mining,
          ["SimpleMining_Captcha"] = "",
          ["SimpleMining_cfduid"] = SimpleMining_cfduid,
          ["SimpleMining_cflb"] = SimpleMining_cflb,
          ["SimpleMining_PHPSESSID"] = SimpleMining_PHPSESSID
        };

        containerLogin.Values.Add("LoginItem" + LoginId, composite);
      }
      else
      {
        loginItem["Mining"] = (int)Mining;
        loginItem["SimpleMining_Captcha"] = "";
        loginItem["SimpleMining_cfduid"] = SimpleMining_cfduid;
        loginItem["SimpleMining_cflb"] = SimpleMining_cflb;
        loginItem["SimpleMining_PHPSESSID"] = SimpleMining_PHPSESSID;

        containerLogin.Values["LoginItem" + LoginId] = loginItem;
      }
    }

    public static void SaveLogin(string LoginId, EnumMining Mining, string Login, string Password, string SimpleMining_Captcha)
    {
      var foundLogin = containerLogin.Values.FirstOrDefault(item => (string)(item.Value as ApplicationDataCompositeValue)["Id"] == LoginId);

      var loginItem = foundLogin.Value as ApplicationDataCompositeValue;
      if (loginItem == null)
      {
        var composite = new ApplicationDataCompositeValue
        {
          ["Id"] = LoginId,
          ["Mining"] = (int)Mining,
          ["Login"] = Login,
          ["Password"] = Password,
          ["SimpleMining_Captcha"] = SimpleMining_Captcha,
          ["IsLogged"] = bool.TrueString
        };

        containerLogin.Values.Add("LoginItem" + LoginId, composite);
      }
      else
      {
        loginItem["Mining"] = (int)Mining;
        loginItem["Login"] = Login;
        loginItem["Password"] = Password;
        loginItem["SimpleMining_Captcha"] = SimpleMining_Captcha;
        loginItem["IsLogged"] = bool.TrueString;

        containerLogin.Values["LoginItem" + LoginId] = loginItem;
      }
    }

    public static void SaveLogin(string LoginId, EnumMining Mining, string Login, string Password, string SimpleMining_cfduid, string SimpleMining_cflb, string SimpleMining_PHPSESSID, string SimpleMining_Captcha, bool IsLogged)
    {
      var foundLogin = containerLogin.Values.FirstOrDefault(item => (string)(item.Value as ApplicationDataCompositeValue)["Id"] == LoginId);

      var loginItem = foundLogin.Value as ApplicationDataCompositeValue;
      if (loginItem == null)
      {
        var composite = new ApplicationDataCompositeValue
        {
          ["Id"] = LoginId,
          ["Mining"] = (int)Mining,
          ["Login"] = Login,
          ["Password"] = Password,
          ["SimpleMining_Captcha"] = SimpleMining_Captcha,
          ["SimpleMining_cfduid"] = SimpleMining_cfduid,
          ["SimpleMining_cflb"] = SimpleMining_cflb,
          ["SimpleMining_PHPSESSID"] = SimpleMining_PHPSESSID,
          ["IsLogged"] = IsLogged.ToString()
        };

        containerLogin.Values.Add("LoginItem" + LoginId, composite);
      }
      else
      {
        loginItem["Mining"] = (int)Mining;
        loginItem["Login"] = Login;
        loginItem["Password"] = Password;
        loginItem["SimpleMining_Captcha"] = SimpleMining_Captcha;
        loginItem["SimpleMining_cfduid"] = SimpleMining_cfduid;
        loginItem["SimpleMining_cflb"] = SimpleMining_cflb;
        loginItem["SimpleMining_PHPSESSID"] = SimpleMining_PHPSESSID;
        loginItem["IsLogged"] = IsLogged.ToString();

        containerLogin.Values["LoginItem" + LoginId] = loginItem;
      }
    }

    public static void SaveLogout(string LoginId)
    {
      var foundLogin = containerLogin.Values.FirstOrDefault(item => (string)(item.Value as ApplicationDataCompositeValue)["Id"] == LoginId);

      var loginItem = foundLogin.Value as ApplicationDataCompositeValue;
      if (loginItem != null)
      {
        loginItem["SimpleMining_Captcha"] = "";
        loginItem["SimpleMining_cfduid"] = "";
        loginItem["SimpleMining_cflb"] = "";
        loginItem["SimpleMining_PHPSESSID"] = "";
        loginItem["IsLogged"] = bool.FalseString;

        containerLogin.Values["LoginItem" + LoginId] = loginItem;
      }
    }

    #endregion Login

    private static string GetTimeSpanFormat()
    {
      return "c";
    }

    #region Event
    public static string Event_LogCopy { get { return resourceLoader.GetString("Event_LogCopy") ?? ""; } }
    public static string Event_LogDelete { get { return resourceLoader.GetString("Event_LogDelete") ?? ""; } }
    public static string Event_LogClear1 { get { return resourceLoader.GetString("Event_LogClear1") ?? ""; } }
    public static string Event_LogClear2 { get { return resourceLoader.GetString("Event_LogClear2") ?? ""; } }

    private static string GetEventIdFormat()
    {
      return "yyyyMMddHHmmssfffffff";
    }

    private static string GetEventId(DateTime dateActionId)
    {
      return dateActionId.ToString(GetEventIdFormat());
    }

    private static string GetTimeSpanString(TimeSpan During)
    {
      return During.ToString(GetTimeSpanFormat());
    }

    public static List<EventObject> ListEvent
    {
      get
      {
        // Для отладки - удаления контейнера для теста
        //appSettings.DeleteContainer("Event");
        //containerEvent = appSettings.CreateContainer("Event", ApplicationDataCreateDisposition.Always);

        CultureInfo provider = CultureInfo.InvariantCulture;

        var listEvent = new List<EventObject>();

        foreach (var item in containerEvent.Values)
        {
          var itemEvent = item.Value as ApplicationDataCompositeValue;

          listEvent.Add(new EventObject
          {
            datetime = DateTime.ParseExact((string)itemEvent["datetime"], GetEventIdFormat(), provider),
            id = (string)itemEvent["id"],
            group = (string)itemEvent["group"],
            name = (string)itemEvent["name"],
            FarmProblem = (EnumFarmProblem)(Convert.ToInt32(itemEvent["FarmProblem"])),
            during = TimeSpan.ParseExact((string)itemEvent["during"], GetTimeSpanFormat(), provider),
            FarmAction = (EnumFarmAction)(Convert.ToInt32(itemEvent["FarmAction"])),
            ip = (string)itemEvent["ip"],
            details = (string)itemEvent["details"],
            CountRestart = Convert.ToInt32(itemEvent["CountRestart"])
          });
        }

        return listEvent;
      }
    }

    public static void EventSave(string id, string group, string name, EnumFarmProblem FarmProblem, TimeSpan during, EnumFarmAction FarmAction, string ip, string details, int CountRestart=0)
    {
      var dateEventId = GetEventId(DateTime.Now);

      var itemEvent = new ApplicationDataCompositeValue
      {
        ["datetime"] = dateEventId,
        ["id"] = id,
        ["group"] = group,
        ["name"] = name,
        ["FarmProblem"] = Convert.ToInt32(FarmProblem),
        ["during"] = GetTimeSpanString(during),
        ["FarmAction"] = Convert.ToInt32(FarmAction),
        ["ip"] = ip,
        ["details"] = details,
        ["CountRestart"] = CountRestart
      };

      containerEvent.Values.Add("EventItem" + dateEventId, itemEvent);
    }

    public static void EventRemove(DateTime datetime)
    {
      containerEvent.Values.Remove("EventItem" + GetEventId(datetime));
    }

    public static void EventClear()
    {
      containerEvent.Values.Clear();
    }
    #endregion Event

    #region Error
    public static string Error_LogClear { get { return resourceLoader.GetString("Error_LogClear") ?? ""; } }
    #endregion Error

    #region About
    public static string ApplicationDisplayName
    {
      get
      {
        var package = Windows.ApplicationModel.Package.Current;
        return $"{package.DisplayName}";
      }
    }

    public static string ApplicationFullVersion
    {
      get
      {
        var package = Windows.ApplicationModel.Package.Current;
        var packageId = package.Id;
        var version = packageId.Version;
        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
      }
    }
    #endregion About

    #region Test for Microsoft Store
    //public static string TestData = '[{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5914848\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5914848\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"03\\\" data-id=\\\"5914848\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5914848<br \\/> IP: 192.168.0.8<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">03<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":31,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 1 day, 2 hours, 10 minutes<br \\/>Miner program started: 2018-02-11 05:56:33<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:04:54 <br \\/> Last seen: 31 seconds ago<\\/b><br \\/>Total restarts: 112 \\\"> ON(112)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 13.69 MH\\/s<br \\/>GPU1: 20.92 MH\\/s<br \\/>GPU2: 23.52 MH\\/s<br \\/>GPU3: 21.66 MH\\/s<br \\/>GPU4: 23.53 MH\\/s<br \\/>GPU5: 22.89 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">126.22 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"43 52 55 47 50 52 ?<br \\/>70 70 70 70 70 70  %\\\">55?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1506 1468 1493 1480 1468 1455  (75)<br \\/>4498 4498 4498 4498 4498 4498 <\\/small>\",\"id\":\"5914848\",\"group\":\"TestGroup\",\"console\":\"ETH: 02\\/12\\/18-08:04:51 - SHARE FOUND - (GPU 0)<br \\/>\\nETH: Share accepted (54 ms)!\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5914848' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5914848'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5914848' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5914848' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5914848' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5914848' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"},{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5917915\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5917915\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"04\\\" data-id=\\\"5917915\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5917915<br \\/> IP: 192.168.0.94<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">04<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":2,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 1 day, 21 hours, 11 minutes<br \\/>Miner program started: 2018-02-10 10:54:46<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:05:23 <br \\/> Last seen: 2 seconds ago<\\/b><br \\/>Total restarts: 90 \\\"> ON(90)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 23.63 MH\\/s<br \\/>GPU1: 23.70 MH\\/s<br \\/>GPU2: 23.70 MH\\/s<br \\/>GPU3: 22.51 MH\\/s<br \\/>GPU4: 22.73 MH\\/s<br \\/>GPU5: 23.73 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">140.01 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"60 46 59 56 52 49 ?<br \\/>70 70 70 70 70 70  %\\\">60?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1480 1493 1493 1506 1417 1442  (75)<br \\/>4498 4498 4498 4498 4498 4498 <\\/small>\",\"id\":\"5917915\",\"group\":\"TestGroup\",\"console\":\"ETH: GPU0 23.688 Mh\\/s, GPU1 23.588 Mh\\/s, GPU2 23.476 Mh\\/s, GPU3 23.648 Mh\\/s, GPU4 23.707 Mh\\/s, GPU5 23.688 Mh\\/s<br \\/>\\nGPU0 t=59C fan=70%, GPU1 t=46C fan=70%, GPU2 t=58C fan=70%, GPU3 t=56C fan=70%, GPU4 t=52C fan=70%, GPU5 t=49C fan=70%\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5917915' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5917915'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5917915' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5917915' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5917915' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5917915' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"},{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5921813\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5921813\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"07\\\" data-id=\\\"5921813\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5921813<br \\/> IP: 192.168.0.29<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">07<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":11,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 5 hours, 11 minutes<br \\/>Miner program started: 2018-02-12 02:55:31<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:05:14 <br \\/> Last seen: 11 seconds ago<\\/b><br \\/>Total restarts: 293 \\\"> ON(293)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 23.67 MH\\/s<br \\/>GPU1: 23.63 MH\\/s<br \\/>GPU2: 23.13 MH\\/s<br \\/>GPU3: 23.63 MH\\/s<br \\/>GPU4: 23.62 MH\\/s<br \\/>GPU5: 22.34 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">140.02 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"39 58 41 46 49 48 ?<br \\/>70 70 70 70 70 70  %\\\">58?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1506 1493 1518 1506 1531 1506  (75)<br \\/>4498 4498 4498 4498 4498 4498 <\\/small>\",\"id\":\"5921813\",\"group\":\"TestGroup\",\"console\":\"ETH - Total Speed: 142.160 Mh\\/s, Total Shares: 282, Rejected: 0, Time: 05:08<br \\/>\\nETH: GPU0 23.691 Mh\\/s, GPU1 23.697 Mh\\/s, GPU2 23.675 Mh\\/s, GPU3 23.652 Mh\\/s, GPU4 23.734 Mh\\/s, GPU5 23.711 Mh\\/s\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5921813' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5921813'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5921813' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5921813' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5921813' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5921813' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"},{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5926277\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5926277\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"06\\\" data-id=\\\"5926277\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5926277<br \\/> IP: 192.168.0.17<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">06<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":20,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 1 day, 20 hours, 32 minutes<br \\/>Miner program started: 2018-02-10 11:34:34<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:05:05 <br \\/> Last seen: 20 seconds ago<\\/b><br \\/>Total restarts: 205 \\\"> ON(205)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 18.66 MH\\/s<br \\/>GPU1: 23.58 MH\\/s<br \\/>GPU2: 22.71 MH\\/s<br \\/>GPU3: 23.65 MH\\/s<br \\/>GPU4: 23.66 MH\\/s<br \\/>GPU5: 23.71 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">135.98 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"45 49 40 51 54 54 ?<br \\/>70 70 70 70 70 70  %\\\">54?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1480 1442 1392 1506 1480 1506  (75)<br \\/>4498 4498 4498 4498 4498 4498 <\\/small>\",\"id\":\"5926277\",\"group\":\"TestGroup\",\"console\":\"ETH: 02\\/12\\/18-08:04:58 - SHARE FOUND - (GPU 0)<br \\/>\\nETH: Share accepted (63 ms)!\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5926277' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5926277'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5926277' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5926277' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5926277' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5926277' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"},{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5926301\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5926301\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"01\\\" data-id=\\\"5926301\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5926301<br \\/> IP: 192.168.0.22<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">01<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":21,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 1 day, 19 hours, 37 minutes<br \\/>Miner program started: 2018-02-10 12:30:00<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:05:04 <br \\/> Last seen: 21 seconds ago<\\/b><br \\/>Total restarts: 150 \\\"> ON(150)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 21.89 MH\\/s<br \\/>GPU1: 23.08 MH\\/s<br \\/>GPU2: 23.64 MH\\/s<br \\/>GPU3: 23.70 MH\\/s<br \\/>GPU4: 23.69 MH\\/s<br \\/>GPU5: 23.67 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">139.66 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"38 50 44 49 47 49 ?<br \\/>70 70 70 70 70 70  %\\\">50?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1480 1468 1480 1480 1480 1480  (75)<br \\/>4498 4498 4498 4498 4498 4498 <\\/small>\",\"id\":\"5926301\",\"group\":\"TestGroup\",\"console\":\"GPU0 t=39C fan=70%, GPU1 t=50C fan=70%, GPU2 t=45C fan=70%, GPU3 t=49C fan=70%, GPU4 t=47C fan=70%, GPU5 t=49C fan=70%<br \\/>\\nGPU0 t=38C fan=70%, GPU1 t=50C fan=70%, GPU2 t=45C fan=70%, GPU3 t=49C fan=70%, GPU4 t=47C fan=70%, GPU5 t=49C fan=70%\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5926301' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5926301'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5926301' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5926301' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5926301' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5926301' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"},{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5926307\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5926307\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"05\\\" data-id=\\\"5926307\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5926307<br \\/> IP: 192.168.0.30<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">05<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":45,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 1 day, 23 hours, 40 minutes<br \\/>Miner program started: 2018-02-10 08:25:35<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:04:40 <br \\/> Last seen: 45 seconds ago<\\/b><br \\/>Total restarts: 193 \\\"> ON(193)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 23.61 MH\\/s<br \\/>GPU1: 23.72 MH\\/s<br \\/>GPU2: 23.52 MH\\/s<br \\/>GPU3: 23.62 MH\\/s<br \\/>GPU4: 23.64 MH\\/s<br \\/>GPU5: 22.21 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">140.33 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"43 35 41 39 40 43 ?<br \\/>70 70 70 70 70 70  %\\\">43?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1493 1493 1544 1506 1556 1506  (75)<br \\/>4498 4498 4498 4498 4498 4498 <\\/small>\",\"id\":\"5926307\",\"group\":\"TestGroup\",\"console\":\"ETH: GPU0 23.332 Mh\\/s, GPU1 23.589 Mh\\/s, GPU2 23.564 Mh\\/s, GPU3 18.238 Mh\\/s, GPU4 23.725 Mh\\/s, GPU5 23.696 Mh\\/s<br \\/>\\nGPU0 t=43C fan=70%, GPU1 t=35C fan=70%, GPU2 t=41C fan=70%, GPU3 t=39C fan=70%, GPU4 t=40C fan=70%, GPU5 t=42C fan=70%\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5926307' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5926307'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5926307' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5926307' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5926307' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5926307' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"},{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5926315\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5926315\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"02\\\" data-id=\\\"5926315\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5926315<br \\/> IP: 192.168.0.72<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">02<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":41,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 1 day, 23 hours, 40 minutes<br \\/>Miner program started: 2018-02-10 08:25:04<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:04:44 <br \\/> Last seen: 41 seconds ago<\\/b><br \\/>Total restarts: 194 \\\"> ON(194)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 23.27 MH\\/s<br \\/>GPU1: 23.65 MH\\/s<br \\/>GPU2: 21.82 MH\\/s<br \\/>GPU3: 21.04 MH\\/s<br \\/>GPU4: 23.73 MH\\/s<br \\/>GPU5: 21.76 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">135.27 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"35 45 39 43 42 44 ?<br \\/>70 70 70 70 70 70  %\\\">45?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1544 1493 1531 1506 1493 1518  (75)<br \\/>4498 4498 4498 4498 4498 4498 <\\/small>\",\"id\":\"5926315\",\"group\":\"TestGroup\",\"console\":\"ETH: GPU0 23.674 Mh\\/s, GPU1 23.489 Mh\\/s, GPU2 23.276 Mh\\/s, GPU3 22.928 Mh\\/s, GPU4 21.187 Mh\\/s, GPU5 23.241 Mh\\/s<br \\/>\\nGPU0 t=35C fan=70%, GPU1 t=44C fan=70%, GPU2 t=39C fan=70%, GPU3 t=43C fan=70%, GPU4 t=41C fan=70%, GPU5 t=44C fan=70%\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5926315' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5926315'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5926315' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5926315' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5926315' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5926315' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"},{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5926332\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5926332\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"08\\\" data-id=\\\"5926332\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5926332<br \\/> IP: 192.168.0.10<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">08<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":21,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 5 hours, 20 minutes<br \\/>Miner program started: 2018-02-12 02:46:38<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:05:04 <br \\/> Last seen: 21 seconds ago<\\/b><br \\/>Total restarts: 213 \\\"> ON(213)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 21.32 MH\\/s<br \\/>GPU1: 21.39 MH\\/s<br \\/>GPU2: 21.97 MH\\/s<br \\/>GPU3: 21.76 MH\\/s<br \\/>GPU4: 22.31 MH\\/s<br \\/>GPU5: 22.38 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">131.13 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"47 69 55 55 62 66 ?<br \\/>70 70 70 70 70 70  %\\\">69?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1582 1442 1468 1569 1544 1544  (80)<br \\/>4404 4404 4404 4404 4404 4404 <\\/small>\",\"id\":\"5926332\",\"group\":\"TestGroup\",\"console\":\"ETH: GPU0 21.599 Mh\\/s, GPU1 20.772 Mh\\/s, GPU2 21.904 Mh\\/s, GPU3 21.107 Mh\\/s, GPU4 21.607 Mh\\/s, GPU5 22.751 Mh\\/s<br \\/>\\nGPU0 t=47C fan=70%, GPU1 t=69C fan=70%, GPU2 t=55C fan=70%, GPU3 t=55C fan=70%, GPU4 t=62C fan=70%, GPU5 t=67C fan=70%\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5926332' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5926332'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5926332' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5926332' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5926332' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5926332' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"},{\"check\":\"<input type=\\\"checkbox\\\" name=\\\"rig[]\\\" class=\\\"tbl-checkbox\\\" value=\\\"5926336\\\" \\/>\",\"notes\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalDescription\\\" class=\\\"change-description-icon table-blue-url\\\" data-id=\\\"5926336\\\">-<\\/span>\",\"name\":\"<span data-toggle=\\\"modal\\\" data-target=\\\"#modalChangeName\\\" class=\\\"change-name-icon table-blue-url\\\" data-name=\\\"09\\\" data-id=\\\"5926336\\\"><span data-toggle=\\\"tooltip\\\" title=\\\"OS Version: NV 1146<br \\/>Rig ID: 5926336<br \\/> IP: 192.168.0.16<br \\/>Linux Kernel: 4.11.12-041112-generic<br \\/>AMDGPU: \\\">09<\\/span><\\/span>\",\"version\":\"NV 1146\",\"valueLastUpdate\":8,\"lastUpdate\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"Rig Uptime: up 1 day, 21 hours, 38 minutes<br \\/>Miner program started: 2018-02-10 10:27:29<br \\/>NOW server time is: 2018-02-12 08:05:25<br \\/> <b>Last seen: 2018-02-12 08:05:17 <br \\/> Last seen: 8 seconds ago<\\/b><br \\/>Total restarts: 227 \\\"> ON(227)<\\/span>\",\"speed\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"GPU0: 23.22 MH\\/s<br \\/>GPU1: 22.86 MH\\/s<br \\/>GPU2: 23.19 MH\\/s<br \\/>GPU3: 22.83 MH\\/s<br \\/>GPU4: 23.18 MH\\/s<br \\/>GPU5: 22.31 MH\\/s<br \\/> <br \\/> Dual mining: <br \\/> \\\">137.59 MH\\/s <\\/span>\",\"temps\":\"<span data-toggle=\\\"tooltip\\\" title=\\\"44 50 52 47 50 57 ?<br \\/>70 70 70 70 70 70  %\\\">57?<br \\/>70 %<\\/span>\",\"gpuCoreMemory\":\"<small>1569 1594 1582 1594 1569 607  (80)<br \\/>4404 4404 4404 4404 4404 405 <\\/small>\",\"id\":\"5926336\",\"group\":\"Kolmovo-City\",\"console\":\"ETH - Total Speed: 123.535 Mh\\/s, Total Shares: 2221, Rejected: 0, Time: 45:37<br \\/>\\nETH: GPU0 20.924 Mh\\/s, GPU1 20.420 Mh\\/s, GPU2 21.105 Mh\\/s, GPU3 20.473 Mh\\/s, GPU4 20.238 Mh\\/s, GPU5 20.376 Mh\\/s\",\"menu\":\"<a data-toggle='modal' data-target='#modalConsole' class='icon-table-rig icon-console' data-id='5926336' href='#'><i data-toggle='tooltip' title='Console' class='glyphicon glyphicon-blackboard gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalOC' href='#' class='icon-table-rig icon-oc' data-id='5926336'><i data-toggle='tooltip' title='Overclocking' class='glyphicon glyphicon-cog gi-3x'> <\\/i><\\/a><a data-toggle='modal' data-target='#modalReload' class='icon-table-rig icon-reload' data-id='5926336' href='#'><i data-toggle='tooltip' title='Reload miner program' class='glyphicon glyphicon-refresh gi-3'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalReboot' class='icon-table-rig icon-reboot' data-id='5926336' href='#'><i data-toggle='tooltip' title='Reboot Rig' class='glyphicon glyphicon-flash gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalSRR' data-id='5926336' href='#' class='icon-table-rig srr-icon'><i data-toggle='tooltip' title='SRR' class='glyphicon glyphicon-qrcode gi-3x'> <\\/i> <\\/a><a data-toggle='modal' data-target='#modalDelete' data-id='5926336' class='icon-table-rig remove-rig' href='#'><i data-toggle='tooltip' title='Delete' class='glyphicon glyphicon-remove gi-3x'> <\\/i> <\\/a>\"}]';
    #endregion Test for Microsoft Store
  }
}

/*
public static string GetTimespanShow(TimeSpan during, EnumDuringShow DuringShow)
{
  string ResultSHow;
  string format_dhms = string.Format("d' {0} 'h' {1} 'm' {2} 'ss' {3}'", Time_Days, Time_Hours, Time_Minuts, Time_Seconds);
  string format_dhm = string.Format("d' {0} 'h' {1} 'm' {2} 'ss' {3}'", Time_Days, Time_Hours, Time_Minuts, Time_Seconds);

  if (during.Days > 0)
  {
    switch (DuringShow)
    {
      case EnumDuringShow.Seconds:
        if (during.Seconds > 0)
          ResultSHow = during.ToString(format_dhms);
        //ResultSHow = during.ToString("d' дн. 'h' час. 'm' мин. 'ss' сек.'");
        else if (during.Minutes > 0)
          ResultSHow = during.ToString("d' дн. 'h' час. 'm' мин.'");
        else if (during.Hours > 0)
          ResultSHow = during.ToString("d' дн. 'h' час.'");
        else
          ResultSHow = during.ToString("d' дн.'");
        break;
      case EnumDuringShow.Minuts:
        if (during.Minutes > 0)
          ResultSHow = during.ToString("d' дн. 'h' час. 'm' мин.'");
        else if (during.Hours > 0)
          ResultSHow = during.ToString("d' дн. 'h' час.'");
        else
          ResultSHow = during.ToString("d' дн.'");

        break;
      case EnumDuringShow.Hours:
        if (during.Hours > 0)
          ResultSHow = during.ToString("d' дн. 'h' час.'");
        else
          ResultSHow = during.ToString("d' дн.'");

        break;
      case EnumDuringShow.Days:
        ResultSHow = during.ToString("d' дн.'");
        break;
      default:
        ResultSHow = during.ToString("d' дн. 'h' час. 'm' мин. 'ss' сек.'");
        break;
    }
  }
  else if (during.Hours > 0)
  {
    switch (DuringShow)
    {
      case EnumDuringShow.Seconds:
        if (during.Seconds > 0)
          ResultSHow = during.ToString("h' час. 'm' мин. 'ss' сек.'");
        else if (during.Minutes > 0)
          ResultSHow = during.ToString("h' час. 'm' мин.'");
        else if (during.Hours > 0)
          ResultSHow = during.ToString("h' час.'");
        else
          ResultSHow = during.ToString("h' час. 'm' мин. 'ss' сек.'");
        break;
      case EnumDuringShow.Minuts:
        if (during.Minutes > 0)
          ResultSHow = during.ToString("h' час. 'm' мин.'");
        else if (during.Hours > 0)
          ResultSHow = during.ToString("h' час.'");
        else
          ResultSHow = during.ToString("h' час. 'm' мин.'");
        break;
      case EnumDuringShow.Hours:
        ResultSHow = during.ToString("h' час.'");
        break;
      case EnumDuringShow.Days:
        ResultSHow = "0 дн.";
        break;
      default:
        ResultSHow = during.ToString("h' час. 'm' мин. 'ss' сек.'");
        break;
    }
  }
  else if (during.Minutes > 0)
  {
    switch (DuringShow)
    {
      case EnumDuringShow.Seconds:
        if (during.Seconds > 0)
          ResultSHow = during.ToString("m' мин. 'ss' сек.'");
        else if (during.Minutes > 0)
          ResultSHow = during.ToString("m' мин.'");
        else
          ResultSHow = during.ToString("m' мин. 'ss' сек.'");
        break;
      case EnumDuringShow.Minuts:
        ResultSHow = during.ToString("m' мин.'");
        break;
      case EnumDuringShow.Hours:
        ResultSHow = "0 час.";
        break;
      case EnumDuringShow.Days:
        ResultSHow = "0 дн.";
        break;
      default:
        ResultSHow = during.ToString("m' мин. 'ss' сек.'");
        break;
    }
  }
  else
    switch (DuringShow)
    {
      case EnumDuringShow.Seconds:
        ResultSHow = during.ToString("ss' сек.'");
        break;
      case EnumDuringShow.Minuts:
        ResultSHow = "0 мин.";
        break;
      case EnumDuringShow.Hours:
        ResultSHow = "0 час.";
        break;
      case EnumDuringShow.Days:
        ResultSHow = "0 дн.";
        break;
      default:
        ResultSHow = during.ToString("ss' сек.'");
        break;
    }

  return ResultSHow;
}
*/
