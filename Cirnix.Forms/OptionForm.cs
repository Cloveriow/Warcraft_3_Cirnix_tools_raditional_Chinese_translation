using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cirnix.Global;
using Cirnix.KeyHook;
using Cirnix.Memory;
using Cirnix.Worker;

using ModernFolderBrowserDialog;

using static Cirnix.Forms.NativeMethods;
using static Cirnix.Global.Globals;
using static Cirnix.Global.Hotkey;
using static Cirnix.Global.Locale;
using static Cirnix.Worker.MainWorker;

namespace Cirnix.Forms
{
    internal partial class OptionForm : DraggableLabelForm
    {
        internal Action<bool> ChannelChatState;
        private enum SelectedKeypadType
        {
            Key7 = 7,
            Key8 = 8,
            Key4 = 4,
            Key5 = 5,
            Key1 = 1,
            Key2 = 2,
            None = 0
        }
        private enum SelectedAutoMouseType { Off, Left, Right }
        private SelectedKeypadType TargetKey;
        private SelectedAutoMouseType TargetMouse;
        private bool IsRemapKeyInput = false, IsChatHotkeyInput = false, IsAutoMouseInput = false, IsUpdating = false;
        private int CurrentChatIndex = 0;

        internal OptionForm()
        {
            InitializeComponent();
            IsUpdating = true;
            Icon = Global.Properties.Resources.CirnixIcon;
            Label_Title.MouseDown += new MouseEventHandler(Label_Title_MouseDown);
            Label_Title.MouseMove += new MouseEventHandler(Label_Title_MouseMove);
            Label_Title.MouseUp += new MouseEventHandler(Label_Title_MouseUp);
            Label_Title.Text = Text = $"{Global.Theme.Title} 設置和幫助";
            Toggle_CommandHide.Checked = Settings.IsCommandHide;
            #region [    Warcraft Tab Initialize    ]
            Num_GameDelay.Value = Settings.GameDelay;
            Num_GameStartDelay.Value = Math.Abs(Convert.ToInt32(Settings.StartSpeed));
            Toggle_HpCommandAuto.Checked = Settings.IsAutoHp;
            TB_MemoryOptimizationDelay.Text = Settings.MemoryOptimizeCoolDown.ToString();
            Toggle_MemoryOptimizationDelay.Checked = Settings.IsMemoryOptimize;
            Toggle_OutGameAutoMemoryOptimization.Checked = Settings.IsOptimizeAfterEndGame;
            Toggle_CheatMapCheck.Checked = Settings.IsCheatMapCheck;
            Combo_ScreenShotExtension.Text = Settings.ConvertExtention;
            Toggle_AutoConvert.Checked = Settings.IsConvertScreenShot;
            Toggle_RemoveOriginal.Checked = Settings.IsOriginalRemove;
            Toggle_War3FixClipboard.Checked = Settings.IsFixClipboard;
            Num_CameraDistance.Value = Convert.ToInt32(Settings.CameraDistance);
            Num_CameraX.Value = Convert.ToInt32(Settings.CameraAngleX);
            Num_CameraY.Value = Convert.ToInt32(Settings.CameraAngleY);
            Toggle_ChannelChatViewer.Checked = Settings.IsChannelChatDetect;
            TB_InstallPath.Text = Settings.InstallPath;
            #endregion
            #region [    RPG Tab Initialize    ]
            RPGListBox.BeginUpdate();
            RPGListBox.Items.Clear();
            RPGListBox.Items.Add("(새로 만들기)");
            if(saveFilePath.Count > 0)
                foreach (SavePath item in saveFilePath)
                    RPGListBox.Items.Add(saveFilePath.ConvertName(item.nameEN));
            RPGListBox.EndUpdate();
            CB_AutoLoad.Checked = Settings.IsAutoLoad;
            CB_NewMapSaveFileAuto.Checked = Settings.IsGrabitiSaveAutoAdd;
            CB_SavesReplayAutoSave.Checked = CB_NoSavesReplaySave.Enabled = Settings.IsAutoReplay;
            CB_NoSavesReplaySave.Checked = Settings.NoSavedReplaySave;
            TabControl_CommandPreset.SelectedIndex = Settings.SelectedCommand - 1;
            TB_CommandPreset1.Text = Settings.CommandPreset1.Replace("\n\n", "\n").Replace("\n", "\r\n");
            TB_CommandPreset2.Text = Settings.CommandPreset2.Replace("\n\n", "\n").Replace("\n", "\r\n");
            TB_CommandPreset3.Text = Settings.CommandPreset3.Replace("\n\n", "\n").Replace("\n", "\r\n");
            #endregion
            #region [    Macro Tab Initialize    ]
            #region [    Smart Key Showing Status Initialize    ]
            if (isSmartKey(Keys.Q))
                Qbutton.BackgroundImage = Properties.Resources.Q1button;
            if (isSmartKey(Keys.W))
                Wbutton.BackgroundImage = Properties.Resources.W1button;
            if (isSmartKey(Keys.E))
                Ebutton.BackgroundImage = Properties.Resources.E1button;
            if (isSmartKey(Keys.R))
                Rbutton.BackgroundImage = Properties.Resources.R1button;
            if (isSmartKey(Keys.T))
                Tbutton.BackgroundImage = Properties.Resources.T1button;
            if (isSmartKey(Keys.A))
                Abutton.BackgroundImage = Properties.Resources.A1button;
            if (isSmartKey(Keys.D))
                Dbutton.BackgroundImage = Properties.Resources.D1button;
            if (isSmartKey(Keys.F))
                Fbutton.BackgroundImage = Properties.Resources.F1button;
            if (isSmartKey(Keys.G))
                Gbutton.BackgroundImage = Properties.Resources.G1button;
            if (isSmartKey(Keys.Z))
                Zbutton.BackgroundImage = Properties.Resources.Z1button;
            if (isSmartKey(Keys.X))
                Xbutton.BackgroundImage = Properties.Resources.X1button;
            if (isSmartKey(Keys.C))
                Cbutton.BackgroundImage = Properties.Resources.C1button;
            if (isSmartKey(Keys.V))
                Vbutton.BackgroundImage = Properties.Resources.V1button;
            SmartKeyPreventionType = Settings.SmartKeyPreventionType;
            #endregion
            #region [    Key Remapping Showing Status Initialize    ]
            if (Settings.KeyMap7 != 0)
            {
                Key7Text.Text = GetHotkeyString(((Keys)Settings.KeyMap7));
                Key7.Text = "X";
            }
            if (Settings.KeyMap8 != 0)
            {
                Key8Text.Text = GetHotkeyString(((Keys)Settings.KeyMap8));
                Key8.Text = "X";
            }
            if (Settings.KeyMap4 != 0)
            {
                Key4Text.Text = GetHotkeyString(((Keys)Settings.KeyMap4));
                Key4.Text = "X";
            }
            if (Settings.KeyMap5 != 0)
            {
                Key5Text.Text = GetHotkeyString(((Keys)Settings.KeyMap5));
                Key5.Text = "X";
            }
            if (Settings.KeyMap1 != 0)
            {
                Key1Text.Text = GetHotkeyString(((Keys)Settings.KeyMap1));
                Key1.Text = "X";
            }
            if (Settings.KeyMap2 != 0)
            {
                Key2Text.Text = GetHotkeyString(((Keys)Settings.KeyMap2));
                Key2.Text = "X";
            }
            Toggle_KeyRemapping.Checked = Settings.IsKeyRemapped;
            #endregion
            #region [    Text Macro Showing Status Initialize   ]
            TB_ChatMacro.Text = chatHotkeyList[CurrentChatIndex].ChatMessage;
            Label_ChatHotkey.Text = GetHotkeyString(chatHotkeyList[CurrentChatIndex].Hotkey);
            BTN_SetChatHotkey.Text = chatHotkeyList.IsKeyRegisted(CurrentChatIndex) ? "단축키 해제" : "단축키 지정";
            Toggle_ChatMacro.Checked = chatHotkeyList[CurrentChatIndex].IsRegisted;
            for (; CurrentChatIndex < 10; CurrentChatIndex++)
            {
                if (chatHotkeyList.IsKeyRegisted(CurrentChatIndex))
                    RB_Chat_SetCurrentFont(0, true);
                if (chatHotkeyList[CurrentChatIndex].IsRegisted)
                    RB_Chat_SetCurrentFont(1, true);
            }
            CurrentChatIndex = 0;
            #endregion
            #region [    Auto Mouse Showing Status Initialize    ]
            Toggle_AutoMouse.Checked = AutoMouse.Enabled;
            BTN_AutoLeftMouseOn.Text = AutoMouse.LeftStartKey == 0 ? "좌클" : "해제";
            BTN_AutoRightMouseOn.Text = AutoMouse.RightStartKey == 0 ? "우클" : "해제";
            BTN_AutoMouseOff.Text = AutoMouse.EndKey == 0 ? "종료" : "해제";
            Label_AutoLeftMouseOn.Text = GetHotkeyString(AutoMouse.LeftStartKey);
            Label_AutoRightMouseOn.Text = GetHotkeyString(AutoMouse.RightStartKey);
            Label_AutoMouseOff.Text = GetHotkeyString(AutoMouse.EndKey);
            Label_AutoMouseDelay.Text = $"{AutoMouse.Interval} ms";
            TrB_AutoMouseDelay.Value = AutoMouse.Interval;
            #endregion
            #endregion
            Toggle_AutoFrequency.Checked = Settings.IsAutoFrequency;
            Number_ChatFrequency.Enabled = BTN_DetectFrequency.Enabled = !Settings.IsAutoFrequency;
            Number_ChatFrequency.Value = Settings.ChatFrequency + 1;
            Banlistload();
            IsUpdating = false;
        }
        private void OptionForm_Activated(object sender, EventArgs e)
        {
            IsUpdating = true;
            #region [    RPG Tab Update    ]
            HeroListBox_Update();
            #endregion
            #region [    Warcraft Tab Update    ]
            Num_GameDelay.Value = Settings.GameDelay;
            Num_GameStartDelay.Value = Settings.StartSpeed >= 1 ? (int)Settings.StartSpeed : 0;
            Num_CameraDistance.Value = Settings.CameraDistance > 6000 ? 6000 : (int)Settings.CameraDistance;
            Num_CameraY.Value = ((int)Settings.CameraAngleY) % 360;
            Num_CameraX.Value = ((int)Settings.CameraAngleX) % 360;
            IsMixFIleFormEnabled = IsMixFileInstalled;
            GetMixFileStates();
            #endregion
            Toggle_KeyRemapping.Checked = Settings.IsKeyRemapped;
            IsUpdating = false;
        }
        #region [    Warcraft Tab Setting    ]
        private void DelayApplyBTN_Click(object sender, EventArgs e)
        {
            Settings.GameDelay = ControlDelay.GameDelay = Convert.ToInt32(Num_GameDelay.Value);
            Settings.StartSpeed = GameDll.StartDelay = Convert.ToInt32(Num_GameStartDelay.Value) <= 0.01 ? 0.01f : Convert.ToInt32(Num_GameStartDelay.Value);
        }
        private void HpCommandAutoToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsAutoHp = Toggle_HpCommandAuto.Checked;
        }
        private void MemoryOptimizationEdit_Leave(object sender, EventArgs e)
        {
            try
            {
                Settings.MemoryOptimizeCoolDown = int.Parse(TB_MemoryOptimizationDelay.Text);
            }
            catch
            {
                TB_MemoryOptimizationDelay.Text = Settings.MemoryOptimizeCoolDown.ToString();
            }
        }
        private void MemoryOptimizationToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsMemoryOptimize = Toggle_MemoryOptimizationDelay.Checked;
        }
        private void OutGameAutoMemoryOptimizationToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsOptimizeAfterEndGame = Toggle_OutGameAutoMemoryOptimization.Checked;
        }
        private void Toggle_CheatMapCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsCheatMapCheck = Toggle_CheatMapCheck.Checked;
        }
        private void BTN_CameraPresetJurrasic_Click(object sender, EventArgs e)
        {
            Num_CameraDistance.Value = 3318;
            GameDll.CameraDistance = Settings.CameraDistance = 3318.4f;
            Num_CameraY.Value = 283;
            GameDll.CameraAngleY = Settings.CameraAngleY = 283.4f;
            Num_CameraX.Value = 101;
            GameDll.CameraAngleX = Settings.CameraAngleX = 100.7f;
            GameDll.CameraInit();
        }
        private void CameraResetBTN_Click(object sender, EventArgs e)
        {
            Num_CameraDistance.Value = 1650;
            GameDll.CameraDistance = Settings.CameraDistance = 1650.0f;
            Num_CameraY.Value = 304;
            GameDll.CameraAngleY = Settings.CameraAngleY = 304.0f;
            Num_CameraX.Value = 90;
            GameDll.CameraAngleX = Settings.CameraAngleX = 90.0f;
            GameDll.CameraInit();
        }
        private void CameraApplyBTN_Click(object sender, EventArgs e)
        {
            GameDll.CameraDistance = Settings.CameraDistance = Convert.ToInt32(Num_CameraDistance.Value);
            GameDll.CameraAngleY = Settings.CameraAngleY = Convert.ToInt32(Num_CameraY.Value);
            GameDll.CameraAngleX = Settings.CameraAngleX = Convert.ToInt32(Num_CameraX.Value);
            GameDll.CameraInit();
        }
        private void ScreenShotFileNameExtensionList_TextChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.ConvertExtention = Combo_ScreenShotExtension.Text;
        }
        private void TgaAutoConvertToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            ScreenShotWatcher.EnableRaisingEvents = Settings.IsConvertScreenShot = Toggle_AutoConvert.Checked;
        }
        private void TgaOriginallyDeleteToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsOriginalRemove = Toggle_RemoveOriginal.Checked;
        }
        private void War3AutoKillToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsFixClipboard = Toggle_War3FixClipboard.Checked;
            if (Toggle_War3FixClipboard.Checked)
                ClipboardConverter.FixStart();
            else
                ClipboardConverter.FixEnd();
        }
        private void CommandHideToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsCommandHide = Toggle_CommandHide.Checked;
        }
        private void Toggle_ChannelChatViewer_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            ChannelChatState(Settings.IsChannelChatDetect = Toggle_ChannelChatViewer.Checked);
        }
        #region [    Mix File Setting    ]
        private void GetMixFileStates()
        {
            Manabar = GetPrivateProfileInt("Cirnix", "Mana Bar", 0, Settings.InstallPath + @"\Cirnix.ini") == 1;
            SpeedNumberize = GetPrivateProfileInt("Cirnix", "Show AS & MS in Number", 0, Settings.InstallPath + @"\Cirnix.ini") == 1;
        }
        private bool IsMixFileInstalled {
            get {
                if (string.IsNullOrEmpty(Settings.InstallPath)) return false;
                return File.Exists($"{Settings.InstallPath}\\Cirnix.mix");
            }
            set {
                if (string.IsNullOrEmpty(Settings.InstallPath))
                {
                    MetroDialog.OK("경로 오류", "경로가 비어있습니다.\n경로를 입력하세요.");
                    return;
                }
                string path = $"{Settings.InstallPath}\\Cirnix";
                if (value)
                {
                    if (!File.Exists(path + ".mix"))
                    {
                        File.WriteAllBytes(path + ".mix", Global.Properties.Resources.Cirnix);
                        WritePrivateProfileString("Cirnix", "Mana Bar", "0", path + ".ini");
                        WritePrivateProfileString("Cirnix", "Show AS & MS in Number", "0", path + ".ini");
                    }
                    return;
                }
                try
                {
                    File.Delete(path + ".mix");
                    File.Delete(path + ".ini");
                }
                catch
                {
                    MetroDialog.OK("파일 접근 오류", "워크래프트 실행 중에는 파일을 제거할 수 없습니다.\n먼저 워크래프트를 종료하세요.");
                }
            }
        }
        private bool IsMixFIleFormEnabled {
            get => BTN_UninstallMix.Enabled;
            set {
                GB_SpeenNumberize.Enabled =
                BTN_UninstallMix.Enabled =
                GB_Manabar.Enabled = value;
                BTN_InstallMix.Enabled = !value;
            }
        }
        private bool SpeedNumberize {
            get => RB_EnableSpeedNumberize.Checked;
            set {
                RB_EnableSpeedNumberize.Checked = value;
                RB_DisableSpeedNumberize.Checked = !value;
            }
        }
        private bool Manabar {
            get => RB_EnableManabar.Checked;
            set {
                RB_EnableManabar.Checked = value;
                RB_DisableManabar.Checked = !value;
            }
        }
        private void BTN_InstallMix_Click(object sender, EventArgs e)
        {
            IsMixFileInstalled = true;
            IsMixFIleFormEnabled = IsMixFileInstalled;
        }
        private void BTN_UninstallMix_Click(object sender, EventArgs e)
        {
            IsMixFileInstalled = false;
            IsMixFIleFormEnabled = IsMixFileInstalled;
            GetMixFileStates();
        }
        private void BTN_InstallPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog FDialog = new OpenFileDialog())
            {
                FDialog.Title = "워크래프트 실행 파일을 선택하세요.";
                FDialog.Filter = "워크래프트 EXE파일|Warcraft III.exe";
                if (FDialog.ShowDialog() != DialogResult.OK) return;
                Settings.InstallPath = TB_InstallPath.Text = Path.GetDirectoryName(FDialog.FileName);
            }
            IsMixFIleFormEnabled = IsMixFileInstalled;
        }
        private void RB_EnableSpeedNumberize_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            WritePrivateProfileString("Cirnix", "Show AS & MS in Number", RB_EnableSpeedNumberize.Checked ? "1" : "0", Settings.InstallPath + @"\Cirnix.ini");
        }
        private void RB_EnableManabar_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            WritePrivateProfileString("Cirnix", "Mana Bar", RB_EnableManabar.Checked ? "1" : "0", Settings.InstallPath + @"\Cirnix.ini");
        }
        #endregion
        #endregion
        #region [    RPG Tab Setting    ]
        #region [    SaveList Setting    ]
        private void RPGListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            TB_RPGKR.Text = TB_RPGEN.Text = TB_RPGPath.Text = string.Empty;
            BTN_RPGDel.Enabled = BTN_RPGFolder.Enabled = false;
            HeroListBox_Update();
            if (RPGListBox.SelectedIndex == -1)
            {
                TB_RPGKR.Enabled = TB_RPGEN.Enabled = TB_RPGPath.Enabled = BTN_RPGPath.Enabled = BTN_RPGAddMod.Enabled = false;
                BTN_RPGAddMod.Text = "添加";
                return;
            }
            TB_RPGKR.Enabled = TB_RPGEN.Enabled = TB_RPGPath.Enabled = BTN_RPGPath.Enabled = BTN_RPGAddMod.Enabled = true;
            string SelectedName = RPGListBox.SelectedItem.ToString();
            if (SelectedName == "(새로 만들기)")
            {
                BTN_RPGAddMod.Text = "添加";
                return;
            }
            BTN_RPGDel.Enabled = BTN_RPGFolder.Enabled = true;
            BTN_RPGAddMod.Text = "改變";
            string NameEN = saveFilePath.ConvertName(SelectedName, true);
            if (SelectedName != NameEN) TB_RPGKR.Text = SelectedName;
            TB_RPGEN.Text = NameEN;
            TB_RPGPath.Text = saveFilePath.GetFullPath(SelectedName);
        }
        private void HeroListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            TB_HeroName.Text = string.Empty;
            BTN_HeroDel.Enabled = BTN_HeroFolder.Enabled = false;
            if (HeroListBox.SelectedIndex == -1)
            {
                TB_HeroName.Enabled = BTN_HeroAddMod.Enabled = false;
                BTN_HeroAddMod.Text = "添加";
                return;
            }
            TB_HeroName.Enabled = BTN_HeroAddMod.Enabled = true;
            string SelectedName = HeroListBox.SelectedItem.ToString();
            if (SelectedName == "(새로 만들기)")
            {
                BTN_HeroAddMod.Text = "添加";
                return;
            }
            BTN_HeroDel.Enabled = BTN_HeroFolder.Enabled = true;
            BTN_HeroAddMod.Text = "改變";
            TB_HeroName.Text = SelectedName;
        }
        private void HeroListBox_Update()
        {
            HeroListBox.Items.Clear();
            TB_HeroName.Text = string.Empty;
            TB_HeroName.Enabled = BTN_HeroAddMod.Enabled = BTN_HeroDel.Enabled = BTN_HeroFolder.Enabled = false;
            BTN_HeroAddMod.Text = "添加";
            if (RPGListBox.SelectedIndex == -1) return;
            string SelectedName = RPGListBox.SelectedItem.ToString();
            if (SelectedName != "(새로 만들기)")
            {
                HeroListBox.Items.Add("(새로 만들기)");
                foreach (string item in GetDirectoryList(saveFilePath.GetFullPath(SelectedName)))
                    HeroListBox.Items.Add(item);
            }
        }
        #region [    RPG Setting    ]
        private void BTN_RPGPath_Click(object sender, EventArgs e)
        {
            FolderBrowser FDialog = new FolderBrowser();
            FDialog.UseDescriptionForTitle = true;
            FDialog.Description = "選擇保存存檔的文件夾";
            if (FDialog.ShowDialog() != DialogResult.OK) return;
            string DirectoryName = FDialog.SelectedPath;
            if (DirectoryName.IndexOf("CustomMapData") == -1)
            {
                MetroDialog.OK("無效路徑", "似乎不是有效路徑。\nCustomMapData 文件夾必須包含在路徑中。");
                return;
            }
            TB_RPGPath.Text = DirectoryName;
            TB_RPGEN.Text = Path.GetFileNameWithoutExtension(DirectoryName);
        }
        private void BTN_RPGAddMod_Click(object sender, EventArgs e)
        {
            int pathIndex;
            if ((pathIndex = TB_RPGPath.Text.IndexOf("CustomMapData")) == -1)
            {
                MetroDialog.OK("Invalid Path", "路徑似乎無效。\n請再次檢查路徑。");
                return;
            }
            switch (BTN_RPGAddMod.Text)
            {
                case "添加":
                    saveFilePath.AddPath(TB_RPGPath.Text, TB_RPGEN.Text, TB_RPGKR.Text);
                    break;
                case "改變":
                    SavePath item = saveFilePath.GetSavePath(RPGListBox.SelectedItem.ToString());
                    item.nameEN = TB_RPGEN.Text;
                    item.nameKR = TB_RPGKR.Text;
                    item.path = TB_RPGPath.Text.Substring(pathIndex + 13);
                    break;
                default:
                    return;
            }
            saveFilePath.Save();
            ListUpdate(0);
            BTN_Refresh_Click(sender, e);
        }
        private void BTN_RPGDel_Click(object sender, EventArgs e)
        {
            if (RPGListBox.Items.Count <= 2)
            {
                MetroDialog.OK("卸載失敗", "至少必須保留 1 項。");
                return;
            }
            string name = RPGListBox.SelectedItem.ToString();
            if (!MetroDialog.YesNo("檢查以刪除", $"只會從列表中刪除.\n真的 {IsKoreanBlock(name, "을", "를")} 確定要刪除它嗎？")) return;
            saveFilePath.RemovePath(name);
            ListUpdate(0);
            BTN_Refresh_Click(sender, e);
        }
        private void BTN_RPGFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(saveFilePath.GetFullPath(RPGListBox.SelectedItem.ToString()));
        }
        private void BTN_RPGSetRegex_Click(object sender, EventArgs e)
        {

        }
        #endregion
        #region [    Hero Setting    ]
        private void TB_HeroName_TextChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            if (HeroListBox.SelectedIndex < 0)
            {
                BTN_HeroAddMod.Enabled = false;
                return;
            }
            BTN_HeroAddMod.Enabled = !GetDirectorySafeName(TB_HeroName.Text).Equals(HeroListBox.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        private void BTN_HeroAddMod_Click(object sender, EventArgs e)
        {
            TB_HeroName.Text = GetDirectorySafeName(TB_HeroName.Text);
            string RPG = saveFilePath.GetFullPath(RPGListBox.SelectedItem.ToString());
            if (Directory.Exists(RPG + @"\" + TB_HeroName.Text))
            {
                MetroDialog.OK("名字已經存在", $"{IsKoreanBlock(TB_HeroName.Text, "은", "는")} 此名稱已存在。\n請使用其他名稱。.");
                return;
            }
            switch (BTN_HeroAddMod.Text)
            {
                case "添加":
                    Directory.CreateDirectory(saveFilePath.GetFullPath(RPGListBox.SelectedItem.ToString()) + @"\" + TB_HeroName.Text);
                    break;
                case "改變":
                    string Hero = HeroListBox.SelectedItem.ToString();
                    try
                    {
                        Directory.Move(RPG + @"\" + Hero, RPG + @"\" + TB_HeroName.Text);
                    }
                    catch
                    {
                        MetroDialog.OK("更改失敗", "另一個進程正在使用該文件，因此無法更改。");
                        return;
                    }
                    break;
                default:
                    return;
            }
            ListUpdate(1);
            HeroListBox_Update();
        }
        private void BTN_HeroDel_Click(object sender, EventArgs e)
        {
            if (HeroListBox.Items.Count <= 2)
            {
                MetroDialog.OK("刪除失敗", "必須至少保留 1 件物品。");
                return;
            }
            string name = HeroListBox.SelectedItem.ToString();
            if (!MetroDialog.YesNo("檢查以刪除", $"所有包含在分類中的保存文件將被刪除。\n真的 {IsKoreanBlock(name, "을", "를")} 確定要刪除它嗎？")) return;
            string path = saveFilePath.GetFullPath(RPGListBox.SelectedItem.ToString()) + @"\" + name;
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles("*.*", SearchOption.AllDirectories))
                file.Attributes = FileAttributes.Normal;
            try
            {
                Directory.Delete(path, true);
            }
            catch
            {
                MetroDialog.OK("刪除失敗", "另一個進程正在使用該文件，因此無法刪除。");
                return;
            }
            ListUpdate(1);
            HeroListBox_Update();
        }
        private void BTN_HeroFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(saveFilePath.GetFullPath(RPGListBox.SelectedItem.ToString()) + @"\" + TB_HeroName.Text);
        }
        #endregion
        private void BTN_Refresh_Click(object sender, EventArgs e)
        {
            RPGListBox.Items.Clear();
            RPGListBox.Items.Add("(새로 만들기)");
            foreach (SavePath item in saveFilePath)
                RPGListBox.Items.Add(saveFilePath.ConvertName(item.nameEN));
            RPGListBox.SelectedIndex = -1;
            RPGListBox_SelectedIndexChanged(sender, e);
        }
        #endregion
        private void CB_AutoLoad_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsAutoLoad = CB_AutoLoad.Checked;
        }
        private void NewMapSaveFileAutoSense_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsGrabitiSaveAutoAdd = CB_NewMapSaveFileAuto.Checked;
        }
        private void SavesReplayAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            ReplayWatcher.EnableRaisingEvents = CB_NoSavesReplaySave.Enabled = Settings.IsAutoReplay = CB_SavesReplayAutoSave.Checked;
        }
        private void NoSavesReplaySave_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.NoSavedReplaySave = CB_NoSavesReplaySave.Checked;
        }
        #region [    Command Preset Setting    ]
        private void TabControl_CommandPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.SelectedCommand = TabControl_CommandPreset.SelectedIndex + 1;
        }
        private void TB_CommandPreset1_TextChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.CommandPreset1 = TB_CommandPreset1.Text.Replace("\n\n", "\n").Replace("\r\n", "\n");
        }
        private void TB_CommandPreset2_TextChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.CommandPreset2 = TB_CommandPreset2.Text.Replace("\n\n", "\n").Replace("\r\n", "\n");
        }
        private void TB_CommandPreset3_TextChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.CommandPreset3 = TB_CommandPreset3.Text.Replace("\n\n", "\n").Replace("\r\n", "\n");
        }
        #endregion
        #endregion
        #region [    Macro Tab Setting    ]
        #region [    Smart Key Setting    ]
        private void Qbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.Q))
            {
                // 사용 안함
                Qbutton.BackgroundImage = Properties.Resources.Qbutton;
                SetSmartKey(Keys.Q, false);
            }
            else
            {
                if (hotkeyCheck(Keys.Q)) return;
                // 사용함
                Qbutton.BackgroundImage = Properties.Resources.Q1button;
                SetSmartKey(Keys.Q, true);
            }
        }
        private void Wbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.W))
            {
                // 사용 안함
                Wbutton.BackgroundImage = Properties.Resources.Wbutton;
                SetSmartKey(Keys.W, false);
            }
            else
            {
                if (hotkeyCheck(Keys.W)) return;
                // 사용함
                Wbutton.BackgroundImage = Properties.Resources.W1button;
                SetSmartKey(Keys.W, true);
            }
        }
        private void Ebutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.E))
            {
                // 사용 안함
                Ebutton.BackgroundImage = Properties.Resources.Ebutton;
                SetSmartKey(Keys.E, false);
            }
            else
            {
                if (hotkeyCheck(Keys.E)) return;
                // 사용함
                Ebutton.BackgroundImage = Properties.Resources.E1button;
                SetSmartKey(Keys.E, true);
            }
        }
        private void Rbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.R))
            {
                // 사용 안함
                Rbutton.BackgroundImage = Properties.Resources.Rbutton;
                SetSmartKey(Keys.R, false);
            }
            else
            {
                if (hotkeyCheck(Keys.R)) return;
                // 사용함
                Rbutton.BackgroundImage = Properties.Resources.R1button;
                SetSmartKey(Keys.R, true);
            }
        }
        private void Tbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.T))
            {
                // 사용 안함
                Tbutton.BackgroundImage = Properties.Resources.Tbutton;
                SetSmartKey(Keys.T, false);
            }
            else
            {
                if (hotkeyCheck(Keys.T)) return;
                // 사용함
                Tbutton.BackgroundImage = Properties.Resources.T1button;
                SetSmartKey(Keys.T, true);
            }
        }
        private void Abutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.A))
            {
                // 사용 안함
                Abutton.BackgroundImage = Properties.Resources.Abutton;
                SetSmartKey(Keys.A, false);
            }
            else
            {
                if (hotkeyCheck(Keys.A)) return;
                // 사용함
                Abutton.BackgroundImage = Properties.Resources.A1button;
                SetSmartKey(Keys.A, true);
            }
        }
        private void Dbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.D))
            {
                // 사용 안함
                Dbutton.BackgroundImage = Properties.Resources.Dbutton;
                SetSmartKey(Keys.D, false);
            }
            else
            {
                if (hotkeyCheck(Keys.D)) return;
                // 사용함
                Dbutton.BackgroundImage = Properties.Resources.D1button;
                SetSmartKey(Keys.D, true);
            }
        }
        private void Fbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.F))
            {
                // 사용 안함
                Fbutton.BackgroundImage = Properties.Resources.Fbutton;
                SetSmartKey(Keys.F, false);
            }
            else
            {
                if (hotkeyCheck(Keys.F)) return;
                // 사용함
                Fbutton.BackgroundImage = Properties.Resources.F1button;
                SetSmartKey(Keys.F, true);
            }
        }
        private void Gbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.G))
            {
                // 사용 안함
                Gbutton.BackgroundImage = Properties.Resources.Gbutton;
                SetSmartKey(Keys.G, false);
            }
            else
            {
                if (hotkeyCheck(Keys.G)) return;
                // 사용함
                Gbutton.BackgroundImage = Properties.Resources.G1button;
                SetSmartKey(Keys.G, true);
            }
        }
        private void Zbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.Z))
            {
                // 사용 안함
                Zbutton.BackgroundImage = Properties.Resources.Zbutton;
                SetSmartKey(Keys.Z, false);
            }
            else
            {
                if (hotkeyCheck(Keys.Z)) return;
                // 사용함
                Zbutton.BackgroundImage = Properties.Resources.Z1button;
                SetSmartKey(Keys.Z, true);
            }
        }
        private void Xbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.X))
            {
                // 사용 안함
                Xbutton.BackgroundImage = Properties.Resources.Xbutton;
                SetSmartKey(Keys.X, false);
            }
            else
            {
                if (hotkeyCheck(Keys.X)) return;
                // 사용함
                Xbutton.BackgroundImage = Properties.Resources.X1button;
                SetSmartKey(Keys.X, true);
            }
        }
        private void Cbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.C))
            {
                // 사용 안함
                Cbutton.BackgroundImage = Properties.Resources.Cbutton;
                SetSmartKey(Keys.C, false);
            }
            else
            {
                if (hotkeyCheck(Keys.C)) return;
                // 사용함
                Cbutton.BackgroundImage = Properties.Resources.C1button;
                SetSmartKey(Keys.C, true);
            }
        }
        private void Vbutton_Click(object sender, EventArgs e)
        {
            if (isSmartKey(Keys.V))
            {
                // 사용 안함
                Vbutton.BackgroundImage = Properties.Resources.Vbutton;
                SetSmartKey(Keys.V, false);
            }
            else
            {
                if (hotkeyCheck(Keys.V)) return;
                // 사용함
                Vbutton.BackgroundImage = Properties.Resources.V1button;
                SetSmartKey(Keys.V, true);
            }
        }
        private void RB_Prev_CheckedChanged(object sender, EventArgs e)
        {
            Settings.SmartKeyPreventionType = SmartKeyPreventionType;
        }
        private int SmartKeyPreventionType {
            get {
                if (RB_Prev2.Checked) return 1;
                if (RB_Prev3.Checked) return 2;
                if (RB_Prev4.Checked) return 3;
                return 0;
            }
            set {
                switch (value)
                {
                    case 0:
                        RB_Prev1.Checked = true;
                        RB_Prev2.Checked =
                        RB_Prev3.Checked =
                        RB_Prev4.Checked = false;
                        break;
                    case 1:
                        RB_Prev2.Checked = true;
                        RB_Prev1.Checked =
                        RB_Prev3.Checked =
                        RB_Prev4.Checked = false;
                        break;
                    case 2:
                        RB_Prev3.Checked = true;
                        RB_Prev1.Checked =
                        RB_Prev2.Checked =
                        RB_Prev4.Checked = false;
                        break;
                    case 3:
                        RB_Prev4.Checked = true;
                        RB_Prev1.Checked =
                        RB_Prev2.Checked =
                        RB_Prev3.Checked = false;
                        break;
                }
            }
        }
        private bool hotkeyCheck(Keys key)
        {
            if (hotkeyList.IsRegistered(key) && !isKeyReMapped(key))
            {
                MetroDialog.OK("이미 등록된 단축키", "해당 키가 이미 단축키로 등록되어 있습니다.\n채팅 단축키에서 먼저 해제해주시기 바랍니다.");
                return true;
            }
            return false;
        }
        #endregion
        #region [    Key Remapping Setting    ]
        private void Key7_Click(object sender, EventArgs e)
        {
            KeyReMapDefault();
            switch (Key7.Text)
            {
                case "7":
                    Key7.Text = "...";
                    IsRemapKeyInput = true;
                    TargetKey = SelectedKeypadType.Key7;
                    break;
                case "...":
                    Key7.Text = "7";
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
                case "X":
                    if (isRemappedSmartkey((Keys)Settings.KeyMap7)) break;
                    Key7.Text = "7";
                    Key7Text.Text = "小鍵盤7";
                    UnRegisterReMappedKey(Keys.NumPad7);
                    Settings.KeyMap7 = 0;
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
            }
        }
        private void Key8_Click(object sender, EventArgs e)
        {
            KeyReMapDefault();
            switch (Key8.Text)
            {
                case "8":
                    Key8.Text = "...";
                    IsRemapKeyInput = true;
                    TargetKey = SelectedKeypadType.Key8;
                    break;
                case "...":
                    Key8.Text = "8";
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
                case "X":
                    if (isRemappedSmartkey((Keys)Settings.KeyMap8)) break;
                    Key8.Text = "8";
                    Key8Text.Text = "小鍵盤8";
                    UnRegisterReMappedKey(Keys.NumPad8);
                    Settings.KeyMap8 = 0;
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
            }
        }
        private void Key4_Click(object sender, EventArgs e)
        {
            KeyReMapDefault();
            switch (Key4.Text)
            {
                case "4":
                    Key4.Text = "...";
                    IsRemapKeyInput = true;
                    TargetKey = SelectedKeypadType.Key4;
                    break;
                case "...":
                    Key4.Text = "4";
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
                case "X":
                    if (isRemappedSmartkey((Keys)Settings.KeyMap4)) break;
                    Key4.Text = "4";
                    Key4Text.Text = "小鍵盤4";
                    UnRegisterReMappedKey(Keys.NumPad4);
                    Settings.KeyMap4 = 0;
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
            }
        }
        private void Key5_Click(object sender, EventArgs e)
        {
            KeyReMapDefault();
            switch (Key5.Text)
            {
                case "5":
                    Key5.Text = "...";
                    IsRemapKeyInput = true;
                    TargetKey = SelectedKeypadType.Key5;
                    break;
                case "...":
                    Key5.Text = "5";
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
                case "X":
                    if (isRemappedSmartkey((Keys)Settings.KeyMap5)) break;
                    Key5.Text = "5";
                    Key5Text.Text = "小鍵盤5";
                    UnRegisterReMappedKey(Keys.NumPad5);
                    Settings.KeyMap5 = 0;
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
            }
        }
        private void Key1_Click(object sender, EventArgs e)
        {
            KeyReMapDefault();
            switch (Key1.Text)
            {
                case "1":
                    Key1.Text = "...";
                    IsRemapKeyInput = true;
                    TargetKey = SelectedKeypadType.Key1;
                    break;
                case "...":
                    Key1.Text = "1";
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
                case "X":
                    if (isRemappedSmartkey((Keys)Settings.KeyMap1)) break;
                    Key1.Text = "1";
                    Key1Text.Text = "小鍵盤1";
                    UnRegisterReMappedKey(Keys.NumPad1);
                    Settings.KeyMap1 = 0;
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
            }
        }
        private void Key2_Click(object sender, EventArgs e)
        {
            KeyReMapDefault();
            switch (Key2.Text)
            {
                case "2":
                    Key2.Text = "...";
                    IsRemapKeyInput = true;
                    TargetKey = SelectedKeypadType.Key2;
                    break;
                case "...":
                    Key2.Text = "2";
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
                case "X":
                    if (isRemappedSmartkey((Keys)Settings.KeyMap2)) break;
                    Key2.Text = "2";
                    Key2Text.Text = "小鍵盤2";
                    UnRegisterReMappedKey(Keys.NumPad2);
                    Settings.KeyMap2 = 0;
                    IsRemapKeyInput = false;
                    TargetKey = SelectedKeypadType.None;
                    break;
            }
        }
        private void Toggle_KeyRemapping_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            KeyReMapDefault();
            if (Toggle_KeyRemapping.Checked
             &&(hotkeyList.IsRegistered((Keys)Settings.KeyMap7)
             || hotkeyList.IsRegistered((Keys)Settings.KeyMap8)
             || hotkeyList.IsRegistered((Keys)Settings.KeyMap4)
             || hotkeyList.IsRegistered((Keys)Settings.KeyMap5)
             || hotkeyList.IsRegistered((Keys)Settings.KeyMap1)
             || hotkeyList.IsRegistered((Keys)Settings.KeyMap2)))
            {
                MetroDialog.OK("이미 등록된 단축키", "해당 키가 이미 단축키로 등록되어 있습니다.\n스마트키나 키리맵핑, 채팅 단축키에서 먼저 해제해주시기 바랍니다.");
                IsUpdating = true;
                Toggle_KeyRemapping.Checked = false;
                IsUpdating = false;
                return;
            }
            Settings.IsKeyRemapped = Toggle_KeyRemapping.Checked;
            if (Toggle_KeyRemapping.Checked)
            {
                if (Settings.KeyMap7 != 0)
                    hotkeyList.Register((Keys)Settings.KeyMap7, KeyRemapping, Keys.NumPad7);
                if (Settings.KeyMap8 != 0)
                    hotkeyList.Register((Keys)Settings.KeyMap8, KeyRemapping, Keys.NumPad8);
                if (Settings.KeyMap4 != 0)
                    hotkeyList.Register((Keys)Settings.KeyMap4, KeyRemapping, Keys.NumPad4);
                if (Settings.KeyMap5 != 0)
                    hotkeyList.Register((Keys)Settings.KeyMap5, KeyRemapping, Keys.NumPad5);
                if (Settings.KeyMap1 != 0)
                    hotkeyList.Register((Keys)Settings.KeyMap1, KeyRemapping, Keys.NumPad1);
                if (Settings.KeyMap2 != 0)
                    hotkeyList.Register((Keys)Settings.KeyMap2, KeyRemapping, Keys.NumPad2);
            }
            else
            {
                if (Settings.KeyMap7 != 0)
                    hotkeyList.UnRegister((Keys)Settings.KeyMap7);
                if (Settings.KeyMap8 != 0)
                    hotkeyList.UnRegister((Keys)Settings.KeyMap8);
                if (Settings.KeyMap4 != 0)
                    hotkeyList.UnRegister((Keys)Settings.KeyMap4);
                if (Settings.KeyMap5 != 0)
                    hotkeyList.UnRegister((Keys)Settings.KeyMap5);
                if (Settings.KeyMap1 != 0)
                    hotkeyList.UnRegister((Keys)Settings.KeyMap1);
                if (Settings.KeyMap2 != 0)
                    hotkeyList.UnRegister((Keys)Settings.KeyMap2);
            }
        }
        private void KeyReMapDefault()
        {
            if (!IsRemapKeyInput) return;
            switch (TargetKey)
            {
                case SelectedKeypadType.Key7:
                    Key7.Text = "7";
                    Settings.KeyMap7 = 0;
                    break;
                case SelectedKeypadType.Key8:
                    Key8.Text = "8";
                    Settings.KeyMap8 = 0;
                    break;
                case SelectedKeypadType.Key4:
                    Key4.Text = "4";
                    Settings.KeyMap4 = 0;
                    break;
                case SelectedKeypadType.Key5:
                    Key5.Text = "5";
                    Settings.KeyMap5 = 0;
                    break;
                case SelectedKeypadType.Key1:
                    Key1.Text = "1";
                    Settings.KeyMap1 = 0;
                    break;
                case SelectedKeypadType.Key2:
                    Key2.Text = "2";
                    Settings.KeyMap2 = 0;
                    break;
            }
            TargetKey = SelectedKeypadType.None;
            IsRemapKeyInput = false;
        }
        private bool isRemappedSmartkey(Keys key)
        {
            if (isSmartKey(key))
            {
                MetroDialog.OK("이미 교차 활성화된 단축키", "해당 키는 스마트키가 활성화되어 있습니다.\n스마트키를 먼저 해제해주시기 바랍니다.");
                return true;
            }
            return false;
        }
        private void KeyReMapping_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (!IsRemapKeyInput) return;
            Keys key, hotkey = e.KeyCode;
            string KeyText = GetHotkeyString(hotkey);
            foreach (string item in new string[] { "Control", "Alt", "Shift", "Menu" })
                if (KeyText.IndexOf(item) != -1)
                    return;

            if (hotkeyList.IsRegistered(hotkey)
             || isKeyReMapped(hotkey))
            {
                MetroDialog.OK("이미 등록된 단축키", "해당 키가 이미 단축키로 등록되어 있습니다.\n스마트키나 키리맵핑, 채팅 단축키에서 먼저 해제해주시기 바랍니다.");
                KeyReMapDefault();
                return;
            }
            GB_KeyReMap.Focus();
            switch (TargetKey)
            {
                case SelectedKeypadType.Key7:
                    key = Keys.NumPad7;
                    Settings.KeyMap7 = (int)hotkey;
                    Key7Text.Text = KeyText;
                    Key7.Text = "X";
                    break;
                case SelectedKeypadType.Key8:
                    key = Keys.NumPad8;
                    Settings.KeyMap8 = (int)hotkey;
                    Key8Text.Text = KeyText;
                    Key8.Text = "X";
                    break;
                case SelectedKeypadType.Key4:
                    key = Keys.NumPad4;
                    Settings.KeyMap4 = (int)hotkey;
                    Key4Text.Text = KeyText;
                    Key4.Text = "X";
                    break;
                case SelectedKeypadType.Key5:
                    key = Keys.NumPad5;
                    Settings.KeyMap5 = (int)hotkey;
                    Key5Text.Text = KeyText;
                    Key5.Text = "X";
                    break;
                case SelectedKeypadType.Key1:
                    key = Keys.NumPad1;
                    Settings.KeyMap1 = (int)hotkey;
                    Key1Text.Text = KeyText;
                    Key1.Text = "X";
                    break;
                case SelectedKeypadType.Key2:
                    key = Keys.NumPad2;
                    Settings.KeyMap2 = (int)hotkey;
                    Key2Text.Text = KeyText;
                    Key2.Text = "X";
                    break;
                default:
                    TargetKey = SelectedKeypadType.None;
                    IsRemapKeyInput = false;
                    return;
            }
            if (Toggle_KeyRemapping.Checked) RegisterReMappedKey(hotkey, key);
            TargetKey = SelectedKeypadType.None;
            IsRemapKeyInput = false;
        }
        private void KeyReMap_Leave(object sender, EventArgs e)
        {
            KeyReMapDefault();
        }
        #endregion
        #region [    Text Macro Setting    ]
        private void RB_Chat_SetCurrentFont(int type, bool state)
        {
            switch (CurrentChatIndex)
            {
                case 0:
                    switch (type)
                    {
                        case 0:
                            RB_Chat1.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat1.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 1:
                    switch (type)
                    {
                        case 0:
                            RB_Chat2.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat2.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 2:
                    switch (type)
                    {
                        case 0:
                            RB_Chat3.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat3.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 3:
                    switch (type)
                    {
                        case 0:
                            RB_Chat4.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat4.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 4:
                    switch (type)
                    {
                        case 0:
                            RB_Chat5.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat5.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 5:
                    switch (type)
                    {
                        case 0:
                            RB_Chat6.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat6.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 6:
                    switch (type)
                    {
                        case 0:
                            RB_Chat7.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat7.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 7:
                    switch (type)
                    {
                        case 0:
                            RB_Chat8.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat8.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 8:
                    switch (type)
                    {
                        case 0:
                            RB_Chat9.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat9.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
                case 9:
                    switch (type)
                    {
                        case 0:
                            RB_Chat0.Font = state ? new Font("新細明體", 9F, FontStyle.Bold) : new Font("新細明體", 9F);
                            return;
                        case 1:
                            RB_Chat0.ForeColor = state ? Color.Red : SystemColors.ControlText;
                            return;
                    }
                    return;
            }
        }
        private void RB_Chat_CheckedChanged(object sender, EventArgs e)
        {
            if (RB_Chat1.Checked) CurrentChatIndex = 0;
            else if (RB_Chat2.Checked) CurrentChatIndex = 1;
            else if (RB_Chat3.Checked) CurrentChatIndex = 2;
            else if (RB_Chat4.Checked) CurrentChatIndex = 3;
            else if (RB_Chat5.Checked) CurrentChatIndex = 4;
            else if (RB_Chat6.Checked) CurrentChatIndex = 5;
            else if (RB_Chat7.Checked) CurrentChatIndex = 6;
            else if (RB_Chat8.Checked) CurrentChatIndex = 7;
            else if (RB_Chat9.Checked) CurrentChatIndex = 8;
            else if (RB_Chat0.Checked) CurrentChatIndex = 9;
            IsUpdating = true;
            TB_ChatMacro.Text = chatHotkeyList[CurrentChatIndex].ChatMessage;
            Label_ChatHotkey.Text = GetHotkeyString(chatHotkeyList[CurrentChatIndex].Hotkey);
            BTN_SetChatHotkey.Text = chatHotkeyList.IsKeyRegisted(CurrentChatIndex) ? "단축키 해제" : "단축키 지정";
            Toggle_ChatMacro.Checked = chatHotkeyList[CurrentChatIndex].IsRegisted;
            IsUpdating = false;
        }
        private void BTN_SetChatHotkey_Click(object sender, EventArgs e)
        {
            switch (BTN_SetChatHotkey.Text)
            {
                case "단축키 지정":
                    BTN_SetChatHotkey.Text = "...";
                    IsChatHotkeyInput = true;
                    break;
                case "...":
                    BTN_SetChatHotkey.Text = "단축키 지정";
                    IsChatHotkeyInput = false;
                    break;
                case "단축키 해제":
                    if (chatHotkeyList[CurrentChatIndex].IsRegisted)
                    {
                        IsUpdating = true;
                        Toggle_ChatMacro.Checked = false;
                        chatHotkeyList.UnRegister(CurrentChatIndex);
                        RB_Chat_SetCurrentFont(1, false);
                        IsUpdating = false;
                    }
                    BTN_SetChatHotkey.Text = "단축키 지정";
                    Label_ChatHotkey.Text = "없음";
                    chatHotkeyList[CurrentChatIndex].Hotkey = 0;
                    IsChatHotkeyInput = false;
                    RB_Chat_SetCurrentFont(0, false);
                    chatHotkeyList.Save();
                    break;
            }
        }
        private void TB_ChatMacro_TextChanged(object sender, EventArgs e)
        {
            chatHotkeyList[CurrentChatIndex].ChatMessage = TB_ChatMacro.Text;
            chatHotkeyList.Save();
        }
        private void BTN_SetChatHotkey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (!IsChatHotkeyInput) return;
            Keys hotkey = e.KeyCode;
            string KeyText = GetHotkeyString(hotkey);
            foreach (string item in new string[] { "Control", "Alt", "Shift", "Menu" })
                if (KeyText.IndexOf(item) != -1)
                    return;
            if (hotkeyList.IsRegistered(hotkey)
             || isKeyReMapped(hotkey))
            {
                MetroDialog.OK("已註冊熱鍵", "該鍵已註冊為熱鍵。\n請先從智能鍵、鍵重映射或聊天快捷方式中將其禁用。");
                BTN_SetChatHotkey.Text = "分配熱鍵";
                Label_ChatHotkey.Text = "不存在";
                chatHotkeyList[CurrentChatIndex].Hotkey = 0;
                IsChatHotkeyInput = false;
                return;
            }
            GB_ChatMacro.Focus();
            chatHotkeyList[CurrentChatIndex].Hotkey = hotkey;
            Label_ChatHotkey.Text = KeyText;
            BTN_SetChatHotkey.Text = "禁用熱鍵";
            RB_Chat_SetCurrentFont(0, true);
        }
        private void BTN_SetChatHotkey_Leave(object sender, EventArgs e)
        {
            if (!IsChatHotkeyInput) return;
            BTN_SetChatHotkey.Text = "分配熱鍵";
            Label_ChatHotkey.Text = "不存在";
            IsChatHotkeyInput = false;
        }
        private void Toggle_ChatMacro_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            if (!chatHotkeyList.IsKeyRegisted(CurrentChatIndex))
            {
                MetroDialog.OK("未指定快捷方式", "沒有設置熱鍵。\n首先，請指定一個熱鍵。");
                goto UnCheck;
            }
            if (!Toggle_ChatMacro.Checked)
            {
                chatHotkeyList.UnRegister(CurrentChatIndex);
                RB_Chat_SetCurrentFont(1, false);
                chatHotkeyList.Save();
                return;
            }
            if (hotkeyList.IsRegistered(chatHotkeyList[CurrentChatIndex].Hotkey))
            {
                MetroDialog.OK("已註冊快捷方式", "該鍵已註冊為快捷方式。\n請先禁用智能鍵、鍵重映射或聊天快捷方式。");
                goto UnCheck;
            }
            chatHotkeyList.Register(CurrentChatIndex);
            RB_Chat_SetCurrentFont(1, true);
            chatHotkeyList.Save();
            return;
        UnCheck:
            IsUpdating = true;
            Toggle_ChatMacro.Checked = false;
            IsUpdating = false;
        }
        #endregion
        #region [    Auto Mouse Setting    ]
        private void Toggle_AutoMouse_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            if ((AutoMouse.LeftStartKey != 0
             || AutoMouse.RightStartKey != 0)
             && AutoMouse.EndKey != 0)
            {
                AutoMouse.Enabled = Toggle_AutoMouse.Checked;
                return;
            }
            MetroDialog.OK("미지정 단축키 발견", "지정되지 않은 단축키가 있습니다.\n최소한 한쪽 시작 단축키는 지정해주세요.");
            IsUpdating = true;
            Toggle_AutoMouse.Checked = false;
            IsUpdating = false;
        }
        private void BTN_AutoLeftMouseOn_Click(object sender, EventArgs e)
        {
            if (AutoMouse.Enabled && AutoMouse.RightStartKey == 0)
                AutoMouse.Enabled = Toggle_AutoMouse.Checked = false;
            if (IsAutoMouseInput || AutoMouse.LeftStartKey != 0)
            {
                Label_AutoLeftMouseOn.Text = "없음";
                BTN_AutoLeftMouseOn.Text = "좌클";
                AutoMouse.LeftStartKey = 0;
                return;
            }
            TargetMouse = SelectedAutoMouseType.Left;
            IsAutoMouseInput = true;
            BTN_AutoLeftMouseOn.Text = "...";
        }
        private void BTN_AutoRightMouseOn_Click(object sender, EventArgs e)
        {
            if (AutoMouse.Enabled && AutoMouse.LeftStartKey == 0)
                AutoMouse.Enabled = Toggle_AutoMouse.Checked = false;
            if (IsAutoMouseInput || AutoMouse.RightStartKey != 0)
            {
                Label_AutoRightMouseOn.Text = "없음";
                BTN_AutoRightMouseOn.Text = "우클";
                AutoMouse.RightStartKey = 0;
                return;
            }
            TargetMouse = SelectedAutoMouseType.Right;
            IsAutoMouseInput = true;
            BTN_AutoRightMouseOn.Text = "...";
        }
        private void BTN_AutoMouseOff_Click(object sender, EventArgs e)
        {
            if (AutoMouse.Enabled)
                AutoMouse.Enabled = Toggle_AutoMouse.Checked = false;
            if (IsAutoMouseInput || AutoMouse.EndKey != 0)
            {
                Label_AutoMouseOff.Text = "없음";
                BTN_AutoMouseOff.Text = "종료";
                AutoMouse.EndKey = 0;
                return;
            }
            TargetMouse = SelectedAutoMouseType.Off;
            IsAutoMouseInput = true;
            BTN_AutoMouseOff.Text = "...";
        }
        private void TrB_AutoMouseDelay_ValueChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Label_AutoMouseDelay.Text = $"{TrB_AutoMouseDelay.Value} ms";
            AutoMouse.Interval = TrB_AutoMouseDelay.Value;
        }
        private void AutoMouse_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (!IsAutoMouseInput) return;
            Keys hotkey = e.KeyCode;
            string KeyText = GetHotkeyString(hotkey);
            foreach (string item in new string[] { "Control", "Alt", "Shift", "Menu" })
                if (KeyText.IndexOf(item) != -1)
                    return;
            if (hotkeyList.IsRegistered(hotkey) || isKeyReMapped(hotkey) || AutoMouse.IsRegistered(hotkey))
            {
                MetroDialog.OK("이미 등록된 단축키", "해당 키가 이미 단축키로 등록되어 있습니다.\n스마트키나 키리맵핑, 채팅 단축키에서 먼저 해제해주시기 바랍니다.");
                switch (TargetMouse)
                {
                    case SelectedAutoMouseType.Off:
                        Label_AutoMouseOff.Text = "없음";
                        BTN_AutoMouseOff.Text = "종료";
                        AutoMouse.EndKey = 0;
                        break;
                    case SelectedAutoMouseType.Left:
                        Label_AutoLeftMouseOn.Text = "없음";
                        BTN_AutoLeftMouseOn.Text = "좌클";
                        AutoMouse.LeftStartKey = 0;
                        break;
                    case SelectedAutoMouseType.Right:
                        Label_AutoRightMouseOn.Text = "없음";
                        BTN_AutoRightMouseOn.Text = "우클";
                        AutoMouse.RightStartKey = 0;
                        break;
                }
            }
            else
            {
                GB_AutoMouse.Focus();
                switch (TargetMouse)
                {
                    case SelectedAutoMouseType.Off:
                        Label_AutoMouseOff.Text = KeyText;
                        BTN_AutoMouseOff.Text = "해제";
                        AutoMouse.EndKey = hotkey;
                        break;
                    case SelectedAutoMouseType.Left:
                        Label_AutoLeftMouseOn.Text = KeyText;
                        BTN_AutoLeftMouseOn.Text = "해제";
                        AutoMouse.LeftStartKey = hotkey;
                        break;
                    case SelectedAutoMouseType.Right:
                        Label_AutoRightMouseOn.Text = KeyText;
                        BTN_AutoRightMouseOn.Text = "해제";
                        AutoMouse.RightStartKey = hotkey;
                        break;
                }
            }
            IsAutoMouseInput = false;
        }
        private void BTN_AutoMouse_Leave(object sender, EventArgs e)
        {
            if (!IsAutoMouseInput) return;
            switch (TargetMouse)
            {
                case SelectedAutoMouseType.Off:
                    Label_AutoMouseOff.Text = "없음";
                    BTN_AutoMouseOff.Text = "종료";
                    AutoMouse.EndKey = 0;
                    break;
                case SelectedAutoMouseType.Left:
                    Label_AutoLeftMouseOn.Text = "없음";
                    BTN_AutoLeftMouseOn.Text = "좌클";
                    AutoMouse.LeftStartKey = 0;
                    break;
                case SelectedAutoMouseType.Right:
                    Label_AutoRightMouseOn.Text = "없음";
                    BTN_AutoRightMouseOn.Text = "우클";
                    AutoMouse.RightStartKey = 0;
                    break;
            }
            IsAutoMouseInput = false;
        }
        #endregion
        #endregion
        private void CommandListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string value = CommandListBox.SelectedItem.ToString();
            switch (value)
            {
                case "dr":
                    Label_CommandTitle.Text = "設置響應延遲時間";
                    Label_CommandKR.Text ="閱讀";
                    Label_ParameterValue.Text = "0 ~ 550";
                    TB_CommandDescription.Text = "設置 Warcraft 的控制延遲時間。\r\n與其他工具不同，指定的值會在 Warcraft 和程序同時運行時自動應用。\r\n通常，最常用的值介於 15 和 45 之間。.\n一些地圖使用 550 的值作為起始限制，但在 Cirnix 的情況下，您不需要使用它，因為它會自動應用該功能。";
                    break;
                case "ss":
                    Label_CommandTitle.Text = "開始等待時間設置";
                    Label_CommandKR.Text = "ss";
                    Label_ParameterValue.Text = "0 ~ 6";
                    TB_CommandDescription.Text = "調整在等候室中單擊開始遊戲時發生的等待時間。\r\n與其他工具不同，當魔獸爭霸和程序同時運行時，會自動應用指定的值。";
                    break;
                case "rg":
                    Label_CommandTitle.Text = "等候室自動刷新";
                    Label_CommandKR.Text = "ㄱㅎ";
                    Label_ParameterValue.Text = "(선택) 1 ~ 2147483647";
                    TB_CommandDescription.Text = "服務器支持的 /rg 命令每隔 11 秒自動輸入一次。\r\n如果不輸入參數，它會無限制地工作，如果你再次輸入它，它會停止工作。\r\n與其他工具不同，只有當遊戲開始後，該功能自動停止。";
                    break;
                case "set":
                    Label_CommandTitle.Text = "選擇保存類別";
                    Label_CommandKR.Text = "ㄴㄷㅅ";
                    Label_ParameterValue.Text = "分類名稱";
                    TB_CommandDescription.Text = "指定保存類別。\r\n如果輸入的類別不存在，請立即創建並指定類別。";
                    break;
                case "lc":
                    Label_CommandTitle.Text = "加載保存代碼";
                    Label_CommandKR.Text = "ㅣㅊ";
                    Label_ParameterValue.Text = "(선택) 분류 이름";
                    TB_CommandDescription.Text = "立即加載當前指定的存檔。\r\n僅支持使用 Grabiti 的 RPG Creator 創建的地圖，並在加載後立即輸入設置命令預設。\r\n如果您輸入類別名稱，將加載該類別中的最新保存，還立即更改當前負載分配。";
                    break;
                case "tlc":
                    Label_CommandTitle.Text = "加載 TWR 保存代碼";
                    Label_CommandKR.Text = "싳";
                    Label_ParameterValue.Text = "(선택) 분류 이름";
                    TB_CommandDescription.Text = "立即加載當前指定的保存。\r\n僅支持 TWRPG 地圖，並在加載後立即輸入設置命令預設。\r\n如果您輸入類別名稱，將加載該類別中的最新保存，並且當前加載指定目標也將被加載。立即更改。";
                    break;
                case "olc":
                    Label_CommandTitle.Text = "一件隨機防禦保存代碼負載";
                    Label_CommandKR.Text = "olc";
                    Label_ParameterValue.Text = "(선택) 분류 이름";
                    TB_CommandDescription.Text = "立即加載當前指定的存檔。\r\n只支持單張隨機防禦地圖，加載後立即輸入設置命令預設。\r\n如果輸入類別名稱，則加載該類別的最新保存並加載當前保存。它還會立即更改加載分配。";
                    break;
                case "cmd":
                    Label_CommandTitle.Text = "加載指令預置";
                    Label_CommandKR.Text = "층";
                    Label_ParameterValue.Text = "1 ~ 3";
                    TB_CommandDescription.Text = "輸入指定的命令預置。";
                    break;
                case "cam":
                    Label_CommandTitle.Text = "設置攝像頭觀看距離";
                    Label_CommandKR.Text = "시야";
                    Label_ParameterValue.Text = "0 ~ 6000";
                    TB_CommandDescription.Text = "調整相機的視野。\r\n魔獸的默認視野為 1650，RPG 地圖的“-Field 100”為 1700，而“-Field 150”的視野為 2550。\r\n推薦的視野視野 距離在3550左右，推薦最大視野為4050.";
                    break;
                case "camy":
                    Label_CommandTitle.Text = "相機Y軸角度設置";
                    Label_CommandKR.Text = "ㅊ므ㅛ";
                    Label_ParameterValue.Text = "0 ~ 360";
                    TB_CommandDescription.Text = "調整相機垂直角度。\r\n魔獸默認垂直角度為304，垂直向下看屏幕的角度為270。\r\n數字越大越低，越低越高上去。";
                    break;
                case "camx":
                    Label_CommandTitle.Text = "相機X軸角度設置";
                    Label_CommandKR.Text = "ㅊ믙";
                    Label_ParameterValue.Text = "0 ~ 360";
                    TB_CommandDescription.Text = "調整攝像機的水平角度。\r\n魔獸默認水平角度為 90。值越高，攝像機越向右旋轉，值越低，攝像機越向左旋轉。";
                    break;
                case "hp":
                    Label_CommandTitle.Text = "移除最大耐力";
                    Label_CommandKR.Text = "ㅗㅔ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "移除狀態窗口顯示的最大體力值，以緩解體力值消失現象。\r\n使用時只能查看當前體力值，重新進入時不使用該功能。";
                    break;
                case "dice":
                    Label_CommandTitle.Text = "擲骰子";
                    Label_CommandKR.Text = "주사위";
                    Label_ParameterValue.Text = "(선택) 0 ~ 2147483646";
                    TB_CommandDescription.Text = "輸出具有參數最大值的隨機整數。\r\n如果未輸入參數，則最大值設置為 100。";
                    break;
                case "mo":
                    Label_CommandTitle.Text = "內存優化";
                    Label_CommandKR.Text = "ㅡㅐ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "優化了魔獸正在使用的內存。\r\n通過優化內存，可以最大程度地減少魔獸因死亡而強制終止的現象，但對於計算機配置較低的用戶，在使用遊戲內優化時可能會出現殘留延遲。.\r\n 使用該命令後，它會立即檢查內存變化量 5 秒並通知您。";
                    break;
                case "chk":
                    Label_CommandTitle.Text = "作弊地圖閱讀";
                    Label_CommandKR.Text = "체크";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "使用程序中預先註冊的語法，檢查是否在地圖中插入了金手指。\r\n目前支持的金手指是'Gaghin'和'Soul Sangdi'金手指，並根據報告提供了額外的支持。也正在考慮。";
                    break;
                case "rework":
                    Label_CommandTitle.Text = "重返";
                    Label_CommandKR.Text = "ㄱㄷ재가";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "魔獸返返功能。";
                    break;
                case "j":
                    Label_CommandTitle.Text = "房間入口";
                    Label_CommandKR.Text = "ㅓ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "進入魔獸指定房間。";
                    break;
                case "c":
                    Label_CommandTitle.Text = "創建房間";
                    Label_CommandKR.Text = "ㅊ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "創建一個 Warcraft 房間。\r\n 創建時，它是使用先前創建的房間的地圖創建的。";
                    break;
                case "wa":
                    Label_CommandTitle.Text = "房間清單";
                    Label_CommandKR.Text = "ㅈㅁ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "黑名單中存儲的IP和ID與\r\n房間連接人的IP和ID匹配，滿足則輸出。\r\n\r\n請輸入部分ID和IP . 如果輸入\r\n\r\n1.2，則搜索1.2.3.4 IP。\r\n如果輸入1.2.3.4，則無條件搜索相同的IP。";
                    break;
                case "va":
                    Label_CommandTitle.Text = "IP匹配";
                    Label_CommandKR.Text = "ㅍㅁ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "輸出連接到房間的人的 IP 和 ID。";
                    break;
                case "max":
                    Label_CommandTitle.Text = "房間人數報警(MAX)";
                    Label_CommandKR.Text = "ㅡㅁㅌ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "當房間數量超過用戶定義的數量時通知。\r\n\r\n 再次輸入 !max 命令停止。";
                    break;
                case "min":
                    Label_CommandTitle.Text = "房間人數報警(MIN)";
                    Label_CommandKR.Text = "ㅡㅑㅜ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "當房間數量小於用戶設置的數量時通知。\r\n\r\n 再次輸入 !min 命令停止。.";
                    break;
                case "as":
                    Label_CommandTitle.Text = "自動開啟";
                    Label_CommandKR.Text = "ㅁㄴ";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "如果人數超過指定人數，10 秒後自動開始。\r\n\r\n 如果再次輸入 !as 命令，它將停止。\r\n如果已經倒計時10秒，就停不下來了。";
                    break;
                case "exit":
                    Label_CommandTitle.Text = "結束魔獸爭霸";
                    Label_CommandKR.Text = "종료";
                    Label_ParameterValue.Text = "없음";
                    TB_CommandDescription.Text = "退出魔獸爭霸.";
                    break;
                default:
                    return;
            }
            Label_CommandEN.Text = value;
        }


        private void Banlistload()
        {
            List<BanlistModel> list = SaveBanlistUsers.Load();
            if (list == null)
            {
                return;
            }
            banlistview.Items.Clear();
            Memory.BanList.Clear();
            try
            {
                banlistview.BeginUpdate();
                foreach (BanlistModel banlistModel in list)
                {
                    string[] row = { banlistModel.ID, banlistModel.IP, banlistModel.Reason };
                    ListViewItem newitem = new ListViewItem(row) { Tag = banlistModel };
                    banlistview.Items.Add(newitem);
                    Memory.BanList.Add(banlistModel);
                }
            }
            finally
            {
                banlistview.EndUpdate();
            }
        }

        private void AddSave(BanlistModel data)
        {
            List<BanlistModel> list = new List<BanlistModel>();
            foreach (object obj in banlistview.Items)
            {
                ListViewItem listViewItem = (ListViewItem)obj;
                list.Add((BanlistModel)listViewItem.Tag);
            }
            list.Add(data);
            SaveBanlistUsers.Save(list);
            Banlistload();
        }

        private void DelSave(BanlistModel data)
        {
            List<BanlistModel> list = new List<BanlistModel>();
            foreach (object obj in banlistview.Items)
            {
                ListViewItem listViewItem = (ListViewItem)obj;
                list.Add((BanlistModel)listViewItem.Tag);
            }
            list.Remove(data);
            SaveBanlistUsers.Save(list);
            Banlistload();
        }

        private async void BTN_HotKeyDebug_Click(object sender, EventArgs e)
        {
            KeyboardHooker.HookEnd();
            await Task.Delay(1);
            KeyboardHooker.HookStart();
            MetroDialog.OK("掛鉤重置完成", "熱鍵掛鉤狀態已重置。");
        }

        private void Toggle_AutoFrequency_CheckedChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.IsAutoFrequency = Toggle_AutoFrequency.Checked;
            Number_ChatFrequency.Enabled = BTN_DetectFrequency.Enabled = !Toggle_AutoFrequency.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BanlistModel banlistModel = new BanlistModel();
            banlistModel.ID = IdTextBox.Text;
            banlistModel.IP = IPTextBox.Text;
            banlistModel.Reason = ReasonTextBox.Text;
            IdTextBox.Text = "";
            IPTextBox.Text = "";
            ReasonTextBox.Text = "";
            this.AddSave(banlistModel);
            Banlistload();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (banlistview.SelectedItems.Count <= 0) return;
            this.DelSave((BanlistModel)this.banlistview.SelectedItems[0].Tag);
        }

        private void Number_ChatFrequency_ValueChanged(object sender, EventArgs e)
        {
            if (IsUpdating) return;
            Settings.ChatFrequency = Convert.ToInt32(Number_ChatFrequency.Value) - 1;
        }
        private async void BTN_DetectFrequency_Click(object sender, EventArgs e)
        {
            await Memory.Message.DetectChatFrequency();
            IsUpdating = true;
            Number_ChatFrequency.Value = Settings.ChatFrequency + 1;
            IsUpdating = false;
        }
        private void RB_Help_CheckedChanged(object sender, EventArgs e)
        {
            if (RB_Help1.Checked)
            {
                TB_Help.Text = "這是宿主玩家設置後生效的設置。\r\n\r\n反應延遲時間：這是指玩家執行動作時發生的延遲時間。 延遲時間越短，反應速度越快。\r\n\r\n開始等待時間：這是您按下開始遊戲時發生的等待時間。 等待時間可由用戶隨意設置，但不能設置超過 7 秒以防止拖釣。\r\n\r\n以上項目存儲在程序中，不像其他程序需要每隔一段時間檢查一次交易time ，一旦指定了一個值，之後會自動連續應用到魔獸中。";
                return;
            }
            if (RB_Help2.Checked)
            {
                TB_Help.Text = "內存優化從操作系統釋放分配給魔獸的內存。\r\n\r\n這是臨時釋放的，如果魔獸需要數據，從魔獸重新加載文件。\r\n\r \n在此時間，電腦配置低的玩家可能會出現卡頓，所以不建議在玩遊戲時使用它。\r\n（電腦配置好的玩家無所謂。）\r \n\r\n魔獸爭霸不斷由於許多未指明的原因而導致內存洩漏。 這種情況在 1.28.5 更新後更頻繁地發生，並且這種內存洩漏會導致致命錯誤。 通過使用此功能，您可以修復未關閉的洩漏，從而減輕魔獸爭霸卡頓。";
                return;
            }
            if (RB_Help3.Checked)
            {
                TB_Help.Text = "它會立即分析當前地圖並檢查它是否是作弊地圖。\r\n\r\n魔獸有一個語法為 Jass 的腳本，所有觸發器都通過該腳本發生。 金手指的話，也是用腳本寫的，這個功能搜索用腳本寫的金手指。\r\n\r\n主要是為了看通用金手指，根據報告。正在考慮";
                return;
            }
            if (RB_Help4.Checked)
            {
                TB_Help.Text = "它的工作原理是每 0.2 秒檢查 Warcraft III.exe 是否正在運行。\r\n\r\n在 1.28.5 更新後，間歇性地，關閉魔獸時進程沒有完全終止，因此通過任務管理器手動終止它。 \r\n\r\n如果 Warcraft 在至少 10 秒內沒有完全關閉，程序會顯示一條指導消息來幫助您關閉。\r\n\r\n如果出現一些消息窗口，則說明存在錯誤在某些不可能出現的情況下會出現消息窗口。\r\n\r\n如果單擊“否”一次，消息將在 30 分鐘內不會出現。";
                return;
            }
            if (RB_Help5.Checked)
            {
                TB_Help.Text = "按文件夾保存和管理存儲在個人計算機上的保存文件。\r\n\r\n由於 RPG 的性質允許您開發多個角色，因此您可以開發多個 RPG 地圖和多個角色。這是一項非常繁瑣的工作。 Cirnix可以創建文件夾對存檔文件進行分類，並提供按存檔文件分類保存的功能。\r\n\r\n-輸入保存命令時，當前指定的RPG地圖當前在分類文件夾中。以時間命名，如果在-save命令後加修飾符，則文件名改為'modifier.txt'。";
                return;
            }
            if (RB_Help6.Checked)
            {
                TB_Help.Text = "魔獸的回放文件是自動保存的。\r\n不過，此時你可以使用韓文名稱。\r\n\r\n回放對於播放和驗證RPG地圖是必不可少的。 每次都保存它，找到正確的保存文件是一件很痛苦的事。\r\n\r\nCirnix 會自動用當前時間命名回放，僅適用於您使用 -save 命令離開的遊戲，如果在之後添加修飾符-save命令，文件名更改為'_modifier.w3g'並保存。\r\n\r\n1.28.5更新後，如果你嘗試在魔獸中保存韓文名稱的回放，編碼有一個輸出被砸的問題，但是Cirnix的回放保存功能使用了外部工具，所以它的特點是可以使用韓文名字。";
                return;
            }
            if (RB_Help7.Checked)
            {
                TB_Help.Text = "僅針對啟用了 Smart Key 的按鍵，按下按鍵時立即點擊鼠標點。\r\n\r\n在 Smart Key 操作期間點擊鼠標後激活防止過度點擊。";
                return;
            }
            if (RB_Help8.Checked)
            {
                TB_Help.Text = "您可以將庫存熱鍵更改為您想要的任何鍵。\r\n\r\n채팅창이 열려 있거나, 워크래프트 창이 메인 화면이 아닐 경우에는 동작하지 않습니다.\r\n\r\n또한, 단축키들은 과도하게 빠른 속도로 연타시에 문제가 발생될 수 있는 점에 유의하시기 바랍니다.";
                return;
            }
            if (RB_Help9.Checked)
            {
                TB_Help.Text = "您可以調整聊天輸入和輸出發生的頻率。\r\n\r\n魔獸爭霸在發送之前將聊天輸入消息存儲在內存中。\r\n在正常情況下，程序會自動搜索相應的頻率但是，有非指定情況下頻率搜索失敗的情況。\r\n\r\n這種情況，您可以手動調整頻率來解決問題。\r\n\r\n我們探索了幾種自動搜索算法，但是這個由於一些用戶不斷遇到的問題而添加了該功能，因此請僅在您覺得打開聊天時沒有任何反應時才使用此功能。";
                return;
            }
            if (RB_Help10.Checked)
            {
                TB_Help.Text = " - 개발 기간: 2017-07-16 ~ ...\r\n\r\n해당 프로그램은 Cirnix의 오픈소스 프로젝트인 OpenCirnix입니다.\r\nhttps://github.com/BlacklightsC/OpenCirnix\r\n\r\n\r\n---------- 후원 안내 ----------\r\n\r\n농협중앙회 302-0627-1751-31 박성현\r\n카카오뱅크 3333-09-4274361 박성현\r\n투네이션: https://toon.at/donate/637131255322131449\r\n페이팔(해외): https://www.paypal.me/BlacklightsC\r\nPatreon(해외 정기후원): https://www.patreon.com/cirnix \r\n\r\n漢化者 Cloveri:感謝作者的程式碼開源 如果您認為我侵犯了您的權利 請聯繫我 cloveriowo@gmail.com\r\nChinese Translator made by Cloveri:Thanks to the author for the open source code If you think I have violated your rights please contact me at cloveriowo@gmail.com ";
                return;
            }
        }
    }
}