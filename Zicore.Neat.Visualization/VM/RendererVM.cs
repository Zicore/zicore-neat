using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zicore.Neat.Base;

namespace Zicore.Neat.Visualization.VM
{
    public class RendererVM : ViewModelBase
    {
        private IGenome genome;
        public IGenome Genome
        {
            get => genome;
            set
            {
                if (Equals(value, genome)) return;
                genome = value;
                OnPropertyChanged();
            }
        }

        public void UpdateGenome()
        {
            OnPropertyChanged(nameof(Genome));
        }
    }
}
