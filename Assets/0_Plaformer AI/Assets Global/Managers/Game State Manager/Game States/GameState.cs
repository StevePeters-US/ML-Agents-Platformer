using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [System.Serializable]
    public class GameStateTransition
    {
        public GameStateDecision canChangeToNextState;
        public GameState nextState;
    }

    public abstract class GameState : ScriptableObject
    {
        public GameStateAction[] actions;
        public GameStateTransition[] transitions;
        public Color sceneGizmoColor = Color.grey;

/*        public float intializeDuration = 2.5f;
        public float terminateDuration = 2.5f;*/

        public GameStateUI gameStateUI;
        private GameStateUI gameStateUIRef;

        public bool displayStateDebugInfo = false;

/*        [System.NonSerialized]
        private List<GameStateAction> currentActions = new List<GameStateAction>();
        public List<GameStateAction> CurrentActions { get => currentActions; }*/

        public int CurrentActionIndex { get => currentAction; }
        [System.NonSerialized] private int currentAction = 0;


        public virtual void InitializeGameState(GameStateManager manager)
        {
            // Add UI
            if (gameStateUI)
                gameStateUIRef = Instantiate<GameStateUI>(gameStateUI);
            /*
                        // Add our first action to our current actions list
                        if (actions.Length > 0)
                        {
                            nextSelectedAction = actions[0];
                            AddNextActionToCurrentActions();
                        }

                        else
                            Debug.LogWarning("No actions assigned for current state", this);*/
        }

        public virtual IEnumerator GameStateTick(GameStateManager manager)
        {
            while (true)
            {
                UpdateState(manager);
                yield return new WaitForFixedUpdate();
            }
        }


        public void UpdateState(GameStateManager manager)
        {
            PerformAction(manager);
            CheckActionTransitions();
            CheckStateTransitions(manager);
        }

        /*    private void AddNextActionToCurrentActions()
            {
                currentActions.Add(nextSelectedAction);
            }*/

        /*    private void RemoveFromCurrentActions(GameStateAction action)
            {
                currentActions.Remove(action);
            }*/

        private void PerformAction(GameStateManager mananger)
        {
            if (actions.Length > currentAction)
            {
                actions[currentAction].Act(mananger, this);
            }
            /*  for (int i = 0; i < actions.Length; i++)
              {
                  actions[i].Act(mananger, this);
              }*/
        }

        private void CheckActionTransitions()
        {
            if (actions.Length > currentAction && actions[currentAction].ActionTransitionConditionsMet)
            {
                currentAction += 1;
            }

            /* for (int i = actions.Length - 1; i > 0; i--)
             {
                 if (actions[i].ActionTransitionConditionsMet)
                     RemoveFromCurrentActions(actions[i]);
             }*/
        }

        private void CheckStateTransitions(GameStateManager manager)
        {
            for (int i = 0; i < transitions.Length; i++)
            {
                if (transitions[i].canChangeToNextState.Decide(manager, this))
                    manager.TransitionToNextState(transitions[i].nextState);
            }
        }

        public virtual void TerminateGameState()
        {
            // Remove UI
            if (gameStateUIRef)
                Destroy(gameStateUIRef.gameObject);
        }
    }
}

/*       public virtual void InitializeGameStateTick(GameStateManager manger)
       {

       }*/
/*     public virtual void TerminateGameStateTick()
     {
     }*/