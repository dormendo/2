// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.StringBuilderExtensions
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Text;

namespace AutomateCommandCreation
{
  public static class StringBuilderExtensions
  {
    private const string IndentString = "    ";

    public static StringBuilder Indent(this StringBuilder sb, int level)
    {
      for (int index = 0; index < level; ++index)
        sb.Append("    ");
      return sb;
    }
  }
}
