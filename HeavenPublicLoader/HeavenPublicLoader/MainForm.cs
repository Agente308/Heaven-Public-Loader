using Guna.UI2.WinForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace HeavenPublicLoader
{
    public class MainForm : Form
    {
        #region Constants
        private const string VERSION_URL = "https://raw.githubusercontent.com/CBaseAnon/HeavenL4D2/main/releases/latest.json";
        private const string CHANGELOG_BASE_URL = "https://raw.githubusercontent.com/CBaseAnon/HeavenL4D2/main/changelogs/";
        private const string VERSION_FILE = "version.txt";
        private const string DLL_NAME = "Heaven-beta.dll";
        private const string TARGET_PROCESS = "left4dead2";
        #endregion

        #region Fields
        private string currentLanguage = "en";
        private Dictionary<string, string> messages;
        private IContainer components;
        #endregion

        #region UI Components
        private Guna2BorderlessForm guna2BorderlessForm1;
        private Guna2Panel guna2Panel1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Guna2ComboBox guna2ComboBox1;
        private Guna2Button btnCheckUpdates;
        private Guna2TextBox txtChangelog;
        private Guna2Button btnShowChangelog;
        private Guna2Button guna2Button1;
        private Guna2DragControl guna2DragControl1;
        private Guna2DragControl guna2DragControl2;
        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();
            InitializeMessages();
            EnsureLocalFilesExist();
            ApplyLanguageToUI();
        }
        #endregion

        #region Initialization Methods
        private void InitializeMessages()
        {
            messages = new Dictionary<string, string>
            {
                // Update Success
                { "en_update_success", "Updated successfully." },
                { "es_update_success", "Actualización completada." },
                { "pt_update_success", "Atualização concluída." },
                { "fr_update_success", "Mis à Jour Correctement." },
                { "cn_update_success", "更新成功。" },

                // Up to Date
                { "en_up_to_date", "Already up to date." },
                { "es_up_to_date", "Ya está actualizado." },
                { "pt_up_to_date", "Já está atualizado." },
                { "fr_up_to_date", "Déjà mis à jour" },
                { "cn_up_to_date", "已是最新版本。" },

                // Update Failed
                { "en_update_failed", "Update check failed:" },
                { "es_update_failed", "Error al buscar actualizaciones:" },
                { "pt_update_failed", "Falha ao verificar atualização:" },
                { "fr_update_failed", "Vérification de mis à jour échoué" },
                { "cn_update_failed", "更新检查失败" },

                // Changelog Failed
                { "en_changelog_failed", "Failed to load changelog:" },
                { "es_changelog_failed", "Error al cargar el log de cambios:" },
                { "pt_changelog_failed", "Falha ao carregar changelog:" },
                { "fr_changelog_failed", "Chargement du Journal de Modifications échoué" },
                { "cn_changelog_failed", "无法加载更新日志:" },

                // Button Labels
                { "en_btn_update", "Check Updates" },
                { "es_btn_update", "Buscar Actualizaciones" },
                { "fr_btn_update", "Vérifier les Mises à Jour" },
                { "cn_btn_update", "检查更新" },

                { "en_btn_changelog", "Get Changelog" },
                { "es_btn_changelog", "Obtener Changelog" },
                { "fr_btn_changelog", "Avoir Le Journal De Modifications" },
                { "cn_btn_changelog", "更新日志" },

                { "en_btn_inject", "Inject" },
                { "es_btn_inject", "Inyectar" },
                { "fr_btn_inject", "Injecter" },
                { "cn_btn_inject", "注入" },

                { "en_lbl_language", "Language" },
                { "es_lbl_language", "Idioma" },
                { "fr_lbl_language", "Langue" },
                { "cn_lbl_language", "语言:" }
            };
        }

        private void EnsureLocalFilesExist()
        {
            string versionPath = Path.Combine(Application.StartupPath, VERSION_FILE);
            if (!File.Exists(versionPath))
            {
                File.WriteAllText(versionPath, "0.0.0");
            }
        }
        #endregion

        #region Localization Methods
        private void ApplyLanguageToUI()
        {
            btnCheckUpdates.Text = T("btn_update");
            btnShowChangelog.Text = T("btn_changelog");
            guna2Button1.Text = T("btn_inject");
            label4.Text = T("lbl_language");
        }

        private string T(string key)
        {
            string fullKey = currentLanguage + "_" + key;
            return messages.ContainsKey(fullKey) ? messages[fullKey] : key;
        }
        #endregion

        #region Version Management
        private string GetLocalVersion()
        {
            return File.ReadAllText(Path.Combine(Application.StartupPath, VERSION_FILE)).Trim();
        }

        private void SetLocalVersion(string version)
        {
            File.WriteAllText(Path.Combine(Application.StartupPath, VERSION_FILE), version);
        }

        private string GetDllPath()
        {
            return Path.Combine(Application.StartupPath, DLL_NAME);
        }

        private bool DllExists()
        {
            return File.Exists(GetDllPath());
        }
        #endregion

        #region Async Methods (Placeholder - requires implementation)
        private async Task<UpdateInfo> GetGithubInfo()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "HeavenPublicLoader");
                var response = await client.GetStringAsync(VERSION_URL);
                return JsonConvert.DeserializeObject<UpdateInfo>(response);
            }
        }

        private async Task<string> GetChangelog()
        {
            string changelogUrl = CHANGELOG_BASE_URL + "changelog_" + currentLanguage + ".txt";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "HeavenPublicLoader");
                return await client.GetStringAsync(changelogUrl);
            }
        }

        private async Task DownloadDll(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "HeavenPublicLoader");
                var dllBytes = await client.GetByteArrayAsync(url);
                File.WriteAllBytes(GetDllPath(), dllBytes);
            }
        }
        #endregion

        #region DLL Injection Methods
        private void InjectDll(Process targetProcess, string dllPath)
        {
            IntPtr processHandle = OpenProcess(0x1F0FFF, false, targetProcess.Id);

            if (processHandle == IntPtr.Zero)
            {
                MessageBox.Show("Failed to open process.");
                return;
            }

            IntPtr allocMemAddress = VirtualAllocEx(
                processHandle,
                IntPtr.Zero,
                (uint)(dllPath.Length + 1),
                0x3000,
                0x04
            );

            if (allocMemAddress == IntPtr.Zero)
            {
                MessageBox.Show("Memory allocation failed.");
                return;
            }

            uint bytesWritten;
            WriteProcessMemory(
                processHandle,
                allocMemAddress,
                dllPath,
                (uint)(dllPath.Length + 1),
                out bytesWritten
            );

            IntPtr loadLibraryAddr = GetProcAddress(
                GetModuleHandle("kernel32.dll"),
                "LoadLibraryA"
            );

            if (loadLibraryAddr == IntPtr.Zero)
            {
                MessageBox.Show("Failed to resolve LoadLibraryA.");
                return;
            }

            IntPtr threadHandle = CreateRemoteThread(
                processHandle,
                IntPtr.Zero,
                0,
                loadLibraryAddr,
                allocMemAddress,
                0,
                IntPtr.Zero
            );

            if (threadHandle == IntPtr.Zero)
            {
                MessageBox.Show("Thread creation failed.");
                return;
            }

            MessageBox.Show("Injection successful.");
        }

        private void TryInject()
        {
            Process targetProcess = Process.GetProcessesByName(TARGET_PROCESS).FirstOrDefault();

            if (targetProcess == null)
            {
                MessageBox.Show("Valve001 process not found.");
                return;
            }

            string dllPath = Path.Combine(Application.StartupPath, DLL_NAME);

            if (!File.Exists(dllPath))
            {
                MessageBox.Show($"DLL not found:\n{dllPath}");
                return;
            }

            InjectDll(targetProcess, dllPath);
        }
        #endregion

        #region Windows API Imports
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, string lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        #endregion

        #region Event Handlers
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Form load logic
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private async void btnShowChangelog_Click(object sender, EventArgs e)
        {
            try
            {
                btnShowChangelog.Enabled = false;
                string changelog = await GetChangelog();
                txtChangelog.Text = changelog;
            }
            catch (Exception ex)
            {
                MessageBox.Show(T("changelog_failed") + " " + ex.Message);
            }
            finally
            {
                btnShowChangelog.Enabled = true;
            }
        }

        private async void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            try
            {
                btnCheckUpdates.Enabled = false;

                UpdateInfo githubInfo = await GetGithubInfo();
                string localVersion = GetLocalVersion();

                if (githubInfo.version != localVersion)
                {
                    await DownloadDll(githubInfo.dll_url);
                    SetLocalVersion(githubInfo.version);
                    MessageBox.Show(T("update_success"));
                }
                else
                {
                    MessageBox.Show(T("up_to_date"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(T("update_failed") + " " + ex.Message);
            }
            finally
            {
                btnCheckUpdates.Enabled = true;
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            TryInject();
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (guna2ComboBox1.SelectedIndex)
            {
                case 0:
                    currentLanguage = "es";
                    break;
                case 1:
                    currentLanguage = "en";
                    break;
                case 2:
                    currentLanguage = "fr";
                    break;
                case 3:
                    currentLanguage = "cn";
                    break;
                case 4:
                    currentLanguage = "pt";
                    break;
                default:
                    currentLanguage = "en";
                    break;
            }
            ApplyLanguageToUI();
        }
        #endregion

        #region Windows Forms Designer Generated Code
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));

            guna2BorderlessForm1 = new Guna2BorderlessForm(components);
            guna2Panel1 = new Guna2Panel();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            btnCheckUpdates = new Guna2Button();
            guna2ComboBox1 = new Guna2ComboBox();
            label4 = new Label();
            btnShowChangelog = new Guna2Button();
            txtChangelog = new Guna2TextBox();
            guna2Button1 = new Guna2Button();
            guna2DragControl1 = new Guna2DragControl(components);
            guna2DragControl2 = new Guna2DragControl(components);

            guna2Panel1.SuspendLayout();
            SuspendLayout();

            // guna2BorderlessForm1
            guna2BorderlessForm1.ContainerControl = this;
            guna2BorderlessForm1.DockForm = false;
            guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6;
            guna2BorderlessForm1.HasFormShadow = false;
            guna2BorderlessForm1.ResizeForm = false;
            guna2BorderlessForm1.ShadowColor = Color.White;
            guna2BorderlessForm1.TransparentWhileDrag = true;

            // guna2Panel1
            guna2Panel1.BackColor = Color.Transparent;
            guna2Panel1.Controls.Add(label3);
            guna2Panel1.Controls.Add(label2);
            guna2Panel1.Controls.Add(label1);
            guna2Panel1.FillColor = Color.FromArgb(18, 18, 18);
            guna2Panel1.Location = new Point(0, 0);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.Size = new Size(600, 30);
            guna2Panel1.TabIndex = 0;

            // label3
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 10.25f);
            label3.ForeColor = Color.White;
            label3.Location = new Point(7, 6);
            label3.Name = "label3";
            label3.Size = new Size(57, 17);
            label3.TabIndex = 3;
            label3.Text = "Heaven";

            // label2
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 10.25f);
            label2.ForeColor = Color.White;
            label2.Location = new Point(557, 6);
            label2.Name = "label2";
            label2.Size = new Size(13, 17);
            label2.TabIndex = 2;
            label2.Text = "-";
            label2.Click += label2_Click;

            // label1
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 10.25f);
            label1.ForeColor = Color.White;
            label1.Location = new Point(576, 6);
            label1.Name = "label1";
            label1.Size = new Size(17, 17);
            label1.TabIndex = 1;
            label1.Text = "X";
            label1.Click += label1_Click;

            // btnCheckUpdates
            btnCheckUpdates.BackColor = Color.Transparent;
            btnCheckUpdates.BorderRadius = 12;
            btnCheckUpdates.DisabledState.BorderColor = Color.DarkGray;
            btnCheckUpdates.DisabledState.CustomBorderColor = Color.DarkGray;
            btnCheckUpdates.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnCheckUpdates.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnCheckUpdates.FillColor = Color.FromArgb(15, 15, 15);
            btnCheckUpdates.Font = new Font("Segoe UI", 9f);
            btnCheckUpdates.ForeColor = Color.White;
            btnCheckUpdates.Location = new Point(331, 48);
            btnCheckUpdates.Name = "btnCheckUpdates";
            btnCheckUpdates.ShadowDecoration.BorderRadius = 12;
            btnCheckUpdates.ShadowDecoration.Color = Color.FromArgb(140, 140, 140);
            btnCheckUpdates.ShadowDecoration.Enabled = true;
            btnCheckUpdates.Size = new Size(180, 45);
            btnCheckUpdates.TabIndex = 1;
            btnCheckUpdates.Text = "Check Updates";
            btnCheckUpdates.Click += btnCheckUpdates_Click;

            // guna2ComboBox1
            guna2ComboBox1.BackColor = Color.Transparent;
            guna2ComboBox1.BorderRadius = 12;
            guna2ComboBox1.BorderThickness = 0;
            guna2ComboBox1.DrawMode = DrawMode.OwnerDrawFixed;
            guna2ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            guna2ComboBox1.FillColor = Color.FromArgb(15, 15, 15);
            guna2ComboBox1.FocusedColor = Color.FromArgb(94, 148, 255);
            guna2ComboBox1.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            guna2ComboBox1.Font = new Font("Segoe UI", 10f);
            guna2ComboBox1.ForeColor = Color.FromArgb(68, 88, 112);
            guna2ComboBox1.HoverState.FillColor = Color.FromArgb(18, 18, 18);
            guna2ComboBox1.ItemHeight = 30;
            guna2ComboBox1.Items.AddRange(new object[] { "ES", "EN", "FR", "CN", "PT" });
            guna2ComboBox1.ItemsAppearance.BackColor = Color.FromArgb(15, 15, 15);
            guna2ComboBox1.ItemsAppearance.SelectedBackColor = Color.FromArgb(20, 20, 20);
            guna2ComboBox1.Location = new Point(459, 402);
            guna2ComboBox1.Name = "guna2ComboBox1";
            guna2ComboBox1.ShadowDecoration.BorderRadius = 12;
            guna2ComboBox1.ShadowDecoration.Color = Color.FromArgb(140, 140, 140);
            guna2ComboBox1.ShadowDecoration.Enabled = true;
            guna2ComboBox1.Size = new Size(121, 36);
            guna2ComboBox1.StartIndex = 1;
            guna2ComboBox1.TabIndex = 2;
            guna2ComboBox1.SelectedIndexChanged += guna2ComboBox1_SelectedIndexChanged;

            // label4
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 10.25f);
            label4.ForeColor = Color.White;
            label4.Location = new Point(456, 372);
            label4.Name = "label4";
            label4.Size = new Size(76, 17);
            label4.TabIndex = 4;
            label4.Text = "Language:";

            // btnShowChangelog
            btnShowChangelog.BackColor = Color.Transparent;
            btnShowChangelog.BorderRadius = 12;
            btnShowChangelog.DisabledState.BorderColor = Color.DarkGray;
            btnShowChangelog.DisabledState.CustomBorderColor = Color.DarkGray;
            btnShowChangelog.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnShowChangelog.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnShowChangelog.FillColor = Color.FromArgb(15, 15, 15);
            btnShowChangelog.Font = new Font("Segoe UI", 9f);
            btnShowChangelog.ForeColor = Color.White;
            btnShowChangelog.Location = new Point(331, 99);
            btnShowChangelog.Name = "btnShowChangelog";
            btnShowChangelog.ShadowDecoration.BorderRadius = 12;
            btnShowChangelog.ShadowDecoration.Color = Color.FromArgb(140, 140, 140);
            btnShowChangelog.ShadowDecoration.Enabled = true;
            btnShowChangelog.Size = new Size(180, 45);
            btnShowChangelog.TabIndex = 5;
            btnShowChangelog.Text = "Get Changelog";
            btnShowChangelog.Click += btnShowChangelog_Click;

            // txtChangelog
            txtChangelog.AutoScroll = true;
            txtChangelog.BorderRadius = 12;
            txtChangelog.Cursor = Cursors.IBeam;
            txtChangelog.DefaultText = "";
            txtChangelog.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtChangelog.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtChangelog.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtChangelog.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtChangelog.FillColor = Color.FromArgb(15, 15, 15);
            txtChangelog.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtChangelog.Font = new Font("Segoe UI", 9f);
            txtChangelog.ForeColor = Color.FromArgb(180, 180, 180);
            txtChangelog.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtChangelog.Location = new Point(10, 48);
            txtChangelog.Multiline = true;
            txtChangelog.Name = "txtChangelog";
            txtChangelog.PlaceholderForeColor = Color.Black;
            txtChangelog.PlaceholderText = "";
            txtChangelog.ReadOnly = true;
            txtChangelog.SelectedText = "";
            txtChangelog.Size = new Size(315, 390);
            txtChangelog.TabIndex = 6;

            // guna2Button1
            guna2Button1.BackColor = Color.Transparent;
            guna2Button1.BorderRadius = 12;
            guna2Button1.DisabledState.BorderColor = Color.DarkGray;
            guna2Button1.DisabledState.CustomBorderColor = Color.DarkGray;
            guna2Button1.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            guna2Button1.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            guna2Button1.FillColor = Color.FromArgb(15, 15, 15);
            guna2Button1.Font = new Font("Segoe UI", 10f);
            guna2Button1.ForeColor = Color.White;
            guna2Button1.Location = new Point(331, 308);
            guna2Button1.Name = "guna2Button1";
            guna2Button1.ShadowDecoration.BorderRadius = 12;
            guna2Button1.ShadowDecoration.Color = Color.FromArgb(170, 170, 170);
            guna2Button1.ShadowDecoration.Enabled = true;
            guna2Button1.ShadowDecoration.Shadow = new Padding(4);
            guna2Button1.Size = new Size(180, 45);
            guna2Button1.TabIndex = 7;
            guna2Button1.Text = "Inject";
            guna2Button1.Click += guna2Button1_Click;

            // guna2DragControl1
            guna2DragControl1.DockIndicatorTransparencyValue = 0.6;
            guna2DragControl1.TargetControl = label3;
            guna2DragControl1.UseTransparentDrag = true;

            // guna2DragControl2
            guna2DragControl2.DockIndicatorTransparencyValue = 0.6;
            guna2DragControl2.TargetControl = guna2Panel1;
            guna2DragControl2.UseTransparentDrag = true;

            // MainForm
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(13, 13, 13);
            ClientSize = new Size(596, 450);
            Controls.Add(guna2Button1);
            Controls.Add(txtChangelog);
            Controls.Add(btnShowChangelog);
            Controls.Add(label4);
            Controls.Add(guna2ComboBox1);
            Controls.Add(btnCheckUpdates);
            Controls.Add(guna2Panel1);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimizeBox = false;
            Name = "MainForm";
            Text = "Heaven-Beta Public Loader \\ By Aika\\Lucky";
            Load += MainForm_Load;

            guna2Panel1.ResumeLayout(false);
            guna2Panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        #region Nested Classes
        public class UpdateInfo
        {
            public string version { get; set; }
            public string dll_url { get; set; }
        }
        #endregion
    }
}