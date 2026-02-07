using UnityEngine;
using Yousician.Expo;
using Yousician.Expo.Messages;
using Yousician.Expo.Messages.Inbound;

namespace Yousician
{
	/// <summary>
	/// Toggles between two cameras based on the CameraViewChanged message
	/// received from the native app. Assign both cameras in the Inspector;
	/// the day camera is active by default.
	/// </summary>
	public sealed class CameraViewManager : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The camera used for the day view.")]
		private Camera dayCamera;

		[SerializeField]
		[Tooltip("The camera used for the simulation view.")]
		private Camera simulationCamera;

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
			if (dayCamera != null)
				dayCamera.gameObject.SetActive(view == CameraView.Day);

			if (simulationCamera != null)
				simulationCamera.gameObject.SetActive(view == CameraView.Simulation);
		}
	}
}
