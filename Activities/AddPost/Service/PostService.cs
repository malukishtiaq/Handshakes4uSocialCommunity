﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Events;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Story;
using WoWonderClient.JobWorker;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.AddPost.Service
{
    [Service(Exported = false)]
    public class PostService : Android.App.Service
    {
        #region Variables Basic

        public string ActionPostService;
        public static string ActionPost;
        public static string ActionStory;
        private static string PagePost;
        private static PostService Service;

        private TabbedMainActivity GlobalContextTabbed;
        public FileModel DataPost;

        #endregion

        #region General

        public static PostService GetPostService()
        {
            return Service;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null!;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                Service = this;


                GlobalContextTabbed = TabbedMainActivity.GetInstance();
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                Create_Progress_Notification();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                base.OnStartCommand(intent, flags, startId);

                ActionPostService = intent.Action;
                var data = intent.GetStringExtra("DataPost");
                PagePost = intent.GetStringExtra("PagePost") ?? "";

                switch (string.IsNullOrEmpty(data))
                {
                    case false:
                        {
                            DataPost = JsonConvert.DeserializeObject<FileModel>(data);
                            if (ActionPostService == ActionPost)
                            {
                                if (DataPost != null)
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { AddPost });
                                    UpdateNotification("Post");
                                }
                            }
                            else if (ActionPostService == ActionStory)
                            {
                                if (DataPost != null)
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { AddStory });
                                    UpdateNotification("Story");
                                }
                            }

                            break;
                        }
                }

                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return StartCommandResult.NotSticky;
            }
        }

        public async Task AddPost()
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Posts.AddNewPostAsync(UserDetails.UserId, DataPost.IdPost, DataPost.PagePost, DataPost.Content, DataPost.PostPrivacy, DataPost.PostFeelingType, DataPost.PostFeelingText, DataPost.PlaceText, DataPost.AttachmentList, DataPost.AnswersList, DataPost.IdColor, DataPost.AlbumName);
                if (apiStatus == 200)
                {
                    if (respond is AddPostObject postObject)
                    {
                        ToastUtils.ShowToast(Application.Context, Application.Context.GetText(Resource.String.Lbl_Post_Added), ToastLength.Long);

                        if (UserDetails.SoundControl)
                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("PopNotificationPost.mp3");

                        GlobalContextTabbed?.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (postObject.PostData != null)
                                {
                                    postObject.PostData.Reaction ??= new Reaction();

                                    if (PagePost is "Normal" or "Normal_More" or "Normal_Gallery")
                                    {
                                        var combine = new FeedCombiner(ApiPostAsync.RegexFilterText(postObject.PostData), GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer, this, NativeFeedType.Global);

                                        var check = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                                        if (check != null)
                                            combine.CombineDefaultPostSections("Top");
                                        else
                                            combine.CombineDefaultPostSections();

                                        var emptyStateChecker = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                        if (emptyStateChecker != null && GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.Count > 1)
                                            GlobalContextTabbed.NewsFeedTab.MainRecyclerView.RemoveByRowIndex(emptyStateChecker);

                                        GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.NotifyDataSetChanged();

                                        if (DataPost.IdPost == UserDetails.UserId)
                                        {
                                            MyProfileActivity myProfileActivity = MyProfileActivity.GetInstance();
                                            if (myProfileActivity == null)
                                                return;

                                            var combine1 = new FeedCombiner(ApiPostAsync.RegexFilterText(postObject.PostData), myProfileActivity.PostFeedAdapter?.ListDiffer, this, NativeFeedType.User);

                                            var check1 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                                            if (check1 != null)
                                                combine1.CombineDefaultPostSections("Top");
                                            else
                                                combine1.CombineDefaultPostSections();

                                            var emptyStateChecker1 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                            if (emptyStateChecker1 != null && myProfileActivity.PostFeedAdapter?.ListDiffer?.Count > 1)
                                                myProfileActivity.MainRecyclerView.RemoveByRowIndex(emptyStateChecker1);

                                            myProfileActivity.PostFeedAdapter?.NotifyDataSetChanged();
                                        }
                                    }
                                    else if (PagePost is "SocialGroup" or "SocialGroup_More" or "SocialGroup_Gallery")
                                    {
                                        GroupProfileActivity groupProfileActivity = GroupProfileActivity.GetInstance();
                                        if (groupProfileActivity == null)
                                            return;

                                        var combine = new FeedCombiner(ApiPostAsync.RegexFilterText(postObject.PostData), groupProfileActivity.PostFeedAdapter?.ListDiffer, this, NativeFeedType.Group);

                                        var check = groupProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                                        if (check != null)
                                            combine.CombineDefaultPostSections("Top");
                                        else
                                            combine.CombineDefaultPostSections();

                                        var emptyStateChecker = groupProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                        if (emptyStateChecker != null && groupProfileActivity.PostFeedAdapter?.ListDiffer?.Count > 1)
                                            groupProfileActivity.MainRecyclerView.RemoveByRowIndex(emptyStateChecker);

                                        groupProfileActivity.PostFeedAdapter?.NotifyDataSetChanged();
                                    }
                                    else if (PagePost is "SocialPage" or "SocialPage_More" or "SocialPage_Gallery")
                                    {
                                        PageProfileActivity pageProfileActivity = PageProfileActivity.GetInstance();
                                        if (pageProfileActivity == null)
                                            return;

                                        var combine = new FeedCombiner(ApiPostAsync.RegexFilterText(postObject.PostData), pageProfileActivity.PostFeedAdapter?.ListDiffer, this, NativeFeedType.Page);

                                        var check = pageProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                                        if (check != null)
                                            combine.CombineDefaultPostSections("Top");
                                        else
                                            combine.CombineDefaultPostSections();

                                        var emptyStateChecker = pageProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                        if (emptyStateChecker != null && pageProfileActivity.PostFeedAdapter?.ListDiffer?.Count > 1)
                                            pageProfileActivity.MainRecyclerView.RemoveByRowIndex(emptyStateChecker);

                                        pageProfileActivity.PostFeedAdapter?.NotifyDataSetChanged();
                                    }
                                    else if (PagePost is "SocialEvent" or "SocialEvent_More" or "SocialEvent_Gallery")
                                    {
                                        EventViewActivity eventViewActivity = EventViewActivity.GetInstance();
                                        if (eventViewActivity == null)
                                            return;

                                        var combine = new FeedCombiner(ApiPostAsync.RegexFilterText(postObject.PostData), eventViewActivity.PostFeedAdapter?.ListDiffer, this, NativeFeedType.Event);

                                        var check = eventViewActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                                        if (check != null)
                                            combine.CombineDefaultPostSections("Top");
                                        else
                                            combine.CombineDefaultPostSections();

                                        var emptyStateChecker = eventViewActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                        if (emptyStateChecker != null && eventViewActivity.PostFeedAdapter?.ListDiffer?.Count > 1)
                                            eventViewActivity.MainRecyclerView.RemoveByRowIndex(emptyStateChecker);

                                        eventViewActivity.PostFeedAdapter?.NotifyDataSetChanged();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
                else if (apiStatus == 201)
                {
                    ToastUtils.ShowToast(Application.Context, GetText(Resource.String.Lbl_VideoUploadWithProcessed), ToastLength.Long);
                }
                else
                {
                    RemoveNotification();
                    Methods.DisplayReportResult(GlobalContextTabbed, respond);
                }

                RemoveNotification();
            }
            catch (Exception e)
            {
                RemoveNotification();
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task AddStory()
        {
            try
            {
                var postFeedAdapter = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter;
                var checkSection = postFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                if (checkSection != null)
                {
                    var modelStory = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.HolderStory.StoryAdapter;

                    string time = Methods.Time.TimeAgo(DateTime.Now, false);
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time2 = unixTimestamp.ToString();

                    var userData = ListUtils.MyProfileList?.FirstOrDefault();

                    //just pass file_path and type video or image
                    var (apiStatus, respond) = await RequestsAsync.Story.CreateStoryAsync(DataPost.StoryTitle, DataPost.StoryDescription, DataPost.StoryFilePath, DataPost.StoryFileType, DataPost.StoryThumbnail);
                    if (apiStatus == 200)
                    {
                        if (respond is CreateStoryObject result)
                        {
                            ToastUtils.ShowToast(Application.Context, Application.Context.GetText(Resource.String.Lbl_Story_Added), ToastLength.Short);

                            if (UserDetails.SoundControl)
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("PopNotificationPost.mp3");

                            GlobalContextTabbed?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    var check = modelStory?.StoryList?.FirstOrDefault(a => a.UserId == UserDetails.UserId);
                                    if (check != null)
                                    {
                                        if (DataPost.StoryFileType == "image")
                                        {
                                            var item = new UserDataStory
                                            {
                                                UserId = UserDetails.UserId,
                                                Id = result.StoryId,
                                                Description = DataPost.StoryDescription,
                                                Title = DataPost.StoryTitle,
                                                TimeText = time,
                                                IsOwner = true,
                                                Expire = "",
                                                Posted = time2,
                                                Thumbnail = DataPost.StoryFilePath,
                                                UserData = userData,
                                                ViewCount = "0",
                                                Images = new List<ImageDataStoryObject>(),
                                                Videos = new List<VideoDataStoryObject>(),
                                                Reaction = new Reaction(),
                                                TypeView = "Image",
                                            };

                                            check.DurationsList ??= new List<long>();
                                            check.DurationsList.Add(AppSettings.StoryImageDuration * 1000);

                                            check.Stories.Add(item);
                                        }
                                        else
                                        {
                                            var item = new UserDataStory
                                            {
                                                UserId = UserDetails.UserId,
                                                Id = result.StoryId,
                                                Description = DataPost.StoryDescription,
                                                Title = DataPost.StoryTitle,
                                                TimeText = time,
                                                IsOwner = true,
                                                Expire = "",
                                                Posted = time2,
                                                Thumbnail = DataPost.StoryThumbnail,
                                                UserData = userData,
                                                ViewCount = "0",
                                                Images = new List<ImageDataStoryObject>(),
                                                Videos = new List<VideoDataStoryObject>
                                        {
                                            new VideoDataStoryObject
                                            {
                                                StoryId = result.StoryId,
                                                Filename = DataPost.StoryFilePath,
                                                Id = time2,
                                                Expire = time2,
                                                Type = "video",
                                            }
                                        },
                                                Reaction = new Reaction(),
                                                TypeView = "Video",
                                            };

                                            check.DurationsList ??= new List<long>();
                                            if (AppSettings.ShowFullVideo)
                                            {
                                                var duration = WoWonderTools.GetDuration(DataPost.StoryFilePath);
                                                check.DurationsList.Add(Long.ParseLong(duration));
                                            }
                                            else
                                            {
                                                check.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                            }

                                            check.Stories.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        if (DataPost.StoryFileType == "image")
                                        {
                                            var item = new StoryDataObject
                                            {
                                                Type = "image",
                                                Stories = new List<UserDataStory>
                                        {
                                            new UserDataStory
                                            {
                                                UserId = UserDetails.UserId,
                                                Id = result.StoryId,
                                                Description = DataPost.StoryDescription,
                                                Title = DataPost.StoryTitle,
                                                TimeText = time,
                                                IsOwner = true,
                                                Expire = "",
                                                Posted = time2,
                                                Thumbnail = DataPost.StoryFilePath,
                                                UserData = userData,
                                                ViewCount = "0",
                                                Images = new List<ImageDataStoryObject>(),
                                                Videos = new List<VideoDataStoryObject>(),
                                                Reaction = new Reaction(),
                                                TypeView = "Image",
                                            }
                                        },
                                                UserId = userData?.UserId,
                                                Username = userData?.Username,
                                                Email = userData?.Email,
                                                FirstName = userData?.FirstName,
                                                LastName = userData?.LastName,
                                                Avatar = userData?.Avatar,
                                                Cover = userData?.Cover,
                                                BackgroundImage = userData?.BackgroundImage,
                                                RelationshipId = userData?.RelationshipId,
                                                Address = userData?.Address,
                                                Working = userData?.Working,
                                                Gender = userData?.Gender,
                                                Facebook = userData?.Facebook,
                                                Google = userData?.Google,
                                                Twitter = userData?.Twitter,
                                                Linkedin = userData?.Linkedin,
                                                Website = userData?.Website,
                                                Instagram = userData?.Instagram,
                                                WebDeviceId = userData?.WebDeviceId,
                                                Language = userData?.Language,
                                                IpAddress = userData?.IpAddress,
                                                PhoneNumber = userData?.PhoneNumber,
                                                Timezone = userData?.Timezone,
                                                Lat = userData?.Lat,
                                                Lng = userData?.Lng,
                                                About = userData?.About,
                                                Birthday = userData?.Birthday,
                                                Registered = userData?.Registered,
                                                Lastseen = userData?.Lastseen,
                                                LastLocationUpdate = userData?.LastLocationUpdate,
                                                Balance = userData?.Balance,
                                                Verified = userData?.Verified,
                                                Status = userData?.Status,
                                                Active = userData?.Active,
                                                Admin = userData?.Admin,
                                                IsPro = userData?.IsPro,
                                                ProType = userData?.ProType,
                                                School = userData?.School,
                                                Name = userData?.Name,
                                                AndroidMDeviceId = userData?.AndroidMDeviceId,
                                                ECommented = userData?.ECommented,
                                                AndroidNDeviceId = userData?.AndroidMDeviceId,
                                                AvatarFull = userData?.AvatarFull,
                                                BirthPrivacy = userData?.BirthPrivacy,
                                                CanFollow = userData?.CanFollow,
                                                ConfirmFollowers = userData?.ConfirmFollowers,
                                                CountryId = userData?.CountryId,
                                                EAccepted = userData?.EAccepted,
                                                EFollowed = userData?.EFollowed,
                                                EJoinedGroup = userData?.EJoinedGroup,
                                                ELastNotif = userData?.ELastNotif,
                                                ELiked = userData?.ELiked,
                                                ELikedPage = userData?.ELikedPage,
                                                EMentioned = userData?.EMentioned,
                                                EProfileWallPost = userData?.EProfileWallPost,
                                                ESentmeMsg = userData?.ESentmeMsg,
                                                EShared = userData?.EShared,
                                                EVisited = userData?.EVisited,
                                                EWondered = userData?.EWondered,
                                                EmailNotification = userData?.EmailNotification,
                                                FollowPrivacy = userData?.FollowPrivacy,
                                                FriendPrivacy = userData?.FriendPrivacy,
                                                GenderText = userData?.GenderText,
                                                InfoFile = userData?.InfoFile,
                                                IosMDeviceId = userData?.IosMDeviceId,
                                                IosNDeviceId = userData?.IosNDeviceId,
                                                IsFollowing = userData?.IsFollowing,
                                                IsFollowingMe = userData?.IsFollowingMe,
                                                LastAvatarMod = userData?.LastAvatarMod,
                                                LastCoverMod = userData?.LastCoverMod,
                                                LastDataUpdate = userData?.LastDataUpdate,
                                                LastFollowId = userData?.LastFollowId,
                                                LastLoginData = userData?.LastLoginData,
                                                LastseenStatus = userData?.LastseenStatus,
                                                LastseenTimeText = userData?.LastseenTimeText,
                                                LastseenUnixTime = userData?.LastseenUnixTime,
                                                MessagePrivacy = userData?.MessagePrivacy,
                                                NewEmail = userData?.NewEmail,
                                                NewPhone = userData?.NewPhone,
                                                NotificationsSound = userData?.NotificationsSound,
                                                OrderPostsBy = userData?.OrderPostsBy,
                                                PaypalEmail = userData?.PaypalEmail,
                                                PostPrivacy = userData?.PostPrivacy,
                                                Referrer = userData?.Referrer,
                                                ShareMyData = userData?.ShareMyData,
                                                ShareMyLocation = userData?.ShareMyLocation,
                                                ShowActivitiesPrivacy = userData?.ShowActivitiesPrivacy,
                                                TwoFactor = userData?.TwoFactor,
                                                TwoFactorVerified = userData?.TwoFactorVerified,
                                                Url = userData?.Url,
                                                VisitPrivacy = userData?.VisitPrivacy,
                                                Vk = userData?.Vk,
                                                Wallet = userData?.Wallet,
                                                WorkingLink = userData?.WorkingLink,
                                                Youtube = userData?.Youtube,
                                                City = userData?.City,
                                                Points = userData?.Points,
                                                DailyPoints = userData?.DailyPoints,
                                                PointDayExpire = userData?.PointDayExpire,
                                                State = userData?.State,
                                                Zip = userData?.Zip,
                                                Details = new DetailsUnion
                                                {
                                                    DetailsClass = new Details(),
                                                },
                                                ApiNotificationSettings = new NotificationSettingsUnion
                                                {
                                                    NotificationSettingsClass = new NotificationSettings()
                                                },
                                            };

                                            item.DurationsList ??= new List<long>();
                                            item.DurationsList.Add(AppSettings.StoryImageDuration * 1000);

                                            modelStory?.StoryList?.Add(item);
                                        }
                                        else
                                        {
                                            var item = new StoryDataObject
                                            {
                                                Type = "video",
                                                Stories = new List<UserDataStory>
                                        {
                                            new UserDataStory
                                            {
                                                UserId = UserDetails.UserId,
                                                Id = result.StoryId,
                                                Description = DataPost.StoryDescription,
                                                Title = DataPost.StoryTitle,
                                                TimeText = time,
                                                IsOwner = true,
                                                Expire = "",
                                                Posted = time2,
                                                Thumbnail = DataPost.StoryThumbnail,
                                                UserData = userData,
                                                ViewCount = "0",
                                                Images = new List<ImageDataStoryObject>(),
                                                Videos = new List<VideoDataStoryObject>
                                                {
                                                    new VideoDataStoryObject
                                                    {
                                                        StoryId = result.StoryId,
                                                        Filename = DataPost.StoryFilePath,
                                                        Id = time2,
                                                        Expire = time2,
                                                        Type = "video",
                                                    }
                                                },
                                                Reaction = new Reaction(),
                                                TypeView = "Video",
                                            }
                                        },
                                                UserId = userData?.UserId,
                                                Username = userData?.Username,
                                                Email = userData?.Email,
                                                FirstName = userData?.FirstName,
                                                LastName = userData?.LastName,
                                                Avatar = userData?.Avatar,
                                                Cover = userData?.Cover,
                                                BackgroundImage = userData?.BackgroundImage,
                                                RelationshipId = userData?.RelationshipId,
                                                Address = userData?.Address,
                                                Working = userData?.Working,
                                                Gender = userData?.Gender,
                                                Facebook = userData?.Facebook,
                                                Google = userData?.Google,
                                                Twitter = userData?.Twitter,
                                                Linkedin = userData?.Linkedin,
                                                Website = userData?.Website,
                                                Instagram = userData?.Instagram,
                                                WebDeviceId = userData?.WebDeviceId,
                                                Language = userData?.Language,
                                                IpAddress = userData?.IpAddress,
                                                PhoneNumber = userData?.PhoneNumber,
                                                Timezone = userData?.Timezone,
                                                Lat = userData?.Lat,
                                                Lng = userData?.Lng,
                                                About = userData?.About,
                                                Birthday = userData?.Birthday,
                                                Registered = userData?.Registered,
                                                Lastseen = userData?.Lastseen,
                                                LastLocationUpdate = userData?.LastLocationUpdate,
                                                Balance = userData?.Balance,
                                                Verified = userData?.Verified,
                                                Status = userData?.Status,
                                                Active = userData?.Active,
                                                Admin = userData?.Admin,
                                                IsPro = userData?.IsPro,
                                                ProType = userData?.ProType,
                                                School = userData?.School,
                                                Name = userData?.Name,
                                                AndroidMDeviceId = userData?.AndroidMDeviceId,
                                                ECommented = userData?.ECommented,
                                                AndroidNDeviceId = userData?.AndroidMDeviceId,
                                                AvatarFull = userData?.AvatarFull,
                                                BirthPrivacy = userData?.BirthPrivacy,
                                                CanFollow = userData?.CanFollow,
                                                ConfirmFollowers = userData?.ConfirmFollowers,
                                                CountryId = userData?.CountryId,
                                                EAccepted = userData?.EAccepted,
                                                EFollowed = userData?.EFollowed,
                                                EJoinedGroup = userData?.EJoinedGroup,
                                                ELastNotif = userData?.ELastNotif,
                                                ELiked = userData?.ELiked,
                                                ELikedPage = userData?.ELikedPage,
                                                EMentioned = userData?.EMentioned,
                                                EProfileWallPost = userData?.EProfileWallPost,
                                                ESentmeMsg = userData?.ESentmeMsg,
                                                EShared = userData?.EShared,
                                                EVisited = userData?.EVisited,
                                                EWondered = userData?.EWondered,
                                                EmailNotification = userData?.EmailNotification,
                                                FollowPrivacy = userData?.FollowPrivacy,
                                                FriendPrivacy = userData?.FriendPrivacy,
                                                GenderText = userData?.GenderText,
                                                InfoFile = userData?.InfoFile,
                                                IosMDeviceId = userData?.IosMDeviceId,
                                                IosNDeviceId = userData?.IosNDeviceId,
                                                IsFollowing = userData?.IsFollowing,
                                                IsFollowingMe = userData?.IsFollowingMe,
                                                LastAvatarMod = userData?.LastAvatarMod,
                                                LastCoverMod = userData?.LastCoverMod,
                                                LastDataUpdate = userData?.LastDataUpdate,
                                                LastFollowId = userData?.LastFollowId,
                                                LastLoginData = userData?.LastLoginData,
                                                LastseenStatus = userData?.LastseenStatus,
                                                LastseenTimeText = userData?.LastseenTimeText,
                                                LastseenUnixTime = userData?.LastseenUnixTime,
                                                MessagePrivacy = userData?.MessagePrivacy,
                                                NewEmail = userData?.NewEmail,
                                                NewPhone = userData?.NewPhone,
                                                NotificationsSound = userData?.NotificationsSound,
                                                OrderPostsBy = userData?.OrderPostsBy,
                                                PaypalEmail = userData?.PaypalEmail,
                                                PostPrivacy = userData?.PostPrivacy,
                                                Referrer = userData?.Referrer,
                                                ShareMyData = userData?.ShareMyData,
                                                ShareMyLocation = userData?.ShareMyLocation,
                                                ShowActivitiesPrivacy = userData?.ShowActivitiesPrivacy,
                                                TwoFactor = userData?.TwoFactor,
                                                TwoFactorVerified = userData?.TwoFactorVerified,
                                                Url = userData?.Url,
                                                VisitPrivacy = userData?.VisitPrivacy,
                                                Vk = userData?.Vk,
                                                Wallet = userData?.Wallet,
                                                WorkingLink = userData?.WorkingLink,
                                                Youtube = userData?.Youtube,
                                                City = userData?.City,
                                                Points = userData?.Points,
                                                DailyPoints = userData?.DailyPoints,
                                                State = userData?.State,
                                                Zip = userData?.Zip,
                                                Details = new DetailsUnion
                                                {
                                                    DetailsClass = new Details(),
                                                },
                                                ApiNotificationSettings = new NotificationSettingsUnion
                                                {
                                                    NotificationSettingsClass = new NotificationSettings()
                                                },
                                            };

                                            item.DurationsList ??= new List<long>();
                                            if (AppSettings.ShowFullVideo)
                                            {
                                                var duration = WoWonderTools.GetDuration(DataPost.StoryFilePath);
                                                item.DurationsList.Add(Long.ParseLong(duration));
                                            }
                                            else
                                            {
                                                item.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                            }

                                            modelStory?.StoryList?.Add(item);
                                        }
                                    }

                                    modelStory?.NotifyDataSetChanged();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });

                           
                        }
                    }
                    else
                    {
                        RemoveNotification();
                        Methods.DisplayReportResult(GlobalContextTabbed, respond);
                    }
                }

                RemoveNotification();
            }
            catch (Exception e)
            {
                RemoveNotification();
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Notification

        private readonly string NotificationChannelId = "wowonder_ch_1";

        private NotificationManager MNotificationManager;
        private NotificationCompat.Builder NotificationBuilder;
        private void Create_Progress_Notification()
        {
            try
            {
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                //NotificationView = new RemoteViews(PackageName, Resource.Layout.ViewProgressNotification);

                Intent resultIntent = new Intent();
                PendingIntent resultPendingIntent = PendingIntent.GetActivity(this, 0, resultIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);
                NotificationBuilder = new NotificationCompat.Builder(this, NotificationChannelId);
                NotificationBuilder.SetSmallIcon(Resource.Mipmap.icon);
                NotificationBuilder.SetColor(ContextCompat.GetColor(this, Resource.Color.accent));
                // NotificationBuilder.SetCustomContentView(NotificationView)
                NotificationBuilder.SetOngoing(true)
                    .SetContentIntent(resultPendingIntent)
                    .SetDefaults(NotificationCompat.DefaultAll)
                    .SetPriority((int)NotificationPriority.High);

                NotificationBuilder.SetVibrate(new[] { 0L });
                NotificationBuilder.SetVisibility(NotificationCompat.VisibilityPublic);

                NotificationBuilder.SetProgress(100, 10, true);
                switch (Build.VERSION.SdkInt)
                {
                    case >= BuildVersionCodes.O:
                        {
                            var importance = NotificationImportance.High;
                            NotificationChannel notificationChannel = new NotificationChannel(NotificationChannelId, AppSettings.ApplicationName, importance);
                            notificationChannel.EnableLights(false);
                            notificationChannel.EnableVibration(false);
                            NotificationBuilder.SetChannelId(NotificationChannelId);

                            MNotificationManager?.CreateNotificationChannel(notificationChannel);
                            break;
                        }
                }

                MNotificationManager?.Notify(2020, NotificationBuilder.Build());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RemoveNotification()
        {
            try
            {
                NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager?.Cancel(2020);

                MNotificationManager?.CancelAll();
                StopSelf();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateNotification(string type)
        {
            try
            {
                switch (type)
                {
                    case "Post":
                        NotificationBuilder.SetContentTitle(GetString(Resource.String.Lbl_UploadingPost));
                        break;
                    case "Story":
                        NotificationBuilder.SetContentTitle(GetString(Resource.String.Lbl_UploadingStory));
                        break;
                }

                MNotificationManager?.Notify(2020, NotificationBuilder.Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}