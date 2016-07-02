using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace tk2dEditor.TileMap
{
	public class Importer
	{
		// Dynamically resolved types
		System.Type zlibType = null;

		// Format enums
		public enum Format 
		{
			TMX,
		};
		
		// Local tile map copy
		int width, height;
		class LayerProxy
		{
			public string name;
			public uint[] tiles;
		};
		List<LayerProxy> layers = new List<LayerProxy>();

		bool staggered = false;
		
		// Constructor - attempt resolving types
		private Importer() 
		{
			// Find all required modules
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					foreach (var module in assembly.GetModules())
					{
						if (module.ScopeName == "zlib.net.dll")
						{
							zlibType = module.GetType("zlib.ZOutputStream");
							break;
						}
					}
				}
				catch { }
			}
		}

		// Xml helpers
		static int ReadIntAttribute(XmlNode node, string attribute) { return int.Parse(node.Attributes[attribute].Value, System.Globalization.NumberFormatInfo.InvariantInfo); }
		static string ReadStringAttribute(XmlNode node, string attribute, string defValue) {
			var val = node.Attributes[attribute];
			return (val == null) ? defValue : val.Value;
		}
		
		const string FormatErrorString = "Unsupported format error.\n" + 
		"Please ensure layer data is stored as xml, base64(zlib) * or base64(uncompressed) in TileD preferences.\n\n" + 
		"* - Preferred format";

		// Import TMX
		string ImportTMX(string path)
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				var mapNode = doc.SelectSingleNode("/map");
				width = ReadIntAttribute(mapNode, "width");
				height = ReadIntAttribute(mapNode, "height");
				string orientation = ReadStringAttribute(mapNode, "orientation", "orthogonal");

				if (orientation != "orthogonal" && orientation != "staggered") {
					throw new System.Exception("ImportTMX only supports orthogonal and staggered tilemaps.\n\n\n");
				}
				staggered = orientation == "staggered";

				// var tileSetNodes = mapNode.SelectNodes("tileset");
				// if (tileSetNodes.Count > 1) return "Only one tileset supported"; // just ignore this
				
				var layersNodes = mapNode.SelectNodes("layer");
				foreach (XmlNode layerNode in layersNodes)
				{
					string name = layerNode.Attributes["name"].Value;
					int layerWidth = ReadIntAttribute(layerNode, "width");
					int layerHeight = ReadIntAttribute(layerNode, "height");
					if (layerHeight != height || layerWidth != width) return "Layer \"" + name + "\" has invalid dimensions";
					
					var dataNode = layerNode.SelectSingleNode("data");
					string encoding = (dataNode.Attributes["encoding"] != null)?dataNode.Attributes["encoding"].Value:"";
					string compression = (dataNode.Attributes["compression"] != null)?dataNode.Attributes["compression"].Value:"";

					uint[] data = null;
					if (encoding == "base64")
					{
						if (compression == "zlib")
						{
							data = BytesToInts(ZlibInflate(System.Convert.FromBase64String(dataNode.InnerText)));
						}
						else if (compression == "")
						{
							data = BytesToInts(System.Convert.FromBase64String(dataNode.InnerText));
						}
						else return FormatErrorString;
					}
					else if (encoding == "")
					{
						List<uint> values = new List<uint>();
						var tileNodes = dataNode.SelectNodes("tile");
						foreach (XmlNode tileNode in tileNodes)
							values.Add( uint.Parse(tileNode.Attributes["gid"].Value, System.Globalization.NumberFormatInfo.InvariantInfo) );
						data = values.ToArray();
					}
					else
					{
						return FormatErrorString;
					}

					if (data != null)
					{
						var layerProxy = new LayerProxy();
						layerProxy.name = name;
						layerProxy.tiles = data;
						layers.Add(layerProxy);
					}
				}
			}
			catch (System.Exception e) { return e.ToString(); }
			return "";
		}
		
		// Zlib helper
		byte[] ZlibInflate(byte[] data)
		{
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			var obj = System.Activator.CreateInstance(zlibType, ms);
			var invokeFlags = System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod;
			zlibType.InvokeMember("Write", 
				invokeFlags,
				null,
				obj,
				new object[] { data, 0, data.Length });
			byte[] bytes = ms.ToArray();
			zlibType.InvokeMember("Close", invokeFlags, null, obj, null);
			return bytes;
		}
		
		// Read little endian ints from byte array
		uint[] BytesToInts(byte[] bytes)
		{
			uint[] ints = new uint[bytes.Length / 4];
			for (int i = 0, j = 0; i < ints.Length; ++i, j += 4)
			{
				ints[i] = (uint)bytes[j] | ((uint)bytes[j+1] << 8) | ((uint)bytes[j+2] << 16) | ((uint)bytes[j+3] << 24);
			}
			return ints;
		}
		
		void PopulateTilemap(tk2dTileMap tileMap)
		{
			int extraWidth = staggered ? 1 : 0;
			tk2dEditor.TileMap.TileMapUtility.ResizeTileMap(tileMap, width + extraWidth, height, tileMap.partitionSizeX, tileMap.partitionSizeY);

			if (staggered) {
				tileMap.data.sortMethod = tk2dTileMapData.SortMethod.TopLeft;
				tileMap.data.tileType = tk2dTileMapData.TileType.Isometric;
			}

			foreach (var layer in layers)
			{
				int index = tk2dEditor.TileMap.TileMapUtility.FindOrCreateLayer(tileMap, layer.name);
				var target = tileMap.Layers[index];

				int ww = width + extraWidth;
				for (int y = 0; y < height; ++y) {
					for (int x = 0; x < ww; ++x) {
						target.SetTile(x, y, -1);
					}
				}


				for (int y = 0; y < height; ++y) {
					for (int x = 0; x < width; ++x) {
						uint rawTmxTile = layer.tiles[y * width + x]; 

						// Set tile
						int tile = (int)(rawTmxTile & ~(0xE0000000)) - 1; // ignore flipping and rotating
						int offset = (!staggered || (staggered && ((y % 2) == 0))) ? 0 : 1;
						target.SetTile(x + offset, height - 1 - y, tile);

						// Set tile flags
						bool flipHorizontal = (rawTmxTile & 0x80000000) != 0;
						bool flipVertical = (rawTmxTile & 0x40000000) != 0;
						bool flipDiagonal = (rawTmxTile & 0x20000000) != 0;
						tk2dTileFlags tileFlags = 0;
						if (flipDiagonal) tileFlags |= (tk2dTileFlags.Rot90 | tk2dTileFlags.FlipX);
						if (flipHorizontal) tileFlags ^= tk2dTileFlags.FlipX;
						if (flipVertical) tileFlags ^= tk2dTileFlags.FlipY;
						target.SetTileFlags(x + offset, height - 1 - y, tileFlags);
					}
				}
				target.Optimize();
			}
		}
		
	#region Static and helper functions
		////////////////////////////////////////////////////////////////////////////////////////////////
		/// Static and helper functions
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool Import(tk2dTileMap tileMap, Format format)
		{
			var importer = new Importer();
			
			string ext = "";
			switch (format)
			{
				case Format.TMX: 
					if (!importer.CheckZlib()) return false;
					ext = "tmx"; 
				break;
			}
			
			string path = EditorUtility.OpenFilePanel("Import tilemap", "", ext);
			if (path.Length == 0)
				return false;
			
			string message = "";
			switch (format)
			{
			case Format.TMX: message = importer.ImportTMX(path); break;
			}
			
			if (message.Length != 0)
			{
				EditorUtility.DisplayDialog("Tilemap failed to import", message, "Ok");
				return false;
			}
			
			importer.PopulateTilemap(tileMap);
			return true;
		}
		
		// Check and handle required modules
		bool CheckZlib()
		{
			if (zlibType == null)
			{
				if (EditorUtility.DisplayDialog("Unable to load required module zlib.net",
					"You can get zlib.net by clicking \"Download\" button.\n\n" +
					"You can also manually get it from http://www.componentace.com/zlib_.NET.htm, and copy the zip file into your Assets folder", 
					"Download", "Cancel"))
				{
					Application.OpenURL("http://www.2dtoolkit.com/external/zlib/");
				}
				return false;
			}
			return true;
		}
	#endregion
	}
}
