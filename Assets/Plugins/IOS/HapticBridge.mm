#import <UIKit/UIKit.h>
#import <AudioToolbox/AudioToolbox.h>

static UIImpactFeedbackGenerator* s_lightGenerator = nil;
static UINotificationFeedbackGenerator* s_notificationGenerator = nil;

extern "C" {

    void HapticPrepareLight() {
        if (s_lightGenerator == nil) {
            s_lightGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        }
        [s_lightGenerator prepare];
    }

    void HapticTickLight() {
        if (s_lightGenerator == nil) {
            s_lightGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        }
        [s_lightGenerator impactOccurred];
        [s_lightGenerator prepare];
    }

    void HapticSuccess() {
        if (s_notificationGenerator == nil) {
            s_notificationGenerator = [[UINotificationFeedbackGenerator alloc] init];
        }
        [s_notificationGenerator prepare];
        [s_notificationGenerator notificationOccurred:UINotificationFeedbackTypeSuccess];
    }

    bool ReduceMotionEnabled() {
        return UIAccessibilityIsReduceMotionEnabled();
    }
}
