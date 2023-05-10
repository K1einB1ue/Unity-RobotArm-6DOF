using System;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class KRandCam : MonoBehaviour
{
    public string saveDepthPath;
    public string saveImagePath;
    public string saveName;
    public int saveIndex;
    public Camera cam;
    public Vector2 randomXRange;
    public Vector2 randomYRange;
    public Vector2 randomZRange;
    public bool randomPosition;
    public bool randomRotation;
    public bool checkInScreen;
    public UnityEvent onScreenOut = new();
    private bool _sampleFlag = false;
    private Texture2D _depth;
    private Texture2D _image;
    private RenderTexture _camRT;
    private readonly StringBuilder _detail = new();


    public StringBuilder Detail => _detail;
    
    private void Awake()
    {
        cam.depthTextureMode |= DepthTextureMode.Depth;
        RenderPipelineManager.endCameraRendering += (context,came) =>
        {
            if (!ReferenceEquals(came, cam)) return;
            if (!_sampleFlag) return;
            var width = cam.pixelWidth;
            var height = cam.pixelHeight;
            var depth = Shader.GetGlobalTexture("_CameraDepthTexture");
            _depth = depth.ToTexture2D(width, height, GraphicsFormat.R32_SFloat);
            _image = _camRT.ToTexture2D(width, height, GraphicsFormat.R32G32B32_SFloat);
            if (_detail.Length == 0)
            {
                ImageExport.SavePNG(_depth, saveDepthPath, $"{saveName}_{saveIndex}");
                ImageExport.SavePNG(_image, saveImagePath, $"{saveName}_{saveIndex}");
            }
            else
            {
                ImageExport.SavePNG(_depth, saveDepthPath, $"{saveName}_{saveIndex},{_detail}");
                ImageExport.SavePNG(_image, saveImagePath, $"{saveName}_{saveIndex},{_detail}");
            }
            _detail.Clear();
            saveIndex++;
            _sampleFlag = false;
        };
        _camRT = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 32);
        cam.targetTexture = _camRT;
    }

    public bool RandomTransform(Transform goTran)
    {
        var camTran = cam.transform;
        if (randomPosition)
        {
            var goPos = goTran.position;
            var camX = goPos.x + Random.Range(randomXRange.x, randomXRange.y);
            var camY = goPos.y + Random.Range(randomYRange.x, randomYRange.y);
            var camZ = goPos.z + Random.Range(randomZRange.x, randomZRange.y);
            camTran.position = new Vector3(camX, camY, camZ);
            
        }
        if (randomRotation)
        {
            camTran.LookAt(goTran);
            var camVertAngle = cam.fieldOfView;
            var camHoriAngle = (camVertAngle / cam.pixelHeight) * cam.pixelWidth;
            var camZRot = Random.Range(-180f, 180f);
            var camYRot = Random.Range(-camHoriAngle, camHoriAngle);
            var camXRot = Random.Range(-camVertAngle, camVertAngle);
            //由于Unity使用YXZ转序(EulerAngle),而此处如果要让相机视野看到物体,则需要使用ZYX/ZXY转序(EulerAngle).
            //所以不能直接使用Quaternion.Euler一步到位.需要四元数连乘.
            var camRot = camTran.rotation;
            var rotAxis = camRot * Vector3.forward;
            var rotZ = Quaternion.AngleAxis(camZRot, rotAxis);
            camRot = rotZ * camRot;
            rotAxis = camRot * Vector3.up;
            var rotY = Quaternion.AngleAxis(camYRot, rotAxis);
            camRot = rotY * camRot;
            rotAxis = camRot * Vector3.right;
            var rotX = Quaternion.AngleAxis(camXRot, rotAxis);
            camTran.rotation = rotX * camRot;
        }
        if (checkInScreen)
        {
            var pos = cam.WorldToScreenPoint(goTran.position);
            if (pos.x < 0 || pos.x > cam.pixelWidth)
            {
                onScreenOut?.Invoke();
                return false;
            }
            if (pos.y < 0 || pos.y > cam.pixelHeight)
            {
                onScreenOut?.Invoke();
                return false;
            }
        }

        return true;
    }
    

    public void Capture()
    {
        _sampleFlag = true;
        cam.Render();
    }
}
