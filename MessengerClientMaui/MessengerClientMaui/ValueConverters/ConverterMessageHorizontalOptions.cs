using MessengerClientMaui.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerClientMaui.ValueConverters
{
    public class ConverterMessageHorizontalOptions : IValueConverter
    {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if ((int?)value == Application.Current.Handler.MauiContext.Services.GetService<Client>().ID)
                return LayoutOptions.End;
            if ((int?)value < 0)
                return LayoutOptions.Center;
            return LayoutOptions.Start;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
