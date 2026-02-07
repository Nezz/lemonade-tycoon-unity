#import <Foundation/Foundation.h>

// NativeCallProxy.h
// This file provides the interface for Unity to communicate with the native iOS app

@protocol NativeCallsProtocol
@required
- (void) sendMessageToMobileApp:(NSString*)message;
@end

__attribute__ ((visibility("default")))
@interface FrameworkLibAPI : NSObject
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi;
@end
