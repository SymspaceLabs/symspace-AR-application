#import "HandPoseDetector.h"
#import <Vision/Vision.h>
#import <ARKit/ARKit.h>

@implementation HandPoseDetector

- (void)detectHandPose:(CVPixelBufferRef)pixelBuffer {
    if (!pixelBuffer) return;
    
    VNImageRequestHandler *handler = [[VNImageRequestHandler alloc] initWithCVPixelBuffer:pixelBuffer orientation:kCGImagePropertyOrientationUp options:@{}];
    
    VNDetectHumanHandPoseRequest *request = [[VNDetectHumanHandPoseRequest alloc] init];
    request.maximumHandCount = 1;
    
    NSError *error;
    [handler performRequests:@[request] error:&error];
    
    if (request.results.count > 0) {
        VNHumanHandPoseObservation *observation = request.results.firstObject;
        
        // Get all finger tips
        NSDictionary<VNRecognizedPointKey, VNRecognizedPoint*> *allLandmarks = [observation recognizedPointsForJointsGroupName:VNHumanHandPoseObservationJointsGroupNameAll error:&error];
        
        // Prepare landmarks for Unity (21 points * 3 values each)
        int landmarkCount = 21;
        float landmarks[landmarkCount * 3];
        int index = 0;
        
        // Order: Wrist, Thumb(4), Index(4), Middle(4), Ring(4), Pinky(4)
        NSArray<VNRecognizedPointKey> *jointOrder = @[
            VNHumanHandPoseObservationJointNameWrist,
            // Thumb
            VNHumanHandPoseObservationJointNameThumbCMC, VNHumanHandPoseObservationJointNameThumbMP, 
            VNHumanHandPoseObservationJointNameThumbIP, VNHumanHandPoseObservationJointNameThumbTip,
            // Index
            VNHumanHandPoseObservationJointNameIndexMCP, VNHumanHandPoseObservationJointNameIndexPIP,
            VNHumanHandPoseObservationJointNameIndexDIP, VNHumanHandPoseObservationJointNameIndexTip,
            // Middle
            VNHumanHandPoseObservationJointNameMiddleMCP, VNHumanHandPoseObservationJointNameMiddlePIP,
            VNHumanHandPoseObservationJointNameMiddleDIP, VNHumanHandPoseObservationJointNameMiddleTip,
            // Ring
            VNHumanHandPoseObservationJointNameRingMCP, VNHumanHandPoseObservationJointNameRingPIP,
            VNHumanHandPoseObservationJointNameRingDIP, VNHumanHandPoseObservationJointNameRingTip,
            // Pinky
            VNHumanHandPoseObservationJointNameLittleMCP, VNHumanHandPoseObservationJointNameLittlePIP,
            VNHumanHandPoseObservationJointNameLittleDIP, VNHumanHandPoseObservationJointNameLittleTip
        ];
        
        CGSize imageSize = CGSizeMake(CVPixelBufferGetWidth(pixelBuffer), CVPixelBufferGetHeight(pixelBuffer));
        
        for (NSString *jointKey in jointOrder) {
            VNRecognizedPoint *point = allLandmarks[jointKey];
            if (point) {
                landmarks[index++] = point.location.x * imageSize.width;
                landmarks[index++] = (1 - point.location.y) * imageSize.height;
                landmarks[index++] = point.confidence;
            } else {
                landmarks[index++] = 0;
                landmarks[index++] = 0;
                landmarks[index++] = 0;
            }
        }
        
        if (self.callback) {
            self.callback(landmarks, landmarkCount, imageSize.width, imageSize.height, "right");
        }
    }
}

- (void)cleanupRingDetection {
    // Cleanup if needed
}

@end