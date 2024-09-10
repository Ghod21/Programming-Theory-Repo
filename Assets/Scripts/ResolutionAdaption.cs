using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionAdaption : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera secondaryCamera;

    private float targetAspect = 256.0f / 145.0f;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Start()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        UpdateCameraViewport(mainCamera);
        if (secondaryCamera != null)
        {
            UpdateCameraViewport(secondaryCamera);
        }
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            UpdateCameraViewport(mainCamera);
            if (secondaryCamera != null)
            {
                UpdateCameraViewport(secondaryCamera);
            }
        }
    }

    void UpdateCameraViewport(Camera camera)
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }
}