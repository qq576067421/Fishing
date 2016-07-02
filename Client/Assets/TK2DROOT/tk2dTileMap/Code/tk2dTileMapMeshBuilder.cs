using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR || !UNITY_FLASH

namespace tk2dRuntime.TileMap
{
	public static class RenderMeshBuilder
	{
		public static void BuildForChunk(tk2dTileMap tileMap, SpriteChunk chunk, ColorChunk colorChunk, bool useColor, bool skipPrefabs, int baseX, int baseY)
		{
			List<Vector3> meshVertices = new List<Vector3>();
			List<Color> meshColors = new List<Color>();
			List<Vector2> meshUvs = new List<Vector2>();
			List<Vector2> meshUv2s = new List<Vector2>();
			//List<int> meshIndices = new List<int>();
			
			int[] spriteIds = chunk.spriteIds;
			Vector3 tileSize = tileMap.data.tileSize;
			int spriteCount = tileMap.SpriteCollectionInst.spriteDefinitions.Length;
			Object[] tilePrefabs = tileMap.data.tilePrefabs;
			tk2dSpriteDefinition firstSprite = tileMap.SpriteCollectionInst.FirstValidDefinition;
			bool buildNormals = (firstSprite != null && firstSprite.normals != null && firstSprite.normals.Length > 0);

			bool generateUv2 = tileMap.data.generateUv2;
			var colorMode = tileMap.data.colorMode;
			
			Color32 clearColor = (useColor && tileMap.ColorChannel != null)?tileMap.ColorChannel.clearColor:Color.white;
					
			// revert to no color mode (i.e. fill with clear color) when there isn't a color channel, or it is empty
			if (colorChunk == null || colorChunk.colors.Length == 0)
				useColor = false;
			
			int x0, x1, dx;
			int y0, y1, dy;
			BuilderUtil.GetLoopOrder(tileMap.data.sortMethod, 
				tileMap.partitionSizeX, tileMap.partitionSizeY, 
				out x0, out x1, out dx,
				out y0, out y1, out dy);
			
			float xOffsetMult = 0.0f, yOffsetMult = 0.0f;
			tileMap.data.GetTileOffset(out xOffsetMult, out yOffsetMult);
			
			List<int>[] meshIndices = new List<int>[tileMap.SpriteCollectionInst.materials.Length];
			for (int j = 0; j < meshIndices.Length; ++j)
				meshIndices[j] = new List<int>();
			
			int colorChunkSize = tileMap.partitionSizeX + 1;
			for (int y = y0; y != y1; y += dy)
			{
				float xOffset = ((baseY + y) & 1) * xOffsetMult;
				for (int x = x0; x != x1; x += dx)
				{
					int spriteId = spriteIds[y * tileMap.partitionSizeX + x];
					int tile = BuilderUtil.GetTileFromRawTile(spriteId);
					bool flipH = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.FlipX);
					bool flipV = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.FlipY);
					bool rot90 = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.Rot90);

					Vector3 currentPos = new Vector3(tileSize.x * (x + xOffset), tileSize.y * y, 0);
	
					if (tile < 0 || tile >= spriteCount) 
						continue;
					
					if (skipPrefabs && tilePrefabs[tile])
						continue;
					
					var sprite = tileMap.SpriteCollectionInst.spriteDefinitions[tile];
					
					int baseVertex = meshVertices.Count;
					for (int v = 0; v < sprite.positions.Length; ++v)
					{
						Vector3 flippedPos = BuilderUtil.ApplySpriteVertexTileFlags(tileMap, sprite, sprite.positions[v], flipH, flipV, rot90);

						if (useColor && colorChunk != null)
						{
							Color tileColorx0y0 = colorChunk.colors[y * colorChunkSize + x];
							Color tileColorx1y0 = colorChunk.colors[y * colorChunkSize + x + 1];
							Color tileColorx0y1 = colorChunk.colors[(y + 1) * colorChunkSize + x];
							Color tileColorx1y1 = colorChunk.colors[(y + 1) * colorChunkSize + (x + 1)];

							switch (colorMode)
							{
								case tk2dTileMapData.ColorMode.Interpolate:
								{
									Vector3 centeredSpriteVertex = flippedPos - sprite.untrimmedBoundsData[0];
									Vector3 alignedSpriteVertex = centeredSpriteVertex + tileMap.data.tileSize * 0.5f;
									float tileColorX = Mathf.Clamp01(alignedSpriteVertex.x / tileMap.data.tileSize.x);
									float tileColorY = Mathf.Clamp01(alignedSpriteVertex.y / tileMap.data.tileSize.y);
									
									Color color = Color.Lerp(
												  Color.Lerp(tileColorx0y0, tileColorx1y0, tileColorX),
												  Color.Lerp(tileColorx0y1, tileColorx1y1, tileColorX),
												  tileColorY);
									meshColors.Add(color);
									break;
								}

								case tk2dTileMapData.ColorMode.Solid:
								{
									meshColors.Add(tileColorx0y0);
									break;
								}
							}
						}
						else
						{
							meshColors.Add(clearColor);
						}

						if (generateUv2)
						{
							if (sprite.normalizedUvs.Length == 0)
							{
								meshUv2s.Add(Vector2.zero);
							}
							else
							{
								meshUv2s.Add(sprite.normalizedUvs[v]);
							}
						}

						meshVertices.Add(currentPos + flippedPos);
						meshUvs.Add(sprite.uvs[v]);
					}

