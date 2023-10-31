using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TaskTrayApplication
{

    // Struct for DEVMODE
    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        private const int CCHDEVICENAME = 0x20;
        private const int CCHFORMNAME = 0x20;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public ScreenOrientation dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    public enum DisplaySettingsFlags : int
    {
        CDS_UPDATEREGISTRY = 1,
        CDS_TEST = 2,
        CDS_FULLSCREEN = 4,
        CDS_GLOBAL = 8,
        CDS_SET_PRIMARY = 0x10,
        CDS_RESET = 0x40000000,
        CDS_NORESET = 0x10000000
    }

    public enum ScreenOrientation : int
    {
        DMDO_DEFAULT = 0,
        DMDO_90 = 1,
        DMDO_180 = 2,
        DMDO_270 = 3
    }
    public struct screenState
    {
        public string name;
        public int x;
        public int y;
        public int width;
        public int height;
        public int rotate;
        public int hz;

        public override string ToString() {
            return $"{name};{x};{y};{width};{height};{rotate};{hz}";
        }

        public screenState fromString(string s) {
            string[] arr = s.Split(';');
            return new screenState { name = arr[0], x = int.Parse(arr[1]), y = int.Parse(arr[2]), width = int.Parse(arr[3]), height = int.Parse(arr[4]), rotate = int.Parse(arr[5]), hz = int.Parse(arr[6]) };
        }
    }


    internal class SetScreen
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, DisplaySettingsFlags flags);

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsExA(
            string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd,
            DisplaySettingsFlags dwflags, IntPtr lParam);

        public static string toString(screenState[] states)
        {
            string s="";
            foreach (screenState ss in states)
            {
                s+= ss.ToString();
            }
            return s;
        }

        public static screenState[] getScreenState()
        {
            screenState[] sss= new screenState[Screen.AllScreens.Count()];
            int i = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                string id = screen.DeviceName;
                Point position = screen.Bounds.Location;
                int width = screen.Bounds.Width;
                int height = screen.Bounds.Height;

                DEVMODE originalMode = new DEVMODE();
                EnumDisplaySettings(id, -1, ref originalMode);

                sss[i] = new screenState()
                {
                    name = id,
                    x = originalMode.dmPositionX,
                    y = originalMode.dmPositionY,
                    width = originalMode.dmPelsWidth,
                    height = originalMode.dmPelsHeight,
                    rotate = (int)originalMode.dmDisplayOrientation,
                    hz = originalMode.dmDisplayFrequency
                };
                i++;
            }
            return sss;
        }

        public static int setScreenState(screenState[] states)
        {
            int Rets = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                string id = screen.DeviceName;

                foreach (screenState ss in states)
                {
                    if (ss.name == id)
                    {
                        DEVMODE originalMode = new DEVMODE();
                        EnumDisplaySettings(id, -1, ref originalMode); // Get the current settings

                        DEVMODE newMode = originalMode;

                        newMode.dmPositionX = ss.x;
                        newMode.dmPositionY = ss.y;
                        newMode.dmPelsWidth = ss.width;
                        newMode.dmPelsHeight = ss.height;
                        newMode.dmDisplayOrientation = (ScreenOrientation)ss.rotate;
                        newMode.dmDisplayFrequency = ss.hz;
                        int iRet = ChangeDisplaySettingsExA(id, ref newMode, IntPtr.Zero, DisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
                        if (iRet != 0 && Rets == 0)
                        {
                            Rets = iRet;
                        }
                        break;
                    }
                }
            }
            return Rets;
        }




    }
}
