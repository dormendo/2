// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.SqlDbTypeHelper
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AutomateCommandCreation
{
  public class SqlDbTypeHelper
  {
    public static SqlDbType RetrieveSqlDbType(string tSqlTypeName)
    {
      if (tSqlTypeName.IndexOf("(") != -1)
        tSqlTypeName = tSqlTypeName.Remove(tSqlTypeName.IndexOf("("));
      KeyValuePair<SqlDbType, ParamTypeMeta> keyValuePair = SqlParamMetaData.Items.FirstOrDefault<KeyValuePair<SqlDbType, ParamTypeMeta>>((Func<KeyValuePair<SqlDbType, ParamTypeMeta>, bool>) (pd => pd.Value.TsqlType == tSqlTypeName));
      if (keyValuePair.Equals((object) new KeyValuePair<SqlDbType, ParamTypeMeta>()))
        throw new Exception(string.Format("SqlDbType not found by tSqlTypeName : {0}", (object) tSqlTypeName));
      return keyValuePair.Key;
    }
  }
}
