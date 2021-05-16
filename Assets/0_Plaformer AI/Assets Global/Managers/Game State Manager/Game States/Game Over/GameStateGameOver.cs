using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Game State Game Over", menuName = "Scriptable Objects/GameStates/Game Over")]
    public class GameStateGameOver : GameState
    {
       // public BoolScriptableObject isPlayerDead;              

        public void LoadMainMenu()
        {
            Debug.Log("implement load main menu", this);
            //SceneManager.LoadScene(0);
        }

        public override void InitializeGameState(GameStateManager manager)
        {
            base.InitializeGameState(manager);
            Debug.Log("Switched to Game State Game Over");
         //   isPlayerDead.runtimeValue = true;
        }

  /*      public override void TerminateGameStateTick()
        {
            base.TerminateGameStateTick();
            LoadMainMenu();
        }*/

    }
}
