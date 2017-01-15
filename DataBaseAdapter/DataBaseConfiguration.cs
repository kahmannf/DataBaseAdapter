using DataBaseAdapter.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAdapter
{
    public static class DataBaseConfiguration
    {
        public enum SupportedDataBases { MySql }

        private static SupportedDataBases? _type;
        public static SupportedDataBases DataBaseType
        {
            get
            {
                if (_type == null)
                {
                    throw new ConfigurationException("The DataBaseConfiguration was not set");
                }
                return (SupportedDataBases)_type;
            }
        }
        private static bool _portSet;
        private static int _port;
        private static string _server;
        private static string _database;
        private static string _userid;
        private static string _password;

        public static void SetDataBase(SupportedDataBases databasetype, string server, int port, string database, string userid, string password)
        {
            SetDataBase(databasetype, server, database, userid, password);
            _port = port;
            _portSet = true;
        }

        public static void SetDataBase(SupportedDataBases databasetype, string server, string database, string userid, string password)
        {
            _type = databasetype;
            _server = server;
            _database = database;
            _userid = userid;
            _password = password;
        }

        public static string GetConnectionString()
        {
            switch (_type)
            {
                case SupportedDataBases.MySql:
                    return GetMySqlConnectionStringInternal();
                default:
                    throw new ConfigurationException("The DataBaseConfiguration was not set");
            }
        }

        private static string GetMySqlConnectionStringInternal()
        {
            if (!_portSet)
            {
                return string.Format("Server={0}; database={1}; UID={2}; password={3}", _server, _database, _userid, _password);
            }
            else
            {
                return string.Format("Server={0}; Port={1}; database={2}; UID={3}; password={4}", _server, _port, _database, _userid, _password);
            }
        }
    }
}
