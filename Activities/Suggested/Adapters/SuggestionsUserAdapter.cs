﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Suggested.Adapters
{
    public class SuggestionsUserAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SuggestionsUserAdapterClickEventArgs> RemoveItemClick;
        public event EventHandler<SuggestionsUserAdapterClickEventArgs> FollowButtonItemClick;
        public event EventHandler<SuggestionsUserAdapterClickEventArgs> ItemClick;
        public event EventHandler<SuggestionsUserAdapterClickEventArgs> ItemLongClick;

        public readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();

        public SuggestionsUserAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_PageCircle_view
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SuggestionsView, parent, false);
                var vh = new SuggestionsUserAdapterViewHolder(itemView, FollowButtonClick, RemoveClick, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case SuggestionsUserAdapterViewHolder holder:
                        {
                            var item = UserList[position];
                            if (item != null)
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                                holder.Username.Text = Methods.FunString.SubStringCutOf("@" + item.Username, 15);
                                holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item), 15);

                                WoWonderTools.SetAddFriendCondition(item, item.IsFollowing, holder.Button);
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case SuggestionsUserAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;

        public UserDataObject GetItem(int position)
        {
            return UserList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void RemoveClick(SuggestionsUserAdapterClickEventArgs args) => RemoveItemClick?.Invoke(this, args);
        private void FollowButtonClick(SuggestionsUserAdapterClickEventArgs args) => FollowButtonItemClick?.Invoke(this, args);
        void Click(SuggestionsUserAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(SuggestionsUserAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                switch (item)
                {
                    case null:
                        return d;
                    default:
                        {
                            switch (string.IsNullOrEmpty(item.Avatar))
                            {
                                case false:
                                    d.Add(item.Avatar);
                                    break;
                            }

                            return d;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class SuggestionsUserAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic


        public View MainView { get; set; }


        public ImageView Image { get; set; }

        public TextView Name { get; set; }
        public TextView Username { get; set; }
        public AppCompatButton Button { get; set; }
        public TextView BtnRemove { get; set; }

        #endregion

        public SuggestionsUserAdapterViewHolder(View itemView, Action<SuggestionsUserAdapterClickEventArgs> followButtonClickListener, Action<SuggestionsUserAdapterClickEventArgs> removeClickListener, Action<SuggestionsUserAdapterClickEventArgs> clickListener, Action<SuggestionsUserAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.people_profile_sos);
                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                Username = MainView.FindViewById<TextView>(Resource.Id.username);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.FollowButton);
                BtnRemove = MainView.FindViewById<TextView>(Resource.Id.btnRemove);

                //Event
                BtnRemove.Click += (sender, e) => removeClickListener(new SuggestionsUserAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, BtnAddUser = Button });
                Button.Click += (sender, e) => followButtonClickListener(new SuggestionsUserAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, BtnAddUser = Button });
                itemView.Click += (sender, e) => clickListener(new SuggestionsUserAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SuggestionsUserAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class SuggestionsUserAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public AppCompatButton BtnAddUser { get; set; }
    }
}