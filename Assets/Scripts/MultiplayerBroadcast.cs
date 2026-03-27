using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;

namespace Unity.RenderStreaming
{
    public class Broadcast : SignalingHandlerBase,
        IOfferHandler, IAddChannelHandler, IDisconnectHandler, IDeletedConnectionHandler,
        IAddReceiverHandler
    {
        public class StreamComponents {
            public string mirror;
            public IStreamSender sender;
            public IDataChannel data;
        }

        [SerializeField] private List<Component> streams = new List<Component>();

        private Dictionary<string, StreamComponents> connectionIds = new Dictionary<string, StreamComponents>();

        public override IEnumerable<Component> Streams => streams;

        public void AddComponent(Component component)
        {
            streams.Add(component);
        }

        public void RemoveComponent(Component component)
        {
            streams.Remove(component);
        }

        public void OnDeletedConnection(SignalingEventData eventData)
        {
            Disconnect(eventData.connectionId);
        }

        public void OnDisconnect(SignalingEventData eventData)
        {
            Disconnect(eventData.connectionId);
        }

        private void Disconnect(string connectionId)
        {
            if (!connectionIds.ContainsKey(connectionId))
                return;
            connectionIds.Remove(connectionId);

            foreach (var sender in streams.OfType<IStreamSender>())
            {
                RemoveSender(connectionId, sender);
            }
            foreach (var receiver in streams.OfType<IStreamReceiver>())
            {
                RemoveReceiver(connectionId, receiver);
            }
            foreach (var channel in streams.OfType<IDataChannel>().Where(c => c.ConnectionId == connectionId))
            {
                RemoveChannel(connectionId, channel);
            }
        }

        public void OnAddReceiver(SignalingEventData data)
        {
            var track = data.transceiver.Receiver.Track;
            IStreamReceiver receiver = GetReceiver(track.Kind);
            SetReceiver(data.connectionId, receiver, data.transceiver);
        }

        public void OnOffer(SignalingEventData data)
        {
            StreamComponents sc = new StreamComponents();

            if (connectionIds.ContainsKey(data.connectionId))
            {
                RenderStreaming.Logger.Log($"Already answered this connectionId : {data.connectionId}");
                return;
            }

            // extract the mirror type from SDP message
            // This is the value the user sets after the IP address
            // e.g. 192.168.1.104:1001/?mirrorType=LeftMirror
            string mirrorType = "";
            string[] linesInMsg = data.sdp.Split('\n');
            foreach (string line in linesInMsg) {
                if (line.Split('=')[0] == "a") {
                    if (line.Split('=')[1].Split(':')[0] == "mirrorType") {
                        mirrorType = line.Split('=')[1].Split(':')[1];
                    }
                }
            }

            // check if this mirror is already in use
            foreach (string key in connectionIds.Keys) {
                StreamComponents component = connectionIds[key];
                if (component.mirror == mirrorType) {
                    // delete this connection so that the new connection can take over
                    Disconnect(key);
                }
            }

            // detect stream corresponding to mirrorType
            // currently done by comparing mirrorType to the name of the sender GO
            foreach (VideoStreamSender videoStreamSender in streams) {
                GameObject camera = videoStreamSender.sourceCamera.gameObject;
                // // UnityEngine.Debug.Log(mirrorType);
                // UnityEngine.Debug.Log(camera.name);
                // UnityEngine.Debug.Log(mirrorType);
                // UnityEngine.Debug.Log(string.Equals(camera.name, mirrorType));
                if (camera.name.Trim().Equals(mirrorType.Trim())) {
                    // this is the stream we want to assign to the current connectionID
                    // UnityEngine.Debug.Log(mirrorType);
                    sc.mirror = mirrorType;

                    IStreamSender vidSrc = videoStreamSender;
                    AddSender(data.connectionId, vidSrc);
                    sc.sender = vidSrc;

                    // IDataChannel channel = NOTWORKING
                    // AddChannel(data.connectionId, channel);
                    // sc.data = channel;

                    connectionIds.Add(data.connectionId, sc);
                    SendAnswer(data.connectionId);
                }
            }
        }

        public void OnAddChannel(SignalingEventData data)
        {
            var channel = streams.OfType<IDataChannel>().
                FirstOrDefault(r => !r.IsConnected && !r.IsLocal);
            channel?.SetChannel(data.connectionId, data.channel);
        }

        IStreamReceiver GetReceiver(WebRTC.TrackKind kind)
        {
            if (kind == WebRTC.TrackKind.Audio)
                return streams.OfType<AudioStreamReceiver>().First();
            if (kind == WebRTC.TrackKind.Video)
                return streams.OfType<VideoStreamReceiver>().First();
            throw new System.ArgumentException();
        }
    }
}


// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// namespace Unity.RenderStreaming
// {
//     public class MultiplayerBroadcast : SignalingHandlerBase,
//         IOfferHandler, IAddChannelHandler, IDisconnectHandler, IDeletedConnectionHandler,
//         IAddReceiverHandler
//     {
//         public class StreamComponents {
//             public string mirror;
//             public IStreamSender sender;
//             public IDataChannel data;
//         }
//         [SerializeField]
//         private List<GameObject> streams = new List<GameObject>();

//         private Dictionary<string, StreamComponents> connectionIds = new Dictionary<string, StreamComponents>();

//         // public void AddComponent(Component component)
//         // {
//         //     streams.Add(component);
//         // }

//         // public void RemoveComponent(Component component)
//         // {
//         //     streams.Remove(component);
//         // }

//         public static MultiplayerBroadcast instance;
//         void Awake()
//         {
//             if (instance != null)
//             {
//                 Destroy(gameObject);
//             }
//             else
//             {
//                 instance = this;
//                 DontDestroyOnLoad(gameObject);
//             }
//         }

//         public void OnDeletedConnection(SignalingEventData eventData)
//         {
//             Disconnect(eventData.connectionId);
//         }

//         public void OnDisconnect(SignalingEventData eventData)
//         {
//             Disconnect(eventData.connectionId);
//         }

//         private void Disconnect(string connectionId)
//         {
//             if (!connectionIds.ContainsKey(connectionId))
//                 return;
//             // connectionIds.Remove(connectionId);

//             // foreach (var source in connectionIds[connectionId].OfType<IStreamSender>()) {
//             //     source.SetSender(connectionId, null);
//             // }

//             // foreach (var source in connectionIds[connectionId].OfType<IDataChannel>()) {
//             //     source.SetChannel(connectionId, null);
//             // }

//             // connectionIds.Remove(connectionId);
//             RemoveSender(connectionId, connectionIds[connectionId].sender);


//             // foreach (var receiver in streams.OfType<IStreamReceiver>())
//             // {
//             //     RemoveReceiver(connectionId, receiver);
//             // }
//             RemoveChannel(connectionId, connectionIds[connectionId].data);

//             // foreach (var channel in connectionIds[connectionId].data)
//             // {
//             //     if (channel != null) {
//             //         RemoveChannel(connectionId, channel);
//             //     }
//             // }
//         }

//         public void OnAddReceiver(SignalingEventData data)
//         {
//             var receiver = streams.OfType<IStreamReceiver>().
//                 FirstOrDefault(r => r.Track == null);
//             receiver?.SetReceiver(data.connectionId, data.receiver);
//         }

//         public void OnOffer(SignalingEventData data)
//         {
//             StreamComponents sc = new StreamComponents();
//             if (connectionIds.ContainsKey(data.connectionId))
//             {
//                 Debug.Log($"Already answered this connectionId : {data.connectionId}");
//                 return;
//             }

//             // extract the mirror type from SDP message
//             // This is the value the user sets after the IP address
//             // e.g. 192.168.1.104:1001/?mirrorType=LeftMirror
//             string mirrorType = "";
//             string[] linesInMsg = data.sdp.Split('\n');
//             foreach (string line in linesInMsg) {
//                 if (line.Split('=')[0] == "a") {
//                     if (line.Split('=')[1].Split(':')[0] == "mirrorType") {
//                         mirrorType = line.Split('=')[1].Split(':')[1];
//                     }
//                 }
//             }

