# Native Messaging

Bidirectional string messaging between Unity and the host mobile app (iOS/Android).

## Architecture

```
React Native Expo App
  |  ^
  |  |  (platform-specific transport)
  v  |
NativeMessageReceiver (MonoBehaviour)  /  NativeMessageSender (static)
  |  ^
  |  |  (forwards raw strings)
  v  |
NativeMessageChannel (static) ── event Action<string> MessageReceived
```

All messages are plain strings for simplicity. In practice we highly recommend using JSON for messages.

## Setup

The `NativeMessageReceiver` must be attached to a GameObject in the scene. The native app sends messages to Unity via `UnitySendMessage`, targeting this object's name and the `Receive` method.

On startup, `NativeMessageReceiver` automatically sends an `"initialized"` message to the native app.

## Usage

### Sending a message to the native app

```csharp
NativeMessageChannel.Send("some-command");
```

### Receiving messages from the native app

```csharp
NativeMessageChannel.MessageReceived += OnNativeMessage;

void OnNativeMessage(string message)
{
    Debug.Log($"Received: {message}");
}
```

### Editor testing

In the editor, outbound messages are logged to the console. To simulate an inbound message:

```csharp
NativeMessageChannel.TriggerFakeMessage("test-payload");
```
