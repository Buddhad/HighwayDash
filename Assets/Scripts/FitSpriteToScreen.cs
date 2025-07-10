using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class FitSpriteToScreen : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        float worldHeight = Camera.main.orthographicSize * 2f;
        float worldWidth = worldHeight * Screen.width / Screen.height;

        transform.localScale = new Vector3(
            worldWidth / sr.bounds.size.x,
            worldHeight / sr.bounds.size.y,
            1);
    }
}

