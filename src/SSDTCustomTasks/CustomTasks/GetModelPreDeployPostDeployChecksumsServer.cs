﻿using System;
using System.Data;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;


namespace CustomTasks
{
    public class GetModelPreDeployPostDeployChecksumsServer : Task
    {
        [Required]
        public string ServerName { get; set; }

        [Required]
        public string DatabaseName { get; set; }

        [Required]
        public string TableName { get; set; }

        [Required]
        public string TableSchemaName { get; set; }

        [Required]
        public string ColumnIdName { get; set; }

        [Required]
        public string ColumnModelChecksumName { get; set; }

        [Required]
        public string ColumnPreDeployChecksumName { get; set; }

        [Required]
        public string ColumnPostDeployChecksumName { get; set; }

        [Output]
        public string ModelChecksum { get; set; }

        [Output]
        public string PreDeployChecksum { get; set; }

        [Output]
        public string PostDeployChecksum { get; set; }


        public override bool Execute()
        {
            Log.LogMessage("Getting checksum for model uri in [{0}].[{1}].[{2}].[{3}]", ServerName, DatabaseName, TableSchemaName, TableName);

            ModelChecksum = string.Format("-1");
            PreDeployChecksum = string.Format("-1");
            PostDeployChecksum = string.Format("-1");

            Server _server = new Server(ServerName);

            try
            {
                Log.LogMessage("Connecting to server {0}...", _server);
                Log.LogMessage("Connected to server {0}. Server Version {1}...", _server, _server.Information.Version);

                #region database
                // Check the database exists, if it does not exist -1 will be returned. 
                DatabaseCollection _databaseCollection = _server.Databases;
                bool databaseExists = _databaseCollection.Contains(DatabaseName);

                Log.LogMessage("Checking database [{0}] exists on server [{1}]", DatabaseName, ServerName);

                if (databaseExists)
                {
                    Log.LogMessage("Database [{0}] exists on server [{1}]", DatabaseName, ServerName);

                    #region table
                    // Check the table exists, if it does not exist -1 will be returned
                    Database _database = _server.Databases[DatabaseName];
                    TableCollection _tableCollection = _database.Tables;

                    Log.LogMessage("Checking table [{0}].[{1}] exists in database [{2}] on server [{3}]", TableSchemaName, TableName, DatabaseName, ServerName);
                    bool tableExists = _tableCollection.Contains(TableName, TableSchemaName);

                    if (tableExists)
                    {
                        Log.LogMessage("Table [{0}].[{1}] exists in database [{2}] on server [{3}]", TableSchemaName, TableName, DatabaseName, ServerName);

                        #region columns
                        // Check the columns exist, if they don't exist -1 will be returned
                        Table _table = _database.Tables[TableName, TableSchemaName];
                        ColumnCollection _columnCollection = _table.Columns;

                        bool ColumnIdExists = _columnCollection.Contains(ColumnIdName);
                        bool ColumnModelChecksumExists = _columnCollection.Contains(ColumnModelChecksumName);
                        bool ColumnPreDeployChecksumExists = _columnCollection.Contains(ColumnPreDeployChecksumName);
                        bool ColumnPostDeployChecksumExists = _columnCollection.Contains(ColumnPostDeployChecksumName);

                        Log.LogMessage("Column Id Name {0} exists: {1}", ColumnIdName, ColumnIdExists);
                        Log.LogMessage("Column Model Checksum Name {0} exists: {1}", ColumnModelChecksumName, ColumnModelChecksumExists);
                        Log.LogMessage("Column PreDeploy Checksum Name {0} exists: {1}", ColumnPreDeployChecksumName, ColumnPreDeployChecksumExists);
                        Log.LogMessage("Column PostDeploy Checksum Name {0} exists: {1}", ColumnPostDeployChecksumName, ColumnPostDeployChecksumExists);

                        if (ColumnIdExists && ColumnModelChecksumExists && ColumnPreDeployChecksumExists && ColumnPostDeployChecksumExists)
                        {
                            string SQLSyntax = @"SELECT TOP 1 [" + ColumnModelChecksumName + "], [" + ColumnPreDeployChecksumName + "], [" + ColumnPostDeployChecksumName + "]  "
                                       + "FROM [" + TableSchemaName + "].[" + TableName + "] "
                                       + "ORDER BY [" + ColumnIdName + "] DESC";

                            Log.LogMessage("Executing sql statement: {0}", SQLSyntax);

                            DataSet _dataSet = _database.ExecuteWithResults(SQLSyntax);
                            if (_dataSet.Tables[0].Rows.Count < 1)
                            {
                                Log.LogWarning("Table [{0}].[{1}] contains 0 rows, returning -1", TableSchemaName, TableName);
                            }
                            //Assumption the query will always return 1 row
                            foreach (DataRow _dataRow in _dataSet.Tables[0].Rows)
                            {
                                ModelChecksum = Convert.ToString(_dataRow[ColumnModelChecksumName]);
                                PreDeployChecksum = Convert.ToString(_dataRow[ColumnPreDeployChecksumName]);
                                PostDeployChecksum = Convert.ToString(_dataRow[ColumnPostDeployChecksumName]);
                            }
                        }
                        else
                        {
                            Log.LogWarning("Column Id, Model, PreDeploy or PostDeploy Checksum Name does not exist, returning -1");
                        }
                        #endregion columns

                    }
                    else
                    {
                        Log.LogWarning("Table [{0}].[{1}] does not exist in database {2} on server {3}, returning -1", TableSchemaName, TableName, DatabaseName, ServerName);
                    }
                    #endregion table
                }
                else
                {
                    Log.LogWarning("Database [{0}] does not exist on Server [{1}], returning -1", DatabaseName, ServerName);
                }
                #endregion database
            }
            catch (SmoException smoex)
            {
                Log.LogError(smoex.Message);

                Exception ex;
                ex = smoex.InnerException;

                while (!object.ReferenceEquals(ex.InnerException, (null)))
                {
                    Log.LogError(ex.InnerException.Message);
                    ex = ex.InnerException;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex.Message);
            }


            return true;
        }
    }
}
