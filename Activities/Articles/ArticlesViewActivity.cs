﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Com.Aghajari.Emojiview.View;
using Com.Facebook.Ads;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using WoWonder.Activities.Articles.Adapters;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.StickersView;
using WoWonderClient.Classes.Articles;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Articles
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ArticlesViewActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private ImageView ImageUser, ImageBlog;
        private TextView TxtUsername, TxtTime, TxtTitle, TxtDescription, TxtViews;
        private WebView TxtHtml;
        private ImageButton BtnMore;
        private ArticleDataObject ArticleData;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;

        public ArticlesCommentAdapter MAdapter;
        private RecyclerView MRecycler;
        private AXEmojiEditText TxtComment;
        public ImageView ImgSent;
        private ImageView EmojisView;
        private string ArticlesId;
        private static ArticlesViewActivity Instance;
        private RewardedVideoAd RewardedVideo;
        private string DataWebHtml;

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
                SetContentView(Resource.Layout.ArticlesViewLayout);

                Instance = this;

                ArticlesId = Intent?.GetStringExtra("Id") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetDataArticles();

                if (AppSettings.ShowFbRewardVideoAds)
                    RewardedVideo = AdsFacebook.InitRewardVideo(this);
                else if (AppSettings.ShowAppLovinRewardAds)
                    AdsAppLovin.Ad_Rewarded(this);
                else
                    AdsGoogle.Ad_RewardedVideo(this);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

                case Resource.Id.action_share:
                    ShareEvent();
                    break;

                case Resource.Id.action_copy:
                    Methods.CopyToClipboard(this, ArticleData.Url);
                    break;

            }

            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MenuArticleShare, menu);
            WoWonderTools.ChangeMenuIconColor(menu, Color.ParseColor("#888888"));

            return base.OnCreateOptionsMenu(menu);

        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ImageUser = FindViewById<ImageView>(Resource.Id.imageAvatar);
                ImageBlog = FindViewById<ImageView>(Resource.Id.imageBlog);
                TxtUsername = FindViewById<TextView>(Resource.Id.username);
                TxtTime = FindViewById<TextView>(Resource.Id.time);
                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                TxtDescription = FindViewById<TextView>(Resource.Id.description);
                TxtHtml = FindViewById<WebView>(Resource.Id.LocalWebView);
                TxtViews = FindViewById<TextView>(Resource.Id.views);
                BtnMore = FindViewById<ImageButton>(Resource.Id.more);

                MRecycler = FindViewById<RecyclerView>(Resource.Id.recycler_view);

                EmojisView = FindViewById<ImageView>(Resource.Id.emojiicon);
                TxtComment = FindViewById<AXEmojiEditText>(Resource.Id.commenttext);
                ImgSent = FindViewById<ImageView>(Resource.Id.send);

                TxtComment.Text = "";
                InitEmojisView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitEmojisView()
        {
            Methods.SetColorEditText(TxtComment, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (WoWonderTools.IsTabDark())
                        EmojisViewTools.LoadDarkTheme();
                    else
                        EmojisViewTools.LoadTheme(AppSettings.MainColor);

                    EmojisViewTools.MStickerView = false;
                    EmojisViewTools.LoadView(this, TxtComment, "", EmojisView);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = "";
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new ArticlesCommentAdapter(this, "Comment")
                {
                    CommentList = new ObservableCollection<CommentsArticlesObject>()
                };

                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentsArticlesObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                        BtnMore.Click += BtnMoreOnClick;
                        ImgSent.Click += ImgSentOnClick;
                        break;
                    default:
                        BtnMore.Click -= BtnMoreOnClick;
                        ImgSent.Click -= ImgSentOnClick;
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
                RewardedVideo?.Destroy();

                MAdapter = null!;
                ImageUser = null!;
                ImageBlog = null!;
                MRecycler = null!;
                TxtUsername = null!;
                TxtTime = null!;
                TxtTitle = null!;
                TxtViews = null!;
                TxtHtml = null!;
                BtnMore = null!;
                ArticleData = null!;
                MAdapter = null!;
                MRecycler = null!;
                TxtComment = null!;
                ArticlesId = null!;
                Instance = null!;
                DataWebHtml = null!;
                RewardedVideo = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public static ArticlesViewActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region Events

        //Api sent Comment
        private async void ImgSentOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtComment.Text) && string.IsNullOrWhiteSpace(TxtComment.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    //remove \n in a string
                    string replacement = Regex.Replace(TxtComment.Text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

                    CommentsArticlesObject comment = new CommentsArticlesObject
                    {
                        Id = unixTimestamp.ToString(),
                        BlogId = ArticlesId,
                        UserId = UserDetails.UserId,
                        Text = replacement,
                        Likes = "0",
                        Posted = unixTimestamp.ToString(),
                        UserData = dataUser,
                        IsOwner = true,
                        Dislikes = "0",
                        IsCommentLiked = false,
                        Replies = new List<CommentsArticlesObject>()
                    };

                    MAdapter.CommentList.Add(comment);

                    var index = MAdapter.CommentList.IndexOf(comment);
                    switch (index)
                    {
                        case > -1:
                            MAdapter.NotifyItemInserted(index);
                            break;
                    }

                    MRecycler.Visibility = ViewStates.Visible;

                    var dd = MAdapter.CommentList.FirstOrDefault();
                    if (dd?.Text == MAdapter.EmptyState)
                    {
                        MAdapter.CommentList.Remove(dd);
                        MAdapter.NotifyItemRemoved(MAdapter.CommentList.IndexOf(dd));
                    }

                    //Hide keyboard
                    TxtComment.Text = "";

                    var (apiStatus, respond) = await RequestsAsync.Article.CreateCommentsAsync(ArticlesId, replacement);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case GetCommentsArticlesObject result:
                                        {
                                            var date = MAdapter.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapter.CommentList.FirstOrDefault(x => x.Id == result.Data[0]?.Id);
                                            if (date != null)
                                            {
                                                date = result.Data[0];
                                                date.Id = result.Data[0].Id;

                                                index = MAdapter.CommentList.IndexOf(MAdapter.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                                switch (index)
                                                {
                                                    case > -1:
                                                        MAdapter.CommentList[index] = result.Data[0];

                                                        //MAdapter.NotifyItemChanged(index);
                                                        MRecycler.ScrollToPosition(index);
                                                        break;
                                                }
                                            }

                                            break;
                                        }
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }

                    //Hide keyboard
                    TxtComment.Text = "";
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //More 
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                switch (CrossShare.IsSupported)
                {
                    //Share Plugin same as video
                    case false:
                        return;
                    default:
                        await CrossShare.Current.Share(new ShareMessage
                        {
                            Title = ArticleData.Title,
                            Text = " ",
                            Url = ArticleData.Url
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.CommentList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Lbl_CopeLink))
                {
                    Methods.CopyToClipboard(this, ArticleData.Url);
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Article & Comment 

        private void GetDataArticles()
        {
            try
            {
                ArticleData = JsonConvert.DeserializeObject<ArticleDataObject>(Intent?.GetStringExtra("ArticleObject") ?? "");
                if (ArticleData != null)
                {
                    GlideImageLoader.LoadImage(this, ArticleData.Author.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                    GlideImageLoader.LoadImage(this, ArticleData.Thumbnail, ImageBlog, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    TxtUsername.Text = WoWonderTools.GetNameFinal(ArticleData.Author);

                    TxtTitle.Text = Methods.FunString.DecodeString(ArticleData.Title);
                    TxtDescription.Text = Methods.FunString.DecodeString(ArticleData.Description);
                    TxtViews.Text = ArticleData.View + " " + GetText(Resource.String.Lbl_Views);

                    string style = WoWonderTools.IsTabDark() ? "<style type='text/css'>body{color: #fff; background-color: #282828; line-height: 1.42857143;}</style>" : "<style type='text/css'>body{color: #444; background-color: #FFFEFE; line-height: 1.42857143;}</style>";
                    string imageFullWidthStyle = "<style>img{display: inline;height: auto;max-width: 100%;}</style>";

                    // This method is deprecated but need to use for old os devices
#pragma warning disable CS0618 // Type or member is obsolete
                    string content = Build.VERSION.SdkInt >= BuildVersionCodes.N ? Html.FromHtml(ArticleData.Content, FromHtmlOptions.ModeCompact)?.ToString() : Html.FromHtml(ArticleData.Content)?.ToString();
#pragma warning restore CS0618 // Type or member is obsolete

                    DataWebHtml = "<!DOCTYPE html>";
                    DataWebHtml += "<head><title></title>" + style + imageFullWidthStyle + "</head>";
                    DataWebHtml += "<body>" + content + "</body>";
                    DataWebHtml += "</html>";
                    // <meta name='viewport' content='width=device-width, user-scalable=no' />
                    TxtHtml.SetWebViewClient(new MyWebViewClient(this));
                    TxtHtml.Settings.LoadsImagesAutomatically = true;
                    TxtHtml.Settings.JavaScriptEnabled = true;
                    TxtHtml.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                    TxtHtml.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                    TxtHtml.Settings.DomStorageEnabled = true;
                    TxtHtml.Settings.AllowFileAccess = true;
                    TxtHtml.Settings.DefaultTextEncodingName = "utf-8";

                    TxtHtml.Settings.UseWideViewPort = true;
                    TxtHtml.Settings.LoadWithOverviewMode = true;

                    TxtHtml.Settings.SetSupportZoom(false);
                    TxtHtml.Settings.BuiltInZoomControls = false;
                    TxtHtml.Settings.DisplayZoomControls = false;

                    //int fontSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 18, Resources?.DisplayMetrics);
                    //TxtHtml.Settings.DefaultFontSize = fontSize;

                    TxtHtml.LoadDataWithBaseURL(null, DataWebHtml, "text/html", "UTF-8", null);

                    bool success = int.TryParse(ArticleData.Posted, out var number);
                    TxtTime.Text = success switch
                    {
                        true => Methods.Time.TimeAgo(number, false),
                        _ => ArticleData.Posted
                    };

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Article.GetBlogByIdAsync(ArticlesId) });

                    Task.Factory.StartNew(() => StartApiService());
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataComment(offset) });
        }

        private async Task LoadDataComment(string offset)
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var countList = MAdapter.CommentList.Count;
                var (apiStatus, respond) = await RequestsAsync.Article.GetCommentsAsync(ArticlesId, "25", offset);
                if (apiStatus != 200 || respond is not GetCommentsArticlesObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data?.Count;
                    switch (respondList)
                    {
                        case > 0 when countList > 0:
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.CommentList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
                                break;
                            }
                        case > 0:
                            MAdapter.CommentList = new ObservableCollection<CommentsArticlesObject>(result.Data);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            break;
                    }
                }

                RunOnUiThread(ShowEmptyPage);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;

                switch (MAdapter.CommentList.Count)
                {
                    case > 0:
                        {
                            var emptyStateChecker = MAdapter.CommentList.FirstOrDefault(a => a.Text == MAdapter.EmptyState);
                            if (emptyStateChecker != null && MAdapter.CommentList.Count > 1)
                            {
                                MAdapter.CommentList.Remove(emptyStateChecker);
                                MAdapter.NotifyDataSetChanged();
                            }

                            break;
                        }
                    default:
                        {
                            MAdapter.CommentList.Clear();
                            var d = new CommentsArticlesObject { Text = MAdapter.EmptyState };
                            MAdapter.CommentList.Add(d);
                            MAdapter.NotifyDataSetChanged();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        private class MyWebViewClient : WebViewClient
        {
            private readonly ArticlesViewActivity Activity;
            public MyWebViewClient(ArticlesViewActivity mActivity)
            {
                Activity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                new IntentController(Activity).OpenBrowserFromApp(request.Url.ToString());
                view.LoadDataWithBaseURL(null, Activity.DataWebHtml, "text/html", "UTF-8", null);
                return true;
            }
        }
    }
}