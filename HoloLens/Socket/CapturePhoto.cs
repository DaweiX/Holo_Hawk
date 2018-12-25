//#define VISIONONLY
#define IS_PC_NEEDED
//#define TEST
#define RECT
/*-------------------------------------------------------------
 *                  HoloLens 视频通信
 *                   CODE BY DAWEIX
 *                      2018.1.26
 *              功能：捕获并实时发送图像帧
 *              
 * ------------------------------------------------------------*/


// 后续工作：log 文件
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using Helpers;
using System.Text;
using System.Collections;
using System.Threading;
//using HoloLensCameraStream;

#if WINDOWS_UWP
using Windows.Storage.Streams;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Foundation;
using Windows.Networking.Sockets;
#endif


public class CapturePhoto : MonoBehaviour
{
    #region VARIABLES_STATING

    // 网络通信
    const string HOSTIP_TX2 = "192.168.0.80";
    const string HOSTIP_PC = "192.168.0.90";

    //bool ShowHoloGrams = false;

    public static bool isSendWarning = false;
    
    public const string PORT_TX2 = "11000";
    public const string PORT_PC = "8888";


    // 状态锁

    //private bool isSending = false;
    private bool isTextInited = false;
    private System.Object lock_timer_task = new System.Object();

    // 照相机
    PhotoCapture photo_capture = null;
    private Resolution camera_resolution;
    private CameraParameters camera_parameters;

    // 在线程间同步数据
    Matrix4x4 martrix_camera_to_world, martrix_projection;
    private System.Object lock_image = new System.Object();
    int num = 0;

#if WINDOWS_UWP
    // 网络组件
    private StreamSocket socket_vstream;
    private StreamSocket socket_pc;

    private DataReader reader_receiving;
    private DataWriter writer_pc;

    private DataWriter writer_vstream;

    StreamSocketListener listener_ai = new StreamSocketListener();
#endif

    // 标记
    public GameObject Parent;
    public LayerMask raycastLayer;
    public GameObject annotationText;
    public GameObject rec;
    public GameObject info;

    #endregion

    #region UNITY_FUNCTION
#if WINDOWS_UWP

    void Awake()
    {
        UnityThread.initUnityThread();
    }

    /// <summary>
    /// 初始化（照相机、网络、UI）
    /// </summary>
    async void Start()
    {
        MyLog.DebugLog("程序入口");
        ShowMsg.UpdateCubeMsg("欢迎");

#if !VISIONONLY
        GameObject.Find("Main Camera/GameObject/Canvas_MAIN").SetActive(true);
#endif
        camera_resolution = PhotoCapture.SupportedResolutions.OrderBy(o => o.height).First();
        /*--------------
        896 x 504 @ 0Hz
        1280 x 720 @ 0Hz
        1344 x 756 @ 0Hz
        1408 x 792 @ 0Hz
        2048 x 1152 @ 0Hz
        ---------------*/
        MyLog.DebugLog(camera_resolution.ToString());
        camera_parameters = new CameraParameters
        {
            hologramOpacity = 1f,
            // 此处选用的格式与openCV对应即可
            pixelFormat = CapturePixelFormat.JPEG,
            cameraResolutionHeight = camera_resolution.height,
            cameraResolutionWidth = camera_resolution.width,        
        };

        //if (ShowMsg.block == null)
        //{
        //    string path = "Main Camera/GameObject/Canvas_MAIN/Canvas_Popup/msg";
        //    ShowMsg.block = GameObject.Find(path).GetComponent<Text>();
        //}


//#if !TEST
        Debug.Log("picture包含");
        await InitializeNetworkAsync();
        RevDetectionsHeaderAsync();
        //#endif
        ShowMsg.ChangeColor(ShowMsg.MyIcons.vision);
    }

    void Update()
    {
        if (!isTextInited) return;
        var t = DateTime.Now.ToString();

        MyLog.Log(t.Remove(t.Length - 2));
    }

#else

    void Start()
    {
        isTextInited = MyLog.Init();
        ShowMsg.ChangeColor(ShowMsg.MyIcons.gps);
    }

    void Update()
    {
        // 显示当前时间
        if (!isTextInited) return;
        var t = DateTime.Now.ToString();
        Helpers.MyLog.Log(t.Remove(t.Length - 2));
    }

#endif
    #endregion

    #region PHOTO_CAPTURE_HANDLER

    /// <summary>
    /// 创建相机对象后调用（仅一次）
    /// </summary>
    void OnCaptureCreated_HOLO(PhotoCapture capture)
    {
        Debug.Log("相机对象创建完毕");
        photo_capture = capture;
        photo_capture.StartPhotoModeAsync(camera_parameters, OnPhotoModeStarted_HOLO);
    }

