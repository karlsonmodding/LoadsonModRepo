using Loadson;
using LoadsonAPI;
using System.Net.Security;
using System.Net;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace LoadsonModRepo
{
    public class Main : Mod
    {
        private const string REPOSITORY_URL = "https://raw.githubusercontent.com/karlsonmodding/LoadsonModRepo/master/Tracker/repository";

        private static Dictionary<string, (string, string)> repository = new Dictionary<string, (string, string)>();
        private const string TAG_OPEN = "<LMR>";
        private const string TAG_CLOSE = "</LMR>";
        private static List<string> updated = new List<string>();
        private static string dialog = "";

        private static int wid;
        private static Rect wir = new Rect((Screen.width - 800) / 2, (Screen.height - 500) / 2, 800, 500);

        public override void OnEnable()
        {
            wid = ImGUI_WID.GetWindowId();
            // download repository
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            WebClient wc = new WebClient();
            string s = wc.DownloadString(REPOSITORY_URL + "?random=" + new System.Random().Next().ToString());
            Loadson.Console.Log(s);
            var lines = s.Split('\n');
            foreach (var line in lines)
            {
                if (line.Trim().Split('|').Length != 3) continue; // invalid line
                var v = line.Trim().Split('|');
                repository.Add(v[0], (v[1], v[2]));
            }

            // check for all pre-loaded mods
            // Note for developers: the reason I need to use reflection
            // is to prevent regular people from messing with the wrong things
            // because this might lead to unexpected bugs and glitches.
            // It's better to obfuscate behind reflection internal things.
            Type modEntry = typeof(Mod).Assembly.GetType("LoadsonInternal.ModEntry");
            IEnumerable mods = (IEnumerable)modEntry.GetField("List").GetValue(null);
            foreach (var mod in mods)
            {
                string guid = (string)modEntry.GetField("ModGUID").GetValue(mod);
                string file = (string)modEntry.GetField("FilePath").GetValue(mod);
                if (repository.ContainsKey(guid))
                {
                    Loadson.Console.Log("Checking " + guid);
                    // this mod is tracked by the repo, check version
                    string description = (string)modEntry.GetField("Description").GetValue(mod);
                    bool update = false;
                    if (!description.Contains(TAG_OPEN) || !description.Contains(TAG_CLOSE))
                    {
                        description += "\n<color=red>[LMR] Couldn't find version tag, mark as needing update</color>";
                        update = true; // this mod doesn't respect <LMR>version</LMR> format for version tracking, so we update if it's in our repo
                    }
                    else
                    {
                        int start = description.IndexOf(TAG_OPEN) + TAG_OPEN.Length;
                        int end = description.IndexOf(TAG_CLOSE);
                        string ver = description.Substring(start, end - start);
                        if (ver != repository[guid].Item1)
                            update = true;
                        if (!update)
                            Loadson.Console.Log("<i> </i><color=green>Up to date</color> (" + ver + ")");
                        else
                            Loadson.Console.Log("<i> </i><color=red>Version change</color> (" + ver + " -> " + repository[guid].Item1 + ")");
                        description = description.Substring(0, start - TAG_OPEN.Length) + "<color=green>Tracked by Loadson Mod Repository (reported version " + ver + ")</color>" + description.Substring(end + TAG_CLOSE.Length);
                    }
                    // change description text
                    modEntry.GetField("Description").SetValue(mod, description);
                    if (update)
                    {
                        Loadson.Console.Log("<i> </i>Download URL: " + repository[guid].Item2);
                        // update mod
                        try
                        {
                            File.WriteAllBytes(file, wc.DownloadData(repository[guid].Item2));
                            updated.Add(guid);
                            Loadson.Console.Log("<i> </i><color=green>Updated.</color>");
                        }
                        catch
                        {
                            Loadson.Console.Log("<i> </i><color=red>Failed to update.</color>");
                        }

                    }
                }
                // TODO: maybe check if mod has LMR tag but is not in repo and alert user
            }
            wc.Dispose();
            if (updated.Count > 0)
            {
                dialog = "The following mods have been automatically updated:\n";
                foreach (var m in updated)
                {
                    dialog += "- " + m + "\n";
                }
                dialog += "\nDo you want to restart Loadson now?";
            }
        }

        public override void OnGUI()
        {
            if (dialog != "")
            {
                GUI.Box(wir, "");
                wir = GUI.Window(wid, wir, (_) =>
                {
                    GUI.Label(new Rect(5, 15, 790, 450), dialog);
                    if (GUI.Button(new Rect(100, 475, 200, 20), "Yes"))
                    {
                        // from https://github.com/karlsonmodding/Loadson/blob/main/Loadson/LoadsonInternal/KernelUpdater.cs#L52
                        Environment.SetEnvironmentVariable("DOORSTOP_INITIALIZED", null);
                        Environment.SetEnvironmentVariable("DOORSTOP_DISABLE", null);
                        // restart
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Path.Combine(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Loadson", "Internal", "karlsonpath")), "Karlson.exe"),
                            WorkingDirectory = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Loadson", "Internal", "karlsonpath")),
                            Arguments = "-silent",
                        });
                        // quit app
                        Application.Quit();
                    }
                    if (GUI.Button(new Rect(500, 475, 200, 20), "No"))
                    {
                        dialog = "";
                    }
                    GUI.DragWindow(new Rect(0, 0, 800, 10));
                }, "Loadson Mod Repository");
            }
        }
    }
}
