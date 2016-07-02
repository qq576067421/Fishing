using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR || !UNITY_FLASH

namespace tk2dRuntime.TileMap
{
	public static class ColliderBuilder2D
	{
		public static void Build(tk2dTileMap tileMap, bool forceBuild)
		{
			bool incremental = !forceBuild;
			int numLayers = tileMap.Layers.Length;
			for (int layerId = 0; layerId < numLayers; ++layerId)
			{
				var layer = tileMap.Layers[layerId];
				if (layer.IsEmpty || !tileMap.data.Layers[layerId].generateCollider)
					continue;
	
				for (int cellY = 0; cellY < layer.numRows; ++cellY)
				{
					int baseY = cellY * layer.divY;
					for (int cellX = 0; cellX < layer.numColumns; ++cellX)
					{
						int baseX = cellX * layer.divX;
						var chunk = layer.GetChunk(cellX, cellY);
						
						if (incremental && !chunk.Dirty)
							continue;
						
						if (chunk.IsEmpty)
							continue;
						
						BuildForChunk(tileMap, chunk, baseX, baseY);

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
						PhysicsMaterial2D material = tileMap.data.Layers[layerId].physicsMaterial2D;
						foreach (EdgeCollider2D ec in chunk.edgeColliders) {
							if (ec != null) {
								ec.sharedMaterial = material;
							}
						}
#endif
					}
				}
			}
		}

		public static void BuildForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY)
		{
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			////////////////////////////////////////////////////////////////////////////////////////
			// 1. Build local edge list
			////////////////////////////////////////////////////////////////////////////////////////
			Vector2[] localVerts = new Vector2[0];
			int[] localIndices = new int[0];
			List<Vector2[]> mergedEdges = new List<Vector2[]>();
			BuildLocalMeshForChunk(tileMap, chunk, baseX, baseY, ref localVerts, ref localIndices);

			////////////////////////////////////////////////////////////////////////////////////////
			// 2. Optimize
			////////////////////////////////////////////////////////////////////////////////////////
			if (localIndices.Length > 4) {
				// Remove duplicate verts, reindex
				localVerts = WeldVertices(localVerts, ref localIndices);
				// Remove duplicate and back-to-back edges
				// Removes inside edges
				localIndices = RemoveDuplicateEdges(localIndices);
			}

			////////////////////////////////////////////////////////////////////////////////////////
			// 3. Build and optimize an edge list
			////////////////////////////////////////////////////////////////////////////////////////
			mergedEdges = MergeEdges( localVerts, localIndices );

			////////////////////////////////////////////////////////////////////////////////////////
			// 4. Build the edge colliders
			////////////////////////////////////////////////////////////////////////////////////////

			if (chunk.meshCollider != null) {
				tk2dUtil.DestroyImmediate(chunk.meshCollider);
				chunk.meshCollider = null;
			}

			if (mergedEdges.Count == 0) {
				for (int i = 0; i < chunk.edgeColliders.Count; ++i) {
					if (chunk.edgeColliders[i] != null) {
						tk2dUtil.DestroyImmediate(chunk.edgeColliders[i]);
					}
				}
				chunk.edgeColliders.Clear();
			}
			else {
				int numEdges = mergedEdges.Count;

				// Destroy surplus
				for (int i = numEdges; i < chunk.edgeColliders.Count; ++i) {
					if (chunk.edgeColliders[i] != null) {
						tk2dUtil.DestroyImmediate(chunk.edgeColliders[i]);
					}
				}
				int numToRemove = chunk.edgeColliders.Count - numEdges;
				if (numToRemove > 0) {
					chunk.edgeColliders.RemoveRange(chunk.edgeColliders.Count - numToRemove, numToRemove);
				}

				// Make sure existing ones are not null
				for (int i = 0; i < chunk.edgeColliders.Count; ++i) {
					if (chunk.edgeColliders[i] == null) {
						chunk.edgeColliders[i] = tk2dUtil.AddComponent<EdgeCollider2D>(chunk.gameObject);
					}
				}
				// Create missing
				while (chunk.edgeColliders.Count < numEdges) {
					chunk.edgeColliders.Add( tk2dUtil.AddComponent<EdgeCollider2D>(chunk.gameObject) );
				}

				for (int i = 0; i < numEdges; ++i) {
					chunk.edgeColliders[i].points = mergedEdges[i];
				}
			}
#endif
		}		
		
