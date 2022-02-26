using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryTab : MonoBehaviour
{
    [SerializeField] TabSelection tabSelection;

    [SerializeField] RawImage submissionRawImagePrefab;
    [SerializeField] RectTransform contentRect;
    [SerializeField, Range(10, 100)] float fillRate;

    Texture2D[] downloadedTextures;

    private void OnEnable()
    {
        StartCoroutine(LoadAllSubmissions());
    }

    IEnumerator LoadAllSubmissions()
    {
        yield return JsonBlobAPI.GetJsonBlobCo(JsonBlobAPI.RegisterID);

        var register = JsonUtility.FromJson<Register>(JsonBlobAPI.json);

        //Mathf.CeilToInt(25 * (1f + (1 - fillRate / 100f)));
        var gridSize = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(register.submissions.Length) * (1f + (1 - fillRate / 100f))));
        var sizeDelta = (contentRect.rect.height / gridSize) * Vector2.one;

        var container = contentRect;
        container.sizeDelta = sizeDelta * (Vector2.one * gridSize);

        var gridCells = new RawImage[gridSize * gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var image = Instantiate(submissionRawImagePrefab, container);
                image.rectTransform.sizeDelta = sizeDelta;
                image.rectTransform.anchoredPosition = (sizeDelta * new Vector2(x, y)) - (container.sizeDelta / 2 - sizeDelta / 2);
                image.GetComponent<Button>().interactable = false;
                gridCells[x + gridSize * y] = image;
            }
        }

        downloadedTextures = new Texture2D[register.submissions.Length];
        var rIndex = 0;
        for (int i = 0; i < gridCells.Length; i++)
        {
            if(fillRate > Random.Range(0f, 100f))
            {
                yield return JsonBlobAPI.GetJsonBlobCo(register.submissions[rIndex].blobId);

                var drawing = JsonUtility.FromJson<DrawCanvasTab.DrawingData>(JsonBlobAPI.json);
                var tex = new Texture2D(drawing.textureSize.x, drawing.textureSize.y);
                tex.LoadImage(drawing.previewTexureJPGEncoded);
                tex.Apply();
                downloadedTextures[rIndex] = tex;

                gridCells[i].texture = tex;
                var but = gridCells[i].GetComponent<Button>();
                but.interactable = true;
                var index = rIndex;
                but.onClick.AddListener(() => 
                {
                    tabSelection.viewerTab.LoadDrawing(register.submissions[index], drawing);
                });


                rIndex++;
                if (rIndex == register.submissions.Length)
                    break;
            }
        }

//#if UNITY_EDITOR
//        var eof = new WaitForEndOfFrame();
//        for (int i = 0; i < 4; i++)
//        {
//            yield return eof;
//        }
//        ScreenCapture.CaptureScreenshot("Assets/Submissions/AllInOne.png");
//#endif
    }

    private void OnDisable()
    {
        if (contentRect != null)
            foreach (Transform child in contentRect)
                Destroy(child.gameObject);

        if (downloadedTextures != null)
            for (int i = 0; i < downloadedTextures.Length; i++)
                Destroy(downloadedTextures[i]);

    }
}