					bool reverseIndices = false; // flipped?
					if (flipH) reverseIndices = !reverseIndices;
					if (flipV) reverseIndices = !reverseIndices;
					
					List<int> indices = meshIndices[sprite.materialId];
					for (int i = 0; i < sprite.indices.Length; ++i) {
						int j = reverseIndices ? (sprite.indices.Length - 1 - i) : i;
						indices.Add(baseVertex + sprite.indices[j]);
					}
					
				}
			}
			
			if (chunk.mesh == null)
				chunk.mesh = tk2dUtil.CreateMesh();

			chunk.mesh.Clear();

			chunk.mesh.vertices = meshVertices.ToArray();
			chunk.mesh.uv = meshUvs.ToArray();
			if (generateUv2)
			{
				chunk.mesh.uv2 = meshUv2s.ToArray();
			}
			chunk.mesh.colors = meshColors.ToArray();

			List<Material> materials = new List<Material>();
			int materialId = 0;
			int subMeshCount = 0;
			foreach (var indices in meshIndices)
			{
				if (indices.Count > 0)
				{
					materials.Add(tileMap.SpriteCollectionInst.materialInsts[materialId]);
					subMeshCount++;
				}
				materialId++;
			}
			if (subMeshCount > 0)
			{
				chunk.mesh.subMeshCount = subMeshCount;
				chunk.gameObject.GetComponent<Renderer>().materials = materials.ToArray();
				int subMeshId = 0;
				foreach (var indices in meshIndices)
				{
					if (indices.Count > 0)
					{
						chunk.mesh.SetTriangles(indices.ToArray(), subMeshId);
						subMeshId++;
					}
				}
			}
			
			chunk.mesh.RecalculateBounds();
			if (buildNormals) {
				chunk.mesh.RecalculateNormals();
			}

			var meshFilter = chunk.gameObject.GetComponent<MeshFilter>();
			meshFilter.sharedMesh = chunk.mesh;
		}

		public static void Build(tk2dTileMap tileMap, bool editMode, bool forceBuild)
		{
			bool skipPrefabs = editMode?false:true;
			bool incremental = !forceBuild;
			int numLayers = tileMap.data.NumLayers;
			
			for (int layerId = 0; layerId < numLayers; ++layerId)
			{
				var layer = tileMap.Layers[layerId];
				if (layer.IsEmpty)
					continue;

				var layerData = tileMap.data.Layers[layerId];
				bool useColor = !tileMap.ColorChannel.IsEmpty && tileMap.data.Layers[layerId].useColor;
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
				bool useSortingLayer = tileMap.data.useSortingLayers;
#endif
	
				for (int cellY = 0; cellY < layer.numRows; ++cellY)
				{
					int baseY = cellY * layer.divY;
					for (int cellX = 0; cellX < layer.numColumns; ++cellX)
					{
						int baseX = cellX * layer.divX;
						var chunk = layer.GetChunk(cellX, cellY);
						
						ColorChunk colorChunk = tileMap.ColorChannel.GetChunk(cellX, cellY);
						
						bool colorChunkDirty = (colorChunk != null) && colorChunk.Dirty;
						if (incremental && !colorChunkDirty && !chunk.Dirty)
							continue;
						
						if (chunk.mesh != null)
							chunk.mesh.Clear();
						
						if (chunk.IsEmpty)
							continue;
						
						if (editMode ||
							(!editMode && !layerData.skipMeshGeneration)) {
							BuildForChunk(tileMap, chunk, colorChunk, useColor, skipPrefabs, baseX, baseY);

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
							if (chunk.gameObject != null && useSortingLayer) {
								Renderer r = chunk.gameObject.GetComponent<Renderer>();
								if (r != null) {
									r.sortingLayerName = layerData.sortingLayerName;
									r.sortingOrder = layerData.sortingOrder;
								}
							}
#endif
						}
						
						if (chunk.mesh != null)
							tileMap.TouchMesh(chunk.mesh);
					}
				}
			}
		}
	}
}

#endif