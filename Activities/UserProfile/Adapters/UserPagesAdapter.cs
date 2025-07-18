﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.UserProfile.Adapters
{
    public class UserPagesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<UserPagesAdapterClickEventArgs> ItemClick;
        public event EventHandler<UserPagesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PageDataObject> PageList = new ObservableCollection<PageDataObject>();

        public UserPagesAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => PageList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_PageCircle_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PageCircleView, parent, false);
                var vh = new UserPagesAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                    case UserPagesAdapterViewHolder holder:
                        {
                            var item = PageList[position];
                            if (item != null)
                            {
                                if (item.Avatar.Contains("http"))
                                    GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.RoundedCrop, ImagePlaceholders.DrawableUser);
                                else
                                    Glide.With(ActivityContext?.BaseContext).Load(new File(item.Avatar)).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).Error(Resource.Drawable.ImagePlacholder)).Into(holder.Image);

                                if (!string.IsNullOrEmpty(item.PageTitle) || !string.IsNullOrWhiteSpace(item.PageTitle))
                                    holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.PageTitle), 20);
                                else
                                    holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.PageName), 20);

                                if (item.UserId == UserDetails.UserId)
                                    item.IsPageOnwer = true;
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
                    case UserPagesAdapterViewHolder viewHolder:
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
        public PageDataObject GetItem(int position)
        {
            return PageList[position];
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

        private void Click(UserPagesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(UserPagesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PageList[p0];
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

    public class UserPagesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public UserPagesAdapterViewHolder(View itemView, Action<UserPagesAdapterClickEventArgs> clickListener,
            Action<UserPagesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
                Name = MainView.FindViewById<TextView>(Resource.Id.Name);
                IconPage = MainView.FindViewById<TextView>(Resource.Id.Icon);

                //Event
                itemView.Click += (sender, e) => clickListener(new UserPagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new UserPagesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });


                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconPage, IonIconsFonts.IosFlag);
                //#####

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }


        public ImageView Image { get; set; }
        public TextView Name { get; set; }
        public TextView IconPage { get; set; }

        #endregion
    }

    public class UserPagesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}