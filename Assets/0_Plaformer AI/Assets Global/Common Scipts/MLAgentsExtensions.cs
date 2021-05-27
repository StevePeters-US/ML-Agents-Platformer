/*using System.Collections;
using System.Collections.Generic;*/
using UnityEngine;
using Unity.MLAgents.Sensors;

public static class MLAgentsExtensions {
    public static float GetGaussianReward(float x, float rewardCenter, int maxSteps, float standardDeviation = 1) {
        float a = 1f;
        return a * Mathf.Exp(-(Mathf.Pow((x - rewardCenter), 2) / Mathf.Pow(2 * standardDeviation, 2)));
    }

    // Based on the match 3 extension methods
    public static void WriteOneHot(this ObservationWriter writer, int offset, int row, int col, int value, int oneHotSize, bool isVisual) {
        if (isVisual) {
            for (var i = 0; i < oneHotSize; i++) {
                writer[row, col, i] = (i == value) ? 1.0f : 0.0f;
            }
        }
        else {
            for (var i = 0; i < oneHotSize; i++) {
                writer[offset] = (i == value) ? 1.0f : 0.0f;
                offset++;
            }
        }
    }

    public static void WriteZero(this ObservationWriter writer, int offset, int row, int col, int oneHotSize, bool isVisual) {
        if (isVisual) {
            for (var i = 0; i < oneHotSize; i++) {
                writer[row, col, i] = 0.0f;
            }
        }
        else {
            for (var i = 0; i < oneHotSize; i++) {
                writer[offset] = 0.0f;
                offset++;
            }
        }
    }
}
