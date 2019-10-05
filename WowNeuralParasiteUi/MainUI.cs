﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;

namespace ClassicWowNeuralParasite
{
    public partial class MainUI : Form
    {
        private string WowAutomaterStatusString = string.Empty;
        private Timer m_ApiDataUpdateTimer = new Timer();
        private bool m_InfoSHown = false;
        private bool m_Recording = false;
        private string m_FilePath = string.Empty;
        private Color m_RedColor = Color.FromArgb(50, 255, 0, 0);
        private Color m_GreenColor = Color.FromArgb(50, 0, 255, 0);
        private InputSimulator m_InputSimulator = new InputSimulator();

        public MainUI()
        {
            InitializeComponent();

            if (!File.Exists("targetpath.txt"))
            {
                File.WriteAllText("targetpath.txt", ";");
            }

            if (!File.Exists("revivepath.txt"))
            {
                File.WriteAllText("revivepath.txt", ";");
            }

            if (!File.Exists("shoppath.txt"))
            {
                File.WriteAllText("shoppath.txt", ";");
            }

            if (!File.Exists("walkpath.txt"))
            {
                File.WriteAllText("walkpath.txt", ";");
            }

            ReadPaths();

            RecordWowPath.RecordPathEvent += Automater_RecordPathEvent;
            RecordWowPath.StopEvent += Automater_StopEvent;

            PathTypeDropDown.SelectedIndex = 0;

            ModeDropDown.SelectedIndex = 0;
            m_ApiDataUpdateTimer.Interval = 100;
            m_ApiDataUpdateTimer.Tick += ApiDataUpdateTimer_Tick;
            m_ApiDataUpdateTimer.Enabled = true;

            WowAutomater.AutomaterStatusEvent += AutomaterStatusEvent;

            Task.Run(() =>
            {
                try
                {
                    WowAutomater.Run();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void Automater_StopEvent(object sender, EventArgs ea)
        {
            if (InvokeRequired)
            {
                InvokeWithoutDisposedException(new MethodInvoker(() => { Automater_StopEvent(sender, ea); }));
                return;
            }

            XTextBox.Enabled = true;
            YTextBox.Enabled = true;
            LoadFileButton.Enabled = true;
            SaveFileButton.Enabled = true;
            OKButton.Enabled = true;

            RecordButton.Text = "Record Path";
            RecordButton.BackColor = System.Drawing.Color.FromArgb(255, 128, 255, 128);

            m_Recording = false;
        }

        private void Automater_RecordPathEvent(object sender, RecordPathEventArgs wea)
        {
            if (InvokeRequired)
            {
                InvokeWithoutDisposedException(new MethodInvoker(() => { Automater_RecordPathEvent(sender, wea); }));
                return;
            }

            XTextBox.Text += wea.X.ToString("N3") + ", ";
            YTextBox.Text += wea.Y.ToString("N3") + ", ";
        }

        private void ReadPaths()
        {
            string allpaths = File.ReadAllText("targetpath.txt");
            string[] paths = allpaths.Split(';');

            WowAutomater.SetPathCoordinates(Program.ExtractCommaDelimitedDoubles(paths[0]),
                                     Program.ExtractCommaDelimitedDoubles(paths[1]));


            allpaths = File.ReadAllText("revivepath.txt");
            paths = allpaths.Split(';');

            WowAutomater.SetReviveCoordinates(Program.ExtractCommaDelimitedDoubles(paths[0]),
                                     Program.ExtractCommaDelimitedDoubles(paths[1]));


            allpaths = File.ReadAllText("shoppath.txt");
            paths = allpaths.Split(';');

            WowAutomater.SetShopCoordinates(Program.ExtractCommaDelimitedDoubles(paths[0]),
                                     Program.ExtractCommaDelimitedDoubles(paths[1]));

            allpaths = File.ReadAllText("walkpath.txt");
            paths = allpaths.Split(';');

            WowAutomater.SetWalkCoordinates(Program.ExtractCommaDelimitedDoubles(paths[0]),
                                     Program.ExtractCommaDelimitedDoubles(paths[1]));

        }

        private void ApiDataUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                InvokeWithoutDisposedException(new MethodInvoker(() => { ApiDataUpdateTimer_Tick(sender, e); }));
                return;
            }

            DataTextBox.Text = WowApi.CurrentPlayerData.ToString();
            StatusLabel.Text = WowAutomaterStatusString;

            if (WowApi.CurrentPlayerData.Found)
            {
                RecordButton.Enabled = true;
                InfoTab.BackColor = m_GreenColor;
            }
            else
            {
                RecordButton.Enabled = false;
                InfoTab.BackColor = m_RedColor;
            }
        }

        private void AutomaterStatusEvent(object sender, AutomaterActionEventArgs e)
        {
            if (InvokeRequired)
            {
                InvokeWithoutDisposedException(new MethodInvoker(() => { AutomaterStatusEvent(sender, e); }));
                return;
            }

            WowAutomaterStatusString = e.CurrentAction.ToString();
        }

        private void InvokeWithoutDisposedException(Delegate method)
        {
            try
            {
                Invoke(method);
            }
            catch (ObjectDisposedException)
            {
                // Do nothing
            }
            
        }

        private void ModeDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(ModeDropDown.SelectedIndex)
            {
                case 0:
                    WowAutomater.CurrentActionMode = ActionMode.FindTarget;
                    break;
                case 1:
                    WowAutomater.CurrentActionMode = ActionMode.AutoAttack;
                    break;
                case 2:
                    WowAutomater.CurrentActionMode = ActionMode.AutoWalk;
                    break;
            }
        }

        private void ShowInfoButton_Click(object sender, EventArgs e)
        {
            if(m_InfoSHown)
            {
                Size = new System.Drawing.Size(236, 100);
                OptionTabs.Visible = false;
                ShowInfoButton.BackgroundImage = Properties.Resources.show;
            }
            else
            {
                Size = new System.Drawing.Size(236, 760);
                OptionTabs.Visible = true;
                ShowInfoButton.BackgroundImage = Properties.Resources.hide;
            }

            m_InfoSHown = !m_InfoSHown;
        }

        private void PathTypeDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetFilePathFromDropDownIndex(PathTypeDropDown.SelectedIndex);
            ReadPathFromFile();
        }

