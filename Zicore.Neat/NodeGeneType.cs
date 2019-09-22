using System;

namespace Zicore.Neat
{
    [Flags]
    public enum NodeGeneType
    {
        Sensor = 1,
        Output = 2,
        Hidden = 4,
    }
}