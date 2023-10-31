using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TaskTrayScreenChanger.Properties;
using System.Diagnostics;
using TaskTrayApplication;
//using System.Collections;
//using System.Text.Json;
//using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace TaskTrayScreenChanger
{


    public class TaskTrayApplicationContext : ApplicationContext
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        MenuItem loadMenuItem = new MenuItem("Load");
        MenuItem removeMenuItem = new MenuItem("Remove");

        Dictionary<string, screenState[]> ssss = new Dictionary<string, screenState[]>();
        Dictionary<string, string> ssss_json = new Dictionary<string, string>();


        readonly string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        readonly string jsonPath = "config.json";

        public static void screenStateDictToJsonFile(Dictionary<string, screenState[]> data,string filePath)
        {
            //var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }; // For pretty printing
            //var jsonString= JsonSerializer.Serialize(data, options);
            var jsonString = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, jsonString);
        }
        public static string screenStateToJson(screenState[] data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static Dictionary<string, screenState[]> ReadFromJsonFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Dictionary<string, screenState[]>>(jsonString);
            }
            return null;
        }


        private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupValue = "screenConfigTray";


        public TaskTrayApplicationContext()
        {
            MenuItem saveMenuItem = new MenuItem("Save", new EventHandler(ShowSaveConfig));
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));
            MenuItem settingMenuItem = new MenuItem("Setting", new EventHandler((sender, e) => { Process.Start("ms-settings:display"); }));


            RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
            MenuItem startupItem = new MenuItem("open on startup", new EventHandler((sender, e) => {
                if (key.GetValue(StartupValue) == null)
                {
                    key.SetValue(StartupValue, Application.ExecutablePath.ToString());

                    ((MenuItem)sender).Checked = true;
                }
                else
                {
                    key.DeleteValue(StartupValue);
                    ((MenuItem)sender).Checked = false;
                }
            }));
            if (key.GetValue(StartupValue) != null)
            {
                startupItem.Checked = true;
            }

            notifyIcon.Icon = Resources.AppIcon;
            notifyIcon.DoubleClick += new EventHandler(LoadDefault);
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {  loadMenuItem, saveMenuItem , removeMenuItem, settingMenuItem, startupItem, exitMenuItem });
            notifyIcon.Visible = true;


            Dictionary<string, screenState[]> ssss2 = ReadFromJsonFile(System.IO.Path.Combine(exePath, jsonPath));
            if (ssss2!=null)
            {
                foreach (KeyValuePair<string, screenState[]> entry in ssss2)
                {
                    if (entry.Key != "default")
                    {
                        ssss[entry.Key] = entry.Value;
                        ssss_json[entry.Key] = screenStateToJson(entry.Value);
                    }
                }
            }

            ssss["default"] = SetScreen.getScreenState();
            ssss_json["default"] = screenStateToJson(ssss["default"]);
            Debug.WriteLine(exePath);
            Debug.WriteLine(ssss_json["default"]);

            renewLoad();


        }

        void renewLoad()
        {
            loadMenuItem.MenuItems.Clear();
            removeMenuItem.MenuItems.Clear();

            int count = 0;
            foreach (KeyValuePair<string, screenState[]> entry in ssss)
            {

                MenuItem m = new MenuItem($"Load: {entry.Key}", new EventHandler(clickLoadConfig));
                m.Tag = entry.Key;
                loadMenuItem.MenuItems.Add(m);

                if (entry.Key != "default")
                {
                    MenuItem m2 = new MenuItem($"Remove: {entry.Key}", new EventHandler(clickRemoveConfig));
                    m2.Tag = entry.Key;
                    removeMenuItem.MenuItems.Add(m2);
                    count++;
                }


                // do something with entry.Value or entry.Key
            }

            removeMenuItem.Visible = count > 0;

            Debug.WriteLine($"count {count}");
        }
        void LoadDefault(object sender, EventArgs e)
        {
            LoadConfig("default");
        }

        void LoadConfig(string config_key)
        {
            int result=SetScreen.setScreenState(ssss[config_key]);
            Debug.WriteLine($"LoadConfig: {config_key} {result}");
        }

        void clickLoadConfig(object sender, EventArgs e)
        {
            LoadConfig((string)(((MenuItem)sender).Tag));
        }

        void clickRemoveConfig(object sender, EventArgs e)
        {
            string remove_id = (string)(((MenuItem)sender).Tag);
            if (ssss.ContainsKey(remove_id))
            {   
                Debug.WriteLine($"remove: {remove_id}");
                ssss.Remove(remove_id);
                renewLoad();
            }
        }

        void ShowSaveConfig(object sender, EventArgs e)
        {
            var ss=SetScreen.getScreenState();
            string ss_str = screenStateToJson(ss);

            foreach (KeyValuePair<string, string> entry in ssss_json)
            {
                if(entry.Value==ss_str && entry.Key!="default")
                {
                    MessageBox.Show($"same as {entry.Key}");
                    Debug.WriteLine(ss_str);
                    return;
                }
            }

            string value = "";
            if (InputBox("Input name:", "", ref value) == DialogResult.OK)
            {
                Debug.WriteLine($"value: {value}");
                ssss[value] = ss;
                ssss_json[value] = ss_str;


                screenStateDictToJsonFile(ssss, System.IO.Path.Combine(exePath, jsonPath));

                renewLoad();
            }
        }
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            notifyIcon.Visible = false;

            Application.Exit();
        }
    }
}
