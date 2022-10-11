using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using Windows.Services.Store;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DashboardMining
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>

  public sealed partial class MainPage : Page
  {
    #region Define vars
    private StoreContext contextLicense = null;
    private StoreAppLicense appLicense = null;

    private CancellationTokenSource cts = null;

    ObservableCollection<RigObject> ListRig = new ObservableCollection<RigObject>();
    ObservableCollection<SandboxObject> ListSandbox = new ObservableCollection<SandboxObject>();
    ConcurrentQueue<TelegramBotObject> QueueTelegramMessageReceive = new ConcurrentQueue<TelegramBotObject>();
    ConcurrentQueue<TelegramBotObject> QueueTelegramMessageSend = new ConcurrentQueue<TelegramBotObject>();

    Object lockListRigs = new object();

    DispatcherTimer timer, timerUptime, timerTelegram;

    int SimpleMiningCountLogin;
    int SimpleMiningCountMain;
    #endregion Define vars

    public MainPage()
    {
      this.InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
      Init();
      await InitializeLicense();
    }

    private async void Init()
    {
      // For TEST ONLY
      //SharedClass.SaveLogout("0"); SharedClass.SaveLogout("1");

      //Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
      //Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ru";
      ApplicationVersion.Text = SharedClass.ApplicationFullVersion;

      txtCountRigs.Text = "0";
      txtUptime.Text = "";
      txtCheckTimeLast.Text = SharedClass.Main_BottomAppBar_CheckLastNotPerformed;

      lblCheckTimeRemain.Visibility = Visibility.Collapsed;
      txtCheckTimeRemain.Text = "";
      lblCheckTimeSeconds.Visibility = Visibility.Collapsed;

      txtError.Visibility = Visibility.Collapsed;

      #region Get settings
      sliderCheckDelay.Value = SharedClass.SettingsCheckDelay; sliderCheckDelay.Tag = "60";
      txtCheckDelay.Text = SharedClass.GetTimeSpanShow(new TimeSpan(0, 0, SharedClass.SettingsCheckDelay), EnumDuringShow.Seconds);

      sliderCheckNotification.Value = SharedClass.SettingsCheckNotification; sliderCheckNotification.Tag = "24";
      txtCheckNotification.Text = SharedClass.GetTimeSpanShow(new TimeSpan(SharedClass.SettingsCheckNotification, 0, 0), EnumDuringShow.Hours);

      sliderCheckRestart.Value = SharedClass.SettingsCheckRestart; sliderCheckRestart.Tag = "10";
      txtCheckRestart.Text = SharedClass.GetTimeSpanShow(new TimeSpan(0, SharedClass.SettingsCheckRestart, 0), EnumDuringShow.Minuts);

      sliderCountRestart.Value = SharedClass.SettingsCountRestart; sliderCountRestart.Tag = "3";
      txtCountRestart.Text = SharedClass.SettingsCountRestart.ToString();

      sliderCountResetRestart.Value = SharedClass.SettingsCountResetRestart; sliderCountResetRestart.Tag = "12";
      txtCountResetRestart.Text = SharedClass.GetTimeSpanShow(new TimeSpan(SharedClass.SettingsCountResetRestart, 0, 0), EnumDuringShow.Hours);

      #region Conditions
      txtRigDuring.Text = SharedClass.SettingsRigDuring.ToString(); txtRigDuring.Tag = "10";

      txtTheVideocardDoesNotWorkDuring.Text = SharedClass.SettingsTheVideocardDoesNotWorkDuring.ToString(); txtTheVideocardDoesNotWorkDuring.Tag = "10";

      txtSpeedSlow.Text = SharedClass.SettingsSpeedSlow.ToString(); txtSpeedSlow.Tag = "15";
      txtSpeedSlowDuring.Text = SharedClass.SettingsSpeedSlowDuring.ToString(); txtSpeedSlowDuring.Tag = "5";

      txtTempMin.Text = SharedClass.SettingsTempMin.ToString(); txtTempMin.Tag = "15";
      txtTempMinDuring.Text = SharedClass.SettingsTempMinDuring.ToString(); txtTempMinDuring.Tag = "20";

      txtTempMax.Text = SharedClass.SettingsTempMax.ToString(); txtTempMax.Tag = "80";
      txtTempMaxDuring.Text = SharedClass.SettingsTempMaxDuring.ToString(); txtTempMaxDuring.Tag = "10";

      txtFanMin.Text = SharedClass.SettingsFanMin.ToString(); txtFanMin.Tag = "70";
      txtFanMinDuring.Text = SharedClass.SettingsFanMinDuring.ToString(); txtFanMinDuring.Tag = "15";

      txtFanMax.Text = SharedClass.SettingsFanMax.ToString(); txtFanMax.Tag = "90";
      txtFanMaxDuring.Text = SharedClass.SettingsFanMaxDuring.ToString(); txtFanMaxDuring.Tag = "10";
      #endregion Conditions

      #endregion Get settings

      cbMining0.ItemsSource = SharedClass.ListMining; cbMining0.SelectedIndex = 0;
      cbMining1.ItemsSource = SharedClass.ListMining; cbMining1.SelectedIndex = 0;

      var ListLoginMining = SharedClass.ListLoginMining;

      txtLogin0.Text = ListLoginMining[0].Login; pswPassword0.Password = ListLoginMining[0].Password; //txtCaptcha0.Text = ListLoginMining[0].SimpleMining_Captcha;
      txtLogin1.Text = ListLoginMining[1].Login; pswPassword1.Password = ListLoginMining[1].Password; //txtCaptcha1.Text = ListLoginMining[1].SimpleMining_Captcha;

      lvEvent.ItemsSource = SharedClass.ListEvent.OrderByDescending(item => item.datetime);

      #region Set Selected Item In ComboBox

      #region Set source
      cbFarmNotAvailable.ItemsSource = SharedClass.ListFarmAction;
      cbTheVideocardDoesNotWorkFor.ItemsSource = SharedClass.ListFarmAction;
      cbTheVideocardIsRunningSlowly.ItemsSource = SharedClass.ListFarmAction;
      cbVideocardTemperatureLow.ItemsSource = SharedClass.ListFarmAction;
      cbVideocardTemperatureHigh.ItemsSource = SharedClass.ListFarmAction;
      cbTheFanOnVideocardIsWeak.ItemsSource = SharedClass.ListFarmAction;
      cbTheFanOnVideocardSpinsHard.ItemsSource = SharedClass.ListFarmAction;
      #endregion Set source

      #region Set value
      cbFarmNotAvailable.SelectedIndex = Convert.ToInt32(SharedClass.ActionRigDuring);
      cbTheVideocardDoesNotWorkFor.SelectedIndex = Convert.ToInt32(SharedClass.ActionTheVideocardDoesNotWorkFor);
      cbTheVideocardIsRunningSlowly.SelectedIndex = Convert.ToInt32(SharedClass.ActionTheVideocardIsRunningSlowly);
      cbVideocardTemperatureLow.SelectedIndex = Convert.ToInt32(SharedClass.ActionVideocardTemperatureLow);
      cbVideocardTemperatureHigh.SelectedIndex = Convert.ToInt32(SharedClass.ActionVideocardTemperatureHigh);
      cbTheFanOnVideocardIsWeak.SelectedIndex = Convert.ToInt32(SharedClass.ActionTheFanOnVideocardIsWeak);
      cbTheFanOnVideocardSpinsHard.SelectedIndex = Convert.ToInt32(SharedClass.ActionTheFanOnVideocardSpinsHard);
      #endregion Set value

      #endregion Set Selected Item In ComboBox      

      #region Timers
      timerUptime = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
      timerUptime.Tick += TimerUptime_Tick;
      timerUptime.Start();

      timerTelegram = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
      timerTelegram.Tick += TimerTelegram_Tick;

      timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
      timer.Tick += Timer_Tick;
      #endregion Timers

      #region Telegram
      txtTelegramBotDashboardName.Text = SharedClass.TelegramBotDashboardName;
      txtTelegramBotToken.Text = SharedClass.TelegramBotToken;
      lvTelegramBotList.ItemsSource = SharedClass.ListTelegramBot;

      if (tsTelegramBot.IsOn == SharedClass.IsTelegramBot)
      {
        TelegramBotCheckState();
      }
      else
      {
        tsTelegramBot.IsOn = SharedClass.IsTelegramBot;
      }

      await Task.Factory.StartNew(async () => { await TelegramBotMessageParse(); });

      #endregion Telegram      

      txtLicense.Text = "";
      ButtonCheckLogin();
    }

    private async Task InitializeLicense()
    {
      if (contextLicense == null) contextLicense = StoreContext.GetDefault();

      // Register for the licenced changed event.
      contextLicense.OfflineLicensesChanged += context_OfflineLicensesChanged;

      await GetLicenseStateAsync();
    }

    private void context_OfflineLicensesChanged(StoreContext sender, object args)
    {
      var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
      {
        await GetLicenseStateAsync();
      });
    }

    private async Task GetLicenseStateAsync()
    {
#if DEBUG
      Debug.WriteLine("{0} GetLicenseStateAsync", DateTime.Now.ToString());
      appLicense = await contextLicense.GetAppLicenseAsync();
      Debug.WriteLine("{0} appLicense.IsActive={1}, appLicense.IsTrial={2}, appLicense.ExpirationDate={3}, appLicense.TrialTimeRemaining={4}", DateTime.Now.ToString(), appLicense.IsActive, appLicense.IsTrial, appLicense.ExpirationDate, appLicense.TrialTimeRemaining);
      var IsActive = true;
#else
      appLicense = await contextLicense.GetAppLicenseAsync();
      var IsActive = appLicense.IsActive;
#endif

      if (IsActive)
      {
        if (appLicense.IsTrial)
        {
          txtLicense.Text = $"This is the trial version. Expiration date: {appLicense.ExpirationDate}. You can buy 'Dashboard Mining' in the Microsoft Store";
        }
        else
        {
          txtLicense.Text = "";
        }
      }
      else
      {
        StopCheckMining();
        txtLicense.Text = $"The trial license is expired. Expiration date: {appLicense.ExpirationDate}. You can buy 'Dashboard Mining' in the Microsoft Store";
        txtLicense.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
      }
    }

    private void TimerUptime_Tick(object sender, object e)
    {
      txtUptime.Text = SharedClass.GetTimeSpanShow(DateTime.Now.Subtract(SharedClass.StartDashboardMining), EnumDuringShow.Seconds);
    }

    private void ButtonCheckLogin()
    {
      string LoginId;
      ComboBox cbMining; TextBox txtLogin; PasswordBox pswPassword; Button cmdLogin, cmdLogout; TextBlock lblErrortxtLogin, lblErrorpswPassword;

      for (int i = 0; i < SharedClass.ListLoginMining.Count; i++)
      {
        LoginId = i.ToString();

        #region Get controls
        cbMining = FindName("cbMining" + LoginId) as ComboBox;
        txtLogin = FindName("txtLogin" + LoginId) as TextBox;
        pswPassword = FindName("pswPassword" + LoginId) as PasswordBox;
        lblErrortxtLogin = FindName("lblErrortxtLogin" + LoginId) as TextBlock;
        lblErrorpswPassword = FindName("lblErrorpswPassword" + LoginId) as TextBlock;
        cmdLogin = FindName("cmdLogin" + LoginId) as Button;
        cmdLogout = FindName("cmdLogout" + LoginId) as Button;
        var wvCaptcha = FindName("wvCaptcha" + LoginId) as WebView;
        #endregion Get controls

        if (SharedClass.ListLoginMining[i].IsLogged)
        {
          cbMining.IsEnabled = false;
          txtLogin.IsEnabled = false;
          pswPassword.IsEnabled = false;
          cmdLogin.IsEnabled = false;
          cmdLogout.IsEnabled = true;

          lblErrortxtLogin.Visibility = Visibility.Collapsed;
          lblErrorpswPassword.Visibility = Visibility.Collapsed;

          wvCaptcha.Visibility = Visibility.Collapsed;
        }
        else
        {
          cbMining.IsEnabled = true;
          txtLogin.IsEnabled = true;
          pswPassword.IsEnabled = true;

          if (string.IsNullOrEmpty(txtLogin.Text)) lblErrortxtLogin.Visibility = Visibility.Visible; else lblErrortxtLogin.Visibility = Visibility.Collapsed;
          if (string.IsNullOrEmpty(pswPassword.Password)) lblErrorpswPassword.Visibility = Visibility.Visible; else lblErrorpswPassword.Visibility = Visibility.Collapsed;

          if (!string.IsNullOrEmpty(txtLogin.Text) && !string.IsNullOrEmpty(pswPassword.Password))
          {
            cmdLogin.IsEnabled = true; cmdLogout.IsEnabled = false;
          }
          else
          {
            cmdLogin.IsEnabled = false; cmdLogout.IsEnabled = false; wvCaptcha.Visibility = Visibility.Collapsed;
          }
        }

        ////For TEST ONLY
        //cbMining.IsEnabled = true;
        //txtLogin.IsEnabled = false;
        //pswPassword.IsEnabled = false;
        //txtCaptcha.IsEnabled = false;
        //cmdLogin.IsEnabled = true;
        //cmdLogout.IsEnabled = true;
      }

      if (SharedClass.ListLoginMining.All(item => !item.IsLogged))
      {
        StopCheckMining();
      }
      else
      {
        if (timer.IsEnabled)
        {
          cmdStart.IsEnabled = false; cmdStop.IsEnabled = true; cmdGetRigs.IsEnabled = false;
        }
        else
        {
          cmdStart.IsEnabled = true; cmdStop.IsEnabled = false; cmdGetRigs.IsEnabled = true;
        }
      }

      if (appLicense != null && !appLicense.IsActive)
      {
#if RELEASE
        StopCheckMining();
#endif
      }

      ////For TEST ONLY
      //cmdStart.IsEnabled = true; cmdStop.IsEnabled = true; cmdGetRigs.IsEnabled = true;
    }

    private void StopCheckMining()
    {
      if (timer.IsEnabled) timer.Stop();

      cmdStart.IsEnabled = false; cmdStop.IsEnabled = false; cmdGetRigs.IsEnabled = false;

      if (cts != null) { cts.Cancel(false); cts = null; }

      lblCheckTimeRemain.Visibility = Visibility.Collapsed;
      txtCheckTimeRemain.Text = "";
      lblCheckTimeSeconds.Visibility = Visibility.Collapsed;
    }

    #region PivotItemDashboard
    private void ButtonShowDisable()
    {
      Button cmdButton;

      for (int i = 0; i < SharedClass.ListLoginMining.Count; i++)
      {
        cmdButton = (FindName("cmdLogin" + i.ToString()) as Button);
        cmdButton.CommandParameter = cmdButton.IsEnabled;
        cmdButton.IsEnabled = false;

        cmdButton = (FindName("cmdLogout" + i.ToString()) as Button);
        cmdButton.CommandParameter = cmdButton.IsEnabled;
        cmdButton.IsEnabled = false;
      }
    }

    private void ButtonShowRestore()
    {
      Button cmdButton;

      for (int i = 0; i < SharedClass.ListLoginMining.Count; i++)
      {
        cmdButton = (FindName("cmdLogin" + i.ToString()) as Button);
        cmdButton.IsEnabled = (bool)cmdButton.CommandParameter;

        cmdButton = (FindName("cmdLogout" + i.ToString()) as Button);
        cmdButton.IsEnabled = (bool)cmdButton.CommandParameter;
      }
    }

    private async void Timer_Tick(object sender, object e)
    {
      if (txtCheckTimeRemain.Text == "0")
      {
        try
        {
          timer.Stop();

          if (cts != null)
          {
            ButtonShowDisable();

            //var CountTime = new Stopwatch(); CountTime.Restart();

            await CheckMining(cts.Token);

            txtCheckTimeRemain.Text = SharedClass.SettingsCheckDelay.ToString();

            //CountTime.Stop(); Debug.WriteLine("Delta={0}", CountTime.ElapsedMilliseconds);
            timer.Start();

            ButtonShowRestore();
          }
        }
        catch (TaskCanceledException)
        {
          //Nothing TO DO - break the task
        }
        catch (Exception ex)
        {
          await ShowDialog("Error Timer_Tick: " + ex.ToString());
        }
      }
      else
      {
        txtCheckTimeRemain.Text = (Convert.ToInt32(txtCheckTimeRemain.Text) - 1).ToString();
        //if (txtCheckTimeRemain.Text == "0") lblCheckTimeSeconds.Text = "Обновление..."; // "Getting list of rigs...";
      }
    }

    private void cmdStart_Click(object sender, RoutedEventArgs e)
    {
      cmdStart.IsEnabled = false;
      cmdStop.IsEnabled = true;
      cmdGetRigs.IsEnabled = false;
      lblCheckTimeRemain.Visibility = Visibility.Visible;
      lblCheckTimeSeconds.Visibility = Visibility.Visible;

      cts = new CancellationTokenSource();

      txtCheckTimeRemain.Text = "0";
      timer.Start();
    }

    private void cmdStop_Click(object sender, RoutedEventArgs e)
    {
      timer.Stop();

      if (cts != null) cts.Cancel(false);
      cts = null;

      cmdStart.IsEnabled = true;
      cmdStop.IsEnabled = false;
      cmdGetRigs.IsEnabled = true;

      lblCheckTimeRemain.Visibility = Visibility.Collapsed;
      txtCheckTimeRemain.Text = "";
      lblCheckTimeSeconds.Visibility = Visibility.Collapsed;
    }

    private async void cmdGetRigs_Click(object sender, RoutedEventArgs e)
    {
      cmdStart.IsEnabled = false;
      cmdStop.IsEnabled = false;
      cmdGetRigs.IsEnabled = false;

      ButtonShowDisable();

      if (cts == null) cts = new CancellationTokenSource();
      await CheckMining(cts.Token);
      cts = null;

      ButtonShowRestore();

      cmdStart.IsEnabled = true;
      cmdStop.IsEnabled = false;
      cmdGetRigs.IsEnabled = true;
    }

    private async Task CheckMining(CancellationToken token)
    {
      CheckProgress.IsActive = true;
      await StartGetRigs(token);
      await CheckRigs();
      CheckProgress.IsActive = false;
    }

    private async Task<Tuple<JObject, string>> SimpleMiningGetRigAsync(CancellationToken token, string Login, string SimpleMining_cfduid, string SimpleMining_cflb, string SimpleMining_PHPSESSID)
    {
      JObject resultJsonTask; string errorMessage;

      var resultListRigs = await SharedLibraryDashboardMining.GetListRigsAsync(token, SimpleMining_cfduid, SimpleMining_cflb, SimpleMining_PHPSESSID);

      if (string.IsNullOrEmpty(resultListRigs.Item2) && !string.IsNullOrEmpty(resultListRigs.Item1))
      {
        try
        {
          resultJsonTask = JObject.Parse(("{" + (char)34 + "rootrigs" + (char)34 + ":" + resultListRigs.Item1 + "}").Replace("  ", " "));
          errorMessage = "";
        }
        catch (Exception ex)
        {
          resultJsonTask = null;
#if DEBUG
          errorMessage = SharedClass.Main_Rigs_Error_Parse + " " + Login + ": " + ex.ToString();
#else
          errorMessage = SharedClass.Main_Rigs_Error_Parse + " " + Login + ": " + ex.Message;
#endif
        }
      }
      else
      {
        resultJsonTask = null;
        errorMessage = SharedClass.Main_Rigs_Error_Get + " " + Login + ": " + resultListRigs.Item2;
      }

      return new Tuple<JObject, string>(resultJsonTask, errorMessage);
    }

    private string CutHtml(string source)
    {
      var pos1 = source.IndexOf("<"); var pos2 = source.IndexOf(">");

      if (pos1 >= 0 && pos2 > pos1 + 1)
      {
        try
        {
          return source.Substring(pos1 + 1, pos2 - pos1 - 1);
        }
        catch (Exception ex)
        {
          return "";
        }
      }
      else if (pos1 == -1 || pos2 == -1)
      {
        return source;
      }
      else if (pos1 + 1 == pos2)
      {
        return "";
      }
      else
      {
        return source;
      }
    }

    private string GetStringFromList(string title, double[] source, double? min = null, double? max = null)
    {
      string result = title + " (";
      for (int i = 0; i < source.Length; i++)
      {
        if ((min != null && source[i] < min) || (max != null && source[i] > max))
        {
          result += (i + 1).ToString() + ": " + source[i].ToString() + ", ";
        }
        else if (min == null && max == null)
        {
          result += source[i].ToString() + " ";
        }
      }
      //result = result.Remove(result.Length - 2, 2) + ")";

      result = result.Trim();
      if (result.Substring(result.Length - 1, 1) == ",") result = result.Remove(result.Length - 1, 1);
      result = result.Trim() + ")";

      return result;
    }

    private async Task StartGetRigs(CancellationToken token)
    {
      #region Define vars
      string id, group, tempName, name, lastUpdate, ip, state, speed, temps, speedAll, tempsAll, coolerAll;
      double[] aSpeed, aTemps, aCooler;
      string[] aSpeedTemp, aTempsCooler, aTempsTemp, aCoolerTemp;
      DateTime LastUpdate;

      RigObject foundRigObject;
      string tempSpeed, totalRestarts, tempTemp;
      int valueLastUpdate;
      int pos1, pos2; int IndexOfRigObject;
      RigObject newRigObject;
      StringBuilder errorMessageTotal = new StringBuilder();
      JObject resultJson;
      SandboxObject foundSandboxObject;
      #endregion Define vars

      txtError.Text = "";

      #region Parse tasks

      foreach (var LoginMiningItem in SharedClass.ListLoginMining)
      {
        if (LoginMiningItem.IsLogged)
        {
          await Task.Delay(SharedClass.Main_Rigs_DelayCheck);
          var resultRig = await SimpleMiningGetRigAsync(token, LoginMiningItem.Login, LoginMiningItem.SimpleMining_cfduid, LoginMiningItem.SimpleMining_cflb, LoginMiningItem.SimpleMining_PHPSESSID);
          if (string.IsNullOrEmpty(resultRig.Item2))
          {
            resultJson = resultRig.Item1;

            #region Add or update rigs
            foreach (var itemRig in resultJson["rootrigs"].Children())
            {
              #region Parse SimpleMining.net
              id = itemRig["id"].ToString().Trim();
              group = itemRig["group"].ToString().Trim();
              name = itemRig["name"].ToString();
              valueLastUpdate = Convert.ToInt32(itemRig["valueLastUpdate"].ToString());
              lastUpdate = itemRig["lastUpdate"].ToString();
              speed = itemRig["speed"].ToString();
              temps = itemRig["temps"].ToString();

              #region Parse name
              pos1 = name.IndexOf("data-name");
              pos2 = name.IndexOf("data-id");
              tempName = name.Substring(pos1 + 10, pos2 - pos1).Split(new char[] { '"' })[1].Trim();
              #endregion Parse name

              #region Parse IP
              try
              {
                pos1 = name.IndexOf("IP:") + 3;
                pos2 = name.Substring(pos1).IndexOf("<br");
                if (pos1 > 2 && pos2 != -1)
                {
                  ip = name.Substring(pos1, pos2).Trim();
                  ip = IPAddress.Parse(ip).ToString();
                }
                else
                {
                  ip = "0.0.0.0";
                }
              }
              catch (Exception)
              {
                ip = "0.0.0.0";
              }
              #endregion Parse IP

              #region Parse state
              try
              {
                totalRestarts = lastUpdate.Substring(lastUpdate.IndexOf("Total restarts:") + 15);
                pos1 = totalRestarts.IndexOf(">") + 1;
                pos2 = totalRestarts.IndexOf("(");
                state = totalRestarts.Substring(pos1, pos2 - pos1).Trim();
              }
              catch (Exception)
              {
                state = "";
              }
              #endregion Parse state

              #region Parse Lastupdate
              if (state.ToLower() == "off")
              {
                try
                {
                  LastUpdate = DateTime.Now.Subtract(new TimeSpan(0, 0, valueLastUpdate));
                }
                catch (Exception)
                {
                  LastUpdate = DateTime.MinValue;
                }
              }
              else
              {
                LastUpdate = DateTime.MinValue;
              }

              #endregion Parse Lastupdate

              #region Parse speed            
              try
              {
                if (speed.IndexOf("title") <= 0) throw new ArgumentException("Speed is fail from farm with id=" + id);
                aSpeedTemp = speed.Split(new string[1] { "GPU" }, StringSplitOptions.None);
                if (aSpeedTemp.Length == 1) throw new ArgumentException("Speed is fail from farm with id=" + id);
                aSpeed = new double[aSpeedTemp.Length - 1];

                for (int i = 1; i < aSpeedTemp.Length; i++)
                {
                  tempSpeed = aSpeedTemp[i].Substring(aSpeedTemp[i].IndexOf(":") + 2);
                  pos1 = tempSpeed.IndexOf(" ");
                  aSpeed[i - 1] = double.Parse(tempSpeed.Substring(0, pos1), CultureInfo.InvariantCulture);
                }
                speedAll = GetStringFromList(aSpeed.Sum().ToString(), aSpeed);
              }
              catch (Exception)
              {
                speedAll = "()"; aSpeed = new double[1];
              }
              #endregion Parse speed

              #region Parse temps and cooler
              aTempsCooler = temps.Split(new string[1] { "<br" }, StringSplitOptions.None);
              if (aTempsCooler.Length == 3)
              {
                #region Parse temps
                try
                {
                  tempTemp = aTempsCooler[0].Substring(aTempsCooler[0].IndexOf("title=") + 7);
                  aTempsTemp = CutHtml(tempTemp).Split(new char[] { ' ' });
                  aTemps = new double[aTempsTemp.Length - 1];
                  for (int i = 0; i < aTempsTemp.Length - 1; i++)
                  {
                    aTemps[i] = double.Parse(aTempsTemp[i], CultureInfo.InvariantCulture);
                  }
                  tempsAll = GetStringFromList("", aTemps);
                }
                catch (Exception)
                {
                  aTemps = new double[1]; tempsAll = "()";
                }
                #endregion Parse temps

                #region Parse cooler
                try
                {
                  aCoolerTemp = CutHtml(aTempsCooler[1]).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                  aCooler = new double[aCoolerTemp.Length - 1];

                  aCooler[0] = double.Parse(aCoolerTemp[0].Substring(2), CultureInfo.InvariantCulture);
                  for (int i = 1; i < aCoolerTemp.Length - 1; i++)
                  {
                    aCooler[i] = double.Parse(aCoolerTemp[i], CultureInfo.InvariantCulture);
                  }
                  coolerAll = GetStringFromList("", aCooler);
                }
                catch (Exception)
                {
                  aCooler = new double[1]; coolerAll = "()";
                }
                #endregion Parse cooler
              }
              else
              {
                aTemps = new double[1]; tempsAll = "";
                aCooler = new double[1]; coolerAll = "";
              }
              #endregion Parse temps and cooler

              #endregion Parse SimpleMining.net

              lock (lockListRigs)
              {
                foundRigObject = ListRig.FirstOrDefault(item => item.id == id);

                if (foundRigObject == null)
                  ListRig.Add(GetNewRig(LoginMiningItem.Login, id, group, tempName, ip, state, aSpeed, speedAll, aTemps, tempsAll, aCooler, coolerAll, LastUpdate));
                else if (foundRigObject.ip != ip || foundRigObject.state != state || foundRigObject.speedAll != speedAll || foundRigObject.tempsAll != tempsAll || foundRigObject.coolerAll != coolerAll)
                {
                  IndexOfRigObject = ListRig.IndexOf(foundRigObject);
                  ListRig.Remove(foundRigObject);

                  newRigObject = GetNewRig(LoginMiningItem.Login, id, group, tempName, ip, state, aSpeed, speedAll, aTemps, tempsAll, aCooler, coolerAll, LastUpdate);
                  newRigObject.LastRestart = foundRigObject.LastRestart;
                  newRigObject.CountRestart = foundRigObject.CountRestart;
                  ListRig.Insert(IndexOfRigObject, newRigObject);
                }
              }
            }
            #endregion Add or update rigs
          }
          else
          {
            errorMessageTotal.AppendLine(resultRig.Item2);

            WriteError(resultRig.Item2);
            //SharedClass.EventSave("", "", "", EnumFarmProblem.ErrorRig, new TimeSpan(0, 0, 1), EnumFarmAction.Notification, "", resultRig.Item2);
            //await TelegramBotMessageToAllAsync(string.Format(SharedClass.Notification_Format2, SharedClass.TelegramBotDashboardName, resultRig.Item2));

            lock (lockListRigs)
            {
              int countListReg = ListRig.Count;
              for (int i = countListReg - 1; i >= 0; i--)
              {
                if (ListRig[i].Login == LoginMiningItem.Login)
                {
                  foundSandboxObject = ListSandbox.FirstOrDefault(itemSandbox => itemSandbox.id == ListRig[i].id);
                  if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

                  ListRig.Remove(ListRig[i]);
                  countListReg = ListRig.Count;
                }
              }
              txtCountRigs.Text = ListRig.Count.ToString();
            }
          }
        }
      }

      aSpeedTemp = null; aTempsCooler = null; aTempsTemp = null; aCoolerTemp = null;
      #endregion Parse tasks

      SortRigsInDashboard();

      txtCountRigs.Text = ListRig.Count.ToString();
      txtCheckTimeLast.Text = DateTime.Now.ToString();

      txtError.Text = errorMessageTotal.ToString();
      if (string.IsNullOrEmpty(txtError.Text))
        txtError.Visibility = Visibility.Collapsed;
      else
      {
        txtError.Visibility = Visibility.Visible;
      }
    }

    private RigObject GetNewRig(string Login, string id, string group, string name, string ip, string state, double[] speed, string speedAll, double[] temps, string tempsAll, double[] cooler, string coolerAll, DateTime LastUpdate)
    {
      var rig = new RigObject
      {
        Login = Login,
        id = id,
        group = group,
        name = name,
        ip = ip,
        state = state,
        speed = speed,
        speedAll = speedAll,
        temps = temps,
        tempsAll = tempsAll,
        cooler = cooler,
        coolerAll = coolerAll,
        LastRestart = DateTime.MinValue,
        LastUpdate = LastUpdate,
        CountRestart = 0
      };
      return rig;
    }

    private async Task CheckRigs()
    {
      #region Define vars
      RigObject foundRigObject; SandboxObject foundSandboxObject; EventObject foundEventObject;
      #endregion Define vars

      #region Try
      try
      {
        string details; bool IsRigOn;

        #region Fill ListSandbox
        foreach (var rig in ListRig)
        {
          #region State
          foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.FarmNotAvailable);

          if (rig.state.ToLower() == "on") //Параметр в норме
          {
            IsRigOn = true;
            if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
          }
          else // OFF - Выход параметра за передельное значение
          {
            IsRigOn = false;

            #region Remove all records from ListSandbox for Rig in OFF
            int countListSandbox = ListSandbox.Count;
            for (int i = countListSandbox - 1; i >= 0; i--)
            {
              if (ListSandbox[i].id == rig.id && ListSandbox[i].FarmProblem != EnumFarmProblem.FarmNotAvailable)
              {
                ListSandbox.Remove(ListSandbox[i]);
                countListSandbox = ListSandbox.Count;
              }
            }
            #endregion Remove all records from ListSandbox for Rig in OFF

            if (foundSandboxObject == null) ListSandbox.Add(GetNewSandboxObject(rig, "", EnumFarmProblem.FarmNotAvailable));
          }
          #endregion State

          #region Speed

          if (IsRigOn) //Farm is available (ON)
          {
            foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.NoVideocardSpeedInfo);
            if (rig.speed.Count() > 0 && rig.speed.Sum() > 0)
            {
              if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.TheVideocardIsRunningSlowly);

              if (rig.speed.Min() >= SharedClass.SettingsSpeedSlow) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList(rig.speed.Sum().ToString(), rig.speed, SharedClass.SettingsSpeedSlow, null);

                if (foundSandboxObject == null)
                  ListSandbox.Add(GetNewSandboxObject(rig, details, EnumFarmProblem.TheVideocardIsRunningSlowly));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
            }
            else
            {
              if (foundSandboxObject == null) ListSandbox.Add(GetNewSandboxObject(rig, "", EnumFarmProblem.NoVideocardSpeedInfo));
            }
          }

          #endregion Speed

          #region Temps
          if (IsRigOn) //Farm is available (ON)
          {
            foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.NoVideocardTemperatureInformation);
            if (rig.temps.Count() > 0 && rig.temps.Sum() > 0)
            {
              if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

              #region Температура видеокарты низкая
              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.VideocardTemperatureLow);
              if (rig.temps.Min() >= SharedClass.SettingsTempMin) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList("", rig.temps, SharedClass.SettingsTempMin, null);

                if (foundSandboxObject == null)
                  ListSandbox.Add(GetNewSandboxObject(rig, details, EnumFarmProblem.VideocardTemperatureLow));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
              #endregion Температура видеокарты низкая

              #region Температура видеокарты высокая
              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.VideocardTemperatureHigh);
              if (rig.temps.Max() <= SharedClass.SettingsTempMax) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList("", rig.temps, null, SharedClass.SettingsTempMax);

                if (foundSandboxObject == null)
                  ListSandbox.Add(GetNewSandboxObject(rig, details, EnumFarmProblem.VideocardTemperatureHigh));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
              #endregion Температура видеокарты высокая
            }
            else
            {
              if (foundSandboxObject == null) ListSandbox.Add(GetNewSandboxObject(rig, "", EnumFarmProblem.NoVideocardTemperatureInformation));
            }
          }

          #endregion Temps

          #region Cooler
          if (IsRigOn) //Farm is available (ON)
          {
            foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.NoFanDataOnVideocards);
            if (rig.cooler.Count() > 0)
            {
              if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

              #region Вентилятор видеокарты крутится слабо
              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.TheFanOnVideocardIsWeak);
              if (rig.cooler.Min() >= SharedClass.SettingsFanMin) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList("", rig.cooler, SharedClass.SettingsFanMin, null);

                if (foundSandboxObject == null)
                  ListSandbox.Add(GetNewSandboxObject(rig, details, EnumFarmProblem.TheFanOnVideocardIsWeak));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
              #endregion Кулер видеокарты слабо крутит

              #region Вентилятор видеокарты крутится сильно
              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.TheFanOnVideocardSpinsHard);
              if (rig.cooler.Max() <= SharedClass.SettingsFanMax) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList("", rig.cooler, null, SharedClass.SettingsFanMax);

                if (foundSandboxObject == null)
                  ListSandbox.Add(GetNewSandboxObject(rig, details, EnumFarmProblem.TheFanOnVideocardSpinsHard));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
              #endregion Вентилятор видеокарты крутится сильно
            }
            else
            {
              if (foundSandboxObject == null) ListSandbox.Add(GetNewSandboxObject(rig, "", EnumFarmProblem.NoFanDataOnVideocards));
            }
          }

          #endregion Cooler           
        }
        #endregion Fill ListSandbox

        #region Triggers

        #region Parameters
        var TheFarmDoesNotWork = new TimeSpan(0, SharedClass.SettingsRigDuring, 0);
        var TheVideocardDoesNotWork = new TimeSpan(0, Convert.ToInt32(txtTheVideocardDoesNotWorkDuring.Text), 0);
        var SpeedSlowDuring = new TimeSpan(0, Convert.ToInt32(txtSpeedSlowDuring.Text), 0);
        var TempMinDuring = new TimeSpan(0, Convert.ToInt32(txtTempMinDuring.Text), 0);
        var TempMaxDuring = new TimeSpan(0, Convert.ToInt32(txtTempMaxDuring.Text), 0);
        var FanMinDuring = new TimeSpan(0, Convert.ToInt32(txtFanMinDuring.Text), 0);
        var FanMaxDuring = new TimeSpan(0, Convert.ToInt32(txtFanMaxDuring.Text), 0);

        var CheckNotification = new TimeSpan(SharedClass.SettingsCheckNotification, 0, 0);
        var CheckRestart = new TimeSpan(0, Convert.ToInt32(sliderCheckRestart.Value), 0);
        #endregion Parameters

        bool IsSendToTelegramBot; EnumFarmAction FarmAction;
        string BotMessage; StringBuilder TelegramBotMessage = new StringBuilder();
        foreach (var item in ListSandbox)
        {
          foundRigObject = ListRig.FirstOrDefault(itemRig => itemRig.id == item.id);
          if (foundRigObject == null) continue;

          foundSandboxObject = item;
          foundEventObject = SharedClass.ListEvent.OrderByDescending(itemEvent1 => itemEvent1.datetime).FirstOrDefault(itemEvent2 => itemEvent2.id == item.id && itemEvent2.FarmProblem == item.FarmProblem);

          IsSendToTelegramBot = false; FarmAction = EnumFarmAction.TakeNoAction;

          #region Swith farm problem
          switch (item.FarmProblem)
          {
            #region FarmNotAvailable
            case EnumFarmProblem.FarmNotAvailable:
              if (item.during >= TheFarmDoesNotWork)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionRigDuring, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionRigDuring;
              }
              break;
            #endregion FarmNotAvailable

            #region NoVideocardSpeedInfo
            case EnumFarmProblem.NoVideocardSpeedInfo:
              if (item.during >= TheFarmDoesNotWork) //Взято время TheFarmDoesNotWork)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionNoVideocardSpeedInfo, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionNoVideocardSpeedInfo;
              }
              break;
            #endregion NoVideocardSpeedInfo

            #region TheVideocardDoesNotWorkFor
            case EnumFarmProblem.TheVideocardDoesNotWorkFor:
              if (item.during >= TheVideocardDoesNotWork)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionTheVideocardDoesNotWorkFor, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionTheVideocardDoesNotWorkFor;
              }
              break;
            #endregion TheVideocardDoesNotWorkFor

            #region TheVideocardIsRunningSlowly
            case EnumFarmProblem.TheVideocardIsRunningSlowly:
              if (item.during >= SpeedSlowDuring)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionTheVideocardIsRunningSlowly, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionTheVideocardIsRunningSlowly;
              }
              break;
            #endregion TheVideocardIsRunningSlowly

            #region NoVideocardTemperatureInformation
            case EnumFarmProblem.NoVideocardTemperatureInformation:
              if (item.during >= TheFarmDoesNotWork)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionNoVideocardTemperatureInformation, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionNoVideocardTemperatureInformation;
              }
              break;
            #endregion NoVideocardTemperatureInformation

            #region VideocardTemperatureLow
            case EnumFarmProblem.VideocardTemperatureLow:
              if (item.during >= TempMinDuring)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionVideocardTemperatureLow, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionVideocardTemperatureLow;
              }

              break;
            #endregion VideocardTemperatureLow

            #region VideocardTemperatureHigh
            case EnumFarmProblem.VideocardTemperatureHigh:
              if (item.during >= TempMaxDuring)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionVideocardTemperatureHigh, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionVideocardTemperatureHigh;
              }
              break;
            #endregion VideocardTemperatureHigh

            #region NoFanDataOnVideocards
            case EnumFarmProblem.NoFanDataOnVideocards:
              if (item.during >= TheFarmDoesNotWork)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionNoFanDataOnVideocards, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionNoFanDataOnVideocards;
              }
              break;
            #endregion NoFanDataOnVideocards

            #region TheFanOnVideocardIsWeak
            case EnumFarmProblem.TheFanOnVideocardIsWeak:
              if (item.during >= FanMinDuring)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionTheFanOnVideocardIsWeak, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionTheFanOnVideocardIsWeak;
              }
              break;
            #endregion TheFanOnVideocardIsWeak

            #region TheFanOnVideocardSpinsHard
            case EnumFarmProblem.TheFanOnVideocardSpinsHard:
              if (item.during >= FanMaxDuring)
              {
                IsSendToTelegramBot = await DoAction(SharedClass.ActionTheFanOnVideocardSpinsHard, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
                FarmAction = SharedClass.ActionTheFanOnVideocardSpinsHard;
              }
              break;
            #endregion TheFanOnVideocardSpinsHard

            default:
              IsSendToTelegramBot = false; FarmAction = EnumFarmAction.TakeNoAction;
              break;
          }

          if (IsSendToTelegramBot)
          {
            if (!string.IsNullOrEmpty(foundSandboxObject.details.Trim()))
            {
              BotMessage = string.Format(SharedClass.Notification_Format1, SharedClass.TelegramBotDashboardName, foundRigObject.group, foundRigObject.name, foundSandboxObject.FarmProblemShow, foundSandboxObject.duringShow, SharedClass.GetFarmAction(FarmAction), foundRigObject.ip, foundSandboxObject.details);
            }
            else
            {
              BotMessage = string.Format(SharedClass.Notification_Format3, SharedClass.TelegramBotDashboardName, foundRigObject.group, foundRigObject.name, foundSandboxObject.FarmProblemShow, foundSandboxObject.duringShow, SharedClass.GetFarmAction(FarmAction), foundRigObject.ip);
            }

            TelegramBotMessage.AppendLine(BotMessage);
          }
          #endregion Swith farm problem
        }

        if (TelegramBotMessage.Length > 0)
        {
          await TelegramBotMessageToAllAsync(TelegramBotMessage.ToString());
        }

        lvEvent.ItemsSource = SharedClass.ListEvent.OrderByDescending(item => item.datetime);

        //var query = from x in list where x.Name = yourcondition select new { x };
        //foreach (var item in query) item.x.FieldToUpdate = SetValue;

        #endregion Triggers

        #region Update source of List Sandbox
        lvSandbox.ItemsSource = ListSandbox.Join(ListRig, first => first.id, second => second.id, (first, second) => new {
          id = first.id,
          group = second.group,
          name = second.name,
          FarmProblem = first.FarmProblem,
          FarmProblemShow = first.FarmProblemShow,
          during = first.during,
          duringShow = first.duringShow,
          ip = second.ip,
          details = first.details
        }).
        OrderBy(item1 => item1.during.Days).
        ThenBy(item2 => item2.during.Hours).
        ThenBy(item3 => item3.during.Minutes).
        ThenBy(item4 => item4.during.Seconds).
        ThenBy(item5 => item5.name);
        #endregion Update source of List Sandbox
      }
      #endregion Try

      #region Catch
      catch (Exception ex)
      {
#if DEBUG
        WriteError(SharedClass.Main_Rigs_Error_Check + ": " + ex.ToString());
#else
        WriteError(SharedClass.Main_Rigs_Error_Check + ": " + ex.Message);
#endif
      }
      #endregion Catch

      #region Get New SandboxObject
      SandboxObject GetNewSandboxObject(RigObject rigObject, string details, EnumFarmProblem FarmProblem)
      {
        DateTime createDateTime;

        if (FarmProblem == EnumFarmProblem.FarmNotAvailable)
        {
          createDateTime = rigObject.LastUpdate;
        }
        else
        {
          createDateTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 1));
        }

        var sandboxObject = new SandboxObject
        {
          id = rigObject.id,
          CreateDateTime = createDateTime,
          FarmProblem = FarmProblem,
          details = details,
          LastNotification = DateTime.MinValue
        };

        return sandboxObject;
      }
      #endregion Get New SandboxObject
    }

    private async Task<bool> DoAction(EnumFarmAction FarmAction, RigObject Rig, SandboxObject Sandbox, EventObject foundEventObject, TimeSpan CheckNotification, TimeSpan CheckRestart)
    {
      bool IsSendToTelegramBot = false;

      LoginMining foundLoginMining;
      switch (FarmAction)
      {
        #region TakeNoAction
        case EnumFarmAction.TakeNoAction:
          if (foundEventObject == null || foundEventObject != null && DateTime.Now.Subtract(foundEventObject.datetime) > CheckNotification)
          {
            SharedClass.EventSave(Rig.id, Rig.group, Rig.name, Sandbox.FarmProblem, Sandbox.during, FarmAction, Rig.ip, Sandbox.details);
          }
          break;
        #endregion TakeNoAction

        #region Notification
        case EnumFarmAction.Notification:
          if (DateTime.Now.Subtract(Sandbox.LastNotification) > CheckNotification && (foundEventObject == null || foundEventObject != null && DateTime.Now.Subtract(foundEventObject.datetime) > CheckNotification))
          {
            IsSendToTelegramBot = true;
            Sandbox.LastNotification = DateTime.Now;
            SharedClass.EventSave(Rig.id, Rig.group, Rig.name, Sandbox.FarmProblem, Sandbox.during, FarmAction, Rig.ip, Sandbox.details);
          }
          break;
        #endregion Notification

        #region RebootTheRig
        case EnumFarmAction.RebootTheRig:
          bool IsRestart;
          if (Rig.CountRestart < SharedClass.SettingsCountRestart)
          {
            if (DateTime.Now.Subtract(Rig.LastRestart) > CheckRestart)
            {
              IsRestart = true;
              Rig.CountRestart++;
            }
            else
            {
              IsRestart = false;
            }
          }
          else
          {
            IsRestart = false;
            if (DateTime.Now.Subtract(Rig.LastRestart) > new TimeSpan(SharedClass.SettingsCountResetRestart, 0, 0))
            {
              Rig.CountRestart = 0;
            }
          }

          if (IsRestart)
          {
            IsSendToTelegramBot = true;

            Rig.LastRestart = DateTime.Now;
            foundLoginMining = SharedClass.ListLoginMining.First(itemLogin => itemLogin.Login == Rig.Login);

            await SharedLibraryDashboardMining.RebootAsync((int)EnumMining.SimpleMiningNet, Rig.id, foundLoginMining.SimpleMining_cfduid, foundLoginMining.SimpleMining_PHPSESSID);
            SharedClass.EventSave(Rig.id, Rig.group, Rig.name, Sandbox.FarmProblem, Sandbox.during, FarmAction, Rig.ip, Sandbox.details, Rig.CountRestart);
          }
          break;
        #endregion RebootTheRig

        default:
          break;
      }

      return IsSendToTelegramBot;
    }

    private void SortItem_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
      SharedClass.DashboardOrderColumn = (string)(e.OriginalSource as TextBlock).Tag;

      if (SharedClass.DashboardOrderDirection == "")
      {
        SharedClass.DashboardOrderDirection = "desc";
      }
      else
      {
        SharedClass.DashboardOrderDirection = "";
      }

      SortRigsInDashboard();
    }

    private void SortRigsInDashboard()
    {
      var tempList = new List<RigObject>();
      lock (lockListRigs)
      {
        switch (SharedClass.DashboardOrderColumn)
        {
          case "group":
            if (SharedClass.DashboardOrderDirection == "")
              foreach (var item in ListRig.OrderBy(item1 => item1.group).ThenBy(item2 => item2.name)) tempList.Add(item);
            else
              foreach (var item in ListRig.OrderByDescending(item1 => item1.group).ThenBy(item2 => item2.name)) tempList.Add(item);
            break;
          case "name":
            if (SharedClass.DashboardOrderDirection == "")
              foreach (var item in ListRig.OrderBy(item1 => item1.name).ThenBy(item2 => item2.group)) tempList.Add(item);
            else
              foreach (var item in ListRig.OrderByDescending(item1 => item1.name).ThenBy(item2 => item2.group)) tempList.Add(item);
            break;
          case "ip":
            if (SharedClass.DashboardOrderDirection == "")
              foreach (var item in ListRig.OrderBy(item1 => item1.ipShow[0]).ThenBy(item2 => item2.ipShow[1]).ThenBy(item3 => item3.ipShow[2]).ThenBy(item4 => item4.ipShow[3])) tempList.Add(item);
            else
              foreach (var item in ListRig.OrderByDescending(item1 => item1.ipShow[0]).ThenByDescending(item2 => item2.ipShow[1]).ThenByDescending(item3 => item3.ipShow[2]).ThenByDescending(item4 => item4.ipShow[3])) tempList.Add(item);
            break;
          case "state":
            if (SharedClass.DashboardOrderDirection == "")
              foreach (var item in ListRig.OrderBy(item1 => item1.state).ThenBy(item2 => item2.group).ThenBy(item3 => item3.name)) tempList.Add(item);
            else
              foreach (var item in ListRig.OrderByDescending(item1 => item1.state).ThenBy(item2 => item2.group).ThenBy(item3 => item3.name)) tempList.Add(item);
            break;
          case "speedAll":
            if (SharedClass.DashboardOrderDirection == "")
              foreach (var item in ListRig.OrderBy(item1 => item1.speed.Sum())) tempList.Add(item);
            //foreach (var item in ListRig.OrderBy(item1 => item1.speed.Min())) tempList.Add(item);
            else
              foreach (var item in ListRig.OrderByDescending(item1 => item1.speed.Sum())) tempList.Add(item);
            //foreach (var item in ListRig.OrderByDescending(item1 => item1.speed.Max())) tempList.Add(item);
            break;
          case "tempsAll":
            if (SharedClass.DashboardOrderDirection == "")
              foreach (var item in ListRig.OrderBy(item1 => item1.temps.Min()).ThenBy(item2 => item2.group).ThenBy(item3 => item3.name)) tempList.Add(item);
            else
              foreach (var item in ListRig.OrderByDescending(item1 => item1.temps.Max()).ThenByDescending(item2 => item2.group).ThenByDescending(item3 => item3.name)) tempList.Add(item);
            break;
          case "coolerAll":
            if (SharedClass.DashboardOrderDirection == "")
              foreach (var item in ListRig.OrderBy(item1 => item1.cooler.Min()).ThenBy(item2 => item2.group).ThenBy(item3 => item3.name)) tempList.Add(item);
            else
              foreach (var item in ListRig.OrderByDescending(item1 => item1.cooler.Max()).ThenByDescending(item2 => item2.group).ThenByDescending(item3 => item3.name)) tempList.Add(item);
            break;
          default:
            if (SharedClass.DashboardOrderDirection == "")
              foreach (var item in ListRig.OrderBy(item1 => item1.group).ThenBy(item2 => item2.name)) tempList.Add(item);
            else
              foreach (var item in ListRig.OrderByDescending(item1 => item1.group).ThenBy(item2 => item2.name)) tempList.Add(item);
            break;
        }
        ListRig.Clear();
        foreach (var item in tempList) ListRig.Add(item);
        tempList.Clear(); tempList = null;
      }
    }


    #endregion PivotItemDashboard

    #region PivotItemEvent

    #region App bar
    private async void appbarLogCopy_Click(object sender, RoutedEventArgs e)
    {
      if (lvEvent.SelectedItems.Count > 0)
      {
        EventObject eventObject; StringBuilder sbEventObject = new StringBuilder();

        DataPackage dataPackage = new DataPackage();
        dataPackage.RequestedOperation = DataPackageOperation.Copy;

        foreach (var item in lvEvent.SelectedItems.OrderBy(item => (item as EventObject).datetime))
        {
          eventObject = (item as EventObject);
          sbEventObject.AppendLine(eventObject.datetime + ";" + eventObject.group + ";" + eventObject.name + ";" + eventObject.FarmProblemShow + ";" + eventObject.duringShow + ";" + eventObject.actionShow + ";" + eventObject.ip + ";" + eventObject.details);
        }
        dataPackage.SetText(sbEventObject.ToString());
        Clipboard.SetContent(dataPackage);

        await ShowDialog(SharedClass.Event_LogCopy);
      }
    }

    private async void appbarLogDelete_Click(object sender, RoutedEventArgs e)
    {
      if (lvEvent.SelectedItems.Count > 0)
      {
        if (await ShowDialogYesNo(SharedClass.Event_LogDelete))
        {
          foreach (var item in lvEvent.SelectedItems)
          {
            SharedClass.EventRemove((item as EventObject).datetime);
          }
          lvEvent.ItemsSource = SharedClass.ListEvent.OrderByDescending(item => item.datetime);
        }
      }
    }

    private async void appbarLogClear_Click(object sender, RoutedEventArgs e)
    {
      if (lvEvent.Items.Count > 0)
      {
        if (await ShowDialogYesNo(SharedClass.Event_LogClear1))
        {
          if (await ShowDialogYesNo(SharedClass.Event_LogClear2))
          {
            SharedClass.EventClear();
            lvEvent.ItemsSource = SharedClass.ListEvent.OrderByDescending(item => item.datetime);
          }
        }
      }
    }
    #endregion App bar

    #endregion PivotItemEvent

    #region PivotItemError
    private async void ErrorLogClear_Click(object sender, RoutedEventArgs e)
    {
      if (!string.IsNullOrEmpty(tbError.Text.Trim()))
      {
        if (await ShowDialogYesNo(SharedClass.Error_LogClear))
        {
          tbError.Text = "";
        }
      }
    }
    #endregion PivotItemError

    #region PivotItemSettings
    private void sliderCheckDelay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (e.OldValue > 0)
      {
        SharedClass.SettingsCheckDelay = Convert.ToInt32(e.NewValue);
        txtCheckDelay.Text = SharedClass.GetTimeSpanShow(new TimeSpan(0, 0, SharedClass.SettingsCheckDelay), EnumDuringShow.Seconds);
      }
    }

    private void sliderCheckNotification_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (e.OldValue > 0)
      {
        SharedClass.SettingsCheckNotification = Convert.ToInt32(e.NewValue);
        txtCheckNotification.Text = SharedClass.GetTimeSpanShow(new TimeSpan(SharedClass.SettingsCheckNotification, 0, 0), EnumDuringShow.Hours);
      }
    }

    private void sliderCheckRestart_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (e.OldValue > 0)
      {
        SharedClass.SettingsCheckRestart = Convert.ToInt32(e.NewValue);
        txtCheckRestart.Text = SharedClass.GetTimeSpanShow(new TimeSpan(0, SharedClass.SettingsCheckRestart, 0), EnumDuringShow.Minuts);
      }
    }

    private void sliderCountRestart_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (e.OldValue > 0)
      {
        SharedClass.SettingsCountRestart = Convert.ToInt32(e.NewValue);
        txtCountRestart.Text = SharedClass.SettingsCountRestart.ToString();
      }
    }

    private void sliderCountResetRestart_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
      if (e.OldValue > 0)
      {
        SharedClass.SettingsCountResetRestart = Convert.ToInt32(e.NewValue);
        txtCountResetRestart.Text = SharedClass.GetTimeSpanShow(new TimeSpan(SharedClass.SettingsCountResetRestart, 0, 0), EnumDuringShow.Hours);
      }
    }

    private int CheckSettings(TextBox txtBox)
    {
      if (txtBox.Text.Trim() != "")
      {
        if (int.TryParse(txtBox.Text, out var intBox))
        {
          if (intBox > 0)
          {
            return intBox;
          }
          else
          {
            txtBox.Text = txtBox.Tag.ToString();
            return Convert.ToInt32(txtBox.Tag.ToString());
          }
        }
        else
        {
          txtBox.Text = txtBox.Tag.ToString();
          return Convert.ToInt32(txtBox.Tag.ToString());
        }
      }
      else
      {
        txtBox.Text = txtBox.Tag.ToString();
        return Convert.ToInt32(txtBox.Tag.ToString());
      }
    }

    private void txtRigDuring_TextChanged(object sender, TextChangedEventArgs e)
    {
      SharedClass.SettingsRigDuring = CheckSettings(sender as TextBox);
    }

    private void txtTheVideocardDoesNotWorkDuring_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsTheVideocardDoesNotWorkDuring = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsTheVideocardDoesNotWorkDuring = CheckSettings(sender as TextBox);
    }

    private void txtSpeedSlow_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsSpeedSlow = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsSpeedSlow = CheckSettings(sender as TextBox);
    }

    private void txtSpeedSlowDuring_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsSpeedSlowDuring = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsSpeedSlowDuring = CheckSettings(sender as TextBox);
    }

    private void txtTempMin_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsTempMin = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsTempMin = CheckSettings(sender as TextBox);
    }

    private void txtTempMinDuring_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsTempMinDuring = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsTempMinDuring = CheckSettings(sender as TextBox);
    }

    private void txtTempMax_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsTempMax = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsTempMax = CheckSettings(sender as TextBox);
    }

    private void txtTempMaxDuring_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsTempMaxDuring = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsTempMaxDuring = CheckSettings(sender as TextBox);
    }

    private void txtFanMin_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsFanMin = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsFanMin = CheckSettings(sender as TextBox);
    }

    private void txtFanMinDuring_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsFanMinDuring = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsFanMinDuring = CheckSettings(sender as TextBox);
    }

    private void txtFanMax_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsFanMax = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsFanMax = CheckSettings(sender as TextBox);
    }

    private void txtFanMaxDuring_TextChanged(object sender, TextChangedEventArgs e)
    {
      //SharedClass.SettingsFanMaxDuring = Convert.ToInt32((sender as TextBox).Text);
      SharedClass.SettingsFanMaxDuring = CheckSettings(sender as TextBox);
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var cbAction = (sender as ComboBox);

      string FarmProblem = (string)cbAction.Tag;
      EnumFarmAction FarmAction = (cbAction.SelectedItem as FarmActionObject).enumFarmAction;

      if (FarmProblem == EnumFarmProblem.FarmNotAvailable.ToString()) SharedClass.ActionRigDuring = FarmAction;
      if (FarmProblem == EnumFarmProblem.TheVideocardDoesNotWorkFor.ToString()) SharedClass.ActionTheVideocardDoesNotWorkFor = FarmAction;
      if (FarmProblem == EnumFarmProblem.NoVideocardSpeedInfo.ToString()) SharedClass.ActionNoVideocardSpeedInfo = FarmAction;
      if (FarmProblem == EnumFarmProblem.TheVideocardIsRunningSlowly.ToString()) SharedClass.ActionTheVideocardIsRunningSlowly = FarmAction;
      if (FarmProblem == EnumFarmProblem.NoVideocardTemperatureInformation.ToString()) SharedClass.ActionNoVideocardTemperatureInformation = FarmAction;
      if (FarmProblem == EnumFarmProblem.VideocardTemperatureLow.ToString()) SharedClass.ActionVideocardTemperatureLow = FarmAction;
      if (FarmProblem == EnumFarmProblem.VideocardTemperatureHigh.ToString()) SharedClass.ActionVideocardTemperatureHigh = FarmAction;
      if (FarmProblem == EnumFarmProblem.NoFanDataOnVideocards.ToString()) SharedClass.ActionNoFanDataOnVideocards = FarmAction;
      if (FarmProblem == EnumFarmProblem.TheFanOnVideocardIsWeak.ToString()) SharedClass.ActionTheFanOnVideocardIsWeak = FarmAction;
      if (FarmProblem == EnumFarmProblem.TheFanOnVideocardSpinsHard.ToString()) SharedClass.ActionTheFanOnVideocardSpinsHard = FarmAction;
    }

    private void cmdGetDefault_Click(object sender, RoutedEventArgs e)
    {
      sliderCheckDelay.Value = Convert.ToDouble(sliderCheckDelay.Tag);
      sliderCheckRestart.Value = Convert.ToDouble(sliderCheckRestart.Tag);
      sliderCheckNotification.Value = Convert.ToDouble(sliderCheckNotification.Tag);
      sliderCountRestart.Value = Convert.ToDouble(sliderCountRestart.Tag);
      sliderCountResetRestart.Value = Convert.ToDouble(sliderCountResetRestart.Tag);

      txtRigDuring.Text = txtRigDuring.Tag.ToString();
      txtTheVideocardDoesNotWorkDuring.Text = txtTheVideocardDoesNotWorkDuring.Tag.ToString();

      txtSpeedSlow.Text = txtSpeedSlow.Tag.ToString();
      txtSpeedSlowDuring.Text = txtSpeedSlowDuring.Tag.ToString();

      txtTempMin.Text = txtTempMin.Tag.ToString();
      txtTempMinDuring.Text = txtTempMinDuring.Tag.ToString();

      txtTempMax.Text = txtTempMax.Tag.ToString();
      txtTempMaxDuring.Text = txtTempMaxDuring.Tag.ToString();

      txtFanMin.Text = txtFanMin.Tag.ToString();
      txtFanMinDuring.Text = txtFanMinDuring.Tag.ToString();

      txtFanMax.Text = txtFanMax.Tag.ToString();
      txtFanMaxDuring.Text = txtFanMaxDuring.Tag.ToString();

      #region Set Selected Item In ComboBox
      cbFarmNotAvailable.SelectedIndex = 1;
      cbTheVideocardDoesNotWorkFor.SelectedIndex = 1;
      cbTheVideocardIsRunningSlowly.SelectedIndex = 2;
      cbVideocardTemperatureLow.SelectedIndex = 0;
      cbVideocardTemperatureHigh.SelectedIndex = 1;
      cbTheFanOnVideocardIsWeak.SelectedIndex = 0;
      cbTheFanOnVideocardSpinsHard.SelectedIndex = 1;
      #endregion Set Selected Item In ComboBox
    }
    #endregion PivotItemSettings

    #region PivotItemNotification
    #region Telegram
    //private async Task TelegramBotMessage(string Message)
    //{
    //  if (SharedClass.IsTelegramBotReady)
    //  {
    //    await Task.Factory.StartNew(async () =>
    //    {
    //      try
    //      {
    //        var result = await TelegramBotGetUpdatesAsync(SharedClass.TelegramBotToken, BotUpdateId);
    //        if (string.IsNullOrEmpty(result.Item3))
    //        {
    //          var ListChatId = result.Item1;
    //          BotUpdateId = Convert.ToInt32(result.Item2);

    //          await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
    //          {
    //            txtTelegramBotCount.Text = ListChatId.Count().ToString();
    //          });

    //          foreach (var BotChatId in ListChatId)
    //          {
    //            await TelegramBotSendMessageAsync(SharedClass.TelegramBotToken, Message, BotChatId);
    //          }
    //        }
    //      }
    //      catch (TaskCanceledException)
    //      {
    //        //Nothing TO DO - break the task
    //      }
    //      catch (Exception ex)
    //      {
    //        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
    //        {
    //          await ShowDialog("Ошибка TelegramBotMessage: " + ex.ToString());
    //        });
    //      }
    //    }
    //    );
    //  }
    //}

    private async void TimerTelegram_Tick(object sender, object e)
    {
      try
      {
        var result = await TelegramBotGetUpdatesAsync(SharedClass.TelegramBotToken, SharedClass.TelegramBotUpdateId);
        if (string.IsNullOrEmpty(result.Item3))
        {
          SharedClass.TelegramBotUpdateId = Convert.ToInt32(result.Item2);

          foreach (var TelegramBotMessage in result.Item1)
          {
            QueueTelegramMessageReceive.Enqueue(new TelegramBotObject
            {
              ChatId = TelegramBotMessage.ChatId,
              Firstname = TelegramBotMessage.Firstname,
              Lastname = TelegramBotMessage.Lastname,
              Username = TelegramBotMessage.Username,
              Language = TelegramBotMessage.Language,
              Text = TelegramBotMessage.Text,
              PhoneNumber = TelegramBotMessage.PhoneNumber,
              MessageId = TelegramBotMessage.MessageId
            });
          }
        }
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
          txtTelegramBotCount.Text = SharedClass.ListTelegramBot.Count(item => item.ChatId > 0).ToString();
          //lvTelegramBotList.ItemsSource = ListTelegramChat.Where(item => item.ChatId > 0);
        });
      }
      catch (TaskCanceledException)
      {
        //Nothing TO DO - break the task
      }
      catch (Exception ex)
      {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
#if DEBUG
          WriteError("Error TimerTelegram_Tick: " + ex.ToString());
#else
          WriteError("Error Telegram: " + ex.Message);
#endif
        });
      }
    }

    private async Task TelegramBotMessageParse()
    {
      string[] aCommand; string CommandFromTelegramBot; string ReplyMessage;
      TelegramBotObject foundTelegramBotObject;
      bool IsUpdateTelegramBotObject = false;

      while (true)
      {
        try
        {
          var QueueTelegramMessageReceiveCount = QueueTelegramMessageReceive.Count();
          for (int i = 0; i < QueueTelegramMessageReceiveCount; i++)
          {
            //if (ct.IsCancellationRequested) break;

            if (QueueTelegramMessageReceive.TryDequeue(out var TelegramMessage))
            {              
              #region Обработка команд
              if (!string.IsNullOrEmpty(TelegramMessage.Text))
              {
                foundTelegramBotObject = SharedClass.ListTelegramBot.FirstOrDefault(item => item.ChatId == TelegramMessage.ChatId);

                if (foundTelegramBotObject == null) //Требуется авторизация
                {
                  await Task.Factory.StartNew(async () => { await TelegramBotRequestContactShowAsync(TelegramMessage.ChatId); });
                }
                else
                {
                  aCommand = TelegramMessage.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                  CommandFromTelegramBot = aCommand[0].Trim();

                  #region switch CommandFromTelegramBot
                  switch (CommandFromTelegramBot)
                  {
                    #region /start
                    case "/start":
                      //Авторизация уже пройдена
                      await Task.Factory.StartNew(async () => { await TelegramBotSendMessageAsync(SharedClass.TelegramBotToken, "You are already authorized", TelegramMessage.ChatId); });

                      break;
                    #endregion /start

                    #region /check
                    case "/check":
                      var TelegramBotMessage = new StringBuilder(); string BotMessage;

                      //TelegramBotMessage.AppendLine("Count farms: " + ListRig.Count.ToString() + ", Running state is " + timer.IsEnabled.ToString());
                      TelegramBotMessage.AppendLine("Count farms: " + ListRig.Count.ToString());
                      TelegramBotMessage.AppendLine("-------------------");
                      TelegramBotMessage.AppendLine("Current state:");
                      TelegramBotMessage.AppendLine("-------------------");

                      foreach (var itemSandbox in ListSandbox.Join(ListRig, first => first.id, second => second.id, (first, second) => new {
                        id = first.id,
                        group = second.group,
                        name = second.name,
                        FarmProblem = first.FarmProblem,
                        FarmProblemShow = first.FarmProblemShow,
                        during = first.during,
                        duringShow = first.duringShow,
                        ip = second.ip,
                        details = first.details
                      }).
                      OrderBy(item1 => item1.during.Days).
                      ThenBy(item2 => item2.during.Hours).
                      ThenBy(item3 => item3.during.Minutes).
                      ThenBy(item4 => item4.during.Seconds).
                      ThenBy(item5 => item5.name))
                      {
                        if (!string.IsNullOrEmpty(itemSandbox.details.Trim()))
                        {
                          BotMessage = string.Format(SharedClass.Notification_Format4, SharedClass.TelegramBotDashboardName, itemSandbox.group, itemSandbox.name, itemSandbox.FarmProblemShow, itemSandbox.duringShow, itemSandbox.ip, itemSandbox.details);
                        }
                        else
                        {
                          BotMessage = string.Format(SharedClass.Notification_Format5, SharedClass.TelegramBotDashboardName, itemSandbox.group, itemSandbox.name, itemSandbox.FarmProblemShow, itemSandbox.duringShow, itemSandbox.ip);
                        }

                        TelegramBotMessage.AppendLine(BotMessage);
                      }

                      await Task.Factory.StartNew(async () => { await TelegramBotSendMessageAsync(SharedClass.TelegramBotToken, TelegramBotMessage.ToString(), TelegramMessage.ChatId); });
                      break;
                    #endregion /check

                    default:
                      break;
                  }
                  #endregion switch CommandFromTelegramBot
                }
              }
              #endregion Обработка команд

              #region Обработка номера телефона
              if (!string.IsNullOrEmpty(TelegramMessage.PhoneNumber))
              {
                foundTelegramBotObject = SharedClass.ListTelegramBot.FirstOrDefault(item => item.PhoneNumber == TelegramMessage.PhoneNumber);
                if (foundTelegramBotObject != null)
                {
                  foundTelegramBotObject.ChatId = TelegramMessage.ChatId;
                  foundTelegramBotObject.Firstname = TelegramMessage.Firstname;
                  foundTelegramBotObject.Lastname = TelegramMessage.Lastname;
                  foundTelegramBotObject.Username = TelegramMessage.Username;
                  foundTelegramBotObject.Language = TelegramMessage.Language;

                  SharedClass.TelegramBotSave(foundTelegramBotObject.PhoneNumber, foundTelegramBotObject.ChatId, foundTelegramBotObject.Firstname, foundTelegramBotObject.Lastname, foundTelegramBotObject.Username, foundTelegramBotObject.Language);

                  ReplyMessage = "You are authorized";
                  IsUpdateTelegramBotObject = true;
                }
                else // Этот номер телефона не авторизован
                {
                  ReplyMessage = "This phone number is not authorized";
                }

                await Task.Factory.StartNew(async () => { await TelegramBotRequestContactHideAsync(TelegramMessage.ChatId, TelegramMessage.MessageId, ReplyMessage); });
              }
              #endregion Обработка номера телефона

              QueueTelegramMessageReceiveCount = QueueTelegramMessageReceive.Count();
            }
            else
            {
            }
          }

          if (IsUpdateTelegramBotObject)
          {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { lvTelegramBotList.ItemsSource = SharedClass.ListTelegramBot; });
            IsUpdateTelegramBotObject = false;
          }
        }
        catch (Exception ex)
        {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
          {
#if DEBUG
            WriteError("Error TelegramBotMessageParse(): " + ex.ToString());
#else
            WriteError("Error Telegram parse: " + ex.Message);
#endif
          });
        }

        await Task.Delay(1000);
      }
    }

    private async Task TelegramBotMessageToAllAsync(string Message)
    {
      if (tsTelegramBot.IsOn)
      {
        foreach (var TelegramChat in SharedClass.ListTelegramBot.Where(item => item.ChatId > 0))
        {
          await Task.Factory.StartNew(async () => { await TelegramBotSendMessageAsync(SharedClass.TelegramBotToken, Message, TelegramChat.ChatId); });
        }
      }
    }

    /*
    KeyboardButton
    This object represents one button of the reply keyboard. For simple text buttons String can be used instead of this object to specify text of the button. Optional fields are mutually exclusive.
    ---------------------------------------------------------------------------------------------------------------------------------------------------------------------
    Field	            Type	    Description
    ---------------------------------------------------------------------------------------------------------------------------------------------------------------------
    text	            String	  Text of the button. If none of the optional fields are used, it will be sent as a message when the button is pressed
    request_contact	  Boolean	  Optional. If True, the user's phone number will be sent as a contact when the button is pressed. Available in private chats only
    request_location	Boolean 	Optional. If True, the user's current location will be sent when the button is pressed. Available in private chats only
    ---------------------------------------------------------------------------------------------------------------------------------------------------------------------
    Note: request_contact and request_location options will only work in Telegram versions released after 9 April, 2016. Older clients will ignore them.
    */
    private async Task TelegramBotRequestContactShowAsync(int ChatId)
    {
      try
      {
        JObject KeyboardButton = new JObject();
        KeyboardButton.Add("text", "Send me a phone number for authorization");
        KeyboardButton.Add("request_contact", true);

        JArray ja = new JArray();
        ja.Add(KeyboardButton);

        JArray aReplyKeyboardMarkup = new JArray();
        aReplyKeyboardMarkup.Add(ja);

        JObject ReplyKeyboardMarkup = new JObject();
        ReplyKeyboardMarkup.Add("keyboard", aReplyKeyboardMarkup);
        ReplyKeyboardMarkup.Add("resize_keyboard", true);
        ReplyKeyboardMarkup.Add("one_time_keyboard", true);
        ReplyKeyboardMarkup.Add("selective", true);

        await TelegramBotSendMessageAsync(SharedClass.TelegramBotToken, "We need to check you. Please, press button below for authorization", ChatId, ReplyKeyboardMarkup);
      }
      catch (Exception ex)
      {
      }
    }

    private async Task TelegramBotRequestContactHideAsync(int ChatId, int ReplyToMessageId, string ReplyMessage)
    {
      try
      {
        JObject ReplyKeyboardMarkup = new JObject();
        ReplyKeyboardMarkup.Add("remove_keyboard", true);
        ReplyKeyboardMarkup.Add("selective", true);

        await TelegramBotSendMessageAsync(SharedClass.TelegramBotToken, ReplyMessage, ChatId, ReplyKeyboardMarkup, ReplyToMessageId);
      }
      catch (Exception ex)
      {
      }
    }

    private async Task<Tuple<List<TelegramBotObject>, int, string>> TelegramBotGetUpdatesAsync(string BotToken, int BotUpdateId)
    {
      #region Define vars
      Tuple<List<TelegramBotObject>, int, string> returnValue;
      int botUpdateIdNext; string PhoneNumber;
      #endregion Define vars

      #region Try
      try
      {
        botUpdateIdNext = BotUpdateId + 1;
        using (var webClient = new WebClient())
        {
          string response = await webClient.DownloadStringTaskAsync(address: "https://api.telegram.org/bot" + BotToken + "/getUpdates" + "?offset=" + botUpdateIdNext.ToString());
          var resultJson = JObject.Parse(response);
          if (bool.Parse(resultJson["ok"].ToString()))
          {
            var ListTelegramBotMessage = new List<TelegramBotObject>();
            foreach (var item in resultJson["result"])
            {
              try
              {
                botUpdateIdNext = Convert.ToInt32(item["update_id"].ToString());

                if (item["message"]["contact"] != null)
                {
                  PhoneNumber = GetStringFromObject(item["message"]["contact"]["phone_number"]);
                }
                else
                {
                  PhoneNumber = "";
                }

                ListTelegramBotMessage.Add(new TelegramBotObject
                {
                  ChatId = Convert.ToInt32(GetStringFromObject(item["message"]["chat"]["id"])),
                  Firstname = GetStringFromObject(item["message"]["from"]["first_name"]),
                  Lastname = GetStringFromObject(item["message"]["from"]["last_name"]),
                  Username = GetStringFromObject(item["message"]["from"]["Username"]),
                  Language = GetStringFromObject(item["message"]["from"]["language_code"]),
                  Text = GetStringFromObject(item["message"]["text"]),
                  PhoneNumber = PhoneNumber,
                  MessageId = Convert.ToInt32(GetStringFromObject(item["message"]["message_id"]))
                });

              }
              catch (Exception ex)
              {
              }
            }
            returnValue = new Tuple<List<TelegramBotObject>, int, string>(ListTelegramBotMessage, botUpdateIdNext, "");
          }
          else
          {
            returnValue = new Tuple<List<TelegramBotObject>, int, string>(null, 0, "Error from Telegram");
          }
        }
      }
      #endregion Try

      #region Catch
      catch (Exception ex)
      {
        returnValue = new Tuple<List<TelegramBotObject>, int, string>(null, 0, ex.Message);
      }
      #endregion Catch

      return returnValue;
    }

    private string GetStringFromObject(Object ObjectToString)
    {
      if (ObjectToString != null)
      {
        return ObjectToString.ToString();
      }
      else
      {
        return "";
      }
    }

    //private async Task<Tuple<List<int>, int, string>> TelegramBotGetUpdatesAsync(string BotToken, int BotUpdateId)
    //{
    //  #region Define vars
    //  Tuple<List<int>, int, string> returnValue;
    //  List<int> ListChatId;
    //  int botChatId;
    //  #endregion Define vars

    //  #region Try
    //  try
    //  {
    //    using (var webClient = new WebClient())
    //    {
    //      //string response = await webClient.DownloadStringTaskAsync(address: "https://api.telegram.org/bot" + BotToken + "/getUpdates" + "?offset=" + BotUpdateId.ToString());          
    //      string response = await webClient.DownloadStringTaskAsync(address: "https://api.telegram.org/bot" + BotToken + "/getUpdates" + "?limit=1");
    //      var resultJson = JObject.Parse(response);
    //      if (bool.Parse(resultJson["ok"].ToString()))
    //      {
    //        ListChatId = new List<int>();
    //        foreach (var item in resultJson["result"])
    //        {
    //          BotUpdateId = Convert.ToInt32(item["update_id"].ToString());
    //          botChatId = Convert.ToInt32(item["message"]["chat"]["id"].ToString());

    //          if (!ListChatId.Contains(botChatId)) ListChatId.Add(botChatId);
    //        }
    //        returnValue = new Tuple<List<int>, int, string>(ListChatId, BotUpdateId, "");
    //      }
    //      else
    //      {
    //        returnValue = new Tuple<List<int>, int, string>(null, 0, "error");
    //      }
    //    }
    //  }
    //  #endregion Try

    //  #region Catch
    //  catch (Exception ex)
    //  {
    //    returnValue = new Tuple<List<int>, int, string>(null, 0, ex.Message);
    //  }
    //  #endregion Catch

    //  return returnValue;
    //}

    private async Task<string> TelegramBotSendMessageAsync(string TelegramBotToken, string TelegramBotMessage, int TelegramBotChatId, JObject ReplyKeyboardMarkup = null, int ReplyToMessageId = 0)
    {
      string returnValue;

      using (var webClient = new WebClient())
      {
        #region Try
        try
        {
          var pars = new NameValueCollection();
          pars.Add("text", TelegramBotMessage);
          pars.Add("chat_id", TelegramBotChatId.ToString());
          if (ReplyKeyboardMarkup != null)
          {
            pars.Add("reply_markup", ReplyKeyboardMarkup.ToString());

            if (ReplyToMessageId > 0)
            {
              pars.Add("reply_to_message_id", ReplyToMessageId.ToString());
            }
          }
          var response = await webClient.UploadValuesTaskAsync("https://api.telegram.org/bot" + TelegramBotToken + "/sendMessage", pars);

          //var resultJson = JObject.Parse(response);
          //foreach (var item in resultJson)
          //{

          //}
          returnValue = "";
        }
        #endregion Try

        #region Catch
        catch (Exception ex)
        {
          returnValue = ex.Message;
        }
        #endregion Catch
      }
      return returnValue;
    }

    private void TelegramBotCheckState()
    {
      if (tsTelegramBot.IsOn)
      {
        if (timerTelegram.IsEnabled) timerTelegram.Stop();

        txtTelegramBotDashboardName.IsEnabled = true;
        txtTelegramBotToken.IsEnabled = true;
        lvTelegramBotList.IsEnabled = true;
        cmdTelegramBotAddUser.IsEnabled = true;
        cmdTelegramBotDeleteUser.IsEnabled = true;
        txtTelegramBotPhone.Visibility = Visibility.Visible;
        lblTelegramBotPhoneError.Visibility = Visibility.Collapsed;

        if (!string.IsNullOrEmpty(txtTelegramBotDashboardName.Text))
          lblTelegramBotDashboardNameError.Visibility = Visibility.Collapsed;
        else
          lblTelegramBotDashboardNameError.Visibility = Visibility.Visible;

        if (!string.IsNullOrEmpty(txtTelegramBotToken.Text))
          lblTelegramBotTokenError.Visibility = Visibility.Collapsed;
        else
          lblTelegramBotTokenError.Visibility = Visibility.Visible;

        //SetListTelegramChat();

        //if (SharedClass.ListTelegramBot.Count > 0)
        //{
        //  lblTelegramBotAllowError.Visibility = Visibility.Collapsed;
        //}
        //else
        //{
        //  lblTelegramBotAllowError.Visibility = Visibility.Visible;
        //}

        if (!string.IsNullOrEmpty(txtTelegramBotDashboardName.Text) && !string.IsNullOrEmpty(txtTelegramBotToken.Text) && SharedClass.ListTelegramBot.Count > 0)
        {
          timerTelegram.Start();
        }
      }
      else
      {
        if (timerTelegram.IsEnabled) timerTelegram.Stop();

        txtTelegramBotDashboardName.IsEnabled = false;
        lblTelegramBotDashboardNameError.Visibility = Visibility.Collapsed;

        txtTelegramBotToken.IsEnabled = false;
        lblTelegramBotTokenError.Visibility = Visibility.Collapsed;

        lvTelegramBotList.IsEnabled = false;
        lblTelegramBotPhoneError.Visibility = Visibility.Collapsed;

        cmdTelegramBotAddUser.IsEnabled = false;
        cmdTelegramBotDeleteUser.IsEnabled = false;
        txtTelegramBotPhone.Visibility = Visibility.Collapsed;
        lblTelegramBotPhoneError.Visibility = Visibility.Collapsed;

        txtTelegramBotCount.Text = "0";
      }
    }

    private void tsTelegramBot_Toggled(object sender, RoutedEventArgs e)
    {
      SharedClass.IsTelegramBot = tsTelegramBot.IsOn;
      TelegramBotCheckState();
    }

    private void txtTelegramBotDashboardName_TextChanged(object sender, TextChangedEventArgs e)
    {
      (sender as TextBox).Text = (sender as TextBox).Text.Trim();
      SharedClass.TelegramBotDashboardName = (sender as TextBox).Text;
      TelegramBotCheckState();
    }

    private void txtTelegramBotDashboardName_LostFocus(object sender, RoutedEventArgs e)
    {
      TelegramBotCheckState();
    }

    private void txtTelegramBotToken_TextChanged(object sender, TextChangedEventArgs e)
    {
      (sender as TextBox).Text = (sender as TextBox).Text.Trim();
      SharedClass.TelegramBotToken = (sender as TextBox).Text;
      TelegramBotCheckState();
    }

    private void txtTelegramBotToken_LostFocus(object sender, RoutedEventArgs e)
    {
      TelegramBotCheckState();
    }

    private void txtTelegramBotPassword_TextChanged(object sender, TextChangedEventArgs e)
    {
      (sender as TextBox).Text = (sender as TextBox).Text.Trim();
      SharedClass.TelegramBotPassword = (sender as TextBox).Text;
      TelegramBotCheckState();
    }

    private void txtTelegramBotPassword_LostFocus(object sender, RoutedEventArgs e)
    {
      TelegramBotCheckState();
    }

    // private void SetListTelegramChat()
    //{
    //   var aTelegramBotAllow = txtTelegramBotAllow.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    //   ListTelegramChat.Clear();
    //   for (int i = 0; i < aTelegramBotAllow.Length; i++)
    //   {
    //     ListTelegramChat.Add(new TelegramBotObject { PhoneNumber = aTelegramBotAllow[i].Trim() });
    //   }
    // }

    private void txtTelegramBotAllow_TextChanged(object sender, TextChangedEventArgs e)
    {
      SharedClass.TelegramBotAllow = (sender as TextBox).Text;
      TelegramBotCheckState();
    }

    private void txtTelegramBotAllow_LostFocus(object sender, RoutedEventArgs e)
    {
      TelegramBotCheckState();
    }

    private async void cmdTelegramBotAddUser_Click(object sender, RoutedEventArgs e)
    {
      string PhoneNumber = txtTelegramBotPhone.Text.Trim();
      if (!string.IsNullOrEmpty(PhoneNumber))
      {
        var foundTelegramBotObject = SharedClass.ListTelegramBot.FirstOrDefault(item => item.PhoneNumber == PhoneNumber);
        if (foundTelegramBotObject == null)
        {
          SharedClass.TelegramBotSave(PhoneNumber);
          lvTelegramBotList.ItemsSource = SharedClass.ListTelegramBot;
        }
        else
        {
          await ShowDialog(SharedClass.TelegramBot_AddUser);
        }

        txtTelegramBotPhone.Text = "";
      }
      else
      {
        lblTelegramBotPhoneError.Visibility = Visibility.Visible;
      }
    }

    private async void cmdTelegramBotDeleteUser_Click(object sender, RoutedEventArgs e)
    {
      var SelectedTelegramBot = lvTelegramBotList.SelectedItem as TelegramBotObject;
      if (SelectedTelegramBot != null)
      {
        if (await ShowDialogYesNo(SharedClass.TelegramBot_DeleteUser))
        {
          SharedClass.TelegramBotRemove(SelectedTelegramBot.PhoneNumber);
          lvTelegramBotList.ItemsSource = SharedClass.ListTelegramBot;
        }
      }

      TelegramBotCheckState();
    }

    private void txtTelegramBotPhone_TextChanged(object sender, TextChangedEventArgs e)
    {
      TelegramBotCheckState();
    }

    #endregion Telegram
    #endregion PivotItemNotification

    #region PivotItemLogin
    private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
    {
      ButtonCheckLogin();
    }

    private void pswPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
      ButtonCheckLogin();
    }

    private void txtCaptcha_TextChanged(object sender, TextChangedEventArgs e)
    {
      var txtCaptcha = (sender as TextBox);
      if (string.IsNullOrEmpty(txtCaptcha.Text))
      {
        var LoginId = txtCaptcha.Tag.ToString().Trim();
        var imgCaptcha = (FindName("imgCaptcha" + LoginId) as Image).Source = null;
      }

      ButtonCheckLogin();
    }

    private void cmdLogin_Click(object sender, RoutedEventArgs e)
    {
      var LoginId = (e.OriginalSource as Button).Tag.ToString().Trim();
      Login(LoginId);
      //ButtonCheckLogin();
    }

    private async void Login(string LoginId)
    {
      try
      {
        var cbMining = FindName("cbMining" + LoginId) as ComboBox;
        var wvCaptcha = FindName("wvCaptcha" + LoginId) as WebView;

        SimpleMiningCountLogin = 0; SimpleMiningCountMain = 0;

        #region Cookies
        var baseFilter = new HttpBaseProtocolFilter();
        var cookies = baseFilter.CookieManager.GetCookies(SharedLibraryDashboardMining.GetSimpleMiningNetURIMain());

        var cookiePHPSESSID = cookies.FirstOrDefault(cookie => cookie.Name == "PHPSESSID");
        if (cookiePHPSESSID == null) cookiePHPSESSID = new HttpCookie("PHPSESSID", "simplemining.net", "/");
        cookiePHPSESSID.Value = SharedLibraryDashboardMining.GetSimpleMining_PHPSESSID();
        baseFilter.CookieManager.SetCookie(cookiePHPSESSID);

        var cookie__cfduid = cookies.FirstOrDefault(cookie => cookie.Name == "__cfduid");
        if (cookie__cfduid == null) cookie__cfduid = new HttpCookie("__cfduid", "simplemining.net", "/");
        cookie__cfduid.Value = SharedLibraryDashboardMining.GetSimpleMining_cfduid();
        baseFilter.CookieManager.SetCookie(cookie__cfduid);

        var cookie__cflb = cookies.FirstOrDefault(cookie => cookie.Name == "__cflb");
        if (cookie__cflb == null) cookie__cflb = new HttpCookie("__cflb", "simplemining.net", "/");
        cookie__cflb.Value = SharedLibraryDashboardMining.GetSimpleMining_cflb();
        baseFilter.CookieManager.SetCookie(cookie__cflb);

        SharedClass.SaveLoginCookie(LoginId, (cbMining.SelectedItem as MiningObject).Mining, cookie__cfduid.Value, cookie__cflb.Value, cookiePHPSESSID.Value);
        #endregion Cookies

        #region Load Login page
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, SharedLibraryDashboardMining.GetSimpleMiningNetURILogin());
        httpRequestMessage.Headers.Append("Referer", SharedLibraryDashboardMining.GetSimpleMiningNetMain());
        wvCaptcha.NavigateWithHttpRequestMessage(httpRequestMessage);
        #endregion Load Login page

        wvCaptcha.Visibility = Visibility.Visible;
      }
      catch (Exception ex)
      {
        await ShowDialog("Login: "+ex.ToString());
      }
    }

    #region Web View SimpeMining.net
    private void WvCaptcha_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
    {
      //Debug.WriteLine(DateTime.Now.ToString() + " " + "WvCaptcha1_NewWindowRequested. " + args.Uri.ToString());

      //Не переходить на внешние ссылки
      args.Handled = true;
    }

    private void NavigateWithHeader(Uri uri)
    {
      //var requestMsg = new HttpRequestMessage(HttpMethod.Get, uri);
      //requestMsg.Headers.Add("User-Agent", "blahblah");
      //wvCaptcha1.NavigateWithHttpRequestMessage(requestMsg);

      //wvCaptcha1.NavigationStarting += Wb_NavigationStarting;
    }

    private void Wb_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
    {
      if (args.Uri == SharedLibraryDashboardMining.GetSimpleMiningNetURILogin())
      {
        SimpleMiningCountLogin++;
        //Debug.WriteLine(DateTime.Now.ToString() + " " + "Wb_NavigationStarting. " + "https://simplemining.net/account/login");
      }
      else if (args.Uri == SharedLibraryDashboardMining.GetSimpleMiningNetURIMain())
      {
        SimpleMiningCountMain++;
        //Debug.WriteLine(DateTime.Now.ToString() + " " + "Wb_NavigationStarting. " + "https://simplemining.net/");
      }
      else
      {
        //Debug.WriteLine(DateTime.Now.ToString() + " " + "Wb_NavigationStarting. " + "Some URI:" + args.Uri.ToString());
        args.Cancel = true;
      }

      //wvCaptcha1.NavigationStarting -= Wb_NavigationStarting;
      //args.Cancel = true;
      //NavigateWithHeader(args.Uri);
    }

    private void WvCaptcha_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
    {
      if (args.Uri == new Uri("https://simplemining.net/account/login"))
      {
        SimpleMiningCountLogin++;
      }
      else if (args.Uri == new Uri("https://simplemining.net/"))
      {
        SimpleMiningCountMain++;
      }

      //Debug.WriteLine(DateTime.Now.ToString() + " " + "WvCaptcha1_DOMContentLoaded. " + args.Uri.ToString());
    }

    private void WvCaptcha_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
    {
      if (args.Uri == SharedLibraryDashboardMining.GetSimpleMiningNetURILogin())
      {
        if (SimpleMiningCountLogin == 2 && SimpleMiningCountMain == 0) // Login page loaded
        {
          SimpleMiningCountLogin = 0; SimpleMiningCountMain = 0;
        }
        if (SimpleMiningCountLogin == 3 && SimpleMiningCountMain == 0) // Login page is failed check login
        {
          SimpleMiningCountLogin = 0; SimpleMiningCountMain = 0;
        }
      }

      if (args.Uri == SharedLibraryDashboardMining.GetSimpleMiningNetURIMain())
      {
        var LoginId = sender.Tag.ToString().Trim();
        var cbMining = FindName("cbMining" + LoginId) as ComboBox;
        var txtLogin = FindName("txtLogin" + LoginId) as TextBox;
        var pswPassword = FindName("pswPassword" + LoginId) as PasswordBox;
        var wvCaptcha = FindName("wvCaptcha" + LoginId) as WebView;

        //WebView wvCaptcha = spWebView.FindName("wvCaptcha") as WebView;        

        if (SimpleMiningCountLogin == 1 && SimpleMiningCountMain == 2) //Login page is OK
        {
          SimpleMiningCountLogin = 0; SimpleMiningCountMain = 0;
          wvCaptcha.Visibility = Visibility.Collapsed;
          SharedClass.SaveLogin(LoginId, (cbMining.SelectedItem as MiningObject).Mining, txtLogin.Text.Trim(), pswPassword.Password.Trim(), "");
          ButtonCheckLogin();
          //spWebView.Children.Remove(wvCaptcha);
          //wvCaptcha = null;
        }
        if (SimpleMiningCountLogin == 0 && SimpleMiningCountMain == 2) //Block change page from Login page
        {
          SimpleMiningCountLogin = 0; SimpleMiningCountMain = 0;
          wvCaptcha.Navigate(SharedLibraryDashboardMining.GetSimpleMiningNetURILogin());
        }
      }

      //Debug.WriteLine(DateTime.Now.ToString() + " " + "WvCaptcha1_NavigationCompleted. " + args.Uri.ToString());
    }

    #endregion Web View SimpeMining.net

    private async void cmdLogout_Click(object sender, RoutedEventArgs e)
    {
      var LoginId = (e.OriginalSource as Button).Tag.ToString().Trim();
      await Logout(LoginId);
      ButtonCheckLogin();
    }

    private async Task Logout(string LoginId)
    {
      var foundLogin = SharedClass.ListLoginMining.First(item => item.Id == LoginId);

      #region Set controls
      var cmdLogout = (FindName("cmdLogout" + LoginId) as Button);
      #endregion Set controls

      cmdLogout.IsEnabled = false;

      var resultLogout = await SharedLibraryDashboardMining.LogoutAsync((int)foundLogin.Mining, foundLogin.SimpleMining_cfduid, foundLogin.SimpleMining_cflb, foundLogin.SimpleMining_PHPSESSID);
      if (string.IsNullOrEmpty(resultLogout))
      {
        SharedClass.SaveLogout(LoginId);

        lock (lockListRigs)
        {
          int countListReg = ListRig.Count; SandboxObject foundSandboxObject;
          for (int i = countListReg - 1; i >= 0; i--)
          {
            if (ListRig[i].Login == foundLogin.Login)
            {
              foundSandboxObject = ListSandbox.FirstOrDefault(itemSandbox => itemSandbox.id == ListRig[i].id);
              if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

              ListRig.Remove(ListRig[i]);
              countListReg = ListRig.Count;
            }
          }

          txtCountRigs.Text = ListRig.Count.ToString();
        }

      }
      else
      {
        var ErrorMessage = SharedClass.Error_Logout + ": " + resultLogout;
        WriteError(ErrorMessage);
        await ShowDialog(ErrorMessage);
      }
    }

    #endregion PivotItemLogin

    private void lvSandbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      //if (e.AddedItems.Count > 0 && ListRig.Count > 0)
      //{
      //  lock (lockListRigs)
      //  {
      //    //var ItemErrorObject = e.AddedItems[0] as ErrorObject;
      //    //gvRigs.SelectedItem = ListRig.First(item => item.id == ItemErrorObject.id);
      //    //gvRigs.ScrollIntoView(gvRigs.SelectedItem, ScrollIntoViewAlignment.Leading);
      //  }
      //}
    }

    #region Utilites
    private async Task ShowDialog(string ShowMessage)
    {
      var showDialog = new ContentDialog()
      {
        Content = ShowMessage,
        PrimaryButtonText = "OK"
        //SecondaryButtonText = SharedClass.SettingsHistoryClearDialogSecondaryButtonText //Нет
      };
      var result = await showDialog.ShowAsync();
    }

    private async Task<bool> ShowDialogYesNo(string ShowMessage)
    {
      var showDialog = new ContentDialog()
      {
        Content = ShowMessage,
        PrimaryButtonText = SharedClass.Dialog_Yes,
        SecondaryButtonText = SharedClass.Dialog_No
      };
      var result = await showDialog.ShowAsync();
      if (result == ContentDialogResult.Primary)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    private void WriteError(string Error)
    {
      try
      {
        tbError.Text = tbError.Text.Insert(0, Environment.NewLine);
        tbError.Text = tbError.Text.Insert(0, string.Format("{0} {1}", DateTime.Now, Error));
      }
      catch (Exception ex)
      {
#if DEBUG
        tbError.Text = string.Format("{0} {1}", DateTime.Now, "Dashboard Mining cleared the list of errors due to a failure: " + ex.ToString());
#else
        tbError.Text = string.Format("{0} {1}", DateTime.Now, "Dashboard Mining cleared the list of errors due to a failure");
#endif

      }
    }
    #endregion Utilites
  }
}