    /// <summary>
    /// 开始照相模式后调用
    /// </summary>
    void OnPhotoModeStarted_HOLO(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("照相模式开始");
            photo_capture.TakePhotoAsync(OnProcessFrame);
        }
        else
        {
            Debug.Log("开启照相模式失败");
        }
    }

    /// <summary>
    /// 照相完毕后调用
    /// </summary>
    /// <param name="result">结果</param>
    /// <param name="photoCaptureFrame">帧</param>
    private void OnProcessFrame(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        Debug.Log("OnProcessFrame");
        if (result.success)
        {
            List<byte> imageBufferList = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            photoCaptureFrame.TryGetCameraToWorldMatrix(out martrix_camera_to_world);
            photoCaptureFrame.TryGetProjectionMatrix(out martrix_projection);
            //photoCaptureFrame.Dispose();
#if WINDOWS_UWP
            SendData(imageBufferList.ToArray());
            //ShowHoloGrams = !ShowHoloGrams;
#endif
            photo_capture.TakePhotoAsync(OnProcessFrame);
        }
    }


    /// <summary>
    /// 停止照相模式后调用
    /// </summary>
    private void OnStoppedPhotoMode_HOLO(PhotoCapture.PhotoCaptureResult result)
    {

    }

    #endregion

    #region NETWORK

#if WINDOWS_UWP

    /// <summary>
    /// 初始化 Socket 通信
    /// </summary>
    async Task InitializeNetworkAsync()
    {
        Debug.Log("初始化网络");
        HostName serverHost = new HostName(HOSTIP_TX2);


        socket_vstream = new StreamSocket();
        try
        {
            await socket_vstream.ConnectAsync(serverHost, PORT_TX2);
            writer_vstream = new DataWriter(socket_vstream.OutputStream)
            {
                ByteOrder = ByteOrder.LittleEndian,
            };
            reader_receiving = new DataReader(socket_vstream.InputStream)
            {
                ByteOrder = ByteOrder.LittleEndian
            };

            Debug.Log("视觉模块连接就绪,准备接收数据");
            ShowMsg.UpdateCubeMsg("就绪");
            PhotoCapture.CreateAsync(true, OnCaptureCreated_HOLO);

#if IS_PC_NEEDED
            await InitNetForPC();
#endif

        }
        catch (Exception e)
        {
            MyLog.DebugLog("初始化网络连接错误" + e.Message);
            await InitializeNetworkAsync();
        }
    }

    async Task InitNetForPC()
    {
        HostName serverHost_pc = new HostName(HOSTIP_PC);

        socket_pc = new StreamSocket();
        try
        {
            await socket_pc.ConnectAsync(serverHost_pc, PORT_PC);
            writer_pc = new DataWriter(socket_pc.OutputStream);
            Debug.Log("与PC连接成功");
        }
        catch (Exception e)
        {
            Debug.Log("与PC连接失败:" + e.Message);
            await InitNetForPC();
        }
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    private void SendData(byte[] v)
    {
        try
        {
            //if (isSending) return;
            //UnityThread.executeInUpdate(() => { isSending = true; });
            Debug.Log("准备发送");
            if (writer_vstream != null)
            {
                writer_vstream.WriteInt32(v.Length);
                writer_vstream.WriteBytes(v);

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        writer_vstream.WriteSingle(martrix_camera_to_world[i, j]);
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        writer_vstream.WriteSingle(martrix_projection[i, j]);
                    }
                }

                DataWriterStoreOperation operation = writer_vstream.StoreAsync();
                operation.Completed = new AsyncOperationCompletedHandler<uint>(DataSentHandler);
            }
            else
            {
                Debug.Log("未与TX2建立连接，跳过本次发送");
            }

            if (writer_pc != null)
            {
                writer_pc.WriteInt32(v.Length);
                writer_pc.WriteBytes(v);

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (isSendWarning)
                        {
                            if (i == 0 && j == 0)
                            {
                                writer_pc.WriteSingle(100);
                            }
                            if (i == 0 && j == 1)
                            {
                                writer_pc.WriteSingle(1);
                                isSendWarning = false;
                            }
                        }
                        else
                        {
                            writer_pc.WriteSingle(martrix_camera_to_world[i, j]);
                        }
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        writer_pc.WriteSingle(martrix_projection[i, j]);
                    }
                }

                DataWriterStoreOperation operation2 = writer_pc.StoreAsync();
                operation2.Completed = new AsyncOperationCompletedHandler<uint>(DataSentHandler_PC);
            }
            else
            {
                Debug.Log("未与PC建立连接，跳过本次发送");
            }
        }
        catch (Exception e)
        {
            MyLog.DebugLog(e.Message);
        }
    }

    private void DataSentHandler_PC(IAsyncOperation<uint> asyncInfo, AsyncStatus asyncStatus)
    {
        if (asyncStatus == AsyncStatus.Error)
        {
            MyLog.DebugLog("发送图像帧(PC)时发生错误" + asyncInfo.ErrorCode);
        }
        else if (asyncStatus == AsyncStatus.Completed)
        {
            MyLog.DebugLog("Sended To PC");
        }
    }

    private void DataSentHandler(IAsyncOperation<uint> asyncInfo, AsyncStatus asyncStatus)
    {
        //UnityThread.executeInUpdate(() => { isSending = false; });
        if (asyncStatus == AsyncStatus.Error)
        {
            MyLog.DebugLog("发送图像帧(TX2)时发生错误" + asyncInfo.ErrorCode);
        }
        else if (asyncStatus == AsyncStatus.Completed)
        {
            MyLog.DebugLog("Sended to tx2");
        }
        else
        {
            Debug.Log(asyncStatus.ToString());
        }
    }
