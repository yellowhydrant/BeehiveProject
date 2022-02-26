using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DrawCanvasTab : MonoBehaviour
{
    public string creatorName;
    public string submissionName;
    public string removalPassword;
    public bool permissionAquired;
    public bool canDraw = true;

    [SerializeField] Camera cam;
    [SerializeField] RectTransform canvas;

    [SerializeField] LineRenderer brushStrokePrefab;
    [SerializeField] float maxDistanceDelta = .05f;

    [SerializeField] LineRenderer brushStrokePreview;

    [SerializeField] SizeOption defaultStartSize;
    [SerializeField] SizeOption defaultEndSize;
    [SerializeField] Button defaultStartColor;
    [SerializeField] Button defaultEndColor;

    [SerializeField] LoadingScreen loadingScreen;
    [SerializeField] ConfirmationScreen confirmationScreen;

    [SerializeField] Camera previewCam;

    Vector2 lastPos;
    LineRenderer currentBrushStroke;
    Transform brushStrokeContainer;

    bool hasTriedFixingErrorByReiteration;

    public static Vector2Int previewSize { get; } = new Vector2Int(128, 128);

    private void Awake()
    {
        defaultStartSize.button.onClick.Invoke();
        defaultEndSize.button.onClick.Invoke();
        defaultStartColor.onClick.Invoke();
        defaultEndColor.onClick.Invoke();
        brushStrokeContainer = new GameObject("Brush Stroke Container").transform;
    }

    private void OnDisable()
    {
        if(brushStrokeContainer != null)
            brushStrokeContainer.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        brushStrokeContainer.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (canDraw && RectTransformUtility.RectangleContainsScreenPoint(canvas, Input.mousePosition, cam))
        {
            Draw();
        }
    }

    //Drawing
    void Draw()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartNewStroke();
        }
        if (Input.GetMouseButton(0))
        {
            var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(mousePos, lastPos) >= maxDistanceDelta)
            {
                AddPoint(mousePos);
                lastPos = mousePos;
            }
        }
        else
        {
            currentBrushStroke = null;
        }
    }

    void StartNewStroke()
    {
        var renderer = Instantiate(brushStrokePrefab, brushStrokeContainer);
        currentBrushStroke = renderer;

        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        currentBrushStroke.SetPosition(0, mousePos);
        currentBrushStroke.SetPosition(1, mousePos);
    }

    void AddPoint(Vector2 point)
    {
        if (currentBrushStroke == null)
            return;
        currentBrushStroke.positionCount++;
        currentBrushStroke.SetPosition(currentBrushStroke.positionCount - 1, point);
    }

    public void Backtrack()
    {
        if (brushStrokeContainer.childCount > 0)
            Destroy(brushStrokeContainer.GetChild(brushStrokeContainer.childCount - 1).gameObject);
    }

    //Export

    public void OnUpload()
    {
        if (permissionAquired)
            StartCoroutine(OnUploadCo());
        else
            confirmationScreen.Show(this);
    }

    public IEnumerator OnUploadCo()
    {
        loadingScreen.Show("Uploading", Color.white, true);

        var previewTex = RenderPreview();

        var data = new DrawingData();
        //previewTex = ScaleTexture(previewTex, previewSize.x, previewSize.y);
        data.previewTexureJPGEncoded = previewTex.EncodeToJPG();
        data.textureSize = new Vector2Int(previewTex.width, previewTex.height);

        data.strokes = new DrawingData.Stroke[brushStrokeContainer.childCount];
        for (int i = 0; i < data.strokes.Length; i++)
        {
            var stroke = new DrawingData.Stroke();

            var renderer = (LineRenderer)brushStrokeContainer.GetChild(i).GetComponent(typeof(LineRenderer));

            stroke.startColor = renderer.startColor;
            stroke.endColor = renderer.endColor;
            stroke.startSize = renderer.startWidth;
            stroke.endSize = renderer.endWidth;

            var positions = new Vector3[renderer.positionCount];
            renderer.GetPositions(positions);
            stroke.points = new Vector2[positions.Length];
            for (int j = 0; j < positions.Length; j++)
                stroke.points[j] = positions[j];

            data.strokes[i] = stroke;
        }

        var dataJson = JsonUtility.ToJson(data);
        yield return JsonBlobAPI.CreateJsonBlobCo(dataJson);
        if(JsonBlobAPI.responseCode != 201)
        {
            loadingScreen.ChangeMessage("Database entry creation error!", Color.red, false);
            yield return new WaitForSeconds(1f);
            loadingScreen.ChangeMessage("Re-trying creation! In 3 seconds, re-open the app or contact the developer if the error keeps occuring!", Color.red, false);
            yield return new WaitForSeconds(3f);

            if (!hasTriedFixingErrorByReiteration)
            {
                hasTriedFixingErrorByReiteration = true;
                yield return OnUploadCo();
                yield break;
            }
            else
            {
                Application.Quit();
            }
        }

        yield return JsonBlobAPI.GetJsonBlobCo(JsonBlobAPI.RegisterID);
        if (JsonBlobAPI.responseCode != 200)
        {
            loadingScreen.ChangeMessage("Register fetch error!", Color.red, false);
            yield return new WaitForSeconds(1f);
            loadingScreen.ChangeMessage("Re-trying fetch! In 3 seconds, re-open the app or contact the developer if the error keeps occuring!", Color.red, false);
            yield return new WaitForSeconds(3f);

            if (!hasTriedFixingErrorByReiteration)
            {
                hasTriedFixingErrorByReiteration = true;
                yield return OnUploadCo();
                yield break;
            }
            else
            {
                Application.Quit();
            }
        }

        var register = JsonUtility.FromJson<Register>(JsonBlobAPI.json);
        var list = new System.Collections.Generic.List<Register.Submission>(register.submissions);
        var date = System.DateTime.Now;
        list.Add(new Register.Submission {blobId = JsonBlobAPI.blobID, creationDate = $"{date.Day}-{date.Month}-{date.Year}", creatorName = creatorName, submissionName = submissionName, removalPassword = removalPassword });
        register.submissions = list.ToArray();

        var registerJson = JsonUtility.ToJson(register);
        yield return JsonBlobAPI.SetJsonBlobCo(JsonBlobAPI.RegisterID, registerJson);
        if (JsonBlobAPI.responseCode != 200)
        {
            loadingScreen.ChangeMessage("Register set error!", Color.red, false);
            yield return new WaitForSeconds(1f);
            loadingScreen.ChangeMessage("Re-trying set! In 3 seconds, re-open the app or contact the developer if the error keeps occuring!", Color.red, false);
            yield return new WaitForSeconds(3f);

            if (!hasTriedFixingErrorByReiteration)
            {
                hasTriedFixingErrorByReiteration = true;
                yield return OnUploadCo();
                yield break;
            }
            else
            {
                Application.Quit();
            }
        }

        ClearContainer();

        hasTriedFixingErrorByReiteration = false;
        Destroy(previewTex);
        permissionAquired = false;
        loadingScreen.Hide();
    }

    public void ClearContainer()
    {
        foreach (Transform child in brushStrokeContainer)
        {
            Destroy(child.gameObject);
        }
    }
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }

    //[System.Runtime.InteropServices.DllImport("__Internal")]
    //private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

    public void OnDownload()
    {
        //RenderPreview();
        //var imageData = previewTex.EncodeToJPG(); //screenShot is Texture2D
        //var customFileName = "bee.jpg";
        //DownloadFile(imageData, imageData.Length, customFileName);
    }

    public Texture2D RenderPreview()
    {
        var canvasSize = new Vector2Int(312, 312);
        var currentTarget = previewCam.targetTexture;
        previewCam.targetTexture = RenderTexture.GetTemporary(canvasSize.x, canvasSize.y);

        var currentRT = RenderTexture.active;
        RenderTexture.active = previewCam.targetTexture;

        var previewTex = new Texture2D(canvasSize.x, canvasSize.y);

        previewCam.Render();

        previewTex.ReadPixels(new Rect(0, 0, canvasSize.x, canvasSize.y), 0, 0);
        previewTex.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(previewCam.targetTexture);
        previewCam.targetTexture = currentTarget;

        return previewTex;
    }

    [System.Serializable]
    public class DrawingData
    {
        public Vector2Int textureSize;
        public byte[] previewTexureJPGEncoded;
        public Stroke[] strokes;

        [System.Serializable]
        public class Stroke
        {
            public Color startColor;
            public Color endColor;
            public float startSize;
            public float endSize;
            public Vector2[] points;
        }
    }

    //Brush Settings
    public void SetBrushStartColor(Image colorOrigin)
    {
        var color = colorOrigin.color;
        brushStrokePrefab.startColor = color;
        brushStrokePreview.startColor = color;
    }

    public void SetBrushEndColor(Image colorOrigin)
    {
        var color = colorOrigin.color;
        brushStrokePrefab.endColor = color;
        brushStrokePreview.endColor = color;
    }

    public void SetBrushStartSize(SizeOption option)
    {
        brushStrokePrefab.startWidth = option.size;
        brushStrokePreview.startWidth = option.size;
    }

    public void SetBrushEndSize(SizeOption option)
    {
        brushStrokePrefab.endWidth = option.size;
        brushStrokePreview.endWidth = option.size;
    }
}
