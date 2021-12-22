using BepInEx;
using BepInEx.Configuration;
using Bounce.Unmanaged;
using System.Linq;
using UnityEngine;

namespace LordAshes
{
    [BepInPlugin(Guid, "Chat Roller Plug-In", Version)]
    [BepInDependency(RadialUI.RadialUIPlugin.Guid)]
    public partial class ChatRollerPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Guid = "org.lordashes.plugins.chatroller";
        public const string Version = "1.4.3.0";

        // Content directory
        public static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Configuration
        private ConfigEntry<KeyboardShortcut>[] triggerKey { get; set; } = new ConfigEntry<KeyboardShortcut>[2];

        // Radial menu creature
        private CreatureGuid radialCreature = CreatureGuid.Empty;

        /// <summary>
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.LogWarning("*************************************************************");
            UnityEngine.Debug.LogWarning("Chat Roller Plugin is Obsolete. Use Chat Roll Plugin instead.");
            UnityEngine.Debug.LogWarning("*************************************************************");

            UnityEngine.Debug.Log("Chat Roller Plugin Active. Expecting character sheets in '" + dir + "/Misc',");
            UnityEngine.Debug.Log("with file name matching the edition dot character name and an .CHS extension.");

            // Read configuration
            triggerKey[0] = Config.Bind("Keys", "Select Character Sheet Edition", new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl));
            triggerKey[1] = Config.Bind("Keys", "Make A Roll", new KeyboardShortcut(KeyCode.R, KeyCode.LeftControl));
            RollHandler.SetEdition(Config.Bind("Settings", "Edition Prefix", "Dnd5e").Value);

            Debug.Log("Chat Roller Plugin: Using Edition '" + RollHandler.GetEdition() + "'");

            // Add icon to radial mini menu if config dictates it
            if (Config.Bind("Settings", "Show In Mini Menu", true).Value)
            {
                // Add icon to radial menu
                Texture2D tex = new Texture2D(32, 32);
                tex.LoadImage(System.IO.File.ReadAllBytes(dir + "Images/Icons/Dice.Png"));
                Sprite icon = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

                RadialUI.RadialUIPlugin.AddCustomButtonOnCharacter(Guid, new MapMenu.ItemArgs
                {
                    Action = (mmi, obj) =>
                    {
                        SystemMessage.AskForTextInput("Roller", "What Should I Roll: ", "Roll", (rollcode) => { RollHandler.ProcessRollRequest(radialCreature, rollcode); }, null, "Cancel", null);
                    },
                    Icon = icon,
                    Title = "Roller",
                    CloseMenuOnActivate = true
                }, Reporter);
            }

            // Post plugin on the TaleSpire main page
            StateDetection.Initialize(this.GetType());
        }

        /// <summary>
        /// Method to track which asset has the radial menu open
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="radialMenu"></param>
        /// <returns></returns>
        private bool Reporter(NGuid selected, NGuid radialMenu)
        {
            radialCreature = new CreatureGuid(radialMenu);
            return true;
        }

        /// <summary>
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            // Ensure that there is a camera controller instance
            if (CameraController.HasInstance)
            {
                // Ensure that there is a board session manager instance
                if (BoardSessionManager.HasInstance)
                {
                    // Ensure that there is a board
                    if (BoardSessionManager.HasBoardAndIsInNominalState)
                    {
                        // Ensure that the board is not loading
                        if (!BoardSessionManager.IsLoading)
                        {
                            // Check for roll shortcut
                            if (StrictKeyCheck(triggerKey[1].Value))
                            {
                                SystemMessage.AskForTextInput("Roller", "What Should I Roll: ", "Roll", (rollcode) => { RollHandler.ProcessRollRequest(LocalClient.SelectedCreatureId, rollcode); }, null, "Cancel", null);
                            }
                            else if (StrictKeyCheck(triggerKey[0].Value))
                            {
                                SystemMessage.AskForTextInput("Roller", "What Is The Edition: ", "Set", (edition) => { RollHandler.SetEdition(edition); }, null, "Cancel", null);
                            }
                        }
                    }
                }
            }
        }

        public static string GetEdition()
        {
            return RollHandler.GetEdition();
        }

        public static void SetEdition(string setting)
        {
            RollHandler.SetEdition(setting);
        }

        public static string GetCreatureName(CreatureBoardAsset asset)
        {
            string name = asset.Creature.Name;
            if (name.Contains("<size=0>")) { name = name.Substring(0, name.IndexOf("<size=0>")).Trim(); }
            return name;
        }

        public bool StrictKeyCheck(KeyboardShortcut check)
        {
            // Check main key
            if (!check.IsUp()) { return false; }
            // Check that specified modifiers are pressed while all other modifiers are not pressed
            foreach (KeyCode modifier in new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftShift, KeyCode.RightShift })
            {
                if (Input.GetKey(modifier) != check.Modifiers.Contains(modifier)) { return false; }
            }
            return true;
        }
    }
}
