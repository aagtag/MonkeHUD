using BepInEx;
using BepInEx.Configuration;
using Utilla;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HoneyLib;
using HoneyLib.Events;
using UnityEngine.Audio;

namespace TagOverlay
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;
        public static GameObject TagThingi = null;
        public static Text TagText = null;
        public static Text FPSText = null; // Add the FPS Text

        private AudioSource audioSource; // New audio source variable

        void Start()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            audioSource = gameObject.AddComponent<AudioSource>(); // Initialize the audio source
        }

        void OnEnable()
        {
            Debug.Log("loaded");
        }

        void Awake()
        {
            Utilla.Events.GameInitialized += GameInitialized;
        }

        private void GameInitialized(object sender, EventArgs e)
        {
            HoneyLib.Events.Events.TagHitLocal += TagHitLocal;

            TagThingi = new GameObject("Tag Thingi");
            TagThingi.transform.SetParent(GorillaTagger.Instance.mainCamera.transform, false);
            TagThingi.transform.localPosition = new Vector3(0.1f, -0.05f, 0.5f);
            TagThingi.transform.localScale = new Vector3(0.0025f, 0.0025f, 0.0025f);

            var canvas = TagThingi.AddComponent<Canvas>();
            canvas.sortingOrder = 999; // Set a high sorting order to make it appear above everything

            GameObject textObject = new GameObject("Tag Text");
            textObject.transform.SetParent(TagThingi.transform, false);
            textObject.AddComponent<CanvasRenderer>();
            TagText = textObject.AddComponent<Text>();
            TagText.font = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/NameTagAnchor/NameTagCanvas/Text/")?.GetComponent<Text>().font;
            TagText.color = Color.white;
            TagText.alignment = TextAnchor.MiddleCenter;

            // Create FPS Text
            GameObject fpsObject = new GameObject("FPS Text");
            fpsObject.transform.SetParent(TagThingi.transform, false);
            fpsObject.AddComponent<CanvasRenderer>();
            FPSText = fpsObject.AddComponent<Text>();
            FPSText.font = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/NameTagAnchor/NameTagCanvas/Text/")?.GetComponent<Text>().font;
            FPSText.color = Color.green;
            FPSText.alignment = TextAnchor.UpperRight; // Align to the upper right corner

            // Set the position of FPS Text relative to Tag Text
            RectTransform tagRectTransform = TagText.GetComponent<RectTransform>();
            RectTransform fpsRectTransform = FPSText.GetComponent<RectTransform>();
            fpsRectTransform.anchoredPosition = new Vector2(tagRectTransform.rect.width / 2f, -tagRectTransform.rect.height / 2f); // Place it on the upper-right corner relative to Tag Text
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
        }

        // ... (existing code)

        public void TagHitLocal(object sender, TagHitLocalArgs e)
        {
            string playerName = e.taggedPlayer.NickName;
            StartCoroutine(DisplayTagMessage(playerName));
        }

        IEnumerator DisplayTagMessage(string playerName)
        {
            string message = "You just tagged " + playerName + "!";
            Debug.Log(message);
            TagText.text = message;
            TagThingi.SetActive(true);

            float displayDuration = 1f; // Set the duration of the display in seconds

            yield return new WaitForSeconds(displayDuration);

            // Clear the Tag Text at the end of the coroutine to hide it
            TagText.text = "";
        }

        // ... (existing code)

        // Update FPS Text in the Update method
        void Update()
        {
            if (FPSText != null)
            {
                FPSText.text = "FPS: " + (1f / Time.deltaTime).ToString("F0");
            }
        }

        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            inRoom = true;
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            inRoom = false;
        }
    }
}
