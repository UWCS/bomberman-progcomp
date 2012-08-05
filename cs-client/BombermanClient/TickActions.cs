using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BombermanClient
{
    class TickActions
    {
        Dictionary<Player, Action> actionDict;

        public TickActions()
        {
            actionDict = new Dictionary<Player, Action>();
        }

        public void AddAction(Player player, Action action)
        {
            if (actionDict.ContainsKey(player))
            {
                actionDict[player] = action;
            }
            else
            {
                actionDict.Add(player, action);
            }
        }

        public Dictionary<Player, Action> ActionDict
        {
            get
            {
                return actionDict;
            }
        }
    }
}
