using System;
using System.Collections.Generic;

// see reference https://github.com/nblumhardt/stateless
public class AKStateMachine<TState, TTrigger>
{
    public interface IStateConfiguration
    {
        IStateConfiguration OnEnter(Action action);

        IStateConfiguration OnEnter<T>(Action<T> action);

        IStateConfiguration OnEnterFrom<T>(TTrigger trigger, Action<T> action);

        IStateConfiguration OnEnterFrom(TTrigger trigger, Action action);

        IStateConfiguration OnEnterFrom(TTrigger [] triggers, Action action);

        IStateConfiguration OnExit(Action action);

        IStateConfiguration Permit(TTrigger trigger, TState state);

        IStateConfiguration SubstateOf(TState state);

        IStateConfiguration PermitIf(TTrigger trigger, TState state, Func<bool> condition);

        IStateConfiguration PermitReentry(TTrigger trigger);

        IStateConfiguration Ignore(TTrigger trigger);
    }

    private class StateConfiguration : IStateConfiguration
    {
        public TState state;
        public bool hasSuperState = false;
        public TState superState;
        public readonly List<Action<TTrigger, object[]>> onEnter = new List<Action<TTrigger, object[]>>();
        public readonly List<Action<TTrigger, object[]>> onExit = new List<Action<TTrigger, object[]>>();
        public readonly Dictionary<TTrigger, TState> nextStates = new Dictionary<TTrigger, TState>();

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

        public IStateConfiguration OnEnterFrom(TTrigger trigger, Action action)
        {
            onEnter.Add((t, args) =>
            {
                if (t.Equals(trigger))
                    action();
            });
            return this;
        }

        public IStateConfiguration OnEnterFrom(TTrigger [] triggers, Action action)
        {
            foreach (var trigger in triggers)
                OnEnterFrom(trigger, action);
            return this;
        }

        public IStateConfiguration OnEnterFrom<T>(TTrigger trigger, Action<T> action)
        {
            onEnter.Add((t, args) =>
            {
                if (t.Equals(trigger) && args.Length == 1 && args[0] is T)
                    action((T)args[0]);
            });
            return this;
        }
            
        public IStateConfiguration OnExit(Action action)
        {
            onExit.Add((t, args) => action());
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
            throw new NotImplementedException();
        }

        public IStateConfiguration Ignore(TTrigger trigger)
        {
            throw new NotImplementedException();
        }

    }

    readonly Dictionary<TState, StateConfiguration> states = new Dictionary<TState, StateConfiguration>();


    public TState State { get; private set; }
    // TODO: use stack of states instead single state, for correct support substates

    public AKStateMachine(TState initialState)
    {
        State = initialState;
    }

    public IStateConfiguration Configure(TState state)
    {
        if (!states.ContainsKey(state))
            states[state] = new StateConfiguration{ state = state };
        return states[state];
    }

    bool IsInState(StateConfiguration current, TState state)
    {
        if (current.state.Equals(state))
            return true;
        if (!current.hasSuperState)
            return false;
        return IsInState(states[current.superState], state);
    }

    bool _fireLocked = false;
    Tuple<TTrigger, object[]> _fireRefire = null;
    public void Fire(TTrigger trigger, params object[] parameters)
    {
        if (_fireLocked)
        {
            if (_fireRefire == null)
                _fireRefire = Tuple.Create(trigger, parameters);
            return;
        }
        _fireLocked = true;

        //--------------------
        try
        {
            var config1 = states[State];
            if (!config1.nextStates.ContainsKey(trigger))
            {
                DDDebug.LogError("AKStateMachine: " + State + " >>" + trigger + ">> (not found)");
                return;
            }
            var nextState = config1.nextStates[trigger];
            var config2 = states[nextState];

            //--------------------

            if (!IsInState(config2, config1.state))
                foreach (var cmd in config1.onExit)
                    cmd(trigger, null);

            State = nextState;

            if (config2.onEnter != null && !IsInState(config1, config2.state))
                foreach (var cmd in config2.onEnter)
                    cmd(trigger, parameters);

        //--------------------
        }
        finally
        {
            _fireLocked = false;

            var ff = _fireRefire;
            _fireRefire = null;
            if (ff != null)
                Fire(ff.Item1, ff.Item2);
        }
    }
}

