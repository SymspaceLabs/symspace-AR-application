#import <Foundation/Foundation.h>

typedef void (*HandPoseCallback)(float* landmarks, int landmarkCount, int width, int height, const char* handness);

@interface HandPoseDetector : NSObject
@property (nonatomic, assign) HandPoseCallback callback;
- (void)detectHandPose:(CVPixelBufferRef)pixelBuffer;
- (void)detectHandForRings:(CVPixelBufferRef)pixelBuffer callback:(HandPoseCallback)ringCallback;
- (void)cleanupRingDetection;
@end