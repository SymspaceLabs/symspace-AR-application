#import "HandPoseDetector.h"

// Global detector instance
static HandPoseDetector* detector = nil;

// C interface for Unity
extern "C" {
    
    void InitializeHandDetector(HandPoseCallback callback) {
        if (!detector) {
            detector = [[HandPoseDetector alloc] init];
            detector.callback = callback;
        }
    }
    
    void ProcessFrame(void* pixelData, int width, int height) {
        if (!detector) return;
        
        CVPixelBufferRef pixelBuffer = NULL;
        CVReturn status = CVPixelBufferCreateWithBytes(
            kCFAllocatorDefault,
            width,
            height,
            kCVPixelFormatType_32BGRA,
            pixelData,
            width * 4,
            NULL,
            NULL,
            NULL,
            &pixelBuffer
        );
        
        if (status == kCVReturnSuccess && pixelBuffer) {
            [detector detectHandPose:pixelBuffer];
            CVPixelBufferRelease(pixelBuffer);
        }
    }

	void ProcessFrameForRings(void* pixelData, int width, int height, 	HandPoseCallback ringCallback) {
        if (!detector) return;
        
        CVPixelBufferRef pixelBuffer = NULL;
        CVReturn status = CVPixelBufferCreateWithBytes(
            kCFAllocatorDefault,
            width,
            height,
            kCVPixelFormatType_32BGRA,
            pixelData,
            width * 4,
            NULL,
            NULL,
            NULL,
            &pixelBuffer
        );
        
        if (status == kCVReturnSuccess && pixelBuffer) {
            [detector detectHandForRings:pixelBuffer callback:ringCallback];
            CVPixelBufferRelease(pixelBuffer);
        }
    }
    
    void CleanupHandDetector() {
        detector = nil;
    }
}