using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetWidthUniform : MonoBehaviour
{
    [SerializeField, Range(0.001f, .05f)] float size = .025f;
    LineRenderer rend;

    private void Awake()
    {
        rend = (LineRenderer)GetComponent(typeof(LineRenderer));
    }
    private void Update()
    {
        rend.startWidth = size;
        rend.endWidth = size;
    }
}
