﻿using InvestmentManager.Entities.Market;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IPriceNP
    {
        IEnumerable<Price> Prices { get; set; }
    }
}
