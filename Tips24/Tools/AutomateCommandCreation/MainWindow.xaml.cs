// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.MainWindow
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using AutomateCommandCreation.Generators;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace AutomateCommandCreation
{
  public partial class MainWindow : Window, IComponentConnector
  {
    private static string connStrMsSql = ConfigurationManager.ConnectionStrings["mssqlconnection"].ConnectionString;
    private List<SqlParamData> _queryParams;
    private List<List<ParamDataBase>> _resultColumns;
    private List<string> _proceduresMsSql;
    private List<SqlParamData> _queryStoredProcParams;
    private List<List<ParamDataBase>> _queryStoredProcOutColumns;

    public MainWindow()
    {
      this.InitializeComponent();
      this.AcquireMsSqlProceduresList();
      this._queryParams = new List<SqlParamData>();
      this._resultColumns = new List<List<ParamDataBase>>();
    }

    private void AcquireMsSqlProceduresList()
    {
      List<string> source = new List<string>();
      using (SqlConnection connection = new SqlConnection(MainWindow.connStrMsSql))
      {
        connection.Open();
        using (SqlCommand sqlCommand = new SqlCommand("SELECT SCHEMA_NAME(schema_id) + '.' + name from SYS.PROCEDURES ORDER BY SCHEMA_NAME(schema_id), name", connection))
        {
          using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
          {
            while (sqlDataReader.Read())
              source.Add(sqlDataReader.GetString(0).Trim());
          }
        }
      }
      this._proceduresMsSql = source.OrderBy<string, string>((Func<string, string>) (r => r)).ToList<string>();
      this.cbSqlProc.ItemsSource = (IEnumerable) this._proceduresMsSql;
    }

    private void AcquireMsSqlStoredProcParams(string procName)
    {
      this._queryStoredProcParams = new List<SqlParamData>();
      using (SqlConnection connection = new SqlConnection(MainWindow.connStrMsSql))
      {
        connection.Open();
        using (SqlCommand command = new SqlCommand(procName, connection))
        {
          command.CommandType = CommandType.StoredProcedure;
          SqlCommandBuilder.DeriveParameters(command);
          foreach (SqlParameter parameter in (DbParameterCollection) command.Parameters)
          {
            string paramName = parameter.ParameterName.Replace("@", "");
            if (parameter.SqlDbType == SqlDbType.Money)
            {
              parameter.Precision = (byte) 19;
              parameter.Scale = (byte) 2;
            }
            switch (parameter.Direction)
            {
              case ParameterDirection.Input:
                this._queryStoredProcParams.Add(new SqlParamData(paramName, parameter.SqlDbType, (int) parameter.Precision, (int) parameter.Scale, parameter.Size));
                continue;
              case ParameterDirection.InputOutput:
                this._queryStoredProcParams.Add(new SqlParamData(paramName, parameter.SqlDbType, (int) parameter.Precision, (int) parameter.Scale, parameter.Size, true));
                continue;
              default:
                continue;
            }
          }
        }
      }
    }

    private List<List<ParamDataBase>> AcquireMsSqlSchemaFromQuery(string text, List<SqlParamData> queryParams, bool isStoredProc = false)
    {
      List<List<ParamDataBase>> paramDataBaseListList = new List<List<ParamDataBase>>();
      try
      {
        bool flag = false;
        SqlConnection connection = new SqlConnection(MainWindow.connStrMsSql);
        try
        {
          connection.Open();
          if (isStoredProc)
            flag = this.CheckForTemporaryTables(ref text, connection);
          using (SqlCommand sqlCommand = new SqlCommand(text, connection))
          {
            if (isStoredProc)
              sqlCommand.CommandType = CommandType.StoredProcedure;
            foreach (SqlParamData queryParam in queryParams)
              sqlCommand.Parameters.Add(queryParam.ParamName, queryParam.Type);
            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.SchemaOnly))
            {
              do
              {
                DataTable schemaTable = sqlDataReader.GetSchemaTable();
                if (schemaTable != null)
                {
                  List<ParamDataBase> paramDataBaseList = new List<ParamDataBase>();
                  foreach (DataRow row in (InternalDataCollectionBase) schemaTable.Rows)
                  {
                    SqlParamData sqlParamData = new SqlParamData(row["ColumnName"] as string, (SqlDbType) row["ProviderType"], (int) (short) row["NumericPrecision"], (int) (short) row["NumericScale"], (int) row["ColumnSize"], (int) row["ColumnOrdinal"]);
                    paramDataBaseList.Add((ParamDataBase) sqlParamData);
                  }
                  paramDataBaseListList.Add(paramDataBaseList);
                }
              }
              while (sqlDataReader.NextResult());
            }
          }
        }
        finally
        {
          if (connection != null)
          {
            if (flag)
            {
              using (SqlCommand sqlCommand = new SqlCommand("DROP PROCEDURE " + text, connection))
                sqlCommand.ExecuteNonQuery();
            }
            connection.Dispose();
          }
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK);
      }
      return paramDataBaseListList;
    }

    private List<SqlParamData> AcquireMsSqlQueryParams(string sqlStatement)
    {
      List<SqlParamData> sqlParamDataList = new List<SqlParamData>();
      try
      {
        using (SqlConnection connection = new SqlConnection(MainWindow.connStrMsSql))
        {
          connection.Open();
          using (SqlCommand sqlCommand = new SqlCommand("sp_describe_undeclared_parameters", connection))
          {
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@tsql", (object) sqlStatement);
            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
            {
              while (sqlDataReader.Read())
              {
                string paramName = (sqlDataReader["name"] as string).Replace("@", "");
                SqlDbType type = SqlDbTypeHelper.RetrieveSqlDbType(sqlDataReader["suggested_system_type_name"] as string);
                sqlParamDataList.Add(new SqlParamData(paramName, type, (int) (byte) sqlDataReader["suggested_precision"], (int) (byte) sqlDataReader["suggested_scale"], (int) sqlDataReader["suggested_tds_length"]));
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK);
      }
      return sqlParamDataList;
    }

    private bool CheckForTemporaryTables(ref string spName, SqlConnection connection)
    {
      using (SqlCommand sqlCommand1 = new SqlCommand(string.Format("SELECT definition FROM sys.sql_modules WHERE object_id = (OBJECT_ID(N'{0}'));", (object) spName), connection))
      {
        string input;
        using (SqlDataReader sqlDataReader = sqlCommand1.ExecuteReader())
        {
          sqlDataReader.Read();
          input = sqlDataReader[0] as string;
          if (input == null)
            throw new Exception("Не удалось получить код определния процедуры");
        }
        RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
        MatchCollection matchCollection = new Regex("(?<tableDef>(CREATE)\\s+(TABLE)\\s+(#)(?<tableName>\\w+)\\s*)\\(", options).Matches(input);
        if (matchCollection.Count > 0)
        {
          string replacement = "CREATE PROCEDURE #" + spName;
          string format = "DECLARE {0} AS TABLE";
          foreach (Match match in matchCollection)
          {
            string oldValue = match.Groups["tableDef"].Value;
            string str1 = match.Groups["tableName"].Value;
            string str2 = Guid.NewGuid().ToString().Substring(0, 8);
            string str3 = string.Format("@{0}_{1}", (object) str1, (object) str2);
            string newValue = string.Format(format, (object) str3);
            input = input.Replace(oldValue, newValue);
            input = Regex.Replace(input, string.Format("(#{0})([\\s\\(\\)\\.])", (object) str1), string.Format("{0}$2", (object) str3), options);
          }
          using (SqlCommand sqlCommand2 = new SqlCommand(Regex.Replace(input, "(CREATE)\\s+(PROCEDURE)\\s+(.+)", replacement, options), connection))
          {
            sqlCommand2.ExecuteNonQuery();
            spName = "#" + spName;
            return true;
          }
        }
      }
      return false;
    }

    private void btnRefresMsSqlSpList_OnClick(object sender, RoutedEventArgs e)
    {
      this.AcquireMsSqlProceduresList();
      string selectedValue = this.cbSqlProc.SelectedValue as string;
      if (selectedValue == null)
        return;
      this.SelectStoredProc(selectedValue);
    }

    private void csCodeForQueryTabItemHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      this.CopyTextIntoClipboard(this.tbCsCodeForQuery);
    }

    private void csCodeForSpTabItemHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      this.CopyTextIntoClipboard(this.tbCsForSpCode);
    }

    private void tbQuery_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.btnGenerateQuery.IsEnabled = this.btnQueryParams.IsEnabled = this.btnQueryMetadata.IsEnabled = this.tbQuery.Text != string.Empty;
    }

    private void btnQueryParams_OnClick(object sender, RoutedEventArgs e)
    {
      this._queryParams = this.AcquireMsSqlQueryParams(this.tbQuery.Text);
      this.dgQueryParams.ItemsSource = (IEnumerable) this._queryParams;
    }

    private void btnQueryMetadata_OnClick(object sender, RoutedEventArgs e)
    {
      this._resultColumns = this.AcquireMsSqlSchemaFromQuery(this.tbQuery.Text, this._queryParams, false);
      this.redrawResultTabItems(this.tcResultColumns, this._resultColumns);
    }

    private void btnGenerateQuery_Click(object sender, RoutedEventArgs e)
    {
      this.tbCsCodeForQuery.Text = new QueryCsCodeGeneratorTemplate((CsCodeGeneratorAbstract) new QueryCsCodeGenerator(), this._queryParams.Any<SqlParamData>(), false, this._resultColumns.Any<List<ParamDataBase>>(), this.cbQueryGenerateRequestResponse.IsChecked.Value, this.cbQueryGenerateConnection.IsChecked.Value, false, this.cbQueryGenerateTransaction.IsChecked.Value, false).GenerateCommand(this.tbQuery.Text, (IEnumerable<ParamDataBase>) this._queryParams, Enumerable.Empty<ParamDataBase>(), (IEnumerable<IEnumerable<ParamDataBase>>) this._resultColumns);
      this.expanderQuery.IsExpanded = true;
    }

    private void cbSqlProc_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count == 0)
      {
        this.dgSqlInParams.ItemsSource = (IEnumerable) null;
        this.dgSqlOutParams.ItemsSource = (IEnumerable) null;
        this.tcStoredProcResultColumns.ItemsSource = (IEnumerable) null;
        this.btnCsGenerate.IsEnabled = false;
      }
      else
        this.SelectStoredProc((string) e.AddedItems[0]);
    }

    private void SelectStoredProc(string procName)
    {
      this.AcquireMsSqlStoredProcParams(procName);
      this._queryStoredProcOutColumns = this.AcquireMsSqlSchemaFromQuery(procName, this._queryStoredProcParams, true);
      this.redrawResultTabItems(this.tcStoredProcResultColumns, this._queryStoredProcOutColumns);
      this.dgSqlInParams.ItemsSource = (IEnumerable) this._queryStoredProcParams.Where<SqlParamData>((Func<SqlParamData, bool>) (p => !p.IsOutput));
      this.dgSqlOutParams.ItemsSource = (IEnumerable) this._queryStoredProcParams.Where<SqlParamData>((Func<SqlParamData, bool>) (p => p.IsOutput));
      this.btnCsGenerate.IsEnabled = true;
    }

    private void btnCsGenerate_Click(object sender, RoutedEventArgs e)
    {
      this.tbCsForSpCode.Text = new QueryCsCodeGeneratorTemplate((CsCodeGeneratorAbstract) new CsCodeGeneratorBase<SqlParamData>(), this._queryStoredProcParams.Any<SqlParamData>((Func<SqlParamData, bool>) (p => !p.IsOutput)), this._queryStoredProcParams.Any<SqlParamData>((Func<SqlParamData, bool>) (p => p.IsOutput)), this._queryStoredProcOutColumns.Any<List<ParamDataBase>>(), this.cbSqlSpGenerateRequestResponse.IsChecked.Value, this.cbSqlSpGenerateConnection.IsChecked.Value, this.cbSqlSpReturnValue.IsChecked.Value, this.cbSqlSpTransaction.IsChecked.Value, true).GenerateCommand(this.cbSqlProc.Text, (IEnumerable<ParamDataBase>) this._queryStoredProcParams.Where<SqlParamData>((Func<SqlParamData, bool>) (p => !p.IsOutput)), (IEnumerable<ParamDataBase>) this._queryStoredProcParams.Where<SqlParamData>((Func<SqlParamData, bool>) (p => p.IsOutput)), (IEnumerable<IEnumerable<ParamDataBase>>) this._queryStoredProcOutColumns);
      this.expanderSqlProc.IsExpanded = true;
    }

    private void CopyTextIntoClipboard(TextBox tb)
    {
      if (tb.Text == null)
        return;
      tb.SelectAll();
      tb.Copy();
      tb.Select(0, 0);
    }

    private void redrawResultTabItems(TabControl tab, List<List<ParamDataBase>> columns)
    {
      tab.Items.Clear();
      int num = 0;
      foreach (List<ParamDataBase> column in columns)
      {
        TabItem tabItem1 = new TabItem();
        tabItem1.Header = (object) ("Результат " + (object) num++);
        tabItem1.Content = (object) new ResultColumnControl();
        tabItem1.DataContext = (object) column;
        TabItem tabItem2 = tabItem1;
        tab.Items.Add((object) tabItem2);
      }
      tab.SelectedIndex = 0;
    }
  }
}
