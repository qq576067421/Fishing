using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// Mirrors the Unity camera class properties
/// </summary>
public class tk2dCameraSettings {
	public enum ProjectionType {
		Orthographic,
		Perspective
	}

	public enum OrthographicType {
		PixelsPerMeter,
		OrthographicSize,
	}

	public enum OrthographicOrigin {
		BottomLeft,
		Center
	}

	public ProjectionType projection = ProjectionType.Orthographic;
	public float orthographicSize = 10.0f;
	public float orthographicPixelsPerMeter = 100;
	public OrthographicOrigin orthographicOrigin = OrthographicOrigin.Center;
	public OrthographicType orthographicType = OrthographicType.PixelsPerMeter;
	public TransparencySortMode transparencySortMode = TransparencySortMode.Default;
	public float fieldOfView = 60.0f;
	public Rect rect = new Rect( 0, 0, 1, 1 );
}

[System.Serializable]
/// <summary>
/// Controls camera scale for different resolutions.
/// Use this to display at 0.5x scale on iPhone3G or 2x scale on iPhone4
/// </summary>
public class tk2dCameraResolutionOverride {
	/// <summary>
	/// Name of the override
	/// </summary>
	public string name;

	/// <summary>
	/// Match by type
	/// </summary>
	public enum MatchByType {
		/// <summary> match by resolution </summary>
		Resolution,

		/// <summary> match by aspect ratio </summary>
		AspectRatio,

		/// <summary> match everything </summary>
		Wildcard
	};

	/// <summary>
	/// How to identify matches for this override.
	/// </summary>
	public MatchByType matchBy = MatchByType.Resolution;
	
	/// <summary>
	/// Screen width to match.
	/// </summary>
	public int width;
	/// <summary>
	/// Screen height to match.
	/// </summary>
	public int height;

	/// <summary>
	/// Aspect ratio to match to, stored as numerator and denominator
	/// to make it easier to match accurately.
	/// </summary>
	public float aspectRatioNumerator = 4.0f;
	public float aspectRatioDenominator = 3.0f;

	/// <summary>
	/// Amount to scale the matched resolution by
	/// 1.0 = pixel perfect, 0.5 = 50% of pixel perfect size
	/// </summary>
	public float scale = 1.0f;
	
	/// <summary>
	/// Amount to offset from the bottom left, in number of pixels in target resolution. Example, if override resolution is
	/// 1024x768, an offset of 20 will offset in by 20 pixels
	/// </summary>
	public Vector2 offsetPixels = new Vector2(0, 0);
	
	/// <summary>
	/// Auto Scale mode
	/// </summary>
	public enum AutoScaleMode
	{
		/// <summary> explicitly use the scale parameter </summary>
		None, 
		
		/// <summary> fits the width to the current resolution </summary>
		FitWidth, 
		
		/// <summary> fits the height to the current resolution </summary>
		FitHeight, 
		
		/// <summary> best fit (either width or height) </summary>
		FitVisible, 
		
		/// <summary> stretch to fit, could be non-uniform and/or very ugly </summary>
		StretchToFit, 
		
		/// <summary> fits to the closest power of two </summary>
		ClosestMultipleOfTwo, 
		
		/// <summary> keeps this pixel perfect always </summary>
		PixelPerfect, 
		
		/// <summary> crop to fit </summary>
		Fill,
	};

	/// <summary>
	/// How scaling is performed
	/// </summary>
	public AutoScaleMode autoScaleMode = AutoScaleMode.None;
	
	public enum FitMode
	{
		Constant,	// Use the screenOffset
		Center, 	// Align to center of screen
	};
	public FitMode fitMode = FitMode.Constant;
	
	
	/// <summary>
	/// Returns true if this instance of tk2dCameraResolutionOverride matches the curent resolution.
	/// In future versions this may  change to support ranges of resolutions in addition to explict ones.
	/// </summary>
	public bool Match(int pixelWidth, int pixelHeight)
	{
		switch (matchBy) {
			case MatchByType.Wildcard: 
				return true;
			case MatchByType.Resolution:
				return (pixelWidth == width && pixelHeight == height);
			case MatchByType.AspectRatio:
				float resolutionAspect = (float)pixelHeight / pixelWidth;
				float foundAspectDenominator  = resolutionAspect * aspectRatioNumerator;
				float difference = Mathf.Abs(foundAspectDenominator - aspectRatioDenominator);
				return difference < 0.05f;
		}

		return false;
	}

	public void Upgrade(int version) {
		if (version == 0) {
			matchBy = ((width == -1 && height == -1) || (width == 0 && height == 0)) ? MatchByType.Wildcard : MatchByType.Resolution;
		}
	}

	public static tk2dCameraResolutionOverride DefaultOverride {
		get {
			tk2dCameraResolutionOverride res = new tk2dCameraResolutionOverride();
			res.name = "Override";
			res.matchBy = MatchByType.Wildcard;
			res.autoScaleMode = AutoScaleMode.FitVisible;
			res.fitMode = FitMode.Center;
			return res;
		}
	}
}
