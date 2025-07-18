﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Page;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Pages.Settings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PagesAdminActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private MembersAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private AdView MAdView;
        private UserDataObject ItemUser;
        private TextView InviteFriendsButton;
        private string PageId;
        private PageDataObject PageDataClass;

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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                PageId = Intent?.GetStringExtra("PageId");

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("PageData")))
                    PageDataClass = JsonConvert.DeserializeObject<PageDataObject>(Intent?.GetStringExtra("PageData") ?? "");

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                Task.Factory.StartNew(StartApiService);
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

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                AdsGoogle.LifecycleAdView(MAdView, "Pause");
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                InviteFriendsButton = (TextView)FindViewById(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, InviteFriendsButton, IonIconsFonts.PersonAdd);
                InviteFriendsButton.SetTextColor(Color.White);
                InviteFriendsButton.SetTextSize(ComplexUnitType.Sp, 20f);
                InviteFriendsButton.Visibility = ViewStates.Visible;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Admin);

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
                MAdapter = new MembersAdapter(this)
                {
                    UserList = new ObservableCollection<UserDataObject>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
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
                        MAdapter.MoreItemClick += MAdapterOnItemClick;
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        InviteFriendsButton.Click += InviteFriendsButtonOnClick;
                        break;
                    default:
                        MAdapter.MoreItemClick -= MAdapterOnItemClick;
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        InviteFriendsButton.Click -= InviteFriendsButtonOnClick;
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
                AdsGoogle.LifecycleAdView(MAdView, "Destroy");

                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                EmptyStateLayout = null!;
                Inflated = null!;
                ItemUser = null!;
                InviteFriendsButton = null!;
                PageId = null!;
                MAdView = null!;
                PageDataClass = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Invite Friends for Page
        private void InviteFriendsButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(InviteMembersPageActivity));
                intent.PutExtra("PageId", PageId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //More
        private void MAdapterOnItemClick(object sender, MembersAdapterClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                ItemUser = MAdapter.GetItem(e.Position);
                if (ItemUser != null)
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialAlertDialogBuilder(this);

                    if (PageDataClass.IsPageOnwer != null && PageDataClass.IsPageOnwer.Value)
                    {
                        switch (ItemUser.IsAdmin)
                        {
                            case "0":
                                arrayAdapter.Add(GetText(Resource.String.Lbl_MakeAdmin));
                                break;
                            case "1":
                                arrayAdapter.Add(GetText(Resource.String.Lbl_RemoveAdmin));
                                break;
                            default:
                                arrayAdapter.Add(GetText(Resource.String.Lbl_MakeAdmin));
                                break;
                        }
                    }

                    arrayAdapter.Add(GetText(Resource.String.Lbl_BlockMember));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_ViewProfile));

                    dialogList.SetTitle(Resource.String.Lbl_More);
                    dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                    dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                    dialogList.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Members 

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadMembersAsync });
        }

        private async Task LoadMembersAsync()
        {
            if (Methods.CheckConnectivity())
            {
                var countList = MAdapter.UserList.Count;
                var (apiStatus, respond) = await RequestsAsync.Page.GetNotInPageMembersAsync(PageId);
                if (apiStatus != 200 || respond is not GetPageMembersObject result || result.Users == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Users.Count;
                    switch (respondList)
                    {
                        case > 0 when countList > 0:
                            {
                                foreach (var item in from item in result.Users let check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    MAdapter.UserList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.UserList.Count - countList); });
                                break;
                            }
                        case > 0:
                            MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Users);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            break;
                        default:
                            {
                                switch (MAdapter.UserList.Count)
                                {
                                    case > 10 when !MRecycler.CanScrollVertically(1):
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                                        break;
                                }

                                break;
                            }
                    }
                }

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                switch (x.EmptyStateButton.HasOnClickListeners)
                {
                    case false:
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                        break;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter.UserList.Count)
                {
                    case > 0:
                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoUsers);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click += null!;
                                    break;
                            }
                            EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Task.Factory.StartNew(StartApiService);
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
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                if (itemString == GetText(Resource.String.Lbl_MakeAdmin))
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.MakePageAdminAsync(PageId, ItemUser.UserId) });

                    var local = MAdapter?.UserList?.FirstOrDefault(a => a.UserId == ItemUser.UserId);
                    if (local != null)
                    {
                        ItemUser.IsAdmin = "1";
                        MAdapter?.NotifyItemChanged(MAdapter.UserList.IndexOf(local));
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_RemoveAdmin))
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.MakePageAdminAsync(PageId, ItemUser.UserId) });

                    var local = MAdapter?.UserList?.FirstOrDefault(a => a.UserId == ItemUser.UserId);
                    if (local != null)
                    {
                        MAdapter.UserList.Remove(local);
                        MAdapter?.NotifyItemRemoved(MAdapter.UserList.IndexOf(local));
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_BlockMember))
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.BlockUserAsync(PageId, true) });

                    var local = MAdapter?.UserList?.FirstOrDefault(a => a.UserId == ItemUser.UserId);
                    if (local != null)
                    {
                        MAdapter.UserList.Remove(local);
                        MAdapter?.NotifyItemRemoved(MAdapter.UserList.IndexOf(local));
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_ViewProfile))
                {
                    WoWonderTools.OpenProfile(this, ItemUser.UserId, ItemUser);
                }

                switch (MAdapter?.UserList?.Count)
                {
                    case 0:
                        ShowEmptyPage();
                        break;
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