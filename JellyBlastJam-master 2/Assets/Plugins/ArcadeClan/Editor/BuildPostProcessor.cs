using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.IO;

public class BuildPostProcessor
    {
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.iOS)
            {
                string plistPath = System.IO.Path.Combine(pathToBuiltProject, "Info.plist");

                if (File.Exists(plistPath))
                {
                    string plistContents = File.ReadAllText(plistPath);

                    if (plistContents.Contains("<key>ITSAppUsesNonExemptEncryption</key>"))
                    {
                        plistContents = plistContents.Replace("<key>ITSAppUsesNonExemptEncryption</key>\n\t<true/>",
                            "<key>ITSAppUsesNonExemptEncryption</key>\n\t<false/>");
                    }
                    else
                    {
                        plistContents = plistContents.Replace("</dict>",
                            "\t<key>ITSAppUsesNonExemptEncryption</key>\n\t<false/>\n</dict>");
                    }

                    File.WriteAllText(plistPath, plistContents);
                }
            }
        }
    }

