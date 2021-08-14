using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SecurityApp
{
    class Install
    {

        private string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private string programs = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";
        private RegistryKey reg = null;
        private string app = null;
        //SOFTWARE\Microsoft\Windows\CurrentVersion\Run
        //SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall

        public string appName
        {
            get { return app; }
            set { app = value; }
        }
        public string regKey
        {
            get { return reg.Name; }
            set { reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\" + value, true); }
        }

        //시작 경로 설정
        public void SetRegKey(string key, string execPath)
        {
            if (reg != null)
                reg.SetValue(key, execPath);
            else
                MessageBox.Show("regkey 설정 필요");
        }

        public void DelRegKey(string Key)
        {
            if (reg != null)
            {
                if (reg.GetValue(Key) != null)
                    reg.DeleteValue(Key);
                else
                    MessageBox.Show("해당 Key값 없음");
            }
            else
                MessageBox.Show("regkey 설정 필요");
        }
        public void DelRegFolder(string subKey)
        {
            if (reg != null)
                if (reg.OpenSubKey(subKey) != null)
                    reg.DeleteSubKey(subKey);
                else
                    MessageBox.Show("해당 폴더 없음");
            else
                MessageBox.Show("regKey 설정 필요");
        }

        public void ShortCut_Desktop(string target, string iconPath = "")
        {
            if (app != null)
            {
                WshShell wshIns;
                wshIns = new WshShell();
                IWshRuntimeLibrary.IWshShortcut myShotCut;
                string path = desktopPath + @"\" + app + ".lnk";

                FileInfo lnk = new FileInfo(path);
                if (lnk.Exists)
                {
                    lnk.Delete();
                }

                myShotCut = (IWshRuntimeLibrary.IWshShortcut)wshIns.CreateShortcut(path);
                myShotCut.TargetPath = target;
                myShotCut.Description = "";  //설명 부분
                //myShotCut.IconLocation = @"C:\TestFolder\아이콘.ico";
                myShotCut.Save();
            }
            else
            {
                MessageBox.Show("appName 설정 필요");
            }
        }

        public void ShortCut_Programs(string target, string iconPath = "")
        {
            if (app != null)
            {
                WshShell wshIns;
                wshIns = new WshShell();
                IWshRuntimeLibrary.IWshShortcut myShotCut;
                string path = programs + "//" + app + ".lnk";

                FileInfo lnk = new FileInfo(path);
                if (lnk.Exists)
                {
                    lnk.Delete();
                }
                myShotCut = (IWshRuntimeLibrary.IWshShortcut)wshIns.CreateShortcut(path);
                myShotCut.TargetPath = target;
                myShotCut.Description = "";  //설명 부분
                //myShotCut.IconLocation = @"C:\TestFolder\아이콘.ico";
                myShotCut.Save();
            }
            else
            {
                MessageBox.Show("appName 설정 필요");
            }

        }

        public void Del_ShortCut()
        {
            string path = desktopPath + @"\" + app + ".lnk";
            FileInfo shortcutFile = new FileInfo(path);
            if (shortcutFile.Exists)
            {
                shortcutFile.Delete();
            }

            path = programs + @"\" + app + ".lnk";
            shortcutFile = new FileInfo(path);
            if (shortcutFile.Exists)
            {
                shortcutFile.Delete();
            }
        }

    }
}
