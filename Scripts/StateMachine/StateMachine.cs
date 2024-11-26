using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Selkie.Scripts.Common;
using UnityEngine;

namespace Selkie.Scripts.StateMachine
{
    public class Context : Dictionary<string, object>
    {
        public T Get<T>(string key)
        {
            if (TryGetValue(key, out var value))
            {
                return (T)value;
            }

            throw new ArgumentException($"unknown key:{key}, {this}");
        }

        public override string ToString()
        {
            return string.Join(",\r\n", this.Select(x => $"[{x.Key}]: {x.Value}"));
        }
    }

    public interface IBehavior
    {
        StateMachine Machine { set; }
        Context RegionContext { set; }
        void Enter();
        void UpdateBehavior(float deltaTime, float elapsedTime);
        void Exit();
    }

    public abstract class CoroutineBehavior : IBehavior
    {
        public StateMachine Machine { get; set; }
        public Context RegionContext { get; set; }

        private Coroutine _currentCoroutine;
        private bool _doneCoroutine;
        private readonly string _finishTrigger;

        protected abstract IEnumerator Coroutine();
        protected abstract void OnTerminate();

        public CoroutineBehavior(string finishTrigger)
        {
            _finishTrigger = finishTrigger;
        }
        
        public void Enter()
        {
            _currentCoroutine = GlobalCoroutine.StartTask(InnerCoroutine());
            IEnumerator InnerCoroutine()
            {
                yield return Coroutine();
                _doneCoroutine = true;
            }
        }

        public void UpdateBehavior(float deltaTime, float elapsedTime)
        {
            if (_doneCoroutine)
            {
                Machine.DoTrigger(_finishTrigger);
            }
        }

        public void Exit()
        {
            if (!_doneCoroutine)
            {
                GlobalCoroutine.StopTask(_currentCoroutine);
                OnTerminate();
            }
        }
    }
    
    public class State
    {
        public State(string name)
        {
            Name = name;
            BehaviorList = new();
        }

        public string Name { get; set; }

        private List<IBehavior> BehaviorList { get; }

        public State AddBehavior(IBehavior behavior)
        {
            BehaviorList.Add(behavior);
            return this;
        }

        internal virtual void Enter(Region parentRegion)
        {
            ElapsedTime = default;
            foreach (var behavior in BehaviorList)
            {
                behavior.Machine = Machine;
                behavior.RegionContext = parentRegion.Context;
                behavior.Enter();
            }
        }
        
        internal virtual void Update(float deltaTime)
        {
            foreach (var behavior in BehaviorList)
            {
                behavior.UpdateBehavior(deltaTime, ElapsedTime);
            }
            ElapsedTime += deltaTime;
        }

        private float ElapsedTime { get; set; }
        public StateMachine Machine { get; set; }

        public virtual bool DoTrigger(string triggerName)
        {
            return false;
        }

