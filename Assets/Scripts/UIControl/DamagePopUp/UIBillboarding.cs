using UnityEngine;

public class UIBillboarding : MonoBehaviour
{
    private Camera cam;
    private void Awake()
    {
        cam = Camera.main; 
    }

    // Update is called once per frame
    private void Update()
    {
        if ( cam == null) return;
        transform.forward = cam.transform.forward; 
    }
}
