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
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Com.Aghajari.Emojiview.View;
using Newtonsoft.Json;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Articles.Adapters;
using WoWonder.Activities.Base;
using WoWonder.Activities.Movies;
using WoWonder.Activities.Movies.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.StickersView;
using WoWonderClient.Classes.Articles;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Comment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ReplyCommentBottomSheet : BaseActivity
    {
        #region Variables Basic

        private static ReplyCommentBottomSheet Instance;
        public ArticlesCommentAdapter MAdapterArticles;
        public MoviesCommentAdapter MAdapterMovies;

        private ViewStub CommentLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private View CommentLayoutView;
        private TextView ReplyCountTextView;
        public AXEmojiEditText TxtComment;
        private ImageView ImgBack, ImgSent, ImgGallery;
        private ImageView EmojisView;
        private LinearLayout RootView;

        private string Type, IdComment;
        private CommentsArticlesObject ArticlesObject;
        private CommentsMoviesObject MoviesObject;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.ReplyCommentLayout);

                Instance = this;

                Type = Intent?.GetStringExtra("Type") ?? "";
                IdComment = Intent?.GetStringExtra("Id") ?? "";

                switch (Type)
                {
                    case "Article":
                        {
                            ArticlesObject = JsonConvert.DeserializeObject<CommentsArticlesObject>(Intent?.GetStringExtra("Object") ?? "");
                            break;
                        }
                    case "Movies":
                        {
                            MoviesObject = JsonConvert.DeserializeObject<CommentsMoviesObject>(Intent?.GetStringExtra("Object") ?? "");
                            break;
                        }
                }

                InitComponent();
                SetRecyclerViewAdapters();

                switch (Type)
                {
                    case "Article":
                        LoadCommentArticle();
                        break;
                    case "Movies":
                        LoadCommentMovies();
                        break;
                }

                AdsGoogle.Ad_Interstitial(this);
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
                RootView = FindViewById<LinearLayout>(Resource.Id.main_content);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler_view);

                EmojisView = FindViewById<ImageView>(Resource.Id.emojiicon);
                TxtComment = FindViewById<AXEmojiEditText>(Resource.Id.commenttext);
                ImgSent = FindViewById<ImageView>(Resource.Id.send);
                ImgGallery = FindViewById<ImageView>(Resource.Id.image);
                ImgBack = FindViewById<ImageView>(Resource.Id.back);
                CommentLayout = FindViewById<ViewStub>(Resource.Id.comment_layout);

                ReplyCountTextView = FindViewById<TextView>(Resource.Id.replyCountTextview);

                var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                ImgBack.SetImageDrawable(icon);

                ImgGallery.Visibility = ViewStates.Gone;

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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                switch (Type)
                {
                    case "Article":
                        {
                            MAdapterArticles = new ArticlesCommentAdapter(this, "Reply")
                            {
                                CommentList = new ObservableCollection<CommentsArticlesObject>()
                            };

                            LayoutManager = new LinearLayoutManager(this);
                            MRecycler.SetLayoutManager(LayoutManager);
                            MRecycler.HasFixedSize = true;
                            MRecycler.SetItemViewCacheSize(10);
                            MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                            var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                            var preLoader = new RecyclerViewPreloader<CommentsArticlesObject>(this, MAdapterArticles, sizeProvider, 10);
                            MRecycler.AddOnScrollListener(preLoader);
                            MRecycler.SetAdapter(MAdapterArticles);

                            RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                            MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                            MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                            MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                            MainScrollEvent.IsLoading = false;
                            break;
                        }
                    case "Movies":
                        {
                            MAdapterMovies = new MoviesCommentAdapter(this, "Reply")
                            {
                                CommentList = new ObservableCollection<CommentsMoviesObject>()
                            };

                            LayoutManager = new LinearLayoutManager(this);
                            MRecycler.SetLayoutManager(LayoutManager);
                            MRecycler.HasFixedSize = true;
                            MRecycler.SetItemViewCacheSize(10);
                            MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                            var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                            var preLoader = new RecyclerViewPreloader<CommentsArticlesObject>(this, MAdapterMovies, sizeProvider, 10);
                            MRecycler.AddOnScrollListener(preLoader);
                            MRecycler.SetAdapter(MAdapterMovies);

                            RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                            MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                            MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                            MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                            MainScrollEvent.IsLoading = false;
                            break;
                        }
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
                        switch (Type)
                        {
                            case "Article":
                                ImgSent.Click += ImgSentArticlesOnClick;
                                break;
                            case "Movies":
                                ImgSent.Click += ImgSentMoviesOnClick;
                                break;
                        }
                        ImgBack.Click += ImgBackOnClick;
                        break;
                    default:
                        switch (Type)
                        {
                            case "Article":
                                ImgSent.Click -= ImgSentArticlesOnClick;
                                break;
                            case "Movies":
                                ImgSent.Click -= ImgSentMoviesOnClick;
                                break;
                        }
                        ImgBack.Click -= ImgBackOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ReplyCommentBottomSheet GetInstance()
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

        #region Get Replies

        private void LoadCommentArticle()
        {
            try
            {
                switch (ArticlesObject)
                {
                    case null:
                        return;
                }

                CommentLayout.LayoutResource = Resource.Layout.Style_CommentView;
                CommentLayoutView = CommentLayout.Inflate();

                var holder = new ArticlesCommentAdapterViewHolder(CommentLayoutView, MAdapterArticles, new ArticlesCommentClickListener(this, "Reply"))
                {
                    ReplyTextView = { Visibility = ViewStates.Gone }
                };

                //Load data same as comment adapter
                var commentAdapter = new ArticlesCommentAdapter(this, "Reply");
                commentAdapter.LoadCommentData(ArticlesObject, holder);

                ReplyCountTextView.Text = ArticlesObject.Replies?.Count > 0 ? ArticlesObject.Replies.Count + " " + GetString(Resource.String.Lbl_Replies) : GetString(Resource.String.Lbl_Replies);
                TxtComment.Text = "@" + ArticlesObject?.UserData?.Username + " " ?? "";

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadCommentMovies()
        {
            try
            {
                switch (MoviesObject)
                {
                    case null:
                        return;
                }

                CommentLayout.LayoutResource = Resource.Layout.Style_CommentView;
                CommentLayoutView = CommentLayout.Inflate();

                var holder = new MoviesCommentAdapterViewHolder(CommentLayoutView, MAdapterMovies, new MoviesCommentClickListener(this, "Reply"))
                {
                    ReplyTextView = { Visibility = ViewStates.Gone }
                };

                //Load data same as comment adapter
                var commentAdapter = new MoviesCommentAdapter(this, "Reply");
                commentAdapter.LoadCommentData(MoviesObject, holder);

                ReplyCountTextView.Text = MoviesObject.Replies?.Count > 0 ? MoviesObject.Replies.Count + " " + GetString(Resource.String.Lbl_Replies) : GetString(Resource.String.Lbl_Replies);
                TxtComment.Text = "@" + MoviesObject?.UserData?.Username + " " ?? "";

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Back
        private void ImgBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Api sent Comment Articles
        private async void ImgSentArticlesOnClick(object sender, EventArgs e)
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
                        BlogId = ArticlesObject.BlogId,
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

                    MAdapterArticles.CommentList.Add(comment);

                    var index = MAdapterArticles.CommentList.IndexOf(comment);
                    switch (index)
                    {
                        case > -1:
                            MAdapterArticles.NotifyItemInserted(index);
                            break;
                    }

                    MRecycler.Visibility = ViewStates.Visible;

                    var dd = MAdapterArticles.CommentList.FirstOrDefault();
                    if (dd?.Text == MAdapterArticles.EmptyState)
                    {
                        MAdapterArticles.CommentList.Remove(dd);
                        MAdapterArticles.NotifyItemRemoved(MAdapterArticles.CommentList.IndexOf(dd));
                    }

                    //Hide keyboard
                    TxtComment.Text = "";

                    var (apiStatus, respond) = await RequestsAsync.Article.CreateReplyAsync(ArticlesObject.BlogId, IdComment, replacement);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case GetCommentsArticlesObject result:
                                        {
                                            var date = MAdapterArticles.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapterArticles.CommentList.FirstOrDefault(x => x.Id == result.Data[0]?.Id);
                                            if (date != null)
                                            {
                                                date = result.Data[0];
                                                date.Id = result.Data[0].Id;

                                                index = MAdapterArticles.CommentList.IndexOf(MAdapterArticles.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                                switch (index)
                                                {
                                                    case > -1:
                                                        MAdapterArticles.CommentList[index] = result.Data[0];

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

        //Api sent Comment Movies
        private async void ImgSentMoviesOnClick(object sender, EventArgs e)
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

                    CommentsMoviesObject comment = new CommentsMoviesObject
                    {
                        Id = unixTimestamp.ToString(),
                        MovieId = MoviesObject.MovieId,
                        UserId = UserDetails.UserId,
                        Text = replacement,
                        Likes = "0",
                        Posted = unixTimestamp.ToString(),
                        UserData = dataUser,
                        IsOwner = true,
                        Dislikes = "0",
                        IsCommentLiked = false,
                        Replies = new List<CommentsMoviesObject>()
                    };

                    MAdapterMovies.CommentList.Add(comment);

                    var index = MAdapterMovies.CommentList.IndexOf(comment);
                    switch (index)
                    {
                        case > -1:
                            MAdapterMovies.NotifyItemInserted(index);
                            break;
                    }

                    MRecycler.Visibility = ViewStates.Visible;

                    var dd = MAdapterMovies.CommentList.FirstOrDefault();
                    if (dd?.Text == MAdapterMovies.EmptyState)
                    {
                        MAdapterMovies.CommentList.Remove(dd);
                        MAdapterMovies.NotifyItemRemoved(MAdapterMovies.CommentList.IndexOf(dd));
                    }

                    //Hide keyboard
                    TxtComment.Text = "";

                    var (apiStatus, respond) = await RequestsAsync.Movies.CreateReplyAsync(MoviesObject.MovieId, IdComment, replacement);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case GetCommentsMoviesObject result:
                                        {
                                            var date = MAdapterMovies.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapterMovies.CommentList.FirstOrDefault(x => x.Id == result.Data[0]?.Id);
                                            if (date != null)
                                            {
                                                date = result.Data[0];
                                                date.Id = result.Data[0].Id;

                                                index = MAdapterMovies.CommentList.IndexOf(MAdapterMovies.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                                switch (index)
                                                {
                                                    case > -1:
                                                        MAdapterMovies.CommentList[index] = result.Data[0];

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

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                switch (Type)
                {
                    case "Article":
                        var itemArticle = MAdapterArticles.CommentList.LastOrDefault(a => a.TypeView != "Ads");
                        if (itemArticle != null && !string.IsNullOrEmpty(itemArticle.Id) && !MainScrollEvent.IsLoading)
                            StartApiService(itemArticle.Id);
                        break;
                    case "Movies":
                        var itemMovies = MAdapterMovies.CommentList.LastOrDefault(a => a.TypeView != "Ads");
                        if (itemMovies != null && !string.IsNullOrEmpty(itemMovies.Id) && !MainScrollEvent.IsLoading)
                            StartApiService(itemMovies.Id);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Comment Reply

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
            {
                switch (Type)
                {
                    case "Article":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataCommentReplyArticle(offset) });
                        break;
                    case "Movies":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataCommentReplyMovies(offset) });
                        break;
                }

            }
        }

        private async Task LoadDataCommentReplyArticle(string offset)
        {
            if (MainScrollEvent.IsLoading) return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var countList = MAdapterArticles.CommentList.Count;
                var (apiStatus, respond) = await RequestsAsync.Article.GetReplyAsync(IdComment, "25", offset);
                if (apiStatus != 200 || respond is not GetCommentsArticlesObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = MAdapterArticles.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            MAdapterArticles.CommentList.Add(item);

                            if (MAdapterArticles.CommentList.Count % AppSettings.ShowAdNativeCommentCount == 0)
                            {
                                MAdapterArticles.CommentList.Add(new CommentsArticlesObject
                                {
                                    Id = "222222",
                                    TypeView = "Ads"
                                });
                            }
                        }

                        if (countList > 0)
                        {
                            RunOnUiThread(() => { MAdapterArticles.NotifyItemRangeInserted(countList, MAdapterArticles.CommentList.Count - countList); });
                        }
                        else
                        {
                            RunOnUiThread(() => { MAdapterArticles.NotifyDataSetChanged(); });
                        }
                    }
                }

                RunOnUiThread(ShowEmptyPage);
            }
        }

        private async Task LoadDataCommentReplyMovies(string offset)
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var countList = MAdapterMovies.CommentList.Count;
                var (apiStatus, respond) = await RequestsAsync.Movies.GetReplyAsync(IdComment, "25", offset);
                if (apiStatus != 200 || respond is not GetCommentsMoviesObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = MAdapterMovies.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            MAdapterMovies.CommentList.Add(item);

                            if (MAdapterMovies.CommentList.Count % AppSettings.ShowAdNativeCommentCount == 0)
                            {
                                MAdapterMovies.CommentList.Add(new CommentsMoviesObject
                                {
                                    Id = "222222",
                                    TypeView = "Ads"
                                });
                            }
                        }

                        if (countList > 0)
                        {
                            RunOnUiThread(() => { MAdapterMovies.NotifyItemRangeInserted(countList, MAdapterMovies.CommentList.Count - countList); });
                        }
                        else
                        {
                            RunOnUiThread(() => { MAdapterMovies.NotifyDataSetChanged(); });
                        }
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
                switch (Type)
                {
                    case "Article":
                        switch (MAdapterArticles.CommentList.Count)
                        {
                            case > 0:
                                {
                                    var emptyStateChecker = MAdapterArticles.CommentList.FirstOrDefault(a => a.Text == MAdapterArticles.EmptyState);
                                    if (emptyStateChecker != null && MAdapterArticles.CommentList.Count > 1)
                                    {
                                        MAdapterArticles.CommentList.Remove(emptyStateChecker);
                                        MAdapterArticles.NotifyDataSetChanged();
                                    }

                                    break;
                                }
                            default:
                                {
                                    MAdapterArticles.CommentList.Clear();
                                    var d = new CommentsArticlesObject { Text = MAdapterArticles.EmptyState };
                                    MAdapterArticles.CommentList.Add(d);
                                    MAdapterArticles.NotifyDataSetChanged();
                                    break;
                                }
                        }
                        break;
                    case "Movies":
                        switch (MAdapterMovies.CommentList.Count)
                        {
                            case > 0:
                                {
                                    var emptyStateChecker = MAdapterMovies.CommentList.FirstOrDefault(a => a.Text == MAdapterMovies.EmptyState);
                                    if (emptyStateChecker != null && MAdapterMovies.CommentList.Count > 1)
                                    {
                                        MAdapterMovies.CommentList.Remove(emptyStateChecker);
                                        MAdapterMovies.NotifyDataSetChanged();
                                    }

                                    break;
                                }
                            default:
                                {
                                    MAdapterMovies.CommentList.Clear();
                                    var d = new CommentsMoviesObject { Text = MAdapterMovies.EmptyState };
                                    MAdapterMovies.CommentList.Add(d);
                                    MAdapterMovies.NotifyDataSetChanged();
                                    break;
                                }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion


    }
}