        internal virtual void Exit()
        {
            foreach (var behavior in BehaviorList)
            {
                behavior.Exit();
            }
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class CompositeState : State
    {
        public CompositeState(string name, string completedTrigger, Region[] childrenRegion) : base(name)
        {
            CompletedTrigger = completedTrigger;
            ChildrenRegion = childrenRegion;
        }

        private string CompletedTrigger { get; }
        public Region[] ChildrenRegion { get; }

        private Region GetMainRegion => ChildrenRegion.Length > 0 ? ChildrenRegion[0] : null;

        public override bool DoTrigger(string triggerName)
        {
            return ChildrenRegion.Aggregate(false, (current, region) => current | region.DoTrigger(triggerName));
        }
        
        internal override void Enter(Region parentRegion)
        {
            base.Enter(parentRegion);
            foreach (var region in ChildrenRegion)
            {
                region.Start();
            }
        }
        
        internal override void Update(float deltaTime)
        {
            if (GetMainRegion.CurrentState is FinalState)
            {
                if (Machine.DoTrigger(CompletedTrigger))
                {
                    return;
                }
            }

            base.Update(deltaTime);
            foreach (var region in ChildrenRegion)
            {
                region.Update(deltaTime);
            }
        }
        
        internal override void Exit()
        {
            base.Exit();
            foreach (var region in ChildrenRegion)
            {
                region.Stop();
            }
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class FinalState : State
    {
        public FinalState() : base("Final")
        {
        }
    }

    public record TransitionRule(string TriggerName, State FromState, State ToState, Func<bool> Guard = null, Action Effect = null);
    
    public sealed class Region
    {
        private readonly struct RuleKey : IEquatable<RuleKey>
        {
            private readonly string _triggerName;
            private readonly State _fromState;

            public RuleKey(string triggerName, State fromState)
            {
                _triggerName = triggerName;
                _fromState = fromState;
            }

            public bool Equals(RuleKey other)
            {
                return _triggerName == other._triggerName && _fromState == other._fromState;
            }

            public override bool Equals(object obj)
            {
                return obj is RuleKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_triggerName, _fromState);
            }
        }

        public Region(State initialState)
        {
            InitialState = CurrentState = initialState;
        }

        public Context Context { get; } = new();

        private record RuleValue(
            State ToState,
            Func<bool> Guard,
            Action Effect);

        private Dictionary<RuleKey, RuleValue> TransitionRules { get; } = new ();

        
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public State LastState { get; private set; }
        public State CurrentState { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool History { get; set; }
        private State InitialState { get; }

        public StateMachine Machine { get; set; }

        public Region AddTransitionRule(TransitionRule rule)
        {
            var key = new RuleKey(rule.TriggerName, rule.FromState);
            var value = new RuleValue(rule.ToState, rule.Guard, rule.Effect);
            TransitionRules.Add(key, value);
            return this;
        }

        private bool TransitionExitCheck { get; set; }

        public bool DoTrigger(string triggerName)
        {
            if (TransitionExitCheck)
            {
                throw new Exception("Exit中にTransitionを呼ぶのは禁止");
            }

            var key = new RuleKey(triggerName, CurrentState);
            if (TransitionRules.TryGetValue(key, out var value))
            {
                if (value.Guard?.Invoke() ?? true)
                {
                    TransitionExitCheck = true;
                    CurrentState.Exit();
                    TransitionExitCheck = false;

                    LastState = CurrentState;
                    CurrentState = value.ToState;
                    CurrentState.Machine = Machine;

                    value.Effect?.Invoke();
                    CurrentState.Enter(this);

                    return true;
                }
            }

            return CurrentState.DoTrigger(triggerName);
        }

        public void Start()
        {
            if (!History)
            {
                Context.Clear();
                CurrentState = InitialState;
            }

            CurrentState.Machine = Machine;
            CurrentState.Enter(this);
        }

        public void Update(float deltaTime)
        {
            CurrentState.Update(deltaTime);
        }

        public void Stop()
        {
            CurrentState.Exit();
        }

        public bool IsCurrentState(string name)
        {
            if (CurrentState.Name == name)
            {
                return true;
            }

            if (CurrentState is CompositeState compositeState)
            {
                foreach (var region in compositeState.ChildrenRegion)
                {
                    if (region.IsCurrentState(name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsCurrentState(State state)
        {
            if (CurrentState == state)
            {
                return true;
            }

            if (CurrentState is CompositeState compositeState)
            {
                foreach (var region in compositeState.ChildrenRegion)
                {
                    if (region.IsCurrentState(state))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public interface IStateMachine
    {
        void Update(float deltaTime);
        bool DoTrigger(string triggerName);
    }

    public class StateMachine : IStateMachine
    {
        public Context Context { get; } = new();
        private Region Region { get; set; }

        public void SetRegion(Region region)
        {
            Region = region;
            Region.Machine = this;
            Region.Start();
        }

        public void Update(float deltaTime)
        {
            Region.Update(deltaTime);
        }

        public bool DoTrigger(string triggerName)
        {
            return Region.DoTrigger(triggerName);
        }

        public bool HasStates(string name)
        {
            return Region.IsCurrentState(name);
        }

        public bool HasStates(State states)
        {
            return Region.IsCurrentState(states);
        }
    }
}
