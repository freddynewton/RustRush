using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace zone.nonon
{
    [ExecuteInEditMode]
    public static class NononZoneLeakDetectionControl
    {
        [MenuItem(NononZoneConstants.CommonEditor.LEAK_DETECTION_ENABLED)]
        private static void LeakDetection()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
        }

        [MenuItem(NononZoneConstants.CommonEditor.LEAK_DETECTION_STACKTRACE_ENABLED)]
        private static void LeakDetectionWithStackTrace()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
        }

        [MenuItem(NononZoneConstants.CommonEditor.LEAK_DETECTION_DISABLED)]
        private static void NoLeakDetection()
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
        }
    }
}