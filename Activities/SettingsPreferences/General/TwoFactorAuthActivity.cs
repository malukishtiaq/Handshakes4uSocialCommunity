﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Dialog;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class TwoFactorAuthActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private EditText TxtTwoFactor, TxtTwoFactorCode;
        private AppCompatButton SaveButton;
        private string TypeTwoFactor, TypeDialog = "Confirmation";

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.TwoFactorAuthLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtTwoFactor = FindViewById<EditText>(Resource.Id.TwoFactorEditText);

                //IconTwoFactorCode = FindViewById<TextView>(Resource.Id.IconTwoCodeFactor);

                TxtTwoFactorCode = FindViewById<EditText>(Resource.Id.TwoFactorCodeEditText);
                TxtTwoFactorCode.Visibility = ViewStates.Invisible;

                SaveButton = FindViewById<AppCompatButton>(Resource.Id.SaveButton);

                Methods.SetColorEditText(TxtTwoFactor, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtTwoFactor);

                var twoFactorUSer = ListUtils.MyProfileList?.FirstOrDefault()?.TwoFactor;
                switch (twoFactorUSer)
                {
                    case "0":
                        TxtTwoFactor.Text = GetText(Resource.String.Lbl_Disable);
                        TypeTwoFactor = "off";
                        break;
                    default:
                        TxtTwoFactor.Text = GetText(Resource.String.Lbl_Enable);
                        TypeTwoFactor = "on";
                        break;
                }

                AdsGoogle.Ad_AdMobNative(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetString(Resource.String.Lbl_TwoFactor);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        TxtTwoFactor.Touch += TxtTwoFactorOnTouch;
                        SaveButton.Click += SaveButtonOnClick;
                        break;
                    default:
                        TxtTwoFactor.Touch -= TxtTwoFactorOnTouch;
                        SaveButton.Click -= SaveButtonOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private void DestroyBasic()
        {
            try
            {
                TxtTwoFactor = null!;
                TxtTwoFactorCode = null!;
                SaveButton = null!;
                TypeTwoFactor = null!;
                TypeDialog = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        // check code email if good or no than update data user 
        private async void SendButtonOnClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                if (string.IsNullOrEmpty(TxtTwoFactorCode.Text) || string.IsNullOrWhiteSpace(TxtTwoFactorCode.Text))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                var (apiStatus, respond) = await RequestsAsync.Global.UpdateTwoFactorAsync("verify", TxtTwoFactorCode.Text);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case MessageObject result:
                                    {
                                        Console.WriteLine(result.Message);

                                        var local = ListUtils.MyProfileList?.FirstOrDefault();
                                        if (local != null)
                                        {
                                            local.TwoFactor = "1";

                                            var sqLiteDatabase = new SqLiteDatabase();
                                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(local);

                                        }

                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_TwoFactorOn), ToastLength.Short);
                                        AndHUD.Shared.Dismiss();

                                        Finish();
                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayAndHudErrorResult(this, respond);
                        break;
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtTwoFactorOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Confirmation";

                var dialogList = new MaterialAlertDialogBuilder(this);

                var arrayAdapter = new List<string>
                {
                    GetString(Resource.String.Lbl_Enable), GetString(Resource.String.Lbl_Disable)
                };

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // send data and send api and show liner add code email 
        private async void SaveButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (TypeDialog)
                {
                    case "Confirmation":
                        switch (TypeTwoFactor)
                        {
                            case "on":
                                {
                                    //Show a progress
                                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                                    int apiStatus; dynamic respond;
                                    if (ListUtils.SettingsSiteList?.TwoFactorType == "phone")
                                    {
                                        var phoneNumber = ListUtils.MyProfileList.FirstOrDefault()?.PhoneNumber;
                                        if (!string.IsNullOrEmpty(phoneNumber) && Methods.FunString.IsPhoneNumber(phoneNumber))
                                        {
                                            (apiStatus, respond) = await RequestsAsync.Global.UpdateTwoFactorAsync(phoneNumber);
                                        }
                                        else
                                        {
                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PhoneNumberIsWrong), ToastLength.Short);
                                            AndHUD.Shared.Dismiss();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        (apiStatus, respond) = await RequestsAsync.Global.UpdateTwoFactorAsync();
                                    }

                                    switch (apiStatus)
                                    {
                                        case 200:
                                            {
                                                if (respond is not MessageObject result) return;
                                                if (result.Message.Contains("confirmation code sent"))
                                                {
                                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ConfirmationCodeSent), ToastLength.Short);

                                                    AndHUD.Shared.Dismiss();

                                                    TypeDialog = "ConfirmationCode";

                                                    TxtTwoFactorCode.Visibility = ViewStates.Visible;
                                                    SaveButton.Text = GetText(Resource.String.Btn_Send);
                                                }
                                                else
                                                {
                                                    //Show a Error image with a message
                                                    AndHUD.Shared.ShowError(this, result.Message, MaskType.Clear, TimeSpan.FromSeconds(1));
                                                }

                                                break;
                                            }
                                        default:
                                            Methods.DisplayAndHudErrorResult(this, respond);
                                            break;
                                    }

                                    break;
                                }
                            case "off":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateTwoFactorAsync() });
                                var local = ListUtils.MyProfileList?.FirstOrDefault();
                                if (local != null)
                                {
                                    local.TwoFactor = "0";

                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(local);

                                }

                                Finish();
                                break;
                        }

                        break;
                    case "ConfirmationCode":
                        SendButtonOnClick();
                        break;
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_Enable))
                {
                    TxtTwoFactor.Text = GetText(Resource.String.Lbl_Enable);
                    TypeTwoFactor = "on";
                    TxtTwoFactorCode.Visibility = ViewStates.Invisible;
                }
                else if (itemString == GetText(Resource.String.Lbl_Disable))
                {
                    TxtTwoFactor.Text = GetText(Resource.String.Lbl_Disable);
                    TypeTwoFactor = "off";
                    TxtTwoFactorCode.Visibility = ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}