        private void SetFilePathFromDropDownIndex(int index)
        {
            switch (index)
            {
                case 0:
                    m_FilePath = "targetpath.txt";
                    break;
                case 1:
                    m_FilePath = "revivepath.txt";
                    break;
                case 2:
                    m_FilePath = "shoppath.txt";
                    break;
                case 3:
                    m_FilePath = "walkpath.txt";
                    break;
            }
        }

        private void ReadPathFromFile()
        {
            if (File.Exists(m_FilePath))
            {
                string allpaths = File.ReadAllText(m_FilePath);
                string[] paths = allpaths.Split(';');

                XTextBox.Text = paths[0];
                YTextBox.Text = paths[1];
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            File.WriteAllText(m_FilePath, XTextBox.Text + ";" + YTextBox.Text);
            ReadPaths();
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            if (m_Recording)
            {
                RecordWowPath.StopRecordingPath();
            }
            else
            {
                XTextBox.Text = "";
                YTextBox.Text = "";

                XTextBox.Enabled = false;
                YTextBox.Enabled = false;
                LoadFileButton.Enabled = false;
                SaveFileButton.Enabled = false;
                OKButton.Enabled = false;

                RecordButton.Text = "Stop Recording";
                RecordButton.BackColor = System.Drawing.Color.FromArgb(255, 255, 128, 128);

                Task.Run(() =>
                {
                    try
                    {
                        RecordWowPath.RecordPath();
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                });

                m_Recording = true;
            }
        }

        private void SaveFileButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text |*.txt";

            sfd.Title = "Save path";
            sfd.FileName = m_FilePath;
            sfd.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (sfd.FileName != "")
            {
                File.WriteAllText(sfd.FileName, XTextBox.Text + ";" + YTextBox.Text);
            }
        }

        private void LoadFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text |*.txt";
            ofd.Title = "Open path";
            ofd.FileName = m_FilePath;
            ofd.ShowDialog();

            if (ofd.FileName != "")
            {
                string allpaths = File.ReadAllText(ofd.FileName);
                string[] paths = allpaths.Split(';');

                XTextBox.Text = paths[0];
                YTextBox.Text = paths[1];
            }
        }

        private void StealthLevelNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.StealthLevel = (int)Math.Round(StealthLevelNumericInput.Value);
        }

