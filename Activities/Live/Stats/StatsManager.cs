﻿using System.Collections.Generic;
using IO.Agora.Rtc2;

namespace WoWonder.Activities.Live.Stats
{
    public class StatsManager
    {
        private readonly List<int> MUidList = new List<int>();
        private readonly Dictionary<int, StatsData> MDataMap = new Dictionary<int, StatsData>();
        private bool MEnable;

        public void AddUserStats(int uid, bool ifLocal)
        {
            if (MUidList.Contains(uid) && MDataMap.ContainsKey(uid))
            {
                return;
            }

            var data = ifLocal
                ? (StatsData)new LocalStatsData()
                : new RemoteStatsData();
            // in case 32-bit unsigned integer uid is received
            data.SetUid(uid & 0xFFFFFFFFL);

            switch (ifLocal)
            {
                case true:
                    MUidList.Add(uid);
                    break;
                default:
                    MUidList.Add(uid);
                    break;
            }

            MDataMap.Add(uid, data);
        }

        public void RemoveUserStats(int uid)
        {
            if (MUidList.Contains(uid) && MDataMap.ContainsKey(uid))
            {
                MUidList.Remove(uid);
                MDataMap.Remove(uid);
            }
        }

        public StatsData GetStatsData(int uid)
        {
            if (MUidList.Contains(uid) && MDataMap.ContainsKey(uid))
            {
                return MDataMap[uid];
            }

            return null!;
        }

        public string QualityToString(int quality)
        {
            return quality switch
            {
                Constants.QualityExcellent => "Exc",
                Constants.QualityGood => "Good",
                Constants.QualityPoor => "Poor",
                Constants.QualityBad => "Bad",
                Constants.QualityVbad => "VBad",
                Constants.QualityDown => "Down",
                _ => "Unk"
            };
        }

        public void EnableStats(bool enabled)
        {
            MEnable = enabled;
        }

        public bool IsEnabled()
        {
            return MEnable;
        }

        public void ClearAllData()
        {
            MUidList.Clear();
            MDataMap.Clear();
        }
    }

}