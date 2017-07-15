using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace spex
{
    public class ValidWindow
    {
        static public bool IsValid(DependencyObject node)
        {
            if (node != null)
            {
                if (Validation.GetHasError(node))
                {
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }
            foreach (object subnode in LogicalTreeHelper.GetChildren(node))
            {
                if (subnode is DependencyObject)
                {
                    if (!IsValid((DependencyObject)subnode))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, System.Globalization.CultureInfo culture)
        {
            return ((double)value).ToString();
        }

        public object ConvertBack(object value, Type typeTarget, object param, System.Globalization.CultureInfo culture)
        {
            return double.Parse((string)value);
        }
    }

    public class DoubleValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            double input;
            if (!double.TryParse((string)value, out input))
            {
                return new ValidationResult(false, "Not a number");
            }

            return new ValidationResult(true, null);
        }
    }

    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type typeTarget, object param, System.Globalization.CultureInfo culture)
        {
            return ((int)value).ToString();
        }

        public object ConvertBack(object value, Type typeTarget, object param, System.Globalization.CultureInfo culture)
        {
            return int.Parse((string)value);
        }
    }

    public class IntValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int input;
            if (!int.TryParse((string)value, out input))
            {
                return new ValidationResult(false, "Not a integer");
            }

            return new ValidationResult(true, null);
        }
    }

    public class PositiveValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (double.Parse((string)value) > 0)
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, "Must be positive");
        }
    }
}
