using System;
using UnityEngine;
using UnityEngine.UI;

public class GridCellScript : MonoBehaviour
{
    [SerializeField] GameObject xObj, oObj;
    bool isClicked = false;
    State state;

    private void Start()
    {
        xObj.SetActive(false);
        oObj.SetActive(false);
        isClicked = false ;
        state = State.Blank;
    }

    public void Clicked(State player)
    {
        switch (player)
        {
            case State.Player1:
                state = State.Player1;
                break;

            case State.Player2:
                state = State.Player2;
                break;

            case State.Player3:
                state = State.Player3;
                break;

            default:
                Debug.LogError("Wrong State. State Doesn't Exists");
                break;
        }
    }
}
