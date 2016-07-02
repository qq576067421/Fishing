using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientSound<TDef> : Component<TDef> where TDef : DefSound, new()
    {
        //-------------------------------------------------------------------------
        public override void init()
        {
            EbLog.Note("ClientSound.init()");

            //SoundManager.SetVolumeMusic(1f);
            //SoundManager.SetCrossDuration(0f);
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            EbLog.Note("ClientSound.release()");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
        }

        //-------------------------------------------------------------------------
        public void playBackgroundSound(string snd_name, bool is_loop = false)
        {
            if (string.IsNullOrEmpty(snd_name))
            {
                EbLog.Note("ClientSound.playBackgroundSound() 声音名称为空");
            }

            string full_snd_name = "Audio/" + snd_name;
            AudioClip audio_clip = (AudioClip)Resources.Load(full_snd_name, typeof(AudioClip));
            //SoundManager.Play(audio_clip, is_loop);
        }
    }
}