//             // check if this mirror is already in use
//             foreach (string key in connectionIds.Keys) {
//                 StreamComponents component = connectionIds[key];
//                 if (component.mirror == mirrorType) {
//                     // delete this connection so that the new connection can take over
//                     Disconnect(key);
//                 }
//             }

//             // detect stream corresponding to mirrorType
//             // currently done by comparing mirrorType to the name of the sender GO
//             foreach (GameObject camera in streams) {
//                 // // UnityEngine.Debug.Log(mirrorType);
//                 // UnityEngine.Debug.Log(camera.name);
//                 // UnityEngine.Debug.Log(mirrorType);
//                 // UnityEngine.Debug.Log(string.Equals(camera.name, mirrorType));
//                 if (camera.name.Trim().Equals(mirrorType.Trim())) {
//                     // this is the stream we want to assign to the current connectionID
//                     // UnityEngine.Debug.Log(mirrorType);
//                     sc.mirror = mirrorType;

//                     IStreamSender vidSrc = camera.GetComponent<CameraStreamSender>();
//                     AddSender(data.connectionId, vidSrc);
//                     sc.sender = vidSrc;

//                     IDataChannel channel = camera.GetComponent<WebBrowserInputChannelReceiver>();
//                     AddChannel(data.connectionId, channel);
//                     sc.data = channel;

//                     connectionIds.Add(data.connectionId, sc);
//                     SendAnswer(data.connectionId);
//                 }
//             }


//             // IStreamSender vidSrc = streams.OfType<IStreamSender>().ElementAt(connectionIds.Count);
//             // AddSender(data.connectionId, vidSrc);
//             // sc.sender = vidSrc;
//             // var transceiver = AddSenderTrack(data.connectionId, vidSrc.Track);
//             // vidSrc.SetSender(data.connectionId, transceiver.Sender);

//             // AudioStreamSender audioSrc = streams.OfType<AudioStreamSender>().ElementAt(connectionIds.Count);
//             // AddSender(data.connectionId, audioSrc);
//             // transceiver = AddSenderTrack(data.connectionId, audioSrc.Track);
//             // audioSrc.SetSender(data.connectionId, transceiver.Sender);


//             // connectionIds.Add(data.connectionId, new List<Component>());
//             // connectionIds[data.connectionId].Add(vidSrc);
//             // connectionIds[data.connectionId].Add(audioSrc);

//             // foreach (var source in streams.OfType<IStreamSender>())
//             // {
//             //     AddSender(data.connectionId, source);
//             // // }
//             // int j = 0;
//             // foreach (var channel in streams.OfType<IDataChannel>().Where(c => c.IsLocal))
//             // {
//             //     j++;
//             // }
//             // sc.data = new IDataChannel[j];
//             // int i = 0;
//             // foreach (var channel in streams.OfType<IDataChannel>().Where(c => c.IsLocal))
//             // {
//             //     AddChannel(data.connectionId, channel);
//             //     sc.data[i] = channel;
//             //     i++;
//             //     // var _channel = CreateChannel(data.connectionId, channel.Label);
//             //     // channel.SetChannel(data.connectionId, _channel);
//             //     // connectionIds[data.connectionId].Add(channel);
//             //     // AddChannel(data.connectionId, channel);
//             // }
//             // connectionIds.Add(data.connectionId, sc);
//             // UnityEngine.Debug.Log(connectionIds.Keys.Count);
//             // UnityEngine.Debug.Log(connectionIds.Keys.ToList());
//             // SendAnswer(data.connectionId);
//         }

//         public void OnAddChannel(SignalingEventData data)
//         {
//             var channel = streams.OfType<IDataChannel>().
//                 FirstOrDefault(r => !r.IsConnected && !r.IsLocal);
//             channel?.SetChannel(data.connectionId, data.channel);
//         }
//     }
// }
