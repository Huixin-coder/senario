using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class TriggerEyeTracking : MonoBehaviour
{
    // private WebSocket ws;
    // private bool isConnected = false; // To track connection status
    // private const int reconnectInterval = 5; // Interval in seconds for reconnection attempts
    // private bool isConnecting = false; // Flag to avoid multiple connection attempts simultaneously
    // private Text connectionStatusText;
    // private bool uiUpdateNeeded = false; // Flag to track when UI needs to be updated
    // private bool showConnectionStatus = false; // Whether to show the connection status UI
    // private string pnr;
    // private string scenarioNumber;

    // void Start()
    // {  
    //     SceneReloaded();
    //     StartCoroutine(ManageWebSocket());
    // }

    // public void SceneReloaded()
    // {
    //     pnr = PlayerPrefs.GetString("PNR");
    //     scenarioNumber = "Scenario 0";

    //     // connectionStatusText = GameObject.FindWithTag("MainCanvas").transform.GetChild(7).transform.gameObject.GetComponent<Text>();
    //     uiUpdateNeeded = true;
    // }

    // void Update()
    // {
    //     if (uiUpdateNeeded)
    //     {
    //         uiUpdateNeeded = false; // Reset the flag
    //         // connectionStatusText.gameObject.SetActive(showConnectionStatus);
    //     }

    //     // Debug.Log(isConnected);
    // }

    // private IEnumerator ManageWebSocket()
    // {
    //     while (true) // Keep the coroutine running to monitor connection status
    //     {
    //         if (!isConnected && !isConnecting)
    //         {
    //             // Show connection status UI
    //             showConnectionStatus = true;
    //             uiUpdateNeeded = true;

    //             // Start connection attempt in a new thread
    //             isConnecting = true;
    //             Thread connectThread = new Thread(TryConnectWebSocket);
    //             connectThread.Start();
    //         }
    //         else if (isConnected && !ws.IsAlive)
    //         {
    //             Debug.Log("Detected disconnection. Attempting to reconnect...");
    //             isConnected = false;
    //             isConnecting = false;

    //             // Show connection status UI
    //             showConnectionStatus = true;
    //             uiUpdateNeeded = true;
    //         }

    //         // Wait a bit before the next check
    //         yield return new WaitForSeconds(reconnectInterval);
    //     }
    // }

    // private void TryConnectWebSocket()
    // {
    //     // Initialize WebSocket
    //     ws = new WebSocket("ws://192.168.75.51/websocket", "g3api");

    //     // Set up event handlers
    //     ws.OnMessage += (sender, e) =>
    //     {
    //         Debug.Log("Received: " + e.Data);
    //     };
    //     ws.OnOpen += (sender, e) =>
    //     {
    //         Debug.Log("WebSocket connection established.");
    //         isConnected = true;
    //         isConnecting = false;

    //         // Hide connection status UI
    //         showConnectionStatus = false;
    //         uiUpdateNeeded = true;
    //     };
    //     ws.OnClose += (sender, e) =>
    //     {
    //         Debug.Log("WebSocket connection closed.");
    //         isConnected = false;
    //         isConnecting = false;

    //         // Show connection status UI
    //         showConnectionStatus = true;
    //         uiUpdateNeeded = true;
    //     };
    //     ws.OnError += (sender, e) =>
    //     {
    //         Debug.LogError("WebSocket error: " + e.Message);
    //         isConnected = false; // Ensure the flag is updated in case of errors
    //         isConnecting = false;

    //         // Show connection status UI
    //         showConnectionStatus = true;
    //         uiUpdateNeeded = true;
    //     };

    //     try
    //     {
    //         // Attempt to connect
    //         ws.Connect();

    //         // Ensure connection state is accurately reflected
    //         if (!ws.IsAlive)
    //         {
    //             Debug.Log("Connection attempt failed.");
    //             isConnected = false;
    //             isConnecting = false;

    //             // Show connection status UI
    //             showConnectionStatus = true;
    //             uiUpdateNeeded = true;
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Debug.LogError("WebSocket connection exception: " + ex.Message);
    //         isConnected = false;
    //         isConnecting = false;

    //         // Show connection status UI
    //         showConnectionStatus = true;
    //         uiUpdateNeeded = true;
    //     }
    // }

    // // Public method to send WebSocket messages externally - no pnr and snr needed
    // public void SendEventToEyeTracker(string eventType)
    // {
    //     SendWebSocketMessage(pnr, scenarioNumber, eventType);
    // }

    // // Public method to send WebSocket messages externally
    // public void SendWebSocketMessage(string pnr, string snr, string eventType)
    // {
    //     if (isConnected)
    //     {
    //         // Debug.Log("Time: " + Time.time);

    //         // Create the JSON body
    //         var body = new JObject
    //         {
    //             ["path"] = "recorder!send-event",
    //             ["id"] = 99,
    //             ["method"] = "POST",
    //             ["body"] = new JArray
    //             {
    //                 "TTL",
    //                 new JObject
    //                 {
    //                     ["participantNumber"] = pnr,
    //                     ["scenarioNumber"] = snr,
    //                     ["event"] = eventType,
    //                     ["inGameTime"] = Time.time,
    //                     ["machineTimestamp"] = System.DateTime.Now
    //                 }
    //             }
    //         };

    //         // Send the message through WebSocket
    //         ws.Send(body.ToString());
    //         // Debug.Log("Sent: " + body.ToString());
    //     }
    // }

    // void OnDestroy()
    // {
    //     // Close the WebSocket when the script is destroyed
    //     if (ws != null)
    //     {
    //         ws.Close();
    //         ws = null;
    //     }
    // }
}
