﻿using InvestManager.Entities.Market;

using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IReportNP
    {
        IEnumerable<Report> Reports { get; set; }
    }
}
