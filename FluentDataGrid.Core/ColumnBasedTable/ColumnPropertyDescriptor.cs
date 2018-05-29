using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentDataGrid.Core.ColumnBasedTable
{
    public class ColumnPropertyDescriptor : PropertyDescriptor
    {
        bool isReadOnly;

        public Type Type { get; private set; }
        public int ColumnIndex { get; internal set; }
        public object DefaultValue { get; private set; }

        public ColumnPropertyDescriptor(string name, Type type, bool isReadOnly = false, object defaultValue = null)
            : base(name, null)
        {
            Type = type;
            this.isReadOnly = isReadOnly;
            DefaultValue = defaultValue;
        }

        #region PropertyDescriptor

        public override Type ComponentType { get { return typeof(Row); } }

        public override bool IsReadOnly { get { return isReadOnly; } }

        public override Type PropertyType { get { return Type; } }

        public override object GetValue(object component)
        {
            return ((Row)component)[ColumnIndex];
        }

        public override void SetValue(object component, object value)
        {
            ((Row)component)[ColumnIndex] = value;
            //((Row)component)[ColumnIndex] = Convert.ChangeType(value, type);
            //((Row)component).OnPropertyChanged();
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {

        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        #endregion
    }
}