/*
    private async Task CheckRigs()
    {
      try
      {
        RigObject foundRigObject; SandboxObject foundSandboxObject;
        string details; bool IsRigOn;

#region Fill ListSandbox
        foreach (var rig in ListRig)
        {
#region State
          foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.FarmNotAvailable);

          if (rig.state.ToLower() == "on") //Параметр в норме
          {
            IsRigOn = true;
            if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
          }
          else // OFF - Выход параметра за передельное значение
          {
            IsRigOn = false;

#region Remove all records from ListSandbox for Rig in OFF
            int countListSandbox = ListSandbox.Count;
            for (int i = countListSandbox - 1; i >= 0; i--)
            {
              if (ListSandbox[i].id == rig.id && ListSandbox[i].FarmProblem != EnumFarmProblem.FarmNotAvailable)
              {
                ListSandbox.Remove(ListSandbox[i]);
                countListSandbox = ListSandbox.Count;
              }
            }
#endregion Remove all records from ListSandbox for Rig in OFF

            if (foundSandboxObject == null) ListSandbox.Add(InitSandboxObject(rig, "", EnumFarmProblem.FarmNotAvailable));
          }
#endregion State

#region Speed

          if (IsRigOn) //Farm is available (ON)
          {
            foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.NoVideocardSpeedInfo);
            if (rig.speed.Count() > 0 && rig.speed.Sum() > 0)
            {
              if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.TheVideocardIsRunningSlowly);

              if (rig.speed.Min() >= SharedClass.SettingsSpeedSlow) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList(rig.speed.Sum().ToString(), rig.speed, SharedClass.SettingsSpeedSlow, null);

                if (foundSandboxObject == null)
                  ListSandbox.Add(InitSandboxObject(rig, details, EnumFarmProblem.TheVideocardIsRunningSlowly));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
            }
            else
            {
              if (foundSandboxObject == null) ListSandbox.Add(InitSandboxObject(rig, "", EnumFarmProblem.NoVideocardSpeedInfo));
            }
          }

#endregion Speed

#region Temps
          if (IsRigOn) //Farm is available (ON)
          {
            foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.NoVideocardTemperatureInformation);
            if (rig.temps.Count() > 0 && rig.temps.Sum() > 0)
            {
              if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

#region Температура видеокарты низкая
              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.VideocardTemperatureLow);
              if (rig.temps.Min() >= SharedClass.SettingsTempMin) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList("", rig.temps, SharedClass.SettingsTempMin, null);

                if (foundSandboxObject == null)
                  ListSandbox.Add(InitSandboxObject(rig, details, EnumFarmProblem.VideocardTemperatureLow));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
#endregion Температура видеокарты низкая

#region Температура видеокарты высокая
              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.VideocardTemperatureHigh);
              if (rig.temps.Max() <= SharedClass.SettingsTempMax) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList("", rig.temps, null, SharedClass.SettingsTempMax);

                if (foundSandboxObject == null)
                  ListSandbox.Add(InitSandboxObject(rig, details, EnumFarmProblem.VideocardTemperatureHigh));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
#endregion Температура видеокарты высокая
            }
            else
            {
              if (foundSandboxObject == null) ListSandbox.Add(InitSandboxObject(rig, "", EnumFarmProblem.NoVideocardTemperatureInformation));
            }
          }

#endregion Temps

#region Cooler
          if (IsRigOn) //Farm is available (ON)
          {
            foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.NoFanDataOnVideocards);
            if (rig.cooler.Count() > 0)
            {
              if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

#region Вентилятор видеокарты крутится слабо
              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.TheFanOnVideocardIsWeak);
              if (rig.cooler.Min() >= SharedClass.SettingsFanMin) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList("", rig.cooler, SharedClass.SettingsFanMin, null);

                if (foundSandboxObject == null)
                  ListSandbox.Add(InitSandboxObject(rig, details, EnumFarmProblem.TheFanOnVideocardIsWeak));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
#endregion Кулер видеокарты слабо крутит

#region Вентилятор видеокарты крутится сильно
              foundSandboxObject = ListSandbox.FirstOrDefault(item => item.id == rig.id && item.FarmProblem == EnumFarmProblem.TheFanOnVideocardSpinsHard);
              if (rig.cooler.Max() <= SharedClass.SettingsFanMax) //Параметр в норме
              {
                if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);
              }
              else //Выход параметра за передельное значение
              {
                details = GetStringFromList("", rig.cooler, null, SharedClass.SettingsFanMax);

                if (foundSandboxObject == null)
                  ListSandbox.Add(InitSandboxObject(rig, details, EnumFarmProblem.TheFanOnVideocardSpinsHard));
                else
                  if (foundSandboxObject.details != details) foundSandboxObject.details = details;
              }
#endregion Вентилятор видеокарты крутится сильно
            }
            else
            {
              if (foundSandboxObject == null) ListSandbox.Add(InitSandboxObject(rig, "", EnumFarmProblem.NoFanDataOnVideocards));
            }
          }

#endregion Cooler           
        }
#endregion Fill ListSandbox

#region Filter ListSandbox

#region Parameters
        var TheFarmDoesNotWork = new TimeSpan(0, SharedClass.SettingsRigDuring, 0);
        var TheVideocardDoesNotWork = new TimeSpan(0, Convert.ToInt32(txtTheVideocardDoesNotWorkDuring.Text), 0);
        var SpeedSlowDuring = new TimeSpan(0, Convert.ToInt32(txtSpeedSlowDuring.Text), 0);
        var TempMinDuring = new TimeSpan(0, Convert.ToInt32(txtTempMinDuring.Text), 0);
        var TempMaxDuring = new TimeSpan(0, Convert.ToInt32(txtTempMaxDuring.Text), 0);
        var FanMinDuring = new TimeSpan(0, Convert.ToInt32(txtFanMinDuring.Text), 0);
        var FanMaxDuring = new TimeSpan(0, Convert.ToInt32(txtFanMaxDuring.Text), 0);
#endregion Parameters

#region Query
        var tempListSandbox = ListSandbox.Where(item =>
          (item.during >= TheFarmDoesNotWork && item.FarmProblem == EnumFarmProblem.FarmNotAvailable) ||
          (item.during >= TheFarmDoesNotWork && item.FarmProblem == EnumFarmProblem.NoVideocardSpeedInfo) || //Взято время TheFarmDoesNotWork
          (item.during >= TheVideocardDoesNotWork && item.FarmProblem == EnumFarmProblem.TheVideocardDoesNotWorkFor) ||
          (item.during >= SpeedSlowDuring && item.FarmProblem == EnumFarmProblem.TheVideocardIsRunningSlowly) ||
          (item.during >= TheFarmDoesNotWork && item.FarmProblem == EnumFarmProblem.NoVideocardTemperatureInformation) || //Взято время TheFarmDoesNotWork
          (item.during >= TempMinDuring && item.FarmProblem == EnumFarmProblem.VideocardTemperatureLow) ||
          (item.during >= TempMaxDuring && item.FarmProblem == EnumFarmProblem.VideocardTemperatureHigh) ||
          (item.during >= TheFarmDoesNotWork && item.FarmProblem == EnumFarmProblem.NoFanDataOnVideocards) || //Взято время TheFarmDoesNotWork
          (item.during >= FanMinDuring && item.FarmProblem == EnumFarmProblem.TheFanOnVideocardIsWeak) ||
          (item.during >= FanMaxDuring && item.FarmProblem == EnumFarmProblem.TheFanOnVideocardSpinsHard)).
        Join(ListRig, first => first.id, second => second.id, (first, second) => new {
          id = first.id,
          group = second.group,
          name = second.name,
          FarmProblem = first.FarmProblem,
          FarmProblemShow = first.FarmProblemShow,
          during = first.during,
          duringShow = first.duringShow,
          ip = second.ip,
          details = first.details
        }).
        OrderBy(item1 => item1.during.Days).
        ThenBy(item2 => item2.during.Hours).
        ThenBy(item3 => item3.during.Minutes).
        ThenBy(item4 => item4.during.Seconds).
        ThenBy(item5 => item5.name);
#endregion Query

#endregion Filter ListSandbox

#region Triggers
        // Copy list
        //var ListLoginMining = SharedClass.ListLoginMining;

        var CheckNotification = new TimeSpan(SharedClass.SettingsCheckNotification, 0, 0);
        var CheckRestart = new TimeSpan(0, Convert.ToInt32(sliderCheckRestart.Value), 0);

        EventObject foundEventObject; string BotMessage; StringBuilder TelegramBotMessage = new StringBuilder();
        foreach (var item in tempListSandbox)
        {
          foundRigObject = ListRig.First(itemRig => itemRig.id == item.id);
          foundSandboxObject = ListSandbox.First(itemSandbox => itemSandbox.id == item.id && itemSandbox.FarmProblem == item.FarmProblem);
          foundEventObject = SharedClass.ListEvent.OrderByDescending(itemEvent => itemEvent.datetime).FirstOrDefault(itemEvent2 => itemEvent2.id == item.id && itemEvent2.FarmProblem == item.FarmProblem);

#region Swith farm problem
          bool IsSendToTelegramBot; EnumFarmAction FarmAction;
          switch (item.FarmProblem)
          {
            case EnumFarmProblem.FarmNotAvailable:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionRigDuring, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionRigDuring;
              break;
            case EnumFarmProblem.NoVideocardSpeedInfo:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionNoVideocardSpeedInfo, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionNoVideocardSpeedInfo;
              break;
            case EnumFarmProblem.TheVideocardDoesNotWorkFor:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionTheVideocardDoesNotWorkFor, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionTheVideocardDoesNotWorkFor;
              break;
            case EnumFarmProblem.TheVideocardIsRunningSlowly:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionTheVideocardIsRunningSlowly, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionTheVideocardIsRunningSlowly;
              break;
            case EnumFarmProblem.NoVideocardTemperatureInformation:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionNoVideocardTemperatureInformation, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionNoVideocardTemperatureInformation;
              break;
            case EnumFarmProblem.VideocardTemperatureLow:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionVideocardTemperatureLow, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionVideocardTemperatureLow;
              break;
            case EnumFarmProblem.VideocardTemperatureHigh:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionVideocardTemperatureHigh, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionVideocardTemperatureHigh;
              break;
            case EnumFarmProblem.NoFanDataOnVideocards:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionNoFanDataOnVideocards, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionNoFanDataOnVideocards;
              break;
            case EnumFarmProblem.TheFanOnVideocardIsWeak:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionTheFanOnVideocardIsWeak, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionTheFanOnVideocardIsWeak;
              break;
            case EnumFarmProblem.TheFanOnVideocardSpinsHard:
              IsSendToTelegramBot = await DoAction(SharedClass.ActionTheFanOnVideocardSpinsHard, foundRigObject, foundSandboxObject, foundEventObject, CheckNotification, CheckRestart);
              FarmAction = SharedClass.ActionTheFanOnVideocardSpinsHard;
              break;
            default:
              IsSendToTelegramBot = false; FarmAction = EnumFarmAction.TakeNoAction;
              break;
          }

          if (IsSendToTelegramBot)
          {
            if (!string.IsNullOrEmpty(foundSandboxObject.details.Trim()))
            {
              BotMessage = string.Format(SharedClass.Notification_Format1, SharedClass.TelegramBotDashboardName, foundRigObject.group, foundRigObject.name, foundSandboxObject.FarmProblemShow, foundSandboxObject.duringShow, SharedClass.GetFarmAction(FarmAction), foundRigObject.ip, foundSandboxObject.details);
            }
            else
            {
              BotMessage = string.Format(SharedClass.Notification_Format3, SharedClass.TelegramBotDashboardName, foundRigObject.group, foundRigObject.name, foundSandboxObject.FarmProblemShow, foundSandboxObject.duringShow, SharedClass.GetFarmAction(FarmAction), foundRigObject.ip);
            }

            TelegramBotMessage.Append(BotMessage);
            TelegramBotMessage.AppendLine();
          }
#endregion Swith farm problem
        }
        if (TelegramBotMessage.Length > 0)
        {
          await TelegramBotMessageToAllAsync(TelegramBotMessage.ToString());
        }

        lvEvent.ItemsSource = SharedClass.ListEvent.OrderByDescending(item => item.datetime);

        //var query = from x in list where x.Name = yourcondition select new { x };
        //foreach (var item in query) item.x.FieldToUpdate = SetValue;

#region Clear some lists
        //ListLoginMining.Clear(); ListLoginMining = null;
#endregion Clear some lists

#endregion Triggers

        lvSandbox.ItemsSource = tempListSandbox;
      }
      catch (Exception ex)
      {
#if DEBUG
        await ShowDialog(SharedClass.Main_Rigs_Error_Check + ": " + ex.ToString());
#else
        await ShowDialog(SharedClass.Main_Rigs_Error_Check + ": " + ex.Message);
#endif
      }

      SandboxObject InitSandboxObject(RigObject rigObject, string details, EnumFarmProblem FarmProblem)
      {
        DateTime startDateTime;

        if (FarmProblem == EnumFarmProblem.FarmNotAvailable)
        {
          startDateTime = rigObject.LastUpdate;
        }
        else
        {
          startDateTime = DateTime.Now;
        }

        var sandboxObject = new SandboxObject
        {
          id = rigObject.id,
          StartDateTime = startDateTime,
          FarmProblem = FarmProblem,
          details = details,
          LastNotification = DateTime.MinValue
        };

        return sandboxObject;
      }
    } 
  
     private async void cmdLogin_Click(object sender, RoutedEventArgs e)
    {
      var LoginId = (e.OriginalSource as Button).Tag.ToString().Trim();

      var cbMining = FindName("cbMining" + LoginId) as ComboBox;
      var txtLogin = FindName("txtLogin" + LoginId) as TextBox;
      var pswPassword = FindName("pswPassword" + LoginId) as PasswordBox;
      var txtCaptcha = FindName("txtCaptcha" + LoginId) as TextBox;
      var cmdLogin = (FindName("cmdLogin" + LoginId) as Button);

      cmdLogin.IsEnabled = false;

      var foundLoginMining = SharedClass.ListLoginMining.FirstOrDefault(item => item.Id == LoginId);
      if (foundLoginMining != null)
      {
        var resultLogin = await SharedLibraryDashboardMining.LoginAsync((int)(cbMining.SelectedItem as MiningObject).Mining, txtLogin.Text.Trim(), pswPassword.Password.Trim(), txtCaptcha.Text.Trim(), foundLoginMining.SimpleMining_cfduid, foundLoginMining.SimpleMining_cflb, foundLoginMining.SimpleMining_PHPSESSID);
        if (string.IsNullOrEmpty(resultLogin))
        {
          SharedClass.SaveLogin(LoginId, (cbMining.SelectedItem as MiningObject).Mining, txtLogin.Text.Trim(), pswPassword.Password.Trim(), txtCaptcha.Text.Trim());
        }
        else
        {
          await ShowDialog("Error login: " + resultLogin);

          var imgCaptcha = (FindName("imgCaptcha" + LoginId) as Image).Source = null;
          txtCaptcha.Text = "";
        }

        ButtonCheckLogin();
      }
    }

    private async void cmdLogout_Click(object sender, RoutedEventArgs e)
    {
      var LoginId = (e.OriginalSource as Button).Tag.ToString().Trim();
      var foundLogin = SharedClass.ListLoginMining.First(item => item.Id == LoginId);

      var cmdLogout = (FindName("cmdLogout" + LoginId) as Button);
      cmdLogout.IsEnabled = false;

      var resultLogout = await SharedLibraryDashboardMining.LogoutAsync((int)foundLogin.Mining, foundLogin.SimpleMining_cfduid, foundLogin.SimpleMining_cflb, foundLogin.SimpleMining_PHPSESSID);
      if (string.IsNullOrEmpty(resultLogout))
      {
        SharedClass.SaveLogout(LoginId);
        var imgCaptcha = (FindName("imgCaptcha" + LoginId) as Image).Source = null;
        var txtCaptcha = (FindName("txtCaptcha" + LoginId) as TextBox).Text = "";

        lock (lockListRigs)
        {
          int countListReg = ListRig.Count;
          for (int i = countListReg - 1; i >= 0; i--)
          {
            if (ListRig[i].Login == foundLogin.Login)
            {
              ListRig.Remove(ListRig[i]);
              countListReg = ListRig.Count;
            }
          }

          txtCountRigs.Text = ListRig.Count.ToString();
        }

        ButtonCheckLogin();
      }
      else
      {
        await ShowDialog("Error logout: " + resultLogout);
        cmdLogout.IsEnabled = true;
      }
    }


 Parallel List<Task<Tuple<JObject, string, string>>> ListTask = new List<Task<Tuple<JObject, string, string>>>();
#region Get mining result
ListTask.Clear();
      foreach (var LoginMiningItem in SharedClass.ListLoginMining)
      {
        if (LoginMiningItem.IsLogged)
        {
          var newTask = Task.Run(async () =>
          {
            JObject resultJsonTask; string errorMessage;

            var resultListRigs = await SharedLibraryDashboardMining.GetListRigsAsync(token, LoginMiningItem.SimpleMining_cfduid, LoginMiningItem.SimpleMining_PHPSESSID);

            if (string.IsNullOrEmpty(resultListRigs.Item2) && !string.IsNullOrEmpty(resultListRigs.Item1))
            {
              try
              {
                resultJsonTask = JObject.Parse(("{" + (char)34 + "rootrigs" + (char)34 + ":" + resultListRigs.Item1 + "}").Replace("  ", " "));
                errorMessage = "";
              }
              catch (Exception ex)
              {
                resultJsonTask = null;
                errorMessage = "Ошибка разбора списка ферм для " + LoginMiningItem.Login + " :" + ex.Message;
              }
            }
            else
            {
              resultJsonTask = null;
              errorMessage = "Ошибка получения списка ферм для " + LoginMiningItem.Login + " :" + resultListRigs.Item2;
            }

            return new Tuple<JObject, string, string>(resultJsonTask, LoginMiningItem.Login, errorMessage);
          }
          );
ListTask.Add(newTask);
        }
      }
      Task.WaitAll(ListTask.ToArray());
#endregion Get mining result

    private async Task TelegramBotGetListChatId()
    {
      try
      {
        var result = await TelegramBotGetUpdatesAsync(SharedClass.TelegramBotToken, SharedClass.TelegramBotUpdateId);
        if (string.IsNullOrEmpty(result.Item3))
        {
          var ListChatId = result.Item1;
          SharedClass.TelegramBotUpdateId = Convert.ToInt32(result.Item2);

          await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
          {
            txtTelegramBotCount.Text = ListChatId.Count().ToString();
          });
        }
      }
      catch (TaskCanceledException)
      {
        //Nothing TO DO - break the task
      }
      catch (Exception ex)
      {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        {
#if DEBUG
          await ShowDialog("Eror TelegramBotMessage: " + ex.ToString());
#else
          await ShowDialog("Error Telegram: " + ex.Message);
#endif
        });
      }
    }
*/

