using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
public class ZEDTracker
{

    const string nameDll = "sl_unitywrapper";
    public struct CamParameters
    {
        public float fx;
        public float fy;
        public float cx;
        public float cy;
        public double[] disto;
    };

    public struct StereoParameters
    {
        public float baseline;
        public float convergence;
        public float Rx;
        public float Rz;
        public CamParameters LeftCam;
        public CamParameters RightCam;
    };

    public enum ZED_SELF_CALIBRATION_STATUS
    {
        SELF_CALIBRATION_NOT_CALLED,
        SELF_CALIBRATION_RUNNING,
        SELF_CALIBRATION_FAILED,
        SELF_CALIBRATION_SUCCESS
    };

    public enum ZEDResolution_mode
    {
        HD2K,
        HD1080,
        HD720,
        VGA
    };

    public enum MODE
    {
        NONE,
        PERFORMANCE,
        MEDIUM,
        QUALITY
    };

    public enum UNIT
    {
        MILLIMETER,
        METER,
        INCH,
        FOOT
    };

    public enum ERRCODE
    {
        SUCCESS,
        NO_GPU_COMPATIBLE,
        NOT_ENOUGH_GPUMEM,
        ZED_NOT_AVAILABLE,
        ZED_SETTINGS_FILE_NOT_AVAILABLE,
        AUTO_CALIBRATION_FAILED,
        INVALID_SVO_FILE,
        RECORDER_ERROR
    };

    public enum SENSING_MODE
    {
        FILL,
        STANDARD,
        FUSED
    };

    public enum MAT_TRACKING_TYPE
    {
        PATH,
        POSE
    };

    public enum TRACKING_FRAME_STATE
    {
        TRACKING_FRAME_NORMAL, /*!< Not a keyframe, normal behavior \ingroup Enumerations*/
        TRACKING_FRAME_KEYFRAME, /*!< The tracking detect a new reference image \ingroup Enumerations*/
        TRACKING_FRAME_CLOSE, /*!< The tracking find a previously known area and optimize the trajectory\ingroup Enumerations*/
        TRACKING_FRAME_LAST
    }


    [DllImport(nameDll, EntryPoint = "setDebugMode")]
    private static extern void setDebugMode();

    [DllImport(nameDll, EntryPoint = "New_Camera")]
    private static extern void New_Camera(int mode = (int)ZEDResolution_mode.HD1080, float fps = 0.0f, int linux_id = 0);

    [DllImport(nameDll, EntryPoint = "destroy")]
    private static extern void destroy();

    [DllImport(nameDll, EntryPoint = "grab")]
    private static extern int grab(int sensingMode, int computeMeasure, int computeDisparity);

    [DllImport(nameDll, EntryPoint = "init")]
    private static extern int init(int mode_, int unit_,
    bool verbose_, int device_, float minDist_, bool disable, bool vflip_);

    [DllImport(nameDll, EntryPoint = "enableTracking")]
    private static extern bool enableTacking(float[] position, bool enableAreaLearning, string areaDBpath);

    [DllImport(nameDll, EntryPoint = "getPositionCamera")]
    private static extern int getPositionCamera(float[] position, int mat_type);

    [DllImport(nameDll, EntryPoint = "getTrackingFrameState")]
    private static extern int getTrackingFrameState();

    [DllImport(nameDll, EntryPoint = "saveAreaLearningDB")]
    private static extern bool saveAreaLearningDB(string areaDBpath);

    [DllImport(nameDll, EntryPoint = "getParameters")]
    private static extern IntPtr getParameters();

    [DllImport(nameDll, EntryPoint = "getSelfCalibrationStatus")]
    private static extern int getSelfCalibrationStatus();

    [DllImport(nameDll, EntryPoint = "stopTracking")]
    private static extern void stopTracking();

    [DllImport(nameDll, EntryPoint = "getTrackingConfidence")]
    private static extern float getTrackingConfidence();

    [DllImport(nameDll, EntryPoint = "getSDKVersion")]
    private static extern IntPtr getSDKVersion();

    [DllImport(nameDll, EntryPoint = "isZedConnected")]
    private static extern int isZedConnected();

    [DllImport(nameDll, EntryPoint = "getZEDSerial")]
    private static extern int getZEDSerial();

    [DllImport(nameDll, EntryPoint = "getZEDFirmware")]
    private static extern int getZEDFirmware();

    [DllImport(nameDll, EntryPoint = "resetSelfCalibration")]
    private static extern bool resetSelfCalibration();

    private StereoParameters param;

    private bool cameraIsReady = false;
    public static ZEDTracker instance = null;


    /// <summary>
    /// Gets an instance of camera
    /// </summary>
    public static ZEDTracker GetInstance()
    {
        if (instance == null)
        {
            instance = new ZEDTracker();
        }

        return instance;
    }



