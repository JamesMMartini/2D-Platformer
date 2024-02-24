using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailFade : MonoBehaviour
{
    [SerializeField] float fadeSpeed;
    SpriteRenderer image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Color newCol = image.color;
        newCol.a -= fadeSpeed * Time.deltaTime;

        if (newCol.a <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            image.color = newCol;
        }
    }
}
