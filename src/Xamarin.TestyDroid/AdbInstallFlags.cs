using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    [Flags]
    public enum AdbInstallFlags
    {
        [Description("")]
        None = 0,
        [Description("-l")]
        ForwardLockApplication = 1,
        [Description("-r")]
        ReplaceExistingApplication = 2,
        [Description("-t")]
        AllowTestPackages = 4,
        [Description("-s")]
        InstallApplicationOnSDCard = 8,
        [Description("-d")]
        AllowVersionCodeDowngrade = 16,
        [Description("-g")]
        GrantAllRuntimePermissions = 32
    }
}
