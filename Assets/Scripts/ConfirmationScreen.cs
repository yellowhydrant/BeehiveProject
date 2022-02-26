using UnityEngine;
using UnityEngine.UI;

public class ConfirmationScreen : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] InputField submissionName;
    [SerializeField] InputField creatorName;
    [SerializeField] InputField removalPassword;
    [SerializeField] LayerMask confirmationMask;

    LayerMask normalMask;

    private void Awake()
    {
        normalMask = cam.cullingMask;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        cam.cullingMask = confirmationMask;
        if(submissionName != null)
            submissionName.text = null;
        gameObject.SetActive(true);
        canvasGroup.interactable = false;
    }

    public void Hide()
    {
        cam.cullingMask = normalMask;
        gameObject.SetActive(false);
        canvasGroup.interactable = true;
    }

    public void OnSubmit(ViewerTab viewer)
    {
        Hide();
        viewer.RemoveSubmission(removalPassword.text);
    }

    public void Show(DrawCanvasTab canvas)
    {
        Show();
        canvas.permissionAquired = false;
        canvas.canDraw = false;
    }

    public void OnSubmit(DrawCanvasTab canvas)
    {
        canvas.creatorName = creatorName.text;
        canvas.submissionName = submissionName.text;
        canvas.removalPassword = removalPassword.text;
        canvas.permissionAquired = true;
        Hide(canvas);
        canvas.OnUpload();
    }

    public void Hide(DrawCanvasTab canvas)
    {
        Hide();
        canvas.canDraw = true;
    }
}