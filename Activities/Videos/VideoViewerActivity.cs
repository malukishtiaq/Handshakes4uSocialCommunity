﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Androidx.Media3.UI;
using AndroidX.RecyclerView.Widget;
using Anjo.Android.YouTubePlayerX.Player;
using Bumptech.Glide.Util;
using Com.Aghajari.Emojiview.View;
using Com.Google.Android.Gms.Ads;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Movies.Adapters;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.MediaPlayers;
using WoWonder.MediaPlayers.Exo;
using WoWonder.StickersView;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Requests;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Videos
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode, ResizeableActivity = true)]
    public class VideoViewerActivity : BaseActivity, IYouTubePlayerInitListener, IYouTubePlayerFullScreenListener
    {
        #region Variables Basic

        private PlayerView PlayerView;
        public ExoController ExoController;
        private MoviesDataObject Video;
        private YouTubePlayerView TubePlayerView;
        private IYouTubePlayer YoutubePlayer;
        private YouTubePlayerEvents YouTubePlayerEvents;
        private string VideoIdYoutube;
        private static VideoViewerActivity Instance;
        private string MoviesId;
        private string TypeYouTubePlayerFullScreen = "RequestedOrientation";

        //About 
        private AdView MAdView;
        private TextView TxtVideoName, TxtName, TxtViewCount, TxtCreateTime, TxtCategory, TxtTags, VideoStars;
        private SuperTextView VideoVideoDescription;
        private TextSanitizer TextSanitizerAutoLink;

        //Comment
        public MoviesCommentAdapter MAdapter;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private RecyclerView MRecycler;
        private AXEmojiEditText TxtComment;
        private ImageView EmojisView, ImgSent, ImgAvatar;

        private PostModelType VideoType;

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

                SetContentView(Resource.Layout.VideoViewerLayout);

                Instance = this;

                MoviesId = Intent?.GetStringExtra("VideoId") ?? "";

                //VideoActionsController = new VideoController(this, "Viewer_Video");
                InitComponent();
                InitPlayer();
                SetRecyclerViewAdapters();
                InitBackPressed("VideoViewerActivity");

                GetDataVideo();

                AdsGoogle.Ad_RewardedVideo(this);
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
                AddOrRemoveEvent(false);
                AdsGoogle.LifecycleAdView(MAdView, "Pause");

                base.OnPause();
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
                AdsGoogle.LifecycleAdView(MAdView, "Resume");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                StopVideo();

                base.OnStop();
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
                ReleaseVideo();

                TabbedMainActivity.GetInstance()?.SetOffWakeLock();

                AdsGoogle.LifecycleAdView(MAdView, "Destroy");

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        MainSettings.ApplyTheme(MainSettings.LightMode);
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                        break;
                }

                Delegate.SetLocalNightMode(WoWonderTools.IsTabDark() ? AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
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

        public void InitComponent()
        {
            try
            {
                TxtVideoName = FindViewById<TextView>(Resource.Id.Title);
                TxtName = FindViewById<TextView>(Resource.Id.tv_subname);

                TxtViewCount = FindViewById<TextView>(Resource.Id.Views_Count);
                TxtCreateTime = FindViewById<TextView>(Resource.Id.tv_time);
                VideoVideoDescription = FindViewById<SuperTextView>(Resource.Id.videoDescriptionTextview);
                TxtCategory = FindViewById<TextView>(Resource.Id.videoCategorytextview);

                VideoStars = FindViewById<TextView>(Resource.Id.videoStartextview);
                TxtTags = FindViewById<TextView>(Resource.Id.videoTagtextview);

                TextSanitizerAutoLink = new TextSanitizer(VideoVideoDescription, this);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);

                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);
                TxtComment = FindViewById<AXEmojiEditText>(Resource.Id.commenttext);
                EmojisView = FindViewById<ImageView>(Resource.Id.Emojiicon);
                ImgSent = FindViewById<ImageView>(Resource.Id.send);

                ImgAvatar = FindViewById<ImageView>(Resource.Id.avatar);

                PlayerView = FindViewById<PlayerView>(Resource.Id.player_view);

                TubePlayerView = FindViewById<YouTubePlayerView>(Resource.Id.youtube_player_view);
                if (TubePlayerView != null)
                {
                    TubePlayerView.Visibility = ViewStates.Gone;

                    // The player will automatically release itself when the activity is destroyed.
                    // The player will automatically pause when the activity is paused
                    // If you don't add YouTubePlayerView as a lifecycle observer, you will have to release it manually.
                    Lifecycle.AddObserver(TubePlayerView);

                    TubePlayerView.PlayerUiController.ShowMenuButton(false);

                    TubePlayerView.PlayerUiController.ShowCustomActionLeft1(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionLeft2(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionRight1(false);
                    TubePlayerView.PlayerUiController.ShowCustomActionRight2(false);

                    //TubePlayerView.PlayerUiController.Menu.AddItem(new MenuItem("example", Resource.Drawable.icon_settings_vector, (view)->Toast.makeText(this, "item clicked", Toast.LENGTH_SHORT).show()));
                }

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

        public void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new MoviesCommentAdapter(this, "Comment")
                {
                    CommentList = new ObservableCollection<CommentsMoviesObject>()
                };

                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentsMoviesObject>(this, MAdapter, sizeProvider, 10);
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


        public void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        ImgSent.Click += ImgSentOnClick;
                        break;
                    default:
                        ImgSent.Click -= ImgSentOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static VideoViewerActivity GetInstance()
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

        #region Back Pressed

        public void BackPressed()
        {
            try
            {
                ReleaseVideo();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                Finish();
            }
        }

        #endregion

        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 2000 && resultCode == Result.Ok)
                {
                    ExoController.RestartPlayAfterShrinkScreen();
                }
                else if (requestCode == 2100 && resultCode == Result.Ok) TubePlayerView?.ExitFullScreen();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Exo Player

        private void InitPlayer()
        {
            try
            {
                ExoController = new ExoController(this, "VideoViewerActivity");
                ExoController.SetPlayer(PlayerView);
                ExoController.SetPlayerControl();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StopVideo()
        {
            try
            {
                if (VideoType == PostModelType.YoutubePost)
                {
                    if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                        YoutubePlayer.Pause();
                }
                else
                {
                    ExoController?.StopVideo();
                }

                TabbedMainActivity.GetInstance()?.SetOffWakeLock();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ReleaseVideo()
        {
            try
            {
                if (VideoType == PostModelType.YoutubePost)
                {
                    if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                        YoutubePlayer.Pause();

                    TubePlayerView.Release();
                }
                else
                {
                    ExoController?.ReleaseVideo();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region YouTube Player

        public void OnInitSuccess(IYouTubePlayer player)
        {
            try
            {
                if (YoutubePlayer == null)
                {
                    YoutubePlayer = player;
                    YouTubePlayerEvents = new YouTubePlayerEvents(player, VideoIdYoutube, "VideoViewerActivity");
                    YoutubePlayer.AddListener(YouTubePlayerEvents);
                    TubePlayerView.AddFullScreenListener(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnYouTubePlayerEnterFullScreen()
        {
            try
            {
                Intent intent = new Intent(this, typeof(YouTubePlayerFullScreenActivity));
                intent.PutExtra("type", TypeYouTubePlayerFullScreen);
                intent.PutExtra("VideoIdYoutube", VideoIdYoutube);
                intent.PutExtra("CurrentSecond", YouTubePlayerEvents.CurrentSecond);
                StartActivityForResult(intent, 2100);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnYouTubePlayerExitFullScreen()
        {
            try
            {
                TypeYouTubePlayerFullScreen = "RequestedOrientation";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

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
                    var time = unixTimestamp.ToString();

                    //remove \n in a string
                    string replacement = Regex.Replace(TxtComment.Text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

                    CommentsMoviesObject comment = new CommentsMoviesObject
                    {
                        Id = unixTimestamp.ToString(),
                        MovieId = MoviesId,
                        UserId = UserDetails.UserId,
                        Text = replacement,
                        Likes = "0",
                        Posted = time,
                        UserData = dataUser,
                        IsOwner = true,
                        Dislikes = "0",
                        IsCommentLiked = false,
                        Replies = new List<CommentsMoviesObject>()
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

                    var (apiStatus, respond) = await RequestsAsync.Movies.CreateCommentsAsync(MoviesId, replacement);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case GetCommentsMoviesObject result:
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

        #region Load Video & Comment

        private async void GetDataVideo()
        {
            try
            {
                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("Viewer_Video")))
                {
                    Video = JsonConvert.DeserializeObject<MoviesDataObject>(Intent?.GetStringExtra("Viewer_Video") ?? "");
                    LoadDataVideo();
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return;
                    }

                    var (apiStatus, respond) = await RequestsAsync.Movies.GetMoviesAsync("", "", MoviesId);
                    if (apiStatus == 200)
                    {
                        if (respond is GetMoviesObject result)
                        {
                            var respondList = result.Movies.Count;
                            if (respondList > 0)
                            {
                                Video = result.Movies.FirstOrDefault(w => w.Id == MoviesId);
                                LoadDataVideo();
                            }
                        }
                    }
                    else
                        Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadDataVideo()
        {
            try
            {
                if (Video == null) return;

                LoadVideo_Data(Video);
                switch (string.IsNullOrEmpty(Video.Iframe))
                {
                    case false:
                        {
                            if (Video.Iframe.Contains("Youtube") || Video.Iframe.Contains("youtu"))
                            {
                                VideoType = PostModelType.YoutubePost;
                                VideoIdYoutube = Video.Iframe.Split(new[] { "v=", "/" }, StringSplitOptions.None).LastOrDefault();

                                TubePlayerView.Visibility = ViewStates.Visible;

                                TubePlayerView.Initialize(this);

                                PlayerView.Visibility = ViewStates.Gone;
                                ExoController?.StopVideo();
                            }

                            break;
                        }
                    default:
                        {
                            VideoType = PostModelType.VideoPost;

                            Uri uri = Uri.Parse(Video.Source);
                            ExoController?.FirstPlayVideo(uri);
                            break;
                        }
                }

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadVideo_Data(MoviesDataObject videoObject)
        {
            try
            {
                if (videoObject != null)
                {
                    TxtVideoName.Text = Methods.FunString.DecodeString(videoObject.Name);
                    TxtName.Text = AppSettings.ApplicationName;
                    //VideoQualityTextView.Text = videoObject.Quality.ToUpperInvariant();
                    TxtViewCount.Text = videoObject.Views + " " + GetText(Resource.String.Lbl_Views);
                    TxtCreateTime.Text = videoObject.Release;

                    TxtCategory.Text = videoObject.Genre;
                    VideoStars.Text = videoObject.Stars;
                    TxtTags.Text = videoObject.Producer;

                    GlideImageLoader.LoadImage(this, UserDetails.Avatar, ImgAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                    TextSanitizerAutoLink.Load(Methods.FunString.DecodeString(videoObject.Description));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                var (apiStatus, respond) = await RequestsAsync.Movies.GetCommentsAsync(MoviesId, "25", offset);
                if (apiStatus != 200 || respond is not GetCommentsMoviesObject result || result.Data == null)
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
                            MAdapter.CommentList = new ObservableCollection<CommentsMoviesObject>(result.Data);
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
                            var d = new CommentsMoviesObject { Text = MAdapter.EmptyState };
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
    }
}