// Варианты работы с WebView
/*
       //var wvCaptcha = new WebView();
      //wvCaptcha.Name = "wvCaptcha";
      //wvCaptcha.Tag = LoginId;
      //wvCaptcha.Width = 500; wvCaptcha.Height = 540;
      //wvCaptcha.Visibility = Visibility.Visible;
      //spWebView.Children.Add(wvCaptcha);      

      //StringBuilder sb = new StringBuilder();
      //sb.Append("<html>");
      //sb.Append("<head>");
      ////sb.Append("<title>reCAPTCHA demo: Simple page</title>");
      ////sb.Append("<script>Object.defineProperty(document, 'referrer', {get : function(){ return 'https://simplemining.net/account/login'; }})</script>");
      ////sb.Append("<script>window.location='https://simplemining.net/account/login'</script>");
      ////sb.Append("<script>delete window.document.referrer; window.document.referrer = 'https://simplemining.net/account/login'</script>");
      //sb.Append("<script src='https://www.google.com/recaptcha/api.js?hl=en'></script>");
      //sb.Append("</head>");
      //sb.Append("<body>");
      //sb.Append("<form action='?' method='POST'>");
      //sb.Append("<div class='g-recaptcha' data-sitekey='6Le99SYUAAAAADlOs_6h1GCD5s3ZfnyX5bcjEA_z'></div><br/>");
      //sb.Append("<input type='submit' value='Submit'>");
      //sb.Append("</form>");
      //sb.Append("</body>");
      //sb.Append("</html>");
      //wvCaptcha1.NavigateToString(sb.ToString());

 
    //// start --------------------------------------------------------
    //// creating the filter
    //var myFilter = new HttpBaseProtocolFilter();
    //myFilter.AllowAutoRedirect = true;
    //myFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.Default;
    //myFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.Default;

    //// get a reference to the cookieManager (this applies to all requests)
    //var cookieManager = myFilter.CookieManager;

    //// make the httpRequest
    //using (var client = new HttpClient())
    //{
    //  HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://simplemining.net/account/login"));

    //  // add any request-specific headers here
    //  // more code been omitted

    //  var result = await client.SendRequestAsync(request);
    //  result.EnsureSuccessStatusCode();

    //  var content = await result.Content.ReadAsStringAsync();

    //  // now we can do whatever we need with the html content we got here 🙂
    //  // Debug.WriteLine(content);
    //}

    //// assuming that the previous request created a session (set cookies, cached some data, etc)
    //// subsequent requests in the webview will share this data
    ////wvCaptcha1.Navigate(new Uri("https://simplemining.net/account/login"));
    //// stop --------------------------------------------------------

    //string ScriptTagString = "<script lang=\"en-us\" type=\"text/javascript\">";
    //int IndexOfScriptTag = MyWebPageString.IndexOf(ScriptTagString);
    //int LengthOfScriptTag = ScriptTagString.Length;
    //string InsertionScriptString = "function SayHelloWorld() { window.external.notify(\"Hello World!\");} ";
    //MyWebPageString = MyWebPageString.Insert(IndexOfScriptTag + LengthOfScriptTag + 1, InsertionScriptString);

    //wvCaptcha1.InvokeScript("SayHelloWorld", null);

    //wvCaptcha1.Source = new Uri("https://simplemining.net/account/login");
    //wvCaptcha1.Navigate(new Uri("https://simplemining.net/account/login"));
    //wvCaptcha1.NavigateToString($"<html><head><script>window.location='https://simplemining.net/account/login'</script></head><body></body></html>");


    //string functionString = String.Format("document.getElementById('nameDiv').innerText = 'Hello, {0}';", nameTextBox.Text);
    //await webView1.InvokeScriptAsync("eval", new string[] { functionString });    
    //await wvCaptcha1.InvokeScriptAsync("eval", new string[] { sb.ToString() });

 */

