using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Arithmic;
using Newtonsoft.Json;
// using MessageBox = System.Windows.Forms.MessageBox;
using Tiger;
using Tiger.Schema;

namespace Tiger;

public class LogSubsystem : Subsystem
{
    protected internal override bool Initialise()
    {
        Log.AddSink<FileSink>();
        Log.AddSink<ConsoleSink>();
        return true;
    }
}
