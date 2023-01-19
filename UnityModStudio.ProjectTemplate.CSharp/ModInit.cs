using System;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;
using UnityEngine;

namespace Doorstop;

class Entrypoint
{
    public static void Start()
    {
        // This method is called by Unity Doorstop during Mono initialization.
        // Place your mod initialization code here.
        // See https://github.com/NeighTools/UnityDoorstop#readme for documentation.

        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
    }

    private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
        // You have to wait until Unity is loaded before interacting with it.
        // The Main method is called before Unity has initialized.
        if (args.LoadedAssembly.GetType("UnityEngine.Application") != null)
        {
            Debug.Log("Hello from $safeprojectname$!");
        }
    }
}
