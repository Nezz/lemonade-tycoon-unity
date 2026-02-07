#import "NativeCallProxy.h"

// NativeCallProxy.mm
// This file implements the bridge between Unity and the native iOS app

@implementation FrameworkLibAPI

static id<NativeCallsProtocol> api = NULL;

+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi
{
    api = aApi;
}

@end

extern "C"
{
    void sendMessageToMobileApp(const char* message)
    {
        if (api != NULL) {
            [api sendMessageToMobileApp:[NSString stringWithUTF8String:message]];
        } else {
            NSLog(@"Unity message received but no API registered: %s", message);
        }
    }
}
