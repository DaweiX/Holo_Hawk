//#define WINDOWS_UWP
/*-------------------------------------------------------------
 *                  HoloLens 视频通信
 *                   CODE BY DAWEIX
 *                      2017.12.5
 *              功能：Socket随机流双向通信
 * ------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Diagnostics;
using System.IO;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking.Connectivity;
using System.Threading.Tasks;
#endif

public class SocketBase : MonoBehaviour, IDisposable
{
    public Text text;
    static string content;
    public delegate void TakeAnother();
    public static event TakeAnother OnTakeAnother;

    // 状态
    private bool isInited = false;
#if WINDOWS_UWP
    public static StreamSocketListener Listener = new StreamSocketListener();
#endif
    const string SERVICE_PORT = "795";
    const string IP_CHE = "192.168.43.254";
    const string IP_BUAA = "10.138.42.53";
    const string IP_MI = "192.168.31.199";
    public const string LISTEN_PORT = "8888";


    void Start()
    {
#if WINDOWS_UWP
        Show("Initing");
        Bind();
        Listener.ConnectionReceived += Listener_ConnectionReceived;
#endif
    }


#if WINDOWS_UWP
    async void Bind()
    {
        await Listener.BindServiceNameAsync(LISTEN_PORT);
    }

    public async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        Show("Received");
        string text = string.Empty;
        using (StreamSocket socket = args.Socket)
        {
            using (DataReader reader = new DataReader(socket.InputStream))
            {
                try
                {
                    await reader.LoadAsync(sizeof(uint));
                    uint length = reader.ReadUInt32();
                    await reader.LoadAsync(length);
                    IBuffer buffer = reader.ReadBuffer(length);
                    // 拿到了buffer
                    Show("r" + buffer.Length.ToString());
                }
                catch (Exception e)
                {
                    Show(e.Message);
                }
            }
        }
    }

    public static async void Send(IRandomAccessStream stream)
    {
        if (stream == null)
        {
            Show("NoData");
            return;
        }
        Show("Sending");
        using (StreamSocket socket = new StreamSocket())
        {
            try
            {
                // 发起连接
                await socket.ConnectAsync(new HostName(IP_CHE), LISTEN_PORT);
                using (DataWriter writer = new DataWriter(socket.OutputStream))
                {
                    // 先写个长度
                    var length = (uint)stream.AsStream().Length;
                    writer.WriteUInt32(length);
                    // 再传输Buffer
                    IBuffer buffer = await StreamToBuffer(stream);
                    writer.WriteBuffer(buffer);
                    await writer.StoreAsync();
                    Show("Sended");
                    // OnTakeAnother();
                }
            }
            catch (Exception e)
            {
                Show(e.Message);
            }
        }
    }

    public static IBuffer BytesToBuffer(byte[] bytes)
    {
        using (var dataWriter = new DataWriter())
        {
            dataWriter.WriteBytes(bytes);
            return dataWriter.DetachBuffer();
        }
    }

    public static async Task<IBuffer> StreamToBuffer(IRandomAccessStream stream)
    {
        var s = stream.AsStreamForRead();
        if (stream != null)
        {
            s = stream.AsStreamForRead();
            int len = (int)s.Length;
            byte[] b = new byte[(uint)s.Length];
            await s.ReadAsync(b, 0, len);
            IBuffer buffer = WindowsRuntimeBufferExtensions.AsBuffer(b, 0, len);
            return buffer;
        }
        return null;
    }
#endif
#if WINDOWS_UWP
    ~SocketBase()
    {
        Dispose();
    }
#endif

    static public void Show(string msg)
    {
        content = msg;
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(content))
            text.text = content;
    }

    public void Dispose()
    {
#if WINDOWS_UWP
        if (Listener != null)
        {
            Listener.Dispose();
            Listener = null;
        }
#endif
    }
}
