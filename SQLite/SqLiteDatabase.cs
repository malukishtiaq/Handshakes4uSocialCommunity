using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Android.Content;
using Java.Lang;
using Newtonsoft.Json;
using SQLite;
using WoWonder.Activities.Chat.ChatHead;
using WoWonder.Activities.Chat.ChatWindow;
using WoWonder.Activities.Chat.MsgTabbes;
using WoWonder.Helpers.Chat;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SocketSystem;
using WoWonder.StickersView;
using WoWonderClient;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.Story;
using Exception = System.Exception;

namespace WoWonder.SQLite
{
    public class SqLiteDatabase
    {
        //############# DON'T MODIFY HERE #############
        private static readonly string Folder = AppDomain.CurrentDomain.BaseDirectory; // Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public static readonly string PathCombine = Path.Combine(Folder, AppSettings.DatabaseName + "24_.db");
        private static readonly List<string> IdMesgList = new List<string>();

        //Open Connection in Database
        //*********************************************************

        #region Connection

        private SQLiteConnection OpenConnection()
        {
            try
            {
                var connection = new SQLiteConnection(new SQLiteConnectionString(PathCombine, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true));
                return connection;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public void CheckTablesStatus()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }

                connection.CreateTable<DataTables.LoginTb>();
                connection.CreateTable<DataTables.SettingsTb>();
                connection.CreateTable<DataTables.MyContactsTb>();
                connection.CreateTable<DataTables.MyProfileTb>();
                connection.CreateTable<DataTables.SearchFilterTb>();
                connection.CreateTable<DataTables.NearByFilterTb>();
                connection.CreateTable<DataTables.GiftsTb>();
                connection.CreateTable<DataTables.StickersTb>();

                connection.CreateTable<DataTables.CallUserTb>();
                connection.CreateTable<DataTables.MuteTb>();
                connection.CreateTable<DataTables.PinTb>();
                connection.CreateTable<DataTables.ArchiveTb>();
                connection.CreateTable<DataTables.StickersTb>();
                connection.CreateTable<DataTables.LastUsersTb>();
                connection.CreateTable<DataTables.MessageTb>();
                connection.CreateTable<DataTables.BroadcastMessageTb>();
                Insert_To_StickersTb();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    CheckTablesStatus();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Delete table 
        public void DropAll()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                connection.DropTable<DataTables.LoginTb>();
                connection.DropTable<DataTables.MyContactsTb>();
                connection.DropTable<DataTables.MyProfileTb>();
                connection.DropTable<DataTables.SearchFilterTb>();
                connection.DropTable<DataTables.NearByFilterTb>();
                connection.DropTable<DataTables.SettingsTb>();
                connection.DropTable<DataTables.GiftsTb>();
                connection.DropTable<DataTables.StickersTb>();

                connection.DropTable<DataTables.CallUserTb>();
                connection.DropTable<DataTables.MuteTb>();
                connection.DropTable<DataTables.PinTb>();
                connection.DropTable<DataTables.ArchiveTb>();
                connection.DropTable<DataTables.StickersTb>();
                connection.DropTable<DataTables.LastUsersTb>();
                connection.DropTable<DataTables.MessageTb>();
                connection.DropTable<DataTables.BroadcastMessageTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DropAll();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //########################## End SQLite_Entity ##########################

        //Start SQL_Commander >>  General 
        //*********************************************************

        #region General

        public void InsertRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.Insert(row);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.Update(row);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.Delete(row);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertListOfRows(List<object> row)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.InsertAll(row);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //Start SQL_Commander >>  Custom 
        //*********************************************************

        #region Login

        //Insert Or Update data Login
        public void InsertOrUpdateLogin_Credentials(DataTables.LoginTb db)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.UserId = UserDetails.UserId;
                    dataUser.AccessToken = UserDetails.AccessToken;
                    dataUser.Cookie = UserDetails.Cookie;
                    dataUser.Username = UserDetails.Username;
                    dataUser.Password = UserDetails.Password;
                    dataUser.Status = UserDetails.Status;
                    dataUser.Lang = AppSettings.Lang;
                    dataUser.Email = UserDetails.Email;

                    connection.Update(dataUser);
                }
                else
                {
                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateLogin_Credentials(db);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data Login
        public DataTables.LoginTb Get_data_Login_Credentials()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return null!;
                }
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    UserDetails.Username = dataUser.Username;
                    UserDetails.FullName = dataUser.Username;
                    UserDetails.Password = dataUser.Password;
                    UserDetails.AccessToken = dataUser.AccessToken;
                    UserDetails.UserId = dataUser.UserId;
                    UserDetails.Status = dataUser.Status;
                    UserDetails.Cookie = dataUser.Cookie;
                    UserDetails.Email = dataUser.Email;
                    AppSettings.Lang = dataUser.Lang;

                    Current.AccessToken = dataUser.AccessToken;
                    ListUtils.DataUserLoginList.Add(dataUser);

                    return dataUser;
                }

                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_data_Login_Credentials();
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region Settings

        public void InsertOrUpdateSettings(GetSiteSettingsObject.ConfigObject settingsData)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                if (settingsData != null)
                {
                    var select = connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                    switch (select)
                    {
                        case null:
                            {
                                var db = ClassMapper.Mapper?.Map<DataTables.SettingsTb>(settingsData);

                                if (db != null)
                                {
                                    db.CurrencyArray = JsonConvert.SerializeObject(settingsData.CurrencyArray.CurrencyList);
                                    db.CurrencySymbolArray = JsonConvert.SerializeObject(settingsData.CurrencySymbolArray.CurrencyList);
                                    db.PageCategories = JsonConvert.SerializeObject(settingsData.PageCategories);
                                    db.GroupCategories = JsonConvert.SerializeObject(settingsData.GroupCategories);
                                    db.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                                    db.ProductsCategories = JsonConvert.SerializeObject(settingsData.ProductsCategories);
                                    db.JobCategories = JsonConvert.SerializeObject(settingsData.JobCategories);
                                    db.Genders = JsonConvert.SerializeObject(settingsData.Genders);
                                    db.Family = JsonConvert.SerializeObject(settingsData.Family);
                                    db.MovieCategory = JsonConvert.SerializeObject(settingsData.MovieCategory);
                                    db.PostColors = JsonConvert.SerializeObject(settingsData.PostColors?.PostColorsList);
                                    db.Fields = JsonConvert.SerializeObject(settingsData.Fields);
                                    db.PostReactionsTypes = JsonConvert.SerializeObject(settingsData.PostReactionsTypes);
                                    db.PageSubCategories = JsonConvert.SerializeObject(settingsData.PageSubCategories?.SubCategoriesList);
                                    db.GroupSubCategories = JsonConvert.SerializeObject(settingsData.GroupSubCategories?.SubCategoriesList);
                                    db.ProductsSubCategories = JsonConvert.SerializeObject(settingsData.ProductsSubCategories?.SubCategoriesList);
                                    db.PageCustomFields = JsonConvert.SerializeObject(settingsData.PageCustomFields);
                                    db.GroupCustomFields = JsonConvert.SerializeObject(settingsData.GroupCustomFields);
                                    db.ProductCustomFields = JsonConvert.SerializeObject(settingsData.ProductCustomFields);
                                    db.ProPackages = JsonConvert.SerializeObject(settingsData.ProPackages);

                                    connection.Insert(db);
                                }

                                break;
                            }
                        default:
                            {
                                select = ClassMapper.Mapper?.Map<DataTables.SettingsTb>(settingsData);
                                if (select != null)
                                {
                                    select.CurrencyArray = JsonConvert.SerializeObject(settingsData.CurrencyArray.CurrencyList);
                                    select.CurrencySymbolArray = JsonConvert.SerializeObject(settingsData.CurrencySymbolArray.CurrencyList);
                                    select.PageCategories = JsonConvert.SerializeObject(settingsData.PageCategories);
                                    select.GroupCategories = JsonConvert.SerializeObject(settingsData.GroupCategories);
                                    select.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                                    select.ProductsCategories = JsonConvert.SerializeObject(settingsData.ProductsCategories);
                                    select.JobCategories = JsonConvert.SerializeObject(settingsData.JobCategories);
                                    select.Genders = JsonConvert.SerializeObject(settingsData.Genders);
                                    select.Family = JsonConvert.SerializeObject(settingsData.Family);
                                    select.MovieCategory = JsonConvert.SerializeObject(settingsData.MovieCategory);
                                    select.PostColors = JsonConvert.SerializeObject(settingsData.PostColors?.PostColorsList);
                                    select.Fields = JsonConvert.SerializeObject(settingsData.Fields);
                                    select.PostReactionsTypes = JsonConvert.SerializeObject(settingsData.PostReactionsTypes);
                                    select.PageSubCategories = JsonConvert.SerializeObject(settingsData.PageSubCategories?.SubCategoriesList);
                                    select.GroupSubCategories = JsonConvert.SerializeObject(settingsData.GroupSubCategories?.SubCategoriesList);
                                    select.ProductsSubCategories = JsonConvert.SerializeObject(settingsData.ProductsSubCategories?.SubCategoriesList);
                                    select.PageCustomFields = JsonConvert.SerializeObject(settingsData.PageCustomFields);
                                    select.GroupCustomFields = JsonConvert.SerializeObject(settingsData.GroupCustomFields);
                                    select.ProductCustomFields = JsonConvert.SerializeObject(settingsData.ProductCustomFields);
                                    select.ProPackages = JsonConvert.SerializeObject(settingsData.ProPackages);

                                    connection.Update(select);
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateSettings(settingsData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Settings
        public GetSiteSettingsObject.ConfigObject GetSettings()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null!;

                var select = connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                if (select != null)
                {
                    var db = ClassMapper.Mapper?.Map<GetSiteSettingsObject.ConfigObject>(select);
                    if (db != null)
                    {
                        GetSiteSettingsObject.ConfigObject asd = db;
                        asd.CurrencyArray = new GetSiteSettingsObject.CurrencyArray();
                        asd.CurrencySymbolArray = new GetSiteSettingsObject.CurrencySymbol();
                        asd.PageCategories = new Dictionary<string, string>();
                        asd.GroupCategories = new Dictionary<string, string>();
                        asd.BlogCategories = new Dictionary<string, string>();
                        asd.ProductsCategories = new Dictionary<string, string>();
                        asd.JobCategories = new Dictionary<string, string>();
                        asd.Genders = new Dictionary<string, string>();
                        asd.Family = new Dictionary<string, string>();
                        asd.MovieCategory = new Dictionary<string, string>();
                        asd.PostColors = new Dictionary<string, PostColorsObject>();
                        asd.Fields = new List<Field>();
                        asd.PostReactionsTypes = new Dictionary<string, PostReactionsType>();
                        asd.PageSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.GroupSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.ProductsSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.PageCustomFields = new List<CustomField>();
                        asd.GroupCustomFields = new List<CustomField>();
                        asd.ProductCustomFields = new List<CustomField>();

                        asd.ProPackages = new Dictionary<string, DataProPackages>();

                        if (!string.IsNullOrEmpty(select.CurrencyArray))
                            asd.CurrencyArray = new GetSiteSettingsObject.CurrencyArray
                            {
                                CurrencyList = JsonConvert.DeserializeObject<List<string>>(select.CurrencyArray)
                            };

                        if (!string.IsNullOrEmpty(select.CurrencySymbolArray))
                            asd.CurrencySymbolArray = new GetSiteSettingsObject.CurrencySymbol
                            {
                                CurrencyList = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.CurrencySymbolArray),
                            };

                        if (!string.IsNullOrEmpty(select.PageCategories))
                            asd.PageCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.PageCategories);

                        if (!string.IsNullOrEmpty(select.GroupCategories))
                            asd.GroupCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.GroupCategories);

                        if (!string.IsNullOrEmpty(select.BlogCategories))
                            asd.BlogCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.BlogCategories);

                        if (!string.IsNullOrEmpty(select.ProductsCategories))
                            asd.ProductsCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.ProductsCategories);

                        if (!string.IsNullOrEmpty(select.JobCategories))
                            asd.JobCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.JobCategories);

                        if (!string.IsNullOrEmpty(select.Genders))
                            asd.Genders = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.Genders);

