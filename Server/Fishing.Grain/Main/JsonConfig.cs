using System;
using System.IO;
using System.Collections.Generic;
using Ps;

public class JsonConfig
{
    public List<JsonPacket> json_packet_list;
    public List<RouteJsonPacket> route_json_packet_list;

    public JsonConfig()
    {
        string path_fishlord = Path.Combine(ServerPath.getPathMediaRoot(), "Fishing\\FishLord\\");
        string path_route = Path.Combine(ServerPath.getPathMediaRoot(), "Fishing\\Route\\");
        json_packet_list = _readJsonPacketListFormFile(path_fishlord);
        route_json_packet_list = _readRouteJsonPacketListFormFile(path_route);
    }

    string _getFromFile(string file_name)
    {
        using (StreamReader file_reader = new StreamReader(file_name))
        {
            return file_reader.ReadToEnd();
        }
    }

    JsonPacket _getJsonPacketFromFile(string file_path_name, string file_name)
    {
        return new JsonPacket(file_name, _getFromFile(file_path_name + file_name));
    }

    List<JsonPacket> _readJsonPacketListFormFile(string file_directory)
    {
        List<JsonPacket> json_packet_list = new List<JsonPacket>();

        StringReader fish_lord_configure = new StringReader(_getFromFile(file_directory + "FishLordConfigure.con"));

        List<string> file_name_list = new List<string>();

        string configure_line = null;
        configure_line = fish_lord_configure.ReadLine();
        while (configure_line != null)
        {
            json_packet_list.Add(_getJsonPacketFromFile(file_directory, configure_line.Trim()));
            configure_line = fish_lord_configure.ReadLine();
        }

        return json_packet_list;
    }

    RouteJsonPacket _getRouteJsonPacketFromFile(string file_path_name, string file_name)
    {
        return new RouteJsonPacket(file_name, _getFromFile(file_path_name + file_name));
    }

    List<RouteJsonPacket> _readRouteJsonPacketListFormFile(string file_directory)
    {
        List<RouteJsonPacket> json_packet_list = new List<RouteJsonPacket>();

        StringReader fish_lord_configure = new StringReader(_getFromFile(file_directory + "RouteConfigure.con"));

        List<string> file_name_list = new List<string>();

        string configure_line = null;
        configure_line = fish_lord_configure.ReadLine();
        while (configure_line != null)
        {
            json_packet_list.Add(_getRouteJsonPacketFromFile(file_directory, configure_line.Trim()));
            configure_line = fish_lord_configure.ReadLine();
        }

        return json_packet_list;
    }
}