        private void TurnToleranceNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WaypointFollower.TurnToleranceRad = (double)TurnToleranceNumericInput.Value;
        }

        private void OptionTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_Recording)
            {
                RecordWowPath.StopRecordingPath();
            }

            // OptionTabs.SelectedIndex == 3 "Classes" tab
            if (OptionTabs.SelectedIndex == 3 && 
                WowApi.CurrentPlayerData.Class > PlayerClass.None && 
                WowApi.CurrentPlayerData.Class <= PlayerClass.LastPlayerClass)
            {
                ClassTabs.SelectedIndex = (int)WowApi.CurrentPlayerData.Class - 1;
            }
        }

        private void StaleStealthNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.StaleStealthTimer.Interval = (double)(StaleStealthNumericInput.Value * 1000);
        }

        private void RegisterDelayNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.RegisterDelay = (double)RegisterDelayNumericInput.Value;
        }

        private void SplitDistanceNumericInput_ValueChanged(object sender, EventArgs e)
        {
            RecordWowPath.SplitDistance = (double)SplitDistanceNumericInput.Value;
        }

        private void SliceNDiceCPNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.SliceAndDice.ComboPointsCost = (ushort)SliceNDiceCPNumericInput.Value ;
        }

        private void RuptureCPNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.Rupture.ComboPointsCost = (ushort)RuptureCPNumericInput.Value;
        }

        private void EvisceratePercentageNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.Eviscerate.TargetHealthPercentage = (ushort)EvisceratePercentageNumericInput.Value;
        }

        private void ClosestPointDistanceNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WaypointFollower.ClosestPointDistance = (double)ClosestPointDistanceNumericInput.Value;
        }

        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            RecordWowPath.RecordPathEvent -= Automater_RecordPathEvent;
            RecordWowPath.StopEvent -= Automater_StopEvent;
            m_ApiDataUpdateTimer.Enabled = false;
            m_ApiDataUpdateTimer.Tick -= ApiDataUpdateTimer_Tick;
            WowAutomater.AutomaterStatusEvent -= AutomaterStatusEvent;

            WowAutomater.Stop();
        }

        private void SkinLootCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            WowAutomater.SkinLoot = SkinLootCheckbox.Checked;
        }

        private void ReversePathButton_Click(object sender, EventArgs e)
        {
            List<double> xCoordinates = Program.ExtractCommaDelimitedDoubles(XTextBox.Text);
            List<double> yCoordinates = Program.ExtractCommaDelimitedDoubles(YTextBox.Text);

            xCoordinates.Reverse();
            yCoordinates.Reverse();

            string xPath = string.Empty;
            string yPath = string.Empty;

            foreach(double xCoordinate in xCoordinates)
            {
                xPath += xCoordinate.ToString("N3") + ", ";
            }

            foreach (double yCoordinate in yCoordinates)
            {
                yPath += yCoordinate.ToString("N3") + ", ";
            }

            XTextBox.Text = xPath;
            YTextBox.Text = yPath;
        }

        private void AlwaysThrowCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.ThrowFlag = AlwaysThrowCheckBox.Checked;
        }

        private void StealthCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.StealthFlag = StealthCheckBox.Checked;
        }

        private void StealthCooldownNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.StealthTimer.Interval = (double)(StealthCooldownNumericInput.Value * 1000);
        }

        private void EvasionPercentaceNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.Rogue.Evasion.PlayerHealthPercentage = (int)EvasionPercentaceNumericInput.Value;
        }

        private void XReviveButtonLocationNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.XReviveButtonLocation = (double)XReviveButtonLocationNumericInput.Value * 100;
        }

        private void YReviveButtonLocationNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.YReviveButtonLocation = (double)YReviveButtonLocationNumericInput.Value * 100;
        }

        private void ReviveButtonLocationTestButton_Click(object sender, EventArgs e)
        {
            m_InputSimulator.Mouse.MoveMouseTo((double)XReviveButtonLocationNumericInput.Value * 100,
                                               (double)YReviveButtonLocationNumericInput.Value * 100);
        }

        private void PassiveHumanoidCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            WowAutomater.Druid.Passive = PassiveHumanoidCheckBox.Checked;
        }

        private void RegenerateVitalsNumericInput_ValueChanged(object sender, EventArgs e)
        {
            WowAutomater.RegenerateVitalsHealthPercentage = (double)RegenerateVitalsNumericInput.Value;
        }
    }
}
