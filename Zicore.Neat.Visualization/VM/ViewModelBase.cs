using System.ComponentModel;
using System.Runtime.CompilerServices;
using Zicore.Neat.Visualization.Properties;

namespace Zicore.Neat.Visualization.VM
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}