/* Рабочий вариант cmdGetCaptcha_Click (до re-captcha)
private async void cmdGetCaptcha_Click(object sender, RoutedEventArgs e)
{
var LoginId = (e.OriginalSource as Button).Tag.ToString().Trim();

var cbMining = FindName("cbMining" + LoginId) as ComboBox;
var cmdGetCaptcha = FindName("cmdGetCaptcha" + LoginId) as Button;
var txtCaptcha = FindName("txtCaptcha" + LoginId) as TextBox;

cmdGetCaptcha.IsEnabled = false;

var resultLogin = await SharedLibraryDashboardMining.GetCaptchaAsync((int)(cbMining.SelectedItem as MiningObject).Mining);
if (string.IsNullOrEmpty(resultLogin.Item5))
{
  SharedClass.SaveLoginCookie(LoginId, (cbMining.SelectedItem as MiningObject).Mining, resultLogin.Item2, resultLogin.Item3, resultLogin.Item4);

  BitmapImage bitmapImage = new BitmapImage();
  bitmapImage.DecodePixelWidth = 60; //match the target Image.Width, not shown
  bitmapImage.DecodePixelHeight = 20; //match the target Image.Width, not shown
  MemoryStream ms = new MemoryStream(resultLogin.Item1);
  await bitmapImage.SetSourceAsync(ms.AsRandomAccessStream());

  var imgCaptcha = FindName("imgCaptcha" + LoginId) as Image;
  imgCaptcha.Source = bitmapImage;

  txtCaptcha.Text = "";
}
else
{
  var ErrorMessage = SharedClass.Error_Captcha + ": " + resultLogin.Item5;
  WriteError(ErrorMessage);
  await ShowDialog(ErrorMessage);
}

ButtonCheckLogin();
}
*/

