using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace WerewolvesCompany.UI
{
    internal class RoleHUD : MonoBehaviour
    {
        public static RoleHUD Instance;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        private GameObject canvasObject;
        private Text roleNameText;
        private Image roleIconImage;

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

        void OnDestroy()
        {
            //logger.LogError($"{name} has been destroyed!");
        }

        private void CreateRoleHUD()
        {
            // Create Canvas
            canvasObject = new GameObject("RoleHUDCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Scale the Canvas to screen size
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            canvasObject.AddComponent<GraphicRaycaster>();

            // Create a Text element for the role name
            GameObject textObject = new GameObject("RoleNameText");
            textObject.transform.SetParent(canvasObject.transform); // Attach the textObject to the Canvas

            // Fill the textObject with the Role Name
            roleNameText = textObject.AddComponent<Text>();
            roleNameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            roleNameText.fontSize = 24;
            roleNameText.alignment = TextAnchor.MiddleCenter;
            roleNameText.color = Color.white;

            // Position and size the text
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(300, 50);
            textRect.anchoredPosition = new Vector2(0, -50); // Adjust position as needed

            // Create an Image element for the role icon
            GameObject imageObject = new GameObject("RoleIconImage");
            imageObject.transform.SetParent(canvasObject.transform);

            // Attach the actual role Icon to the imageObject
            roleIconImage = imageObject.AddComponent<Image>();
            roleIconImage.color = Color.white; // Set default color

            // Position and size the image
            RectTransform imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.sizeDelta = new Vector2(50, 50);
            imageRect.anchoredPosition = new Vector2(0, 0); // Adjust position as needed
        }

        public void UpdateRoleDisplay(Role role)
        {
            if (canvasObject == null)
            {
                CreateRoleHUD();
            }
            logdebug.LogInfo($"Updating display at layer = {canvasObject.GetComponent<Canvas>().sortingOrder}");
            if (roleNameText != null)
            {
                roleNameText.text = role.roleName;
            }

            if (roleIconImage != null && role.roleIcon != null)
            {
                roleIconImage.sprite = role.roleIcon;
                roleIconImage.enabled = true;
            }
            else
            {
                roleIconImage.enabled = false;
            }
        }
    }
}
