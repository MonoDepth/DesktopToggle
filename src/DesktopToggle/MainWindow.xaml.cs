using MonoUtilities.Files;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonoUtilities.Conversions;
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
        private readonly Controller controller;
        private readonly Settings settings;


        public MainWindow()
        {
            settings = new Settings(IniFile.GetOrCreateProgramAppdataFolder("DesktopToggle") + "/settings.ini");
            settings.LoadConfig();
            controller = new Controller(ref settings, this);
            controller.SetupKeyboardHooks();
            InitializeComponent();
            TriggerKeyBtn.Content = settings.TriggerButton.ToString();
            DelayInput.Text = settings.DoubleClickMilliseconds.ToString();
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            TriggerKeyBtn.Content = "Press any key (Esc to abort)";
            int keyCode = await controller.ReadKeyAsync();
            if (keyCode != GlobalKeyboardHook.VkEscape) {
                TriggerKeyBtn.Content = keyCode.ToString();
                settings.TriggerButton = keyCode;
                settings.SaveConfig();
            }
        }
        private void TextBox_PreviewInput(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(DelayInput.Text + e.Text, out int delay) && delay > 0)
            {
                settings.DoubleClickMilliseconds = delay;
                settings.SaveConfig();
            }
            else
            {
                e.Handled = true;
            }
        }

        internal class Controller : IDisposable
        {
            public enum Mode
            {
                Assign,
                Listen
            }
            private GlobalKeyboardHook _globalKeyboardHook;
            private DateTime lastKeyPress = DateTime.MinValue;
            private int lastKeyUp = 0;
            private int lastKeyDown = 0;
            private int previousKeyPress = 0;
            private DesktopToggler desktopToggler = new DesktopToggler();
            private readonly Settings settings;
            private readonly MainWindow mainWindow;
            public Mode mode = Mode.Listen;
            

            public Controller(ref Settings settings, MainWindow mainWindow)
            {
                this.settings = settings;
                this.mainWindow = mainWindow;
            }

            public void SetupKeyboardHooks()
            {
                _globalKeyboardHook = new GlobalKeyboardHook();
                _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
            }

            private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
            {   
                switch(mode)
                {
                    case Mode.Listen:
                        HandleListen(e);
                        break;
                    case Mode.Assign:
                        HandleAssign(e);
                        break;
                }
            }

            public async Task<int> ReadKeyAsync()
            {                
                mode = Mode.Assign;
                lastKeyDown = 0;
                while (lastKeyDown == 0)
                {
                    await Task.Delay(50);
                }
                int key = lastKeyDown;
                lastKeyDown = 0;
                mode = Mode.Listen;
                return key;
            }

            private void HandleAssign(GlobalKeyboardHookEventArgs e)
            {
                if (e.KeyboardState != GlobalKeyboardHook.KeyboardState.KeyDown && e.KeyboardState != GlobalKeyboardHook.KeyboardState.SysKeyDown)
                    return;
                if (lastKeyDown == 0)
                    lastKeyDown = e.KeyboardData.VirtualCode;
            }

            private void HandleListen(GlobalKeyboardHookEventArgs e)
            {
                int currentKey = e.KeyboardData.VirtualCode;
                if (mainWindow.Visibility == Visibility.Visible)
                {
                    mainWindow.InputDebugTextBox.Text += e.KeyboardState.ToString() + " " +  e.KeyboardData.VirtualCode.ToString() + Environment.NewLine;
                }


                if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp && (currentKey == settings.TriggerButton && lastKeyUp == settings.TriggerButton && previousKeyPress == lastKeyUp) && (DateTime.Now - lastKeyPress).TotalMilliseconds <= settings.DoubleClickMilliseconds)
                {
                    lastKeyDown = 0;
                    lastKeyUp = 0;
                    desktopToggler.ToggleDesktopIcons();
                }
                else
                {
                    switch (e.KeyboardState)
                    {
                        case GlobalKeyboardHook.KeyboardState.SysKeyDown:
                        case GlobalKeyboardHook.KeyboardState.KeyDown:
                            previousKeyPress = lastKeyDown;
                            lastKeyDown = currentKey;
                            break;
                        case GlobalKeyboardHook.KeyboardState.SysKeyUp:
                        case GlobalKeyboardHook.KeyboardState.KeyUp:
                            lastKeyUp = currentKey;
                            lastKeyPress = DateTime.Now;
                            break;
                    }
                }
            }

            public void Dispose()
            {
                _globalKeyboardHook?.Dispose();
            }
        }
    }
}