                        if (!string.IsNullOrEmpty(select.Family))
                            asd.Family = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.Family);

                        if (!string.IsNullOrEmpty(select.MovieCategory))
                            asd.MovieCategory =
                                JsonConvert.DeserializeObject<Dictionary<string, string>>(select.MovieCategory);

                        if (!string.IsNullOrEmpty(select.PostColors))
                            asd.PostColors = new GetSiteSettingsObject.PostColorUnion
                            {
                                PostColorsList = JsonConvert.DeserializeObject<Dictionary<string, PostColorsObject>>(select.PostColors)
                            };

                        if (!string.IsNullOrEmpty(select.PostReactionsTypes))
                            asd.PostReactionsTypes =
                                JsonConvert.DeserializeObject<Dictionary<string, PostReactionsType>>(select.PostReactionsTypes);

                        if (!string.IsNullOrEmpty(select.Fields))
                            asd.Fields = JsonConvert.DeserializeObject<List<Field>>(select.Fields);

                        if (!string.IsNullOrEmpty(select.PageSubCategories))
                            asd.PageSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList = JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(select.PageSubCategories)
                            };

                        if (!string.IsNullOrEmpty(select.GroupSubCategories))
                            asd.GroupSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList = JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(select.GroupSubCategories)
                            };

                        if (!string.IsNullOrEmpty(select.ProductsSubCategories))
                            asd.ProductsSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList = JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(select.ProductsSubCategories)
                            };

                        if (!string.IsNullOrEmpty(select.PageCustomFields))
                            asd.PageCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(select.PageCustomFields);

                        if (!string.IsNullOrEmpty(select.GroupCustomFields))
                            asd.GroupCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(select.GroupCustomFields);

                        if (!string.IsNullOrEmpty(select.ProductCustomFields))
                            asd.ProductCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(select.ProductCustomFields);

                        if (!string.IsNullOrEmpty(select.ProPackages))
                            asd.ProPackages = JsonConvert.DeserializeObject<Dictionary<string, DataProPackages>>(select.ProPackages);

                        CategoriesController.SetListCategories(asd);

                        return asd;
                    }

                    return null!;
                }

                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSettings();
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        //remove Settings
        public void remove_Settings()
        {
            try
            {
                var connection = OpenConnection();
                connection.DeleteAll<DataTables.SettingsTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    remove_Settings();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region My Contacts >> Following

        //Insert data To My Contact Table
        public void Insert_Or_Replace_MyContactTable(ObservableCollection<UserDataObject> usersContactList)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var result = connection.Table<DataTables.MyContactsTb>().ToList();
                List<DataTables.MyContactsTb> list = new List<DataTables.MyContactsTb>();

                connection.BeginTransaction();

                foreach (var info in usersContactList)
                {
                    var db = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                    if (info.Details.DetailsClass != null && db != null)
                    {
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);
                        list.Add(db);
                    }

                    var update = result.FirstOrDefault(a => a.UserId == info.UserId);
                    if (update != null)
                    {
                        update = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                        if (info.Details.DetailsClass != null && update != null)
                        {
                            update.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                            update.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);
                            connection.Update(update);
                        }
                    }
                }

                switch (list.Count)
                {
                    case <= 0:
                        return;
                }


                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                switch (newItemList.Count)
                {
                    case > 0:
                        connection.InsertAll(newItemList);
                        break;
                }

                result = connection.Table<DataTables.MyContactsTb>().ToList();
                var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                switch (deleteItemList.Count)
                {
                    case > 0:
                        {
                            foreach (var delete in deleteItemList)
                                connection.Delete(delete);
                            break;
                        }
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_MyContactTable(usersContactList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To My Contact Table
        public ObservableCollection<UserDataObject> Get_MyContact(/*int id = 0, int nSize = 20*/)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return new ObservableCollection<UserDataObject>();
                }
                // var query = Connection.Table<DataTables.MyContactsTb>().Where(w => w.AutoIdMyFollowing >= id).OrderBy(q => q.AutoIdMyFollowing).Take(nSize).ToList();

                var select = connection.Table<DataTables.MyContactsTb>().ToList();
                switch (select.Count)
                {
                    case > 0:
                        {
                            var list = new ObservableCollection<UserDataObject>();

                            foreach (var item in select)
                            {
                                UserDataObject infoObject = new UserDataObject
                                {
                                    UserId = item.UserId,
                                    Username = item.Username,
                                    Email = item.Email,
                                    FirstName = item.FirstName,
                                    LastName = item.LastName,
                                    Avatar = item.Avatar,
                                    Cover = item.Cover,
                                    BackgroundImage = item.BackgroundImage,
                                    RelationshipId = item.RelationshipId,
                                    Address = item.Address,
                                    Working = item.Working,
                                    Gender = item.Gender,
                                    Facebook = item.Facebook,
                                    Google = item.Google,
                                    Twitter = item.Twitter,
                                    Linkedin = item.Linkedin,
                                    Website = item.Website,
                                    Instagram = item.Instagram,
                                    WebDeviceId = item.WebDeviceId,
                                    Language = item.Language,
                                    IpAddress = item.IpAddress,
                                    PhoneNumber = item.PhoneNumber,
                                    Timezone = item.Timezone,
                                    Lat = item.Lat,
                                    Lng = item.Lng,
                                    About = item.About,
                                    Birthday = item.Birthday,
                                    Registered = item.Registered,
                                    Lastseen = item.Lastseen,
                                    LastLocationUpdate = item.LastLocationUpdate,
                                    Balance = item.Balance,
                                    Verified = item.Verified,
                                    Status = item.Status,
                                    Active = item.Active,
                                    Admin = item.Admin,
                                    IsPro = item.IsPro,
                                    ProType = item.ProType,
                                    School = item.School,
                                    Name = item.Name,
                                    AndroidMDeviceId = item.AndroidMDeviceId,
                                    ECommented = item.ECommented,
                                    AndroidNDeviceId = item.AndroidMDeviceId,
                                    AvatarFull = item.AvatarFull,
                                    BirthPrivacy = item.BirthPrivacy,
                                    CanFollow = item.CanFollow,
                                    ConfirmFollowers = item.ConfirmFollowers,
                                    CountryId = item.CountryId,
                                    EAccepted = item.EAccepted,
                                    EFollowed = item.EFollowed,
                                    EJoinedGroup = item.EJoinedGroup,
                                    ELastNotif = item.ELastNotif,
                                    ELiked = item.ELiked,
                                    ELikedPage = item.ELikedPage,
                                    EMentioned = item.EMentioned,
                                    EProfileWallPost = item.EProfileWallPost,
                                    ESentmeMsg = item.ESentmeMsg,
                                    EShared = item.EShared,
                                    EVisited = item.EVisited,
                                    EWondered = item.EWondered,
                                    EmailNotification = item.EmailNotification,
                                    FollowPrivacy = item.FollowPrivacy,
                                    FriendPrivacy = item.FriendPrivacy,
                                    GenderText = item.GenderText,
                                    InfoFile = item.InfoFile,
                                    IosMDeviceId = item.IosMDeviceId,
                                    IosNDeviceId = item.IosNDeviceId,
                                    IsBlocked = item.IsBlocked,
                                    IsFollowing = item.IsFollowing,
                                    IsFollowingMe = item.IsFollowingMe,
                                    LastAvatarMod = item.LastAvatarMod,
                                    LastCoverMod = item.LastCoverMod,
                                    LastDataUpdate = item.LastDataUpdate,
                                    LastFollowId = item.LastFollowId,
                                    LastLoginData = item.LastLoginData,
                                    LastseenStatus = item.LastseenStatus,
                                    LastseenTimeText = item.LastseenTimeText,
                                    LastseenUnixTime = item.LastseenUnixTime,
                                    MessagePrivacy = item.MessagePrivacy,
                                    NewEmail = item.NewEmail,
                                    NewPhone = item.NewPhone,
                                    NotificationsSound = item.NotificationsSound,
                                    OrderPostsBy = item.OrderPostsBy,
                                    PaypalEmail = item.PaypalEmail,
                                    PostPrivacy = item.PostPrivacy,
                                    Referrer = item.Referrer,
                                    ShareMyData = item.ShareMyData,
                                    ShareMyLocation = item.ShareMyLocation,
                                    ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                                    TwoFactor = item.TwoFactor,
                                    TwoFactorVerified = item.TwoFactorVerified,
                                    Url = item.Url,
                                    VisitPrivacy = item.VisitPrivacy,
                                    Vk = item.Vk,
                                    Wallet = item.Wallet,
                                    WorkingLink = item.WorkingLink,
                                    Youtube = item.Youtube,
                                    City = item.City,
                                    State = item.State,
                                    Zip = item.Zip,
                                    Points = item.Points,
                                    DailyPoints = item.DailyPoints,
                                    PointDayExpire = item.PointDayExpire,
                                    CashfreeSignature = item.CashfreeSignature,
                                    IsAdmin = item.IsAdmin,
                                    MemberId = item.MemberId,
                                    ChatColor = item.ChatColor,
                                    PaystackRef = item.PaystackRef,
                                    RefUserId = item.RefUserId,
                                    SchoolCompleted = item.SchoolCompleted,
                                    Type = item.Type,
                                    UserPlatform = item.UserPlatform,
                                    WeatherUnit = item.WeatherUnit,
                                    AvatarPostId = item.AvatarPostId,
                                    CodeSent = item.CodeSent,
                                    CoverPostId = item.CoverPostId,
                                    Discord = item.Discord,
                                    IsArchive = item.IsArchive,
                                    IsMute = item.IsMute,
                                    IsPin = item.IsPin,
                                    IsReported = item.IsReported,
                                    IsStoryMuted = item.IsStoryMuted,
                                    Mailru = item.Mailru,
                                    NotificationSettings = item.NotificationSettings,
                                    IsNotifyStopped = item.IsNotifyStopped,
                                    Qq = item.Qq,
                                    StripeSessionId = item.StripeSessionId,
                                    Time = item.Time,
                                    TimeCodeSent = item.TimeCodeSent,
                                    Banned = item.Banned,
                                    BannedReason = item.BannedReason,
                                    CoinbaseCode = item.CoinbaseCode,
                                    CoinbaseHash = item.CoinbaseHash,
                                    CurrentlyWorking = item.CurrentlyWorking,
                                    IsOpenToWork = item.IsOpenToWork,
                                    IsProvidingService = item.IsProvidingService,
                                    Languages = item.Languages,
                                    Permission = item.Permission,
                                    ProvidingService = item.ProvidingService,
                                    Skills = item.Skills,
                                    Wechat = item.Wechat,
                                    Details = new DetailsUnion(),
                                    Selected = false,
                                    ApiNotificationSettings = new NotificationSettingsUnion(),
                                };

                                infoObject.Details = string.IsNullOrEmpty(item.Details) switch
                                {
                                    false => new DetailsUnion
                                    {
                                        DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details)
                                    },
                                    _ => infoObject.Details
                                };

                                infoObject.ApiNotificationSettings = string.IsNullOrEmpty(item.ApiNotificationSettings) switch
                                {
                                    false => new NotificationSettingsUnion
                                    {
                                        NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings)
                                    },
                                    _ => infoObject.ApiNotificationSettings
                                };

                                list.Add(infoObject);
                            }

                            return list;
                        }
                    default:
                        return new ObservableCollection<UserDataObject>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_MyContact();
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<UserDataObject>();
            }
        }

        public void Insert_Or_Replace_OR_Delete_UsersContact(UserDataObject info, string type)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var user = connection.Table<DataTables.MyContactsTb>().FirstOrDefault(c => c.UserId == info.UserId);
                if (user != null)
                {
                    switch (type)
                    {
                        case "Delete":
                            connection.Delete(user);
                            break;
                        default: // Update
                            {
                                user = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                                if (info.Details.DetailsClass != null)
                                    user.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                                if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                                    user.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                                connection.Update(user);
                                break;
                            }
                    }
                }
                else
                {
                    DataTables.MyContactsTb db = new DataTables.MyContactsTb
                    {
                        UserId = info.UserId,
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastseenTimeText = info.LastseenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsArchive = info.IsArchive,
                        IsMute = info.IsMute,
                        IsPin = info.IsPin,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        Skills = info.Skills,
                        Type = info.Type,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        Wechat = info.Wechat,
                        ApiNotificationSettings = string.Empty,
                        Details = string.Empty,
                        Selected = false,
                    };

                    if (info.Details.DetailsClass != null)
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_OR_Delete_UsersContact(info, type);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data One user To My Contact Table
        public UserDataObject Get_DataOneUser(string userName)
        {
            try
            {
                using var connection = OpenConnection();
                var item = connection?.Table<DataTables.MyContactsTb>().FirstOrDefault(a => a.Username == userName || a.Name == userName);
                if (item != null)
                {
                    UserDataObject infoObject = new UserDataObject
                    {
                        UserId = item.UserId,
                        Username = item.Username,
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Avatar = item.Avatar,
                        Cover = item.Cover,
                        BackgroundImage = item.BackgroundImage,
                        RelationshipId = item.RelationshipId,
                        Address = item.Address,
                        Working = item.Working,
                        Gender = item.Gender,
                        Facebook = item.Facebook,
                        Google = item.Google,
                        Twitter = item.Twitter,
                        Linkedin = item.Linkedin,
                        Website = item.Website,
                        Instagram = item.Instagram,
                        WebDeviceId = item.WebDeviceId,
                        Language = item.Language,
                        IpAddress = item.IpAddress,
                        PhoneNumber = item.PhoneNumber,
                        Timezone = item.Timezone,
                        Lat = item.Lat,
                        Lng = item.Lng,
                        About = item.About,
                        Birthday = item.Birthday,
                        Registered = item.Registered,
                        Lastseen = item.Lastseen,
                        LastLocationUpdate = item.LastLocationUpdate,
                        Balance = item.Balance,
                        Verified = item.Verified,
                        Status = item.Status,
                        Active = item.Active,
                        Admin = item.Admin,
                        IsPro = item.IsPro,
                        ProType = item.ProType,
                        School = item.School,
                        Name = item.Name,
                        AndroidMDeviceId = item.AndroidMDeviceId,
                        ECommented = item.ECommented,
                        AndroidNDeviceId = item.AndroidMDeviceId,
                        AvatarFull = item.AvatarFull,
                        BirthPrivacy = item.BirthPrivacy,
                        CanFollow = item.CanFollow,
                        ConfirmFollowers = item.ConfirmFollowers,
                        CountryId = item.CountryId,
                        EAccepted = item.EAccepted,
                        EFollowed = item.EFollowed,
                        EJoinedGroup = item.EJoinedGroup,
                        ELastNotif = item.ELastNotif,
                        ELiked = item.ELiked,
                        ELikedPage = item.ELikedPage,
                        EMentioned = item.EMentioned,
                        EProfileWallPost = item.EProfileWallPost,
                        ESentmeMsg = item.ESentmeMsg,
                        EShared = item.EShared,
                        EVisited = item.EVisited,
                        EWondered = item.EWondered,
                        EmailNotification = item.EmailNotification,
                        FollowPrivacy = item.FollowPrivacy,
                        FriendPrivacy = item.FriendPrivacy,
                        GenderText = item.GenderText,
                        InfoFile = item.InfoFile,
                        IosMDeviceId = item.IosMDeviceId,
                        IosNDeviceId = item.IosNDeviceId,
                        IsBlocked = item.IsBlocked,
                        IsFollowing = item.IsFollowing,
                        IsFollowingMe = item.IsFollowingMe,
                        LastAvatarMod = item.LastAvatarMod,
                        LastCoverMod = item.LastCoverMod,
                        LastDataUpdate = item.LastDataUpdate,
                        LastFollowId = item.LastFollowId,
                        LastLoginData = item.LastLoginData,
                        LastseenStatus = item.LastseenStatus,
                        LastseenTimeText = item.LastseenTimeText,
                        LastseenUnixTime = item.LastseenUnixTime,
                        MessagePrivacy = item.MessagePrivacy,
                        NewEmail = item.NewEmail,
                        NewPhone = item.NewPhone,
                        NotificationsSound = item.NotificationsSound,
                        OrderPostsBy = item.OrderPostsBy,
                        PaypalEmail = item.PaypalEmail,
                        PostPrivacy = item.PostPrivacy,
                        Referrer = item.Referrer,
                        ShareMyData = item.ShareMyData,
                        ShareMyLocation = item.ShareMyLocation,
                        ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                        TwoFactor = item.TwoFactor,
                        TwoFactorVerified = item.TwoFactorVerified,
                        Url = item.Url,
                        VisitPrivacy = item.VisitPrivacy,
                        Vk = item.Vk,
                        Wallet = item.Wallet,
                        WorkingLink = item.WorkingLink,
                        Youtube = item.Youtube,
                        City = item.City,
                        State = item.State,
                        Zip = item.Zip,
                        Points = item.Points,
                        DailyPoints = item.DailyPoints,
                        PointDayExpire = item.PointDayExpire,
                        CashfreeSignature = item.CashfreeSignature,
                        IsAdmin = item.IsAdmin,
                        MemberId = item.MemberId,
                        ChatColor = item.ChatColor,
                        PaystackRef = item.PaystackRef,
                        RefUserId = item.RefUserId,
                        SchoolCompleted = item.SchoolCompleted,
                        Type = item.Type,
                        UserPlatform = item.UserPlatform,
                        WeatherUnit = item.WeatherUnit,
                        AvatarPostId = item.AvatarPostId,
                        CodeSent = item.CodeSent,
                        CoverPostId = item.CoverPostId,
                        Discord = item.Discord,
                        IsArchive = item.IsArchive,
                        IsMute = item.IsMute,
                        IsPin = item.IsPin,
                        IsReported = item.IsReported,
                        IsStoryMuted = item.IsStoryMuted,
                        Mailru = item.Mailru,
                        NotificationSettings = item.NotificationSettings,
                        IsNotifyStopped = item.IsNotifyStopped,
                        Qq = item.Qq,
                        StripeSessionId = item.StripeSessionId,
                        Time = item.Time,
                        TimeCodeSent = item.TimeCodeSent,
                        Banned = item.Banned,
                        BannedReason = item.BannedReason,
                        CoinbaseCode = item.CoinbaseCode,
                        CoinbaseHash = item.CoinbaseHash,
                        CurrentlyWorking = item.CurrentlyWorking,
                        IsOpenToWork = item.IsOpenToWork,
                        IsProvidingService = item.IsProvidingService,
                        Languages = item.Languages,
                        Permission = item.Permission,
                        ProvidingService = item.ProvidingService,
                        Skills = item.Skills,
                        Wechat = item.Wechat,
                        Details = new DetailsUnion(),
                        Selected = false,
                        ApiNotificationSettings = new NotificationSettingsUnion(),
                    };

                    infoObject.Details = string.IsNullOrEmpty(item.Details) switch
                    {
                        false => new DetailsUnion { DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details) },
                        _ => infoObject.Details
                    };

                    infoObject.ApiNotificationSettings = string.IsNullOrEmpty(item.ApiNotificationSettings) switch
                    {
                        false => new NotificationSettingsUnion { NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings) },
                        _ => infoObject.ApiNotificationSettings
                    };

                    return infoObject;
                }
                else
                {
                    var infoObject = ListUtils.MyFollowingList.FirstOrDefault(a => a.Username == userName || a.Name == userName);
                    if (infoObject != null) return infoObject;
                }

                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_DataOneUser(userName);
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region My Profile

        //Insert Or Update data My Profile Table
        public void Insert_Or_Update_To_MyProfileTable(UserDataObject info)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var resultInfoTb = connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                if (resultInfoTb != null)
                {
                    resultInfoTb = new DataTables.MyProfileTb
                    {
                        UserId = info.UserId,
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastseenTimeText = info.LastseenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsArchive = info.IsArchive,
                        IsMute = info.IsMute,
                        IsPin = info.IsPin,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Wechat = info.Wechat,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        Skills = info.Skills,
                        Type = info.Type,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        ApiNotificationSettings = string.Empty,
                        Details = string.Empty,
                        Selected = false,
                    };

                    if (info.Details.DetailsClass != null)
                        resultInfoTb.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        resultInfoTb.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                    connection.Update(resultInfoTb);
                }
                else
                {
                    DataTables.MyProfileTb db = new DataTables.MyProfileTb
                    {
                        UserId = info.UserId,
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastseenTimeText = info.LastseenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsArchive = info.IsArchive,
                        IsMute = info.IsMute,
                        IsPin = info.IsPin,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        Skills = info.Skills,
                        Type = info.Type,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        Wechat = info.Wechat,
                        ApiNotificationSettings = string.Empty,
                        Details = string.Empty,
                        Selected = false,
                    };

                    if (info.Details.DetailsClass != null)
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                    connection.Insert(db);
                }

                UserDetails.Avatar = info.Avatar;
                UserDetails.Cover = info.Cover;
                UserDetails.Username = info.Username;
                UserDetails.FullName = info.Name;
                UserDetails.Email = info.Email;

                ListUtils.MyProfileList = new ObservableCollection<UserDataObject>();
                ListUtils.MyProfileList?.Clear();
                ListUtils.MyProfileList?.Add(info);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_To_MyProfileTable(info);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To My Profile Table
        public UserDataObject Get_MyProfile()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return null!;
                }
                var item = connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                if (item != null)
                {
                    UserDataObject infoObject = new UserDataObject
                    {
                        UserId = item.UserId,
                        Username = item.Username,
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Avatar = item.Avatar,
                        Cover = item.Cover,
                        BackgroundImage = item.BackgroundImage,
                        RelationshipId = item.RelationshipId,
                        Address = item.Address,
                        Working = item.Working,
                        Gender = item.Gender,
                        Facebook = item.Facebook,
                        Google = item.Google,
                        Twitter = item.Twitter,
                        Linkedin = item.Linkedin,
                        Website = item.Website,
                        Instagram = item.Instagram,
                        WebDeviceId = item.WebDeviceId,
                        Language = item.Language,
                        IpAddress = item.IpAddress,
                        PhoneNumber = item.PhoneNumber,
                        Timezone = item.Timezone,
                        Lat = item.Lat,
                        Lng = item.Lng,
                        About = item.About,
                        Birthday = item.Birthday,
                        Registered = item.Registered,
                        Lastseen = item.Lastseen,
                        LastLocationUpdate = item.LastLocationUpdate,
                        Balance = item.Balance,
                        Verified = item.Verified,
                        Status = item.Status,
                        Active = item.Active,
                        Admin = item.Admin,
                        IsPro = item.IsPro,
                        ProType = item.ProType,
                        School = item.School,
                        Name = item.Name,
                        AndroidMDeviceId = item.AndroidMDeviceId,
                        ECommented = item.ECommented,
                        AndroidNDeviceId = item.AndroidMDeviceId,
                        AvatarFull = item.AvatarFull,
                        BirthPrivacy = item.BirthPrivacy,
                        CanFollow = item.CanFollow,
                        ConfirmFollowers = item.ConfirmFollowers,
                        CountryId = item.CountryId,
                        EAccepted = item.EAccepted,
                        EFollowed = item.EFollowed,
                        EJoinedGroup = item.EJoinedGroup,
                        ELastNotif = item.ELastNotif,
                        ELiked = item.ELiked,
                        ELikedPage = item.ELikedPage,
                        EMentioned = item.EMentioned,
                        EProfileWallPost = item.EProfileWallPost,
                        ESentmeMsg = item.ESentmeMsg,
                        EShared = item.EShared,
                        EVisited = item.EVisited,
                        EWondered = item.EWondered,
                        EmailNotification = item.EmailNotification,
                        FollowPrivacy = item.FollowPrivacy,
                        FriendPrivacy = item.FriendPrivacy,
                        GenderText = item.GenderText,
                        InfoFile = item.InfoFile,
                        IosMDeviceId = item.IosMDeviceId,
                        IosNDeviceId = item.IosNDeviceId,
                        IsBlocked = item.IsBlocked,
                        IsFollowing = item.IsFollowing,
                        IsFollowingMe = item.IsFollowingMe,
                        LastAvatarMod = item.LastAvatarMod,
                        LastCoverMod = item.LastCoverMod,
                        LastDataUpdate = item.LastDataUpdate,
                        LastFollowId = item.LastFollowId,
                        LastLoginData = item.LastLoginData,
                        LastseenStatus = item.LastseenStatus,
                        LastseenTimeText = item.LastseenTimeText,
                        LastseenUnixTime = item.LastseenUnixTime,
                        MessagePrivacy = item.MessagePrivacy,
                        NewEmail = item.NewEmail,
                        NewPhone = item.NewPhone,
                        NotificationsSound = item.NotificationsSound,
                        OrderPostsBy = item.OrderPostsBy,
                        PaypalEmail = item.PaypalEmail,
                        PostPrivacy = item.PostPrivacy,
                        Referrer = item.Referrer,
                        ShareMyData = item.ShareMyData,
                        ShareMyLocation = item.ShareMyLocation,
                        ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                        TwoFactor = item.TwoFactor,
                        TwoFactorVerified = item.TwoFactorVerified,
                        Url = item.Url,
                        VisitPrivacy = item.VisitPrivacy,
                        Vk = item.Vk,
                        Wallet = item.Wallet,
                        WorkingLink = item.WorkingLink,
                        Youtube = item.Youtube,
                        City = item.City,
                        State = item.State,
                        Zip = item.Zip,
                        Points = item.Points,
                        DailyPoints = item.DailyPoints,
                        PointDayExpire = item.PointDayExpire,
                        CashfreeSignature = item.CashfreeSignature,
                        IsAdmin = item.IsAdmin,
                        MemberId = item.MemberId,
                        ChatColor = item.ChatColor,
                        PaystackRef = item.PaystackRef,
                        RefUserId = item.RefUserId,
                        SchoolCompleted = item.SchoolCompleted,
                        Type = item.Type,
                        UserPlatform = item.UserPlatform,
                        WeatherUnit = item.WeatherUnit,
                        AvatarPostId = item.AvatarPostId,
                        CodeSent = item.CodeSent,
                        CoverPostId = item.CoverPostId,
                        Discord = item.Discord,
                        IsArchive = item.IsArchive,
                        IsMute = item.IsMute,
                        IsPin = item.IsPin,
                        IsReported = item.IsReported,
                        IsStoryMuted = item.IsStoryMuted,
                        Mailru = item.Mailru,
                        NotificationSettings = item.NotificationSettings,
                        IsNotifyStopped = item.IsNotifyStopped,
                        Qq = item.Qq,
                        StripeSessionId = item.StripeSessionId,
                        Time = item.Time,
                        TimeCodeSent = item.TimeCodeSent,
                        Banned = item.Banned,
                        BannedReason = item.BannedReason,
                        CoinbaseCode = item.CoinbaseCode,
                        CoinbaseHash = item.CoinbaseHash,
                        CurrentlyWorking = item.CurrentlyWorking,
                        IsOpenToWork = item.IsOpenToWork,
                        IsProvidingService = item.IsProvidingService,
                        Languages = item.Languages,
                        Permission = item.Permission,
                        ProvidingService = item.ProvidingService,
                        Skills = item.Skills,
                        Wechat = item.Wechat,
                        Details = new DetailsUnion(),
                        Selected = false,
                        ApiNotificationSettings = new NotificationSettingsUnion(),
                    };

                    infoObject.Details = string.IsNullOrEmpty(item.Details) switch
                    {
                        false => new DetailsUnion { DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details) },
                        _ => infoObject.Details
                    };

                    infoObject.ApiNotificationSettings = string.IsNullOrEmpty(item.ApiNotificationSettings) switch
                    {
                        false => new NotificationSettingsUnion { NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings) },
                        _ => infoObject.ApiNotificationSettings
                    };

                    UserDetails.Avatar = item.Avatar;
                    UserDetails.Cover = item.Cover;
                    UserDetails.Username = item.Username;
                    UserDetails.FullName = item.Name;
                    UserDetails.Email = item.Email;

                    ListUtils.MyProfileList = new ObservableCollection<UserDataObject> { infoObject };

                    return infoObject;
                }

                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_MyProfile();
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region Search Filter 

        public void InsertOrUpdate_SearchFilter(DataTables.SearchFilterTb dataFilter)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var data = connection.Table<DataTables.SearchFilterTb>().FirstOrDefault();
                switch (data)
                {
                    case null:
                        connection.Insert(dataFilter);
                        break;
                    default:
                        data.Gender = dataFilter.Gender;
                        data.Country = dataFilter.Country;
                        data.Status = dataFilter.Status;
                        data.Verified = dataFilter.Verified;
                        data.ProfilePicture = dataFilter.ProfilePicture;
                        data.FilterByAge = dataFilter.FilterByAge;
                        data.AgeFrom = dataFilter.AgeFrom;
                        data.AgeTo = dataFilter.AgeTo;

                        connection.Update(data);
                        break;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdate_SearchFilter(dataFilter);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public DataTables.SearchFilterTb GetSearchFilterById()
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection?.Table<DataTables.SearchFilterTb>().FirstOrDefault();
                return data;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSearchFilterById();
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region Near By Filter 

        public void InsertOrUpdate_NearByFilter(DataTables.NearByFilterTb dataFilter)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var data = connection.Table<DataTables.NearByFilterTb>().FirstOrDefault();
                switch (data)
                {
                    case null:
                        connection.Insert(dataFilter);
                        break;
                    default:
                        data.DistanceValue = dataFilter.DistanceValue;
                        data.Gender = dataFilter.Gender;
                        data.Status = dataFilter.Status;
                        data.Relationship = dataFilter.Relationship;

                        connection.Update(data);
                        break;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdate_NearByFilter(dataFilter);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public DataTables.NearByFilterTb GetNearByFilterById()
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection?.Table<DataTables.NearByFilterTb>().FirstOrDefault();
                return data;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetNearByFilterById();
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region Gifts

        //Insert data Gifts
        public void InsertAllGifts(ObservableCollection<GiftObject.DataGiftObject> listData)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var result = connection.Table<DataTables.GiftsTb>().ToList();

                List<DataTables.GiftsTb> list = new List<DataTables.GiftsTb>();
                foreach (var info in listData)
                {
                    var gift = new DataTables.GiftsTb
                    {
                        Id = info.Id,
                        MediaFile = info.MediaFile,
                        Name = info.Name,
                        Time = info.Time,
                        TimeText = info.TimeText,
                    };

                    list.Add(gift);

                    var update = result.FirstOrDefault(a => a.Id == info.Id);
                    if (update != null)
                    {
                        update = ClassMapper.Mapper?.Map<DataTables.GiftsTb>(info);
                        connection.Update(update);
                    }
                }

                switch (list.Count)
                {
                    case <= 0:
                        return;
                }
                connection.BeginTransaction();

                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                switch (newItemList.Count)
                {
                    case > 0:
                        connection.InsertAll(newItemList);
                        break;
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertAllGifts(listData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get List Gifts 
        public ObservableCollection<GiftObject.DataGiftObject> GetGiftsList()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return new ObservableCollection<GiftObject.DataGiftObject>();
                }
                var result = connection.Table<DataTables.GiftsTb>().ToList();
                switch (result?.Count)
                {
                    case > 0:
                        {
                            List<GiftObject.DataGiftObject> list = result.Select(gift => new GiftObject.DataGiftObject
                            {
                                Id = gift.Id,
                                MediaFile = gift.MediaFile,
                                Name = gift.Name,
                                Time = gift.Time,
                                TimeText = gift.TimeText,
                            }).ToList();

                            return new ObservableCollection<GiftObject.DataGiftObject>(list);
                        }
                    default:
                        return new ObservableCollection<GiftObject.DataGiftObject>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetGiftsList();
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<GiftObject.DataGiftObject>();
            }
        }

        #endregion

        #region Stickers

        //Insert data To Stickers Table
        public void Insert_To_StickersTb()
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.StickersTb>().ToList()?.Count;
                if (data == 0)
                {
                    var stickersList = new ObservableCollection<DataTables.StickersTb>();
                    DataTables.StickersTb s1 = new DataTables.StickersTb
                    {
                        PackageId = "1",
                        Name = "Rappit",
                        Visibility = true,
                        Count = StickersModel.Locally.StickerList1.Count.ToString()
                    };
                    stickersList.Add(s1);

                    DataTables.StickersTb s2 = new DataTables.StickersTb
                    {
                        PackageId = "2",
                        Name = "Water Drop",
                        Visibility = true,
                        Count = StickersModel.Locally.StickerList2.Count.ToString()
                    };
                    stickersList.Add(s2);

                    DataTables.StickersTb s3 = new DataTables.StickersTb
                    {
                        PackageId = "3",
                        Name = "Monster",
                        Visibility = true,
                        Count = StickersModel.Locally.StickerList3.Count.ToString()
                    };
                    stickersList.Add(s3);

                    DataTables.StickersTb s4 = new DataTables.StickersTb
                    {
                        PackageId = "4",
                        Name = "NINJA Nyankko",
                        Visibility = true,
                        Count = StickersModel.Locally.StickerList4.Count.ToString()
                    };
                    stickersList.Add(s4);

                    DataTables.StickersTb s5 = new DataTables.StickersTb
                    {
                        PackageId = "5",
                        Name = "So Much Love",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList5.Count.ToString()
                    };
                    stickersList.Add(s5);

                    DataTables.StickersTb s6 = new DataTables.StickersTb
                    {
                        PackageId = "6",
                        Name = "Sukkara chan",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList6.Count.ToString()
                    };
                    stickersList.Add(s6);

                    DataTables.StickersTb s7 = new DataTables.StickersTb
                    {
                        PackageId = "7",
                        Name = "Flower Hijab",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList7.Count.ToString()
                    };
                    stickersList.Add(s7);

                    DataTables.StickersTb s8 = new DataTables.StickersTb
                    {
                        PackageId = "8",
                        Name = "Trendy boy",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList8.Count.ToString()
                    };
                    stickersList.Add(s8);

                    DataTables.StickersTb s9 = new DataTables.StickersTb
                    {
                        PackageId = "9",
                        Name = "The stickman",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList9.Count.ToString()
                    };
                    stickersList.Add(s9);

                    DataTables.StickersTb s10 = new DataTables.StickersTb
                    {
                        PackageId = "10",
                        Name = "Chip Dale Animated",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList10.Count.ToString()
                    };
                    stickersList.Add(s10);

                    DataTables.StickersTb s11 = new DataTables.StickersTb
                    {
                        PackageId = "11",
                        Name = AppSettings.ApplicationName + " Stickers",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList11.Count.ToString()
                    };
                    stickersList.Add(s11);

                    connection.InsertAll(stickersList);

                    ListUtils.StickersList = new ObservableCollection<DataTables.StickersTb>(stickersList);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("database is locked"))
                    Insert_To_StickersTb();
                else
                    Methods.DisplayReportResultTrack(ex);
            }
        }

        //Get  data To Stickers Table
        public ObservableCollection<DataTables.StickersTb> Get_From_StickersTb()
        {
            try
            {
                using var connection = OpenConnection();
                var stickersList = new ObservableCollection<DataTables.StickersTb>();
                var data = connection.Table<DataTables.StickersTb>().ToList();

                foreach (var s in data.Select(item => new DataTables.StickersTb
                {
                    PackageId = item.PackageId,
                    Name = item.Name,
                    Visibility = item.Visibility,
                    Count = item.Count
                }))
                {
                    stickersList.Add(s);
                }

                return stickersList;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_From_StickersTb();
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        //Update data To Stickers Table
        public void Update_To_StickersTable(string typeName, bool visibility)
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.StickersTb>().FirstOrDefault(a => a.Name == typeName);
                if (data != null)
                {
                    data.Visibility = visibility;
                }
                connection.Update(data);

                var data2 = ListUtils.StickersList.FirstOrDefault(a => a.Name == typeName);
                if (data2 != null)
                {
                    data2.Visibility = visibility;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("database is locked"))
                    Update_To_StickersTable(typeName, visibility);
                else
                    Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Message

        //Insert data To Message Table
        public void Insert_Or_Replace_MessagesTable(ObservableCollection<AdapterModelsClassMessage> messageList)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                List<DataTables.MessageTb> listOfDatabaseForInsert = new List<DataTables.MessageTb>();

                // get data from database
                var resultMessage = connection.Table<DataTables.MessageTb>().ToList();

                foreach (var messages in messageList)
                {
                    var maTb = ClassMapper.Mapper?.Map<DataTables.MessageTb>(messages.MesData);
                    maTb.SendFile = false;

                    maTb.Product = JsonConvert.SerializeObject(messages.MesData.Product?.ProductClass);

                    maTb.MessageUser = JsonConvert.SerializeObject(messages.MesData.MessageUser?.UserDataClass);

                    if (messages.MesData.UserData != null)
                        maTb.UserData = JsonConvert.SerializeObject(messages.MesData.UserData);

                    maTb.ToData = JsonConvert.SerializeObject(messages.MesData.ToData);
                    maTb.Reaction = JsonConvert.SerializeObject(messages.MesData.Reaction);
                    maTb.Reply = JsonConvert.SerializeObject(messages.MesData.Reply?.ReplyClass);
                    maTb.Story = JsonConvert.SerializeObject(messages.MesData.Story?.StoryClass);

                    var dataCheck = resultMessage.FirstOrDefault(a => a.Id == messages.MesData.Id);
                    if (dataCheck != null)
                    {
                        var checkForUpdate = resultMessage.FirstOrDefault(a => a.Id == dataCheck.Id);
                        if (checkForUpdate != null)
                        {
                            checkForUpdate = ClassMapper.Mapper?.Map<DataTables.MessageTb>(messages.MesData);
                            checkForUpdate.SendFile = false;
                            checkForUpdate.ChatColor = messages.MesData.ChatColor;

                            checkForUpdate.Product = JsonConvert.SerializeObject(messages.MesData.Product?.ProductClass);

                            checkForUpdate.MessageUser = JsonConvert.SerializeObject(messages.MesData.MessageUser?.UserDataClass);
                            checkForUpdate.UserData = JsonConvert.SerializeObject(messages.MesData.UserData);
                            checkForUpdate.ToData = JsonConvert.SerializeObject(messages.MesData.ToData);
                            checkForUpdate.Reaction = JsonConvert.SerializeObject(messages.MesData.Reaction);
                            checkForUpdate.Reply = JsonConvert.SerializeObject(messages.MesData.Reply?.ReplyClass);
                            checkForUpdate.Story = JsonConvert.SerializeObject(messages.MesData.Story?.StoryClass);

                            connection.Update(checkForUpdate);

                            var cec = ChatWindowActivity.GetInstance()?.StartedMessageList?.FirstOrDefault(a => a.Id == Long.ParseLong(dataCheck.Id));
                            if (cec != null)
                            {
                                cec.MesData = messages.MesData;
                                cec.MesData.Fav = "yes";
                                cec.TypeView = messages.TypeView;
                            }
                        }
                        else
                        {
                            listOfDatabaseForInsert.Add(maTb);
                        }
                    }
                    else
                    {
                        listOfDatabaseForInsert.Add(maTb);
                    }
                }

                connection.BeginTransaction();

                //Bring new  
                if (listOfDatabaseForInsert.Count > 0)
                {
                    connection.InsertAll(listOfDatabaseForInsert);
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_MessagesTable(messageList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Update one Messages Table
        public void Insert_Or_Update_To_one_MessagesTable(MessageDataExtra item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                var data = connection.Table<DataTables.MessageTb>().FirstOrDefault(a => a.Id == item.Id);
                if (data != null)
                {
                    data = ClassMapper.Mapper?.Map<DataTables.MessageTb>(item);
                    data.SendFile = false;

                    data.Product = JsonConvert.SerializeObject(item.Product?.ProductClass);

                    data.MessageUser = JsonConvert.SerializeObject(item.MessageUser?.UserDataClass);
                    data.UserData = JsonConvert.SerializeObject(item.UserData);
                    data.ToData = JsonConvert.SerializeObject(item.ToData);
                    data.Reaction = JsonConvert.SerializeObject(item.Reaction);
                    data.Reply = JsonConvert.SerializeObject(item.Reply?.ReplyClass);
                    data.Story = JsonConvert.SerializeObject(item.Story?.StoryClass);

                    connection.Update(data);
                }
                else
                {
                    var maTb = ClassMapper.Mapper?.Map<DataTables.MessageTb>(item);
                    maTb.SendFile = false;

                    maTb.Product = JsonConvert.SerializeObject(item.Product?.ProductClass);

                    maTb.MessageUser = JsonConvert.SerializeObject(item.MessageUser?.UserDataClass);
                    maTb.UserData = JsonConvert.SerializeObject(item.UserData);
                    maTb.ToData = JsonConvert.SerializeObject(item.ToData);
                    maTb.Reaction = JsonConvert.SerializeObject(item.Reaction);
                    maTb.Reply = JsonConvert.SerializeObject(item.Reply?.ReplyClass);
                    maTb.Story = JsonConvert.SerializeObject(item.Story?.StoryClass);

                    //Insert  one Messages Table
                    connection.Insert(maTb);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_To_one_MessagesTable(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data To Messages
        public ObservableCollection<AdapterModelsClassMessage> GetMessages_List(string fromId, string toId, string beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;
                var beforeQ = "";
                if (beforeMessageId != "0")
                {
                    beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                }

                var query2 = connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + ")) " + beforeQ);

                List<DataTables.MessageTb> query = query2.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).OrderBy(q => q.Time).TakeLast(10).ToList();
                ObservableCollection<AdapterModelsClassMessage> listMessages = new ObservableCollection<AdapterModelsClassMessage>();
                if (query.Count > 0)
                {
                    foreach (var item in from item in query let check = ChatWindowActivity.GetInstance()?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                    {
                        var maTb = ClassMapper.Mapper?.Map<MessageDataExtra>(item);
                        if (maTb != null)
                        {
                            maTb.SendFile = false;
                            maTb.ChatColor = ChatWindowActivity.MainChatColor;

                            maTb.Product = new ProductUnion();
                            maTb.MessageUser = new MessageUserUnion();
                            maTb.UserData = new UserDataObject();
                            maTb.ToData = new UserDataObject();
                            maTb.Reaction = new Reaction();
                            maTb.Reply = new ReplyUnion();
                            maTb.Story = new StoryUnion();

                            if (!string.IsNullOrEmpty(item.Product))
                                maTb.Product = new ProductUnion
                                {
                                    ProductClass = JsonConvert.DeserializeObject<ProductDataObject>(item.Product)
                                };

                            if (!string.IsNullOrEmpty(item.MessageUser))
                                maTb.MessageUser = new MessageUserUnion
                                {
                                    UserDataClass = JsonConvert.DeserializeObject<UserDataObject>(item.MessageUser)
                                };

                            if (!string.IsNullOrEmpty(item.UserData))
                                maTb.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                            if (!string.IsNullOrEmpty(item.ToData))
                                maTb.ToData = JsonConvert.DeserializeObject<UserDataObject>(item.ToData);

                            if (!string.IsNullOrEmpty(item.Reaction))
                                maTb.Reaction = JsonConvert.DeserializeObject<Reaction>(item.Reaction);

                            if (!string.IsNullOrEmpty(item.Reply))
                                maTb.Reply = new ReplyUnion
                                {
                                    ReplyClass = JsonConvert.DeserializeObject<MessageData>(item.Reply)
                                };

                            if (!string.IsNullOrEmpty(item.Story))
                                maTb.Story = new StoryUnion
                                {
                                    StoryClass = JsonConvert.DeserializeObject<UserDataStory>(item.Story)
                                };

                            var type = ChatTools.GetTypeModel(maTb);
                            if (type == MessageModelType.None)
                                continue;

                            if (beforeMessageId == "0")
                            {
                                listMessages.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = type,
                                    Id = Long.ParseLong(item.Id),
                                    MesData = maTb
                                });
                            }
                            else
                            {
                                listMessages.Insert(0, new AdapterModelsClassMessage
                                {
                                    TypeView = type,
                                    Id = Long.ParseLong(item.Id),
                                    MesData = maTb
                                });
                            }
                        }
                    }
                    return listMessages;
                }

                return listMessages;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessages_List(fromId, toId, beforeMessageId);
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<AdapterModelsClassMessage>();
            }
        }

        //Get data To where first Messages >> load more
        public ObservableCollection<AdapterModelsClassMessage> GetMessageList_loadMore(string fromId, string toId, string beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;
                var beforeQ = "";
                if (beforeMessageId != "0")
                {
                    beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                }

                var query2 = connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + ")) " + beforeQ);

                List<DataTables.MessageTb> query = query2.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).OrderBy(q => q.Time).TakeLast(10).ToList();

                ObservableCollection<AdapterModelsClassMessage> listMessages = new ObservableCollection<AdapterModelsClassMessage>();
                query.Reverse();

                if (query.Count > 0)
                {
                    foreach (var item in from item in query let check = ChatWindowActivity.GetInstance()?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                    {
                        var maTb = ClassMapper.Mapper?.Map<MessageDataExtra>(item);
                        if (maTb != null)
                        {
                            maTb.SendFile = false;
                            maTb.ChatColor = ChatWindowActivity.MainChatColor;

                            maTb.Product = new ProductUnion();
                            maTb.MessageUser = new MessageUserUnion();
                            maTb.UserData = new UserDataObject();
                            maTb.ToData = new UserDataObject();
                            maTb.Reaction = new Reaction();
                            maTb.Reply = new ReplyUnion();
                            maTb.Story = new StoryUnion();

                            if (!string.IsNullOrEmpty(item.Product))
                                maTb.Product = new ProductUnion
                                {
                                    ProductClass = JsonConvert.DeserializeObject<ProductDataObject>(item.Product)
                                };

                            if (!string.IsNullOrEmpty(item.MessageUser))
                                maTb.MessageUser = new MessageUserUnion
                                {
                                    UserDataClass = JsonConvert.DeserializeObject<UserDataObject>(item.MessageUser)
                                };

                            if (!string.IsNullOrEmpty(item.UserData))
                                maTb.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                            if (!string.IsNullOrEmpty(item.ToData))
                                maTb.ToData = JsonConvert.DeserializeObject<UserDataObject>(item.ToData);

                            if (!string.IsNullOrEmpty(item.Reaction))
                                maTb.Reaction = JsonConvert.DeserializeObject<Reaction>(item.Reaction);

                            if (!string.IsNullOrEmpty(item.Reply))
                                maTb.Reply = new ReplyUnion
                                {
                                    ReplyClass = JsonConvert.DeserializeObject<MessageData>(item.Reply)
                                };

                            if (!string.IsNullOrEmpty(item.Story))
                                maTb.Story = new StoryUnion
                                {
                                    StoryClass = JsonConvert.DeserializeObject<UserDataStory>(item.Story)
                                };

                            var type = ChatTools.GetTypeModel(maTb);
                            if (type == MessageModelType.None)
                                continue;

                            listMessages.Add(new AdapterModelsClassMessage
                            {
                                TypeView = type,
                                Id = Long.ParseLong(item.Id),
                                MesData = maTb
                            });
                        }
                    }

                    return listMessages;
                }

                return listMessages;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessageList_loadMore(fromId, toId, beforeMessageId);
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<AdapterModelsClassMessage>();
            }
        }

        //Remove data To Messages Table
        public void Delete_OneMessageUser(string messageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var user = connection.Table<DataTables.MessageTb>().FirstOrDefault(c => c.Id == messageId);
                if (user != null)
                {
                    connection.Delete(user);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_OneMessageUser(messageId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteAllMessagesUser(string fromId, string toId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var query = connection.Query<DataTables.MessageTb>("Delete FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + "))");
                Console.WriteLine(query);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllMessagesUser(fromId, toId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void ClearAll_Messages()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.MessageTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_Messages();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Broadcast Message 

        //Insert data To Broadcast Message  Table
        public void Insert_Or_Replace_BroadcastMessagesTable(ObservableCollection<AdapterModelsClassMessage> messageList)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                List<DataTables.BroadcastMessageTb> listOfDatabaseForInsert = new List<DataTables.BroadcastMessageTb>();

                // get data from database
                var resultMessage = connection.Table<DataTables.BroadcastMessageTb>().ToList();

                foreach (var messages in messageList)
                {
                    var maTb = ClassMapper.Mapper?.Map<DataTables.BroadcastMessageTb>(messages.MesData);
                    maTb.SendFile = false;

                    maTb.Product = JsonConvert.SerializeObject(messages.MesData.Product?.ProductClass);

                    maTb.MessageUser = JsonConvert.SerializeObject(messages.MesData.MessageUser?.UserDataClass);

                    if (messages.MesData.UserData != null)
                        maTb.UserData = JsonConvert.SerializeObject(messages.MesData.UserData);

                    maTb.ToData = JsonConvert.SerializeObject(messages.MesData.ToData);
                    maTb.Reaction = JsonConvert.SerializeObject(messages.MesData.Reaction);
                    maTb.Reply = JsonConvert.SerializeObject(messages.MesData.Reply?.ReplyClass);
                    maTb.Story = JsonConvert.SerializeObject(messages.MesData.Story?.StoryClass);

                    var dataCheck = resultMessage.FirstOrDefault(a => a.Id == messages.MesData.Id);
                    if (dataCheck != null)
                    {
                        var checkForUpdate = resultMessage.FirstOrDefault(a => a.Id == dataCheck.Id);
                        if (checkForUpdate != null)
                        {
                            checkForUpdate = ClassMapper.Mapper?.Map<DataTables.BroadcastMessageTb>(messages.MesData);
                            checkForUpdate.SendFile = false;
                            checkForUpdate.ChatColor = messages.MesData.ChatColor;

                            checkForUpdate.Product = JsonConvert.SerializeObject(messages.MesData.Product?.ProductClass);

                            checkForUpdate.MessageUser = JsonConvert.SerializeObject(messages.MesData.MessageUser?.UserDataClass);
                            checkForUpdate.UserData = JsonConvert.SerializeObject(messages.MesData.UserData);
                            checkForUpdate.ToData = JsonConvert.SerializeObject(messages.MesData.ToData);
                            checkForUpdate.Reaction = JsonConvert.SerializeObject(messages.MesData.Reaction);
                            checkForUpdate.Reply = JsonConvert.SerializeObject(messages.MesData.Reply?.ReplyClass);
                            checkForUpdate.Story = JsonConvert.SerializeObject(messages.MesData.Story?.StoryClass);

                            connection.Update(checkForUpdate);

                            var cec = ChatWindowActivity.GetInstance()?.StartedMessageList?.FirstOrDefault(a => a.Id == Long.ParseLong(dataCheck.Id));
                            if (cec != null)
                            {
                                cec.MesData = messages.MesData;
                                cec.MesData.Fav = "yes";
                                cec.TypeView = messages.TypeView;
                            }
                        }
                        else
                        {
                            listOfDatabaseForInsert.Add(maTb);
                        }
                    }
                    else
                    {
                        listOfDatabaseForInsert.Add(maTb);
                    }
                }

                connection.BeginTransaction();

                //Bring new  
                if (listOfDatabaseForInsert.Count > 0)
                {
                    connection.InsertAll(listOfDatabaseForInsert);
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_BroadcastMessagesTable(messageList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Update one Broadcast Messages Table
        public void Insert_Or_Update_To_one_BroadcastMessagesTable(MessageDataExtra item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                var data = connection.Table<DataTables.BroadcastMessageTb>().FirstOrDefault(a => a.Id == item.Id);
                if (data != null)
                {
                    data = ClassMapper.Mapper?.Map<DataTables.BroadcastMessageTb>(item);
                    data.SendFile = false;

                    data.Product = JsonConvert.SerializeObject(item.Product?.ProductClass);

                    data.MessageUser = JsonConvert.SerializeObject(item.MessageUser?.UserDataClass);
                    data.UserData = JsonConvert.SerializeObject(item.UserData);
                    data.ToData = JsonConvert.SerializeObject(item.ToData);
                    data.Reaction = JsonConvert.SerializeObject(item.Reaction);
                    data.Reply = JsonConvert.SerializeObject(item.Reply?.ReplyClass);
                    data.Story = JsonConvert.SerializeObject(item.Story?.StoryClass);

                    connection.Update(data);
                }
                else
                {
                    var maTb = ClassMapper.Mapper?.Map<DataTables.BroadcastMessageTb>(item);
                    maTb.SendFile = false;

                    maTb.Product = JsonConvert.SerializeObject(item.Product?.ProductClass);

                    maTb.MessageUser = JsonConvert.SerializeObject(item.MessageUser?.UserDataClass);
                    maTb.UserData = JsonConvert.SerializeObject(item.UserData);
                    maTb.ToData = JsonConvert.SerializeObject(item.ToData);
                    maTb.Reaction = JsonConvert.SerializeObject(item.Reaction);
                    maTb.Reply = JsonConvert.SerializeObject(item.Reply?.ReplyClass);
                    maTb.Story = JsonConvert.SerializeObject(item.Story?.StoryClass);

                    //Insert  one Messages Table
                    connection.Insert(maTb);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_To_one_BroadcastMessagesTable(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data To Messages
        public ObservableCollection<AdapterModelsClassMessage> GetBroadcastMessages_List(string fromId, string broadcastId, string beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;

                var query = connection.Table<DataTables.BroadcastMessageTb>().Where(w => w.BroadcastId == broadcastId).OrderBy(q => q.Time).ToList();

                ObservableCollection<AdapterModelsClassMessage> listMessages = new ObservableCollection<AdapterModelsClassMessage>();
                if (query.Count > 0)
                {
                    foreach (var item in from item in query let check = ChatWindowActivity.GetInstance()?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                    {
                        var maTb = ClassMapper.Mapper?.Map<MessageDataExtra>(item);
                        if (maTb != null)
                        {
                            maTb.SendFile = false;
                            maTb.ChatColor = ChatWindowActivity.MainChatColor;

                            maTb.Product = new ProductUnion();
                            maTb.MessageUser = new MessageUserUnion();
                            maTb.UserData = new UserDataObject();
                            maTb.ToData = new UserDataObject();
                            maTb.Reaction = new Reaction();
                            maTb.Reply = new ReplyUnion();
                            maTb.Story = new StoryUnion();

                            if (!string.IsNullOrEmpty(item.Product))
                                maTb.Product = new ProductUnion
                                {
                                    ProductClass = JsonConvert.DeserializeObject<ProductDataObject>(item.Product)
                                };

                            if (!string.IsNullOrEmpty(item.MessageUser))
                                maTb.MessageUser = new MessageUserUnion
                                {
                                    UserDataClass = JsonConvert.DeserializeObject<UserDataObject>(item.MessageUser)
                                };

                            if (!string.IsNullOrEmpty(item.UserData))
                                maTb.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                            if (!string.IsNullOrEmpty(item.ToData))
                                maTb.ToData = JsonConvert.DeserializeObject<UserDataObject>(item.ToData);

                            if (!string.IsNullOrEmpty(item.Reaction))
                                maTb.Reaction = JsonConvert.DeserializeObject<Reaction>(item.Reaction);

                            if (!string.IsNullOrEmpty(item.Reply))
                                maTb.Reply = new ReplyUnion
                                {
                                    ReplyClass = JsonConvert.DeserializeObject<MessageData>(item.Reply)
                                };

                            if (!string.IsNullOrEmpty(item.Story))
                                maTb.Story = new StoryUnion
                                {
                                    StoryClass = JsonConvert.DeserializeObject<UserDataStory>(item.Story)
                                };

                            var type = ChatTools.GetTypeModel(maTb);
                            if (type == MessageModelType.None)
                                continue;

                            if (beforeMessageId == "0")
                            {
                                listMessages.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = type,
                                    Id = Long.ParseLong(item.Id),
                                    MesData = maTb
                                });
                            }
                            else
                            {
                                listMessages.Insert(0, new AdapterModelsClassMessage
                                {
                                    TypeView = type,
                                    Id = Long.ParseLong(item.Id),
                                    MesData = maTb
                                });
                            }
                        }
                    }
                    return listMessages;
                }

                return listMessages;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetBroadcastMessages_List(fromId, broadcastId, beforeMessageId);
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<AdapterModelsClassMessage>();
            }
        }

        //Get data To where first Messages >> load more
        public ObservableCollection<AdapterModelsClassMessage> GetBroadcastMessageList_loadMore(string fromId, string broadcastId, string beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;
                var beforeQ = "";
                if (beforeMessageId != "0")
                {
                    beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                }

                var query2 = connection.Query<DataTables.BroadcastMessageTb>("SELECT * FROM BroadcastMessageTb WHERE ((FromId =" + fromId + " and BroadcastId=" + broadcastId + ") OR (FromId =" + broadcastId + " and BroadcastId=" + fromId + ")) " + beforeQ);

                List<DataTables.BroadcastMessageTb> query = query2.Where(w => w.FromId == fromId && w.BroadcastId == broadcastId || w.BroadcastId == fromId && w.FromId == broadcastId).OrderBy(q => q.Time).TakeLast(10).ToList();

                ObservableCollection<AdapterModelsClassMessage> listMessages = new ObservableCollection<AdapterModelsClassMessage>();
                query.Reverse();

                if (query.Count > 0)
                {
                    foreach (var item in from item in query let check = ChatWindowActivity.GetInstance()?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                    {
                        var maTb = ClassMapper.Mapper?.Map<MessageDataExtra>(item);
                        if (maTb != null)
                        {
                            maTb.SendFile = false;
                            maTb.ChatColor = ChatWindowActivity.MainChatColor;

                            maTb.Product = new ProductUnion();
                            maTb.MessageUser = new MessageUserUnion();
                            maTb.UserData = new UserDataObject();
                            maTb.ToData = new UserDataObject();
                            maTb.Reaction = new Reaction();
                            maTb.Reply = new ReplyUnion();
                            maTb.Story = new StoryUnion();

                            if (!string.IsNullOrEmpty(item.Product))
                                maTb.Product = new ProductUnion
                                {
                                    ProductClass = JsonConvert.DeserializeObject<ProductDataObject>(item.Product)
                                };

                            if (!string.IsNullOrEmpty(item.MessageUser))
                                maTb.MessageUser = new MessageUserUnion
                                {
                                    UserDataClass = JsonConvert.DeserializeObject<UserDataObject>(item.MessageUser)
                                };

                            if (!string.IsNullOrEmpty(item.UserData))
                                maTb.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                            if (!string.IsNullOrEmpty(item.ToData))
                                maTb.ToData = JsonConvert.DeserializeObject<UserDataObject>(item.ToData);

                            if (!string.IsNullOrEmpty(item.Reaction))
                                maTb.Reaction = JsonConvert.DeserializeObject<Reaction>(item.Reaction);

                            if (!string.IsNullOrEmpty(item.Reply))
                                maTb.Reply = new ReplyUnion
                                {
                                    ReplyClass = JsonConvert.DeserializeObject<MessageData>(item.Reply)
                                };

                            if (!string.IsNullOrEmpty(item.Story))
                                maTb.Story = new StoryUnion
                                {
                                    StoryClass = JsonConvert.DeserializeObject<UserDataStory>(item.Story)
                                };

                            var type = ChatTools.GetTypeModel(maTb);
                            if (type == MessageModelType.None)
                                continue;

                            listMessages.Add(new AdapterModelsClassMessage
                            {
                                TypeView = type,
                                Id = Long.ParseLong(item.Id),
                                MesData = maTb
                            });
                        }
                    }

                    return listMessages;
                }

                return listMessages;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetBroadcastMessageList_loadMore(fromId, broadcastId, beforeMessageId);
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<AdapterModelsClassMessage>();
            }
        }

        //Remove data To Messages Table
        public void Delete_OneBroadcastMessageUser(string messageId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var user = connection.Table<DataTables.BroadcastMessageTb>().FirstOrDefault(c => c.Id == messageId);
                if (user != null)
                {
                    connection.Delete(user);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_OneBroadcastMessageUser(messageId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteAllBroadcastMessagesUser(string fromId, string broadcastId)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var query = connection.Query<DataTables.BroadcastMessageTb>("Delete FROM BroadcastMessageTb WHERE ((FromId =" + fromId + " and BroadcastId=" + broadcastId + ") OR (FromId =" + broadcastId + " and BroadcastId=" + fromId + "))");
                Console.WriteLine(query);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllBroadcastMessagesUser(fromId, broadcastId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void ClearAll_BroadcastMessages()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.BroadcastMessageTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_BroadcastMessages();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Call User

        public void Insert_CallUser(Classes.CallUser user)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var result = connection.Table<DataTables.CallUserTb>().ToList();
                var check = result.FirstOrDefault(a => a.Time == user.Time);
                if (check == null && !string.IsNullOrEmpty(user.Id))
                {
                    DataTables.CallUserTb cv = new DataTables.CallUserTb
                    {
                        CallId = user.Id,
                        UserId = user.UserId,
                        Avatar = user.Avatar,
                        Name = user.Name,
                        AccessToken = user.AccessToken,
                        AccessToken2 = user.AccessToken2,
                        FromId = user.FromId,
                        Active = user.Active,
                        Time = user.Time,
                        Status = user.Status,
                        RoomName = user.RoomName,
                        Type = user.Type.ToString(),
                        TypeIcon = user.TypeIcon,
                        TypeColor = user.TypeColor
                    };
                    connection.Insert(cv);
                }
                else
                {
                    check = new DataTables.CallUserTb
                    {
                        CallId = user.Id,
                        UserId = user.UserId,
                        Avatar = user.Avatar,
                        Name = user.Name,
                        AccessToken = user.AccessToken,
                        AccessToken2 = user.AccessToken2,
                        FromId = user.FromId,
                        Active = user.Active,
                        Time = user.Time,
                        Status = user.Status,
                        RoomName = user.RoomName,
                        Type = user.Type.ToString(),
                        TypeIcon = user.TypeIcon,
                        TypeColor = user.TypeColor
                    };

                    connection.Update(check);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_CallUser(user);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public ObservableCollection<Classes.CallUser> Get_CallUserList()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return new ObservableCollection<Classes.CallUser>();
                var list = new ObservableCollection<Classes.CallUser>();
                var result = connection.Table<DataTables.CallUserTb>().ToList();
                if (result.Count <= 0) return new ObservableCollection<Classes.CallUser>();
                foreach (var cv in result.Select(item => new Classes.CallUser
                {
                    Id = item.CallId,
                    UserId = item.UserId,
                    Avatar = item.Avatar,
                    Name = item.Name,
                    AccessToken = item.AccessToken,
                    AccessToken2 = item.AccessToken2,
                    FromId = item.FromId,
                    Active = item.Active,
                    Time = item.Time,
                    Status = item.Status,
                    RoomName = item.RoomName,
                    Type = item.Type == "Video" ? TypeCall.Video : TypeCall.Audio,
                    TypeIcon = item.TypeIcon,
                    TypeColor = item.TypeColor
                }))
                {
                    list.Add(cv);
                }

                return new ObservableCollection<Classes.CallUser>(list.OrderBy(a => a.Time).ToList());
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_CallUserList();
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<Classes.CallUser>();
            }
        }

        public void Clear_CallUser_List()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.CallUserTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Clear_CallUser_List();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Last User Chat

        //Insert Or Update data To Users Table
        public void Insert_Or_Update_LastUsersChat(Context context, ObservableCollection<ChatObject> chatList, bool showFloating = false)
        {
            try
            {
                using (var connection = OpenConnection())
                {
                    if (connection == null) return;

                    var result = connection.Table<DataTables.LastUsersTb>().ToList();
                    List<DataTables.LastUsersTb> list = new List<DataTables.LastUsersTb>();
                    foreach (var item in chatList)
                    {
                        //ChatObject item = ChatTools.FilterDataLastChatNewV(itemChatObject);

                        DataTables.LastUsersTb db = new DataTables.LastUsersTb
                        {
                            Id = item.Id,
                            AvatarOrg = item.AvatarOrg,
                            BackgroundImageStatus = item.BackgroundImageStatus,
                            Boosted = item.Boosted,
                            CallActionType = item.CallActionType,
                            CallActionTypeUrl = item.CallActionTypeUrl,
                            Category = item.Category,
                            ChatTime = item.ChatTime,
                            ChatType = item.ChatType,
                            Company = item.Company,
                            CoverFull = item.CoverFull,
                            CoverOrg = item.CoverOrg,
                            CssFile = item.CssFile,
                            EmailCode = item.EmailCode,
                            GroupId = item.GroupId,
                            Instgram = item.Instgram,
                            Joined = item.Joined,
                            LastEmailSent = item.LastEmailSent,
                            PageCategory = item.PageCategory,
                            PageDescription = item.PageDescription,
                            PageId = item.PageId,
                            PageName = item.PageName,
                            PageTitle = item.PageTitle,
                            Phone = item.Phone,
                            GroupName = item.GroupName,
                            ProTime = item.ProTime,
                            Rating = item.Rating,
                            Showlastseen = item.Showlastseen,
                            SidebarData = item.SidebarData,
                            SmsCode = item.SmsCode,
                            SocialLogin = item.SocialLogin,
                            Src = item.Src,
                            StartUp = item.StartUp,
                            StartupFollow = item.StartupFollow,
                            StartupImage = item.StartupImage,
                            StartUpInfo = item.StartUpInfo,
                            UserId = item.UserId,
                            Username = item.Username,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Avatar = item.Avatar,
                            Cover = item.Cover,
                            BackgroundImage = item.BackgroundImage,
                            RelationshipId = item.RelationshipId,
                            Address = item.Address,
                            Working = item.Working,
                            Gender = item.Gender,
                            Facebook = item.Facebook,
                            Google = item.Google,
                            Twitter = item.Twitter,
                            Linkedin = item.Linkedin,
                            Website = item.Website,
                            Instagram = item.Instagram,
                            WebDeviceId = item.WebDeviceId,
                            Language = item.Language,
                            IpAddress = item.IpAddress,
                            PhoneNumber = item.PhoneNumber,
                            Timezone = item.Timezone,
                            Lat = item.Lat,
                            Lng = item.Lng,
                            About = item.About,
                            Birthday = item.Birthday,
                            Registered = item.Registered,
                            Lastseen = item.Lastseen,
                            LastLocationUpdate = item.LastLocationUpdate,
                            Balance = item.Balance,
                            Verified = item.Verified,
                            Status = item.Status,
                            Active = item.Active,
                            Admin = item.Admin,
                            IsPro = item.IsPro,
                            ProType = item.ProType,
                            School = item.School,
                            Name = item.Name,
                            AndroidMDeviceId = item.AndroidMDeviceId,
                            ECommented = item.ECommented,
                            AndroidNDeviceId = item.AndroidMDeviceId,
                            AvatarFull = item.AvatarFull,
                            BirthPrivacy = item.BirthPrivacy,
                            CanFollow = item.CanFollow,
                            ConfirmFollowers = item.ConfirmFollowers,
                            CountryId = item.CountryId,
                            EAccepted = item.EAccepted,
                            EFollowed = item.EFollowed,
                            EJoinedGroup = item.EJoinedGroup,
                            ELastNotif = item.ELastNotif,
                            ELiked = item.ELiked,
                            ELikedPage = item.ELikedPage,
                            EMentioned = item.EMentioned,
                            EProfileWallPost = item.EProfileWallPost,
                            ESentmeMsg = item.ESentmeMsg,
                            EShared = item.EShared,
                            EVisited = item.EVisited,
                            EWondered = item.EWondered,
                            EmailNotification = item.EmailNotification,
                            FollowPrivacy = item.FollowPrivacy,
                            FriendPrivacy = item.FriendPrivacy,
                            GenderText = item.GenderText,
                            InfoFile = item.InfoFile,
                            IosMDeviceId = item.IosMDeviceId,
                            IosNDeviceId = item.IosNDeviceId,
                            IsBlocked = item.IsBlocked,
                            IsFollowing = item.IsFollowing,
                            IsFollowingMe = item.IsFollowingMe,
                            LastAvatarMod = item.LastAvatarMod,
                            LastCoverMod = item.LastCoverMod,
                            LastDataUpdate = item.LastDataUpdate,
                            LastFollowId = item.LastFollowId,
                            LastLoginData = item.LastLoginData,
                            LastseenStatus = item.LastseenStatus,
                            LastseenTimeText = item.LastseenTimeText,
                            LastseenUnixTime = item.LastseenUnixTime,
                            MessagePrivacy = item.MessagePrivacy,
                            NewEmail = item.NewEmail,
                            NewPhone = item.NewPhone,
                            NotificationsSound = item.NotificationsSound,
                            OrderPostsBy = item.OrderPostsBy,
                            PaypalEmail = item.PaypalEmail,
                            PostPrivacy = item.PostPrivacy,
                            Referrer = item.Referrer,
                            ShareMyData = item.ShareMyData,
                            ShareMyLocation = item.ShareMyLocation,
                            ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                            TwoFactor = item.TwoFactor,
                            TwoFactorVerified = item.TwoFactorVerified,
                            Url = item.Url,
                            VisitPrivacy = item.VisitPrivacy,
                            Vk = item.Vk,
                            Wallet = item.Wallet,
                            WorkingLink = item.WorkingLink,
                            Youtube = item.Youtube,
                            City = item.City,
                            State = item.State,
                            Zip = item.Zip,
                            Points = item.Points,
                            DailyPoints = item.DailyPoints,
                            PointDayExpire = item.PointDayExpire,
                            CashfreeSignature = item.CashfreeSignature,
                            IsAdmin = item.IsAdmin,
                            MemberId = item.MemberId,
                            ChatColor = item.ChatColor,
                            PaystackRef = item.PaystackRef,
                            RefUserId = item.RefUserId,
                            SchoolCompleted = item.SchoolCompleted,
                            Type = item.Type,
                            UserPlatform = item.UserPlatform,
                            WeatherUnit = item.WeatherUnit,
                            AvatarPostId = item.AvatarPostId,
                            CodeSent = item.CodeSent,
                            CoverPostId = item.CoverPostId,
                            Discord = item.Discord,
                            IsArchive = item.IsArchive,
                            IsMute = item.IsMute,
                            IsPin = item.IsPin,
                            IsReported = item.IsReported,
                            IsStoryMuted = item.IsStoryMuted,
                            Mailru = item.Mailru,
                            NotificationSettings = item.NotificationSettings,
                            IsNotifyStopped = item.IsNotifyStopped,
                            Qq = item.Qq,
                            StripeSessionId = item.StripeSessionId,
                            Time = item.Time,
                            TimeCodeSent = item.TimeCodeSent,
                            Wechat = item.Wechat,
                            AlbumData = item.AlbumData,
                            ChatId = item.ChatId,
                            IsPageOnwer = item.IsPageOnwer,
                            MessageCount = item.MessageCount,
                            Selected = item.Selected,
                            AamarpayTranId = item.AamarpayTranId,
                            Banned = item.Banned,
                            BannedReason = item.BannedReason,
                            CoinbaseCode = item.CoinbaseCode,
                            CoinbaseHash = item.CoinbaseHash,
                            CoinpaymentsTxnId = item.CoinpaymentsTxnId,
                            ConversationId = item.ConversationId,
                            ConvertedPoints = item.ConvertedPoints,
                            Credits = item.Credits,
                            CurrentlyWorking = item.CurrentlyWorking,
                            FortumoHash = item.FortumoHash,
                            GoogleSecret = item.GoogleSecret,
                            HaveMonetization = item.HaveMonetization,
                            IsOpenToWork = item.IsOpenToWork,
                            IsProvidingService = item.IsProvidingService,
                            IsReportedUser = item.IsReportedUser,
                            IsVerified = item.IsVerified,
                            Languages = item.Languages,
                            NgeniusRef = item.NgeniusRef,
                            Okru = item.Okru,
                            PageSubCategory = item.PageSubCategory,
                            Permission = item.Permission,
                            PhonePrivacy = item.PhonePrivacy,
                            ProRemainder = item.ProRemainder,
                            ProvidingService = item.ProvidingService,
                            SecurionpayKey = item.SecurionpayKey,
                            TwoFactorMethod = item.TwoFactorMethod,
                            Skills = item.Skills,
                            SubCategory = item.SubCategory,
                            TwoFactorHash = item.TwoFactorHash,
                            UsersPost = item.UsersPost,
                            YoomoneyHash = item.YoomoneyHash,
                            Owner = Convert.ToBoolean(item.Owner),
                            UserData = JsonConvert.SerializeObject(item.UserData),
                            LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass),
                            Parts = JsonConvert.SerializeObject(item.Parts),
                            Details = JsonConvert.SerializeObject(item.Details.DetailsClass),
                            ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass),
                            Mute = JsonConvert.SerializeObject(item.Mute),
                        };

                        list.Add(db);

                        var update = result.FirstOrDefault(a => a.ChatId == item.ChatId);
                        if (update != null)
                        {
                            update = db;
                            update.UserData = JsonConvert.SerializeObject(item.UserData);
                            update.LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass);
                            update.Parts = JsonConvert.SerializeObject(item.Parts);
                            update.Details = JsonConvert.SerializeObject(item.Details.DetailsClass);
                            update.ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass);

                            var chk = IdMesgList.FirstOrDefault(a => a == item.LastMessage.LastMessageClass?.Id);

                            bool allowAppState = true;
                            if (Methods.AppLifecycleObserver.AppState == "Foreground")
                            {
                                var instance = ChatTabbedMainActivity.GetInstance();
                                if (instance?.ChatTab != null)
                                    allowAppState = false;
                            }

                            if (item.LastMessage.LastMessageClass != null && item.LastMessage.LastMessageClass.Seen == "0" && chk == null && item.LastMessage.LastMessageClass.FromId != UserDetails.UserId && item.Mute?.Notify == "yes" && allowAppState)
                            {
                                string id = "", name = "";
                                switch (item.ChatType)
                                {
                                    case "user":
                                        id = item.UserId;
                                        name = item.Name;
                                        break;
                                    case "page":
                                        id = item.PageId;
                                        name = item.PageName;
                                        break;
                                    case "group":
                                        id = item.GroupId;
                                        name = item.GroupName;
                                        break;
                                }

                                var instance = ChatHeadHelper.GetInstance(context);
                                if (showFloating && instance.CheckPermission())
                                {
                                    var floating = new ChatHeadObject
                                    {
                                        ChatType = item.ChatType,
                                        ChatId = item.ChatId,
                                        UserId = item.UserId,
                                        PageId = item.PageId,
                                        GroupId = item.GroupId,
                                        Avatar = item.Avatar,
                                        ChatColor = "",
                                        LastSeen = item.Lastseen,
                                        LastSeenUnixTime = item.LastseenUnixTime,
                                        Name = name,
                                        MessageCount = item.LastMessage.LastMessageClass.MessageCount ?? "1"
                                    };

                                    IdMesgList.Add(item.LastMessage.LastMessageClass.Id);

                                    if (instance.RunBubble)
                                        instance.DismissNotification();

                                    instance.ShowNotification(floating);
                                }
                                else
                                    AppNotificationsManager.Instance?.ShowUserNotification(item.ChatType, item.LastMessage.LastMessageClass?.Id, Methods.FunString.DecodeString(name), context.GetText(Resource.String.Lbl_SendMessage), id, item.ChatId, item.Avatar, AppSettings.MainColor, Convert.ToInt32(item.MessageCount));
                            }

                            connection.Update(update);
                        }
                    }

                    if (list.Count <= 0) return;

                    connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                    if (newItemList.Count > 0)
                        connection.InsertAll(newItemList);

                    result = connection.Table<DataTables.LastUsersTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            connection.Delete(delete);

                    connection.Commit();
                }

                ListUtils.UserList = chatList;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_LastUsersChat(context, chatList, showFloating);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To Users Table
        public ObservableCollection<ChatObject> Get_LastUsersChat_List()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;
                var select = connection.Table<DataTables.LastUsersTb>().ToList();
                if (select.Count > 0)
                {
                    var list = new List<ChatObject>();

                    foreach (DataTables.LastUsersTb item in select)
                    {
                        //Classes.LastChatArchive archiveObject = null;
                        ChatObject db = new ChatObject
                        {
                            Id = item.Id,
                            AvatarOrg = item.AvatarOrg,
                            BackgroundImageStatus = item.BackgroundImageStatus,
                            Boosted = item.Boosted,
                            CallActionType = item.CallActionType,
                            CallActionTypeUrl = item.CallActionTypeUrl,
                            Category = item.Category,
                            ChatTime = item.ChatTime,
                            ChatType = item.ChatType,
                            Company = item.Company,
                            CoverFull = item.CoverFull,
                            CoverOrg = item.CoverOrg,
                            CssFile = item.CssFile,
                            EmailCode = item.EmailCode,
                            GroupId = item.GroupId,
                            Instgram = item.Instgram,
                            Joined = item.Joined,
                            LastEmailSent = item.LastEmailSent,
                            PageCategory = item.PageCategory,
                            PageDescription = item.PageDescription,
                            PageId = item.PageId,
                            PageName = item.PageName,
                            PageTitle = item.PageTitle,
                            Phone = item.Phone,
                            GroupName = item.GroupName,
                            ProTime = item.ProTime,
                            Rating = item.Rating,
                            Showlastseen = item.Showlastseen,
                            SidebarData = item.SidebarData,
                            SmsCode = item.SmsCode,
                            SocialLogin = item.SocialLogin,
                            Src = item.Src,
                            StartUp = item.StartUp,
                            StartupFollow = item.StartupFollow,
                            StartupImage = item.StartupImage,
                            StartUpInfo = item.StartUpInfo,
                            UserId = item.UserId,
                            Username = item.Username,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Avatar = item.Avatar,
                            Cover = item.Cover,
                            BackgroundImage = item.BackgroundImage,
                            RelationshipId = item.RelationshipId,
                            Address = item.Address,
                            Working = item.Working,
                            Gender = item.Gender,
                            Facebook = item.Facebook,
                            Google = item.Google,
                            Twitter = item.Twitter,
                            Linkedin = item.Linkedin,
                            Website = item.Website,
                            Instagram = item.Instagram,
                            WebDeviceId = item.WebDeviceId,
                            Language = item.Language,
                            IpAddress = item.IpAddress,
                            PhoneNumber = item.PhoneNumber,
                            Timezone = item.Timezone,
                            Lat = item.Lat,
                            Lng = item.Lng,
                            About = item.About,
                            Birthday = item.Birthday,
                            Registered = item.Registered,
                            Lastseen = item.Lastseen,
                            LastLocationUpdate = item.LastLocationUpdate,
                            Balance = item.Balance,
                            Verified = item.Verified,
                            Status = item.Status,
                            Active = item.Active,
                            Admin = item.Admin,
                            IsPro = item.IsPro,
                            ProType = item.ProType,
                            School = item.School,
                            Name = item.Name,
                            AndroidMDeviceId = item.AndroidMDeviceId,
                            ECommented = item.ECommented,
                            AndroidNDeviceId = item.AndroidMDeviceId,
                            AvatarFull = item.AvatarFull,
                            BirthPrivacy = item.BirthPrivacy,
                            CanFollow = item.CanFollow,
                            ConfirmFollowers = item.ConfirmFollowers,
                            CountryId = item.CountryId,
                            EAccepted = item.EAccepted,
                            EFollowed = item.EFollowed,
                            EJoinedGroup = item.EJoinedGroup,
                            ELastNotif = item.ELastNotif,
                            ELiked = item.ELiked,
                            ELikedPage = item.ELikedPage,
                            EMentioned = item.EMentioned,
                            EProfileWallPost = item.EProfileWallPost,
                            ESentmeMsg = item.ESentmeMsg,
                            EShared = item.EShared,
                            EVisited = item.EVisited,
                            EWondered = item.EWondered,
                            EmailNotification = item.EmailNotification,
                            FollowPrivacy = item.FollowPrivacy,
                            FriendPrivacy = item.FriendPrivacy,
                            GenderText = item.GenderText,
                            InfoFile = item.InfoFile,
                            IosMDeviceId = item.IosMDeviceId,
                            IosNDeviceId = item.IosNDeviceId,
                            IsBlocked = item.IsBlocked,
                            IsFollowing = item.IsFollowing,
                            IsFollowingMe = item.IsFollowingMe,
                            LastAvatarMod = item.LastAvatarMod,
                            LastCoverMod = item.LastCoverMod,
                            LastDataUpdate = item.LastDataUpdate,
                            LastFollowId = item.LastFollowId,
                            LastLoginData = item.LastLoginData,
                            LastseenStatus = item.LastseenStatus,
                            LastseenTimeText = item.LastseenTimeText,
                            LastseenUnixTime = item.LastseenUnixTime,
                            MessagePrivacy = item.MessagePrivacy,
                            NewEmail = item.NewEmail,
                            NewPhone = item.NewPhone,
                            NotificationsSound = item.NotificationsSound,
                            OrderPostsBy = item.OrderPostsBy,
                            PaypalEmail = item.PaypalEmail,
                            PostPrivacy = item.PostPrivacy,
                            Referrer = item.Referrer,
                            ShareMyData = item.ShareMyData,
                            ShareMyLocation = item.ShareMyLocation,
                            ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                            TwoFactor = item.TwoFactor,
                            TwoFactorVerified = item.TwoFactorVerified,
                            Url = item.Url,
                            VisitPrivacy = item.VisitPrivacy,
                            Vk = item.Vk,
                            Wallet = item.Wallet,
                            WorkingLink = item.WorkingLink,
                            Youtube = item.Youtube,
                            City = item.City,
                            State = item.State,
                            Zip = item.Zip,
                            Points = item.Points,
                            DailyPoints = item.DailyPoints,
                            PointDayExpire = item.PointDayExpire,
                            CashfreeSignature = item.CashfreeSignature,
                            IsAdmin = item.IsAdmin,
                            MemberId = item.MemberId,
                            ChatColor = item.ChatColor,
                            PaystackRef = item.PaystackRef,
                            RefUserId = item.RefUserId,
                            SchoolCompleted = item.SchoolCompleted,
                            Type = item.Type,
                            UserPlatform = item.UserPlatform,
                            WeatherUnit = item.WeatherUnit,
                            AvatarPostId = item.AvatarPostId,
                            CodeSent = item.CodeSent,
                            CoverPostId = item.CoverPostId,
                            Discord = item.Discord,
                            IsArchive = item.IsArchive,
                            IsMute = item.IsMute,
                            IsPin = item.IsPin,
                            IsReported = item.IsReported,
                            IsStoryMuted = item.IsStoryMuted,
                            Mailru = item.Mailru,
                            NotificationSettings = item.NotificationSettings,
                            IsNotifyStopped = item.IsNotifyStopped,
                            Qq = item.Qq,
                            StripeSessionId = item.StripeSessionId,
                            Time = item.Time,
                            TimeCodeSent = item.TimeCodeSent,
                            Wechat = item.Wechat,
                            AlbumData = item.AlbumData,
                            ChatId = item.ChatId,
                            IsPageOnwer = item.IsPageOnwer,
                            MessageCount = item.MessageCount,
                            AamarpayTranId = item.AamarpayTranId,
                            Banned = item.Banned,
                            BannedReason = item.BannedReason,
                            CoinbaseCode = item.CoinbaseCode,
                            CoinbaseHash = item.CoinbaseHash,
                            CoinpaymentsTxnId = item.CoinpaymentsTxnId,
                            ConversationId = item.ConversationId,
                            ConvertedPoints = item.ConvertedPoints,
                            Credits = item.Credits,
                            CurrentlyWorking = item.CurrentlyWorking,
                            FortumoHash = item.FortumoHash,
                            GoogleSecret = item.GoogleSecret,
                            HaveMonetization = item.HaveMonetization,
                            IsOpenToWork = item.IsOpenToWork,
                            IsProvidingService = item.IsProvidingService,
                            IsReportedUser = item.IsReportedUser,
                            IsVerified = item.IsVerified,
                            Languages = item.Languages,
                            NgeniusRef = item.NgeniusRef,
                            Okru = item.Okru,
                            PageSubCategory = item.PageSubCategory,
                            Permission = item.Permission,
                            PhonePrivacy = item.PhonePrivacy,
                            ProRemainder = item.ProRemainder,
                            ProvidingService = item.ProvidingService,
                            SecurionpayKey = item.SecurionpayKey,
                            TwoFactorMethod = item.TwoFactorMethod,
                            Skills = item.Skills,
                            SubCategory = item.SubCategory,
                            TwoFactorHash = item.TwoFactorHash,
                            UsersPost = item.UsersPost,
                            YoomoneyHash = item.YoomoneyHash,
                            Selected = false,
                            UserData = new UserDataObject(),
                            LastMessage = new LastMessageUnion { LastMessageClass = new MessageData() },
                            Parts = new List<UserDataObject>(),
                            Details = new DetailsUnion(),
                            ApiNotificationSettings = new NotificationSettingsUnion(),
                            Mute = new Mute(),
                        };

                        if (!string.IsNullOrEmpty(item.UserData))
                            db.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                        if (!string.IsNullOrEmpty(item.LastMessage))
                        {
                            var sss = JsonConvert.DeserializeObject<MessageData>(item.LastMessage);
                            if (sss != null)
                            {
                                db.LastMessage = new LastMessageUnion
                                {
                                    LastMessageClass = sss
                                };
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Parts))
                            db.Parts = JsonConvert.DeserializeObject<List<UserDataObject>>(item.Parts);

                        if (!string.IsNullOrEmpty(item.Mute))
                            db.Mute = JsonConvert.DeserializeObject<Mute>(item.Mute);

                        db = ChatTools.FilterDataLastChatNewV(db);

                        if (!string.IsNullOrEmpty(item.Details))
                        {
                            var sss = JsonConvert.DeserializeObject<Details>(item.Details);
                            if (sss != null)
                            {
                                db.Details = new DetailsUnion
                                {
                                    DetailsClass = sss
                                };
                            }
                        }

                        if (!string.IsNullOrEmpty(item.ApiNotificationSettings))
                        {
                            var sss = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings);
                            if (sss != null)
                            {
                                db.ApiNotificationSettings = new NotificationSettingsUnion
                                {
                                    NotificationSettingsClass = sss
                                };
                            }
                        }

                        if (db.IsPin)
                        {
                            list.Insert(0, db);
                        }
                        else
                        {
                            list.Add(db);
                        }
                    }

                    return new ObservableCollection<ChatObject>(list);
                }

                return new ObservableCollection<ChatObject>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_LastUsersChat_List();
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<ChatObject>();
            }
        }

        //Insert Or Update data from Users Table
        public void Insert_Or_Update_one_LastUsersChat(ChatObject item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                var chat = connection.Table<DataTables.LastUsersTb>().FirstOrDefault(c => c.ChatId == item.ChatId && c.ChatType == item.ChatType);
                if (chat != null)
                {
                    chat.Id = item.Id;
                    chat.AvatarOrg = item.AvatarOrg;
                    chat.BackgroundImageStatus = item.BackgroundImageStatus;
                    chat.Boosted = item.Boosted;
                    chat.CallActionType = item.CallActionType;
                    chat.CallActionTypeUrl = item.CallActionTypeUrl;
                    chat.Category = item.Category;
                    chat.ChatTime = item.ChatTime;
                    chat.ChatType = item.ChatType;
                    chat.Company = item.Company;
                    chat.CoverFull = item.CoverFull;
                    chat.CoverOrg = item.CoverOrg;
                    chat.CssFile = item.CssFile;
                    chat.EmailCode = item.EmailCode;
                    chat.GroupId = item.GroupId;
                    chat.Instgram = item.Instgram;
                    chat.Joined = item.Joined;
                    chat.LastEmailSent = item.LastEmailSent;
                    chat.PageCategory = item.PageCategory;
                    chat.PageDescription = item.PageDescription;
                    chat.PageId = item.PageId;
                    chat.PageName = item.PageName;
                    chat.PageTitle = item.PageTitle;
                    chat.Phone = item.Phone;
                    chat.GroupName = item.GroupName;
                    chat.ProTime = item.ProTime;
                    chat.Rating = item.Rating;
                    chat.Showlastseen = item.Showlastseen;
                    chat.SidebarData = item.SidebarData;
                    chat.SmsCode = item.SmsCode;
                    chat.SocialLogin = item.SocialLogin;
                    chat.Src = item.Src;
                    chat.StartUp = item.StartUp;
                    chat.StartupFollow = item.StartupFollow;
                    chat.StartupImage = item.StartupImage;
                    chat.StartUpInfo = item.StartUpInfo;
                    chat.UserId = item.UserId;
                    chat.Username = item.Username;
                    chat.Email = item.Email;
                    chat.FirstName = item.FirstName;
                    chat.LastName = item.LastName;
                    chat.Avatar = item.Avatar;
                    chat.Cover = item.Cover;
                    chat.BackgroundImage = item.BackgroundImage;
                    chat.RelationshipId = item.RelationshipId;
                    chat.Address = item.Address;
                    chat.Working = item.Working;
                    chat.Gender = item.Gender;
                    chat.Facebook = item.Facebook;
                    chat.Google = item.Google;
                    chat.Twitter = item.Twitter;
                    chat.Linkedin = item.Linkedin;
                    chat.Website = item.Website;
                    chat.Instagram = item.Instagram;
                    chat.WebDeviceId = item.WebDeviceId;
                    chat.Language = item.Language;
                    chat.IpAddress = item.IpAddress;
                    chat.PhoneNumber = item.PhoneNumber;
                    chat.Timezone = item.Timezone;
                    chat.Lat = item.Lat;
                    chat.Lng = item.Lng;
                    chat.About = item.About;
                    chat.Birthday = item.Birthday;
                    chat.Registered = item.Registered;
                    chat.Lastseen = item.Lastseen;
                    chat.LastLocationUpdate = item.LastLocationUpdate;
                    chat.Balance = item.Balance;
                    chat.Verified = item.Verified;
                    chat.Status = item.Status;
                    chat.Active = item.Active;
                    chat.Admin = item.Admin;
                    chat.IsPro = item.IsPro;
                    chat.ProType = item.ProType;
                    chat.School = item.School;
                    chat.Name = item.Name;
                    chat.AndroidMDeviceId = item.AndroidMDeviceId;
                    chat.ECommented = item.ECommented;
                    chat.AndroidNDeviceId = item.AndroidMDeviceId;
                    chat.AvatarFull = item.AvatarFull;
                    chat.BirthPrivacy = item.BirthPrivacy;
                    chat.CanFollow = item.CanFollow;
                    chat.ConfirmFollowers = item.ConfirmFollowers;
                    chat.CountryId = item.CountryId;
                    chat.EAccepted = item.EAccepted;
                    chat.EFollowed = item.EFollowed;
                    chat.EJoinedGroup = item.EJoinedGroup;
                    chat.ELastNotif = item.ELastNotif;
                    chat.ELiked = item.ELiked;
                    chat.ELikedPage = item.ELikedPage;
                    chat.EMentioned = item.EMentioned;
                    chat.EProfileWallPost = item.EProfileWallPost;
                    chat.ESentmeMsg = item.ESentmeMsg;
                    chat.EShared = item.EShared;
                    chat.EVisited = item.EVisited;
                    chat.EWondered = item.EWondered;
                    chat.EmailNotification = item.EmailNotification;
                    chat.FollowPrivacy = item.FollowPrivacy;
                    chat.FriendPrivacy = item.FriendPrivacy;
                    chat.GenderText = item.GenderText;
                    chat.InfoFile = item.InfoFile;
                    chat.IosMDeviceId = item.IosMDeviceId;
                    chat.IosNDeviceId = item.IosNDeviceId;
                    chat.IsBlocked = item.IsBlocked;
                    chat.IsFollowing = item.IsFollowing;
                    chat.IsFollowingMe = item.IsFollowingMe;
                    chat.LastAvatarMod = item.LastAvatarMod;
                    chat.LastCoverMod = item.LastCoverMod;
                    chat.LastDataUpdate = item.LastDataUpdate;
                    chat.LastFollowId = item.LastFollowId;
                    chat.LastLoginData = item.LastLoginData;
                    chat.LastseenStatus = item.LastseenStatus;
                    chat.LastseenTimeText = item.LastseenTimeText;
                    chat.LastseenUnixTime = item.LastseenUnixTime;
                    chat.MessagePrivacy = item.MessagePrivacy;
                    chat.NewEmail = item.NewEmail;
                    chat.NewPhone = item.NewPhone;
                    chat.NotificationsSound = item.NotificationsSound;
                    chat.OrderPostsBy = item.OrderPostsBy;
                    chat.PaypalEmail = item.PaypalEmail;
                    chat.PostPrivacy = item.PostPrivacy;
                    chat.Referrer = item.Referrer;
                    chat.ShareMyData = item.ShareMyData;
                    chat.ShareMyLocation = item.ShareMyLocation;
                    chat.ShowActivitiesPrivacy = item.ShowActivitiesPrivacy;
                    chat.TwoFactor = item.TwoFactor;
                    chat.TwoFactorVerified = item.TwoFactorVerified;
                    chat.Url = item.Url;
                    chat.VisitPrivacy = item.VisitPrivacy;
                    chat.Vk = item.Vk;
                    chat.Wallet = item.Wallet;
                    chat.WorkingLink = item.WorkingLink;
                    chat.Youtube = item.Youtube;
                    chat.City = item.City;
                    chat.State = item.State;
                    chat.Zip = item.Zip;
                    chat.Points = item.Points;
                    chat.DailyPoints = item.DailyPoints;
                    chat.PointDayExpire = item.PointDayExpire;
                    chat.CashfreeSignature = item.CashfreeSignature;
                    chat.IsAdmin = item.IsAdmin;
                    chat.MemberId = item.MemberId;
                    chat.ChatColor = item.ChatColor;
                    chat.PaystackRef = item.PaystackRef;
                    chat.RefUserId = item.RefUserId;
                    chat.SchoolCompleted = item.SchoolCompleted;
                    chat.Type = item.Type;
                    chat.UserPlatform = item.UserPlatform;
                    chat.WeatherUnit = item.WeatherUnit;
                    chat.AvatarPostId = item.AvatarPostId;
                    chat.CodeSent = item.CodeSent;
                    chat.CoverPostId = item.CoverPostId;
                    chat.Discord = item.Discord;
                    chat.IsArchive = item.IsArchive;
                    chat.IsMute = item.IsMute;
                    chat.IsPin = item.IsPin;
                    chat.IsReported = item.IsReported;
                    chat.IsStoryMuted = item.IsStoryMuted;
                    chat.Mailru = item.Mailru;
                    chat.NotificationSettings = item.NotificationSettings;
                    chat.IsNotifyStopped = item.IsNotifyStopped;
                    chat.Qq = item.Qq;
                    chat.StripeSessionId = item.StripeSessionId;
                    chat.Time = item.Time;
                    chat.TimeCodeSent = item.TimeCodeSent;
                    chat.Wechat = item.Wechat;
                    chat.AlbumData = item.AlbumData;
                    chat.ChatId = item.ChatId;
                    chat.IsPageOnwer = item.IsPageOnwer;
                    chat.MessageCount = item.MessageCount;
                    chat.Selected = item.Selected;
                    chat.AamarpayTranId = item.AamarpayTranId;
                    chat.Banned = item.Banned;
                    chat.BannedReason = item.BannedReason;
                    chat.CoinbaseCode = item.CoinbaseCode;
                    chat.CoinbaseHash = item.CoinbaseHash;
                    chat.CoinpaymentsTxnId = item.CoinpaymentsTxnId;
                    chat.ConversationId = item.ConversationId;
                    chat.ConvertedPoints = item.ConvertedPoints;
                    chat.Credits = item.Credits;
                    chat.CurrentlyWorking = item.CurrentlyWorking;
                    chat.FortumoHash = item.FortumoHash;
                    chat.GoogleSecret = item.GoogleSecret;
                    chat.HaveMonetization = item.HaveMonetization;
                    chat.IsOpenToWork = item.IsOpenToWork;
                    chat.IsProvidingService = item.IsProvidingService;
                    chat.IsReportedUser = item.IsReportedUser;
                    chat.IsVerified = item.IsVerified;
                    chat.Languages = item.Languages;
                    chat.NgeniusRef = item.NgeniusRef;
                    chat.Okru = item.Okru;
                    chat.PageSubCategory = item.PageSubCategory;
                    chat.Permission = item.Permission;
                    chat.PhonePrivacy = item.PhonePrivacy;
                    chat.ProRemainder = item.ProRemainder;
                    chat.ProvidingService = item.ProvidingService;
                    chat.SecurionpayKey = item.SecurionpayKey;
                    chat.TwoFactorMethod = item.TwoFactorMethod;
                    chat.Skills = item.Skills;
                    chat.SubCategory = item.SubCategory;
                    chat.TwoFactorHash = item.TwoFactorHash;
                    chat.UsersPost = item.UsersPost;
                    chat.YoomoneyHash = item.YoomoneyHash;
                    chat.Owner = Convert.ToBoolean(item.Owner);
                    chat.UserData = JsonConvert.SerializeObject(item.UserData);
                    chat.LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass);
                    chat.Parts = JsonConvert.SerializeObject(item.Parts);
                    chat.Details = JsonConvert.SerializeObject(item.Details.DetailsClass);
                    chat.ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass);
                    chat.Mute = JsonConvert.SerializeObject(item.Mute);

                    connection.Update(chat);
                }
                else
                {
                    DataTables.LastUsersTb db = new DataTables.LastUsersTb
                    {
                        Id = item.Id,
                        AvatarOrg = item.AvatarOrg,
                        BackgroundImageStatus = item.BackgroundImageStatus,
                        Boosted = item.Boosted,
                        CallActionType = item.CallActionType,
                        CallActionTypeUrl = item.CallActionTypeUrl,
                        Category = item.Category,
                        ChatTime = item.ChatTime,
                        ChatType = item.ChatType,
                        Company = item.Company,
                        CoverFull = item.CoverFull,
                        CoverOrg = item.CoverOrg,
                        CssFile = item.CssFile,
                        EmailCode = item.EmailCode,
                        GroupId = item.GroupId,
                        Instgram = item.Instgram,
                        Joined = item.Joined,
                        LastEmailSent = item.LastEmailSent,
                        PageCategory = item.PageCategory,
                        PageDescription = item.PageDescription,
                        PageId = item.PageId,
                        PageName = item.PageName,
                        PageTitle = item.PageTitle,
                        Phone = item.Phone,
                        GroupName = item.GroupName,
                        ProTime = item.ProTime,
                        Rating = item.Rating,
                        Showlastseen = item.Showlastseen,
                        SidebarData = item.SidebarData,
                        SmsCode = item.SmsCode,
                        SocialLogin = item.SocialLogin,
                        Src = item.Src,
                        StartUp = item.StartUp,
                        StartupFollow = item.StartupFollow,
                        StartupImage = item.StartupImage,
                        StartUpInfo = item.StartUpInfo,
                        UserId = item.UserId,
                        Username = item.Username,
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Avatar = item.Avatar,
                        Cover = item.Cover,
                        BackgroundImage = item.BackgroundImage,
                        RelationshipId = item.RelationshipId,
                        Address = item.Address,
                        Working = item.Working,
                        Gender = item.Gender,
                        Facebook = item.Facebook,
                        Google = item.Google,
                        Twitter = item.Twitter,
                        Linkedin = item.Linkedin,
                        Website = item.Website,
                        Instagram = item.Instagram,
                        WebDeviceId = item.WebDeviceId,
                        Language = item.Language,
                        IpAddress = item.IpAddress,
                        PhoneNumber = item.PhoneNumber,
                        Timezone = item.Timezone,
                        Lat = item.Lat,
                        Lng = item.Lng,
                        About = item.About,
                        Birthday = item.Birthday,
                        Registered = item.Registered,
                        Lastseen = item.Lastseen,
                        LastLocationUpdate = item.LastLocationUpdate,
                        Balance = item.Balance,
                        Verified = item.Verified,
                        Status = item.Status,
                        Active = item.Active,
                        Admin = item.Admin,
                        IsPro = item.IsPro,
                        ProType = item.ProType,
                        School = item.School,
                        Name = item.Name,
                        AndroidMDeviceId = item.AndroidMDeviceId,
                        ECommented = item.ECommented,
                        AndroidNDeviceId = item.AndroidMDeviceId,
                        AvatarFull = item.AvatarFull,
                        BirthPrivacy = item.BirthPrivacy,
                        CanFollow = item.CanFollow,
                        ConfirmFollowers = item.ConfirmFollowers,
                        CountryId = item.CountryId,
                        EAccepted = item.EAccepted,
                        EFollowed = item.EFollowed,
                        EJoinedGroup = item.EJoinedGroup,
                        ELastNotif = item.ELastNotif,
                        ELiked = item.ELiked,
                        ELikedPage = item.ELikedPage,
                        EMentioned = item.EMentioned,
                        EProfileWallPost = item.EProfileWallPost,
                        ESentmeMsg = item.ESentmeMsg,
                        EShared = item.EShared,
                        EVisited = item.EVisited,
                        EWondered = item.EWondered,
                        EmailNotification = item.EmailNotification,
                        FollowPrivacy = item.FollowPrivacy,
                        FriendPrivacy = item.FriendPrivacy,
                        GenderText = item.GenderText,
                        InfoFile = item.InfoFile,
                        IosMDeviceId = item.IosMDeviceId,
                        IosNDeviceId = item.IosNDeviceId,
                        IsBlocked = item.IsBlocked,
                        IsFollowing = item.IsFollowing,
                        IsFollowingMe = item.IsFollowingMe,
                        LastAvatarMod = item.LastAvatarMod,
                        LastCoverMod = item.LastCoverMod,
                        LastDataUpdate = item.LastDataUpdate,
                        LastFollowId = item.LastFollowId,
                        LastLoginData = item.LastLoginData,
                        LastseenStatus = item.LastseenStatus,
                        LastseenTimeText = item.LastseenTimeText,
                        LastseenUnixTime = item.LastseenUnixTime,
                        MessagePrivacy = item.MessagePrivacy,
                        NewEmail = item.NewEmail,
                        NewPhone = item.NewPhone,
                        NotificationsSound = item.NotificationsSound,
                        OrderPostsBy = item.OrderPostsBy,
                        PaypalEmail = item.PaypalEmail,
                        PostPrivacy = item.PostPrivacy,
                        Referrer = item.Referrer,
                        ShareMyData = item.ShareMyData,
                        ShareMyLocation = item.ShareMyLocation,
                        ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                        TwoFactor = item.TwoFactor,
                        TwoFactorVerified = item.TwoFactorVerified,
                        Url = item.Url,
                        VisitPrivacy = item.VisitPrivacy,
                        Vk = item.Vk,
                        Wallet = item.Wallet,
                        WorkingLink = item.WorkingLink,
                        Youtube = item.Youtube,
                        City = item.City,
                        State = item.State,
                        Zip = item.Zip,
                        Points = item.Points,
                        DailyPoints = item.DailyPoints,
                        PointDayExpire = item.PointDayExpire,
                        CashfreeSignature = item.CashfreeSignature,
                        IsAdmin = item.IsAdmin,
                        MemberId = item.MemberId,
                        ChatColor = item.ChatColor,
                        PaystackRef = item.PaystackRef,
                        RefUserId = item.RefUserId,
                        SchoolCompleted = item.SchoolCompleted,
                        Type = item.Type,
                        UserPlatform = item.UserPlatform,
                        WeatherUnit = item.WeatherUnit,
                        AvatarPostId = item.AvatarPostId,
                        CodeSent = item.CodeSent,
                        CoverPostId = item.CoverPostId,
                        Discord = item.Discord,
                        IsArchive = item.IsArchive,
                        IsMute = item.IsMute,
                        IsPin = item.IsPin,
                        IsReported = item.IsReported,
                        IsStoryMuted = item.IsStoryMuted,
                        Mailru = item.Mailru,
                        NotificationSettings = item.NotificationSettings,
                        IsNotifyStopped = item.IsNotifyStopped,
                        Qq = item.Qq,
                        StripeSessionId = item.StripeSessionId,
                        Time = item.Time,
                        TimeCodeSent = item.TimeCodeSent,
                        Wechat = item.Wechat,
                        AlbumData = item.AlbumData,
                        ChatId = item.ChatId,
                        IsPageOnwer = item.IsPageOnwer,
                        MessageCount = item.MessageCount,
                        Selected = item.Selected,
                        AamarpayTranId = item.AamarpayTranId,
                        Banned = item.Banned,
                        BannedReason = item.BannedReason,
                        CoinbaseCode = item.CoinbaseCode,
                        CoinbaseHash = item.CoinbaseHash,
                        CoinpaymentsTxnId = item.CoinpaymentsTxnId,
                        ConversationId = item.ConversationId,
                        ConvertedPoints = item.ConvertedPoints,
                        Credits = item.Credits,
                        CurrentlyWorking = item.CurrentlyWorking,
                        FortumoHash = item.FortumoHash,
                        GoogleSecret = item.GoogleSecret,
                        HaveMonetization = item.HaveMonetization,
                        IsOpenToWork = item.IsOpenToWork,
                        IsProvidingService = item.IsProvidingService,
                        IsReportedUser = item.IsReportedUser,
                        IsVerified = item.IsVerified,
                        Languages = item.Languages,
                        NgeniusRef = item.NgeniusRef,
                        Okru = item.Okru,
                        PageSubCategory = item.PageSubCategory,
                        Permission = item.Permission,
                        PhonePrivacy = item.PhonePrivacy,
                        ProRemainder = item.ProRemainder,
                        ProvidingService = item.ProvidingService,
                        SecurionpayKey = item.SecurionpayKey,
                        TwoFactorMethod = item.TwoFactorMethod,
                        Skills = item.Skills,
                        SubCategory = item.SubCategory,
                        TwoFactorHash = item.TwoFactorHash,
                        UsersPost = item.UsersPost,
                        YoomoneyHash = item.YoomoneyHash,
                        Owner = Convert.ToBoolean(item.Owner),
                        UserData = JsonConvert.SerializeObject(item.UserData),
                        LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass),
                        Parts = JsonConvert.SerializeObject(item.Parts),
                        Details = JsonConvert.SerializeObject(item.Details.DetailsClass),
                        ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass),
                        Mute = JsonConvert.SerializeObject(item.Mute),
                    };
                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_one_LastUsersChat(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove data from Users Table
        public void Delete_LastUsersChat(string id, string chatType, string recipientId = "")
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                if (chatType == "user")
                {
                    var user = connection.Table<DataTables.LastUsersTb>().FirstOrDefault(c => c.UserId == id && c.ChatType == "user");
                    if (user != null)
                        connection.Delete(user);
                }
                else if (chatType == "page")
                {
                    var page = connection.Table<DataTables.LastUsersTb>().FirstOrDefault(c => c.PageId == id && JsonConvert.DeserializeObject<MessageDataExtra>(c.LastMessage).ToData.UserId == recipientId && c.ChatType == "page");
                    if (page != null)
                        connection.Delete(page);
                }
                else if (chatType == "group")
                {
                    var group = connection.Table<DataTables.LastUsersTb>().FirstOrDefault(c => c.GroupId == id && c.ChatType == "group");
                    if (group != null)
                        connection.Delete(group);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_LastUsersChat(id, chatType, recipientId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove All data To Users Table
        public void ClearAll_LastUsersChat()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.LastUsersTb>();

                DeleteAllPin();
                DeleteAllMute();
                ClearAll_Archive();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_LastUsersChat();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Mute

        public void InsertORDelete_Mute(Classes.OptionLastChat item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var result = connection.Table<DataTables.MuteTb>().ToList();
                var check = result.FirstOrDefault(a => a.ChatId == item.ChatId && a.ChatType == item.ChatType);
                if (check == null)
                {
                    DataTables.MuteTb cv = new DataTables.MuteTb
                    {
                        ChatType = item.ChatType,
                        ChatId = item.ChatId,
                        UserId = item.UserId,
                        GroupId = item.GroupId,
                        PageId = item.PageId,
                        Name = item.Name
                    };
                    connection.Insert(cv);
                }
                else
                {
                    connection.Delete(check);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertORDelete_Mute(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public ObservableCollection<Classes.OptionLastChat> Get_MuteList()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return new ObservableCollection<Classes.OptionLastChat>();
                var list = new ObservableCollection<Classes.OptionLastChat>();
                var result = connection.Table<DataTables.MuteTb>().ToList();
                if (result.Count <= 0) return new ObservableCollection<Classes.OptionLastChat>();
                foreach (var cv in result.Select(item => new Classes.OptionLastChat
                {
                    ChatType = item.ChatType,
                    ChatId = item.ChatId,
                    UserId = item.UserId,
                    GroupId = item.GroupId,
                    PageId = item.PageId,
                    Name = item.Name
                }))
                {
                    list.Add(cv);
                }

                return new ObservableCollection<Classes.OptionLastChat>(list);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_MuteList();
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<Classes.OptionLastChat>();
            }
        }

        public void DeleteAllMute()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.MuteTb>();

                ListUtils.MuteList?.Clear();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllMute();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Pin

        public void InsertORDelete_Pin(Classes.OptionLastChat item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                var result = connection.Table<DataTables.PinTb>().ToList();
                var check = result.FirstOrDefault(a => a.ChatId == item.ChatId && a.ChatType == item.ChatType);
                if (check == null)
                {
                    DataTables.PinTb cv = new DataTables.PinTb
                    {
                        ChatType = item.ChatType,
                        ChatId = item.ChatId,
                        UserId = item.UserId,
                        GroupId = item.GroupId,
                        PageId = item.PageId,
                        Name = item.Name
                    };
                    connection.Insert(cv);
                }
                else
                {
                    connection.Delete(check);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertORDelete_Pin(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertORUpdateORDelete_ListPin(List<Classes.OptionLastChat> lastChatArchive)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                var result = connection.Table<DataTables.PinTb>().ToList();
                List<DataTables.PinTb> list = new List<DataTables.PinTb>();

                connection.BeginTransaction();

                foreach (var item in lastChatArchive)
                {
                    var db = new DataTables.PinTb
                    {
                        ChatType = item.ChatType,
                        ChatId = item.ChatId,
                        UserId = item.UserId,
                        GroupId = item.GroupId,
                        PageId = item.PageId,
                        Name = item.Name,
                    };
                    list.Add(db);

                    var update = result.FirstOrDefault(a => a.ChatId == item.ChatId);
                    if (update != null)
                    {
                        update.ChatType = item.ChatType;
                        update.ChatId = item.ChatId;
                        update.UserId = item.UserId;
                        update.GroupId = item.GroupId;
                        update.PageId = item.PageId;
                        update.Name = item.Name;

                        connection.Update(update);
                    }
                }

                if (list.Count <= 0) return;

                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.ChatId).Contains(c.ChatId)).ToList();
                if (newItemList.Count > 0) connection.InsertAll(newItemList);

                result = connection.Table<DataTables.PinTb>().ToList();
                var deleteItemList = result.Where(c => !list.Select(fc => fc.ChatId).Contains(c.ChatId)).ToList();
                if (deleteItemList.Count > 0)
                    foreach (var delete in deleteItemList)
                        connection.Delete(delete);

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertORUpdateORDelete_ListPin(lastChatArchive);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public ObservableCollection<Classes.OptionLastChat> Get_PinList()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return new ObservableCollection<Classes.OptionLastChat>();
                var list = new ObservableCollection<Classes.OptionLastChat>();
                var result = connection.Table<DataTables.PinTb>().ToList();
                if (result.Count <= 0) return new ObservableCollection<Classes.OptionLastChat>();
                foreach (var cv in result.Select(item => new Classes.OptionLastChat
                {
                    ChatType = item.ChatType,
                    ChatId = item.ChatId,
                    UserId = item.UserId,
                    GroupId = item.GroupId,
                    PageId = item.PageId,
                    Name = item.Name
                }))
                {
                    list.Add(cv);
                }

                return new ObservableCollection<Classes.OptionLastChat>(list);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_PinList();
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<Classes.OptionLastChat>();
            }
        }

        public void DeleteAllPin()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.PinTb>();

                ListUtils.PinList?.Clear();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllPin();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Archive

        //Insert Or Update data To Users Table
        public void Insert_Or_Update_Archive(ObservableCollection<Classes.LastChatsClass> chatList)
        {
            try
            {
                using (var connection = OpenConnection())
                {
                    if (connection == null) return;

                    var result = connection.Table<DataTables.ArchiveTb>().ToList();
                    List<DataTables.ArchiveTb> list = new List<DataTables.ArchiveTb>();
                    foreach (var chat in chatList)
                    {
                        ChatObject item = chat.LastChat;

                        DataTables.ArchiveTb db = new DataTables.ArchiveTb
                        {
                            Id = item.Id,
                            AvatarOrg = item.AvatarOrg,
                            BackgroundImageStatus = item.BackgroundImageStatus,
                            Boosted = item.Boosted,
                            CallActionType = item.CallActionType,
                            CallActionTypeUrl = item.CallActionTypeUrl,
                            Category = item.Category,
                            ChatTime = item.ChatTime,
                            ChatType = item.ChatType,
                            Company = item.Company,
                            CoverFull = item.CoverFull,
                            CoverOrg = item.CoverOrg,
                            CssFile = item.CssFile,
                            EmailCode = item.EmailCode,
                            GroupId = item.GroupId,
                            Instgram = item.Instgram,
                            Joined = item.Joined,
                            LastEmailSent = item.LastEmailSent,
                            PageCategory = item.PageCategory,
                            PageDescription = item.PageDescription,
                            PageId = item.PageId,
                            PageName = item.PageName,
                            PageTitle = item.PageTitle,
                            Phone = item.Phone,
                            GroupName = item.GroupName,
                            ProTime = item.ProTime,
                            Rating = item.Rating,
                            Showlastseen = item.Showlastseen,
                            SidebarData = item.SidebarData,
                            SmsCode = item.SmsCode,
                            SocialLogin = item.SocialLogin,
                            Src = item.Src,
                            StartUp = item.StartUp,
                            StartupFollow = item.StartupFollow,
                            StartupImage = item.StartupImage,
                            StartUpInfo = item.StartUpInfo,
                            UserId = item.UserId,
                            Username = item.Username,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Avatar = item.Avatar,
                            Cover = item.Cover,
                            BackgroundImage = item.BackgroundImage,
                            RelationshipId = item.RelationshipId,
                            Address = item.Address,
                            Working = item.Working,
                            Gender = item.Gender,
                            Facebook = item.Facebook,
                            Google = item.Google,
                            Twitter = item.Twitter,
                            Linkedin = item.Linkedin,
                            Website = item.Website,
                            Instagram = item.Instagram,
                            WebDeviceId = item.WebDeviceId,
                            Language = item.Language,
                            IpAddress = item.IpAddress,
                            PhoneNumber = item.PhoneNumber,
                            Timezone = item.Timezone,
                            Lat = item.Lat,
                            Lng = item.Lng,
                            About = item.About,
                            Birthday = item.Birthday,
                            Registered = item.Registered,
                            Lastseen = item.Lastseen,
                            LastLocationUpdate = item.LastLocationUpdate,
                            Balance = item.Balance,
                            Verified = item.Verified,
                            Status = item.Status,
                            Active = item.Active,
                            Admin = item.Admin,
                            IsPro = item.IsPro,
                            ProType = item.ProType,
                            School = item.School,
                            Name = item.Name,
                            AndroidMDeviceId = item.AndroidMDeviceId,
                            ECommented = item.ECommented,
                            AndroidNDeviceId = item.AndroidMDeviceId,
                            AvatarFull = item.AvatarFull,
                            BirthPrivacy = item.BirthPrivacy,
                            CanFollow = item.CanFollow,
                            ConfirmFollowers = item.ConfirmFollowers,
                            CountryId = item.CountryId,
                            EAccepted = item.EAccepted,
                            EFollowed = item.EFollowed,
                            EJoinedGroup = item.EJoinedGroup,
                            ELastNotif = item.ELastNotif,
                            ELiked = item.ELiked,
                            ELikedPage = item.ELikedPage,
                            EMentioned = item.EMentioned,
                            EProfileWallPost = item.EProfileWallPost,
                            ESentmeMsg = item.ESentmeMsg,
                            EShared = item.EShared,
                            EVisited = item.EVisited,
                            EWondered = item.EWondered,
                            EmailNotification = item.EmailNotification,
                            FollowPrivacy = item.FollowPrivacy,
                            FriendPrivacy = item.FriendPrivacy,
                            GenderText = item.GenderText,
                            InfoFile = item.InfoFile,
                            IosMDeviceId = item.IosMDeviceId,
                            IosNDeviceId = item.IosNDeviceId,
                            IsBlocked = item.IsBlocked,
                            IsFollowing = item.IsFollowing,
                            IsFollowingMe = item.IsFollowingMe,
                            LastAvatarMod = item.LastAvatarMod,
                            LastCoverMod = item.LastCoverMod,
                            LastDataUpdate = item.LastDataUpdate,
                            LastFollowId = item.LastFollowId,
                            LastLoginData = item.LastLoginData,
                            LastseenStatus = item.LastseenStatus,
                            LastseenTimeText = item.LastseenTimeText,
                            LastseenUnixTime = item.LastseenUnixTime,
                            MessagePrivacy = item.MessagePrivacy,
                            NewEmail = item.NewEmail,
                            NewPhone = item.NewPhone,
                            NotificationsSound = item.NotificationsSound,
                            OrderPostsBy = item.OrderPostsBy,
                            PaypalEmail = item.PaypalEmail,
                            PostPrivacy = item.PostPrivacy,
                            Referrer = item.Referrer,
                            ShareMyData = item.ShareMyData,
                            ShareMyLocation = item.ShareMyLocation,
                            ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                            TwoFactor = item.TwoFactor,
                            TwoFactorVerified = item.TwoFactorVerified,
                            Url = item.Url,
                            VisitPrivacy = item.VisitPrivacy,
                            Vk = item.Vk,
                            Wallet = item.Wallet,
                            WorkingLink = item.WorkingLink,
                            Youtube = item.Youtube,
                            City = item.City,
                            State = item.State,
                            Zip = item.Zip,
                            Points = item.Points,
                            DailyPoints = item.DailyPoints,
                            PointDayExpire = item.PointDayExpire,
                            CashfreeSignature = item.CashfreeSignature,
                            IsAdmin = item.IsAdmin,
                            MemberId = item.MemberId,
                            ChatColor = item.ChatColor,
                            PaystackRef = item.PaystackRef,
                            RefUserId = item.RefUserId,
                            SchoolCompleted = item.SchoolCompleted,
                            Type = item.Type,
                            UserPlatform = item.UserPlatform,
                            WeatherUnit = item.WeatherUnit,
                            AvatarPostId = item.AvatarPostId,
                            CodeSent = item.CodeSent,
                            CoverPostId = item.CoverPostId,
                            Discord = item.Discord,
                            IsArchive = item.IsArchive,
                            IsMute = item.IsMute,
                            IsPin = item.IsPin,
                            IsReported = item.IsReported,
                            IsStoryMuted = item.IsStoryMuted,
                            Mailru = item.Mailru,
                            NotificationSettings = item.NotificationSettings,
                            IsNotifyStopped = item.IsNotifyStopped,
                            Qq = item.Qq,
                            StripeSessionId = item.StripeSessionId,
                            Time = item.Time,
                            TimeCodeSent = item.TimeCodeSent,
                            Wechat = item.Wechat,
                            AlbumData = item.AlbumData,
                            ChatId = item.ChatId,
                            IsPageOnwer = item.IsPageOnwer,
                            MessageCount = item.MessageCount,
                            Selected = item.Selected,
                            AamarpayTranId = item.AamarpayTranId,
                            Banned = item.Banned,
                            BannedReason = item.BannedReason,
                            CoinbaseCode = item.CoinbaseCode,
                            CoinbaseHash = item.CoinbaseHash,
                            CoinpaymentsTxnId = item.CoinpaymentsTxnId,
                            ConversationId = item.ConversationId,
                            ConvertedPoints = item.ConvertedPoints,
                            Credits = item.Credits,
                            CurrentlyWorking = item.CurrentlyWorking,
                            FortumoHash = item.FortumoHash,
                            GoogleSecret = item.GoogleSecret,
                            HaveMonetization = item.HaveMonetization,
                            IsOpenToWork = item.IsOpenToWork,
                            IsProvidingService = item.IsProvidingService,
                            IsReportedUser = item.IsReportedUser,
                            IsVerified = item.IsVerified,
                            Languages = item.Languages,
                            NgeniusRef = item.NgeniusRef,
                            Okru = item.Okru,
                            PageSubCategory = item.PageSubCategory,
                            Permission = item.Permission,
                            PhonePrivacy = item.PhonePrivacy,
                            ProRemainder = item.ProRemainder,
                            ProvidingService = item.ProvidingService,
                            SecurionpayKey = item.SecurionpayKey,
                            TwoFactorMethod = item.TwoFactorMethod,
                            Skills = item.Skills,
                            SubCategory = item.SubCategory,
                            TwoFactorHash = item.TwoFactorHash,
                            UsersPost = item.UsersPost,
                            YoomoneyHash = item.YoomoneyHash,
                            Owner = Convert.ToBoolean(item.Owner),
                            UserData = JsonConvert.SerializeObject(item.UserData),
                            LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass),
                            Parts = JsonConvert.SerializeObject(item.Parts),
                            Details = JsonConvert.SerializeObject(item.Details.DetailsClass),
                            ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass),
                            Mute = JsonConvert.SerializeObject(item.Mute),
                        };

                        list.Add(db);

                        var update = result.FirstOrDefault(a => a.ChatId == item.ChatId);
                        if (update != null)
                        {
                            update = db;
                            update.UserData = JsonConvert.SerializeObject(item.UserData);
                            update.LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass);
                            update.Parts = JsonConvert.SerializeObject(item.Parts);
                            update.Details = JsonConvert.SerializeObject(item.Details.DetailsClass);
                            update.ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass);

                            connection.Update(update);
                        }
                    }

                    if (list.Count <= 0) return;

                    connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                    if (newItemList.Count > 0)
                        connection.InsertAll(newItemList);

                    result = connection.Table<DataTables.ArchiveTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            connection.Delete(delete);

                    connection.Commit();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_Archive(chatList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To Users Table
        public ObservableCollection<ChatObject> Get_Archive_List()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return null;
                var select = connection.Table<DataTables.ArchiveTb>().ToList();
                if (select.Count > 0)
                {
                    var list = new List<ChatObject>();

                    foreach (DataTables.ArchiveTb item in select)
                    {
                        //Classes.LastChatArchive archiveObject = null;
                        ChatObject db = new ChatObject
                        {
                            Id = item.Id,
                            AvatarOrg = item.AvatarOrg,
                            BackgroundImageStatus = item.BackgroundImageStatus,
                            Boosted = item.Boosted,
                            CallActionType = item.CallActionType,
                            CallActionTypeUrl = item.CallActionTypeUrl,
                            Category = item.Category,
                            ChatTime = item.ChatTime,
                            ChatType = item.ChatType,
                            Company = item.Company,
                            CoverFull = item.CoverFull,
                            CoverOrg = item.CoverOrg,
                            CssFile = item.CssFile,
                            EmailCode = item.EmailCode,
                            GroupId = item.GroupId,
                            Instgram = item.Instgram,
                            Joined = item.Joined,
                            LastEmailSent = item.LastEmailSent,
                            PageCategory = item.PageCategory,
                            PageDescription = item.PageDescription,
                            PageId = item.PageId,
                            PageName = item.PageName,
                            PageTitle = item.PageTitle,
                            Phone = item.Phone,
                            GroupName = item.GroupName,
                            ProTime = item.ProTime,
                            Rating = item.Rating,
                            Showlastseen = item.Showlastseen,
                            SidebarData = item.SidebarData,
                            SmsCode = item.SmsCode,
                            SocialLogin = item.SocialLogin,
                            Src = item.Src,
                            StartUp = item.StartUp,
                            StartupFollow = item.StartupFollow,
                            StartupImage = item.StartupImage,
                            StartUpInfo = item.StartUpInfo,
                            UserId = item.UserId,
                            Username = item.Username,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Avatar = item.Avatar,
                            Cover = item.Cover,
                            BackgroundImage = item.BackgroundImage,
                            RelationshipId = item.RelationshipId,
                            Address = item.Address,
                            Working = item.Working,
                            Gender = item.Gender,
                            Facebook = item.Facebook,
                            Google = item.Google,
                            Twitter = item.Twitter,
                            Linkedin = item.Linkedin,
                            Website = item.Website,
                            Instagram = item.Instagram,
                            WebDeviceId = item.WebDeviceId,
                            Language = item.Language,
                            IpAddress = item.IpAddress,
                            PhoneNumber = item.PhoneNumber,
                            Timezone = item.Timezone,
                            Lat = item.Lat,
                            Lng = item.Lng,
                            About = item.About,
                            Birthday = item.Birthday,
                            Registered = item.Registered,
                            Lastseen = item.Lastseen,
                            LastLocationUpdate = item.LastLocationUpdate,
                            Balance = item.Balance,
                            Verified = item.Verified,
                            Status = item.Status,
                            Active = item.Active,
                            Admin = item.Admin,
                            IsPro = item.IsPro,
                            ProType = item.ProType,
                            School = item.School,
                            Name = item.Name,
                            AndroidMDeviceId = item.AndroidMDeviceId,
                            ECommented = item.ECommented,
                            AndroidNDeviceId = item.AndroidMDeviceId,
                            AvatarFull = item.AvatarFull,
                            BirthPrivacy = item.BirthPrivacy,
                            CanFollow = item.CanFollow,
                            ConfirmFollowers = item.ConfirmFollowers,
                            CountryId = item.CountryId,
                            EAccepted = item.EAccepted,
                            EFollowed = item.EFollowed,
                            EJoinedGroup = item.EJoinedGroup,
                            ELastNotif = item.ELastNotif,
                            ELiked = item.ELiked,
                            ELikedPage = item.ELikedPage,
                            EMentioned = item.EMentioned,
                            EProfileWallPost = item.EProfileWallPost,
                            ESentmeMsg = item.ESentmeMsg,
                            EShared = item.EShared,
                            EVisited = item.EVisited,
                            EWondered = item.EWondered,
                            EmailNotification = item.EmailNotification,
                            FollowPrivacy = item.FollowPrivacy,
                            FriendPrivacy = item.FriendPrivacy,
                            GenderText = item.GenderText,
                            InfoFile = item.InfoFile,
                            IosMDeviceId = item.IosMDeviceId,
                            IosNDeviceId = item.IosNDeviceId,
                            IsBlocked = item.IsBlocked,
                            IsFollowing = item.IsFollowing,
                            IsFollowingMe = item.IsFollowingMe,
                            LastAvatarMod = item.LastAvatarMod,
                            LastCoverMod = item.LastCoverMod,
                            LastDataUpdate = item.LastDataUpdate,
                            LastFollowId = item.LastFollowId,
                            LastLoginData = item.LastLoginData,
                            LastseenStatus = item.LastseenStatus,
                            LastseenTimeText = item.LastseenTimeText,
                            LastseenUnixTime = item.LastseenUnixTime,
                            MessagePrivacy = item.MessagePrivacy,
                            NewEmail = item.NewEmail,
                            NewPhone = item.NewPhone,
                            NotificationsSound = item.NotificationsSound,
                            OrderPostsBy = item.OrderPostsBy,
                            PaypalEmail = item.PaypalEmail,
                            PostPrivacy = item.PostPrivacy,
                            Referrer = item.Referrer,
                            ShareMyData = item.ShareMyData,
                            ShareMyLocation = item.ShareMyLocation,
                            ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                            TwoFactor = item.TwoFactor,
                            TwoFactorVerified = item.TwoFactorVerified,
                            Url = item.Url,
                            VisitPrivacy = item.VisitPrivacy,
                            Vk = item.Vk,
                            Wallet = item.Wallet,
                            WorkingLink = item.WorkingLink,
                            Youtube = item.Youtube,
                            City = item.City,
                            State = item.State,
                            Zip = item.Zip,
                            Points = item.Points,
                            DailyPoints = item.DailyPoints,
                            PointDayExpire = item.PointDayExpire,
                            CashfreeSignature = item.CashfreeSignature,
                            IsAdmin = item.IsAdmin,
                            MemberId = item.MemberId,
                            ChatColor = item.ChatColor,
                            PaystackRef = item.PaystackRef,
                            RefUserId = item.RefUserId,
                            SchoolCompleted = item.SchoolCompleted,
                            Type = item.Type,
                            UserPlatform = item.UserPlatform,
                            WeatherUnit = item.WeatherUnit,
                            AvatarPostId = item.AvatarPostId,
                            CodeSent = item.CodeSent,
                            CoverPostId = item.CoverPostId,
                            Discord = item.Discord,
                            IsArchive = item.IsArchive,
                            IsMute = item.IsMute,
                            IsPin = item.IsPin,
                            IsReported = item.IsReported,
                            IsStoryMuted = item.IsStoryMuted,
                            Mailru = item.Mailru,
                            NotificationSettings = item.NotificationSettings,
                            IsNotifyStopped = item.IsNotifyStopped,
                            Qq = item.Qq,
                            StripeSessionId = item.StripeSessionId,
                            Time = item.Time,
                            TimeCodeSent = item.TimeCodeSent,
                            Wechat = item.Wechat,
                            AlbumData = item.AlbumData,
                            ChatId = item.ChatId,
                            IsPageOnwer = item.IsPageOnwer,
                            MessageCount = item.MessageCount,
                            AamarpayTranId = item.AamarpayTranId,
                            Banned = item.Banned,
                            BannedReason = item.BannedReason,
                            CoinbaseCode = item.CoinbaseCode,
                            CoinbaseHash = item.CoinbaseHash,
                            CoinpaymentsTxnId = item.CoinpaymentsTxnId,
                            ConversationId = item.ConversationId,
                            ConvertedPoints = item.ConvertedPoints,
                            Credits = item.Credits,
                            CurrentlyWorking = item.CurrentlyWorking,
                            FortumoHash = item.FortumoHash,
                            GoogleSecret = item.GoogleSecret,
                            HaveMonetization = item.HaveMonetization,
                            IsOpenToWork = item.IsOpenToWork,
                            IsProvidingService = item.IsProvidingService,
                            IsReportedUser = item.IsReportedUser,
                            IsVerified = item.IsVerified,
                            Languages = item.Languages,
                            NgeniusRef = item.NgeniusRef,
                            Okru = item.Okru,
                            PageSubCategory = item.PageSubCategory,
                            Permission = item.Permission,
                            PhonePrivacy = item.PhonePrivacy,
                            ProRemainder = item.ProRemainder,
                            ProvidingService = item.ProvidingService,
                            SecurionpayKey = item.SecurionpayKey,
                            TwoFactorMethod = item.TwoFactorMethod,
                            Skills = item.Skills,
                            SubCategory = item.SubCategory,
                            TwoFactorHash = item.TwoFactorHash,
                            UsersPost = item.UsersPost,
                            YoomoneyHash = item.YoomoneyHash,
                            Selected = false,
                            UserData = new UserDataObject(),
                            LastMessage = new LastMessageUnion { LastMessageClass = new MessageData() },
                            Parts = new List<UserDataObject>(),
                            Details = new DetailsUnion(),
                            ApiNotificationSettings = new NotificationSettingsUnion(),
                            Mute = new Mute(),
                        };

                        if (!string.IsNullOrEmpty(item.UserData))
                            db.UserData = JsonConvert.DeserializeObject<UserDataObject>(item.UserData);

                        if (!string.IsNullOrEmpty(item.LastMessage))
                        {
                            var sss = JsonConvert.DeserializeObject<MessageData>(item.LastMessage);
                            if (sss != null)
                            {
                                db.LastMessage = new LastMessageUnion
                                {
                                    LastMessageClass = sss
                                };
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Parts))
                            db.Parts = JsonConvert.DeserializeObject<List<UserDataObject>>(item.Parts);

                        if (!string.IsNullOrEmpty(item.Mute))
                            db.Mute = JsonConvert.DeserializeObject<Mute>(item.Mute);

                        db = ChatTools.FilterDataLastChatNewV(db);

                        if (!string.IsNullOrEmpty(item.Details))
                        {
                            var sss = JsonConvert.DeserializeObject<Details>(item.Details);
                            if (sss != null)
                            {
                                db.Details = new DetailsUnion
                                {
                                    DetailsClass = sss
                                };
                            }
                        }

                        if (!string.IsNullOrEmpty(item.ApiNotificationSettings))
                        {
                            var sss = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings);
                            if (sss != null)
                            {
                                db.ApiNotificationSettings = new NotificationSettingsUnion
                                {
                                    NotificationSettingsClass = sss
                                };
                            }
                        }

                        if (db.IsPin)
                        {
                            list.Insert(0, db);
                        }
                        else
                        {
                            list.Add(db);
                        }
                    }

                    return new ObservableCollection<ChatObject>(list);
                }

                return new ObservableCollection<ChatObject>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_Archive_List();
                Methods.DisplayReportResultTrack(e);
                return new ObservableCollection<ChatObject>();
            }
        }

        //Insert Or Update data from Users Table
        public void Insert_Or_Update_one_Archive(ChatObject item)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                var chat = connection.Table<DataTables.ArchiveTb>().FirstOrDefault(c => c.ChatId == item.ChatId && c.ChatType == item.ChatType);
                if (chat != null)
                {
                    chat.Id = item.Id;
                    chat.AvatarOrg = item.AvatarOrg;
                    chat.BackgroundImageStatus = item.BackgroundImageStatus;
                    chat.Boosted = item.Boosted;
                    chat.CallActionType = item.CallActionType;
                    chat.CallActionTypeUrl = item.CallActionTypeUrl;
                    chat.Category = item.Category;
                    chat.ChatTime = item.ChatTime;
                    chat.ChatType = item.ChatType;
                    chat.Company = item.Company;
                    chat.CoverFull = item.CoverFull;
                    chat.CoverOrg = item.CoverOrg;
                    chat.CssFile = item.CssFile;
                    chat.EmailCode = item.EmailCode;
                    chat.GroupId = item.GroupId;
                    chat.Instgram = item.Instgram;
                    chat.Joined = item.Joined;
                    chat.LastEmailSent = item.LastEmailSent;
                    chat.PageCategory = item.PageCategory;
                    chat.PageDescription = item.PageDescription;
                    chat.PageId = item.PageId;
                    chat.PageName = item.PageName;
                    chat.PageTitle = item.PageTitle;
                    chat.Phone = item.Phone;
                    chat.GroupName = item.GroupName;
                    chat.ProTime = item.ProTime;
                    chat.Rating = item.Rating;
                    chat.Showlastseen = item.Showlastseen;
                    chat.SidebarData = item.SidebarData;
                    chat.SmsCode = item.SmsCode;
                    chat.SocialLogin = item.SocialLogin;
                    chat.Src = item.Src;
                    chat.StartUp = item.StartUp;
                    chat.StartupFollow = item.StartupFollow;
                    chat.StartupImage = item.StartupImage;
                    chat.StartUpInfo = item.StartUpInfo;
                    chat.UserId = item.UserId;
                    chat.Username = item.Username;
                    chat.Email = item.Email;
                    chat.FirstName = item.FirstName;
                    chat.LastName = item.LastName;
                    chat.Avatar = item.Avatar;
                    chat.Cover = item.Cover;
                    chat.BackgroundImage = item.BackgroundImage;
                    chat.RelationshipId = item.RelationshipId;
                    chat.Address = item.Address;
                    chat.Working = item.Working;
                    chat.Gender = item.Gender;
                    chat.Facebook = item.Facebook;
                    chat.Google = item.Google;
                    chat.Twitter = item.Twitter;
                    chat.Linkedin = item.Linkedin;
                    chat.Website = item.Website;
                    chat.Instagram = item.Instagram;
                    chat.WebDeviceId = item.WebDeviceId;
                    chat.Language = item.Language;
                    chat.IpAddress = item.IpAddress;
                    chat.PhoneNumber = item.PhoneNumber;
                    chat.Timezone = item.Timezone;
                    chat.Lat = item.Lat;
                    chat.Lng = item.Lng;
                    chat.About = item.About;
                    chat.Birthday = item.Birthday;
                    chat.Registered = item.Registered;
                    chat.Lastseen = item.Lastseen;
                    chat.LastLocationUpdate = item.LastLocationUpdate;
                    chat.Balance = item.Balance;
                    chat.Verified = item.Verified;
                    chat.Status = item.Status;
                    chat.Active = item.Active;
                    chat.Admin = item.Admin;
                    chat.IsPro = item.IsPro;
                    chat.ProType = item.ProType;
                    chat.School = item.School;
                    chat.Name = item.Name;
                    chat.AndroidMDeviceId = item.AndroidMDeviceId;
                    chat.ECommented = item.ECommented;
                    chat.AndroidNDeviceId = item.AndroidMDeviceId;
                    chat.AvatarFull = item.AvatarFull;
                    chat.BirthPrivacy = item.BirthPrivacy;
                    chat.CanFollow = item.CanFollow;
                    chat.ConfirmFollowers = item.ConfirmFollowers;
                    chat.CountryId = item.CountryId;
                    chat.EAccepted = item.EAccepted;
                    chat.EFollowed = item.EFollowed;
                    chat.EJoinedGroup = item.EJoinedGroup;
                    chat.ELastNotif = item.ELastNotif;
                    chat.ELiked = item.ELiked;
                    chat.ELikedPage = item.ELikedPage;
                    chat.EMentioned = item.EMentioned;
                    chat.EProfileWallPost = item.EProfileWallPost;
                    chat.ESentmeMsg = item.ESentmeMsg;
                    chat.EShared = item.EShared;
                    chat.EVisited = item.EVisited;
                    chat.EWondered = item.EWondered;
                    chat.EmailNotification = item.EmailNotification;
                    chat.FollowPrivacy = item.FollowPrivacy;
                    chat.FriendPrivacy = item.FriendPrivacy;
                    chat.GenderText = item.GenderText;
                    chat.InfoFile = item.InfoFile;
                    chat.IosMDeviceId = item.IosMDeviceId;
                    chat.IosNDeviceId = item.IosNDeviceId;
                    chat.IsBlocked = item.IsBlocked;
                    chat.IsFollowing = item.IsFollowing;
                    chat.IsFollowingMe = item.IsFollowingMe;
                    chat.LastAvatarMod = item.LastAvatarMod;
                    chat.LastCoverMod = item.LastCoverMod;
                    chat.LastDataUpdate = item.LastDataUpdate;
                    chat.LastFollowId = item.LastFollowId;
                    chat.LastLoginData = item.LastLoginData;
                    chat.LastseenStatus = item.LastseenStatus;
                    chat.LastseenTimeText = item.LastseenTimeText;
                    chat.LastseenUnixTime = item.LastseenUnixTime;
                    chat.MessagePrivacy = item.MessagePrivacy;
                    chat.NewEmail = item.NewEmail;
                    chat.NewPhone = item.NewPhone;
                    chat.NotificationsSound = item.NotificationsSound;
                    chat.OrderPostsBy = item.OrderPostsBy;
                    chat.PaypalEmail = item.PaypalEmail;
                    chat.PostPrivacy = item.PostPrivacy;
                    chat.Referrer = item.Referrer;
                    chat.ShareMyData = item.ShareMyData;
                    chat.ShareMyLocation = item.ShareMyLocation;
                    chat.ShowActivitiesPrivacy = item.ShowActivitiesPrivacy;
                    chat.TwoFactor = item.TwoFactor;
                    chat.TwoFactorVerified = item.TwoFactorVerified;
                    chat.Url = item.Url;
                    chat.VisitPrivacy = item.VisitPrivacy;
                    chat.Vk = item.Vk;
                    chat.Wallet = item.Wallet;
                    chat.WorkingLink = item.WorkingLink;
                    chat.Youtube = item.Youtube;
                    chat.City = item.City;
                    chat.State = item.State;
                    chat.Zip = item.Zip;
                    chat.Points = item.Points;
                    chat.DailyPoints = item.DailyPoints;
                    chat.PointDayExpire = item.PointDayExpire;
                    chat.CashfreeSignature = item.CashfreeSignature;
                    chat.IsAdmin = item.IsAdmin;
                    chat.MemberId = item.MemberId;
                    chat.ChatColor = item.ChatColor;
                    chat.PaystackRef = item.PaystackRef;
                    chat.RefUserId = item.RefUserId;
                    chat.SchoolCompleted = item.SchoolCompleted;
                    chat.Type = item.Type;
                    chat.UserPlatform = item.UserPlatform;
                    chat.WeatherUnit = item.WeatherUnit;
                    chat.AvatarPostId = item.AvatarPostId;
                    chat.CodeSent = item.CodeSent;
                    chat.CoverPostId = item.CoverPostId;
                    chat.Discord = item.Discord;
                    chat.IsArchive = item.IsArchive;
                    chat.IsMute = item.IsMute;
                    chat.IsPin = item.IsPin;
                    chat.IsReported = item.IsReported;
                    chat.IsStoryMuted = item.IsStoryMuted;
                    chat.Mailru = item.Mailru;
                    chat.NotificationSettings = item.NotificationSettings;
                    chat.IsNotifyStopped = item.IsNotifyStopped;
                    chat.Qq = item.Qq;
                    chat.StripeSessionId = item.StripeSessionId;
                    chat.Time = item.Time;
                    chat.TimeCodeSent = item.TimeCodeSent;
                    chat.Wechat = item.Wechat;
                    chat.AlbumData = item.AlbumData;
                    chat.ChatId = item.ChatId;
                    chat.IsPageOnwer = item.IsPageOnwer;
                    chat.MessageCount = item.MessageCount;
                    chat.Selected = item.Selected;
                    chat.AamarpayTranId = item.AamarpayTranId;
                    chat.Banned = item.Banned;
                    chat.BannedReason = item.BannedReason;
                    chat.CoinbaseCode = item.CoinbaseCode;
                    chat.CoinbaseHash = item.CoinbaseHash;
                    chat.CoinpaymentsTxnId = item.CoinpaymentsTxnId;
                    chat.ConversationId = item.ConversationId;
                    chat.ConvertedPoints = item.ConvertedPoints;
                    chat.Credits = item.Credits;
                    chat.CurrentlyWorking = item.CurrentlyWorking;
                    chat.FortumoHash = item.FortumoHash;
                    chat.GoogleSecret = item.GoogleSecret;
                    chat.HaveMonetization = item.HaveMonetization;
                    chat.IsOpenToWork = item.IsOpenToWork;
                    chat.IsProvidingService = item.IsProvidingService;
                    chat.IsReportedUser = item.IsReportedUser;
                    chat.IsVerified = item.IsVerified;
                    chat.Languages = item.Languages;
                    chat.NgeniusRef = item.NgeniusRef;
                    chat.Okru = item.Okru;
                    chat.PageSubCategory = item.PageSubCategory;
                    chat.Permission = item.Permission;
                    chat.PhonePrivacy = item.PhonePrivacy;
                    chat.ProRemainder = item.ProRemainder;
                    chat.ProvidingService = item.ProvidingService;
                    chat.SecurionpayKey = item.SecurionpayKey;
                    chat.TwoFactorMethod = item.TwoFactorMethod;
                    chat.Skills = item.Skills;
                    chat.SubCategory = item.SubCategory;
                    chat.TwoFactorHash = item.TwoFactorHash;
                    chat.UsersPost = item.UsersPost;
                    chat.YoomoneyHash = item.YoomoneyHash;
                    chat.Owner = Convert.ToBoolean(item.Owner);
                    chat.UserData = JsonConvert.SerializeObject(item.UserData);
                    chat.LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass);
                    chat.Parts = JsonConvert.SerializeObject(item.Parts);
                    chat.Details = JsonConvert.SerializeObject(item.Details.DetailsClass);
                    chat.ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass);
                    chat.Mute = JsonConvert.SerializeObject(item.Mute);

                    connection.Update(chat);
                }
                else
                {
                    DataTables.ArchiveTb db = new DataTables.ArchiveTb
                    {
                        Id = item.Id,
                        AvatarOrg = item.AvatarOrg,
                        BackgroundImageStatus = item.BackgroundImageStatus,
                        Boosted = item.Boosted,
                        CallActionType = item.CallActionType,
                        CallActionTypeUrl = item.CallActionTypeUrl,
                        Category = item.Category,
                        ChatTime = item.ChatTime,
                        ChatType = item.ChatType,
                        Company = item.Company,
                        CoverFull = item.CoverFull,
                        CoverOrg = item.CoverOrg,
                        CssFile = item.CssFile,
                        EmailCode = item.EmailCode,
                        GroupId = item.GroupId,
                        Instgram = item.Instgram,
                        Joined = item.Joined,
                        LastEmailSent = item.LastEmailSent,
                        PageCategory = item.PageCategory,
                        PageDescription = item.PageDescription,
                        PageId = item.PageId,
                        PageName = item.PageName,
                        PageTitle = item.PageTitle,
                        Phone = item.Phone,
                        GroupName = item.GroupName,
                        ProTime = item.ProTime,
                        Rating = item.Rating,
                        Showlastseen = item.Showlastseen,
                        SidebarData = item.SidebarData,
                        SmsCode = item.SmsCode,
                        SocialLogin = item.SocialLogin,
                        Src = item.Src,
                        StartUp = item.StartUp,
                        StartupFollow = item.StartupFollow,
                        StartupImage = item.StartupImage,
                        StartUpInfo = item.StartUpInfo,
                        UserId = item.UserId,
                        Username = item.Username,
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Avatar = item.Avatar,
                        Cover = item.Cover,
                        BackgroundImage = item.BackgroundImage,
                        RelationshipId = item.RelationshipId,
                        Address = item.Address,
                        Working = item.Working,
                        Gender = item.Gender,
                        Facebook = item.Facebook,
                        Google = item.Google,
                        Twitter = item.Twitter,
                        Linkedin = item.Linkedin,
                        Website = item.Website,
                        Instagram = item.Instagram,
                        WebDeviceId = item.WebDeviceId,
                        Language = item.Language,
                        IpAddress = item.IpAddress,
                        PhoneNumber = item.PhoneNumber,
                        Timezone = item.Timezone,
                        Lat = item.Lat,
                        Lng = item.Lng,
                        About = item.About,
                        Birthday = item.Birthday,
                        Registered = item.Registered,
                        Lastseen = item.Lastseen,
                        LastLocationUpdate = item.LastLocationUpdate,
                        Balance = item.Balance,
                        Verified = item.Verified,
                        Status = item.Status,
                        Active = item.Active,
                        Admin = item.Admin,
                        IsPro = item.IsPro,
                        ProType = item.ProType,
                        School = item.School,
                        Name = item.Name,
                        AndroidMDeviceId = item.AndroidMDeviceId,
                        ECommented = item.ECommented,
                        AndroidNDeviceId = item.AndroidMDeviceId,
                        AvatarFull = item.AvatarFull,
                        BirthPrivacy = item.BirthPrivacy,
                        CanFollow = item.CanFollow,
                        ConfirmFollowers = item.ConfirmFollowers,
                        CountryId = item.CountryId,
                        EAccepted = item.EAccepted,
                        EFollowed = item.EFollowed,
                        EJoinedGroup = item.EJoinedGroup,
                        ELastNotif = item.ELastNotif,
                        ELiked = item.ELiked,
                        ELikedPage = item.ELikedPage,
                        EMentioned = item.EMentioned,
                        EProfileWallPost = item.EProfileWallPost,
                        ESentmeMsg = item.ESentmeMsg,
                        EShared = item.EShared,
                        EVisited = item.EVisited,
                        EWondered = item.EWondered,
                        EmailNotification = item.EmailNotification,
                        FollowPrivacy = item.FollowPrivacy,
                        FriendPrivacy = item.FriendPrivacy,
                        GenderText = item.GenderText,
                        InfoFile = item.InfoFile,
                        IosMDeviceId = item.IosMDeviceId,
                        IosNDeviceId = item.IosNDeviceId,
                        IsBlocked = item.IsBlocked,
                        IsFollowing = item.IsFollowing,
                        IsFollowingMe = item.IsFollowingMe,
                        LastAvatarMod = item.LastAvatarMod,
                        LastCoverMod = item.LastCoverMod,
                        LastDataUpdate = item.LastDataUpdate,
                        LastFollowId = item.LastFollowId,
                        LastLoginData = item.LastLoginData,
                        LastseenStatus = item.LastseenStatus,
                        LastseenTimeText = item.LastseenTimeText,
                        LastseenUnixTime = item.LastseenUnixTime,
                        MessagePrivacy = item.MessagePrivacy,
                        NewEmail = item.NewEmail,
                        NewPhone = item.NewPhone,
                        NotificationsSound = item.NotificationsSound,
                        OrderPostsBy = item.OrderPostsBy,
                        PaypalEmail = item.PaypalEmail,
                        PostPrivacy = item.PostPrivacy,
                        Referrer = item.Referrer,
                        ShareMyData = item.ShareMyData,
                        ShareMyLocation = item.ShareMyLocation,
                        ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                        TwoFactor = item.TwoFactor,
                        TwoFactorVerified = item.TwoFactorVerified,
                        Url = item.Url,
                        VisitPrivacy = item.VisitPrivacy,
                        Vk = item.Vk,
                        Wallet = item.Wallet,
                        WorkingLink = item.WorkingLink,
                        Youtube = item.Youtube,
                        City = item.City,
                        State = item.State,
                        Zip = item.Zip,
                        Points = item.Points,
                        DailyPoints = item.DailyPoints,
                        PointDayExpire = item.PointDayExpire,
                        CashfreeSignature = item.CashfreeSignature,
                        IsAdmin = item.IsAdmin,
                        MemberId = item.MemberId,
                        ChatColor = item.ChatColor,
                        PaystackRef = item.PaystackRef,
                        RefUserId = item.RefUserId,
                        SchoolCompleted = item.SchoolCompleted,
                        Type = item.Type,
                        UserPlatform = item.UserPlatform,
                        WeatherUnit = item.WeatherUnit,
                        AvatarPostId = item.AvatarPostId,
                        CodeSent = item.CodeSent,
                        CoverPostId = item.CoverPostId,
                        Discord = item.Discord,
                        IsArchive = item.IsArchive,
                        IsMute = item.IsMute,
                        IsPin = item.IsPin,
                        IsReported = item.IsReported,
                        IsStoryMuted = item.IsStoryMuted,
                        Mailru = item.Mailru,
                        NotificationSettings = item.NotificationSettings,
                        IsNotifyStopped = item.IsNotifyStopped,
                        Qq = item.Qq,
                        StripeSessionId = item.StripeSessionId,
                        Time = item.Time,
                        TimeCodeSent = item.TimeCodeSent,
                        Wechat = item.Wechat,
                        AlbumData = item.AlbumData,
                        ChatId = item.ChatId,
                        IsPageOnwer = item.IsPageOnwer,
                        MessageCount = item.MessageCount,
                        Selected = item.Selected,
                        AamarpayTranId = item.AamarpayTranId,
                        Banned = item.Banned,
                        BannedReason = item.BannedReason,
                        CoinbaseCode = item.CoinbaseCode,
                        CoinbaseHash = item.CoinbaseHash,
                        CoinpaymentsTxnId = item.CoinpaymentsTxnId,
                        ConversationId = item.ConversationId,
                        ConvertedPoints = item.ConvertedPoints,
                        Credits = item.Credits,
                        CurrentlyWorking = item.CurrentlyWorking,
                        FortumoHash = item.FortumoHash,
                        GoogleSecret = item.GoogleSecret,
                        HaveMonetization = item.HaveMonetization,
                        IsOpenToWork = item.IsOpenToWork,
                        IsProvidingService = item.IsProvidingService,
                        IsReportedUser = item.IsReportedUser,
                        IsVerified = item.IsVerified,
                        Languages = item.Languages,
                        NgeniusRef = item.NgeniusRef,
                        Okru = item.Okru,
                        PageSubCategory = item.PageSubCategory,
                        Permission = item.Permission,
                        PhonePrivacy = item.PhonePrivacy,
                        ProRemainder = item.ProRemainder,
                        ProvidingService = item.ProvidingService,
                        SecurionpayKey = item.SecurionpayKey,
                        TwoFactorMethod = item.TwoFactorMethod,
                        Skills = item.Skills,
                        SubCategory = item.SubCategory,
                        TwoFactorHash = item.TwoFactorHash,
                        UsersPost = item.UsersPost,
                        YoomoneyHash = item.YoomoneyHash,
                        Owner = Convert.ToBoolean(item.Owner),
                        UserData = JsonConvert.SerializeObject(item.UserData),
                        LastMessage = JsonConvert.SerializeObject(item.LastMessage.LastMessageClass),
                        Parts = JsonConvert.SerializeObject(item.Parts),
                        Details = JsonConvert.SerializeObject(item.Details.DetailsClass),
                        ApiNotificationSettings = JsonConvert.SerializeObject(item.ApiNotificationSettings.NotificationSettingsClass),
                        Mute = JsonConvert.SerializeObject(item.Mute),
                    };
                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_one_Archive(item);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove data from Users Table
        public void Delete_Archive(string id)
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;

                var user = connection.Table<DataTables.ArchiveTb>().FirstOrDefault(c => c.ChatId == id);
                if (user != null)
                    connection.Delete(user);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_Archive(id);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove All data To Users Table
        public void ClearAll_Archive()
        {
            try
            {
                using var connection = OpenConnection();
                if (connection == null) return;
                connection.DeleteAll<DataTables.ArchiveTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_Archive();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}