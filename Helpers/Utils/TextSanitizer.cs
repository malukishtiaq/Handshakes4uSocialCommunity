﻿using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using Google.Android.Material.Dialog;
using Java.Lang;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.Controller;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.SQLite;
using Exception = System.Exception;

namespace WoWonder.Helpers.Utils
{
    public class TextSanitizer : Object, StTools.IXAutoLinkOnClickListener
    {
        private readonly SuperTextView SuperTextView;
        private readonly AppCompatActivity Activity;
        private readonly string TypePage;

        public TextSanitizer(SuperTextView linkTextView, AppCompatActivity activity, string typePage = "normal")
        {
            try
            {
                SuperTextView = linkTextView;
                Activity = activity;
                TypePage = typePage;
                SuperTextView.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Load(string text, string position = "Left")
        {
            try
            {
                SuperTextView.AddAutoLinkMode(new[] { StTools.XAutoLinkMode.ModePhone, StTools.XAutoLinkMode.ModeEmail, StTools.XAutoLinkMode.ModeHashTag, StTools.XAutoLinkMode.ModeUrl, StTools.XAutoLinkMode.ModeMention, StTools.XAutoLinkMode.ModeCustom });
                SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModePhone_color)));
                SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeEmail_color)));
                SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeHashtag_color)));
                SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeMention_color)));
                SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.accent)));

                var textt = text.Split('/');
                if (textt.Length > 1)
                {
                    SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                    SuperTextView.SetCustomRegex(@"\b(" + textt.LastOrDefault() + @")\b");
                }

                string laststring = text.Replace(" /", " ");
                if (!string.IsNullOrEmpty(laststring))
                    SuperTextView.SetText(laststring, TextView.BufferType.Spannable);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void AutoLinkTextClick(StTools.XAutoLinkMode autoLinkMode, string matchedText, Dictionary<string, string> userData)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(matchedText.Replace(" ", "").Replace("\n", "").Replace("\n", ""));
                if (typetext == "Email" || autoLinkMode == StTools.XAutoLinkMode.ModeEmail)
                {
                    Methods.App.SendEmail(Activity, matchedText.Replace(" ", "").Replace("\n", ""));
                }
                else if (typetext == "Website" || autoLinkMode == StTools.XAutoLinkMode.ModeUrl)
                {
                    string url = matchedText.Replace(" ", "").Replace("\n", "");
                    if (!matchedText.Contains("http"))
                    {
                        url = "http://" + matchedText.Replace(" ", "").Replace("\n", "");
                    }

                    //var intent = new Intent(Activity, typeof(LocalWebViewActivity));
                    //intent.PutExtra("URL", url);
                    //intent.PutExtra("Type", url);
                    //Activity.StartActivity(intent);
                    new IntentController(Activity).OpenBrowserFromApp(url);
                }
                else if (typetext == "Hashtag" || autoLinkMode == StTools.XAutoLinkMode.ModeHashTag)
                {
                    var intent = new Intent(Activity, typeof(HashTagPostsActivity));
                    intent.PutExtra("Id", matchedText.Replace(" ", ""));
                    intent.PutExtra("Tag", matchedText.Replace(" ", ""));
                    Activity.StartActivity(intent);
                }
                else if (typetext == "Mention" || autoLinkMode == StTools.XAutoLinkMode.ModeMention)
                {
                    var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                    string name = matchedText.Replace("@", "").Replace(" ", "");

                    var sqlEntity = new SqLiteDatabase();
                    var user = sqlEntity.Get_DataOneUser(name);

                    if (user != null)
                    {
                        WoWonderTools.OpenProfile(Activity, user.UserId, user);
                    }
                    else
                    {
                        if (name == dataUSer?.Name || name == dataUSer?.Username)
                        {
                            switch (PostClickListener.OpenMyProfile)
                            {
                                case true:
                                    return;
                                default:
                                    {
                                        var intent = new Intent(Activity, typeof(MyProfileActivity));
                                        Activity.StartActivity(intent);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            var intent = new Intent(Activity, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("name", name);
                            Activity.StartActivity(intent);
                        }
                    }
                }
                else if (typetext == "Number" || autoLinkMode == StTools.XAutoLinkMode.ModePhone)
                {
                    Methods.App.SaveContacts(Activity, matchedText.Replace(" ", "").Replace("\n", ""), "", "2");
                }
                else if (autoLinkMode == StTools.XAutoLinkMode.ModeCustom && TypePage == "AddPost")
                {
                    var dialog = new MaterialAlertDialogBuilder(Activity);

                    dialog.SetTitle(Activity.GetText(Resource.String.Lbl_Location));
                    dialog.SetPositiveButton(Activity.GetText(Resource.String.Lbl_RemoveLocation), (sender, args) =>
                    {
                        try
                        {
                            ((AddPostActivity)Activity)?.RemoveLocation();
                            ((PostSharingActivity)Activity)?.RemoveLocation();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.SetNeutralButton(Activity.GetText(Resource.String.Lbl_ChangeLocation), (sender, args) =>
                    {
                        try
                        {
                            //Open intent Location when the request code of result is 502
                            new IntentController(Activity).OpenIntentLocation();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.SetNegativeButton(Activity.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                    //dialog.;
                    dialog.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}