    /// <summary>
    /// Camera constructor. The ZEDResolution_mode sets the sensor
    ///resolution and defines the size of the output images, including the
    ///measures (disparity map, confidence map..).
    ///
    ///All computation is done on a CUDA capable device
    ///(That means that every CPU computation will need a memory retrieve
    ///of the images, which takes some time). If the performance is the main focus,
    ///all external computation should run on GPU. The retrieve*_gpu gives
    ///directly access to the gpu buffer.
    /// </summary>
    /// <param name="mode">the chosen ZED resolution</param>
    /// <param name="fps">a requested fps for this resolution. set as 0.0 will choose the default fps for this resolution ( see User guide)</param>
    /// <param name="linux_id">ONLY for LINUX : if multiple ZEDs are connected, it will choose the first zed listed (if zed_linux_id=0), the second listed (if zed_linux_id=1), ...
    ///  Each ZED will create its own memory (CPU and GPU), therefore the number of ZED available will depend on the configuration of your computer.
    /// Currently not available for Windows</param>
    public void CreateCamera(int mode = (int)ZEDResolution_mode.HD720, float fps = 0.0f, int linux_id = 0)
    {
        destroy();
        //setDebugMode();
        New_Camera(mode, fps, linux_id);
    }



    /// <summary>
    /// The init function must be called after the instantiation. The function checks if the ZED camera is plugged and opens it, if the graphics card is compatible, allocates the memory and launches the automatic calibration.
    /// </summary>
    /// <param name="mode_">defines the quality of the disparity map, affects the level of details and also the computation time.</param>
    /// <param name="unit_">define the unit metric for all the depth-distance values.</param>
    /// <param name="verbose_"> if set to true, it will output some information about the current status of initialization.</param>
    /// <param name="device_">defines the graphics card on which the computation will be done. The default value -1 search the more powerful usable GPU.</param>
    /// <param name="minDist_">specify the minimum depth information that will be computed, in the unit you previously define.</param>
    /// <param name="disable">if set to true, it will disable self-calibration and take the optional calibration parameters without optimizing them</param>
    /// <returns>ERRCODE : The error code gives information about the
    /// internal process, if SUCCESS is returned, the camera is ready to use.
    /// Every other code indicates an error and the program should be stopped.
    /// For more details see sl::zed::ERRCODE.</returns>
    public ERRCODE Init(MODE mode_ = MODE.PERFORMANCE, UNIT unit_ = UNIT.MILLIMETER, bool verbose_ = false, int device_ = -1, float minDist_ = -1, bool disable = false)
    {
        int v = init((int)mode_, (int)unit_, verbose_, device_, minDist_, disable, false);
        if (v == -1)
        {
            cameraIsReady = false;
            throw new ZEDException("Error init camera, no zed available");
        }
        cameraIsReady = true;
        return ((ERRCODE)v);
    }



    /// <summary>
    /// The function grabs a new image, rectifies it and computes the
    ///disparity map and optionally the depth map.
    ///The grabbing function is typically called in the main loop.
    /// </summary>
    /// <param name="sensingMode">defines the type of disparity map, more info : SENSING_MODE definition</param>
    /// <returns>the function returns false if no problem was encountered,
    /// true otherwise.</returns>
    public int Grab(SENSING_MODE sensingMode = SENSING_MODE.STANDARD)
    {
        if (!cameraIsReady)
            throw new ZEDException("Error init camera, no init called");
        return grab((int)sensingMode, Convert.ToInt32(true), Convert.ToInt32(true));
    }


    /// <summary>
    /// Initialize and Start the tracking functions
    /// </summary>
    /// <param name="position">position of the first camera, used as reference. By default it should be identity.</param>
    /// <param name="enableAreaLearning">define if the relocalization is enable or not.</param>
    /// <param name="areaDBpath"> define if and where a relocalization database from a previous run on the same scene has to be loaded.</param>
    /// <returns></returns>
    public bool EnableTracking(float[] position, bool enableAreaLearning = false, string areaDBpath = "")
    {
        if (!cameraIsReady)
            throw new ZEDException("Error init camera, no init called");

        return enableTacking(position, enableAreaLearning, areaDBpath);
    }

    /// <summary>
    ///  return the position of the camera and the current state of the Tracker
    /// </summary>
    /// <param name="position">the matrix containing the position of the camera</param>
    /// <param name="mat_type">define if the function return the path (the cumulate displacement of the camera) or juste the pose (the displacement from the previous position).</param>
    /// <returns></returns>
    public int GetPosition(float[] position, MAT_TRACKING_TYPE mat_type = MAT_TRACKING_TYPE.POSE)
    {
        if (!cameraIsReady) 
            throw new ZEDException("Error init camera, no init called");

        return getPositionCamera(position, (int)mat_type);
    }

