using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkingTest : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return JsonBlobAPI.SetJsonBlobCo(JsonBlobAPI.RegisterID, JsonUtility.ToJson(new Register { submissions = System.Array.Empty<Register.Submission>()}));
    }
}