#endif
    #endregion

    #region DRAWSQUARE

#if WINDOWS_UWP

    // 开始接收坐标的函数，调用在初始化和一次接收完毕后
    public void RevDetectionsHeaderAsync()
    {
        uint count = 136; // 128 + 4 + 4
        DataReaderLoadOperation operation = reader_receiving.LoadAsync(count);
        operation.Completed = new AsyncOperationCompletedHandler<uint>(HeaderRevd);
    }

    // 读取136后后执行本函数
    private void HeaderRevd(IAsyncOperation<uint> asyncInfo, AsyncStatus asyncStatus)
    {
        if (asyncStatus == AsyncStatus.Error)
        {
            System.Diagnostics.Debug.WriteLine("读取头部数据错误");
            ResetDraw();
        }
        else
        {
            try
            {
                ResetDraw();
                Matrix4x4 ctw = Matrix4x4.zero;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        ctw[i, j] = reader_receiving.ReadSingle();
                    }
                }

                Matrix4x4 projection = Matrix4x4.zero;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        projection[i, j] = reader_receiving.ReadSingle();
                    }
                }

                num = reader_receiving.ReadInt32();
                uint total_size = reader_receiving.ReadUInt32();
                Debug.Log("---------------------------------------\nHeader Received, num: " + num + " total size: " + total_size + "\nCTW:\n" + ctw.ToString() + "\nProjection:\n" + projection.ToString());
                if (num > 0 && total_size > 0)
                {
                    ShowMsg.UpdateCubeMsg($"识别出{num}个目标");
                    Debug.Log("-------------------------\nnum:" + num + "\n-------------------------");
                    DataReaderLoadOperation drlo = reader_receiving.LoadAsync(total_size);
                    drlo.Completed = new AsyncOperationCompletedHandler<uint>(
                        (op, stat) => DetectionBodyRevHandler(op, stat, num, total_size, ctw, projection)
                  
                        );
                }
                else
                {
                    RevDetectionsHeaderAsync();
                }
            }
            catch (Exception e)
            {
                Debug.Log("There was an error reading the recieved detection header:\n" + e);
                ResetDraw();
            }
        }
    }

    // 重置（未使用）
    private void ResetDraw()
    {
        UnityThread.executeInUpdate(() =>
        {
            DestroyAnnotations();
        });
    }

    List<string> label_current = new List<string>();

    // 执行有坐标时的数据解析
    private void DetectionBodyRevHandler(IAsyncOperation<uint> op, AsyncStatus stat, int num, uint size, Matrix4x4 ctw, object projection)
    {
        if (stat == AsyncStatus.Error)
        {
            ResetDraw();
            System.Diagnostics.Debug.WriteLine("读取坐标数据错误");
        }
        else
        {
            UnityThread.executeInUpdate(() =>
            {
                try
                {
                    label_current.Clear();

                    for (int i = 0; i < num; i++)
                    {
                        Debug.Log("-------------LOOP--------------");

                        int left = reader_receiving.ReadInt32();
                        int top = camera_resolution.height - reader_receiving.ReadInt32();
                        int right = reader_receiving.ReadInt32();
                        int bottom = camera_resolution.height - reader_receiving.ReadInt32();
                        int R = reader_receiving.ReadInt32();
                        int G = reader_receiving.ReadInt32();
                        int B = reader_receiving.ReadInt32();
                        uint size_label = reader_receiving.ReadUInt32();
                        String label = reader_receiving.ReadString(size_label);

                        if (label_current.Contains(label) || label == "_unknown")
                        {
                            continue;
                        }

                        label_current.Add(label);
                        Debug.Log("left:" + left + "\ntop:" + top + "\nbottom:" + bottom + "\nright:" + right + "\nRGB:" + R + "," + G + "," + B + "\nlabel: " + label);

                        Ray center = PixelToWorldSpaceRay((left + right) / 2, (top + bottom) / 2, ctw);

                        // 镭射命中
                        //if (Physics.Raycast(center, out RaycastHit centerHit, Mathf.Infinity, raycastLayer))
                        //{
                            Ray topleft = PixelToWorldSpaceRay(left, top, ctw);
                            Ray topright = PixelToWorldSpaceRay(right, top, ctw);
                            Ray bottomleft = PixelToWorldSpaceRay(left, bottom, ctw);

                        //                            float distance = centerHit.distance;
                        //                            float goScaleX = Vector3.Distance(topleft.GetPoint(distance), topright.GetPoint(distance));
                        //                            float goScaleY = Vector3.Distance(topleft.GetPoint(distance), bottomleft.GetPoint(distance));

                        //#if RECT
                        //                            GameObject go = Instantiate(rec) as GameObject;
                        //                            go.transform.SetParent(Parent.transform);
                        //                            go.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
                        //                            go.transform.position = centerHit.point;
                        //                            go.transform.localScale = new Vector3(goScaleX * 200, goScaleY * 200, 0.1f);
                        //                            go.GetComponentInChildren<Renderer>().material.color = new Color(R / 255.0f, G / 255.0f, B / 255.0f);

                        //#endif

                        var renderer = (GameObject)Instantiate(Resources.Load("Prefabs/" + label));

                        renderer.transform.SetParent(Parent.transform);
                        var a = (left + right) / 2 < Screen.width / 2 ? right - 280 : left + 280;
                        renderer.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
                        var position = new Vector3(a, bottom, 0);
                        renderer.transform.localScale = new Vector3(80, 80, -1);
                        renderer.transform.localPosition = position;
                        //if (!label_current.Contains(label))
                        //{
                        //label_current.Add(label);
                        //Debug.Log("显示Info完毕");
                        //}
                        //Destroy(go, 0.3f);
                        //ShowMsg.ShowMessage(label);
                        //DelayToInvokeDo(() =>
                        //{
                        //    label_current.Remove(label);
                        //    Debug.Log("销毁");
                        //}, 5);
                        //ShowMsg.UpdateCubeMsg($"目标出现");

                        //}
                        //else
                        //{
                        //if (label_current.Contains(label))
                        //{
                        //    DelayToInvokeDo(() =>
                        //    {
                        //        label_current.Remove(label);
                        //        Debug.Log("销毁");
                        //    }, 5);
                        //}
                        System.Diagnostics.Debug.WriteLine("射线无限远");
                        //}
                    }

                    RevDetectionsHeaderAsync();
                }
                catch (Exception e)
                {
                    Debug.Log("ERROR3:" + e.Message);
                    ResetDraw();
                }
            });
        }
    }

    public static IEnumerator DelayToInvokeDo(Action action, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        action();
    }

    public Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
    {
        Vector3 from = Vector3.zero;
        var X = proj.GetRow(0);
        var Y = proj.GetRow(1);
        var Z = proj.GetRow(2);
        from.z = to.z / Z.z;
        from.y = (to.y - (from.z * Y.z)) / Y.y;
        from.x = (to.x - (from.z * X.z)) / X.x;
        return from;
    }

    public Ray PixelToWorldSpaceRay(int x, int y, Matrix4x4 cameraToWorld)
    {
        Vector2 ImagePosZeroToOne = new Vector2(x * 1.0f / camera_resolution.width, y * 1.0f / camera_resolution.height);
        Vector2 ImagePosProjected = (ImagePosZeroToOne * 2.0f) - new Vector2(1, 1);
        Vector3 CameraSpacePos = UnProjectVector(martrix_projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1));
        // camera location in world space
        Vector3 WorldSpaceRayPoint1 = cameraToWorld.MultiplyPoint(Vector3.zero);
        // ray point in world space
        Vector3 WorldSpaceRayPoint2 = cameraToWorld.MultiplyPoint(CameraSpacePos);
        // arg:[origin, direction]
        return new Ray(WorldSpaceRayPoint1, WorldSpaceRayPoint2 - WorldSpaceRayPoint1);
    }

    // 移除所有标记（未使用）
    private void DestroyAnnotations()
    {
        foreach (Transform child in Parent.transform)
        {
            Destroy(child.gameObject);
        }
    }

#endif
#endregion
}