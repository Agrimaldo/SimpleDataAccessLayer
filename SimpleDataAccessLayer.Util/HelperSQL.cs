using SimpleDataAccessLayer.Util.Attributes;
using SimpleDataAccessLayer.Util.Enumerators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleDataAccessLayer.Util
{
    public static class HelperSQL
    {
        public static Dictionary<string, string> GetFieldNameAndValue(this object obj, SQLCommand command)
        {
            Dictionary<string, string> _dictionary = new Dictionary<string, string>();

            if (obj != null)
            {
                var _properties = obj.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                    .Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any()).Select(a => new { Property = a, Inherited = false }).ToList();

                _properties.AddRange(obj.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                    .Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any() && !_properties.Where(b => b.Property.Name.Equals(a.Name)).Any())
                    .Select(a => new { Property = a, Inherited = true }).ToList());

                _properties.ForEach(a =>
                {
                    bool _addField = true;
                    bool _isPrimaryKey = ((TableColumnAttribute)a.Property.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).PrimaryKey;


                    if ((_isPrimaryKey && !a.Inherited) || (!_isPrimaryKey && a.Inherited))
                        _addField = false;

                    if (command == SQLCommand.Update && !((TableColumnAttribute)a.Property.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Update)
                        _addField = false;

                    if (_addField)
                    {
                        _dictionary.Add(
                            ((TableColumnAttribute)a.Property.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name,
                            string.Format("'{0}'", (a.Property.PropertyType == typeof(DateTime) ? ((DateTime)a.Property.GetValue(obj, null)).ToString("yyyy-MM-dd HH:mm") : a.Property.GetValue(obj, null) == null ? string.Empty : a.Property.GetValue(obj, null).ToString()))
                            );
                    }
                });
            }


            return _dictionary;
        }

        public static Dictionary<string, string> GetFieldNameAndValue<T>(this object obj, SQLCommand command)
        {
            Dictionary<string, string> _dictionary = new Dictionary<string, string>();

            if (obj != null)
            {
                
                var _properties = typeof(T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                    .Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any()).Select(a => new { Property = a, Inherited = false }).ToList();

                _properties.AddRange(typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                    .Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any() && !_properties.Where(b => b.Property.Name.Equals(a.Name)).Any())
                    .Select(a => new { Property = a, Inherited = true }).ToList());

                _properties.ForEach(a =>
                {
                    bool _addField = true;
                    bool _isPrimaryKey = ((TableColumnAttribute)a.Property.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).PrimaryKey;


                    if ((_isPrimaryKey && !a.Inherited) || (!_isPrimaryKey && a.Inherited))
                        _addField = false;

                    if (command == SQLCommand.Update && !((TableColumnAttribute)a.Property.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Update)
                        _addField = false;

                    if (_addField)
                    {
                        string _fieldName = _isPrimaryKey && a.Inherited ? obj.GetTableAttribute().ForeignKey : ((TableColumnAttribute)a.Property.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name;

                        _dictionary.Add(
                            _fieldName,
                            string.Format("'{0}'", (a.Property.PropertyType == typeof(DateTime) ? ((DateTime)a.Property.GetValue(obj, null)).ToString("yyyy-MM-dd HH:mm") : a.Property.GetValue(obj, null) == null ? string.Empty : a.Property.GetValue(obj, null).ToString()))
                            );
                    }
                });
            }


            return _dictionary;
        }

        public static string GetPrimaryKeyName<T>()
        {
            string _return = string.Empty;

            typeof(T).GetProperties().Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any()).ToList().ForEach(a =>
            {
                if (((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).PrimaryKey)
                {
                    _return = ((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name;
                }
            });

            return _return;
        }

        public static string GetPrimaryKeyValue(this object obj)
        {
            string _return = string.Empty;

            obj.GetType().GetProperties().Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any()).ToList().ForEach(a =>
            {
                if (((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).PrimaryKey)
                {
                    _return = string.Format("{0} = '{1}'",
                        ((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name,
                        a.GetValue(obj, null).ToString()
                        );
                }
            });

            return _return;
        }

        public static string GetConditional(this object obj)
        {
            StringBuilder _return = new StringBuilder();

            obj.GetType().GetProperties().Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any()).ToList().ForEach(a =>
            {
                if (((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).PrimaryKey ||
                ((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Update == false)
                {
                    if (a.GetValue(obj, null) != null)
                    {
                        if (_return.Length > 0)
                            _return.AppendLine(" AND ");

                        _return.AppendLine(string.Format("{0} = '{1}'",
                            ((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name,
                            a.PropertyType == typeof(DateTime) ? ((DateTime)a.GetValue(obj, null)).ToString("yyyy-MM-dd HH:mm:ss") : a.GetValue(obj, null).ToString()
                            ));
                    }
                }
            });

            return _return.ToString();
        }

        public static List<string> GetFieldName<T>(Dictionary<string, string> dictionaryField = null)
        {
            List<string> _list = new List<string>();

            typeof(T).GetProperties().Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any()).ToList().ForEach(a =>
            {
                bool _add = true;
                string item = string.Empty;
                if (dictionaryField != null)
                {
                    if (!dictionaryField.Where(b => b.Key.ToUpper().Equals(a.Name.ToUpper())).Any())
                        _add = false;
                    else
                    {
                        if (a.PropertyType == typeof(string))
                        {
                            item = string.Format("{0} LIKE '%{1}%'",
                                ((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name,
                                dictionaryField.Where(b => b.Key.ToUpper().Equals(a.Name.ToUpper())).Select(b => b.Value).FirstOrDefault());

                        }
                        else
                        {
                            item = string.Format("{0} = '{1}'",
                                ((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name,
                                dictionaryField.Where(b => b.Key.ToUpper().Equals(a.Name.ToUpper())).Select(b => b.Value).FirstOrDefault());

                        }
                    }
                }
                else
                    item = ((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name;

                if (_add)
                    _list.Add(item);
            });

            return _list;
        }

        public static List<string> GetFieldName<T>(bool primaryKey)
        {
            List<string> _list = new List<string>();

            typeof(T).GetProperties().Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any()).ToList().ForEach(a =>
            {
                bool _add = true;
                string item = string.Empty;
                if (!((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).PrimaryKey)
                    _add = false;
                else
                    item = ((TableColumnAttribute)a.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name;

                if (_add)
                    _list.Add(item);
            });

            return _list;
        }

        public static string GetTableName(this object obj)
        {
            return ((TableAttribute)obj.GetType().GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault()).Name;
        }

        public static string GetTableName<T>()
        {
            return ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault()).Name;
        }

        public static TableAttribute GetTableAttribute(this object obj)
        {
            return ((TableAttribute)obj.GetType().GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault());
        }

        public static T DataReaderToObject<T>(this IDataReader dataReader)
        {
            T _object = (T)Activator.CreateInstance(typeof(T));

            typeof(T).GetProperties().Where(a => a.GetCustomAttributes(typeof(TableColumnAttribute), false).Any()).ToList().ForEach(p =>
            {
                string _fieldName = ((TableColumnAttribute)p.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name;
                if (dataReader.ColumnExist(_fieldName) && !dataReader[_fieldName].ToString().Trim().Equals(""))
                {
                    if (p.PropertyType == typeof(System.Guid))
                        p.SetValue(_object, new Guid(dataReader[_fieldName].ToString()), null);
                    else
                        p.SetValue(_object, Convert.ChangeType(dataReader[_fieldName] != DBNull.Value ? dataReader[_fieldName].ToString() : string.Empty, p.PropertyType), null);
                }
            });

            return _object;
        }

        private static bool ColumnExist(this IDataReader dataReader, string columnName)
        {
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (dataReader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public static bool CheckForeignKey<T>()
        {
            return typeof(T).GetProperties().Where(a => a.GetCustomAttributes(typeof(ForeignKeyAttribute), false).Any()).Any();
        }

        #region CLAUSE WHERE
        public static string ConvertExpression<T>(this Expression<Func<T, bool>> expression)
        {
            return string.Format("WHERE {0}", RecursiveConverter(expression.Body, true).Replace("''", "'"));
        }

        private static string RecursiveConverter(Expression expression, bool unary = false, bool quote = true)
        {
            if (expression is UnaryExpression _unary)
            {
                string _right = RecursiveConverter(_unary.Operand, true);
                return string.Format("({0} {1})", NodeTypeText(_unary.NodeType, _right == "NULL"), _right);
            }

            if (expression is BinaryExpression _binaryExpression)
            {
                string _right = string.Empty;
                _right = RecursiveConverter(_binaryExpression.Right);
                return string.Format("({0} {1} {2})", RecursiveConverter(_binaryExpression.Left), NodeTypeText(_binaryExpression.NodeType, _right == "NULL"), _right);
            }

            if (expression is ConstantExpression _constantExpression)
                return ValueToText(_constantExpression.Value, unary, quote);

            if (expression is MemberExpression _memberExpression)
            {
                if (_memberExpression.Member is PropertyInfo _propertyInfo)
                {
                    string _column = string.Empty;

                    if (_propertyInfo.GetCustomAttributes(typeof(TableColumnAttribute), false).Any() && quote)
                        _column = ((TableColumnAttribute)_propertyInfo.GetCustomAttributes(typeof(TableColumnAttribute), false).FirstOrDefault()).Name;
                    else
                        return string.Format("'{0}'", ValueToText(GetMemberValue(_memberExpression), unary, quote));

                    if (unary && _memberExpression.Type == typeof(bool))
                        return string.Format("({0} = 1)", _column);

                    return string.Format("{0}", _column);
                }

                if (_memberExpression.Member is FieldInfo)
                    return string.Format("{0}", ValueToText(GetMemberValue(_memberExpression), unary, quote));
            }

            if (expression is MethodCallExpression _methodCallExpression)
            {
                if (_methodCallExpression.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                    return string.Format("({0} LIKE '%{1}%')", RecursiveConverter(_methodCallExpression.Object), RecursiveConverter(_methodCallExpression.Arguments[0], quote: false));
                if (_methodCallExpression.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                    return string.Format("({0} LIKE '{1}%')", RecursiveConverter(_methodCallExpression.Object), RecursiveConverter(_methodCallExpression.Arguments[0], quote: false));

                if (_methodCallExpression.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                    return string.Format("({0} LIKE '%{1}')", RecursiveConverter(_methodCallExpression.Object), RecursiveConverter(_methodCallExpression.Arguments[0], quote: false));

                if (_methodCallExpression.Method.Name.Equals("Equals"))
                    return string.Format("({0} = '{1}')", RecursiveConverter(_methodCallExpression.Object), RecursiveConverter(_methodCallExpression.Arguments[0], quote: false));

                if (_methodCallExpression.Method.Name.Equals("Contains"))
                {
                    Expression _ExpressionCollection = null;
                    Expression _ExpressionProperty = null;

                    if (_methodCallExpression.Method.IsDefined(typeof(ExtensionAttribute)) && _methodCallExpression.Arguments.Count == 2)
                    {
                        _ExpressionCollection = _methodCallExpression.Arguments[0];
                        _ExpressionProperty = _methodCallExpression.Arguments[1];
                    }
                    else if (!_methodCallExpression.Method.IsDefined(typeof(ExtensionAttribute)) && _methodCallExpression.Arguments.Count == 1)
                    {
                        _ExpressionCollection = _methodCallExpression.Object;
                        _ExpressionProperty = _methodCallExpression.Arguments[0];
                    }

                    IEnumerable _values = ((IEnumerable)GetMemberValue(_ExpressionCollection));
                    string _concatenation = "";
                    foreach (var valor in _values)
                    {
                        _concatenation += ValueToText(valor, false, true) + ", ";
                    }

                    if (_concatenation == "")
                        return ValueToText(false, true, false);

                    return string.Format("({0} IN ({1}))", RecursiveConverter(_ExpressionProperty), _concatenation);

                }
            }
            return string.Empty;
        }

        private static object NodeTypeText(ExpressionType nodeType, bool rightNull)
        {
            switch (nodeType)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return rightNull ? "IS" : "=";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Not:
                    return "NOT";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Subtract:
                    return "-";
                default:
                    return string.Empty;

            }
        }

        public static string ValueToText(object value, bool isUnary, bool quote)
        {
            if (value is bool)
            {
                if (isUnary)
                {
                    return (bool)value ? "(1=1)" : "(1=0)";
                }
                return (bool)value ? "1" : "0";
            }
            else if (value is DateTime)
                return string.Format("'{0}'", ((DateTime)value).ToString("yyyy-MM-dd HH:mm"));

            return value.ToString();
        }

        private static object GetMemberValue(Expression member)
        {
            var _objMember = Expression.Convert(member, typeof(object));
            var _getterLambda = Expression.Lambda<Func<object>>(_objMember);
            var _getter = _getterLambda.Compile();

            return _getter();
        }
        #endregion
    }
}
