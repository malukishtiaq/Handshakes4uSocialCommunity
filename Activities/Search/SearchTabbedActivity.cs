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
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using WoWonder.Activities.Base;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Search.Fragment;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Search
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SearchTabbedActivity : BaseActivity, TextView.IOnEditorActionListener, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        private AppBarLayout AppBarLayout;
        private TabLayout TabLayout;
        public ViewPager2 ViewPager;
        private AutoCompleteTextView SearchView;
        private RecyclerView HashRecyclerView;
        public string DataKey, SearchText = "";
        public string OffsetUser = "", OffsetPage = "", OffsetGroup = "";
        public SearchUserFragment UserTab;
        public SearchPagesFragment PagesTab;
        public SearchGroupsFragment GroupsTab;
        private ImageView ImgFilter;
        private Toolbar Toolbar;
        private HashtagUserAdapter HashTagUserAdapter;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Window?.SetSoftInputMode(SoftInput.AdjustNothing);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.SearchTabbedLayout);

                DataKey = Intent?.GetStringExtra("Key") ?? "Data not available";
                if (DataKey != "Data not available" && !string.IsNullOrEmpty(DataKey))
                {
                    SearchText = DataKey;
                }

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                switch (SearchText)
                {
                    case "Random":
                    case "Random_Groups":
                    case "Random_Pages":
                        SearchText = "a";
                        break;
                    default:
                        {
                            switch (string.IsNullOrEmpty(SearchText))
                            {
                                case false:
                                    Search(SearchText);
                                    break;
                                default:
                                    {
                                        switch (SearchView)
                                        {
                                            case null:
                                                return;
                                        }
                                        //SearchView.SetQuery(SearchText, false);
                                        SearchView.ClearFocus();
                                        //SearchView.OnActionViewCollapsed();
                                        break;
                                    }
                            }

                            break;
                        }
                }
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
                //SearchView.SetQuery("", false);
                SearchView?.ClearFocus();
                //SearchView.OnActionViewCollapsed();

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
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TabLayout = FindViewById<TabLayout>(Resource.Id.Searchtabs);
                ViewPager = FindViewById<ViewPager2>(Resource.Id.Searchviewpager);

                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.mainAppBarLayout);
                AppBarLayout.SetExpanded(true);

                HashRecyclerView = FindViewById<RecyclerView>(Resource.Id.HashRecyler);

                switch (AppSettings.ShowTrendingHashTags)
                {
                    case true when ListUtils.HashTagList?.Count > 0:
                        HashTagUserAdapter = new HashtagUserAdapter(this)
                        {
                            MHashtagList = new ObservableCollection<TrendingHashtag>(ListUtils.HashTagList)
                        };
                        HashRecyclerView.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                        HashRecyclerView.SetAdapter(HashTagUserAdapter);
                        HashTagUserAdapter.ItemClick += HashTagUserAdapterOnItemClick;

                        HashRecyclerView.Visibility = ViewStates.Gone;
                        break;
                    case true:
                        HashRecyclerView.Visibility = ViewStates.Gone;
                        break;
                    default:
                        HashRecyclerView.Visibility = ViewStates.Gone;
                        break;
                }

                ImgFilter = FindViewById<ImageView>(Resource.Id.filter_icon);

                ViewPager.OffscreenPageLimit = 3;
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();
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
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = " ";
                    Toolbar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);
                }

                SearchView = FindViewById<AutoCompleteTextView>(Resource.Id.searchBox);
                SearchView.SetOnEditorActionListener(this);
                //SearchView.ClearFocus();

                //Change text colors
                SearchView.SetHintTextColor(Color.ParseColor(AppSettings.MainColor));
                SearchView.SetTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
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
                        ImgFilter.Click += FloatingActionButtonViewOnClick;
                        break;
                    default:
                        ImgFilter.Click -= FloatingActionButtonViewOnClick;
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
                TabLayout = null!;
                ViewPager = null!;
                AppBarLayout = null!;
                HashRecyclerView = null!;
                Toolbar = null!;
                SearchText = null!;
                OffsetUser = "";
                OffsetPage = "";
                OffsetGroup = "";
                DataKey = "";
                SearchText = "";
                UserTab = null!;
                PagesTab = null!;
                GroupsTab = null!;
                ImgFilter = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab 

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                UserTab = new SearchUserFragment();
                PagesTab = new SearchPagesFragment();
                GroupsTab = new SearchGroupsFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(UserTab, GetText(Resource.String.Lbl_Users));
                if (AppSettings.ShowCommunitiesPages)
                    Adapter.AddFragment(PagesTab, GetText(Resource.String.Lbl_Pages));

                if (AppSettings.ShowCommunitiesGroups)
                    Adapter.AddFragment(GroupsTab, GetText(Resource.String.Lbl_Groups));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                // viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Events

        private void HashTagUserAdapterOnItemClick(object sender, HashtagUserAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = HashTagUserAdapter.GetItem(position);
                            if (item != null)
                            {
                                string id = item.Hash.Replace("#", "").Replace("_", " ");
                                string tag = item?.Tag?.Replace("#", "");
                                var intent = new Intent(this, typeof(HashTagPostsActivity));
                                intent.PutExtra("Id", id);
                                intent.PutExtra("Tag", tag);
                                StartActivity(intent);
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Filter
        private void FloatingActionButtonViewOnClick(object sender, EventArgs e)
        {
            try
            {
                FilterSearchDialogFragment mFragment = new FilterSearchDialogFragment();
                mFragment.Show(SupportFragmentManager, mFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchViewOnQueryTextSubmit(string newText)
        {
            try
            {
                SearchText = newText;

                SearchView.ClearFocus();

                UserTab.MAdapter.UserList.Clear();
                UserTab.MAdapter.NotifyDataSetChanged();

                switch (AppSettings.ShowCommunitiesPages)
                {
                    case true:
                        PagesTab.MAdapter.PageList.Clear();
                        PagesTab.MAdapter.NotifyDataSetChanged();
                        break;
                }

                switch (AppSettings.ShowCommunitiesGroups)
                {
                    case true:
                        GroupsTab.MAdapter.GroupList.Clear();
                        GroupsTab.MAdapter.NotifyDataSetChanged();
                        break;
                }

                OffsetUser = "0";
                OffsetPage = "0";
                OffsetGroup = "0";

                if (Methods.CheckConnectivity())
                {
                    if (UserTab.ProgressBarLoader != null)
                        UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;

                    UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;

                    switch (AppSettings.ShowCommunitiesPages)
                    {
                        case true:
                            {
                                if (PagesTab.ProgressBarLoader != null)
                                    PagesTab.ProgressBarLoader.Visibility = ViewStates.Visible;

                                PagesTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                                break;
                            }
                    }

                    switch (AppSettings.ShowCommunitiesGroups)
                    {
                        case true:
                            {
                                if (GroupsTab.ProgressBarLoader != null)
                                    GroupsTab.ProgressBarLoader.Visibility = ViewStates.Visible;

                                GroupsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                                break;
                            }
                    }

                    Task.Factory.StartNew(StartApiService);
                }
                else
                {
                    UserTab.Inflated ??= UserTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoConnection);
                    switch (x.EmptyStateButton.HasOnClickListeners)
                    {
                        case false:
                            x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x.EmptyStateButton.Click -= TryAgainButton_Click;
                            x.EmptyStateButton.Click += null!;
                            break;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Search 

        public void Search(string text)
        {
            try
            {
                SearchText = text;

                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (Methods.CheckConnectivity())
                    {
                        UserTab.MAdapter?.UserList?.Clear();
                        UserTab.MAdapter?.NotifyDataSetChanged();

                        if (UserTab.ProgressBarLoader != null)
                            UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;

                        UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;

                        if (AppSettings.ShowCommunitiesPages)
                        {
                            PagesTab.MAdapter?.PageList?.Clear();
                            PagesTab.MAdapter?.NotifyDataSetChanged();

                            if (PagesTab.ProgressBarLoader != null)
                                PagesTab.ProgressBarLoader.Visibility = ViewStates.Visible;

                            PagesTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }

                        if (AppSettings.ShowCommunitiesGroups)
                        {
                            GroupsTab.MAdapter?.GroupList?.Clear();
                            GroupsTab.MAdapter?.NotifyDataSetChanged();

                            if (GroupsTab.ProgressBarLoader != null)
                                GroupsTab.ProgressBarLoader.Visibility = ViewStates.Visible;

                            GroupsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }

                        Task.Factory.StartNew(StartApiService);
                    }
                }
                else
                {
                    UserTab.Inflated ??= UserTab.EmptyStateLayout?.Inflate();

                    EmptyStateInflater x1 = new EmptyStateInflater();
                    x1.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (!x1.EmptyStateButton.HasOnClickListeners)
                    {
                        x1.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x1.EmptyStateButton.Click -= TryAgainButton_Click;
                        x1.EmptyStateButton.Click += null!;
                    }

                    x1.EmptyStateButton.Click += TryAgainButton_Click;
                    if (UserTab.EmptyStateLayout != null)
                    {
                        UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }

                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;

                    if (AppSettings.ShowCommunitiesPages)
                    {
                        PagesTab.Inflated ??= PagesTab.EmptyStateLayout?.Inflate();

                        EmptyStateInflater x2 = new EmptyStateInflater();
                        x2.InflateLayout(PagesTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                        if (!x2.EmptyStateButton.HasOnClickListeners)
                        {
                            x2.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x2.EmptyStateButton.Click -= TryAgainButton_Click;
                            x2.EmptyStateButton.Click += null!;
                        }

                        x2.EmptyStateButton.Click += TryAgainButton_Click;
                        if (PagesTab.EmptyStateLayout != null)
                        {
                            PagesTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        PagesTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                    }

                    if (AppSettings.ShowCommunitiesGroups)
                    {
                        GroupsTab.Inflated ??= GroupsTab.EmptyStateLayout?.Inflate();

                        EmptyStateInflater x3 = new EmptyStateInflater();
                        x3.InflateLayout(GroupsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                        if (!x3.EmptyStateButton.HasOnClickListeners)
                        {
                            x3.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x3.EmptyStateButton.Click -= TryAgainButton_Click;
                            x3.EmptyStateButton.Click += null!;
                        }

                        x3.EmptyStateButton.Click += TryAgainButton_Click;
                        if (GroupsTab.EmptyStateLayout != null)
                        {
                            GroupsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        GroupsTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
            {
                if (SearchText != null && SearchText.Contains("@"))
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetDataUserByName });
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
            }
        }

        private async Task GetDataUserByName()
        {
            try
            {
                string userName = SearchText.Replace("@", "");
                var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataByUsernameAsync(userName);
                if (apiStatus != 200 || respond is not GetUserDataByUsernameObject result || result.UserData == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    UserTab.MAdapter.UserList = new ObservableCollection<UserDataObject> { result.UserData };
                    RunOnUiThread(() => { UserTab.MAdapter.NotifyDataSetChanged(); });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task StartSearchRequest()
        {
            if (UserTab.MainScrollEvent.IsLoading) return;

            UserTab.MainScrollEvent.IsLoading = true;
            int countUserList = UserTab.MAdapter.UserList.Count;
            if (AppSettings.ShowCommunitiesPages) PagesTab.MainScrollEvent.IsLoading = true;
            if (AppSettings.ShowCommunitiesGroups) GroupsTab.MainScrollEvent.IsLoading = true;

            var dictionary = new Dictionary<string, string>
            {
                {"user_id", UserDetails.UserId},
                {"limit", "30"},
                {"user_offset", OffsetUser},
                {"group_offset", OffsetGroup},
                {"page_offset", OffsetPage},
                {"gender", UserDetails.SearchGender},
                {"search_key", SearchText},
                {"country", UserDetails.SearchCountry},
                {"status", UserDetails.SearchStatus},
                {"verified", UserDetails.SearchVerified},
                {"filterbyage", UserDetails.SearchFilterByAge},
                {"age_from", UserDetails.SearchAgeFrom},
                {"age_to", UserDetails.SearchAgeTo},
                {"image", UserDetails.SearchProfilePicture},
            };

            var (apiStatus, respond) = await RequestsAsync.Global.SearchAsync(dictionary);
            if (apiStatus == 200)
            {
                if (respond is GetSearchObject result)
                {
                    var respondUserList = result.Users?.Count;
                    if (respondUserList > 0 && countUserList > 0)
                    {
                        foreach (var item in from item in result.Users let check = UserTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                        {
                            UserTab.MAdapter.UserList.Add(item);
                        }

                        RunOnUiThread(() => { UserTab.MAdapter.NotifyItemRangeInserted(countUserList, UserTab.MAdapter.UserList.Count - countUserList); });
                    }
                    else if (respondUserList > 0)
                    {
                        UserTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Users);
                        RunOnUiThread(() => { UserTab.MAdapter.NotifyDataSetChanged(); });
                    }
                    else
                    {
                        if (UserTab.MAdapter.UserList.Count > 10 && !UserTab.MRecycler.CanScrollVertically(1))
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                    }

                    if (AppSettings.ShowCommunitiesPages)
                    {
                        int countPageList = PagesTab.MAdapter.PageList.Count;
                        var respondPageList = result.Pages?.Count;
                        if (respondPageList > 0 && countPageList > 0)
                        {
                            foreach (var item in from item in result.Pages let check = PagesTab.MAdapter.PageList.FirstOrDefault(a => a.PageId == item.PageId) where check == null select item)
                            {
                                PagesTab.MAdapter.PageList.Add(item);
                            }

                            RunOnUiThread(() =>
                            {
                                PagesTab.MAdapter.NotifyItemRangeInserted(countPageList, PagesTab.MAdapter.PageList.Count - countPageList);
                            });
                        }
                        else if (respondPageList > 0)
                        {
                            PagesTab.MAdapter.PageList = new ObservableCollection<PageDataObject>(result.Pages);
                            RunOnUiThread(() => { PagesTab.MAdapter.NotifyDataSetChanged(); });
                        }
                        else
                        {
                            if (PagesTab.MAdapter.PageList.Count > 10 && !PagesTab.MRecycler.CanScrollVertically(1))
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMorePages), ToastLength.Short);
                        }
                    }

                    if (AppSettings.ShowCommunitiesGroups)
                    {
                        int countGroupList = GroupsTab.MAdapter.GroupList.Count;
                        var respondGroupList = result.Groups?.Count;
                        if (respondGroupList > 0 && countGroupList > 0)
                        {
                            foreach (var item in from item in result.Groups let check = GroupsTab.MAdapter.GroupList.FirstOrDefault(a => a.GroupId == item.GroupId) where check == null select item)
                            {
                                GroupsTab.MAdapter.GroupList.Add(item);
                            }

                            RunOnUiThread(() =>
                            {
                                GroupsTab.MAdapter.NotifyItemRangeInserted(countGroupList,
                                    GroupsTab.MAdapter.GroupList.Count - countGroupList);
                            });
                        }
                        else if (respondGroupList > 0)
                        {
                            GroupsTab.MAdapter.GroupList = new ObservableCollection<GroupDataObject>(result.Groups);
                            RunOnUiThread(() => { GroupsTab.MAdapter.NotifyDataSetChanged(); });
                        }
                        else
                        {
                            if (GroupsTab.MAdapter.GroupList.Count > 10 && !GroupsTab.MRecycler.CanScrollVertically(1))
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short);
                        }
                    }
                }
            }
            else
                Methods.DisplayReportResult(this, respond);

            RunOnUiThread(ShowEmptyPage);
            UserTab.MainScrollEvent.IsLoading = false;
            if (AppSettings.ShowCommunitiesPages) PagesTab.MainScrollEvent.IsLoading = false;
            if (AppSettings.ShowCommunitiesGroups) GroupsTab.MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                if (AppSettings.ShowCommunitiesPages) PagesTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                if (AppSettings.ShowCommunitiesGroups) GroupsTab.ProgressBarLoader.Visibility = ViewStates.Gone;

                switch (UserTab.MAdapter.UserList.Count)
                {
                    case > 0:
                        UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            UserTab.Inflated ??= UserTab.EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                                    x.EmptyStateButton.Click -= TryAgainButton_Click;
                                    break;
                            }

                            x.EmptyStateButton.Click += TryAgainButton_Click;
                            UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }

                switch (AppSettings.ShowCommunitiesPages)
                {
                    case true when PagesTab.MAdapter.PageList.Count > 0:
                        PagesTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    case true:
                        {
                            PagesTab.Inflated ??= PagesTab.EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(PagesTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                                    x.EmptyStateButton.Click -= TryAgainButton_Click;
                                    break;
                            }

                            x.EmptyStateButton.Click += TryAgainButton_Click;
                            PagesTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }

                switch (AppSettings.ShowCommunitiesGroups)
                {
                    case true when GroupsTab.MAdapter.GroupList.Count > 0:
                        GroupsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    case true:
                        {
                            GroupsTab.Inflated ??= GroupsTab.EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(GroupsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                                    x.EmptyStateButton.Click -= TryAgainButton_Click;
                                    x.EmptyStateButton.Click += null!;
                                    break;
                            }

                            x.EmptyStateButton.Click += TryAgainButton_Click;
                            GroupsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (UserTab.EmptyStateLayout != null) UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;

                ViewPager.SetCurrentItem(0, true);

                Search("a");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchView.ClearFocus();
                UserTab.MAdapter.UserList.Clear();
                UserTab.MAdapter.NotifyDataSetChanged();
                switch (AppSettings.ShowCommunitiesPages)
                {
                    case true:
                        PagesTab.MAdapter.PageList.Clear();
                        PagesTab.MAdapter.NotifyDataSetChanged();
                        break;
                }

                switch (AppSettings.ShowCommunitiesGroups)
                {
                    case true:
                        GroupsTab.MAdapter.GroupList.Clear();
                        GroupsTab.MAdapter.NotifyDataSetChanged();
                        break;
                }

                OffsetUser = "0";
                OffsetPage = "0";
                OffsetGroup = "0";

                if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchText = "a";
                }

                ViewPager.SetCurrentItem(0, true);

                if (Methods.CheckConnectivity())
                {
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;
                    Task.Factory.StartNew(StartApiService);
                }
                else
                {
                    UserTab.Inflated ??= UserTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    switch (x.EmptyStateButton.HasOnClickListeners)
                    {
                        case false:
                            x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x.EmptyStateButton.Click -= TryAgainButton_Click;
                            x.EmptyStateButton.Click += null!;
                            break;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public bool OnEditorAction(TextView v, [GeneratedEnum] ImeAction actionId, KeyEvent e)
        {
            switch (actionId)
            {
                case ImeAction.Search:
                    SearchText = v.Text;

                    SearchView.ClearFocus();
                    v.ClearFocus();

                    SearchViewOnQueryTextSubmit(SearchText);

                    SearchView.ClearFocus();
                    v.ClearFocus();

                    HideKeyboard();

                    return true;
                default:
                    return false;
            }
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}