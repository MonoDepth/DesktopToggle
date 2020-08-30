using MonoUtilities.Files;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopToggle
{
    class Settings : IniFile
    {
        public Settings(string aFilePath) : base(aFilePath) { }

        [Section("Behaviour")]
        public bool HideOnMinimize { get; set; } = true;
    }
}