/* Рабочий вариант ButtonCheckLogin() (до re-captcha)
  private void ButtonCheckLogin()
  {
    string LoginId;
    ComboBox cbMining; Image imgCaptcha; TextBox txtLogin, txtCaptcha; PasswordBox pswPassword; Button cmdGetCaptcha, cmdLogin, cmdLogout; TextBlock lblErrortxtLogin, lblErrorpswPassword;

    for (int i = 0; i < SharedClass.ListLoginMining.Count; i++)
    {
      LoginId = i.ToString();

      #region Get controls
      cbMining = FindName("cbMining" + LoginId) as ComboBox;
      txtLogin = FindName("txtLogin" + LoginId) as TextBox;
      pswPassword = FindName("pswPassword" + LoginId) as PasswordBox;
      cmdGetCaptcha = FindName("cmdGetCaptcha" + LoginId) as Button;
      imgCaptcha = FindName("imgCaptcha" + i.ToString()) as Image;
      txtCaptcha = FindName("txtCaptcha" + LoginId) as TextBox;
      lblErrortxtLogin = FindName("lblErrortxtLogin" + LoginId) as TextBlock;
      lblErrorpswPassword = FindName("lblErrorpswPassword" + LoginId) as TextBlock;
      cmdLogin = FindName("cmdLogin" + LoginId) as Button;
      cmdLogout = FindName("cmdLogout" + LoginId) as Button;
      #endregion Get controls

      if (SharedClass.ListLoginMining[i].IsLogged)
      {
        cbMining.IsEnabled = false;
        txtLogin.IsEnabled = false;
        pswPassword.IsEnabled = false;
        cmdGetCaptcha.Visibility = Visibility.Visible;
        cmdGetCaptcha.IsEnabled = false;
        imgCaptcha.Visibility = Visibility.Collapsed;
        txtCaptcha.IsEnabled = false;
        cmdLogin.IsEnabled = false;
        cmdLogout.IsEnabled = true;

        lblErrortxtLogin.Visibility = Visibility.Collapsed;
        lblErrorpswPassword.Visibility = Visibility.Collapsed;
      }
      else
      {
        cbMining.IsEnabled = true;
        txtLogin.IsEnabled = true;
        pswPassword.IsEnabled = true;

        if (imgCaptcha.Source == null)
        {
          cmdGetCaptcha.Visibility = Visibility.Visible;
          cmdGetCaptcha.IsEnabled = true;
          imgCaptcha.Visibility = Visibility.Collapsed;
          txtCaptcha.IsEnabled = false;
        }
        else
        {
          cmdGetCaptcha.Visibility = Visibility.Collapsed;
          imgCaptcha.Visibility = Visibility.Visible;
          txtCaptcha.IsEnabled = true;
        }

        if (string.IsNullOrEmpty(txtLogin.Text)) lblErrortxtLogin.Visibility = Visibility.Visible; else lblErrortxtLogin.Visibility = Visibility.Collapsed;
        if (string.IsNullOrEmpty(pswPassword.Password)) lblErrorpswPassword.Visibility = Visibility.Visible; else lblErrorpswPassword.Visibility = Visibility.Collapsed;

        if (!string.IsNullOrEmpty(txtLogin.Text) && !string.IsNullOrEmpty(pswPassword.Password) && !string.IsNullOrEmpty(txtCaptcha.Text))
        {
          cmdLogin.IsEnabled = true; cmdLogout.IsEnabled = false;
        }
        else
        {
          cmdLogin.IsEnabled = false; cmdLogout.IsEnabled = false;
        }
      }

      ////For TEST ONLY
      //cbMining.IsEnabled = true;
      //txtLogin.IsEnabled = false;
      //pswPassword.IsEnabled = false;
      //cmdGetCaptcha.Visibility = Visibility.Visible;
      //cmdGetCaptcha.IsEnabled = true;
      //imgCaptcha.Visibility = Visibility.Collapsed;
      //txtCaptcha.IsEnabled = false;
      //cmdLogin.IsEnabled = true;
      //cmdLogout.IsEnabled = true;
    }

    if (SharedClass.ListLoginMining.All(item => !item.IsLogged))
    {
      StopCheckMining();
    }
    else
    {
      if (timer.IsEnabled)
      {
        cmdStart.IsEnabled = false; cmdStop.IsEnabled = true; cmdGetRigs.IsEnabled = false;
      }
      else
      {
        cmdStart.IsEnabled = true; cmdStop.IsEnabled = false; cmdGetRigs.IsEnabled = true;
      }
    }

    if (appLicense != null && !appLicense.IsActive)
    {
#if RELEASE
      StopCheckMining();
#endif
    }

    ////For TEST ONLY
    //cmdStart.IsEnabled = true; cmdStop.IsEnabled = true; cmdGetRigs.IsEnabled = true;
  }
*/

