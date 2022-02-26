using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabSelection : MonoBehaviour
{
    public DrawCanvasTab drawCanvasTab;
    public ViewerTab viewerTab;
    public GalleryTab galleryTab;

    [SerializeField] GameObject[] tabs;

    public void SwitchToTab(GameObject newTab)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }
        newTab.SetActive(true);
    }
}
