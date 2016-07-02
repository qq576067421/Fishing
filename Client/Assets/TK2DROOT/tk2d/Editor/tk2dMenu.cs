public static class tk2dMenu
{
	public const string root = "2D Toolkit/";
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1  || UNITY_5_2  || UNITY_5_3  || UNITY_5_4  || UNITY_5_5  || UNITY_5_6  || UNITY_5_7  || UNITY_5_8  || UNITY_5_9 || UNITY_6_0
	public const string createBase = "GameObject/2D Object/tk2d/";
#else
	public const string createBase = "GameObject/Create Other/tk2d/";
#endif
}
