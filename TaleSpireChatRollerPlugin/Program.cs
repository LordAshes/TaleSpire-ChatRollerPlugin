using BepInEx;

namespace LordAshes
{
    [BepInPlugin(Guid, "Chat Roller Plug-In", Version)]
    public class ChatRollerPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Guid = "org.lordashes.plugins.chatroller";
        public const string Version = "1.3.0.0";

        // Content directory
        private static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Chat handelr
        private static ChatHandler chatHandler = new ChatHandler(dir);

        /// <summary>
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Chat Roller Plugin Active. Expecting character sheets in '"+dir+"/Misc',");
            UnityEngine.Debug.Log("with file name matching the edition dot character name and an .CHS extension.");

            // Post plugin on the TaleSpire main page
            StateDetection.Initialize(this.GetType());
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
                            chatHandler.CheckForPickedUpAssets();
                            chatHandler.CheckChatRequests();
                        }
                    }
                }
            }
        }

        public static string GetEdition()
        {
            return chatHandler.GetEdition();
        }

        public static void SetEdition(string setting)
        {
            chatHandler.SetEdition(setting);
        }

        public static string GetCreatureName(CreatureBoardAsset asset)
        {
            string name = asset.Creature.Name;
            if (name.Contains("<size=0>")) { name = name.Substring(0, name.IndexOf("<size=0>")).Trim(); }
            return name;
        }
    }
}
