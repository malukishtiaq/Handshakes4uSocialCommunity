﻿using System.Timers;
using Android.Media;
using WoWonderClient.Classes.Message;

namespace WoWonder.Helpers.Chat
{
    public class MessageDataExtra : MessageData
    {
        public new MediaPlayer MediaPlayer { get; set; }
        public new Timer MediaTimer { get; set; }
    }

    public class AdapterModelsClassMessage
    {
        public long Id { get; set; }
        public MessageModelType TypeView { get; set; }
        public MessageDataExtra MesData { get; set; }
    }
}