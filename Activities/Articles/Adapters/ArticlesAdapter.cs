﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using ImageViews.Rounded;
using Java.Util;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Articles.Adapters
{
    public class ArticlesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {

        private readonly Activity ActivityContext;

        public ObservableCollection<ArticleDataObject> ArticlesList =
            new ObservableCollection<ArticleDataObject>();

        public ArticlesAdapter(Activity context)
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

        public override int ItemCount => ArticlesList?.Count ?? 0;

        public event EventHandler<ArticlesAdapterClickEventArgs> UserItemClick;
        public event EventHandler<ArticlesAdapterClickEventArgs> ItemClick;
        public event EventHandler<ArticlesAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Article_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ArticleView, parent, false);
                var vh = new ArticlesAdapterViewHolder(itemView, UserClick, Click, LongClick);
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
                    case ArticlesAdapterViewHolder holder:
                        {
                            var item = ArticlesList[position];
                            if (item != null) Initialize(holder, item);
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Initialize(ArticlesAdapterViewHolder holder, ArticleDataObject item)
        {
            try
            {

                var colorImage = Color.ParseColor(Methods.FunString.RandomColor().Item1);

                Glide.With(ActivityContext?.BaseContext)
                    .Load(item.Thumbnail)
                    .Apply(RequestOptions.CenterCropTransform().Placeholder(new ColorDrawable(colorImage)).Fallback(new ColorDrawable(colorImage)).SetPriority(Priority.High))
                    .Into(holder.Image);

                Glide.With(ActivityContext?.BaseContext)
                    .Load(item.Author.Avatar)
                    .Apply(RequestOptions.CircleCropTransform())
                    .Into(holder.UserImageProfile);

                holder.Category.Background.SetTint(colorImage);

                CategoriesController cat = new CategoriesController();
                string id = item.CategoryLink.Split('/').Last();

                holder.Category.Text = cat.Get_Translate_Categories_Communities(id, item.CategoryName, "Blog");

                holder.Description.Text = Methods.FunString.DecodeString(item.Description);
                holder.Title.Text = Methods.FunString.DecodeString(item.Title);

                holder.Username.Text = WoWonderTools.GetNameFinal(item.Author);

                if (AppSettings.FlowDirectionRightToLeft)
                    holder.Username.SetCompoundDrawablesWithIntrinsicBounds(item.Author.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0, 0, 0);
                else
                    holder.Username.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Author.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                holder.ViewMore.Text = ActivityContext.GetText(Resource.String.Lbl_ReadMore) + " >"; //READ MORE &gt; 
                holder.Time.Text = ActivityContext.GetText(Resource.String.Lbl_Posted) + " " + item.Posted;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public override void OnViewRecycled(Object holder)
        {
            try
            {
                switch (holder)
                {
                    case ArticlesAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.Image);
                        Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.UserImageProfile);
                        break;
                }

                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public ArticleDataObject GetItem(int position)
        {
            return ArticlesList[position];
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

        private void UserClick(ArticlesAdapterClickEventArgs args)
        {
            UserItemClick?.Invoke(this, args);
        }

        private void Click(ArticlesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(ArticlesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = ArticlesList[p0];

                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                if (item.Thumbnail != "")
                {
                    d.Add(item.Thumbnail);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }
    }

    public class ArticlesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ArticlesAdapterViewHolder(View itemView, Action<ArticlesAdapterClickEventArgs> userClickListener, Action<ArticlesAdapterClickEventArgs> clickListener, Action<ArticlesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                UserItem = MainView.FindViewById<RelativeLayout>(Resource.Id.UserItem_Layout);

                Image = MainView.FindViewById<RoundedImageView>(Resource.Id.Image);
                Category = MainView.FindViewById<TextView>(Resource.Id.Category);
                Title = MainView.FindViewById<TextView>(Resource.Id.Title);
                Description = MainView.FindViewById<TextView>(Resource.Id.description);
                UserImageProfile = MainView.FindViewById<ImageView>(Resource.Id.UserImageProfile);
                Username = MainView.FindViewById<TextView>(Resource.Id.Username);
                Time = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                ViewMore = MainView.FindViewById<TextView>(Resource.Id.View_more);

                //Event
                UserItem.Click += (sender, e) => userClickListener(new ArticlesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.Click += (sender, e) => clickListener(new ArticlesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ArticlesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        private View MainView { get; }

        public RoundedImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Description { get; private set; }
        public ImageView UserImageProfile { get; private set; }
        public TextView Category { get; private set; }
        public TextView Username { get; private set; }
        public TextView Time { get; private set; }
        public TextView ViewMore { get; private set; }
        public RelativeLayout UserItem { get; private set; }

        #endregion
    }

    public class ArticlesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}