﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using WoWonder.Activities.Base;
using WoWonder.Activities.Default;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PasswordActivity : BaseActivity
    {
        #region Variables Basic

        private EditText TxtCurrentPassword, TxtNewPassword, TxtRepeatPassword;
        private TextView TxtLinkForget;
        private AppCompatButton BtnSave;

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
                SetContentView(Resource.Layout.PasswordLayout);

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
                BtnSave = FindViewById<AppCompatButton>(Resource.Id.SaveButton);

                TxtCurrentPassword = FindViewById<EditText>(Resource.Id.CurrentPasswordEditText);

                TxtNewPassword = FindViewById<EditText>(Resource.Id.NewPasswordEditText);

                TxtRepeatPassword = (EditText)FindViewById(Resource.Id.RepeatPasswordEditText);

                TxtLinkForget = FindViewById<TextView>(Resource.Id.linkText);

                Methods.SetColorEditText(TxtCurrentPassword, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtNewPassword, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtRepeatPassword, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

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
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_Change_Password);
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
                        TxtLinkForget.Click += TxtLinkForget_OnClick;
                        BtnSave.Click += SaveData_OnClick;
                        break;
                    default:
                        TxtLinkForget.Click -= TxtLinkForget_OnClick;
                        BtnSave.Click -= SaveData_OnClick;
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
                BtnSave = null!;
                TxtCurrentPassword = null!;
                TxtNewPassword = null!;
                TxtRepeatPassword = null!;
                TxtLinkForget = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void TxtLinkForget_OnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(ForgetPasswordActivity));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save data 
        private async void SaveData_OnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                if (string.IsNullOrEmpty(TxtCurrentPassword.Text) || string.IsNullOrEmpty(TxtNewPassword.Text) || string.IsNullOrEmpty(TxtRepeatPassword.Text))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long);
                    return;
                }

                if (TxtNewPassword.Text != TxtRepeatPassword.Text)
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Your_password_dont_match), ToastLength.Long);
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();

                var dataPrivacy = new Dictionary<string, string>
                {
                    {"new_password", TxtNewPassword.Text},
                    {"current_password", TxtCurrentPassword.Text},

                };

                var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case MessageObject result when result.Message.Contains("updated"):
                                    UserDetails.Password = TxtNewPassword.Text;

                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourDetailsWasUpdated), ToastLength.Short);
                                    AndHUD.Shared.Dismiss();
                                    break;
                                case MessageObject result:
                                    //Show a Error image with a message
                                    AndHUD.Shared.ShowError(this, result.Message, MaskType.Clear, TimeSpan.FromSeconds(1));
                                    break;
                            }

                            break;
                        }
                    default:
                        Methods.DisplayAndHudErrorResult(this, respond);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                //Show a Error image with a message
                AndHUD.Shared.ShowError(this, e.Message, MaskType.Clear, TimeSpan.FromSeconds(1));
            }
        }

        #endregion

    }
}