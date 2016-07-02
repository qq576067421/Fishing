using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class RenderFileReader
    {
        //-------------------------------------------------------------------------
        string mFileName = string.Empty;

        //-------------------------------------------------------------------------
        public RenderFileReader(string file_name)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            mFileName = Application.dataPath + "/StreamingAssets/" + file_name;
#elif UNITY_IPHONE
            mFileName = Application.dataPath + "/Raw/" + file_name;
#elif UNITY_ANDROID
            mFileName = "jar:file://" + Application.dataPath + "!/assets/" + file_name;
#endif
        }

        //-------------------------------------------------------------------------
        public string ReadToEnd()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return readOnWindows();
#elif UNITY_IPHONE
            return readOnIPHONE();
#elif UNITY_ANDROID
            return readOnANDROID();
#endif
        }

        //-------------------------------------------------------------------------
        string readOnWindows()
        {
            using (StreamReader file_reader = new StreamReader(mFileName))
            {
                return file_reader.ReadToEnd();
            }
        }

        //-------------------------------------------------------------------------
        string readOnIPHONE()
        {
            using (StreamReader file_reader = new StreamReader(mFileName))
            {
                return file_reader.ReadToEnd();
            }
        }

        //-------------------------------------------------------------------------
        string readOnANDROID()
        {
            WWW www = new WWW(mFileName);
            while (!www.isDone) { }
            return new StreamReader(new MemoryStream(www.bytes)).ReadToEnd();
        }
    }

    public class JsonPacketReader
    {
        //-------------------------------------------------------------------------
        string mFileDirectory = null;
        public JsonPacketReader(string file_directory)
        {
            mFileDirectory = file_directory;
        }

        //---------------------------------------------------------------------
        public List<JsonPacket> readJsonPacketList()
        {
            List<JsonPacket> json_packet_list = new List<JsonPacket>();

            StringReader fish_lord_configure = new StringReader(_getFromFile("FishLordConfigure.con"));

            List<string> file_name_list = new List<string>();

            string configure_line = null;
            configure_line = fish_lord_configure.ReadLine();
            while (configure_line != null)
            {
                json_packet_list.Add(_getJsonPacketFromFile(configure_line.Trim()));
                configure_line = fish_lord_configure.ReadLine();
            }

            return json_packet_list;
        }

        //---------------------------------------------------------------------
        public List<RouteJsonPacket> readRouteJsonPacketList()
        {
            List<RouteJsonPacket> json_packet_list = new List<RouteJsonPacket>();

            StringReader fish_lord_configure = new StringReader(_getFromFile("RouteConfigure.con"));

            List<string> file_name_list = new List<string>();

            string configure_line = null;
            configure_line = fish_lord_configure.ReadLine();
            while (configure_line != null)
            {
                json_packet_list.Add(_getRouteJsonPacketFromFile(configure_line.Trim()));
                configure_line = fish_lord_configure.ReadLine();
            }

            return json_packet_list;
        }

        //---------------------------------------------------------------------
        string _getFromFile(string file_name)
        {
            return new RenderFileReader(mFileDirectory + file_name).ReadToEnd();
        }

        //---------------------------------------------------------------------
        JsonPacket _getJsonPacketFromFile(string file_name)
        {
            return new JsonPacket(file_name, _getFromFile(file_name));
        }

        //---------------------------------------------------------------------
        RouteJsonPacket _getRouteJsonPacketFromFile(string file_name)
        {
            return new RouteJsonPacket(file_name, _getFromFile(file_name));
        }
    }
}
