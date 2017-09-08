using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPath
{
    public interface IQPathUnit
    {
        float CostToEnterTile(IQPathTile sourceTile, IQPathTile destinationTile);
        float AggregateTurnsToEnterTile(Tile tile, float turnsToDate);
    }
}