    /// <summary>
    /// Get a quaternion from a matrix with a minimum size of 3x3
    /// </summary>
    /// <param name="m">The matrix </param>
    /// <returns>A quaternion which contains the rotation</returns>
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }


    /// <summary>
    /// Return a confidence metric of the tracking [0-100], 0 means that the tracking is lost
    /// </summary>
    /// <returns></returns>
    public float GetTrackingConfidence()
    {
        if (!cameraIsReady) 
            throw new ZEDException("Error init camera, no init called");
        return getTrackingConfidence();
    }


    /// <summary>
    /// stop the tracker, if you want to restart, call enableTracking()
    /// </summary>
    public void StopTracking()
    {
        if (!cameraIsReady) 
            throw new ZEDException("Error init camera, no init called");
        stopTracking();
    }

    /// <summary>
    /// return the state of the current tracked frame
    /// </summary>
    /// <returns>TRACKING_FRAME_STATE</returns>
    public TRACKING_FRAME_STATE GetTrackingFrameState()
    {
        if (!cameraIsReady) 
            throw new ZEDException("Error init camera, no init called");
        return ((TRACKING_FRAME_STATE)getTrackingFrameState());
    }


    /// <summary>
    /// Save the area learning information in a file.
    /// </summary>
    /// <param name="areaDBpath">the path to the file</param>
    /// <returns></returns>
    public bool SaveAreaLearningDB(string areaDBpath)
    {
        if (!cameraIsReady) throw new ZEDException("Error init camera, no init called");
        return saveAreaLearningDB(areaDBpath);
    }


    /// <summary>
    ///  StereoParameters pointer containing the intrinsic parameters of each camera
    ///  and the baseline (mm) and convergence (radian) of the ZED.
    /// </summary>
    /// <returns></returns>
    public StereoParameters GetParameters()
    {
        float[] v = new float[30];
        Marshal.Copy(getParameters(), v, 0, 30);
        param.baseline = v[0];
        param.convergence = v[1];
        param.Rx = v[2];
        param.Rz = v[3];
        param.LeftCam.fx = v[4];
        param.LeftCam.fy = v[5];
        param.LeftCam.cx = v[6];
        param.LeftCam.cy = v[7];
        param.LeftCam.disto = new double[5];
        for (int i = 0; i < 5; ++i)
        {
            param.LeftCam.disto[i] = v[8 + i];
        }
        param.RightCam.fx = v[14];
        param.RightCam.fy = v[15];
        param.RightCam.cx = v[16];
        param.RightCam.cy = v[17];
        param.RightCam.disto = new double[5];
        for (int i = 0; i < 5; ++i)
        {
            param.RightCam.disto[i] = v[18 + i];
        }
        return param;
    }


    /// <summary>
    /// Shut down the Zed
    /// </summary>
    public void Destroy()
    {
        cameraIsReady = false;
        destroy();
    }


    /// <summary>
    /// The function return the version of the currently installed ZED SDK
    /// </summary>
    /// <returns>ZED SDK version as a string with the following format : MAJOR.MINOR.PATCH</returns>
    public static string GetSDKVersion()
    {
        return PtrToStringUtf8(getSDKVersion());
    }

    /// <summary>
    /// The function checks if ZED cameras are connected, can be called before instantiating a Camera object
    /// </summary>
    /// <remarks> On Windows, only one ZED is accessible so this function will return 1 even if multiple ZED are connected.</remarks>
    /// <returns>the number of connected ZED</returns>
    public static bool IsZedConnected()
    {
        return Convert.ToBoolean(isZedConnected());
    }


    /// <summary>
    /// Gets the ZED Serial Number
    /// </summary>
    /// <returns>Returns the ZED Serial Number (as uint) (Live or SVO).</returns>
    public int GetZEDSerial()
    {
        return getZEDSerial();
    }


    /// <summary>
    /// Gets the zed firmware
    /// </summary>
    /// <returns></returns>
    public int GetZEDFirmware()
    {
        return getZEDFirmware();
    }


    /// <summary>
    ///  The reset function can be called at any time AFTER the Init function has been called.
    ///  It will reset and calculate again correction for misalignment, convergence and color mismatch.
    ///  It can be called after changing camera parameters without needing to restart your executable.
    /// </summary>
    ///
    /// <returns>ERRCODE : error boolean value : the function returns false if no problem was encountered,
    ///true otherwise.
    ///if no problem was encountered, the camera will use new parameters. Otherwise, it will be the old ones
    ///</returns>
    public bool ResetSelfCalibration()
    {
        return resetSelfCalibration();
    }


    /// <summary>
    /// Gets the status calibration
    /// </summary>
    /// <returns></returns>
    public ZED_SELF_CALIBRATION_STATUS GetSelfCalibrationStatus()
    {
        return (ZED_SELF_CALIBRATION_STATUS)getSelfCalibrationStatus();
    }

    private static string PtrToStringUtf8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
        {
            return "";
        }
        int len = 0;
        while (System.Runtime.InteropServices.Marshal.ReadByte(ptr, len) != 0)
            len++;
        if (len == 0)
        {
            return "";
        }
        byte[] array = new byte[len];
        System.Runtime.InteropServices.Marshal.Copy(ptr, array, 0, len);
        return System.Text.Encoding.ASCII.GetString(array);
    }

}
