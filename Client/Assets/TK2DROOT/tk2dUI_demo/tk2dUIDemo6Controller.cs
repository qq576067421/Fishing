using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class tk2dUIDemo6Controller : tk2dUIBaseDemoController {

	public tk2dUILayout prefabItem;
	float itemStride = 0;

	// Manually set up a scrollable area by working out offsets manually
	public tk2dUIScrollableArea scrollableArea;
	public tk2dTextMesh numItemsTextMesh;

	// This is a representation of an entry in the list. Replace with what you need here.
	// If you create this procedurally, you can have an infinite scrollable area
	class ItemDef {
		public string name = "";
		public int score = 10;
		public int time = 200;
		public Color color = Color.white;
	}
	List<ItemDef> allItems = new List<ItemDef>();

	// Internal lists for caching
	List<Transform> cachedContentItems = new List<Transform>();
	List<Transform> unusedContentItems = new List<Transform>();
	int firstCachedItem = -1;
	int maxVisibleItems = 0;

	void OnEnable() {
		scrollableArea.OnScroll += OnScroll;
	}

	void OnDisable() {
		scrollableArea.OnScroll -= OnScroll;
	}

	void Start () {
		// Disable the prefab item
		// don't want it visible when the game is running, as it is in the scene
		prefabItem.transform.parent = null;
		DoSetActive( prefabItem.transform, false );

		// How many items do we need to buffer?
		itemStride = (prefabItem.GetMaxBounds() - prefabItem.GetMinBounds()).x;
		maxVisibleItems = Mathf.CeilToInt(scrollableArea.VisibleAreaLength / itemStride) + 1;

		// Buffer the prefabs that we will need
		float x = 0;
		for (int i = 0; i < maxVisibleItems; ++i) {
			tk2dUILayout layout = Instantiate(prefabItem) as tk2dUILayout;
			layout.transform.parent = scrollableArea.contentContainer.transform;
			layout.transform.localPosition = new Vector3(x, 0, 0);
			DoSetActive( layout.transform, false );
			unusedContentItems.Add( layout.transform );
			x += itemStride;
		}

		// Add some items to the List
		SetItemCount(100);
	}

	// Customize this function to fill the contents of the item at Id
	// This will be called as an item comes into view, so don't do any crazy
	// processing in here. It is possible to start a coroutine to cache a profile
	// picture, for instance.
	void CustomizeListObject( Transform contentRoot, int itemId ) {
		contentRoot.Find("Name").GetComponent<tk2dTextMesh>().text = allItems[itemId].name;
		contentRoot.Find("Score").GetComponent<tk2dTextMesh>().text = "Score: " + allItems[itemId].score;
		contentRoot.Find("Time").GetComponent<tk2dTextMesh>().text = "Time: " + allItems[itemId].time;
		contentRoot.Find("Portrait").GetComponent<tk2dBaseSprite>().color = allItems[itemId].color;
		contentRoot.localPosition = new Vector3(itemId * itemStride, 0, 0);
	}

	// Populate the backing fields with some values
	void SetItemCount(int numItems) {
		if (numItems < allItems.Count) {
			allItems.RemoveRange(numItems, allItems.Count - numItems);
		}
		else {
			for (int j = allItems.Count; j < numItems; ++j) {
				string[] firstPart = { "Ba", "Po", "Re", "Zu", "Meh", "Ra'", "B'k", "Adam", "Ben", "George" };
				string[] secondPart = { "Hoopler", "Hysleria", "Yeinydd", "Nekmit", "Novanoid", "Toog1t", "Yboiveth", "Resaix", "Voquev", "Yimello", "Oleald", "Digikiki", "Nocobot", "Morath", "Toximble", "Rodrup", "Chillaid", "Brewtine", "Surogou", "Winooze", "Hendassa", "Ekcle", "Noelind", "Animepolis", "Tupress", "Jeren", "Yoffa", "Acaer" };
				string name = string.Format( "[{0}] {1} {2}", j, firstPart[Random.Range(0, firstPart.Length)], secondPart[Random.Range(0, secondPart.Length)] );
		 		Color color = new Color32((byte)Random.Range(192, 255), (byte)Random.Range(192, 255), (byte)Random.Range(192, 255), 255);
				ItemDef item = new ItemDef();
				item.name = name;
				item.color = color;
				item.time = Random.Range(10, 1000);
				item.score = (item.time * Random.Range(0, 30)) / 60;
				allItems.Add(item);
			}
		}

		UpdateListGraphics();
		numItemsTextMesh.text = "COUNT: " + numItems.ToString();
	}

	void OnScroll(tk2dUIScrollableArea scrollableArea) {
		UpdateListGraphics();
	}

	// Synchronizes the graphics with the scroll amount
	// Figures out the first and last visible list items, and if that doesn't correspond
	// to what is cached, it rectifies the situation
	// Only the items that actually need to be changed are changed, so as you scroll the one that goes out 
	// of view is removed, recycled and reused for the one coming into view.
	void UpdateListGraphics() {
		// Previous offset - we will need to reset the value to match the new content length
		float previousOffset = scrollableArea.Value * (scrollableArea.ContentLength - scrollableArea.VisibleAreaLength);
		int firstVisibleItem = Mathf.FloorToInt( previousOffset / itemStride );

		// If the number of elements has changed - we do some processing
		float newContentLength = allItems.Count * itemStride;
		if (!Mathf.Approximately(newContentLength, scrollableArea.ContentLength)) {
			// If all items are visible, we simply populate as needed
			if (newContentLength < scrollableArea.VisibleAreaLength) {
				scrollableArea.Value = 0; // no more scrolling
				for (int i = 0; i < cachedContentItems.Count; ++i) {
					DoSetActive( cachedContentItems[i], false );
					unusedContentItems.Add(cachedContentItems[i]); // clear whole list
				}
				cachedContentItems.Clear();
				firstCachedItem = -1;
				firstVisibleItem = 0;
			}

			// The total size required to display all elements
			scrollableArea.ContentLength = newContentLength;
	
			// Rescale the previousOffset so it remains constant
			if (scrollableArea.ContentLength > 0) {
				scrollableArea.Value = previousOffset / (scrollableArea.ContentLength - scrollableArea.VisibleAreaLength);
			}
		}
		int lastVisibleItem = Mathf.Min(firstVisibleItem + maxVisibleItems, allItems.Count);

		// If any items are visible that shouldn't need to be visible, get rid of them
		while (firstCachedItem >= 0 && firstCachedItem < firstVisibleItem) {
			firstCachedItem++;
			DoSetActive(cachedContentItems[0], false);
			unusedContentItems.Add(cachedContentItems[0]);
			cachedContentItems.RemoveAt(0);
			if (cachedContentItems.Count == 0) {
				firstCachedItem = -1;
			}
		}

		// Ditto for end of list
		while (firstCachedItem >= 0 && (firstCachedItem + cachedContentItems.Count) > lastVisibleItem ) {
			DoSetActive(cachedContentItems[cachedContentItems.Count - 1], false);
			unusedContentItems.Add(cachedContentItems[cachedContentItems.Count - 1]);
			cachedContentItems.RemoveAt(cachedContentItems.Count - 1);
			if (cachedContentItems.Count == 0) {
				firstCachedItem = -1;
			}
		}

		// Nothing visible, simply fill as needed
		if (firstCachedItem < 0) {
			firstCachedItem = firstVisibleItem;
			int maxToAdd = Mathf.Min( firstCachedItem + maxVisibleItems, allItems.Count );
			for (int i = firstCachedItem; i < maxToAdd; ++i) {
				Transform t = unusedContentItems[0];
				cachedContentItems.Add(t);
				unusedContentItems.RemoveAt(0);
				CustomizeListObject( t, i );
				DoSetActive(t, true);
			}
		}
		else {
			// Fill in items that should be visible but aren't
			while (firstCachedItem > firstVisibleItem) {
				--firstCachedItem;
				Transform t = unusedContentItems[0];
				unusedContentItems.RemoveAt(0);
				cachedContentItems.Insert(0, t);
				CustomizeListObject(t, firstCachedItem);
				DoSetActive(t, true);
			}
			while (firstCachedItem + cachedContentItems.Count < lastVisibleItem) {
				Transform t = unusedContentItems[0];
				unusedContentItems.RemoveAt(0);
				CustomizeListObject(t, firstCachedItem + cachedContentItems.Count);
				cachedContentItems.Add(t);
				DoSetActive(t, true);
			}
		}
	}

#region ButtonHandlers
	int numToAdd = 100;

	// Event handler for "Add more..." button
	void AddMoreItems() {
		SetItemCount(allItems.Count + Random.Range(numToAdd / 10, numToAdd));
		numToAdd *= 2;
	}
	// Event handler for "Reset" button
	void ResetItems() {
		numToAdd = 100;
		SetItemCount(3);
	}
#endregion
}
