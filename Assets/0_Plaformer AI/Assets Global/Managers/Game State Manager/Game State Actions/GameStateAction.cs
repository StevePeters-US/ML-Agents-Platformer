using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    public abstract class GameStateAction : ScriptableObject
    {
        // Immediately transitions to the next action without destroying this one. This way we can
        //do a camera fade in while performing another action at the same time.Transitioning to the next action
        //merely removes this action from the current actions list
        public bool RunConcurrent = false;

        [Tooltip("0 for infinite lifetime")]
        public float lifeTime = 0;  // 0 == indefinite
        public bool doOnce = false;

        // Set this to true to move to next action. Can be set from state ie on game over to transition to a fade out effect.
        [System.NonSerialized] private bool actionTransitionConditionsMet = false;
        public bool ActionTransitionConditionsMet { get => actionTransitionConditionsMet; }

        [System.NonSerialized] private float startTime = 0;
        [System.NonSerialized] private int numTimesActionPerformed = 0;


        public virtual void InitializeAction(GameStateManager manager, GameState state)
        {
            startTime = Time.time;
        }

        public virtual void Act(GameStateManager manager, GameState state)
        {
            if (CheckActionTransition())
                return;

            numTimesActionPerformed += 1;
        }

        public virtual bool CheckActionTransition()
        {
            // Check timer
            if (lifeTime > 0 && Time.time > (startTime + lifeTime))
            {
                actionTransitionConditionsMet = true;
            }

            if (doOnce && numTimesActionPerformed > 0)
            {
                actionTransitionConditionsMet = true;
            }

            return actionTransitionConditionsMet;
        }

        public virtual void TerminateAction(GameStateManager manager, GameState state)
        {

        }
        // public abstract void CheckTransition();
    }
}
