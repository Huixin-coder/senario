using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHorizonalFlipper : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
//         Camera cam = this.gameObject.GetComponent<Camera>();
//         Matrix4x4 mat = cam.projectionMatrix;
//         mat *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
//         cam.projectionMatrix = mat;

//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }

 {
     void OnPreCull()
     {
         Camera m_camera = this.gameObject.GetComponent<Camera>();
         m_camera.ResetWorldToCameraMatrix();
         m_camera.ResetProjectionMatrix();
         m_camera.projectionMatrix = m_camera.projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
     }
 
     void OnPreRender()
     {
         GL.invertCulling = true;
     }
 
     void OnPostRender()
     {
         GL.invertCulling = false;
     }
     
 }
