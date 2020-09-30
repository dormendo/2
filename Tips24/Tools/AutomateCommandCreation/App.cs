// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.App
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;

namespace AutomateCommandCreation
{
  public class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [STAThread]
    public static void Main()
    {
      App app = new App();
      app.InitializeComponent();
      app.Run();
    }
  }
}
