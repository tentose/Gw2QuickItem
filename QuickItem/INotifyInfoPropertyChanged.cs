using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public interface INotifyInfoPropertyChanged
    {
        event EventHandler<string> PropertyChanged;

        bool PauseChangedNotifications { get; set; }
    }
}
