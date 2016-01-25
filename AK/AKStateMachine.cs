using System;
using System.Collections.Generic;

// see reference https://github.com/nblumhardt/stateless
public class AKStateMachine<TState, TTrigger>
{
    public interface IStateConfiguration
    {
        IStateConfiguration OnEnter(Action action);

        IStateConfiguration OnEnter<T>(Action<T> action);

        IStateConfiguration OnTrigger(TTrigger trigger, Action action);

        IStateConfiguration OnTrigger<T>(TTrigger trigger, Action<T> action);


//        IStateConfiguration OnEnterFrom<T>(TTrigger trigger, Action<T> action);

//        IStateConfiguration OnEnterFrom(TTrigger trigger, Action action);

//        IStateConfiguration OnEnterFrom(TTrigger [] triggers, Action action);

        IStateConfiguration OnExit(Action action);

        IStateConfiguration Permit(TTrigger trigger, TState state);

        IStateConfiguration SubstateOf(TState state);

        IStateConfiguration PermitIf(TTrigger trigger, TState state, Func<bool> condition);

        IStateConfiguration PermitReentry(TTrigger trigger);

        IStateConfiguration Ignore(TTrigger trigger);
    }

    private class StateConfiguration : IStateConfiguration
    {
        public readonly TState state;
        public bool hasSuperState = false;
        public TState superState;
        public readonly List<Action<TTrigger, object[]>> onEnter = new List<Action<TTrigger, object[]>>();
        public readonly List<Action<TTrigger, object[]>> onTrigger = new List<Action<TTrigger, object[]>>();
        public readonly List<Action> onExit = new List<Action>();
        public readonly Dictionary<TTrigger, TState> nextStates = new Dictionary<TTrigger, TState>();
        public readonly List<TTrigger> reentryTriggers = new List<TTrigger>();
        public readonly List<TTrigger> ignoreTriggers = new List<TTrigger>();

        public StateConfiguration(TState state)
        {
            this.state = state;
        }

        public IStateConfiguration OnEnter(Action action)
        {
            onEnter.Add((t, args) => action());
            return this;
        }

        public IStateConfiguration OnEnter<T>(Action<T> action)
        {
            onEnter.Add((t, args) =>
            {
                if (args.Length == 1 && args[0] is T)
                    action((T)args[0]);
            });
            return this;
        }

        public IStateConfiguration OnTrigger(TTrigger trigger, Action action)
        {
            onTrigger.Add((t, args) => {
                if (t.Equals(trigger))
                    action();
            });
            return this;
        }

        public IStateConfiguration OnTrigger<T>(TTrigger trigger, Action<T> action)
        {
            onTrigger.Add((t, args) => {
                if (t.Equals(trigger) && args.Length == 1 && args[0] is T)
                    action((T)args[0]);
            });
            return this;
        }


//        public IStateConfiguration OnEnterFrom(TTrigger trigger, Action action)
//        {
//            onEnter.Add((t, args) =>
//            {
//                if (t.Equals(trigger))
//                    action();
//            });
//            return this;
//        }
//
//        public IStateConfiguration OnEnterFrom(TTrigger [] triggers, Action action)
//        {
//            foreach (var trigger in triggers)
//                OnEnterFrom(trigger, action);
//            return this;
//        }
//
//        public IStateConfiguration OnEnterFrom<T>(TTrigger trigger, Action<T> action)
//        {
//            onEnter.Add((t, args) =>
//            {
//                if (t.Equals(trigger) && args.Length == 1 && args[0] is T)
//                    action((T)args[0]);
//            });
//            return this;
//        }
            
        public IStateConfiguration OnExit(Action action)
        {
            onExit.Add(action);
            return this;
        }

        public IStateConfiguration Permit(TTrigger trigger, TState state)
        {
            nextStates[trigger] = state;
            return this;
        }

        public IStateConfiguration SubstateOf(TState state)
        {
            superState = state;
            hasSuperState = true;
            return this;
        }

        public IStateConfiguration PermitIf(TTrigger trigger, TState state, Func<bool> condition)
        {
            throw new NotImplementedException();
        }

        public IStateConfiguration PermitReentry(TTrigger trigger)
        {
            reentryTriggers.Add(trigger);
            return this;
        }

        public IStateConfiguration Ignore(TTrigger trigger)
        {
            ignoreTriggers.Add(trigger);
            return this;
//            throw new NotImplementedException();
        }


        public void Enter(TTrigger trigger, object [] parameters)
        {
            foreach (var cmd in onEnter)
                cmd(trigger, parameters);
        }

        public void Trigger(TTrigger trigger, object [] parameters)
        {
            foreach (var cmd in onTrigger)
                cmd(trigger, parameters);
        }

