using System;

namespace TestyDroid
{
    public class MicrosoftAndroidEmulatorFactory : IEmulatorFactory
    {
        private IAndroidDebugBridgeFactory adbFactory;
        private Guid emuId;
        private string emulatorExePath;
        private string imageName;
        private ILogger logger;
        private int portNumber;
        private SingleInstanceMode singleInstanceMode;
        private bool noBootAnim;
        private bool noWindow;

        public MicrosoftAndroidEmulatorFactory(ILogger logger, string emulatorExePath, IAndroidDebugBridgeFactory adbFactory, string imageName, int portNumber, bool noBootAnim, bool noWindow, Guid emuId, SingleInstanceMode singleInstanceMode = SingleInstanceMode.Abort)
        {
            this.logger = logger;
            this.emulatorExePath = emulatorExePath;
            this.adbFactory = adbFactory;
            this.imageName = imageName;
            this.portNumber = portNumber;
            this.noBootAnim = noBootAnim;
            this.noWindow = noWindow;
            this.emuId = emuId;
            this.singleInstanceMode = singleInstanceMode;
            //this.Sku = "Android";
        }

        // public string Sku { get; set; }


        public IAndroidEmulator GetEmulator()
        {
           
            return new MicrosoftAndroidEmulator(logger, emulatorExePath, imageName, adbFactory, emuId, portNumber, this.singleInstanceMode, noBootAnim, noWindow);
            //throw new NotImplementedException();
        }
    }
}