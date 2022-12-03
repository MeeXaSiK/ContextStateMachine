﻿using System;
using System.Collections.Generic;

namespace NTC.ContextStateMachine
{
    public class StateMachine
    {
        public event Action<State> OnStateChanged;

        public bool AutoSelectState { get; set; }
        
        public State CurrentState { get; private set; }
        public Transition CurrentTransition { get; private set; }

        private readonly List<Transition> _anyTransitions = new(16);
        private readonly List<Transition> _transitions = new(16);

        public StateMachine(bool autoSelectState = true)
        {
            AutoSelectState = autoSelectState;
        }
        
        public void SetState(State state)
        {
            if (state == null)
                throw new NullReferenceException(nameof(state), null);
            
            if (state == CurrentState)
                return;
            
            CurrentState?.OnExit();
            
            CurrentState = state;
            
            CurrentState.OnEnter();
            
            OnStateChanged?.Invoke(state);
        }
        
        public void AddTransition(State from, State to, Func<bool> condition)
        {
            _transitions.Add(new Transition(from, to, condition));
        }

        public void AddAnyTransition(State to, Func<bool> condition)
        {
            _anyTransitions.Add(new Transition(null, to, condition));
        }
        
        public void Run()
        {
            if (AutoSelectState)
            {
                CurrentTransition = GetTransition();

                if (CurrentTransition != null)
                {
                    SetState(CurrentTransition.To);
                }   
            }

            CurrentState?.OnRun();
        }

        private Transition GetTransition()
        {
            for (var i = 0; i < _anyTransitions.Count; i++)
            {
                if (_anyTransitions[i].Condition())
                {
                    return _anyTransitions[i];
                }
            }

            for (var i = 0; i < _transitions.Count; i++)
            {
                if (_transitions[i].Condition())
                {
                    return _transitions[i];
                }
            }

            return default;
        }
    }
}