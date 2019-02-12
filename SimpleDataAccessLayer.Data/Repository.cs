using Microsoft.Win32.SafeHandles;
using SimpleDataAccessLayer.Util;
using SimpleDataAccessLayer.Util.Enumerators;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleDataAccessLayer.Data
{
    public class Repository : IDisposable
    {
        string textConnection;
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        public Repository()
        {
            this.textConnection = ConfigurationManager.ConnectionStrings["mainConnection"].ConnectionString;
        }

        #region Generic Methods
        public virtual List<T> ListModel<T>(Expression<Func<T, bool>> conditional = null)
        {
            List<T> _list = new List<T>();

            List<string> _fields = HelperSQL.GetFieldName<T>();

            StringBuilder _textCommand = new StringBuilder();
            _textCommand.AppendLine(
                string.Format("SELECT {0} FROM {1} {2};",
                    string.Join(", ", _fields),
                    HelperSQL.GetTableName<T>(),
                    conditional != null ? conditional.ConvertExpression<T>() : ""
                    )
                );

            SqlCommand _command = new SqlCommand(_textCommand.ToString(), new SqlConnection(this.textConnection), null);
            _command.Connection.Open();
            try
            {
                SqlDataReader _dataReader = _command.ExecuteReader();
                while (_dataReader != null && _dataReader.Read())
                {
                    T _object = _dataReader.DataReaderToObject<T>();
                    _list.Add(_object);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _command.Connection.Close();

            return _list;
        }

        public virtual T Add<T>(T obj)
        {
            Dictionary<string, string> _fieldAndValue = obj.GetFieldNameAndValue<T>(SQLCommand.Insert);

            StringBuilder _textCommand = new StringBuilder();

            _textCommand.AppendLine(string.Format(" INSERT INTO {0} ({1}) ", HelperSQL.GetTableName<T>(), string.Join(", ", _fieldAndValue.Select(a => a.Key).ToList())));
            _textCommand.AppendLine(string.Format(" OUTPUT inserted.* ", HelperSQL.GetPrimaryKeyName<T>()));
            _textCommand.AppendLine(string.Format(" VALUES ({0}); ", string.Join(", ", _fieldAndValue.Select(a => a.Value).ToList())));

            using (SqlConnection _connection = new SqlConnection(this.textConnection))
            {
                _connection.Open();

                SqlCommand _sqlCommand = new SqlCommand(_textCommand.ToString(), _connection, null);
                _sqlCommand.Transaction = _connection.BeginTransaction();
                try
                {
                    using (SqlDataReader _dataReader = _sqlCommand.ExecuteReader())
                    {
                        if (_dataReader != null && _dataReader.Read())
                        {
                            obj = _dataReader.DataReaderToObject<T>();
                        }
                    }
                    _sqlCommand.Transaction.Commit();
                }
                catch (Exception)
                {
                    if (_sqlCommand.Transaction != null)
                        _sqlCommand.Transaction.Rollback();
                }

                _connection.Close();
            }

            return obj;
        }

        public virtual void Update<T>(T obj, Expression<Func<T, bool>> conditional = null)
        {
            Dictionary<string, string> _fieldAndValue = obj.GetFieldNameAndValue(SQLCommand.Update);

            StringBuilder _textCommand = new StringBuilder();
            _textCommand.AppendLine(
                string.Format("UPDATE {0} SET {1} {2};",
                    obj.GetTableName(),
                    string.Join(", ", _fieldAndValue.Select(a => string.Format("{0} = {1}", a.Key, a.Value)).ToList()),
                    conditional != null ? conditional.ConvertExpression<T>() : ""
                    ));

            using (SqlConnection _connection = new SqlConnection(this.textConnection))
            {
                _connection.Open();

                SqlCommand _sqlCommand = new SqlCommand(_textCommand.ToString(), _connection, null);
                _sqlCommand.Transaction = _connection.BeginTransaction();
                _sqlCommand.CommandTimeout = 9999;
                try
                {
                    _sqlCommand.ExecuteNonQuery();
                    _sqlCommand.Transaction.Commit();
                }
                catch (Exception)
                {
                    if (_sqlCommand.Transaction != null)
                        _sqlCommand.Transaction.Rollback();
                }

                _connection.Close();
            }
        }

        public virtual void Update<T>(T obj)
        {
            Dictionary<string, string> _fieldAndValue = obj.GetFieldNameAndValue(SQLCommand.Update);

            StringBuilder _textCommand = new StringBuilder();
            _textCommand.AppendLine(
                string.Format("UPDATE {0} SET {1} WHERE {2};",
                    obj.GetTableName(),
                    string.Join(", ", _fieldAndValue.Select(a => string.Format("{0} = {1}", a.Key, a.Value)).ToList()),
                    obj.GetConditional()
                    ));

            using (var _connection = new SqlConnection(this.textConnection))
            {
                _connection.Open();

                SqlCommand _sqlCommand = new SqlCommand(_textCommand.ToString(), _connection, null);
                _sqlCommand.Transaction = _connection.BeginTransaction();

                try
                {
                    _sqlCommand.ExecuteNonQuery();
                    _sqlCommand.Transaction.Commit();
                }
                catch (Exception)
                {
                    if (_sqlCommand.Transaction != null)
                        _sqlCommand.Transaction.Rollback();
                }

                _connection.Close();
            }
        }

        public virtual void Delete<T>(Expression<Func<T, bool>> conditional = null)
        {
            List<string> _fields = HelperSQL.GetFieldName<T>();

            StringBuilder _textCommand = new StringBuilder();
            _textCommand.AppendLine(
                string.Format("DELETE FROM {0} {1};",
                    HelperSQL.GetTableName<T>(),
                    conditional != null ? conditional.ConvertExpression<T>() : ""
                    )
                );

            using (SqlConnection _connection = new SqlConnection(this.textConnection))
            {
                _connection.Open();

                SqlCommand _sqlCommand = new SqlCommand(_textCommand.ToString(), _connection, null);
                _sqlCommand.Transaction = _connection.BeginTransaction();

                try
                {
                    _sqlCommand.ExecuteNonQuery();
                    _sqlCommand.Transaction.Commit();
                }
                catch (Exception)
                {
                    if (_sqlCommand.Transaction != null)
                        _sqlCommand.Transaction.Rollback();
                }
                _connection.Close();
            }

        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                handle.Dispose();

            disposed = true;
        }
    }
}
