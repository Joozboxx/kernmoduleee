using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
	void OnStateEnter();
	void OnStateUpdate();
	void OnStateExit();

	void OnStateFixedUpdate();
}