/* Рабочий вариант Login (до re-captcha)
private async Task Login(string LoginId)
{
#region Set controls
var cbMining = FindName("cbMining" + LoginId) as ComboBox;
var txtLogin = FindName("txtLogin" + LoginId) as TextBox;
var pswPassword = FindName("pswPassword" + LoginId) as PasswordBox;
var txtCaptcha = FindName("txtCaptcha" + LoginId) as TextBox;
var cmdLogin = (FindName("cmdLogin" + LoginId) as Button);
#endregion Set controls

cmdLogin.IsEnabled = false;

var foundLoginMining = SharedClass.ListLoginMining.FirstOrDefault(item => item.Id == LoginId);
if (foundLoginMining != null)
{
  var resultLogin = await SharedLibraryDashboardMining.LoginAsync((int)(cbMining.SelectedItem as MiningObject).Mining, txtLogin.Text.Trim(), pswPassword.Password.Trim(), txtCaptcha.Text.Trim(), foundLoginMining.SimpleMining_cfduid, foundLoginMining.SimpleMining_cflb, foundLoginMining.SimpleMining_PHPSESSID);
  if (string.IsNullOrEmpty(resultLogin))
  {
    SharedClass.SaveLogin(LoginId, (cbMining.SelectedItem as MiningObject).Mining, txtLogin.Text.Trim(), pswPassword.Password.Trim(), txtCaptcha.Text.Trim());
  }
  else
  {
    var ErrorMessage = SharedClass.Error_Login + ": " + resultLogin;
    WriteError(ErrorMessage);
    await ShowDialog(ErrorMessage);

    var imgCaptcha = (FindName("imgCaptcha" + LoginId) as Image).Source = null;
    txtCaptcha.Text = "";
  }
}
}
*/

