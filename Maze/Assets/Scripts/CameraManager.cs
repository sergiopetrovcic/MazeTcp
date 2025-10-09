using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public List<Camera> cameras = new List<Camera>();
    private int currentCamera = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DisableAllCameras();
        Enable(currentCamera);
    }

    private void EnableAllCameras()
    {
        foreach (Camera c in cameras)
        {
            c.enabled = true;
            c.tag = "Untagged";
            AudioListener audioListener = c.GetComponent<AudioListener>();
            if (audioListener != null)
                audioListener.enabled = false;
        }
    }

    private void DisableAllCameras()
    {
        foreach (Camera c in cameras)
        {
            c.enabled = false;
            c.tag = "Untagged";
            AudioListener audioListener = c.GetComponent<AudioListener>();
            if (audioListener != null)
                audioListener.enabled = false;
        }
    }

    private void Enable(Camera cam)
    {
        DisableAllCameras();
        Enable(cameras.IndexOf(cam));
    }

    private void Enable(int c)
    {
        DisableAllCameras();
        currentCamera = c;
        cameras[c].enabled = true;
        cameras[c].tag = "MainCamera";
        cameras[c].rect = new Rect(0f, 0f, 1f, 1f);
        AudioListener audioListener = cameras[c].GetComponent<AudioListener>();
        if (audioListener != null)
            audioListener.enabled = true;
    }

    public void Next()
    {
        currentCamera++;
        if (currentCamera >= cameras.Count)
            currentCamera = 0;
        Enable(currentCamera);
    }

    public void Previous()
    {
        currentCamera--;
        if (currentCamera < 0)
            currentCamera = cameras.Count - 1;
        Enable(currentCamera);
    }

    public void ShowAll()
    {
        EnableAllCameras();
        cameras[0].rect = new Rect(0f, 0f, 0.5f, 0.5f);
        cameras[1].rect = new Rect(0f, 0.5f, 0.5f, 1f);
        cameras[2].rect = new Rect(0.5f, 0f, 1f, 0.5f);
        cameras[3].rect = new Rect(0.5f, 0.5f, 1f, 1f);
    }
}
