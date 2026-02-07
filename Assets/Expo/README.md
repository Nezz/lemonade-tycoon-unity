# Native Messaging

Bidirectional JSON messaging between Unity and the host mobile app (iOS/Android).

## Architecture

```
React Native Expo App
  |  ^
  |  |  (platform-specific transport, JSON strings over the wire)
  v  |
NativeMessageReceiver (MonoBehaviour)  /  NativeMessageSender (static)
  |  ^
  |  |  (JSON serialization / deserialization)
  v  |
NativeMessageChannel (static) ── event Action<InboundMessage> MessageReceived
                                    Send(OutboundMessage)
```

Messages are strongly-typed C# objects serialized as JSON using Newtonsoft.Json. A `messageType` discriminator field in the JSON determines the concrete type.

## Message Types

### OutboundMessage (Unity -> Native)

Defined in `Messages/OutboundMessage.cs`. Add new outbound types by:

1. Creating a concrete class in `Messages/Outbound/` that extends `OutboundMessage`.
2. Adding a corresponding entry to the private `MessageType` enum inside `OutboundMessage`.

### InboundMessage (Native -> Unity)

Defined in `Messages/InboundMessage.cs`. Add new inbound types the same way, under `Messages/Inbound/`.

### Example JSON on the wire

```json
{"messageType": "Initialized"}
```

## Setup

The `NativeMessageReceiver` must be attached to a GameObject in the scene. The native app sends messages to Unity via `UnitySendMessage`, targeting this object's name and the `Receive` method.

On startup, `NativeMessageReceiver` automatically sends an `Initialized` message to the native app.

## Usage

### Sending a message to the native app

```csharp
NativeMessageChannel.Send(new Initialized());
```

### Receiving messages from the native app

```csharp
using Yousician.Expo;
using Yousician.Expo.Messages;

NativeMessageChannel.MessageReceived += OnNativeMessage;

void OnNativeMessage(InboundMessage message)
{
    switch (message)
    {
        case SomeConcreteInbound cmd:
            // handle...
            break;
    }
}
```

### Editor testing

In the editor, outbound messages are logged to the console. To simulate an inbound message:

```csharp
// From a typed object:
NativeMessageChannel.TriggerFakeMessage(someInboundMessage);

// From raw JSON:
NativeMessageChannel.TriggerFakeMessage("{\"messageType\": \"SomeCommand\"}");
```

## Adding a new message type

For example, adding a new inbound `ConfigUpdate` message:

1. Create `Assets/Expo/Messages/Inbound/ConfigUpdate.cs`:

```csharp
using Newtonsoft.Json;

namespace Yousician.Expo.Messages.Inbound
{
    public sealed class ConfigUpdate : InboundMessage
    {
        [JsonProperty("volume")]
        public float Volume { get; set; }
    }
}
```

2. Register it in `InboundMessage.cs`:

```csharp
[EnumMember(Value = "ConfigUpdate")]
[JsonTypeDiscriminator(typeof(Inbound.ConfigUpdate))]
ConfigUpdate,
```
