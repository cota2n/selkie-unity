using System.Collections.Generic;
using Selkie.Scripts.Common;
using UnityEngine;

namespace Selkie.Scripts.StateMachine
{
    public class StateMachineWorker : SingletonObject<StateMachineWorker>
    {
        private HashSet<IStateMachine> Machines { get; } = new();

        private HashSet<IStateMachine> AddMachines { get; } = new();
        private HashSet<IStateMachine> RemoveMachines { get; } = new();

        public static void Add(IStateMachine stateMachineUpdate)
        {
            Instance.AddMachines.Add(stateMachineUpdate);
        }
        
        public static void Remove(IStateMachine stateMachineUpdate)
        {
            Instance.RemoveMachines.Add(stateMachineUpdate);
        }
        
        void Update()
        {
            if (AddMachines.Count > 0)
            {
                Machines.UnionWith(AddMachines);
                AddMachines.Clear();
            }
            if (RemoveMachines.Count > 0)
            {
                Machines.ExceptWith(RemoveMachines);
                RemoveMachines.Clear();
            }

            foreach (var machine in Machines)
            {
                machine.Update(Time.deltaTime);
            }
        }
    }
}
