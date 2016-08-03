using UnityEngine;
using System.Collections;

public class ZEDCamera : MonoBehaviour
{

    private ZEDTracker zed;
    private float[] path;
    private Matrix4x4 Rt;

    public ZEDTracker.UNIT unit = ZEDTracker.UNIT.METER;

    void Start()
    {
        if (!ZEDTracker.IsZedConnected()) return;

        zed = ZEDTracker.GetInstance();
        zed.CreateCamera();

        Debug.Log("SDK Version " + ZEDTracker.GetSDKVersion());
        ZEDTracker.ERRCODE e = zed.Init(ZEDTracker.MODE.PERFORMANCE, unit);
        if (e != ZEDTracker.ERRCODE.SUCCESS)
        {
            throw new ZEDException("Error, initialization failed " + e.ToString());
        }

        path = IdentityMatrix();

        bool tracking = zed.EnableTracking(path, true);
        if (!tracking)
            throw new ZEDException("Error, tracking not available");
    }

    private float[] IdentityMatrix()
    {
        float[] identityMatrix = new float[16];

        for (int i = 0; i < 16; ++i)
        {
            identityMatrix[i] = 0;
        }
        identityMatrix[0] = identityMatrix[5] = identityMatrix[10] = identityMatrix[15] = 1;
        return identityMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        if (zed != null)
        {
            zed.Grab();
            zed.GetPosition(path, ZEDTracker.MAT_TRACKING_TYPE.PATH);
            
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    Rt[i, j] = path[i * 4 + j];
                }
            }

            Vector4 t_ = Rt.GetColumn(3);
            Vector3 translation = new Vector3(t_.x, t_.y, t_.z);
            Quaternion rotation = ZEDTracker.QuaternionFromMatrix(Rt);

            transform.localRotation = rotation;
            transform.localPosition = translation;
        }
    }

    void OnApplicationQuit()
    {
        if (zed != null)
            zed.Destroy();
    }

}
