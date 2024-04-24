using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class spriteChange : MonoBehaviour
{

    public Image oldImage;
    public Sprite replace;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ImageChange()
    {
        oldImage.sprite = replace; 
    }
}
