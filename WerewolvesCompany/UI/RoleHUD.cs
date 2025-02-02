﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;
using WerewolvesCompany.Managers;

namespace WerewolvesCompany.UI
{
    internal class RoleHUD : MonoBehaviour
    {
        public RoleHUD Instance;

        public RolesManager rolesManager = Utils.GetRolesManager();

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;
        public ManualLogSource logupdate = Plugin.Instance.logupdate;



        public Canvas canvas;
        public Text roleText;
        //public Image roleIcon;
        //public Text toolTipText;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep it across scenes if needed
            }
            else
            {
                logger.LogInfo("Duplicate detected, delted the just-created RoleHUD");
                Destroy(gameObject); // Prevent duplicate instances
            }

            logger.LogInfo("Manually Starting the RoleHUD");
            Start();
        }
        void Start()
        {
            //logger.LogInfo("Creating the RoleHUD");
            //CreateRoleHUD();
            //logger.LogInfo("RoleHUD has been created");
        }

        void Update()
        {
            if (rolesManager == null)
            {
                rolesManager = Utils.GetRolesManager();
            }
        }

        void OnDestroy()
        {
            //logger.LogError($"{name} has been destroyed!");
        }

        private void CreateRoleHUD()
        {
            // Create the canvas
            canvas = new GameObject("RoleHUDCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            DontDestroyOnLoad(canvas.gameObject);

            // Create a parent GameObject to hold the image and text
            GameObject container = new GameObject("RoleContainer");
            container.transform.SetParent(canvas.transform);

            // Add Horizontal Layout Group to the container
            HorizontalLayoutGroup layoutGroup = container.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 10; // Space between image and text
            layoutGroup.padding = new RectOffset(10, 10, 10, 10); // Add padding

            // Configure RectTransform of the container
            RectTransform containerTransform = container.GetComponent<RectTransform>();
            containerTransform.anchorMin = new Vector2(0.5f, 1f); // Center top
            containerTransform.anchorMax = new Vector2(0.5f, 1f); // Center top
            containerTransform.pivot = new Vector2(0.5f, 1f);      // Pivot around top-center
            containerTransform.anchoredPosition = new Vector2(0,-5); // Offset 50 units down from the top
            containerTransform.sizeDelta = new Vector2(500, 200); // Offset 50 units down from the top

            // Create a GameObject for the role image
            //GameObject imageObject = new GameObject("RoleImage");
            //imageObject.transform.SetParent(container.transform);
            //roleIcon = imageObject.AddComponent<Image>();
            //roleIcon.color = Color.white; // Default color

            //// Load a placeholder image (replace "your-image-path" with your actual image path or asset)
            //Sprite placeholderSprite = Resources.Load<Sprite>("placeholder-image");
            //if (placeholderSprite != null)
            //{
            //    roleIcon.sprite = placeholderSprite;
            //}

            // Configure RectTransform of the image
            //RectTransform imageTransform = roleIcon.GetComponent<RectTransform>();
            //imageTransform.sizeDelta = new Vector2(50, 50); // Set image size (adjust as needed)

            // Create a GameObject for the role text
            GameObject textObject = new GameObject("RoleText");
            textObject.transform.SetParent(container.transform);
            roleText = textObject.AddComponent<Text>();
            roleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Use a default font
            roleText.text = "Role: Unknown";
            //roleText.alignment = TextAnchor.MiddleLeft;
            roleText.alignment = TextAnchor.UpperCenter;
            roleText.fontSize = 24;
            roleText.color = Color.white;

            // Configure RectTransform of the text
            RectTransform textTransform = roleText.GetComponent<RectTransform>();
            textTransform.sizeDelta = new Vector2(500, 200); // Width = 200, Height = 50 (adjust as needed)
            textTransform.anchorMin = new Vector2(0.5f, 1.0f); // Anchor to center-left of the parent
            textTransform.anchorMax = new Vector2(0.5f, 1.0f);

            textTransform.anchoredPosition = new Vector2(0, 0); // Offset 50 units down from the top
            textTransform.pivot = new Vector2(0.5f, 1f);    // Pivot around center-left


            

            //// Create the tooltip in the middle  of the screen
            //GameObject toolTipTextObject = new GameObject("RoleActionToolTip");
            //toolTipTextObject.transform.SetParent(canvas.transform);

            //toolTipText = toolTipTextObject.AddComponent<Text>();
            //toolTipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Use a default font
            //toolTipText.text = "Role: Unknown";
            //toolTipText.alignment = TextAnchor.MiddleLeft;
            //toolTipText.fontSize = 24;
            //toolTipText.color = Color.white;

            //// Configure RectTransform of the text
            //RectTransform toolTipTextTransform = toolTipText.GetComponent<RectTransform>();
            //toolTipTextTransform.sizeDelta = new Vector2(1000, 50); // Width = 200, Height = 50 (adjust as needed)
            //toolTipTextTransform.anchorMin = new Vector2(0, 0.5f); // Anchor to center-left of the parent
            //toolTipTextTransform.anchorMax = new Vector2(0, 0.5f);
            //toolTipTextTransform.pivot = new Vector2(0, 0.5f);    // Pivot around center-left

        }

        public void UpdateRoleDisplay()
        {
            if (canvas == null)
            {
                CreateRoleHUD();
            }

            if (rolesManager.myRole == null)
            {
                roleText.text = "";
                //roleIcon.enabled = false;
                return;
            }

            Role myRole = rolesManager.myRole;


            if (roleText != null)
            {
                // Build the text to be displayed;
                string text = $"{myRole.roleName}\n" + 
                              $"{myRole.roleActionText.Replace("  ", " ")}";

                roleText.text = text;
            }

            //if (roleIcon != null && myRole.roleIcon != null)
            //{
            //    roleIcon.sprite = myRole.roleIcon;
            //    roleIcon.enabled = true;
            //}
            //else
            //{
            //    roleIcon.enabled = false;
            //}

            //toolTipText.text = myRole.roleActionText;
        }

        public void UpdateToolTip()
        {
            PlayerControllerB localPlayer = Utils.GetLocalPlayerControllerB();

            if (localPlayer.cursorTip.text.Contains(rolesManager.myRole.mainActionName))
            {
                localPlayer.cursorTip.text = "";
            }

            //if (rolesManager.myRole.targetInRangeId == null)
            //{
            //    if (localPlayer.cursorTip.text.Contains(rolesManager.myRole.roleActionText))
            //    {
            //        localPlayer.cursorTip.text = "";
            //    }
            //}

            //logdebug.LogInfo("UpdateToolTip Grab my role");
            Role myRole = rolesManager.myRole;



            //logdebug.LogInfo("Check for targetInRangeId");
            if (!(rolesManager.myRole.targetInRangeId == null))
            {
                localPlayer.cursorTip.text = rolesManager.myRole.roleActionText;

            }


        }
    }
}
