using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ShaderBox
{
    public class ImGuiTexturePicker
    {
        private const string TexturePickerID = "###TexturePicker";
        private static readonly Dictionary<object, ImGuiTexturePicker> s_filePickers = new Dictionary<object, ImGuiTexturePicker>();
        private static readonly Vector2 DefaultFilePickerSize = new Vector2(600, 400);

        public string CurrentFolder { get; set; }
        public string SelectedFile { get; set; }

        public static ImGuiTexturePicker GetFilePicker(object o, string startingPath)
        {
            if (File.Exists(startingPath))
            {
                startingPath = new FileInfo(startingPath).DirectoryName;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                //startingPath = Application.Instance.ProjectContext.GetAssetRootPath();
                
                if (string.IsNullOrEmpty(startingPath))
                {
                    startingPath = AppContext.BaseDirectory;
                }
            }

            if (!s_filePickers.TryGetValue(o, out ImGuiTexturePicker fp))
            {
                fp = new ImGuiTexturePicker();
                fp.CurrentFolder = startingPath;
                s_filePickers.Add(o, fp);
            }

            return fp;
        }

        public bool Draw(ref string selected)
        {
            string label = "<Select File>";
            if (selected != null)
            {
                var realFile = new System.IO.FileInfo(selected);
                //if (Util.TryGetFileInfo(selected, out FileInfo realFile))
                {
                    label = realFile.Name;
                }
                //else
                //{
                //    label = "<Select File>";
                //}
            }
            if (ImGui.Button(label))
            {
                ImGui.OpenPopup(TexturePickerID);
            }

            bool result = false;
            bool open = true;
            ImGui.SetNextWindowSize(DefaultFilePickerSize, ImGuiCond.FirstUseEver);
            if (ImGui.BeginPopupModal(TexturePickerID, ref open, ImGuiWindowFlags.NoTitleBar))
            {
                result = DrawFolder(ref selected, true);
                ImGui.EndPopup();
            }

            return result;
        }

        Vector4 directoryColor = new Vector4(0, 0, 1, 1);

        private bool DrawFolder(ref string selected, bool returnOnSelection = false)
        {
            ImGui.Text("Current Folder: " + CurrentFolder);
            bool result = false;

            if (ImGui.BeginChildFrame(1, new Vector2(0, 350), ImGuiWindowFlags.None))
            {
                DirectoryInfo di = new DirectoryInfo(CurrentFolder);
                if (di.Exists && !di.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    if (di.Parent != null)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, directoryColor);
                        if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                        {
                            CurrentFolder = di.Parent.FullName;
                        }
                        ImGui.PopStyleColor();
                    }
                    foreach (var fse in Directory.EnumerateFileSystemEntries(di.FullName))
                    {
                        if (Directory.Exists(fse))
                        {
                            string name = Path.GetFileName(fse);
                            ImGui.PushStyleColor(ImGuiCol.Text, directoryColor);
                            if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups))
                            {
                                CurrentFolder = fse;
                            }
                            ImGui.PopStyleColor();
                        }
                        else
                        {
                            string name = Path.GetFileName(fse);
                            bool isSelected = SelectedFile == fse;
                            if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups))
                            {
                                SelectedFile = fse;
                                if (returnOnSelection)
                                {
                                    result = true;
                                    selected = SelectedFile;
                                }
                            }
                            if (ImGui.IsMouseDoubleClicked(0))
                            {
                                result = true;
                                selected = SelectedFile;
                                ImGui.CloseCurrentPopup();
                            }
                        }
                    }
                }

            }
            ImGui.EndChildFrame();


            if (ImGui.Button("Cancel"))
            {
                result = false;
                ImGui.CloseCurrentPopup();
            }

            if (SelectedFile != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    selected = SelectedFile;
                    ImGui.CloseCurrentPopup();
                }
            }

            return result;
        }
    }
}
