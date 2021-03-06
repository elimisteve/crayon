﻿using Common;
using System;

namespace Crayon
{
    internal class ShowPerformanceMetricsWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
#if DEBUG
            string summary = PerformanceTimer.GetSummary();
            Console.WriteLine(summary);
#endif
            return new CrayonWorkerResult();
        }
    }
}
