/*using System.Collections;
using System.Collections.Generic;*/
using UnityEngine;
using Unity.MLAgents.Sensors;

public static class MLAgentsExtensions {
    public static float GetGaussianReward(float x, float rewardCenter, float standardDeviation = 1f, float height = 1f) {
        float y = height * Mathf.Exp(-(Mathf.Pow(x - rewardCenter, 2) / 2 * Mathf.Pow(standardDeviation, 2)));
        return y;
    }

    public static float GetGaussianSlope(float x, float rewardCenter, float standardDeviation = 1f, float height = 1f, float pointsDistance = 0.25f) {

        float a = MLAgentsExtensions.GetGaussianReward(x + pointsDistance, rewardCenter, standardDeviation, height);
        float b = MLAgentsExtensions.GetGaussianReward(x - pointsDistance, rewardCenter, standardDeviation, height);
        return a - b;
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
