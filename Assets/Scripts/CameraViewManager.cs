using UnityEngine;
using Yousician.Expo;
using Yousician.Expo.Messages;
using Yousician.Expo.Messages.Inbound;

namespace Yousician
{
	/// <summary>
	/// Toggles between two sets of GameObjects based on the CameraViewChanged message
	/// received from the native app. Assign GameObjects for each view in the Inspector;
	/// the day objects are active by default.
	/// </summary>
	public sealed class CameraViewManager : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("GameObjects active during the day view (cameras, lights, etc.).")]
		private GameObject[] dayObjects;

		[SerializeField]
		[Tooltip("GameObjects active during the simulation view (cameras, lights, etc.).")]
		private GameObject[] simulationObjects;

		private void OnEnable()
		{
			NativeMessageChannel.MessageReceived += OnMessageReceived;
		}

		private void OnDisable()
		{
			NativeMessageChannel.MessageReceived -= OnMessageReceived;
		}

		private void OnMessageReceived(InboundMessage message)
		{
			if (message is CameraViewChanged changed)
			{
				SetView(changed.View);
			}
		}

		private void SetView(CameraView view)
		{
			SetObjectsActive(dayObjects, view == CameraView.Day);
			SetObjectsActive(simulationObjects, view == CameraView.Simulation);
		}

		private static void SetObjectsActive(GameObject[] objects, bool active)
		{
			if (objects == null) return;

			for (int i = 0; i < objects.Length; i++)
			{
				if (objects[i] != null)
					objects[i].SetActive(active);
			}
		}
	}
}