		// Builds an unoptimized mesh for this chunk
		static void BuildLocalMeshForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY, ref Vector2[] vertices, ref int[] indices)
		{
			List<Vector2> verts = new List<Vector2>();
			List<int> inds = new List<int>();
			Vector2[] boxPos = new Vector2[4]; // used for box collider
			int[] boxInds = { 0, 1, 1, 2, 2, 3, 3, 0 };
			int[] boxIndsFlipped = { 0, 3, 3, 2, 2, 1, 1, 0 };
			
			int spriteCount = tileMap.SpriteCollectionInst.spriteDefinitions.Length;
			Vector2 tileSize = new Vector3(tileMap.data.tileSize.x, tileMap.data.tileSize.y);
			
			var tilePrefabs = tileMap.data.tilePrefabs;
			
			float xOffsetMult = 0.0f, yOffsetMult = 0.0f;
			tileMap.data.GetTileOffset(out xOffsetMult, out yOffsetMult);

			var chunkData = chunk.spriteIds;
			for (int y = 0; y < tileMap.partitionSizeY; ++y)
			{
				float xOffset = ((baseY + y) & 1) * xOffsetMult;
				for (int x = 0; x < tileMap.partitionSizeX; ++x)
				{
					int spriteId = chunkData[y * tileMap.partitionSizeX + x];
					int spriteIdx = BuilderUtil.GetTileFromRawTile(spriteId);
					Vector2 currentPos = new Vector2(tileSize.x * (x + xOffset), tileSize.y * y);
	
					if (spriteIdx < 0 || spriteIdx >= spriteCount) 
						continue;
					
					if (tilePrefabs[spriteIdx])
						continue;

					bool flipH = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.FlipX);
					bool flipV = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.FlipY);
					bool rot90 = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.Rot90);

					bool reverseIndices = false;
					if (flipH) reverseIndices = !reverseIndices;
					if (flipV) reverseIndices = !reverseIndices;

					tk2dSpriteDefinition spriteData = tileMap.SpriteCollectionInst.spriteDefinitions[spriteIdx];
					int baseVertexIndex = verts.Count;
					
					if (spriteData.colliderType == tk2dSpriteDefinition.ColliderType.Box)
					{
						Vector3 origin = spriteData.colliderVertices[0];
						Vector3 extents = spriteData.colliderVertices[1];
						Vector3 min = origin - extents;
						Vector3 max = origin + extents;

						boxPos[0] = new Vector2(min.x, min.y);
						boxPos[1] = new Vector2(max.x, min.y);
						boxPos[2] = new Vector2(max.x, max.y);
						boxPos[3] = new Vector2(min.x, max.y);
						for (int i = 0; i < 4; ++i) {
							verts.Add( BuilderUtil.ApplySpriteVertexTileFlags(tileMap, spriteData, boxPos[i], flipH, flipV, rot90) + currentPos );
						}

						int[] boxIndices = reverseIndices ? boxIndsFlipped : boxInds;
						for (int i = 0; i < 8; ++i) {
							inds.Add( baseVertexIndex + boxIndices[i] );
						}

					}
					else if (spriteData.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
					{
						foreach (tk2dCollider2DData dat in spriteData.edgeCollider2D) {
							baseVertexIndex = verts.Count;
							foreach (Vector2 pos in dat.points) {
								verts.Add( BuilderUtil.ApplySpriteVertexTileFlags(tileMap, spriteData, pos, flipH, flipV, rot90) + currentPos );
							}
							int numVerts = dat.points.Length;
							if (reverseIndices) {
								for (int i = numVerts - 1; i > 0; --i) {
									inds.Add( baseVertexIndex + i );
									inds.Add( baseVertexIndex + i - 1 );
								}
							}
							else {
								for (int i = 0; i < numVerts - 1; ++i) {
									inds.Add(baseVertexIndex + i);
									inds.Add(baseVertexIndex + i + 1);
								}
							}
						}
						foreach (tk2dCollider2DData dat in spriteData.polygonCollider2D) {
							baseVertexIndex = verts.Count;
							foreach (Vector2 pos in dat.points) {
								verts.Add( BuilderUtil.ApplySpriteVertexTileFlags(tileMap, spriteData, pos, flipH, flipV, rot90) + currentPos );
							}
							int numVerts = dat.points.Length;
							if (reverseIndices) {
								for (int i = numVerts; i > 0; --i) {
									inds.Add( baseVertexIndex + (i % numVerts) );
									inds.Add( baseVertexIndex + i - 1 );
								}
							}
							else {
								for (int i = 0; i < numVerts; ++i) {
									inds.Add(baseVertexIndex + i);
									inds.Add(baseVertexIndex + (i + 1) % numVerts);
								}
							}
						}
					}
				}
			}
			
			vertices = verts.ToArray();
			indices = inds.ToArray();
		}
		
		static int CompareWeldVertices(Vector2 a, Vector2 b)
		{
			// Compare one component at a time, using epsilon
			float epsilon = 0.01f;
			float dx = a.x - b.x;
			if (Mathf.Abs(dx) > epsilon) return (int)Mathf.Sign(dx);
			float dy = a.y - b.y;
			if (Mathf.Abs(dy) > epsilon) return (int)Mathf.Sign(dy);
			return 0;
		}
		
		static Vector2[] WeldVertices(Vector2[] vertices, ref int[] indices)
		{
			// Sort by x, y and z
			// Adjacent values could be the same after this sort
			int[] sortIndex = new int[vertices.Length];
			for (int i = 0; i < vertices.Length; ++i) {
				sortIndex[i] = i;
			}
			System.Array.Sort<int>(sortIndex, (a, b) => CompareWeldVertices(vertices[a], vertices[b]) );
			
			// Step through the list, comparing current with previous value
			// If they are the same, use the current index
			// Otherwise add a new vertex to the vertex list, and use this index
			// Welding all similar vertices
			List<Vector2> newVertices = new List<Vector2>();
			int[] vertexRemap = new int[vertices.Length];
			// prime first value
			Vector2 previousValue = vertices[sortIndex[0]];
			newVertices.Add(previousValue);
			vertexRemap[sortIndex[0]] = newVertices.Count - 1;
			for (int i = 1; i < sortIndex.Length; ++i) {
				Vector2 v = vertices[sortIndex[i]];
				if (CompareWeldVertices(v, previousValue) != 0) {
					// add new vertex
					previousValue = v;
					newVertices.Add(previousValue);
					vertexRemap[sortIndex[i]] = newVertices.Count - 1;
				}
				vertexRemap[sortIndex[i]] = newVertices.Count - 1;
			}
			
			// remap indices
			for (int i = 0; i < indices.Length; ++i) {
				indices[i] = vertexRemap[indices[i]];
			}
			
			return newVertices.ToArray();
		}
		
		static int CompareDuplicateFaces(int[] indices, int face0index, int face1index)
		{
			for (int i = 0; i < 2; ++i)
			{
				int d = indices[face0index + i] - indices[face1index + i];
				if (d != 0) return d;
			}
			return 0;
		}
		
		static int[] RemoveDuplicateEdges(int[] indices)
		{
			// Create an ascending sorted list of edge indices
			// If 2 sets of indices are identical, then the edges share the same vertices, and either
			// is a duplicate, or back-to-back
			int[] sortedFaceIndices = new int[indices.Length];
			for (int i = 0; i < indices.Length; i += 2)
			{
				if (indices[i] > indices[i + 1]) {
					sortedFaceIndices[i] = indices[i+1];
					sortedFaceIndices[i+1] = indices[i];
				}
				else {
					sortedFaceIndices[i] = indices[i];
					sortedFaceIndices[i+1] = indices[i+1];
				}
			}
			
			// Sort by faces
			int[] sortIndex = new int[indices.Length / 2];
			for (int i = 0; i < indices.Length; i += 2) {
				sortIndex[i / 2] = i;
			}
			System.Array.Sort<int>(sortIndex, (a, b) => CompareDuplicateFaces(sortedFaceIndices, a, b));
			
			List<int> newIndices = new List<int>();
			for (int i = 0; i < sortIndex.Length; ++i)
			{
				if (i != sortIndex.Length - 1 && CompareDuplicateFaces(sortedFaceIndices, sortIndex[i], sortIndex[i+1]) == 0)
				{
					// skip both faces
					// this will fail in the case where there are 3 coplanar faces
					// but that is probably likely user error / intentional
					i++;
					continue;
				}
				
				for (int j = 0; j < 2; ++j)
					newIndices.Add(indices[sortIndex[i] + j]);
			}
			
			return newIndices.ToArray();
		}

		static List<Vector2[]> MergeEdges(Vector2[] verts, int[] indices) {
			// Brute force search, but almost entirely deals with ints
			// Search area significantly reduced by previous welding and other opts
			// While possible to optimize further, this is almost never the bottleneck.
			// Normals are generated exactly once per vertex
			List<Vector2[]> edges = new List<Vector2[]>();
			List<Vector2> edgeVerts = new List<Vector2>();
			List<int> edgeIndices = new List<int>();

			Vector2 d0 = Vector2.zero;
			Vector2 d1 = Vector2.zero;
			bool[] edgeUsed = new bool[indices.Length / 2];
			bool processedEdge = true;
			while (processedEdge) {
				processedEdge = false;
				for (int i = 0; i < edgeUsed.Length; ++i) {
					if (!edgeUsed[i]) {
						edgeUsed[i] = true;
						int v0 = indices[i * 2 + 0];
						int v1 = indices[i * 2 + 1];
						d0 = (verts[v1] - verts[v0]).normalized;
						edgeIndices.Add(v0);
						edgeIndices.Add(v1);

						// The connecting vertex for this edge list
						for (int k = i + 1; k < edgeUsed.Length; ++k) {
							if (edgeUsed[k]) {
								continue;
							}
							int w0 = indices[k * 2 + 0];
							if (w0 == v1) {
								int w1 = indices[k * 2 + 1];
								d1 = (verts[w1] - verts[w0]).normalized;
								// Same direction?
								if (Vector2.Dot(d1, d0) > 0.999f) {
									edgeIndices.RemoveAt(edgeIndices.Count - 1); // remove last
								}
								edgeIndices.Add(w1);
								edgeUsed[k] = true;
								d0 = d1; // new normal
								k = i; // restart the loop
								v1 = w1; // continuing from the end of the loop
								continue;
							}
						}

						processedEdge = true;
						break;
					}
				}

				if (processedEdge) {
					edgeVerts.Clear();
					edgeVerts.Capacity = Mathf.Max(edgeVerts.Capacity, edgeIndices.Count);

					for (int i = 0; i < edgeIndices.Count; ++i) {
						edgeVerts.Add( verts[edgeIndices[i]] );
					}
					edges.Add(edgeVerts.ToArray());

					edgeIndices.Clear();
				}
			}

			return edges;
		}
	}
}

#endif