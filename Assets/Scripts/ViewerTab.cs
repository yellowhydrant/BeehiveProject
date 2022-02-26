using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewerTab : MonoBehaviour
{
    [SerializeField] LineRenderer brushStrokePrefab;
    [SerializeField] LoadingScreen loadingScreen;
    [SerializeField] TabSelection tabSelection;

    [SerializeField] Text submissionName;
    [SerializeField] Text creatorName;
    [SerializeField] Text creationDate;

    Register.Submission shownSubmission;
    DrawCanvasTab.DrawingData shownDrawing;

    Transform brushStrokeContainer;
    bool hasTriedFixingErrorByReiteration;

    private void Awake()
    {
        brushStrokeContainer = new GameObject("Viewer Brush Stroke Container").transform;
    }

    private void OnEnable()
    {
        if (shownDrawing == null)
        {
            ClearContainer();
            tabSelection.SwitchToTab(tabSelection.galleryTab.gameObject);
        }

        brushStrokeContainer.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (brushStrokeContainer != null)
        {
            brushStrokeContainer.gameObject.SetActive(false);
        }
    }

    public void ClearContainer()
    {
        foreach (Transform child in brushStrokeContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void LoadDrawing(Register.Submission submission, DrawCanvasTab.DrawingData drawing)
    {
        shownDrawing = drawing;
        shownSubmission = submission;
        tabSelection.SwitchToTab(gameObject);
        StartCoroutine(LoadDrawingCo(submission, drawing));
    }

    IEnumerator LoadDrawingCo(Register.Submission submission, DrawCanvasTab.DrawingData drawing)
    {
        ClearContainer();

        if (drawing == null)
            yield break;

        loadingScreen.Show($"Loading \n \"{submission.submissionName}\"", Color.white, true);

        //yield return JsonBlobAPI.GetJsonBlobCo(drawing.blobId);
        //if (JsonBlobAPI.responseCode != 200)
        //{
        //    loadingScreen.ChangeMessage("Database entry fetch error!", Color.red, false);
        //    yield return new WaitForSeconds(1f);
        //    loadingScreen.ChangeMessage("Re-trying fetching! In 3 seconds, re-open the app or contact the developer if the error keeps occuring!", Color.red, false);
        //    yield return new WaitForSeconds(3f);

        //    if (!hasTriedFixingErrorByReiteration)
        //    {
        //        hasTriedFixingErrorByReiteration = true;
        //        yield return LoadSubmissionCo(drawing);
        //        yield break;
        //    }
        //    else
        //    {
        //        tabSelection.SwitchToTab(tabSelection.drawCanvasTab.gameObject);
        //    }
        //}

        //var drawing = JsonUtility.FromJson<DrawCanvasTab.DrawingData>(JsonBlobAPI.json);

        //for (int i = 0; i < drawing.strokes.Length; i++)
        for (int i = drawing.strokes.Length - 1; i >= 0; i--)
        {
            var renderer = Instantiate(brushStrokePrefab, brushStrokeContainer);
            renderer.startColor = drawing.strokes[i].startColor;
            renderer.endColor = drawing.strokes[i].endColor;
            renderer.startWidth = drawing.strokes[i].startSize;
            renderer.endWidth = drawing.strokes[i].endSize;
            renderer.positionCount = drawing.strokes[i].points.Length;
            for (int j = 0; j < drawing.strokes[i].points.Length; j++)
                renderer.SetPosition(j, drawing.strokes[i].points[j]);
        }

        submissionName.text = string.IsNullOrEmpty(submission.submissionName) ? $"Bee #{Random.Range(0, 1000)}" : submission.submissionName;
        creatorName.text = string.Format("Made by {0}", string.IsNullOrEmpty(submission.creatorName) ? "anonymous" : submission.creatorName);
        creationDate.text = string.Format("Creation date: {0}", submission.creationDate);

        loadingScreen.Hide();
    }

    [ContextMenu("screenshot")]
    void Screenshot()
    {
        StartCoroutine(ScreenshotCo());
    }

    IEnumerator ScreenshotCo()
    {
        yield return new WaitForEndOfFrame();

        ScreenCapture.CaptureScreenshot("Assets/ViewScreenshot.png");
    }

    public void RemoveSubmission(string password)
    {
        if (password.ToLower() == "lemonhydrant" || (!string.IsNullOrEmpty(shownSubmission.removalPassword) && password.ToLower() == shownSubmission.removalPassword.ToLower()))
            StartCoroutine(RemoveSubmissionCo());
        else
            StartCoroutine(CantRemoveSubmissionCo());
    }

    IEnumerator RemoveSubmissionCo()
    {
        loadingScreen.Show("Removing", Color.white, true);
        yield return JsonBlobAPI.DeleteJsonBlobCo(shownSubmission.blobId);

        yield return JsonBlobAPI.GetJsonBlobCo(JsonBlobAPI.RegisterID);
        var register = JsonUtility.FromJson<Register>(JsonBlobAPI.json);
        var list = new List<Register.Submission>(register.submissions);
        list.Remove(list.Find((x) => x.blobId == shownSubmission.blobId));
        register.submissions = list.ToArray();
        yield return JsonBlobAPI.SetJsonBlobCo(JsonBlobAPI.RegisterID, JsonUtility.ToJson(register));

        shownDrawing = null;
        shownSubmission = null;
        ClearContainer();
        loadingScreen.Hide();
        tabSelection.SwitchToTab(tabSelection.galleryTab.gameObject);
        
    }

    IEnumerator CantRemoveSubmissionCo()
    {
        loadingScreen.Show("Error: The provided password doesn't match the password associated with this contribution! " +
            "Can't delete contribution! \n " +
            "If you forgot the password or have a valid reason for the removal of this contribution please contact the developer!", Color.red, false);
        yield return new WaitForSeconds(4f);
        loadingScreen.Hide();
    }
}
