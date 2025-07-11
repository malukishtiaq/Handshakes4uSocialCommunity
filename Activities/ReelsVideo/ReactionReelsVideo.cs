﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.ReelsVideo
{
    public class ReactionReelsVideo : Object, PopupWindow.IOnDismissListener
    {
        private readonly Activity MainContext;
        private PopupWindow PopupWindow;

        //ImagesButton one for every Reaction
        private ImageView MImgButtonOne;
        private ImageView MImgButtonTwo;
        private ImageView MImgButtonThree;
        private ImageView MImgButtonFour;
        private ImageView MImgButtonFive;
        private ImageView MImgButtonSix;

        private TextView ReactionLabel;

        //Array of six Reaction one for every ImageButton Icon
        private readonly List<Reaction> MReactionPack = XReactions.GetReactions();

        private GlobalClickEventArgs PostData;
        private readonly ViewReelsVideoFragment ViewReelsVideoFragment;
        public ReactionReelsVideo(Activity context, ViewReelsVideoFragment reelsVideoFragment)
        {
            MainContext = context;
            ViewReelsVideoFragment = reelsVideoFragment;
        }

        /// <summary>
        /// Show Reaction dialog when user long click on react button
        /// </summary>
        public void ClickDialog(GlobalClickEventArgs postData)
        {
            try
            {
                PostData = postData;

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("appear.mp3");
                        break;
                }

                LayoutInflater layoutInflater = (LayoutInflater)MainContext?.GetSystemService(Context.LayoutInflaterService);
                View popupView = layoutInflater?.Inflate(Resource.Layout.XReactDialogLayout, null);
                popupView?.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);

                PopupWindow = new PopupWindow(popupView, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, true);

                InitializingReactImages(popupView);
                ClickImageButtons();

                PopupWindow.SetBackgroundDrawable(new ColorDrawable());
                PopupWindow.AnimationStyle = Resource.Style.Animation;
                PopupWindow.Focusable = true;
                PopupWindow.ClippingEnabled = true;
                PopupWindow.OutsideTouchable = false;
                PopupWindow.SetOnDismissListener(this);

                var likeLayout = MainContext.FindViewById<LinearLayout>(Resource.Id.likeLayout);
                if (likeLayout != null)
                {
                    likeLayout.OnWindowFocusChanged(true);
                    likeLayout.PostDelayed(() =>
                    {
                        try
                        {
                            // Values should no longer be 0
                            int[] point = new int[2];
                            likeLayout.GetLocationInWindow(point); // or getLocationInWindow(point)
                            int x = point[0];
                            int y = point[1];

                            int offsetY = -400;
                            PopupWindow.ShowAtLocation(likeLayout, GravityFlags.NoGravity, x, y + offsetY);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }, 100);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view">View Object to initialize all ImagesButton</param>
        private void InitializingReactImages(View view)
        {
            try
            {
                MImgButtonOne = view.FindViewById<ImageView>(Resource.Id.imgButtonOne);
                MImgButtonTwo = view.FindViewById<ImageView>(Resource.Id.imgButtonTwo);
                MImgButtonThree = view.FindViewById<ImageView>(Resource.Id.imgButtonThree);
                MImgButtonFour = view.FindViewById<ImageView>(Resource.Id.imgButtonFour);
                MImgButtonFive = view.FindViewById<ImageView>(Resource.Id.imgButtonFive);
                MImgButtonSix = view.FindViewById<ImageView>(Resource.Id.imgButtonSix);

                ReactionLabel = view.FindViewById<TextView>(Resource.Id.reactLabel);
                ReactionLabel.Visibility = ViewStates.Invisible;

                MImgButtonOne.Visibility = ViewStates.Gone;
                MImgButtonTwo.Visibility = ViewStates.Gone;
                MImgButtonThree.Visibility = ViewStates.Gone;
                MImgButtonFour.Visibility = ViewStates.Gone;
                MImgButtonFive.Visibility = ViewStates.Gone;
                MImgButtonSix.Visibility = ViewStates.Gone;

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                        Glide.With(MainContext?.BaseContext).Load(Resource.Drawable.emoji_like).Apply(new RequestOptions().CenterCrop()).Into(MImgButtonOne);
                        Glide.With(MainContext?.BaseContext).Load(Resource.Drawable.emoji_love).Apply(new RequestOptions().CenterCrop()).Into(MImgButtonTwo);
                        Glide.With(MainContext?.BaseContext).Load(Resource.Drawable.emoji_haha).Apply(new RequestOptions().CenterCrop()).Into(MImgButtonThree);
                        Glide.With(MainContext?.BaseContext).Load(Resource.Drawable.emoji_wow).Apply(new RequestOptions().CenterCrop()).Into(MImgButtonFour);
                        Glide.With(MainContext?.BaseContext).Load(Resource.Drawable.emoji_sad).Apply(new RequestOptions().CenterCrop()).Into(MImgButtonFive);
                        Glide.With(MainContext?.BaseContext).Load(Resource.Drawable.emoji_angry).Apply(new RequestOptions().CenterCrop()).Into(MImgButtonSix);
                        break;
                }

                SetTranslateAnimation(MImgButtonOne, ReactConstants.Like);
                SetTranslateAnimation(MImgButtonTwo, ReactConstants.Love);
                SetTranslateAnimation(MImgButtonThree, ReactConstants.HaHa);
                SetTranslateAnimation(MImgButtonFour, ReactConstants.Wow);
                SetTranslateAnimation(MImgButtonFive, ReactConstants.Sad);
                SetTranslateAnimation(MImgButtonSix, ReactConstants.Angry);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetTranslateAnimation(View view, string type)
        {
            try
            {
                bool mCanceled = false;
                // Load the bounce animation from the XML resource
                AnimatorSet animation = (AnimatorSet)AnimatorInflater.LoadAnimator(MainContext, Resource.Animator.reaction_bounce);

                if (type == ReactConstants.Like)
                {
                    animation = (AnimatorSet)AnimatorInflater.LoadAnimator(MainContext, Resource.Animator.reaction_bounce);
                }
                else if (type == ReactConstants.Love)
                {
                    animation = (AnimatorSet)AnimatorInflater.LoadAnimator(MainContext, Resource.Animator.reaction_heart);
                }
                else if (type == ReactConstants.HaHa)
                {
                    animation = (AnimatorSet)AnimatorInflater.LoadAnimator(MainContext, Resource.Animator.reaction_swing);
                }
                else if (type == ReactConstants.Wow)
                {
                    animation = (AnimatorSet)AnimatorInflater.LoadAnimator(MainContext, Resource.Animator.reaction_pulse);
                }
                else if (type == ReactConstants.Sad)
                {
                    animation = (AnimatorSet)AnimatorInflater.LoadAnimator(MainContext, Resource.Animator.reaction_fadeInDown);
                }
                else if (type == ReactConstants.Angry)
                {
                    animation = (AnimatorSet)AnimatorInflater.LoadAnimator(MainContext, Resource.Animator.reaction_headShake);
                }

                animation.AnimationStart += (sender, args) =>
                {
                    mCanceled = false;
                };

                animation.AnimationEnd += (sender, args) =>
                {
                    try
                    {
                        if (!mCanceled)
                        {
                            animation.Start();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };

                animation.AnimationCancel += (sender, args) =>
                {
                    mCanceled = true;
                };

                animation.SetDuration(200);
                animation.SetTarget(view);
                animation.Start();
                view.Visibility = ViewStates.Visible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        /// <summary>
        /// Set onClickListener For every Image Buttons on Reaction Dialog
        /// </summary>
        private void ClickImageButtons()
        {
            try
            {
                ImgButtonSetListener(MImgButtonOne, 0, ReactConstants.Like);
                ImgButtonSetListener(MImgButtonTwo, 1, ReactConstants.Love);
                ImgButtonSetListener(MImgButtonThree, 2, ReactConstants.HaHa);
                ImgButtonSetListener(MImgButtonFour, 3, ReactConstants.Wow);
                ImgButtonSetListener(MImgButtonFive, 4, ReactConstants.Sad);
                ImgButtonSetListener(MImgButtonSix, 5, ReactConstants.Angry);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnDismiss()
        {
            try
            {
                PopupWindow?.Dismiss();
                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("leave.mp3");
                        break;
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgButton">ImageButton view to set onClickListener</param>
        /// <param name="reactIndex">Index of Reaction to take it from ReactionPack</param>
        /// <param name="reactName"></param>
        private void ImgButtonSetListener(ImageView imgButton, int reactIndex, string reactName)
        {
            try
            {
                //if (imgButton != null && !imgButton.HasOnClickListeners)
                //    imgButton.Click += (sender, e) => ImgButtonOnClick(new ReactionsClickEventArgs { ImgButton = imgButton, Position = reactIndex, React = reactName });

                if (imgButton != null && !imgButton.HasOnClickListeners)
                    imgButton.Touch += (sender, e) => ImgButtonOnTouch(new ReactionsTouchEventArgs(e.Handled, e.Event) { ImgButton = imgButton, Position = reactIndex });

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private long Then;
        private readonly int LongClickDuration = 500; //for long click to trigger after 0.5 seconds
        private int PositionSelect = -1;

        private void ImgButtonOnTouch(ReactionsTouchEventArgs e)
        {
            try
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        {
                            Then = Methods.Time.CurrentTimeMillis();
                            //ImgButtonOnLongClick(new ReactionsClickLongClickEventArgs(e.Handled) { ImgButton = e.ImgButton, Position = e.Position });

                            if (ReactionLabel != null)
                            {
                                PositionSelect = e.Position;
                                Reaction data = MReactionPack[e.Position];
                                ReactionLabel.Text = data.GetReactText();
                                ReactionLabel.Visibility = ViewStates.Visible;
                            }

                            switch (UserDetails.SoundControl)
                            {
                                case true:
                                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("down.mp3");
                                    break;
                            }
                            break;
                        }

                    case MotionEventActions.Up:
                        if (Methods.Time.CurrentTimeMillis() - Then > LongClickDuration)
                        {
                            /* Implement long click behavior here */
                            //ImgButtonOnLongClick(new ReactionsClickLongClickEventArgs(e.Handled) { ImgButton = e.ImgButton, Position = e.Position });

                            if (ReactionLabel != null)
                                ReactionLabel.Visibility = ViewStates.Invisible;
                        }
                        else
                        {
                            /* Implement short click behavior here or do nothing */
                            ImgButtonOnClick(new ReactionsClickEventArgs { ImgButton = e.ImgButton, Position = e.Position, React = ReactionLabel.Text });
                        }
                        break;
                    case MotionEventActions.Move when PositionSelect != e.Position:
                        {
                            Then = Methods.Time.CurrentTimeMillis();
                            //ImgButtonOnLongClick(new ReactionsClickLongClickEventArgs(e.Handled) { ImgButton = e.ImgButton, Position = e.Position });

                            if (ReactionLabel != null)
                            {
                                PositionSelect = e.Position;
                                Reaction data = MReactionPack[e.Position];
                                ReactionLabel.Text = data.GetReactText();
                                ReactionLabel.Visibility = ViewStates.Visible;
                            }

                            switch (UserDetails.SoundControl)
                            {
                                case true:
                                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("select.mp3");
                                    break;
                            }
                            break;
                        }
                    case MotionEventActions.Cancel:

                        PositionSelect = -1;

                        if (ReactionLabel != null)
                            ReactionLabel.Visibility = ViewStates.Invisible;

                        switch (UserDetails.SoundControl)
                        {
                            case true:
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("cancel.mp3");
                                break;
                        }
                        break;
                    default:
                        return;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImgButtonOnClick(ReactionsClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("select.mp3");
                        break;
                }

                PostData.NewsFeedClass.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                if (PostData.NewsFeedClass.Reaction.IsReacted != null && !PostData.NewsFeedClass.Reaction.IsReacted.Value)
                {
                    PostData.NewsFeedClass.Reaction.IsReacted = true;
                    PostData.NewsFeedClass.Reaction.Count++;
                }

                if (ViewReelsVideoFragment.TxtLikeCount != null && PostData.NewsFeedClass.Reaction.Count > 0)
                {
                    ViewReelsVideoFragment.TxtLikeCount.Text = Methods.FunString.FormatPriceValue(PostData.NewsFeedClass.Reaction.Count);
                }

                var data = MReactionPack[e.Position];
                if (data != null)
                {
                    //SetReactionPack(PostData.Holder, data.GetReactText());
                }
                ViewReelsVideoFragment.LikeLayout.Tag = "Liked";

                if (e.React == ReactConstants.Like)
                {
                    ViewReelsVideoFragment.ImgLike.SetImageResource(Resource.Drawable.emoji_like);

                    PostData.NewsFeedClass.Reaction.Type = "1";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Like").Value?.Id ?? "1";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.NewsFeedClass.PostId, "reaction", react) });
                }
                else if (e.React == ReactConstants.Love)
                {
                    ViewReelsVideoFragment.ImgLike.SetImageResource(Resource.Drawable.emoji_love);

                    PostData.NewsFeedClass.Reaction.Type = "2";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Love").Value?.Id ?? "2";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.NewsFeedClass.PostId, "reaction", react) });
                }
                else if (e.React == ReactConstants.HaHa)
                {
                    ViewReelsVideoFragment.ImgLike.SetImageResource(Resource.Drawable.emoji_haha);

                    PostData.NewsFeedClass.Reaction.Type = "3";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "HaHa").Value?.Id ?? "3";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.NewsFeedClass.PostId, "reaction", react) });
                }
                else if (e.React == ReactConstants.Wow)
                {
                    ViewReelsVideoFragment.ImgLike.SetImageResource(Resource.Drawable.emoji_wow);

                    PostData.NewsFeedClass.Reaction.Type = "4";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Wow").Value?.Id ?? "4";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.NewsFeedClass.PostId, "reaction", react) });
                }
                else if (e.React == ReactConstants.Sad)
                {
                    ViewReelsVideoFragment.ImgLike.SetImageResource(Resource.Drawable.emoji_sad);

                    PostData.NewsFeedClass.Reaction.Type = "5";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Sad").Value?.Id ?? "5";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.NewsFeedClass.PostId, "reaction", react) });
                }
                else if (e.React == ReactConstants.Angry)
                {
                    ViewReelsVideoFragment.ImgLike.SetImageResource(Resource.Drawable.emoji_angry);
                    ViewReelsVideoFragment.ImgLike.ClearColorFilter();
                    PostData.NewsFeedClass.Reaction.Type = "6";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Angry").Value?.Id ?? "6";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.NewsFeedClass.PostId, "reaction", react) });
                }
                ViewReelsVideoFragment.ImgLike.ClearColorFilter();

                var dataObject = ListUtils.VideoReelsList?.LastOrDefault(a => a.VideoData?.PostId == PostData?.NewsFeedClass?.PostId & a.Type == Classes.ItemType.ReelsVideo);
                if (dataObject?.VideoData != null)
                {
                    dataObject.VideoData.Reaction = PostData.NewsFeedClass.Reaction;
                }

                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == PostData?.NewsFeedClass.Id).ToList();
                switch (dataGlobal?.Count)
                {
                    case > 0:
                        {
                            foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                            {
                                dataClass.PostData.Reaction = PostData.NewsFeedClass.Reaction;

                                adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass));
                            }

                            break;
                        }
                }

                var adapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                var data22 = adapter?.ListDiffer?.Where(a => a.PostData?.Id == PostData?.NewsFeedClass.Id).ToList();
                switch (data22?.Count)
                {
                    case > 0:
                        {
                            foreach (var dataClass in from dataClass in data22 let index = adapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                            {
                                dataClass.PostData.Reaction = PostData.NewsFeedClass.Reaction;

                                adapter.NotifyItemChanged(adapter.ListDiffer.IndexOf(dataClass));
                            }

                            break;
                        }
                }

                PopupWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}