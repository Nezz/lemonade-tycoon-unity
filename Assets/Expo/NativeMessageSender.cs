namespace Yousician.Expo
{
	internal static class NativeMessageSender
	{
		public static void SendMessageToMobileApp(string message)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			using (UnityEngine.AndroidJavaClass jc = new UnityEngine.AndroidJavaClass("com.yousician.unity.UnityModule"))
			{
				jc.CallStatic("sendMessageToMobileApp", message);
			}
#elif UNITY_IOS && !UNITY_EDITOR
			sendMessageToMobileApp(message);
#else
			UnityEngine.Debug.Log($"Sending message: {message}");
#endif
		}

#if UNITY_IOS && !UNITY_EDITOR
		[System.Runtime.InteropServices.DllImport("__Internal")]
		public static extern void sendMessageToMobileApp(string message);
#endif
	}
}
