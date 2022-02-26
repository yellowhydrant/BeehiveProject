using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderAll : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return JsonBlobAPI.GetJsonBlobCo(JsonBlobAPI.RegisterID);

        var register = JsonUtility.FromJson<Register>(JsonBlobAPI.json);

        for (int i = 0; i < register.submissions.Length; i++)
        {
            yield return JsonBlobAPI.GetJsonBlobCo(register.submissions[i].blobId);

            var drawing = JsonUtility.FromJson<DrawCanvasTab.DrawingData>(JsonBlobAPI.json);
            System.IO.File.WriteAllBytes($"Assets/Submissions/{register.submissions[i].blobId}.jpg", drawing.previewTexureJPGEncoded);
        }

        Debug.Break();
    }
}
