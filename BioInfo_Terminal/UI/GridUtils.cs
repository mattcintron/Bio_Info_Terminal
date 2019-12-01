using System.Windows;
using System.Windows.Controls;

namespace BioInfo_Terminal.UI
{
    public class GridUtils
    {
        /// <summary>
        ///     Identified the RowDefinitions attached property
        /// </summary>
        public static readonly DependencyProperty RowDefinitionsProperty =
            DependencyProperty.RegisterAttached("RowDefinitions", typeof(string), typeof(GridUtils),
                new PropertyMetadata("", OnRowDefinitionsPropertyChanged));

        /// <summary>
        ///     Parses a string to create a GridLength
        /// </summary>
        private static GridLength ParseLength(string length)
        {
            length = length.Trim();
            if (length.ToLowerInvariant().Equals("auto")) return new GridLength(0, GridUnitType.Auto);
            if (length.Contains("*"))
            {
                length = length.Replace("*", "");
                if (string.IsNullOrEmpty(length)) length = "1";
                return new GridLength(double.Parse(length), GridUnitType.Star);
            }

            return new GridLength(double.Parse(length), GridUnitType.Pixel);
        }

        /// <summary>
        ///     Gets the value of the RowDefinitions property
        /// </summary>
        public static string GetRowDefinitions(DependencyObject d)
        {
            return (string) d.GetValue(RowDefinitionsProperty);
        }

        /// <summary>
        ///     Sets the value of the RowDefinitions property
        /// </summary>
        public static void SetRowDefinitions(DependencyObject d, string value)
        {
            d.SetValue(RowDefinitionsProperty, value);
        }

        /// <summary>
        ///     Handles property changed event for the RowDefinitions property, constructing
        ///     the required RowDefinitions elements on the grid which this property is attached to.
        /// </summary>
        private static void OnRowDefinitionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // construct the required row definitions
            if (d is Grid targetGrid)
            {
                targetGrid.RowDefinitions.Clear();
                if (e.NewValue is string rowDefs)
                {
                    var rowDefArray = rowDefs.Split(',');
                    foreach (var rowDefinition in rowDefArray)
                        if (rowDefinition.Trim() == "")
                            targetGrid.RowDefinitions.Add(new RowDefinition());
                        else
                            targetGrid.RowDefinitions.Add(new RowDefinition
                            {
                                Height = ParseLength(rowDefinition)
                            });
                }
            }
        }
    }
}