/* Рабочий вариант Logout(string LoginId) (до re-captcha)
private async Task Logout(string LoginId)
    {
      var foundLogin = SharedClass.ListLoginMining.First(item => item.Id == LoginId);

      #region Set controls
      var cmdLogout = (FindName("cmdLogout" + LoginId) as Button);
      #endregion Set controls

      cmdLogout.IsEnabled = false;

      var resultLogout = await SharedLibraryDashboardMining.LogoutAsync((int)foundLogin.Mining, foundLogin.SimpleMining_cfduid, foundLogin.SimpleMining_cflb, foundLogin.SimpleMining_PHPSESSID);
      if (string.IsNullOrEmpty(resultLogout))
      {
        SharedClass.SaveLogout(LoginId);
        (FindName("imgCaptcha" + LoginId) as Image).Source = null;
        (FindName("txtCaptcha" + LoginId) as TextBox).Text = "";

        lock (lockListRigs)
        {
          int countListReg = ListRig.Count; SandboxObject foundSandboxObject;
          for (int i = countListReg - 1; i >= 0; i--)
          {
            if (ListRig[i].Login == foundLogin.Login)
            {
              foundSandboxObject = ListSandbox.FirstOrDefault(itemSandbox => itemSandbox.id == ListRig[i].id);
              if (foundSandboxObject != null) ListSandbox.Remove(foundSandboxObject);

              ListRig.Remove(ListRig[i]);
              countListReg = ListRig.Count;
            }
          }

          txtCountRigs.Text = ListRig.Count.ToString();
        }
      }
      else
      {
        var ErrorMessage = SharedClass.Error_Logout + ": " + resultLogout;
        WriteError(ErrorMessage);
        await ShowDialog(ErrorMessage);
      }
    }*/