        public void Exit()
        {
            foreach (var cmd in onExit)
                cmd();
        }

//        public List<TState> GetSuperStates(Dictionary<TState, StateConfiguration> states)
//        {
//            var ret = new List<TState>();
//            var s = this;
//            while (s.hasSuperState) {
//                ret.Add(s.superState);
//                s = states[s.superState];
//            }
//            return ret;
//        }

        public List<StateConfiguration> GetSelfAndSuperConfigs(Dictionary<TState, StateConfiguration> states)
        {
            var ret = new List<StateConfiguration>();
            var config = this;
            while (true) {
                ret.Add(config);
                if (!config.hasSuperState)
                    break;
                config = states[config.superState];
            }
            return ret;
        }

    }

    readonly Dictionary<TState, StateConfiguration> states = new Dictionary<TState, StateConfiguration>();

//    readonly Stack<TState> stack = new Stack<TState>();
//    public TState State { get { return stack.Peek(); } }
    public TState State { get; private set; }
    // TODO: use stack of states instead single state, for correct support substates

    public AKStateMachine(TState initialState)
    {
//        stack.Push(initialState);
        State = initialState;
    }

    public IStateConfiguration Configure(TState state)
    {
        if (!states.ContainsKey(state))
            states[state] = new StateConfiguration(state);
        return states[state];
    }

//    bool IsInState(StateConfiguration current, TState state)
//    {
//        if (current.state.Equals(state))
//            return true;
//        if (!current.hasSuperState)
//            return false;
//        return IsInState(states[current.superState], state);
//    }
//
//
//    bool IsThisOrSuperStateOf(TState current, TState other)
//    {
//        if (current.Equals(other))
//            return true;
//        var config = states[current];
//        if (!config.hasSuperState)
//            return false;
//        return IsThisOrSuperStateOf(config.superState, other);
//    }

    bool _fireLocked = false;
//    Tuple<TTrigger, object[]> _fireRefire = null;
    public void Fire(TTrigger trigger, params object[] parameters)
    {
//        if (_fireLocked)
//        {
//            if (_fireRefire == null)
//                _fireRefire = Tuple.Create(trigger, parameters);
//            return;
//        }
        try
        {
            DDDebug.Assert(!_fireLocked, "Fire() already run");
            DDDebug.Trace("current:" + State, "trigger:" + trigger, "params:" + parameters.DDJoinToString(", "));
            _fireLocked = true;

            //--------------------
//            var currentState = State;
//            var config1 = states[currentState];
//            if (!config1.nextStates.ContainsKey(trigger))
//            {
//                DDDebug.Trace("AKStateMachine: " + State + " >>" + trigger + ">> (not found)");
//                return;
//            }

            var currentState = State;
            var currentConfig = states[currentState];

            currentConfig.Trigger(trigger, parameters);

            if (currentConfig.ignoreTriggers.Contains(trigger))
                return;

            if (currentConfig.reentryTriggers.Contains(trigger)){
                currentConfig.Exit();
                currentConfig.Enter(trigger, parameters);
                return;
            }

            var nextState = currentConfig.nextStates[trigger];
            var nextConfig = states[nextState];


            var currentCC = currentConfig.GetSelfAndSuperConfigs(states);
            var nextCC = nextConfig.GetSelfAndSuperConfigs(states);



//            if (currentState.Equals(nextState))
//            {
//                currentConfig.Enter(trigger, parameters);
//                return;
//            }

            for (int i = 0; i < currentCC.Count; i++)
            {
                if (!nextCC.Contains(currentCC[i]))
                    currentCC[i].Exit();
            }

            for (int i = nextCC.Count - 1; i >= 0; i--)
            {
                if (!currentCC.Contains(nextCC[i]))
                    nextCC[i].Enter(trigger, parameters);
            }

            State = nextState;

            DDDebug.Trace("next:" + State);

//            while (!IsThisOrSuperStateOf(nextState, currentState))
//            while (nextConfig.GetSuperStates(states).Contains(cur))
//            {
//                currentConfig.Exit(trigger);
//                stack.Pop();
//                currentState = State;
//                currentConfig = states[currentState];
//            }

            //--------------------

//            if (!IsInState(config2, config1.state))
//                foreach (var cmd in config1.onExit)
//                    cmd(trigger, null);

//            State = nextState;

//            if (config2.onEnter != null && !IsInState(config1, config2.state))
//                foreach (var cmd in config2.onEnter)
//                    cmd(trigger, parameters);

        //--------------------
        }
        finally
        {
            _fireLocked = false;
//
//            var ff = _fireRefire;
//            _fireRefire = null;
//            if (ff != null)
//                Fire(ff.Item1, ff.Item2);
        }
    }
}

