using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
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

        public RolesManager rolesManager => Utils.GetRolesManager();

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;
        public ManualLogSource logupdate = Plugin.Instance.logupdate;

        GUIStyle style = new GUIStyle();

        public Canvas canvas;
        public GameObject roleTextContainer;
        public Text roleText;

        public GameObject voteWindowContainer;
        public Text voteText;
        public Text voteTitleText;


        public string voteWindowHeaderText = "-------------------------------------\n" +
                                             "Open/Close: [N]\n" +
                                             "Select: UP & DOWN arrows\n" +
                                             "Vote: ENTER\n" +
                                             "-------------------------------------";
        public string voteWindowPlayersText = "";
        public string voteWindowFullText => $"{voteWindowHeaderText}\n\n{voteWindowPlayersText}";

        public int voteWindowSelectedPlayer = 0;
        public int? voteCastedPlayer = null;

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

            CreateRoleHUD();
            //logger.LogInfo("Manually Starting the RoleHUD");
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
            //if (rolesManager == null)
            //{
            //    rolesManager = Utils.GetRolesManager();
            
        }


        public void UpdateHUD()
        {
            // This cannot be put into the regular Update() method, because it needs to run after PlayerControllerB.LateUpdate().
            // Therefore, I'm not using the regular LateUpdate() method for RoleHUD because I'm not risking it running before the one of PlayerControllerB one.
            // I'm calling this directly from the HarmonyPostfix Patched method LateUpdate() of PlayerControllerB
            UpdateRoleDisplay();
            UpdateToolTip();
            UpdateVoteWindowText();
        }

        void OnDestroy()
        {
            //logger.LogError($"{name} has been destroyed!");
        }

        private void CreateRoleHUD()
        {
            // Create the canvas
            if (canvas == null)
            {
                canvas = new GameObject("RoleHUDCanvas").AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                DontDestroyOnLoad(canvas.gameObject);
            }
            

            if (roleTextContainer == null)
            {
                CreateRoleText();
            }
            

            if (voteWindowContainer == null)
            {
                CreateVoteWindow();
            }
            
        }

        public void CreateRoleText()
        {
            // --------------------------------------------------------------
            // Create a parent GameObject to hold the text
            roleTextContainer = new GameObject("RoleTextContainer");
            roleTextContainer.transform.SetParent(canvas.transform);

            // Add Horizontal Layout Group to the container
            HorizontalLayoutGroup layoutGroup = roleTextContainer.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 10; // Space between image and text
            layoutGroup.padding = new RectOffset(10, 10, 10, 10); // Add padding

            // Configure RectTransform of the container
            RectTransform containerTransform = roleTextContainer.GetComponent<RectTransform>();
            containerTransform.anchorMin = new Vector2(0.5f, 1f); // Center top
            containerTransform.anchorMax = new Vector2(0.5f, 1f); // Center top
            containerTransform.pivot = new Vector2(0.5f, 1f);      // Pivot around top-center
            containerTransform.anchoredPosition = new Vector2(0, -5); // Offset 50 units down from the top
            containerTransform.sizeDelta = new Vector2(500, 200); // Offset 50 units down from the top

            // Create a GameObject for the role text
            GameObject textObject = new GameObject("RoleText");
            textObject.transform.SetParent(roleTextContainer.transform);

            roleText = textObject.AddComponent<Text>();
            roleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Use a default font
            roleText.text = "";
            roleText.supportRichText = true;
            //roleText.alignment = TextAnchor.MiddleLeft;
            roleText.alignment = TextAnchor.UpperCenter;
            roleText.fontSize = 24;
            roleText.color = UnityEngine.Color.white;



            // Configure RectTransform of the text
            RectTransform textTransform = roleText.GetComponent<RectTransform>();
            textTransform.sizeDelta = new Vector2(50, 50); // Width = 200, Height = 50 (adjust as needed)
            textTransform.anchorMin = new Vector2(0.5f, 1.0f); // Anchor to center-left of the parent
            textTransform.anchorMax = new Vector2(0.5f, 1.0f);

            textTransform.anchoredPosition = new Vector2(0, 0); // Offset 50 units down from the top
            textTransform.pivot = new Vector2(0.5f, 1f);    // Pivot around center-left
        }


        public VerticalLayoutGroup AddLayoutGroup(GameObject gameObject)
        {
            VerticalLayoutGroup layoutGroupVertical = gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroupVertical.childAlignment = TextAnchor.UpperCenter;
            layoutGroupVertical.childControlHeight = true;
            layoutGroupVertical.childControlWidth = true;
            //layoutGroupVertical.padding = new RectOffset(0, 0, 0, 0);
            return layoutGroupVertical;
        }
        public void CreateVoteWindow()
        {
            // --------------------------------------------------------------
            // Create a parent GameObject to hold the text
            voteWindowContainer = new GameObject("VoteWindowContainer",typeof(RectTransform));
            voteWindowContainer.transform.SetParent(canvas.transform);


            // Add Vertical Layout Group to the container
            AddLayoutGroup(voteWindowContainer);

            //VerticalLayoutGroup layoutGroup = voteWindowContainer.AddComponent<VerticalLayoutGroup>();
            //layoutGroup.childAlignment = TextAnchor.UpperCenter;
            //layoutGroup.childControlHeight = true;
            //layoutGroup.childControlWidth = true;
            //layoutGroup.padding = new RectOffset(0, 0, 0, 0);

            //layoutGroup.spacing = 10; // Space between image and text
            //layoutGroup.padding = new RectOffset(10, 10, 10, 10); // Add padding


            // Configure RectTransform of the container
            RectTransform containerTransform = voteWindowContainer.GetComponent<RectTransform>();
            containerTransform.anchorMin = new Vector2(0.99f, 0.99f); // Center top
            containerTransform.anchorMax = new Vector2(0.99f, 0.99f); // Center top
            containerTransform.pivot = new Vector2(1.0f, 1.0f);      // Pivot around top-center
            containerTransform.anchoredPosition = new Vector2(0, 0); // Offset 50 units down from the top
            containerTransform.sizeDelta = new Vector2(50, 50); // Offset 50 units down from the top

            ContentSizeFitter containerFitter = voteWindowContainer.AddComponent<ContentSizeFitter>();
            containerFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            containerFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;



            // Create a GameObject for the vote window image background
            GameObject bckgObject = new GameObject("VoteWindowBckg");
            bckgObject.transform.SetParent(voteWindowContainer.transform);

            Image image = bckgObject.AddComponent<Image>();
            image.color = new UnityEngine.Color(1.0f,1.0f,1.0f,0.01f);

            // Vertical Layout group to the image
            //VerticalLayoutGroup imageLayoutGroup = bckgObject.AddComponent<VerticalLayoutGroup>();
            //imageLayoutGroup.childAlignment= TextAnchor.MiddleCenter;
            //imageLayoutGroup.childControlHeight = true;
            //imageLayoutGroup.childControlWidth = true;
            //imageLayoutGroup.padding = new RectOffset(0,0,0,0);

            VerticalLayoutGroup imageLayoutGroup = AddLayoutGroup(bckgObject);
            imageLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
            imageLayoutGroup.spacing = 10;


            // Rect Transform to the image
            RectTransform bckgTransform = bckgObject.GetComponent<RectTransform>();
            bckgTransform.sizeDelta = new Vector2(50, 50); // Width = 200, Height = 50 (adjust as needed)
            bckgTransform.anchorMin = new Vector2(0.5f, 0.5f); // Anchor to center-left of the parent
            bckgTransform.anchorMax = new Vector2(0.5f, 0.5f);

            bckgTransform.anchoredPosition = new Vector2(0, 0); // Offset 50 units down from the top
            bckgTransform.pivot = new Vector2(0.5f, 0.5f);    // Pivot around center-left


            // Create a GameObject for the title of the window
            GameObject windowTitleObject = new GameObject("VoteWindowTitleText");
            windowTitleObject.transform.SetParent(bckgObject.transform);

            voteTitleText = windowTitleObject.AddComponent<Text>();
            voteTitleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Use a default font
            voteTitleText.text = "Vote Window";
            voteTitleText.supportRichText = true;
            voteTitleText.alignment = TextAnchor.UpperCenter;
            voteTitleText.fontSize = 24;
            voteTitleText.color = UnityEngine.Color.white;

            // Configure RectTransform of the text
            RectTransform titleTextTransform = voteTitleText.rectTransform;
            titleTextTransform.sizeDelta = new Vector2(50, 50); // Width = 200, Height = 50 (adjust as needed)
            titleTextTransform.anchorMin = new Vector2(0.5f, 1.0f); // Anchor to center-left of the parent
            titleTextTransform.anchorMax = new Vector2(0.5f, 1.0f);

            titleTextTransform.anchoredPosition = new Vector2(0, 0); // Offset 50 units down from the top
            titleTextTransform.pivot = new Vector2(0.5f, 0.5f);    // Pivot around center-left


            // Create a GameObject for the players list text
            GameObject playersListObject = new GameObject("VoteWindowPlayersListText");
            playersListObject.transform.SetParent(bckgObject.transform);



            voteText = playersListObject.AddComponent<Text>();
            voteText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Use a default font
            voteWindowPlayersText = "Doep\nBananonymous\nPawaeca\nAlmerit\nCookynou\nSynaeh";
            
            voteText.text = voteWindowFullText;
            voteText.supportRichText = true;
            voteText.alignment = TextAnchor.UpperLeft;
            voteText.fontSize = 24;
            voteText.color = UnityEngine.Color.white;



            // Configure RectTransform of the text
            RectTransform textTransform = voteText.rectTransform;
            textTransform.sizeDelta = new Vector2(50, 50); // Width = 200, Height = 50 (adjust as needed)
            textTransform.anchorMin = new Vector2(0.5f, 0.5f); // Anchor to center-left of the parent
            textTransform.anchorMax = new Vector2(0.5f, 0.5f);

            textTransform.anchoredPosition = new Vector2(0, 0); // Offset 50 units down from the top
            textTransform.pivot = new Vector2(0.5f, 0.5f);    // Pivot around center-left

            voteWindowContainer.SetActive(false);

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
                return;
            }

            Role myRole = rolesManager.myRole;

            if (roleText != null)
            {
                // Build the text to be displayed;
                string text = $"{myRole.roleNameColored}\n" + 
                              $"[N] {voteTitleText.text}\n" +
                              $"{myRole.roleActionText.Replace("  ", " ")}";
                roleText.text = text;

                //roleText.text = $"{isVoteWindowOpened}"
            }

        }

        public void UpdateToolTip()
        {
            PlayerControllerB localPlayer = Utils.GetLocalPlayerControllerB();
            if (localPlayer == null) return;
            if (rolesManager.myRole == null) return;


            if (localPlayer.cursorTip.text.Contains(rolesManager.myRole.mainActionName))
            {
                localPlayer.cursorTip.text = "";
            }


            if (localPlayer.isPlayerDead) return;


            //logdebug.LogInfo("UpdateToolTip Grab my role");
            Role myRole = rolesManager.myRole;



            //logdebug.LogInfo("Check for targetInRangeId");
            if (!(rolesManager.myRole.targetInRangeId == null))
            {
                localPlayer.cursorTip.text = rolesManager.myRole.roleActionText;
            }
        }

        public void UpdateVoteWindowText()
        {
            if (rolesManager.myRole == null) return;
            // Update players list
            string displayString = "";
            for (int i = 0; i < rolesManager.allPlayersList.Count; i++)
            {
                ulong playerId = rolesManager.allPlayersIds[i];
                string playerName = rolesManager.allPlayersList[playerId];
                string playerString = playerName;
                if (voteCastedPlayer != null)
                {
                    if (voteCastedPlayer.Value == i)
                    {
                        playerString = $"<color=red>{playerName}</color>";
                    }
                    
                }
                if (i == voteWindowSelectedPlayer)
                {
                    playerString = $"-> {playerString}";
                }
                displayString += $"{playerString}\n";
            }

            voteWindowPlayersText = displayString.Trim('\n');
            voteText.text = voteWindowFullText;

            // Update cooldown for the vote

            //voteTitleText = "Vote";
            string cooldownText;
            float currentCooldown = rolesManager.voteKillCurrentCooldown;
            
            if (currentCooldown > 0)
            { 
                cooldownText = $" ({(int)currentCooldown}s)";
            }
            else
            {
                cooldownText = $" Available";
            }

            voteTitleText.text = $"Vote{cooldownText}";



        }

        public void OpenCloseVoteTab()
        {
            if (voteWindowContainer.activeSelf) CloseVoteTab();
            else OpenVoteTab();
        }

        private void OpenVoteTab()
        {
            voteWindowContainer.SetActive(true);
        }

        private void CloseVoteTab()
        {
            voteWindowContainer.SetActive(false);
        }
    }
}
