using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    public class GameStateManager : Singleton<GameStateManager>
    {
        public GameState currentState;
        public event Action<GameStateManager> OnStateChanged;

        private float timeStateStarted;
        public float TimeStateStarted { get => timeStateStarted; }

        private IEnumerator coroutine;
   //     private bool transitioningToNewState = false;

       public override void Awake()
        {
            base.Awake();

            timeStateStarted = Time.time;
        }

        void Start()
        {
            InitializeCurrentState();
        }

/*        void Update()
        {
            if (!transitioningToNewState)
                currentState.UpdateState(this);
        }*/

        void OnDrawGizmosSelected()
        {
            if (currentState != null)
            {
                Color priorColor = Gizmos.color;
                Gizmos.color = currentState.sceneGizmoColor;
                Gizmos.DrawWireSphere(transform.position, 1);
                Gizmos.color = priorColor;
            }
        }

        // Things that need to happen as soon as the state is loaded (ui)
        private void InitializeCurrentState()
        {
            if (currentState)
            {
                currentState.InitializeGameState(this);

                coroutine = currentState.GameStateTick(this);
                StartCoroutine(coroutine);
            }

            /*      coroutine = InitiailizeCurrentStateTick(currentState.intializeDuration);
                  StartCoroutine(coroutine);*/
        }

        public string GetStateName() {
            return currentState.name;
        }

        // Allows for fading in effects. This happens before actions are performed in the state
  /*      public IEnumerator InitiailizeCurrentStateTick(float intializeDuration)
        {
            float startTime = Time.time;

            while (Time.time < startTime + intializeDuration)
            {
                currentState.InitializeGameStateTick(this);
                yield return new WaitForSeconds(0.1f);
            }

            CompleteStateInitialization();
        }*/

        // Things that need to happen after tick cycle completes, but before state actions start (set UI visible)
/*        private void CompleteStateInitialization()
        {
            transitioningToNewState = false;
            // Broadcast that the game state was switched (mainly to the game manager).
            OnStateChanged?.Invoke(this);
        }*/

/*        public void TransitionToNextState(GameState nextState)
        {
            transitioningToNewState = true;
            Debug.Log(currentState.ToString() + " : Begin transition to next state", this);

            // timer to fade out state, etc
            coroutine = TransitioningStateTick(currentState.terminateDuration, nextState);
            StartCoroutine(coroutine);
        }

        public IEnumerator TransitioningStateTick(float terminateDuration, GameState nextState)
        {
            float startTime = Time.time;

            while (Time.time < startTime + terminateDuration)
            {
                currentState.TerminateGameStateTick();
                yield return new WaitForSeconds(0.1f);
            }

            CompleteTransitionToNextState(nextState);
        }*/

        public void TransitionToNextState(GameState nextState)
        {
            if (currentState.displayStateDebugInfo)
                Debug.Log("Completing Transition to next state", this);

            StopCoroutine(coroutine);

            currentState.TerminateGameState();

            timeStateStarted = Time.time;
            currentState = nextState;

            // Broadcast that the game state was switched (mainly to the game manager).
            OnStateChanged?.Invoke(this);

            InitializeCurrentState();
        }
    }
}
