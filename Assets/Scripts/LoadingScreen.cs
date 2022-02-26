using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Text text;
    [SerializeField] LayerMask loadingMask;

    LayerMask normalMask;
    Coroutine textAnimation;

    void Awake()
    {
        normalMask = cam.cullingMask;
        gameObject.SetActive(false);
    }

    public void Show(string msg, Color color, bool animate = true)
    {
        gameObject.SetActive(true);
        cam.cullingMask = loadingMask;
        ChangeMessage(msg, color, animate);
    }

    public void ChangeMessage(string msg, Color color, bool animate = true)
    {
        if(textAnimation != null)
            StopCoroutine(textAnimation);
        text.text = msg;
        text.color = color;
        if(animate)
            textAnimation = StartCoroutine(AnimateTextCo(msg));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        cam.cullingMask = normalMask;
        if(textAnimation != null)
            StopCoroutine(textAnimation);
    }

    IEnumerator AnimateTextCo(string msg)
    {
        var frames = new string[] { msg, msg + ".", msg + "..", msg + "..." };
        var frame = -1;
        var frameWait = new WaitForSeconds(1f / 2);
        while (true)
        {
            frame++;
            if (frame == frames.Length)
                frame = 0;
            text.text = frames[frame];
            yield return frameWait;
        }
    }
}
