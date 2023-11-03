using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TaskTrayScreenChanger.Properties;
using System.Diagnostics;
using TaskTrayApplication;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace TaskTrayScreenChanger
{


    public class TaskTrayApplicationContext : ApplicationContext
    {
        
        private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupValue = "MultiScreenProfileSwitcher";

        readonly string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        readonly string configFilename = "config.json";

        NotifyIcon notifyIcon = new NotifyIcon();
        MenuItem loadMenuItem = new MenuItem("Load Profiles");
        MenuItem removeMenuItem = new MenuItem("Remove Profiles");

        Dictionary<string, screenState[]> screenStatesDict = new Dictionary<string, screenState[]>();

        public static void saveToFile(Dictionary<string, screenState[]> data,string filePath)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, jsonString);
        }

        public static Dictionary<string, screenState[]> loadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Dictionary<string, screenState[]>>(jsonString);
            }
            return null;
        }




        public TaskTrayApplicationContext()
        {
            //some buttons
            MenuItem saveMenuItem = new MenuItem("Save Current Profile", new EventHandler(ShowSaveConfig));
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));
            MenuItem settingMenuItem = new MenuItem("Display Setting", new EventHandler((sender, e) => { Process.Start("ms-settings:display"); }));

            //screen setting button
            RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
            MenuItem startupItem = new MenuItem("Auto Open at Startup", new EventHandler((sender, e) => {
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

            //icon
            notifyIcon.Icon = Resources.AppIcon;
            notifyIcon.DoubleClick += new EventHandler(LoadDefault);
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {  loadMenuItem, saveMenuItem , removeMenuItem, settingMenuItem, startupItem, exitMenuItem });
            notifyIcon.Visible = true;

            //default screen profile
            screenStatesDict["default"] = SetScreen.getScreenState();

            //load config
            Dictionary<string, screenState[]> screenStatesDict2 = loadFile(System.IO.Path.Combine(exePath, configFilename));
            if (screenStatesDict2!=null)
            {
                foreach (KeyValuePair<string, screenState[]> entry in screenStatesDict2)
                {
                    if (entry.Key != "default")
                    {
                        screenStatesDict[entry.Key] = entry.Value;
                    }
                }
            }

            renewLoad();
        }

        void renewLoad()
        {
            loadMenuItem.MenuItems.Clear();
            removeMenuItem.MenuItems.Clear();


            string currentStr = SetScreen.ScreenStateArrayToString(SetScreen.getScreenState());
            string currUsing="default";
            foreach (KeyValuePair<string, screenState[]> entry in screenStatesDict)
            {
                if (entry.Key== "default")
                {
                    continue;
                }

                if (SetScreen.ScreenStateArrayToString(screenStatesDict["default"]) == SetScreen.ScreenStateArrayToString(screenStatesDict[entry.Key]))
                {
                    currUsing = entry.Key;
                    break;
                }
            }

            int count = 0;
            foreach (KeyValuePair<string, screenState[]> entry in screenStatesDict)
            {
                if(entry.Key == "default" && currUsing != "default")
                {
                    continue;
                }

                MenuItem m = new MenuItem($"Load: {entry.Key}", new EventHandler(clickLoadConfig));
                m.Tag = entry.Key;
                loadMenuItem.MenuItems.Add(m);

                if (SetScreen.ScreenStateArrayToString(screenStatesDict[entry.Key]) == currentStr)
                {
                    m.Checked = true;
                }

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
            int result=SetScreen.setScreenState(screenStatesDict[config_key]);

            renewLoad();
            Debug.WriteLine($"LoadConfig: {config_key} {result}");
        }

        void clickLoadConfig(object sender, EventArgs e)
        {
            LoadConfig((string)(((MenuItem)sender).Tag));
        }

        void clickRemoveConfig(object sender, EventArgs e)
        {
            string remove_id = (string)(((MenuItem)sender).Tag);
            if (screenStatesDict.ContainsKey(remove_id))
            {   
                Debug.WriteLine($"remove: {remove_id}");
                screenStatesDict.Remove(remove_id);
                renewLoad();
            }
        }

        void ShowSaveConfig(object sender, EventArgs e)
        {
            var ss=SetScreen.getScreenState();
            string ss_str = SetScreen.ScreenStateArrayToString(ss);


            foreach (KeyValuePair<string, screenState[]> entry in screenStatesDict)
            {
                if (SetScreen.ScreenStateArrayToString(screenStatesDict[entry.Key]) == ss_str && entry.Key!="default")
                {
                    MessageBox.Show($"Current Profile is same as {entry.Key}");
                    Debug.WriteLine(ss_str);
                    return;
                }
            }

            string value = "";
            if (InputBox("Input Profile Name:", "", ref value) == DialogResult.OK)
            {
                Debug.WriteLine($"value: {value}");
                screenStatesDict[value] = ss;

                saveToFile(screenStatesDict, System.IO.Path.Combine(exePath, configFilename));

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
