using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using ZXMAK2.Presentation.Attributes;


namespace ZXMAK2.Presentation
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly IEnumerable<PropertyInfo> _propInfos;
        private readonly Dictionary<string, string[]> _propGraph;

        public ViewModelBase()
        {
            _propInfos = BuildPropertyInfos(GetType());
            _propGraph = BuildDependencyGraph(GetType());
        }


        #region IDataErrorInfo

        private string _error;

        public string Error
        {
            get { return _error; }
            set { PropertyChangeRef("Error", ref _error, value); }
        }

        public string this[string columnName]
        {
            get { return OnPropertyValidate(columnName); }
        }

        protected virtual string OnPropertyValidate(string propName)
        {
            return null;
        }

        #endregion IDataErrorInfo


        public bool HasError()
        {
            return _propInfos.Any(pi => !string.IsNullOrEmpty(this[pi.Name]));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            OnPropertyVerify(propName);
            OnPropertyChangedRecursive(propName, new HashSet<string>());
        }

        [Conditional("DEBUG")]
        private void OnPropertyVerify(string propName)
        {
            if (GetType().GetProperty(propName) == null)
            {
                throw new ArgumentException(
                    string.Format("Invalid property name: {0}",
                        propName));
            }
        }

        private void OnPropertyChangedRecursive(string propName, HashSet<string> notifiedSet)
        {
            if (notifiedSet.Contains(propName))
            {
                return;
            }

            var handler = PropertyChanged;
            if (handler != null)
            {
                var arg = new PropertyChangedEventArgs(propName);
                handler(this, arg);
            }
            notifiedSet.Add(propName);

            string[] dependencyPropNames;
            if (_propGraph.TryGetValue(propName, out dependencyPropNames))
            {
                dependencyPropNames
                    .ToList()
                    .ForEach(name => OnPropertyChangedRecursive(name, notifiedSet));
            }
        }

        protected bool PropertyChangeRef<T>(string propName, ref T fieldRef, T value) where T : class
        {
            if (fieldRef == value)
            {
                return false;
            }
            fieldRef = value;
            OnPropertyChanged(propName);
            return true;
        }

        protected bool PropertyChangeVal<T>(string propName, ref T fieldRef, T value) where T : struct
        {
            if (fieldRef.Equals(value))
            {
                return false;
            }
            fieldRef = value;
            OnPropertyChanged(propName);
            return true;
        }

        protected bool PropertyChangeNul<T>(string propName, ref Nullable<T> fieldRef, Nullable<T> value) where T : struct
        {
            if ((!fieldRef.HasValue && !value.HasValue) ||
                (fieldRef.HasValue && value.HasValue && fieldRef.Value.Equals(value.Value)))
            {
                return false;
            }
            fieldRef = value;
            OnPropertyChanged(propName);
            return true;
        }


        #region Static

        private static readonly Dictionary<Type, IEnumerable<PropertyInfo>> _propInfosByType = new Dictionary<Type, IEnumerable<PropertyInfo>>();
        private static readonly Dictionary<Type, Dictionary<string, string[]>> _dependencyGraphByType = new Dictionary<Type, Dictionary<string, string[]>>();

        private static IEnumerable<PropertyInfo> BuildPropertyInfos(Type type)
        {
            if (!_propInfosByType.ContainsKey(type))
            {
                _propInfosByType[type] = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(pi => pi.Name != "Item");
            }
            return _propInfosByType[type];
        }

        private static Dictionary<string, string[]> BuildDependencyGraph(Type type)
        {
            if (!_dependencyGraphByType.ContainsKey(type))
            {
                var propInfos = BuildPropertyInfos(type);
                var graph =
                (
                        from pi in propInfos
                        from propName in
                            from attribute in pi.GetCustomAttributes(typeof(DependsOnPropertyAttribute), true).Cast<DependsOnPropertyAttribute>()
                            select attribute.PropertyName
                        group pi.Name by propName
                            into relation
                            select new
                            {
                                Parent = relation.Key,
                                Childs = relation
                            }
                );
                _dependencyGraphByType[type] = graph.ToDictionary(
                    pair => pair.Parent,
                    pair => pair.Childs.ToArray());
            }
            return _dependencyGraphByType[type];
        }

        #endregion Static
    }
}
