﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public interface IAndroidDebugBridgeFactory
    {
        IAndroidDebugBridge GetAndroidDebugBridge();
    }
}
