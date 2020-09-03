using MonoUtilities.Files;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopToggle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Controller controller;
        private Settings settings;


        public MainWindow()
        {
            controller = new Controller();
            controller.SetupKeyboardHooks();
            settings = new Settings(IniFile.GetOrCreateProgramAppdataFolder("DesktopToggle"));
            settings.LoadConfig();
            InitializeComponent();
            MainNotifyIcon.DoubleClickCommand = new TrayClickCommand(ToggleWindow);            
        }

        public bool ToggleWindow()
        {
            switch(Visibility)
            {
                case Visibility.Visible:
                    Visibility = Visibility.Collapsed;
                    break;
                default:
                    Visibility = Visibility.Visible;
                    break;
            }            
            return true;
        }

        internal class Controller : IDisposable
        {
            private GlobalKeyboardHook _globalKeyboardHook;
            private DateTime lastKeyPress = DateTime.MinValue;
            private int lastKey = 0;
            private DesktopToggler desktopToggler = new DesktopToggler();

            public void SetupKeyboardHooks()
            {
                _globalKeyboardHook = new GlobalKeyboardHook();
                _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
            }

            private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
            {
                //Debug.WriteLine(e.KeyboardData.VirtualCode);
                if (e.KeyboardState != GlobalKeyboardHook.KeyboardState.KeyUp)
                    return;

                int currentKey = e.KeyboardData.VirtualCode;
                if (currentKey == GlobalKeyboardHook.VkLControl && lastKey == currentKey && (DateTime.Now - lastKeyPress).TotalMilliseconds <= 650)
                {
                    lastKey = 0;
                    desktopToggler.ToggleDesktopIcons();
                }
                else
                {
                    lastKeyPress = DateTime.Now;
                    lastKey = e.KeyboardData.VirtualCode;
                }                

                // seems, not needed in the life.
                //if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown &&
                //    e.KeyboardData.Flags == GlobalKeyboardHook.LlkhfAltdown)
                //{
                //    MessageBox.Show("Alt + Print Screen");
                //    e.Handled = true;
                //}
                //else
            }

            public void Dispose()
            {
                _globalKeyboardHook?.Dispose();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            controller.Dispose();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (settings.HideOnMinimize && WindowState == WindowState.Minimized)
            {
                ToggleWindow();
            }
        }
    }
}
