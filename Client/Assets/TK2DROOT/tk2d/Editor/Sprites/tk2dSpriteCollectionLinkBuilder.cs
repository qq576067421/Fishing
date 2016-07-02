using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace tk2dEditor.SpriteCollectionBuilder
{
	public static class LinkBuilder
	{
		public static void ValidateLinkedSpriteCollection(tk2dSpriteCollection gen) {
			if (gen.linkParent == null) {
				return;
			}

			if (gen.textureParams.Length != gen.linkParent.textureParams.Length) {
				Debug.LogError("Linked sprite collection mismatch. Please rebuild source collection");
				gen.linkParent = null;
			}
		}

		public static void ValidateTextureParam(tk2dSpriteCollection gen, int i) {
			var param = gen.textureParams[i];
			if (param.texture != null && gen.linkParent != null) {
				if (gen.linkParent.textureParams[i].texture == null ||
					gen.linkParent.textureParams[i].texture.width != param.texture.width ||
					gen.linkParent.textureParams[i].texture.height != param.texture.height) {
					Debug.LogError("Linked sprite collection mismatch " + param.texture.name);
				}
			}			
		}

		public static void Build(tk2dSpriteCollection data) {
			if (data.linkedSpriteCollections.Count > 0 && !data.disableTrimming) {
				return;
			}

			string errors = "";
			int errorCount = 0;
			string root = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(data)) + "/Linked";
			foreach (tk2dLinkedSpriteCollection link in data.linkedSpriteCollections) {
				if (link.spriteCollection == null) {
					if (!System.IO.Directory.Exists(root)) {
						System.IO.Directory.CreateDirectory(root);
					}
					link.spriteCollection = tk2dSpriteCollectionEditor.CreateSpriteCollection(root, data.name + link.name);
				}

				tk2dEditor.SpriteCollectionEditor.SpriteCollectionProxy proxy = new tk2dEditor.SpriteCollectionEditor.SpriteCollectionProxy(data, false);
				proxy.CopyBuiltFromSource(link.spriteCollection);
				proxy.linkedSpriteCollections.Clear(); // stop recursion
				string thisErrors = "";

				foreach (tk2dSpriteCollectionDefinition tp in proxy.textureParams) {
					if (tp.texture != null) {
						Texture2D repl = FindReplacementTexture(tp.texture, link.name);
						if (repl == null) {
							thisErrors += string.Format("Unable to find replacement for texture '{0}' for link '{1}'\n", tp.texture.name, link.name);
							++errorCount;
						}
						tp.texture = repl;
					}
				}

				if (thisErrors.Length == 0) {
					proxy.CopyToTarget(link.spriteCollection);
					link.spriteCollection.linkParent = data;
					tk2dUtil.SetDirty(link.spriteCollection);

					tk2dSpriteCollectionBuilder.Rebuild(link.spriteCollection);
				}
				else {
					errors += thisErrors;
				}
			}

			if (errors.Length > 0) {
				Debug.LogError("There were " + errorCount.ToString() + " errors building the sprite collection\n" + errors);
			}
		}

		static Texture2D FindReplacementTexture(Texture2D tex, string name) {
			string path = AssetDatabase.GetAssetPath(tex);
			string dir = System.IO.Path.GetDirectoryName(path);
			string fname = System.IO.Path.GetFileNameWithoutExtension(path);
			int plat = fname.LastIndexOf('@');
			if (plat == -1) {
				fname = fname + name;
			}
			else {
				fname = fname.Insert(plat, name);
			}
			string ext = System.IO.Path.GetExtension(path);

			path = dir + "/" + fname + ext;
			Texture2D r = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;

			return r;
		}
	}
}