using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
	private List<Transition> allTransitions = new List<Transition>();
	private List<Transition> activeTransitions = new List<Transition>();
	private IState currentState;

	private Dictionary<System.Type, IState> stateCollection = new Dictionary<System.Type, IState>();

	public StateMachine(params IState[] states)
	{
		for (int i = 0; i < states.Length; i++)
		{
			stateCollection.Add(states[i].GetType(), states[i]);
		}
	}

	public void StateUpdate()
	{
		currentState?.OnStateUpdate();
	}

	public void StateFixedUpdate()
	{
		currentState?.OnStateFixedUpdate();
	}

	public void OnFixedUpdate()
	{
		foreach (Transition transition in activeTransitions)
		{
			if (transition.Evalutate())
			{
				SwitchState(transition.toState);
				return;
			}
		}
	}

	public void SwitchState(System.Type newStateType)
	{
		if (stateCollection.ContainsKey(newStateType))
		{
			SwitchState(stateCollection[newStateType]);
		}
		else
		{
			Debug.LogError($"State {newStateType.ToString()} not found in stateCollection");
		}
	}

	public void SwitchState(IState newState)
	{
		currentState?.OnStateExit();
		currentState = newState;
		activeTransitions = allTransitions.FindAll(x => x.fromState == currentState || x.fromState == null);
		currentState?.OnStateEnter();
	}

	public void AddState(IState state)
	{
		stateCollection.Add(state.GetType(), state);
	}

	public void AddTransition(Transition transition)
	{
		allTransitions.Add(transition);
	}

	public bool IsInState(System.Type state)
    {
        if (currentState.ToString() == state.ToString())
        {
			return true;
        }
		return false;
    }
}

/// <summary>
/// The Transition class allows to inject Transitions into the Statemachine.
/// In this way the state themselves don't know about transitions which keeps states oblivious to eachother.
/// We can pass a condition along with the transition, either in lamba-format or just a function which returns a boolean.
/// </summary>
public class Transition
{
	public IState fromState;
	public IState toState;
	public System.Func<bool> condition;

	public Transition(IState fromState, IState toState, System.Func<bool> condition)
	{
		this.fromState = fromState;
		this.toState = toState;
		this.condition = condition;
	}

	public bool Evalutate()
	{
		return condition();
	}
}