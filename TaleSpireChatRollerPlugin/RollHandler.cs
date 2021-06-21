using System.Data;

namespace LordAshes
{
    public partial class ChatRollerPlugin
    {
        public static class RollHandler
        {
            // Randomizer
            private static System.Random rnd = new System.Random();

            // Edition
            private static string edition;

            /// <summary>
            /// Method to process char requests
            /// </summary>
            /// <param name="request">Chat request without the identifier</param>
            /// <returns>Message to be displayed or null</returns>
            public static void ProcessRollRequest(CreatureGuid activeObjectId, string request)
            {
                UnityEngine.Debug.Log("Roll Request: " + request);

                CreatureBoardAsset activeObject;
                CreaturePresenter.TryGetAsset(activeObjectId, out activeObject);

                string expanded = request;

                UnityEngine.Debug.Log("Replacing Place Holders Based On Edition '"+GetEdition()+"'");

                expanded = ReplacePlaceHolders(expanded, activeObject);

                UnityEngine.Debug.Log("Modified Roll Request After Placeholder Replacement: " + expanded);

                expanded = ReplaceDiceRolls(expanded);

                UnityEngine.Debug.Log("Modified Roll Request After Dice Roll Replacement: " + expanded);

                DataTable dt = new DataTable();

                object v = null;
                try
                {
                    v = dt.Compute(expanded.Replace("=", "~"), "");

                    // string msg = "<align=\"center\"><size=24>" + WordCase(request) + "</size></align>\r\n<align=\"center\"><size=20>" + expanded + "</size></align>\r\n<align=\"center\"><size=32>" + v.ToString().Replace("~", "=") + "</size></align>";
                    string msg = "<align=\"center\">" + WordCase(request) + "\r\n" + expanded + "\r\n" + v.ToString().Replace("~", "=") + "</align>";
                    ChatManager.SendChatMessage(msg, activeObject.Creature.CreatureId.Value);
                }
                catch (System.Exception)
                {
                    SystemMessage.DisplayInfoText("Roll Error: Resolved down to:\r\n" + expanded);
                }

            }

            /// <summary>
            /// Allows setting an edition. This can be used by plugins that depend on this plugin.
            /// </summary>
            /// <param name="setting"></param>
            public static void SetEdition(string setting)
            {
                if (setting != "") { edition = setting + "."; } else { edition = ""; }
            }

            /// <summary>
            /// Method to get the edition
            /// </summary>
            /// <returns></returns>
            public static string GetEdition()
            {
                return edition;
            }

            private static string ReplacePlaceHolders(string request, CreatureBoardAsset activeObject)
            {
                if (activeObject == null) { UnityEngine.Debug.LogWarning("No Mini Selected. Placeholder Replacement Cannot Be Performed."); return request; }

                UnityEngine.Debug.Log("Using '" + ChatRollerPlugin.dir + "Misc/" + GetEdition() + ChatRollerPlugin.GetCreatureName(activeObject) + ".chs' for lookup data.");
                string charSheet = ChatRollerPlugin.dir + "Misc/" + GetEdition() + ChatRollerPlugin.GetCreatureName(activeObject) + ".chs";
                if (!System.IO.File.Exists(charSheet)) { UnityEngine.Debug.LogWarning("Missing Character Sheet '" + charSheet + "'."); return request; }

                string[] replacements = System.IO.File.ReadAllLines(charSheet);

                for (int l = 0; l < 5; l++)
                {
                    string[] segments = request.Split('\'');
                    for (int s = 0; s < segments.Length; s = s + 2)
                    {
                        foreach (string rep in replacements)
                        {
                            string[] parts = rep.Split('=');
                            if (parts.Length == 2)
                            {
                                segments[s] = segments[s].Replace(parts[0], parts[1]);
                            }
                        }
                    }
                    request = segments[0];
                    for (int s = 1; s < segments.Length; s++)
                    {
                        request = request + "'" + segments[s];
                    }
                    UnityEngine.Debug.Log("Modified Request Is Now: " + request);
                }

                return request;
            }

            private static string ReplaceDiceRolls(string request)
            {
                bool quoted = false;
                for (int i = 0; i < request.Length; i++)
                {
                    if (request.Substring(i, 1) == "'") { quoted = !quoted; }
                    if (!quoted)
                    {
                        if (request.Substring(i, 1).ToUpper() == "D")
                        {
                            // Dice Notation
                            int s = i - 1;
                            while ("0123456789".Contains(request.Substring(s, 1)))
                            {
                                s = s - 1;
                                if (s == -1) { break; }
                            }
                            s++;
                            int numDice = int.Parse(request.Substring(s, i - s));
                            int e = i + 1;
                            while ("0123456789".Contains(request.Substring(e, 1)))
                            {
                                e = e + 1;
                                if (e == request.Length) { break; }
                            }
                            e--;
                            int numFaces = int.Parse(request.Substring(i + 1, e - i));
                            string rolls = "(";
                            for (int d = 0; d < numDice; d++)
                            {
                                int roll = rnd.Next(1, numFaces + 1);
                                rolls = rolls + roll + "+";
                            }
                            rolls = rolls.Substring(0, rolls.Length - 1) + ")";
                            request = request.Substring(0, s) + rolls + request.Substring(e + 1);
                            i = 0;
                        }
                    }
                }

                UnityEngine.Debug.Log("Modified Request Is Now: " + request);

                return request;
            }

            private static string WordCase(string txt)
            {
                string result = "";
                int p = 0;
                for (; p < txt.Length; p++)
                {
                    if (!("{_}".Contains(txt.Substring(p, 1))))
                    {
                        result = txt.Substring(p, 1).ToUpper();
                        p++;
                        break;
                    }
                }
                for (; p < txt.Length; p++)
                {
                    if ("{_}".Contains(txt.Substring(p, 1)))
                    {
                        result = result + " ";
                    }
                    else if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(txt.Substring(p - 1, 1)))
                    {
                        // Previous character was a letter
                        result = result + txt.Substring(p, 1).ToLower();
                    }
                    else
                    {
                        // Previous character was not a letter
                        result = result + txt.Substring(p, 1).ToUpper();
                    }
                }
                return result;
            }
        }
    }
}
