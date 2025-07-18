﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Newtonsoft.Json;
using WoWonder.Activities.ReelsVideo.Adapters;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.MediaPlayers.Exo;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;

namespace WoWonder.Activities.ReelsVideo
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ReelsVideoDetailsActivity : AppCompatActivity
    {
        #region Variables Basic

        private readonly string KeySelectedPage = "KEY_SELECTED_PAGE_REELS";

        private ViewPager2 Pager;

        private ReelsVideoPagerAdapter MAdapter;
        private int SelectedPage, VideosCount;
        private string Type;
        private ObservableCollection<Classes.ReelsVideoClass> DataVideos = new ObservableCollection<Classes.ReelsVideoClass>();
        public PreCachingExoPlayerVideo PreCachingExoPlayerVideo;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                //Overlap();

                if (savedInstanceState != null)
                {
                    SelectedPage = savedInstanceState.GetInt(KeySelectedPage);
                }

                // Create your application here
                SetContentView(Resource.Layout.ReelsVideoLayout);

                TabbedMainActivity.GetInstance()?.SetOnWakeLock();

                PreCachingExoPlayerVideo = new PreCachingExoPlayerVideo(this);

                if (Intent != null)
                {
                    Type = Intent?.GetStringExtra("Type") ?? "";
                    VideosCount = Intent?.GetIntExtra("VideosCount", 0) ?? 0;
                    if (Type == "VideoReels")
                    {
                        foreach (var video in ListUtils.VideoReelsList)
                        {
                            var check = DataVideos.FirstOrDefault(a => a.Id == video.Id);
                            if (check == null)
                            {
                                DataVideos.Add(new Classes.ReelsVideoClass
                                {
                                    Id = video.Id,
                                    Type = Classes.ItemType.ReelsVideo,
                                    VideoData = video.VideoData
                                });
                            }

                            if (AdsGoogle.NativeAdsPool?.Count > 0 && DataVideos.Count % AppSettings.ShowAdNativeReelsCount == 0)
                            {
                                DataVideos.Add(new Classes.ReelsVideoClass
                                {
                                    Type = Classes.ItemType.AdMob,
                                });
                            }
                        }
                    }
                    else
                    {
                        var list = JsonConvert.DeserializeObject<ObservableCollection<PostDataObject>>(Intent?.GetStringExtra("DataItem") ?? "");
                        if (list?.Count > 0)
                        {
                            foreach (var data in list)
                            {
                                var check = DataVideos.FirstOrDefault(a => a.Id == data.Id);
                                if (check == null)
                                {
                                    DataVideos.Add(new Classes.ReelsVideoClass
                                    {
                                        Id = data.Id,
                                        Type = Classes.ItemType.ReelsVideo,
                                        VideoData = data
                                    });
                                }

                                if (AdsGoogle.NativeAdsPool?.Count > 0 && DataVideos.Count % AppSettings.ShowAdNativeReelsCount == 0)
                                {
                                    DataVideos.Add(new Classes.ReelsVideoClass
                                    {
                                        Type = Classes.ItemType.AdMob,
                                    });
                                }
                            }
                        }

                    }
                }

                //Get Value And Set Toolbar
                InitComponent();

                if (Type == "VideoReels")
                    Task.Factory.StartNew(StartApiService);
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
                var instance = ViewReelsVideoFragment.GetInstance();
                instance?.StopVideo();
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
                base.OnStop();
                var instance = ViewReelsVideoFragment.GetInstance();
                instance?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            try
            {
                base.OnSaveInstanceState(outState);
                outState.PutInt(KeySelectedPage, Pager.CurrentItem);
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
                TabbedMainActivity.GetInstance()?.SetOffWakeLock();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                var instance = ViewReelsVideoFragment.GetInstance();
                instance?.StopVideo();
                PreCachingExoPlayerVideo?.Destroy();

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
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void Overlap()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window?.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            try
            {
                Pager = FindViewById<ViewPager2>(Resource.Id.viewpager);

                MAdapter = new ReelsVideoPagerAdapter(this, VideosCount, DataVideos);

                //Pager.CurrentItem = MAdapter.ItemCount;
                //Pager.OffscreenPageLimit = 0;

                Pager.Orientation = ViewPager2.OrientationVertical;
                //Pager.SetPageTransformer(new CustomViewPageTransformer(TransformType.Flow));
                Pager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                Pager.Adapter = MAdapter;
                Pager.Adapter.NotifyDataSetChanged();

                Pager.SetCurrentItem(SelectedPage, false);

                AdsGoogle.AdMobNative ads = new AdsGoogle.AdMobNative();
                ads.BindAdMobNative(this);
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
                Pager = null!;
                MAdapter = null!;
                SelectedPage = 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Result

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                var instance = ViewReelsVideoFragment.GetInstance();
                switch (requestCode)
                {
                    case 2100 when resultCode == Result.Ok:
                        {
                            instance?.TubePlayerView?.ExitFullScreen();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly ReelsVideoDetailsActivity Activity;

            public MyOnPageChangeCallback(ReelsVideoDetailsActivity activity)
            {
                try
                {
                    Activity = activity;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            private int LastPosition = -1;
            public override void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                try
                {
                    base.OnPageScrolled(position, positionOffset, positionOffsetPixels);

                    if (LastPosition == -1)
                    {
                        LastPosition = position;
                    }
                    else
                    {
                        LastPosition = position;
                        var instance = ViewReelsVideoFragment.GetInstance();
                        instance?.StopVideo();
                    }

                    if (position > Activity.DataVideos.Count - 3 && Activity.Type == "VideoReels")
                        Task.Factory.StartNew(Activity.StartApiService);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetAllPostVideo });
        }

        private bool ApiRun;
        private async Task GetAllPostVideo()
        {
            try
            {
                if (!Methods.CheckConnectivity() || ApiRun)
                    return;

                ApiRun = true;
                var postId = ListUtils.VideoReelsList.LastOrDefault()?.VideoData?.PostId ?? "0";

                var (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, postId, "get_news_feed", "", "", "0", "0", "video");
                if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    bool add = false;
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = ListUtils.VideoReelsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            var checkViewed = ListUtils.VideoReelsViewsList.FirstOrDefault(a => a.Id == item.Id);
                            if (checkViewed == null)
                            {
                                if (!AppSettings.ShowYouTubeReels && !string.IsNullOrEmpty(item.PostYoutube))
                                    continue;

                                if (!string.IsNullOrEmpty(item.PostFacebook) || !string.IsNullOrEmpty(item.PostVimeo) || !string.IsNullOrEmpty(item.PostDeepsound) || !string.IsNullOrEmpty(item.PostPlaytube))
                                    continue;

                                add = true;
                                ListUtils.VideoReelsList.Add(new Classes.ReelsVideoClass
                                {
                                    Id = item.Id,
                                    Type = Classes.ItemType.ReelsVideo,
                                    VideoData = item
                                });

                                if (AdsGoogle.NativeAdsPool?.Count > 0 && ListUtils.VideoReelsList.Count % AppSettings.ShowAdNativeReelsCount == 0)
                                {
                                    ListUtils.VideoReelsList.Add(new Classes.ReelsVideoClass
                                    {
                                        Type = Classes.ItemType.AdMob,
                                    });
                                }

                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        Glide.With(this).Load(item.Publisher.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CircleCrop()).Preload();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                    }

                    RunOnUiThread(() =>
                    {
                        try
                        {
                            if (add)
                            {
                                DataVideos = ListUtils.VideoReelsList;
                                MAdapter.UpdateReelsVideoPager(DataVideos.Count, DataVideos);
                                //Pager.Adapter.NotifyDataSetChanged();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });

                    ApiRun = false;
                }
            }
            catch (Exception e)
            {
                ApiRun = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}