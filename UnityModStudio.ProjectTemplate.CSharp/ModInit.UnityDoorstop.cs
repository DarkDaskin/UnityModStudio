﻿using System;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;
using UnityEngine;

namespace $safeprojectname$
{
    public static class ModInit
    {
        public static void Main()
        {
            // This method is called by Unity Doorstop during Mono initialization.
            // Place your mod initialization code here.
            // See https://github.com/NeighTools/UnityDoorstop/wiki for documentation.
        }
    }
}